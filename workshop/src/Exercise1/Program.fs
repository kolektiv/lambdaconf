open System
open System.IO

// Freya

open Freya.Core

let helloWorld =
    freya {
        let body = "Hello World"B
        let! state = Freya.getState

        state.Environment.["owin.ResponseStatusCode"] <- 200
        state.Environment.["owin.ResponseReasonPhrase"] <- "Awesome"
        state.Environment.["owin.ResponseBody"] :?> Stream |> fun x -> x.Write (body, 0, body.Length) }

// Katana

open Microsoft.Owin.Hosting

type FreyaApplication () =
    member __.Configuration () =
        OwinAppFunc.ofFreya (helloWorld)

// Main

[<EntryPoint>]
let main _ = 
    let _ = WebApp.Start<FreyaApplication> ("http://localhost:8080")
    let _ = Console.ReadLine ()
    0