namespace BS.Domain.DAL

open Amazon.DynamoDBv2.Model

open BS.Domain.EngagementManagement
open System.Collections.Generic
open BS.Domain.EngagementManagement
open BS.Domain.DAL.DataAccess

module EngagementEventDal =

    // type EngagementDetails2 = {
    //     TeamsName: string
    //     Region: string
    //     SfdcId: string
    //     Owner: string
    //     ProjectName: string
    // }

    // type FakeEvent2 =
    //     | Created of EngagementDetails2
    //     interface IEventSourcingEvent

    let appendOpt item list =
        match item with
        | None -> list
        | Some(value) -> value


// type EngagementDetails = {
//     CustomerName: string option
//     ProjectName: string option
//     SfdcProjectId: string option
//     SfdcProjectSlug: string option
//     SecurityOwner: string option
//     Team: string option    
//     Cti: CTI option
// }

    // Write interface

    let ctiToAttributes (cti:CTI) = 
        [   ("Category", ScalarString cti.Category)
            ("Type", ScalarString cti.Type)
            ("Item", ScalarString cti.Item) ]

    let engagementToAttributes (ee:EngagementDetails) =
        [ ("CustomerName", ScalarString ee.CustomerName)
          ("ProjectName", ScalarString ee.ProjectName)
          ("SfdcProjectId", ScalarString ee.SfdcProjectId)
          ("SfdcProjectSlug", ScalarString ee.SfdcProjectSlug)
          ("SecurityOwner", ScalarStringOpt ee.SecurityOwner)
          ("Team", ScalarStringOpt ee.Team)
          ("CTI", DocMapOpt <| Option.map ctiToAttributes ee.Cti) ]         

    let fakeEventToAttributes (fe:EngagementEvent) = 
        match fe with
        | Created ee ->     
            engagementToAttributes ee        
            // [ ("Action", ScalarString "Created")
            //   ("ActionVersion", ScalarString "1") ]
            // |> List.append (engagementToAttributes ee)
        | _ -> failwith "FakeEvent case not supported"

    // Read interface

    let buildCti category ``type`` item =
        { CTI.Category = category
          Type = ``type``
          Item = item}

    let readCti _ = 
        Reader.withBuilder buildCti
        |> Reader.readString "Category"
        |> Reader.readString "Type"
        |> Reader.readString "Item"

    let buildCreatedEvent 
            customerName
            projectName
            sfdcId
            sfdcProjectSlug
            securityOwner
            team
            cti :IEventSourcingEvent =
        { 
            EngagementDetails.CustomerName = customerName
            ProjectName = projectName
            SfdcProjectId = sfdcId
            SfdcProjectSlug = sfdcProjectSlug
            SecurityOwner = securityOwner
            Team = team
            Cti = cti
        } |> EngagementEvent.Created :> IEventSourcingEvent     

    let engagementCreatedReader :EventSourceReader = 
        Reader.withBuilder buildCreatedEvent
        |> Reader.readString "CustomerName"
        |> Reader.readString "ProjectName"
        |> Reader.readString "SfdcProjectId"
        |> Reader.readString "SfdcProjectSlug"
        |> Reader.readStringOpt "SecurityOwner"
        |> Reader.readStringOpt "Team"
        |> Reader.readNestedOpt "CTI" readCti


    type EngagementEventConverter () = 
        interface IEventConverter with
            member _.GetReader (``type``:string) (action:string) : EventSourceReader option =
                match ``type``, action with
                | "BS.Domain.EngagementManagement+EngagementEvent+Created", "Created" -> Some engagementCreatedReader
                | _ -> None
            member _.GetAttributes (event:IEventSourcingEvent) : list<Attr> option =
                match event with 
                | :? EngagementEvent as fe -> fakeEventToAttributes fe |> Some
                | _ -> None




