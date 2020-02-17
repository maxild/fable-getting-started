module App

// open Browser.Dom

// let btnPlus = document.getElementById "btnPlus"
// let btnMinus = document.getElementById "btnMinus"
// let textCounter = document.getElementById "textCounter"

// let mutable counter = 0

// btnPlus.addEventListener ("click", (fun _ ->
//     counter <- counter + 1
//     textCounter.innerText <- (string counter)
//     ))

// btnMinus.addEventListener ("click", (fun _ ->
//     counter <- counter - 1
//     textCounter.innerText <- (string counter)
//     ))

//
// Elm Architecture
//

type Model =
    { Counter: int }

type Msg =
    | Increment
    | Decrement

let init () =
    { Counter = 42 }

let update msg model =
    match msg with
    | Increment -> { model with Counter = model.Counter + 1 }
    | Decrement -> { model with Counter = model.Counter - 1 }

// dotnet add package Fable.React//
open Fable.React // Helpers to create react elements (str, button, div etc...)
open Fable.React.Props

// TODO: Add event listeners
let view model dispatch =
    div []
        [
            button [ OnClick (fun _ -> dispatch Increment) ] [ str "+"]
            h2 [] [ ofInt model.Counter ]
            button [ OnClick (fun _ -> dispatch Decrement) ] [ str "-"]
        ]

// dotnet add package Fable.Elmish.React
open Elmish
open Elmish.React

Program.mkSimple init update view
|> Program.withReactSynchronous "app"
|> Program.run

