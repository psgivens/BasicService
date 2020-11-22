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
open Microsoft.FSharp.Reflection

module EventEnvelopeDal =

    // TODO: Add Action and ActionVersion to envelope
    // TODO: Rewrite swtich logic to use the envelope action fields
    type Envelope = {
        Id:string
        Version:string
        UserName:string
        TimeStamp:string
        Event: IEventSourcingEvent
    }

    let eventToActionName (x:'a) = 
        match FSharpValue.GetUnionFields(x, x.GetType()) with
        | case, _ -> case.Name

    let eventToTypeName (e:IEventSourcingEvent) =
        e.GetType().Name

    // Write interface
    let eventPayloadToAttributes (e:IEventSourcingEvent) = 
        match e with
        | :? FakeEvent2 as fe -> fakeEventToAttributes fe
        | _ -> failwith "IEventSourcingEvent type not supported"

    let eventToAttributes (ee:Envelope)=
        [ Attr ("EventId", ScalarString ee.Id)
          Attr ("EventVersion", ScalarString ee.Version)          
          Attr ("UserName", ScalarString ee.UserName)
          Attr ("TimeStamp", ScalarString ee.TimeStamp)
          Attr ("Type", ScalarString (eventToActionName ee.Event))
          Attr ("Action", ScalarString (eventToTypeName ee.Event))
          Attr ("Event", DocMap (eventPayloadToAttributes ee.Event)) ]


    // Read interfaces
    let readPayloadSwtich action =      
        match action with  
        | "Create" -> readEventPayload
        | _ -> failwith "Unsupported action"             

    let buildEnvelope id version userName timeStamp event = 
        {
            Envelope.Id = id
            Version = version
            UserName = userName
            TimeStamp = timeStamp
            Event = event
        }

    let readEnvelope =
        Reader.withBuilder buildEnvelope
        |> Reader.readString "EventId"
        |> Reader.readString "EventVersion"
        |> Reader.readString "UserName"
        |> Reader.readString "TimeStamp"
        |> Reader.readNestedSwitch "Event" readPayloadSwtich "Action"


    let getEnvelope client tableName id version : Envelope =
      getItem 
        client tableName readEnvelope  
        [ Attr ("EventId", ScalarString id)
          Attr ("EventVersion", ScalarString version)]

    let insertEventEnvelope client tableName (envelope:Envelope) = 
        putItem client tableName <| eventToAttributes envelope

    let envelop userName id version event =
        {
            Envelope.Id = id
            Version = version
            UserName = userName
            TimeStamp = DateTime.Now.ToString()
            Event = event
        }