module App

//
// Elm Architecture
//
// Two main React DSLs
//   * Feliz: Uses single list (Html.div [ ]). It's also providing strongly typing for all the properties.
//   * Fable.React: Uses two lists (div [ ] [ ])
// NOTE: Memo components in Fable.React and Feliz can be keyed and named, unlike lazyView at present.
// NOTE: LazyView was created before memo/FunctionComponent.Of (React.functionComponent in Feliz)
// Main state DSL
//   * Fable.React.Elmish (Fable.Elmish)

type TodoId = TodoId of System.Guid

type Todo =
    { Id: TodoId
      Description: string
      Completed: bool }

// a.k.a. Model, ViewModel
type State =
    { Todos: Todo list
      NewTodo: string }

// List of all the events in the UI
type Msg =
    | NewTodoChanged of string // SetNewTodo
    | AddNewTodo

// initialState
let init () =
    { Todos = []
      NewTodo = "" }

let update (msg: Msg) (state: State) : State = state

// dotnet add package Fable.React
//open Fable.React // Helpers to create react elements (str, button, div etc...)
//open Fable.React.Props

// dotnet add package Feliz
open Feliz // Alternative DSL for writing 'JSX'

let title =
    Html.p [
        prop.className "title"
        prop.text "Elmish TODO App"
    ]

let newTodoInput (currentNewTodo: string) (dispatch: Msg -> unit) =
    Html.div [
        prop.classes ["field"; "has-addons"]
        prop.children [
            // Text input
            Html.div [
                prop.classes ["control"; "is-expanded"]
                prop.children [
                    Html.input [
                        prop.classes ["input"; "is-medium"]
                        prop.valueOrDefault currentNewTodo
                        prop.onTextChange (NewTodoChanged >> dispatch)
                    ]
                ]
            ]
            // Add button
            Html.div [
                prop.classes ["control"]
                prop.children [
                    Html.button [
                        prop.classes ["button"; "is-primary"; "is-medium"]
                        prop.children [
                            Html.i [ prop.classes ["fa"; "fa-plus"] ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let render (state: State) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [ style.padding 20 ]
        prop.children [
            title
            newTodoInput state.NewTodo dispatch

        ]
    ]

// dotnet add package Fable.Elmish.React
open Elmish
open Elmish.React

Program.mkSimple init update render
|> Program.withReactSynchronous "app"
|> Program.run

