namespace BS.Domain.DAL


open System

open BS.Domain.Common
open BS.Domain.DAL.DataAccess
open Microsoft.FSharp.Reflection
open Amazon.DynamoDBv2
open System.Collections.Generic
open Amazon.DynamoDBv2.Model

module EventEnvelopeDal =

    type EventEnvelopeDao (eventConverters:IEventConverter list, client:AmazonDynamoDBClient, userName:string) =
        let tableName = "EventSourceTable"

        (***************************** 
         * Writing event envelopes 
         *****************************)

        let eventToActionName (event:IEventSourcingEvent) = 
            let case, _ = FSharpValue.GetUnionFields(event, event.GetType())
            case.Name

        let eventToTypeName (e:IEventSourcingEvent) =
            e.GetType().FullName

        let executeOrFail (f:'a -> 'b option) (error:string) (eventConverters:'a list) = 
            let rec executeOrFail' = function            
            | eventConverter :: tail -> 
                match f eventConverter with
                | None -> executeOrFail' tail 
                | Some(attrs) -> attrs
            | [] -> failwith error

            executeOrFail' eventConverters

        // Write interface
        let eventPayloadToAttributes (e:IEventSourcingEvent) = 
            let getAttributes (m:IEventConverter) = m.GetAttributes e
            eventConverters |> executeOrFail getAttributes "Converter not found"

        let eventToAttributes (ee:EvtEnvelope) =
            [ ("EventId", ScalarString ee.Id)
              ("EventVersion", ScalarString ee.Version)          
              ("UserName", ScalarString ee.UserName)
              ("TimeStamp", ScalarString ee.TimeStamp)
              ("Type", ScalarString (eventToTypeName ee.Event))
              ("Action", ScalarString (eventToActionName ee.Event))
              ("Event", DocMap (eventPayloadToAttributes ee.Event)) ]


        (***************************** 
         * Reading event envelopes 
         *****************************)

        let chooseEventReader ``type`` action =
            let getReader (m:IEventConverter) = m.GetReader ``type`` action
            eventConverters |> executeOrFail getReader "Converter not found"

        let getEventReader = 
            (Reader.withBuilder chooseEventReader
            |> Reader.readString "Type"
            |> Reader.readString "Action") 

        let buildEnvelope id version userName timeStamp event = 
            {
                EvtEnvelope.Id = id
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
            |> Reader.selectNestedReader "Event" getEventReader

        (***************************** 
         * Using event envelopes 
         *****************************)

        member _.GetEnvelope id version : EvtEnvelope =
          getItem client tableName readEnvelope  
            [ ("EventId", ScalarString id)
              ("EventVersion", ScalarString version)]

        member _.GetEnvelopes id : EvtEnvelope list =
          getItems client tableName readEnvelope id

        member _.InsertEventEnvelope (envelope:EvtEnvelope) = 
            putItem client tableName <| eventToAttributes envelope

        member _.EnvelopCommand id command :CmdEnvelope=
            {
                CmdEnvelope.Id = id
                UserName = userName
                TimeStamp = DateTime.Now.ToString()
                Command = command
            }

        member _.EnvelopEvent id version event =
            {
                EvtEnvelope.Id = id
                Version = version
                UserName = userName
                TimeStamp = DateTime.Now.ToString()
                Event = event
            }

    type EventEnvelopeDaoFactory = AmazonDynamoDBClient -> string -> EventEnvelopeDao

    let createEventEnvelopeFactory (eventConverters:IEventConverter list) : EventEnvelopeDaoFactory=
        fun client userName -> EventEnvelopeDao (eventConverters, client, userName)

    let buildState<'Event, 'State when 'Event :> IEventSourcingEvent> (evolver:DomainEvolver<'Event, 'State>) (envDao:EventEnvelopeDao) id = 
        let envs = envDao.GetEnvelopes id
        let engagementVersion = 
            envs
            |> List.maxBy (fun env -> int env.Version)
            |> fun env -> int env.Version
            |> fun version -> ((version+1).ToString ())
        let state = 
            envs
            |> List.map (fun env -> env.Event :?> 'Event)
            |> List.fold evolver None
        engagementVersion, state
