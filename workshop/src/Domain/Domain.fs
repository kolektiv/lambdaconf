module Domain

open System

// Types

open Chiron

type Todo =
    { Id: Guid
      Url: string
      Order: int option
      Title: string
      Completed: bool }

    static member ToJson (x: Todo) =
        json {
            do! Json.write "id" x.Id
            do! Json.write "url" x.Url
            do! Json.write "order" x.Order
            do! Json.write "title" x.Title
            do! Json.write "completed" x.Completed }

type TodoCreate =
    { Title: string
      Order: int option }

    static member FromJson (_: TodoCreate) =
        json {
            let! title = Json.read "title"
            let! order = Json.tryRead "order"

            return {
                Title = title
                Order = order } }

type TodoUpdate =
    { Title: string option
      Order: int option
      Completed: bool option }

    static member FromJson (_: TodoUpdate) =
        json {
            let! title = Json.tryRead "title"
            let! order = Json.tryRead "order"
            let! completed = Json.tryRead "completed"

            return {
                Title = title
                Order = order
                Completed = completed } }

// Store

type TodoStoreAction =
    | Add of TodoCreate * AsyncReplyChannel<Todo>
    | Clear of AsyncReplyChannel<unit>
    | Delete of Guid * AsyncReplyChannel<unit>
    | Get of Guid * AsyncReplyChannel<Todo option>
    | List of AsyncReplyChannel<Todo list>
    | Update of Guid * TodoUpdate * AsyncReplyChannel<Todo>

type TodoStore () =
    let store = MailboxProcessor.Start (fun inbox ->
        let rec loop (todos: Map<Guid, Todo>) =
            async {
                let! action = inbox.Receive ()

                match action with
                | Add (todoCreate, chan) ->
                    let id =
                        Guid.NewGuid ()

                    let todo =
                        { Id = id
                          Url = sprintf "http://localhost:8080/%A" id
                          Order = todoCreate.Order
                          Title = todoCreate.Title
                          Completed = false }

                    let todos =
                        Map.add id todo todos

                    chan.Reply (todo)
                    return! loop todos
                | Clear (chan) ->
                    chan.Reply ()
                    return! loop Map.empty
                | Delete (id, chan) ->
                    let todos =
                        Map.remove id todos

                    chan.Reply ()
                    return! loop todos
                | Get (id, chan) ->
                    let todo =
                        Map.tryFind id todos

                    chan.Reply (todo)
                    return! loop todos
                | List (chan) ->
                    let todoList =
                        (Map.toList >> List.map snd) todos

                    chan.Reply (todoList)
                    return! loop todos
                | Update (id, todoUpdate, chan) ->
                    match Map.tryFind id todos with
                    | Some todo ->
                        let todo =
                            match todoUpdate.Order with
                            | Some order -> { todo with Order = Some order }
                            | _ -> todo

                        let todo =
                            match todoUpdate.Title with
                            | Some title -> { todo with Title = title }
                            | _ -> todo

                        let todo =
                            match todoUpdate.Completed with
                            | Some completed -> { todo with Completed = completed }
                            | _ -> todo

                        let todos =
                            Map.add id todo todos

                        chan.Reply (todo)
                        return! loop todos
                    | _ ->
                        return! loop todos }

        loop Map.empty)

    member __.Add (todoCreate) =
        store.PostAndAsyncReply (fun chan -> Add (todoCreate, chan))

    member __.Clear () =
        store.PostAndAsyncReply (fun chan -> Clear (chan))

    member __.Delete (id) =
        store.PostAndAsyncReply (fun chan -> Delete (id, chan))

    member __.Get (id) =
        store.PostAndAsyncReply (fun chan -> Get (id, chan))

    member __.List () =
        store.PostAndAsyncReply (fun chan -> List (chan))

    member __.Update (id, todoUpdate) =
        store.PostAndAsyncReply (fun chan -> Update (id, todoUpdate, chan))