// Learn more about F# at http://fsharp.org

open System
open BS.Domain.DAL
open BS.Domain.DAL.FirstDAL
open BS.Domain.DAL.ReadWrite

[<EntryPoint>]
let main argv =
    let id = "4e53920c-505a-4a90-a694-b9300791f0ae"
    let version = "1"
    FirstDAL.hello2 "sharon"
    let result = ReadWrite.getEnvelope id version
    printfn "Result: %A" result
    Say.hello "Phillip Scott Givens"
    printfn "Hello World from F#!"
    0 // return an integer exit code
