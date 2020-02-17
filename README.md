# Fable/Elmish Getting Started

Requirements

 - [.NET Core](https://www.microsoft.com/net/download) 3.x
 - [Node.js](https://nodejs.org/en/) 11.6.0+ (LTS)

## Dependencies

The Fable/Elmish client-side application has the following dependencies.

### .NET

`Fable.React` will pull in `Fable.Core` and `Fable.Browser.Dom`. `Fable.Elmish.React` will make us setup the binding for the Elm Archtecture that is backed by React (virtual dom).

```
dotnet add package Fable.React
dotnet add package Fable.Elmish.React
```

### NodeJS

Besides the dev dependencies (`webpack` and `fable-compiler`) we need react and react-dom, because the `Fable.React` package will emit javascript that requires those NPM packages.

```
npm install react react-dom
```

## Documentation

### Compiler

* [Fable](https://github.com/fable-compiler/Fable)

### Basics

* [Fable.Browser.*](https://github.com/fable-compiler/fable-browser)
  * Many packages Fable.Browser.[Dom|Event|...]
* [Fable.Fetch](https://github.com/fable-compiler/fable-fetch)

### React

This is needed to write the render (`view :: Model -> (Msg -> unit) -> ReactElement`) function

* [Fable.React](https://github.com/fable-compiler/fable-react)

### Elmish.React

This is needed to implement the Elm Architecture

```fsharp
Program.mkSimple init update view
|> Program.withReactSynchronous "app"
|> Program.run
```

* [Fable.Elmish](https://github.com/elmish/elmish)
* [Fable.Elmish.React](https://github.com/elmish/react)

## Installation

To compile the project, run the following commands

```bash
npm install
npm run build
```
`npm install` will install dependencies from [npm](https://www.npmjs.com/) which is the Node.js equivalent of dotnet's Nuget registry. These dependencies include the Fable compiler itself as it is distributed to npm to make compilation worflow as simple as possible.

`npm run build` will then start building the project by invoking [webpack](https://webpack.js.org/) which is responsible of orchestrating the compilation process.

After `npm run build` finished running, the generated javascript will be bundled in a single file called `bundle.js` located in the `public` directory along with the `index.html` page that references that script file.

## Development mode

While developing the application, you don't want to recompile the application every time you make a change. Instead of that, you can start the compilation process in development mode which will watch changes you make in the file and re-compile automatically really fast:
```bash
npm install
npm start
```

If you already ran `npm install` then you don't need to run it again. `npm start` will start the developement mode by invoking `webpack-dev-server`: the webpack development server that starts a lightweight local server at http://localhost:8080 from which the server will serve the client application
