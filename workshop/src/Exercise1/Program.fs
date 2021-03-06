﻿open System
open System.IO

// Freya

(* Exercise

   Complete the helloWorld function, this time using the environment
   exposed by the Freya state. *)

open Freya.Core

let helloWorld =
    freya {
        return () }

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