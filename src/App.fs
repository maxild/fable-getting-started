module App

// dotnet add package Fable.React
//open Fable.React // Helpers to create react elements (str, button, div etc...)
//open Fable.React.Props

// NOTE: We are using Feliz now...
// dotnet add package Feliz
open Feliz // Alternative DSL for writing 'JSX'

open Elmish // Cmd
open Elmish.React

open Fable.SimpleHttp

open Zanaptak.TypedCssClasses

type Bulma = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/bulma/0.8.0/css/bulma.min.css", Naming.PascalCase>
type FA = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.12.1/css/all.min.css", Naming.PascalCase>


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

//
// ---------- Model (a.k.a. State, ViewModel) ---------
//

type TodoId = TodoId of System.Guid

type Todo =
    { Id: TodoId
      Description: string
      Completed: bool }

// Two options for handling errors
// 1. RemoteData<Result<string, string>>
// 2. Add another union case for 'FailedWhileLoadingData of string'...or maybe to generic params
type RemoteData<'T> =
    | HasNotLoaded
    | Loading
    | FinishedLoading of 'T

// a.k.a. Model
type State =
    { Todos: Todo list
      NewTodoDescription: string
      ResponseText: RemoteData<Result<string, string>> }

//
// ------------ State/Model helpers -----------
//

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

// NOTE: So much easier just to map/traverse the list and change a single item
// find an item and calculate (head-list, itemOpt, tail-list)
//let find2 (p: 'a -> bool) (xs: 'a list) : 'a list * 'a option * 'a list =
//    let rec find2' (p: 'a -> bool) (acc: 'a list) (xs: 'a list) : 'a list * 'a option * 'a list =
//        match xs with
//        | [] -> (acc, None, [])
//        | x::xs -> if p x then (acc, Some x, xs) else find2' p (acc @ [x]) xs
//    find2' p [] xs

let withCompletedFlipped todoId state =
    let newTodos =
        state.Todos 
        |> List.map (fun todo -> 
            if todo.Id = todoId then
                { todo with Completed = not todo.Completed }
            else 
                todo
        )
    { state with Todos = newTodos }    

//
// ---------- Define events ---------
//

// List of all the events in the UI
type Msg =
    // To-do app example
    | NewTodoChanged of string // SetNewTodo
    | AddNewTodo
    //| AddNewTodoTwice // artificial command for learning purposes
    | DeleteTodo of TodoId
    | ToggleCompleted of TodoId // An alternative is CompleteTodo/UncompleteTodo
    // Http example
    | GetData
    | DataReceived of string
    | ErrorWhileReceivingData of string
    
//
// ---------- Define initial state -------
//

let getData () =
    async {
        do! Async.Sleep 2000
        // Http.get will never throw exception, any failure is communicated as statusCode
        let! statusCode, responseText = Http.get "/data.txt"
        return
            if (statusCode = 200)
            then DataReceived responseText
            else ErrorWhileReceivingData <| sprintf "Something weird happened: %i" statusCode
    }

let withGetDataCmd state =
    { state with ResponseText = Loading }, Cmd.OfAsync.perform getData () id

let withoutCmd state =
    state, Cmd.none

let init () : State * Cmd<Msg> =
    { Todos = []
      NewTodoDescription = ""
      ResponseText = HasNotLoaded }
    |> withGetDataCmd

//
// ------------ Compute next state ----------------
//


// The dispatch of events (messages) can happen asynchronously!!! But events in the update -> newState -> render -> dispatch are sync 
// Command (Cmd) tell the runtime, what command should be executed after this update
// NOTE: The update function is running synchronously in the Elm architecture. Any Asynchronous events
//       (subscriptions, XHR requests/responses, delayed functions etc) are handled by command-architecture, and the runtime)
// NOTE: The update function is (in principle) a pure function. 
// This update (a.k.a. compute) can be tested separately (without using the DOM)
// It doesn't even have to know about Fable (and therefore can be tested on .NET Core)
let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | NewTodoChanged description ->
        state
        |> withNewTodoOf description
        |> withoutCmd
    | AddNewTodo ->
        state
        |> withAddedTodoOf state.NewTodoDescription
        |> withClearedDescription
        |> withoutCmd
