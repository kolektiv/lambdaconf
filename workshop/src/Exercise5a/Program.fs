open System
open System.Text

// Freya

open Freya.Core
open Freya.Core.Operators
open Freya.Router
open Freya.Types.Http
open Freya.Types.Uri.Template

let messageName message name =
        Freya.setLensPartial Response.statusCode 200
     *> Freya.setLensPartial Response.reasonPhrase "Awesome"
     *> Freya.mapLens Response.body (fun stream ->
            let text = Encoding.UTF8.GetBytes (sprintf "%s %s" message name)
            let _ = stream.Write (text, 0, text.Length);
            
            stream)
     *> Freya.next

let readName =
    Option.get <!> Freya.getLensPartial (Route.atom "name")

let helloName =
    readName >>= messageName "Hello"

let goodbyeName =
    readName >>= messageName "Goodbye"

let converseWorld =
    freyaRouter {
        route All (UriTemplate.Parse "/hello/{name}") helloName
        route All (UriTemplate.Parse "/goodbye/{name}") goodbyeName } |> FreyaRouter.toPipeline

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