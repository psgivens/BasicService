namespace BS.Domain.DAL

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Net
open BS.Domain.EngagementManagement

module DataAccess =

    type Attr = (string * AttrValue)
    and AttrValue =
      | ScalarString of string
      | ScalarDecimal of decimal
      | ScalarBinary of string
      | ScalarBool of bool
      | ScalarStringOpt of string option
      | ScalarDecimalOpt of decimal option
      | ScalarBoolOpt of bool option
      | ScalarNull
      | SetString of string Set
      | SetDecimal of decimal Set
      | SetBinary of string Set
      | DocList of AttrValue list
      | DocMap of Attr list
      | DocMapOpt of Attr list option

    type Attributes = Dictionary<string,AttributeValue>

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
      | ScalarStringOpt s  -> AttributeValue (S = Option.get s )
      | ScalarDecimalOpt n -> AttributeValue (N = string (Option.get n) )
      | ScalarBoolOpt b    -> AttributeValue (BOOL = Option.get b)
      | DocMapOpt m        -> AttributeValue (M = mapAttrsToDictionary (Option.get m))

    and mapAttr (name, value) =
      name, mapAttrValue value

    and mapAttrsToDictionary =
      let removeNone = function
        | _, ScalarStringOpt None -> false
        | _, ScalarDecimalOpt None -> false
        | _, ScalarBoolOpt None -> false
        | _,_ -> true
      List.filter removeNone >> List.map mapAttr >> dict >> Attributes

    type Reader<'a, 'b> = Reader of ('a -> 'b)

    module Reader =
      let run (Reader f) a = f a            
      let apply r r' = Reader (fun a -> run r a |> run r' a) // Reader applicative function
      let withBuilder f = Reader (fun _ -> f)

      let _extract key f (d:Attributes) = f (d.[key])
      let _extractOpt key (f:AttributeValue -> 'a) (d:Attributes) = 
        match d.TryGetValue key with
        | true, value -> Some (f value)
        | _, _ -> None

      let readString key = _extract key (fun attr -> attr.S) |> Reader |> apply
      let readBool   key = _extract key (fun attr -> attr.BOOL) |> Reader |> apply
      let readNumber key f = _extract key (fun attr -> attr.N) >> f |> Reader |> apply
      let readStringOpt key = _extractOpt key (fun attr -> attr.S) |> Reader |> apply
      let readBoolOpt   key = _extractOpt key (fun attr -> attr.BOOL) |> Reader |> apply
      let readNumberOpt key f = _extractOpt key (fun attr -> attr.N) >> f |> Reader |> apply

      // Working
      let selectNestedReader key subReader = 
        let readNested = run subReader
        let readDoc = _extract key (fun attr -> attr.M)
        ((fun d' -> readDoc d' |> run (readNested d')) |> Reader |> apply)

      // May not work. I've never tried
      let selectNestedReaderOpt key subReader = 
        let readNested = run subReader
        let readDoc = _extractOpt key (fun attr -> attr.M)
        ((fun d' -> readDoc d' |> Option.map (run (readNested d'))) |> Reader |> apply)

      // may not owrk. I've never tried.
      let readNested key subReader = 
        let readNested = subReader
        let readDoc = _extract key (fun attr -> attr.M)
        ((fun d' -> readDoc d' |> run (subReader d')) |> Reader |> apply)

      // Working
      let readNestedOpt key subReader = 
        let readNested = subReader
        let readDoc = _extractOpt key (fun attr -> attr.M)
        ((fun d' -> readDoc d' |> Option.map (run (subReader d'))) |> Reader |> apply)

    let putItem (client:AmazonDynamoDBClient) (tableName:string) fields : Result<Unit, string> =
      PutItemRequest (tableName, mapAttrsToDictionary fields)
      |> client.PutItemAsync
      |> Async.AwaitTask
      |> Async.RunSynchronously
      |> fun r ->
        match r.HttpStatusCode with
        | HttpStatusCode.OK -> Ok ()
        | status -> Error <| sprintf "Unexpected status code '%A'" status

    let getItem (client:AmazonDynamoDBClient) tableName (reader:Reader<Attributes,'a>) fields =
      GetItemRequest (tableName, mapAttrsToDictionary fields)
      |> client.GetItemAsync
      |> Async.AwaitTask
      |> Async.RunSynchronously
      |> fun r -> r.Item
      |> Reader.run reader

    let getItems (client:AmazonDynamoDBClient) tableName (reader:Reader<Attributes,'a>) id' = 
      QueryRequest (
        TableName = tableName,
        KeyConditionExpression = "EventId = :v_Id",
        ExpressionAttributeValues = mapAttrsToDictionary [ (":v_Id", ScalarString id') ]
      )
      |> client.QueryAsync
      |> Async.AwaitTask
      |> Async.RunSynchronously
      |> fun r -> r.Items
      |> List.ofSeq
      |> List.map (Reader.run reader)

    type EventSourceReader = Reader<Attributes,IEventSourcingEvent>

    type IEventConverter = 
        abstract member GetReader : ``type``:string -> action:string -> EventSourceReader option
        abstract member GetAttributes : event:IEventSourcingEvent -> list<Attr> option


