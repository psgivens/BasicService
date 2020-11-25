// Learn more about F# at http://fsharp.org

open System
open BS.Domain.DAL
open Amazon.DynamoDBv2
open BS.Domain.DAL.EventEnvelopeDal
open BS.Domain.DAL.EngagementEventDal

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

    let envDao = EventEnvelopeDao ([EngagementEventConverter ()], client, tableName, "sample_user")

    let id = "c292cd6c-4d32-461d-bff9-7b5f3bfae82b"
    let version = "1"
    let result = envDao.GetEnvelope id version
    printfn "Result: %A" result

    // let dao = EngagementDao envDao
    // let item = dao.MakeSampleEngagement ()
    // dao.CreateEngagement item |> ignore
    
    // EventEnvelopeDal.putEnvelope () |> ignore

    printfn "Hello World from F#!"
    0 // return an integer exit code
