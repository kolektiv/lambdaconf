open System
open System.IO

// Freya

(* Exercise

   Complete the helloWorld function, this time using the environment
   exposed by the Freya state. *)

open Freya.Core

let helloWorld =
    freya {
        let text = "Hello World"B
        let! state = Freya.getState

        state.Environment.["owin.ResponseStatusCode"] <- 200
        state.Environment.["owin.ResponseReasonPhrase"] <- "Awesome"
        state.Environment.["owin.ResponseBody"] :?> Stream |> fun x -> x.Write (text, 0, text.Length) }

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

(* Solution

*)