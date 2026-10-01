#load "../scripts/common.fsx"

let arguments = fsi.CommandLineArgs |> Array.skip 1 |> Array.toList
Helper.runDotnet ([ "run"; "--project"; "_build/_build.csproj"; "--configuration"; "Release"; "--" ] @ arguments)
