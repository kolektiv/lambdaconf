open System
open System.IO
open System.Text

// Domain

open Domain

let todoStore =
    TodoStore ()

// Freya

open Freya.Core
open Freya.Machine
open Freya.Machine.Extensions.Http
open Freya.Machine.Extensions.Http.Cors
open Freya.Machine.Router
open Freya.Router
open Freya.Types.Http
open Freya.Types.Http.Cors
open Freya.Types.Language
open Freya.Types.Uri.Template

// -- Helpers

open Chiron

let inline read () =
    freya {
        let! body = Freya.getLens Request.body

        use reader = new StreamReader (body)
        let data = reader.ReadToEnd ()
        
        match Json.tryParse data with
        | Choice1Of2 json ->
            match Json.tryDeserialize json with
            | Choice1Of2 obj -> return Some obj
            | _ -> return None
        | _ ->
            return None }

let inline write obj =
    freya {
        let json = Json.serialize obj
        let text = Json.format json

        return {
            Data = Encoding.UTF8.GetBytes text
            Description =
                { Charset = Some Charset.Utf8
                  Encodings = None
                  MediaType = Some MediaType.Json
                  Languages = Some [ LanguageTag.Parse "en" ] } } }

// -- Common

let commonCorsHeaders =
    freya {
        return [
            "accept"
            "content-type" ] }

let commonCorsOrigins =
    freya {
        return AccessControlAllowOriginRange.Any }

let commonMediaTypes =
    freya {
        return [
            MediaType.Json ] }

let common =
    freyaMachine {
        using http
        using httpCors
        corsHeadersSupported commonCorsHeaders
        corsOriginsSupported commonCorsOrigins
        mediaTypesSupported commonMediaTypes }

// -- Todos

let todosAdd =
    freya {
        let! todoCreate = read ()
        return! Freya.fromAsync todoStore.Add todoCreate.Value } |> Freya.memo

let todosAddDo =
    freya {
        let! _ = todosAdd
        return () }

let todosAddHandle _ =
    freya {
        let! todo = todosAdd
        return! write todo }

let todosClear =
    freya {
        return! Freya.fromAsync todoStore.Clear () } |> Freya.memo

let todosClearDo =
    freya {
        let! _ = todosClear
        return () }

let todosList =
    freya {
        return! Freya.fromAsync todoStore.List () } |> Freya.memo

let todosListHandle _ =
    freya {
        let! todos = todosList
        return! write todos }

let todosMethods =
    freya {
        return [
            DELETE
            GET
            OPTIONS
            POST ] }

let todos =
    freyaMachine {
        including common
        corsMethodsSupported todosMethods
        methodsSupported todosMethods
        doDelete todosClearDo
        doPost todosAddDo
        handleCreated todosAddHandle
        handleOk todosListHandle } |> FreyaMachine.toPipeline

// -- Todo

let todoId =
    freya {
        let! id = Freya.getLensPartial (Route.atom "id")
        return Guid.Parse id.Value }

let todoGet =
    freya {
        let! id = todoId
        return! Freya.fromAsync todoStore.Get id } |> Freya.memo

let todoGetHandle _ =
    freya {
        let! todo = todoGet
        return! write todo }

let todoUpdate =
    freya {
        let! id = todoId
        let! todoUpdate = read ()
        return! Freya.fromAsync todoStore.Update (id, todoUpdate.Value) } |> Freya.memo

let todoUpdateDo =
    freya {
        let! _ = todoUpdate
        return () }

let todoDelete =
    freya {
        let! id = todoId
        return! Freya.fromAsync todoStore.Delete id } |> Freya.memo

let todoDeleteDo =
    freya {
        let! _ = todoDelete
        return () }

let todoMethods =
    freya {
        return [
            DELETE
            GET
            OPTIONS
            Method.Custom "PATCH" ] }

let todo =
    freyaMachine {
        including common
        corsMethodsSupported todoMethods
        methodsSupported todoMethods
        doDelete todoDeleteDo
        doPatch todoUpdateDo
        handleOk todoGetHandle } |> FreyaMachine.toPipeline

// -- Todo Backend

let todoBackend =
    freyaRouter {
        resource (UriTemplate.Parse "/") todos
        resource (UriTemplate.Parse "/{id}") todo } |> FreyaRouter.toPipeline

// Katana

open Microsoft.Owin.Hosting

type Project () =
    member __.Configuration () =
        OwinAppFunc.ofFreya todoBackend

// Entry

[<EntryPoint>]
let run _ =
    let _ = WebApp.Start<Project> ("http://localhost:8080")
    let _ = Console.ReadLine ()
    0