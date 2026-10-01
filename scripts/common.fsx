module Helper

open System.Diagnostics
open System.IO

let root = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, ".."))

let runDotnet (arguments: string list) =
    let start = ProcessStartInfo("dotnet")
    start.WorkingDirectory <- root
    start.UseShellExecute <- false
    for argument in arguments do
        start.ArgumentList.Add(argument)
    use child = Process.Start(start)
    child.WaitForExit()
    if child.ExitCode <> 0 then
        exit child.ExitCode
