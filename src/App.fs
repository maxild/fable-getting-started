module App

open Fable.React

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

// TODOs
//  filter todos (completed, not completed)
//  See other versions of the todo list app

type TodoId = TodoId of System.Guid

type Todo =
    { Id: TodoId
      Description: string
      Completed: bool }

// a.k.a. Model, ViewModel
type State =
    { Todos: Todo list
      NewTodoDescription: string }

// TODO: Maybe create module State functions

let withNewTodoOf description (state: State) =
    { state with NewTodoDescription = description }

let withAddedTodoOf (description: string) (state: State) : State =
    let newTodo = { Id = TodoId <| System.Guid.NewGuid()
                    Description = description
                    Completed = false }
    // todos are added to the bottom using append operator (cons :: operator adds to the top)
    { state with Todos = state.Todos @ [newTodo] }

let withClearedDescription (state: State) =
    { state with NewTodoDescription = "" }

// List of all the events in the UI
type Msg =
    | NewTodoChanged of string // SetNewTodo
    | AddNewTodo
    | DeleteTodo of TodoId
    | ToggleCompleted of TodoId

// initialState
let init () =
    { Todos = []
      NewTodoDescription = "" }

// find an item and calculate (head-list, itemOpt, tail-list)
let find2 (p: 'a -> bool) (xs: 'a list) : 'a list * 'a option * 'a list =
    let rec find2' (p: 'a -> bool) (acc: 'a list) (xs: 'a list) : 'a list * 'a option * 'a list =
        match xs with
        | [] -> (acc, None, [])
        | x::xs -> if p x then (acc, Some x, xs) else find2' p (acc @ [x]) xs
    find2' p [] xs

// This update (a.k.a. compute) can be tested separately (without using the DOM)
// It doesn't even have to know about Fable (and therefore can be tested on .NET Core)
let update (msg: Msg) (state: State) : State =
    match msg with
    | NewTodoChanged description ->
        state
        |> withNewTodoOf description
    | AddNewTodo ->
        state
        |> withAddedTodoOf state.NewTodoDescription
        |> withClearedDescription
    | DeleteTodo id ->
        let newTodos =
            state.Todos
            |> List.filter (fun todo -> todo.Id <> id)
        { state with Todos = newTodos }
    | ToggleCompleted id ->
        let (head_list, itemOpt, tail_list) =
            state.Todos
            |> find2 (fun todo -> todo.Id = id)
        match itemOpt with
        | Some todo ->
            let newTodo = { todo with Completed = not todo.Completed }
            { state with Todos = head_list @ [newTodo] @ tail_list }
        | None -> failwith "UNEXPECTED error"

// dotnet add package Fable.React
//open Fable.React // Helpers to create react elements (str, button, div etc...)
//open Fable.React.Props

// dotnet add package Feliz
open Feliz // Alternative DSL for writing 'JSX'

// Helper function using the old syntax of the Fable.React DSL
let div (classes: string list) (children: Fable.React.ReactElement list) =
    Html.div [
        prop.classes classes
        prop.children children
    ]

// Helper function
let iconClasses (classes: string list) =
    prop.children [
        Html.i [ prop.classes classes ]
    ]

// helper function as prop extension
type prop with
    static member inline iconClasses (names: seq<string>) =
        prop.children [
            Html.i [ prop.classes names ]
        ]

let title =
    // <p class="title">Elmish To-Do App</p>
    Html.p [
        prop.className "title"
        prop.text "Elmish To-Do App"
    ]

let newTodoInput (currentNewTodo: string) (dispatch: Msg -> unit) =
    //    <div class="field has-addons">
    //        <div class="control is-expanded">
    //            <input class="input is-medium">
    //        </div>
    //        <div class="control">
    //            <button class="button is-primary is-medium"><i class="fa fa-plus"></i></button>
    //        </div>
    //    </div>
    Html.div [
        prop.classes ["field"; "has-addons"]
        prop.children [
            Html.div [
                prop.classes ["control"; "is-expanded"]
                prop.children [
                    Html.input [
                        prop.classes ["input"; "is-medium"]
                        prop.valueOrDefault currentNewTodo
                        // TODO: 13.0 is enter (remove magic float). It should be an int!!!
                        prop.onKeyUp (fun ev -> if ev.keyCode = 13.0 then dispatch AddNewTodo)
                        prop.onTextChange (NewTodoChanged >> dispatch)
                    ]
                ]
            ]
            Html.div [
                prop.classes ["control"]
                prop.children [
                    Html.button [
                        prop.classes ["button"; "is-primary"; "is-medium"]
                        prop.onClick (fun _ -> dispatch AddNewTodo)
                        prop.iconClasses ["fa"; "fa-plus"]
                    ]
                ]
            ]
        ]
    ]

// TODO: Learn about 'key' helping React to calculate the diff
let renderTodo (todo: Todo) (dispatch: Msg -> unit) =
    // old DSL syntax here
    // 2 column layout: 1. description & 2. buttons
    // TODO: Add todo is missing
    div [ "box" ] [
        div ["columns"; "is-mobile"] [
            // text
            div ["column"] [
                Html.p [
                    if todo.Completed then prop.style [ style.textDecoration.lineThrough ]
                    prop.className "subtitle"
                    prop.text todo.Description
                ]
            ]
            // button
            div ["column"; "is-narrow"] [
                div ["buttons"] [
                    Html.button [
                        prop.classes ["button"]
                        prop.onClick (fun _ -> dispatch <| ToggleCompleted todo.Id)
                        prop.iconClasses ["fa"; "fa-plus"]
                    ]
                    Html.button [
                        prop.classes ["button"; "is-danger"]
                        prop.onClick (fun _ -> dispatch <| DeleteTodo todo.Id)
                        prop.iconClasses ["fa"; "fa-times"]
                    ]
                ]
            ]
        ]
    ]

let todoList (todos: Todo list) (dispatch: Msg -> unit) =
    Html.ul [
        prop.children [ for todo in todos -> renderTodo todo dispatch ]
    ]

let render (state: State) (dispatch: Msg -> unit) =
    //<div style="padding: 20px;">
    //    <Title />
    //    <NewTodoInput />
    //    <TodoList />
    //</div>
    Html.div [
        prop.style [ style.padding 20 ]
        prop.children [
            title
            newTodoInput state.NewTodoDescription dispatch
            todoList state.Todos dispatch
        ]
    ]

// dotnet add package Fable.Elmish.React
open Elmish
open Elmish.React

Program.mkSimple init update render
|> Program.withReactSynchronous "app"
|> Program.run

