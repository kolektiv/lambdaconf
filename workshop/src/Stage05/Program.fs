open System
open System.IO

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

let todosMethods =
    freya {
        return [
            GET
            OPTIONS
            POST ] }

let todos =
    freyaMachine {
        including common
        corsMethodsSupported todosMethods
        methodsSupported todosMethods
        doPost todosAddDo } |> FreyaMachine.toPipeline

// -- Todo Backend

let todoBackend =
    freyaRouter {
        resource (UriTemplate.Parse "/") todos } |> FreyaRouter.toPipeline

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