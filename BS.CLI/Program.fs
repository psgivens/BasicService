// Learn more about F# at http://fsharp.org

open System
open BS.Domain.DAL



[<EntryPoint>]
let main argv =
    let id = "4e53920c-505a-4a90-a694-b9300791f0ae"
    let version = "1"
    let result = EventEnvelopeDal.getEnvelope id version
    printfn "Result: %A" result

    EventEnvelopeDal.putEnvelope () |> ignore

    printfn "Hello World from F#!"
    0 // return an integer exit code