//    | AddNewTodoTwice ->
//        state, Cmd.batch [ Cmd.ofMsg AddNewTodo; Cmd.ofMsg AddNewTodo ]
    | DeleteTodo id ->
        let newTodos =
            state.Todos
            |> List.filter (fun todo -> todo.Id <> id)
        { state with Todos = newTodos }
        |> withoutCmd
    | ToggleCompleted id ->
        state 
        |> withCompletedFlipped id
        |> withoutCmd
        
    | GetData ->
        // NOTE: The API is the way it is because of consistency with Task and
        // Promise implementation that bot start immediately (cold vs hot start)
        state
        |> withGetDataCmd
    // The following 2 cases will transform the result of the command
    | ErrorWhileReceivingData msg ->
        { state with ResponseText = FinishedLoading (Error msg) }
        |> withoutCmd
    | DataReceived responseText ->
        { state with ResponseText = FinishedLoading (Ok responseText) }
        |> withoutCmd
        

//
// ------------ React DSL ---------------
//            


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
        prop.className Bulma.Title
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
        prop.classes [ Bulma.Field; Bulma.HasAddons]
        prop.children [
            Html.div [
                prop.classes [Bulma.Control; Bulma.IsExpanded]
                prop.children [
                    Html.input [
                        prop.classes [Bulma.Input; Bulma.IsMedium]
                        prop.valueOrDefault currentNewTodo
                        // TODO: 13.0 is enter (remove magic float). It should be an int!!!
                        prop.onKeyUp (fun ev -> if ev.keyCode = 13.0 then dispatch AddNewTodo)
                        prop.onTextChange (NewTodoChanged >> dispatch)
                    ]
                ]
            ]
            Html.div [
                prop.classes [Bulma.Control]
                prop.children [
                    Html.button [
                        prop.classes [Bulma.Button; Bulma.IsPrimary; Bulma.IsMedium]
                        prop.onClick (fun _ -> dispatch AddNewTodo)
                        prop.iconClasses [FA.Fa; FA.FaPlus]
                    ]
                ]
            ]
        ]
    ]

// TODO: Learn about 'key' helping React to calculate the diff
let renderTodo (todo: Todo) (dispatch: Msg -> unit) =
    // old DSL syntax here
    // 2 column layout: 1. description & 2. buttons
    div [ "box" ] [
        div ["columns"; "is-mobile"] [
            div ["column"] [
                Html.p [
                    if todo.Completed then prop.style [ style.textDecoration.lineThrough ]
                    //if todo.Completed then prop.style [style.color.lightGray]
                    prop.className Bulma.Subtitle
                    prop.text todo.Description
                ]
            ]
            div ["column"; "is-narrow"] [
                // Cannot work out icons for complete/uncomplete
//                Html.button [
//                    prop.classes [Bulma.; Bulma.IsMedium]
//                    prop.onClick (fun _ -> dispatch <| ToggleCompleted todo.Id)
//                    // TODO: Find better icon class here...
//                    //prop.iconClasses [FA.Fa; FA.FaAsterisk]
//                ]
                
                Html.input [
                    prop.type'.checkbox
                    prop.onCheckedChange (fun _ -> dispatch <| ToggleCompleted todo.Id) 
                ]        
                Html.text "Completed"
            ]
            div [ "column"; "is-narrow" ] [
                // This works too!
//                Html.a [ 
//                    prop.classes [Bulma.Delete; Bulma.IsMedium]
//                    prop.onClick (fun _ -> dispatch (DeleteTodo todo.Id))
//                ]
                Html.button [
                        prop.classes [Bulma.Delete; Bulma.IsMedium]
                        prop.onClick (fun _ -> dispatch <| DeleteTodo todo.Id)
                ]
            ]   
        ]
    ]

let todoList (todos: Todo list) (dispatch: Msg -> unit) =
    Html.ul [
        prop.children [ for todo in todos -> renderTodo todo dispatch ]
    ]

let renderResponseText (responseText: RemoteData<Result<string, string>>) =
    match responseText with
    | HasNotLoaded ->
        Html.none
    | Loading ->
        Html.div "Some fancy spinner"
    | FinishedLoading (Error msg) ->
        Html.div [
            prop.style [style.color.red]
            prop.text msg
        ]
    | FinishedLoading (Ok data) ->
        Html.div data

let render (state: State) (dispatch: Msg -> unit) =
    //<div style="padding: 20px;">
    //    <Title />
    //    <NewTodoInput />
    //    <TodoList />
    //</div>
    
    Html.div [
    
        Html.div [
            prop.style [ style.padding 20 ]
            prop.children [
                title
                newTodoInput state.NewTodoDescription dispatch
                todoList state.Todos dispatch
            ]
        ]

        // Http get response        
        renderResponseText state.ResponseText
        
        Html.button [
            prop.onClick (fun _ -> dispatch GetData)
            prop.text "Fetch data"    
        ]
    ]
//
// --------- Elmish -------------
//

// dotnet add package Fable.Elmish.React

Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run

