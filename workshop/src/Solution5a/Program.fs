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

(* Note

   This is an alternative approach to working with Freya. If you prefer
   monadic (sorry!) operator syntax to computation expression syntax,
   you can get that by opening the Freya.Core.Operators module. *)

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