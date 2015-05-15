open System
open System.Collections.Generic
open System.IO
open System.Threading.Tasks

// OWIN

let helloWorld =
    Func<IDictionary<string, obj>, Task> (fun env ->
        Async.StartAsTask (async {
            let body = "Hello World"B

            env.["owin.ResponseStatusCode"] <- 200
            env.["owin.ResponseReasonPhrase"] <- "Awesome"
            env.["owin.ResponseBody"] :?> Stream |> fun x -> x.Write (body, 0, body.Length) }) :> Task)

// Katana

open Microsoft.Owin.Hosting

type Exercise () =
    member __.Configuration () =
        helloWorld

// Entry

[<EntryPoint>]
let run _ =
    let _ = WebApp.Start<Exercise> ("http://localhost:8080")
    let _ = Console.ReadLine ()
    0