﻿module Frack.Specs.AppSpecs
open Frack
open Frack.Specs
open NaturalSpec

let errors, env = Env.create Fakes.context
let hdrs = Map.ofList [("Content_Type","text/plain");("Content_Length","5")] 
let body = seq { yield "Howdy" } 
let app (env:Environment) = ( 200, hdrs, body )

let head (app:Environment -> int * Map<string,string> * seq<string>) =
  fun env -> let status, hdrs, body = app env
             if env.HTTP_METHOD = "HEAD" then
               ( status, hdrs, Seq.empty )
             else
               ( status, hdrs, body )

[<Scenario>]
let ``When running an app that just returns pre-defined values, those values should be returned.``() =
  let ``running an app with predefined values`` (env:Environment) =
    printMethod "200, type = text/plain and length = 5, Howdy"
    app env
  Given env
  |> When ``running an app with predefined values``
  |> It should equal ( 200, hdrs, body )
  |> Verify

let ``running a middleware for a`` (m:string) (env:Environment) =
  printMethod m
  let e = { env with HTTP_METHOD = m }
  head app e

[<Scenario>]
let ``When running a middleware on an app handling a GET request, the body should be left alone.``() =
  Given env
  |> When ``running a middleware for a`` "GET"
  |> It should have (fun result -> match result with _, _, bd -> bd = body)
  |> Verify

[<Scenario>]
let ``When running a middleware on an app handling a HEAD request, the body should be empty.``() =
  Given env
  |> When ``running a middleware for a`` "HEAD"
  |> It should have (fun result -> match result with _, _, bd -> bd = Seq.empty)
  |> Verify