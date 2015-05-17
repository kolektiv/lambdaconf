open System
open System.Collections.Generic
open System.IO
open System.Threading.Tasks

// OWIN

(* Exercise

   Complete the helloWorld function, modifying the OWIN environment.
   We want to set the response code to 200, the reason phrase to "Awesome",
   and we want to write the text "Hello World" to the body stream. *)

let helloWorld =
    Func<IDictionary<string, obj>, Task> (fun env ->
        Async.StartAsTask (async {
            let text = "Hello World"B

            env.["owin.ResponseStatusCode"] <- 200
            env.["owin.ResponseReasonPhrase"] <- "Awesome"
            env.["owin.ResponseBody"] :?> Stream |> fun x -> x.Write (text, 0, text.Length) }) :> Task)

// Katana

open Microsoft.Owin.Hosting

type Exercise () =
    member __.Configuration () =
        helloWorld

// Entry

[<EntryPoint>]
let run _ =
    let _ = WebApp.Start<Exercise> ("http://localhost:8080")
    let _ = Console.ReadLine ()
    0