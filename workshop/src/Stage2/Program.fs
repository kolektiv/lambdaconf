open System

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

let corsHeaders =
    freya {
        return [
            "accept"
            "content-type" ] }

let corsOrigins =
    freya {
        return AccessControlAllowOriginRange.Any }

let todosMethods =
    freya {
        return [
            GET
            OPTIONS ] }

let todos =
    freyaMachine {
        using http
        using httpCors
        corsHeadersSupported corsHeaders
        corsMethodsSupported todosMethods
        corsOriginsSupported corsOrigins
        methodsSupported todosMethods } |> FreyaMachine.toPipeline

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