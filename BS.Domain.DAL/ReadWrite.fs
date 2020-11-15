namespace BS.Domain.DAL
open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.DataModel
open Amazon.DynamoDBv2.DocumentModel
open Amazon.DynamoDBv2.Model

open System.Collections.Generic
open BS.Domain.EngagementManagement

open System
open System.IO
open System.IO.Compression
open System.Net


module ReadWrite =

    let endpointDomain = "ddb-local"
    let endpointPort = 8000
    let endpoint = sprintf "http://%s:%d" endpointDomain endpointPort

    let tableName = "EngagementEventsTable"


    printfn "  -- Setting up a DynamoDB-Local client (DynamoDB Local seems to be running)" 
    let ddbConfig = AmazonDynamoDBConfig ( ServiceURL = endpoint )
    let client = new AmazonDynamoDBClient(ddbConfig)
    printfn "doing the work"


    type Attr =
      | Attr of name:string * AttrValue
    and  AttrValue =
      | ScalarString of string
      | ScalarDecimal of decimal
      | ScalarBinary of string
      | ScalarBool of bool
      | ScalarNull
      | SetString of string Set
      | SetDecimal of decimal Set
      | SetBinary of string Set
      | DocList of AttrValue list
      | DocMap of Attr list

    let toGzipMemoryStream (s:string) =
      let output = new MemoryStream ()
      use zipStream = new GZipStream (output, CompressionMode.Compress, true)
      use writer = new StreamWriter (zipStream)
      writer.Write s
      output      

    let rec mapAttrValue = function
      | ScalarString s  -> AttributeValue (S = s)
      | ScalarDecimal n -> AttributeValue (N = string n)
      | ScalarBinary s  -> AttributeValue (B = toGzipMemoryStream s)
      | ScalarBool b    -> AttributeValue (BOOL = b)
      | ScalarNull      -> AttributeValue (NULL = true)
      | SetString ss    -> AttributeValue (SS = ResizeArray ss)
      | SetDecimal ns   -> AttributeValue (NS = ResizeArray (Seq.map string ns))
      | SetBinary bs    -> AttributeValue (BS = ResizeArray (Seq.map toGzipMemoryStream bs))
      | DocList l       -> AttributeValue (L = ResizeArray (List.map mapAttrValue l))
      | DocMap m        -> AttributeValue (M = mapAttrsToDictionary m)

    and mapAttr (Attr (name, value)) =
      name, mapAttrValue value

    and mapAttrsToDictionary =
      List.map mapAttr >> dict >> Dictionary<string,AttributeValue>

    let putItem tableName fields : Result<Unit, string> =
      use client = new AmazonDynamoDBClient()
      PutItemRequest (tableName, mapAttrsToDictionary fields)
      |> client.PutItemAsync
      |> Async.AwaitTask
      |> Async.RunSynchronously
      |> fun r ->
        match r.HttpStatusCode with
        | HttpStatusCode.OK -> Ok ()
        | status -> Error <| sprintf "Unexpected status code '%A'" status


    let attributes =      
        [ "EngagementEventId", AttributeValue (S = "4e53920c-505a-4a90-a694-b9300791f0ae")
          "EngagementVersion", AttributeValue (S = "2")]
        |> dict
        |> Dictionary<string,AttributeValue>

    let response : GetItemResponse =
        GetItemRequest (tableName, attributes)
        |> client.GetItemAsync
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let item : Dictionary<string,AttributeValue> =
      response.Item




    type Reader<'a, 'b> =
      Reader of ('a -> 'b)

    module Reader =

      let run (Reader f) a =
        f a

      let retn a =
        Reader (fun _ -> a)

      let map f r =
        Reader (fun a -> run r a |> f)

      let apply r' r = // Reader applicative function
        Reader (fun a -> run r a |> run r' a)











        
    type Order =
      { Name : string
        Description : string
        IsVerified : bool
        Quantity : int
        Cost : float }

    let buildOrder name desc isVerified qty cost =
      { Name = name
        Description = desc
        IsVerified = isVerified
        Quantity = qty
        Cost = cost }




    let getItem tableName reader fields =
      GetItemRequest (tableName, mapAttrsToDictionary fields)
      |> client.GetItemAsync
      |> Async.AwaitTask
      |> Async.RunSynchronously
      |> fun r -> r.Item
      |> Reader.run reader

    let (<!>) = Reader.map
    let (<*>) = Reader.apply

    let extract f (d:Dictionary<string,AttributeValue>) = f d
    let readString key   = extract (fun d -> d.[key].S) |> Reader
    let readBool   key   = extract (fun d -> d.[key].BOOL) |> Reader
    let readNumber key f = extract (fun d -> d.[key].N) >> f |> Reader


    let readOrder =
      buildOrder
      <!> readString "name"
      <*> readString "description"
      <*> readBool   "isVerified"
      <*> readNumber "quantity" int
      <*> readNumber "cost" float

    let getOrder id : Order =
      getItem "orders" readOrder [ Attr ("id", ScalarString id) ]


    type EngagementDetails2 = {
        TeamsName: string
        Region: string
        SfdcId: string
        Owner: string
        ProjectName: string
    }

    type FakeEvent2 =
        | Created of EngagementDetails2
        interface IEventSourcingEvent

    type Envelope2 = {
        Id:string
        Version:string
        TimeStamp:string
        Event: IEventSourcingEvent
    }

    let buildEnvelope id version timeStamp event = 
        {
            Envelope2.Id = id
            Version = version
            TimeStamp = timeStamp
            Event = event
        }

    let buildCreatedEvent owner projectName teamsName region sfdcId :IEventSourcingEvent =
        { 
            EngagementDetails2.Owner = owner
            ProjectName = projectName
            TeamsName = teamsName
            Region = region
            SfdcId = sfdcId
        } |> FakeEvent2.Created :> IEventSourcingEvent

    let buildCreatedEvent2 owner projectName teamsName region sfdcId foobar:IEventSourcingEvent =
        { 
            EngagementDetails2.Owner = owner
            ProjectName = projectName
            TeamsName = teamsName
            Region = region
            SfdcId = sfdcId
        } |> FakeEvent2.Created :> IEventSourcingEvent

    let readNested2 key f = extract (fun d -> d.[key].M) >> Reader.run f |> Reader

    let buildEventPayload = function  
        | "Create" -> buildCreatedEvent 
        | _ -> failwith "Unsupported type found"

    let readEventPayload = 
        buildEventPayload
        <!> readString "Action"
        <*> readString "Owner"
        <*> readString "ProjectName"
        <*> readString "TeamsName"
        <*> readString "Region"
        <*> readString "SFDCID"
       
    let readNested key f = extract (fun d -> d.[key].M) >> f |> Reader
    let readNestedSwitch key f subkey = 
      let getDocument = extract (fun d -> d.[key].M)
      let chooseNested (d:Dictionary<string,AttributeValue>) = Reader.run (f d.[subkey].S) d
      getDocument >> chooseNested |> Reader

    let readPayloadSwtich action =      
        match action with  
        | "Create" -> readEventPayload
        | _ -> failwith "Unsupported action"             

    let readEnvelope =
        buildEnvelope
        <!> readString "EngagementEventId"
        <*> readString "EngagementVersion"
        <*> readString "TimeStamp"
        <*> readNestedSwitch "Event" readPayloadSwtich "Action"

    let getEnvelope id version : Envelope2 =
      getItem tableName readEnvelope  [ Attr ("EngagementEventId", ScalarString id)
                                        Attr ("EngagementVersion", ScalarString version)]





