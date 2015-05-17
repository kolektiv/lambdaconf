open System
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

let messageName message name =
    freya {
        let text = Encoding.UTF8.GetBytes (sprintf "%s %s" message name)

        do! Freya.setLensPartial Response.statusCode 200
        do! Freya.setLensPartial Response.reasonPhrase "Awesome"
        do! Freya.mapLens Response.body (fun x -> x.Write (text, 0, text.Length); x)

        return! Freya.next }

let helloName =
    freya {
        let! name = Freya.getLensPartial (Route.atom "name")
        return! messageName "Hello" name.Value }

let goodbyeName =
    freya {
        let! name = Freya.getLensPartial (Route.atom "name")
        return! messageName "Goodbye" name.Value }

let converseName =
    freyaRouter {
        route All (UriTemplate.Parse "/hello/{name}") helloName
        route All (UriTemplate.Parse "/goodbye/{name}") goodbyeName } |> FreyaRouter.toPipeline

// Katana

open Microsoft.Owin.Hosting

type Exercise () =
    member __.Configuration () =
        OwinAppFunc.ofFreya converseName

// Entry

[<EntryPoint>]
let run _ =
    let _ = WebApp.Start<Exercise> ("http://localhost:8080")
    let _ = Console.ReadLine ()
    0