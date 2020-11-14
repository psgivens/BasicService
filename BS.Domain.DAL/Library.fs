namespace BS.Domain.DAL

// open System
// open System.Net
// open System.Net.NetworkInformation
open Amazon.DynamoDBv2

module FirstDAL = 
    let endpointDomain = "localhost"
    let endpointPort = 8000
    let endpoint = sprintf "http://%s:%d" endpointDomain endpointPort

    printfn "  -- Setting up a DynamoDB-Local client (DynamoDB Local seems to be running)" 
    let ddbConfig = AmazonDynamoDBConfig ( ServiceURL = endpoint )
    let client = new AmazonDynamoDBClient(ddbConfig)
    printfn "doing the work"


module Say =
    let hello name =
        printfn "Hello %s" name
