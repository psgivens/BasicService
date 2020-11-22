namespace BS.Domain.DAL

open Amazon.DynamoDBv2.Model

open System.Collections.Generic
open BS.Domain.EngagementManagement
open BS.Domain.DAL.ReadWrite

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


    // Write interface
    let engagementToAttributes (ee:EngagementDetails2)=
        [ Attr ("Owner", ScalarString ee.Owner)
          Attr ("ProjectName", ScalarString ee.ProjectName)
          Attr ("Region", ScalarString ee.Region)
          Attr ("SfdcId", ScalarString ee.SfdcId)
          Attr ("TeamsName", ScalarString ee.TeamsName)
        ]

    let fakeEventToAttributes (fe:FakeEvent2) = 
        match fe with
        | Created ee ->             
            [ Attr ("Action", ScalarString "Created")
              Attr ("ActionVersion", ScalarString "1") ]
            |> List.append (engagementToAttributes ee)
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

    let readEventPayload :Reader<Dictionary<string,AttributeValue>,IEventSourcingEvent> = 
        Reader.withBuilder buildCreatedEvent
        |> Reader.readString "Owner"
        |> Reader.readString "ProjectName"
        |> Reader.readString "TeamsName"
        |> Reader.readString "Region"
        |> Reader.readString "SFDCID"


