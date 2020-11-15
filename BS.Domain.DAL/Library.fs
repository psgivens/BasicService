namespace BS.Domain.DAL

// open System
// open System.Net
// open System.Net.NetworkInformation
open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.DataModel
open Amazon.DynamoDBv2.DocumentModel
open Amazon.DynamoDBv2.Model

open System.Collections.Generic
open BS.Domain.EngagementManagement

open System.IO
open System.IO.Compression

module FirstDAL = 

    type EngagementDetails1 = {
        TeamsName: string
        Region: string
        SfdcID: string
        Owner: string
        ProjectName: string
    }

    type FakeEvent1 =
        | Created of EngagementDetails1
        interface IEventSourcingEvent

    type Envelope = {
        Id:string
        Version:string
        TimeStamp:string
        Event: IEventSourcingEvent
    }

    let hello2 name =
        
        let endpointDomain = "ddb-local"
        let endpointPort = 8000
        let endpoint = sprintf "http://%s:%d" endpointDomain endpointPort

        let tableName = "EngagementEventsTable"

        printfn "  -- Setting up a DynamoDB-Local client (DynamoDB Local seems to be running)" 
        let ddbConfig = AmazonDynamoDBConfig ( ServiceURL = endpoint )
        let client = new AmazonDynamoDBClient(ddbConfig)
        printfn "doing the work"

        let table = Table.LoadTable(client, tableName)
        
        let attributes =
          [ "EngagementEventId", AttributeValue (S = "4e53920c-505a-4a90-a694-b9300791f0ae"); 
            "EngagementVersion", AttributeValue (S = "2")]
          |> dict
          |> Dictionary<string,AttributeValue>

        let response = 
            GetItemRequest(tableName, attributes)
            |> client.GetItemAsync
            |> Async.AwaitTask
            |> Async.RunSynchronously

        

        printfn "%A" response.Item.Keys

    // let response : GetItemResponse =
    //   new GetItemRequest ("my_table_name", attributes)
    //   |> client.GetItemAsync
    //   |> Async.AwaitTask
    //   |> Async.RunSynchronously

    // let item : Dictionary<string,AttributeValue> =
    //   response.Item

    //     let scanFilter = ScanFilter()
    //     scanFilter.AddCondition ("ForumId", ScanOperator.Equal, 101);
    //     scanFilter.AddCondition ("Tags", ScanOperator.Contains, "sortkey");

    //     Search search = ThreadTable.Scan (scanFilter);
       
        let toEventPayload (action:string, payload:Dictionary<string,AttributeValue>) = 
            { 
                EngagementDetails1.Owner = payload.["Owner"].S
                ProjectName = payload.["ProjectName"].S
                TeamsName = payload.["TeamsName"].S
                Region = payload.["Region"].S
                SfdcID = payload.["SFDCID"].S
            }

        let toEngagementEvent (item:Dictionary<string,AttributeValue>) = 
            let action = item.["Event"].M.["Action"].S
            let payload = item.["Event"].M.["Payload"].M
            {
                Envelope.Id = item.["EngagementEventId"].S
                Version = item.["EngagementVersion"].S
                TimeStamp = item.["TimeStamp"].S
                Event = FakeEvent1.Created (toEventPayload (action, payload))
            }

        let envelope = toEngagementEvent response.Item 
    
        printfn "Hello %A" envelope



module Say =
    let hello name =
        printfn "Hello %s" name
