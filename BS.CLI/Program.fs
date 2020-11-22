// Learn more about F# at http://fsharp.org

open System
open BS.Domain.DAL
open Amazon.DynamoDBv2
open BS.Domain.DAL.EventEnvelopeDal

let endpointDomain = "ddb-local"
let endpointPort = 8000
let endpoint = sprintf "http://%s:%d" endpointDomain endpointPort

let tableName = "EventSourceTable"

printfn "  -- Setting up a DynamoDB-Local client (DynamoDB Local seems to be running)" 
let ddbConfig = AmazonDynamoDBConfig ( ServiceURL = endpoint )
printfn "doing the work"

[<EntryPoint>]
let main argv =
    use client = new AmazonDynamoDBClient(ddbConfig)

    let dao = EngagementDao(client, tableName, "sample_user")

    // let id = "6e3d399a-861b-4b24-8b9b-f49d6d3dd065"
    // let version = "1"
    // let result = getEnvelope client tableName id version
    // printfn "Result: %A" result

    let item = dao.MakeSampleEngagement ()
    dao.CreateEngagement item |> ignore
    
    // EventEnvelopeDal.putEnvelope () |> ignore

    printfn "Hello World from F#!"
    0 // return an integer exit code
