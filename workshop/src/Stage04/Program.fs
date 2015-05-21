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

let todosMethods =
    freya {
        return [
            GET
            OPTIONS ] }

let todos =
    freyaMachine {
        including common
        corsMethodsSupported todosMethods
        methodsSupported todosMethods } |> FreyaMachine.toPipeline

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