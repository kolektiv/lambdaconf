open System

// Freya

open Freya.Core
open Freya.Router
open Freya.Types.Http
open Freya.Types.Uri.Template

let helloWorld =
    freya {
        let  text = "Hello World"B

        do! Freya.setLensPartial Response.statusCode 200
        do! Freya.setLensPartial Response.reasonPhrase "Awesome"
        do! Freya.mapLens Response.body (fun x -> x.Write (text, 0, text.Length); x)

        return! Freya.next }

let goodbyeWorld =
    freya {
        let  text = "Goodbye World"B

        do! Freya.setLensPartial Response.statusCode 200
        do! Freya.setLensPartial Response.reasonPhrase "Awesome"
        do! Freya.mapLens Response.body (fun x -> x.Write (text, 0, text.Length); x)

        return! Freya.next }

let converseWorld =
    freyaRouter {
        route All (UriTemplate.Parse "/hello") helloWorld
        route All (UriTemplate.Parse "/goodbye") goodbyeWorld } |> FreyaRouter.toPipeline

// Katana

open Microsoft.Owin.Hosting

type Exercise () =
    member __.Configuration () =
        OwinAppFunc.ofFreya converseWorld

// Entry

[<EntryPoint>]
let run _ =
    let _ = WebApp.Start<Exercise> ("http://localhost:8080")
    let _ = Console.ReadLine ()
    0