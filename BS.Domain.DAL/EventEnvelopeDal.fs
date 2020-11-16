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

open BS.Domain.DAL.EngagementEventDal
open BS.Domain.DAL.ReadWrite

module EventEnvelopeDal =

    type Envelope2 = {
        Id:string
        Version:string
        TimeStamp:string
        Event: IEventSourcingEvent
    }


    // Write interface
    let eventPayloadToAttributes (e:IEventSourcingEvent) = 
        match e with
        | :? FakeEvent2 as fe -> fakeEventToAttributes fe
        | _ -> failwith "IEventSourcingEvent type not supported"

    let eventToAttributes (ee:Envelope2)=
        [ Attr ("EngagementEventId", ScalarString ee.Id)
          Attr ("EngagementVersion", ScalarString ee.Version)
          Attr ("TimeStamp", ScalarString ee.TimeStamp)
          Attr ("Event", DocMap (eventPayloadToAttributes ee.Event) ) ]


    // Read interfaces
    let readPayloadSwtich action =      
        match action with  
        | "Create" -> readEventPayload
        | _ -> failwith "Unsupported action"             

    let buildEnvelope id version timeStamp event = 
        {
            Envelope2.Id = id
            Version = version
            TimeStamp = timeStamp
            Event = event
        }

    let readEnvelope =
        Reader.withBuilder buildEnvelope
        |> Reader.readString "EngagementEventId"
        |> Reader.readString "EngagementVersion"
        |> Reader.readString "TimeStamp"
        |> Reader.readNestedSwitch "Event" readPayloadSwtich "Action"


    let getEnvelope id version : Envelope2 =
      getItem tableName readEnvelope  [ Attr ("EngagementEventId", ScalarString id)
                                        Attr ("EngagementVersion", ScalarString version)]

    let envelope = {
        Envelope2.Id = System.Guid.NewGuid.ToString()
        Version = "1"
        TimeStamp = DateTime.Now.ToString()
        Event = {
            EngagementDetails2.Owner = "Fun Guy"
            ProjectName = "A major project"
            TeamsName = "flytrap and co"
            Region = "NARNIA"
            SfdcId = "PROJ-1234"
        } |> FakeEvent2.Created
    }

    let putEnvelope () = 
        putItem tableName <| eventToAttributes envelope