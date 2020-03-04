module App2

// Elm Architecture _not_ using Elmish using built-in React stuff

open Feliz

// a.k.a. Model
type State = { Count: int }

type Msg =
    | Increment
    | Decrement

let initialState = { Count = 0 }

let update model msg =
    match msg with
    | Increment -> { Count = model.Count + 1 }
    | Decrement -> { Count = model.Count - 1 }

// function component in React
let counter = React.functionComponent(fun () ->
    let (state, dispatch) = React.useReducer(update, initialState)
    Html.div [
        Html.h3 state.Count
        Html.button [ prop.onClick (fun _ -> dispatch Increment); prop.text "+" ]
        Html.button [ prop.onClick (fun _ -> dispatch Decrement); prop.text "-" ]
    ]
)

open Browser.Dom

// <Counter /> in JSX
ReactDOM.render(counter, document.getElementById "elmish-app")