namespace BS.Domain.DAL

open Amazon.DynamoDBv2.Model

open System.Collections.Generic
open BS.Domain.EngagementManagement
open BS.Domain.DAL.DataAccess

module EngagementEventDal =

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

    let appendOpt item list =
        match item with
        | None -> list
        | Some(value) -> value

    // Write interface
    let engagementToAttributes (ee:EngagementDetails2)=
        [ ("Owner", ScalarString ee.Owner)
          ("ProjectName", ScalarString ee.ProjectName)
          ("Region", ScalarString ee.Region)
          ("SfdcId", ScalarString ee.SfdcId)
          ("TeamsName", ScalarString ee.TeamsName) ]

    let fakeEventToAttributes (fe:FakeEvent2) = 
        match fe with
        | Created ee ->     
            engagementToAttributes ee        
            // [ ("Action", ScalarString "Created")
            //   ("ActionVersion", ScalarString "1") ]
            // |> List.append (engagementToAttributes ee)
        | _ -> failwith "FakeEvent case not supported"

    // Read interface

    let buildCreatedEvent owner projectName teamsName region sfdcId :IEventSourcingEvent =
        { 
            EngagementDetails2.Owner = owner
            ProjectName = projectName
            TeamsName = teamsName
            Region = region
            SfdcId = sfdcId
        } |> FakeEvent2.Created :> IEventSourcingEvent     

    let engagementCreatedReader :EventSourceReader = 
        Reader.withBuilder buildCreatedEvent
        |> Reader.readString "Owner"
        |> Reader.readString "ProjectName"
        |> Reader.readString "TeamsName"
        |> Reader.readString "Region"
        |> Reader.readString "SfdcId"


    type EngagementEventConverter () = 
        interface IEventConverter with
            member _.GetReader (``type``:string) (action:string) : EventSourceReader option =
                match ``type``, action with
                | "FakeEvent2", "Created" -> Some engagementCreatedReader
                | _ -> None
            member _.GetAttributes (event:IEventSourcingEvent) : list<Attr> option =
                match event with 
                | :? FakeEvent2 as fe -> fakeEventToAttributes fe |> Some
                | _ -> None




