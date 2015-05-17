open System

// Freya

(* Exercise

   Convert the helloWorld function to use lenses from the
   Response module to modify the environment using functions in the
   Freya module.
   
   Note: We've opened the Freya.Types.Http module... *)

open Freya.Core
open Freya.Types.Http

let helloWorld =
    freya {
        let text = "Hello World"B

        do! Freya.setLensPartial Response.statusCode 200
        do! Freya.setLensPartial Response.reasonPhrase "Awesome"
        do! Freya.mapLens Response.body (fun x -> x.Write (text, 0, text.Length); x) }

// Katana

open Microsoft.Owin.Hosting

type Exercise () =
    member __.Configuration () =
        OwinAppFunc.ofFreya helloWorld

// Entry

[<EntryPoint>]
let run _ =
    let _ = WebApp.Start<Exercise> ("http://localhost:8080")
    let _ = Console.ReadLine ()
    0