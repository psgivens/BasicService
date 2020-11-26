namespace BS.Domain.DAL

open BS.Domain.EngagementManagement
open BS.Domain.DAL.DataAccess

module EngagementEventDal =

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
              ("SecurityOwner", ScalarStringOpt ee.SecurityOwner)
              ("Team", ScalarStringOpt ee.Team)
              ("CTI", DocMapOpt <| Option.map ctiToAttributes ee.Cti) ]         

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

        let buildCreatedEvent customerName projectName sfdcId sfdcProjectSlug securityOwner team cti :IEventSourcingEvent =
            { 
                EngagementCreatedDetails.CustomerName = customerName
                ProjectName = projectName
                SfdcProjectId = sfdcId
                SfdcProjectSlug = sfdcProjectSlug
                SecurityOwner = securityOwner
                Team = team
                Cti = cti
            } 
            |> Created 
            :> IEventSourcingEvent

        let engagementCreatedReader :EventSourceReader = 
            Reader.withBuilder buildCreatedEvent
            |> Reader.readString "CustomerName"
            |> Reader.readString "ProjectName"
            |> Reader.readString "SfdcProjectId"
            |> Reader.readString "SfdcProjectSlug"
            |> Reader.readStringOpt "SecurityOwner"
            |> Reader.readStringOpt "Team"
            |> Reader.readNestedOpt "CTI" readCti


        interface IEventConverter with
            member _.GetReader (``type``:string) (action:string) : EventSourceReader option =
                match ``type``, action with
                | "BS.Domain.EngagementManagement+EngagementEvent+Created", "Created" -> Some engagementCreatedReader
                | _ -> None
            member _.GetAttributes (event:IEventSourcingEvent) : list<Attr> option =
                match event with 
                | :? EngagementEvent as fe -> engagementEventToAttributes fe |> Some
                | _ -> None




