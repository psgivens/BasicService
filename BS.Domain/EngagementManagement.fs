module BS.Domain.EngagementManagement

open FSharp.Control.Tasks.V2
open System.Threading.Tasks

type IEventSourcingEvent = interface end

type CTI = {
    Category: string
    Type: string
    Item: string
}

type EngagementDetails = {
    CustomerName: string option
    ProjectName: string option
    SfdcProjectId: string option
    SfdcProjectSlug: string option
    SecurityOwner: string option
    Team: string option    
    Cti: CTI option
}

type EngagementCommand =
    | Create of EngagementDetails
    | Update of EngagementDetails

type EngagementEvent =
    | Created of EngagementDetails
    | Updated of EngagementDetails
    interface IEventSourcingEvent

type EngagementState = {
    CustomerName: string 
    ProjectName: string 
    SfdcProjectId: string 
    SfdcProjectSlug: string 
    SecurityOwner: string 
    Team: string  
    Cti: CTI 
}

let hanlde (state: EngagementState option) (cmd: EngagementCommand) : Task<IEventSourcingEvent list> =
    task {
        return 
            match state, cmd with 
            | None, EngagementCommand.Create details -> [ EngagementEvent.Created details ] 
            | Some state', EngagementCommand.Update details -> [ EngagementEvent.Updated details ]
            | _,_ -> failwith "invalid command"
        }

let evolve (state: EngagementState option) (event:EngagementEvent) =
    match state, event with
    | None, EngagementEvent.Created details -> {
            EngagementState.CustomerName = details.CustomerName |> Option.defaultValue "not supplied"
            ProjectName = details.ProjectName |> Option.defaultValue "not supplied"
            SfdcProjectId = details.SfdcProjectId |> Option.defaultValue ""
            SfdcProjectSlug = details.SfdcProjectSlug |> Option.defaultValue ""
            SecurityOwner = details.SecurityOwner |> Option.defaultValue ""
            Team = details.Team |> Option.defaultValue ""
            Cti = details.Cti |> Option.defaultValue { CTI.Category=""; Type=""; Item="" }
        }
    | Some state', EngagementEvent.Updated details -> {
            EngagementState.CustomerName = details.CustomerName |> Option.defaultValue state'.CustomerName
            ProjectName = details.ProjectName |> Option.defaultValue state'.ProjectName
            SfdcProjectId = details.SfdcProjectId |> Option.defaultValue state'.SfdcProjectId
            SfdcProjectSlug = details.SfdcProjectSlug |> Option.defaultValue state'.SfdcProjectSlug
            SecurityOwner = details.SecurityOwner |> Option.defaultValue state'.SecurityOwner
            Team = details.Team |> Option.defaultValue state'.Team
            Cti = details.Cti |> Option.defaultValue state'.Cti
        }    
    | _,_ -> failwith "invalid event"
