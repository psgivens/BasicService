module BS.Domain.EngagementManagement

type IEventSourcingEvent = interface end

type CTI = {
    Category: string
    Type: string
    Item: string
}

// TODO: Add logic to use RequiredString where appropriate
// type RequiredString (value: string) =
//     member _.ToString () =
//         value

type EngagementCreatedDetails = {
    CustomerName: string
    ProjectName: string
    SfdcProjectId: string
    SfdcProjectSlug: string
    SecurityOwner: string
    Team: string
    Cti: CTI
}

type EngagementUpdatedDetails = {
    CustomerName: string option
    ProjectName: string option
    SfdcProjectId: string option
    SfdcProjectSlug: string option
    SecurityOwner: string option
    Team: string option    
    Cti: CTI option
}

type EngagementCommand =
    | Create of EngagementCreatedDetails
    | Update of EngagementUpdatedDetails

type EngagementEvent =
    | Created of EngagementCreatedDetails
    | Updated of EngagementUpdatedDetails
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

let evolve (state: EngagementState option) (event:EngagementEvent) =
    match state, event with
    | None, Created details -> 
        {
            EngagementState.CustomerName    = details.CustomerName 
            ProjectName                     = details.ProjectName 
            SfdcProjectId                   = details.SfdcProjectId 
            SfdcProjectSlug                 = details.SfdcProjectSlug 
            SecurityOwner                   = details.SecurityOwner 
            Team                            = details.Team 
            Cti                             = details.Cti 
        } |> Some
    | Some s, Updated d -> 
        let (|?) o1 dv = o1 |> Option.defaultValue dv
        {
            EngagementState.CustomerName    = d.CustomerName |? s.CustomerName 
            ProjectName                     = d.ProjectName |? s.ProjectName
            SfdcProjectId                   = d.SfdcProjectId |? s.SfdcProjectId
            SfdcProjectSlug                 = d.SfdcProjectSlug |? s.SfdcProjectSlug
            SecurityOwner                   = d.SecurityOwner |? s.SecurityOwner
            Team                            = d.Team |? s.Team
            Cti                             = d.Cti |? s.Cti
        } |> Some
    | _,_ -> failwith "invalid event"
