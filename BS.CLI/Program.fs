// Learn more about F# at http://fsharp.org

open System
open BS.Domain.DAL
open BS.Domain.DAL.FirstDAL


[<EntryPoint>]
let main argv =
    FirstDAL.hello2 "sharon"
    Say.hello "Phillip Scott Givens"
    printfn "Hello World from F#!"
    0 // return an integer exit code
