namespace BS.Domain.DAL

open BS.Domain.EngagementManagement

open System

open BS.Domain.DAL.EngagementEventDal
open BS.Domain.DAL.ReadWrite
open Microsoft.FSharp.Reflection

module EventEnvelopeDal =

    type Envelope = {
        Id:string
        Version:string
        UserName:string
        TimeStamp:string
        Event: IEventSourcingEvent
    }


    (***************************** 
     * Writing event envelopes 
     *****************************)

    let eventToActionName (event:IEventSourcingEvent) = 
        let case, _ = FSharpValue.GetUnionFields(event, event.GetType())
        case.Name

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
          Attr ("Type", ScalarString (eventToTypeName ee.Event))
          Attr ("Action", ScalarString (eventToActionName ee.Event))
          Attr ("Event", DocMap (eventPayloadToAttributes ee.Event)) ]


    (***************************** 
     * Reading event envelopes 
     *****************************)

    let chooseEventReader ``type`` action =
        match ``type``, action with  
        | "FakeEvent2", "Created" -> engagementCreatedReader
        | _, _ -> failwith "Unsupported action"             

    let getEventReader =  //:Reader<Dictionary<string,AttributeValue>,Reader<Dictionary<string,AttributeValue>,IEventSourcingEvent>> = 
        (Reader.withBuilder chooseEventReader
        |> Reader.readString "Type"
        |> Reader.readString "Action") 

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
        |> Reader.readNested "Event" getEventReader

    (***************************** 
     * Using event envelopes 
     *****************************)

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