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

let todoAdd =
    freya {
        let! todoCreate = read ()
        return! Freya.fromAsync todoStore.Add todoCreate.Value } |> Freya.memo

let todoAddDo =
    freya {
        let! _ = todoAdd
        return () }

let todoAddHandle _ =
    freya {
        let! todo = todoAdd
        return! write todo }

let todoClear =
    freya {
        return! Freya.fromAsync todoStore.Clear () } |> Freya.memo

let todoClearDo =
    freya {
        let! _ = todoClear
        return () }

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
        doDelete todoClearDo
        doPost todoAddDo
        handleCreated todoAddHandle } |> FreyaMachine.toPipeline

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