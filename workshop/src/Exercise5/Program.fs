﻿open System
open System.Text

// Freya

(* Exercise

   Refector the messageWorld function to messageName - taking an extra
   parameter for who to message. Change the logic where needed!
   
   Refactor the helloWorld and goodbyeWorld functions to helloName and
   goodbyeName, getting the name to use with the Route.atom lens.
   
   Refactor the converseWorld function to converseName, changing the paths
   to require a {name} segment. *)

open Freya.Core
open Freya.Router
open Freya.Types.Http
open Freya.Types.Uri.Template

let messageWorld message =
    freya {
        let text = Encoding.UTF8.GetBytes (sprintf "%s World" message)

        do! Freya.setLensPartial Response.statusCode 200
        do! Freya.setLensPartial Response.reasonPhrase "Awesome"
        do! Freya.mapLens Response.body (fun x -> x.Write (text, 0, text.Length); x)

        return! Freya.next }

let helloWorld =
    freya {
        return! messageWorld "Hello" }

let goodbyeWorld =
    freya {
        return! messageWorld "Goodbye" }

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