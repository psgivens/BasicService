namespace BS.Domain.DAL

open BS.Domain.Common
open BS.Domain.EngagementManagement
open BS.Domain.DAL.DataAccess

module EngagementEventDal =

    let ctiToAttributes (cti:CTI) = 
        [   ("Category", ScalarString cti.Category)
            ("Type", ScalarString cti.Type)
            ("Item", ScalarString cti.Item) ]
            
    type EngagementEventConverter () = 

        (**************************
         * Write interface
         **************************)
        let ctiToAttributes (cti:CTI) = 
            [   ("Category", ScalarString cti.Category)
                ("Type", ScalarString cti.Type)
                ("Item", ScalarString cti.Item) ]

        let engagementToAttributes (ee:EngagementCreatedDetails) =
            [ ("CustomerName", ScalarString ee.CustomerName)
              ("ProjectName", ScalarString ee.ProjectName)
              ("SfdcProjectId", ScalarString ee.SfdcProjectId)
              ("SfdcProjectSlug", ScalarString ee.SfdcProjectSlug)
              ("SecurityOwner", ScalarString ee.SecurityOwner)
              ("Team", ScalarString ee.Team)
              ("CTI", DocMap <| ctiToAttributes ee.Cti) ]         

        let engagementEventToAttributes (event:EngagementEvent) = 
            match event with
            | Created details -> engagementToAttributes details
            | _ -> failwith "EngagementEvent case not supported"

        (**************************
         * Read interface
         **************************)
        let buildCti category ``type`` item =
            { CTI.Category = category
              Type = ``type``
              Item = item}

        let readCti _ = 
            Reader.withBuilder buildCti
            |> Reader.readString "Category"
            |> Reader.readString "Type"
            |> Reader.readString "Item"

        let buildCreatedEvent troopId customerName projectName sfdcId sfdcProjectSlug securityOwner team cti :IEventSourcingEvent =
            { 
                EngagementCreatedDetails.CustomerName = customerName
                ProjectName = projectName
                SfdcProjectId = sfdcId
                SfdcProjectSlug = sfdcProjectSlug
                SecurityOwner = securityOwner
                Team = team
                Cti = cti
                TroopId = troopId
            } 
            |> Created 
            :> IEventSourcingEvent

        let engagementCreatedReader :EventSourceReader = 
            Reader.withBuilder buildCreatedEvent
            |> Reader.readString "TroopId"
            |> Reader.readString "CustomerName"
            |> Reader.readString "ProjectName"
            |> Reader.readString "SfdcProjectId"
            |> Reader.readString "SfdcProjectSlug"
            |> Reader.readString "SecurityOwner"
            |> Reader.readString "Team"
            |> Reader.readNested "CTI" readCti

        interface IEventConverter with
            member _.GetReader (``type``:string) (action:string) : EventSourceReader option =
                match ``type``, action with
                | "BS.Domain.EngagementManagement+EngagementEvent+Created", "Created" -> Some engagementCreatedReader
                | _ -> None
            member _.GetAttributes (event:IEventSourcingEvent) : list<Attr> option =
                match event with 
                | :? EngagementEvent as fe -> engagementEventToAttributes fe |> Some
                | _ -> None




