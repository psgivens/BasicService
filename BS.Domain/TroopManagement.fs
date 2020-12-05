module BS.Domain.TroopManagement

open System
open BS.Domain.Common

type CTI = {
    Category: string
    Type: string
    Item: string
}

type TroopCreatedDetails = {
    Name: string
    CustomerName: string
    SecurityOwner: string
    Team: string
    Cti: CTI
}

type TroopUpdatedDetails = {
    Name: string option
    CustomerName: string option
    SecurityOwner: string option
    Team: string option    
    Cti: CTI option
}

type TroopCommand =
    | Create of TroopCreatedDetails
    | Update of TroopUpdatedDetails
    | AddEngagement of string
    | RemoveEngagement of string
    interface IEventSourcingCommand

type TroopEvent =
    | Created of TroopCreatedDetails
    | Updated of TroopUpdatedDetails
    | EngagementAdded of string
    | EngagementRemoved of string
    interface IEventSourcingEvent

type TroopState = {
    Name: string
    CustomerName: string 
    SecurityOwner: string 
    Team: string  
    Cti: CTI 
    Engagements: string list
}

let evolve (state: TroopState option) (event:TroopEvent) =
    match state, event with
    | None, Created details -> 
        {
            TroopState.Name     = details.Name 
            CustomerName        = details.CustomerName 
            SecurityOwner       = details.SecurityOwner 
            Team                = details.Team 
            Cti                 = details.Cti 
            Engagements         = []
        } |> Some
    | Some s, Updated d -> 
        let (|?) o1 dv = o1 |> Option.defaultValue dv
        {   s with 
                Name            = d.Name |? s.Name
                CustomerName    = d.CustomerName |? s.CustomerName 
                SecurityOwner   = d.SecurityOwner |? s.SecurityOwner
                Team            = d.Team |? s.Team
                Cti             = d.Cti |? s.Cti
        } |> Some
    | Some s, EngagementAdded id -> { s with Engagements = id :: s.Engagements } |> Some
    | Some s, EngagementRemoved id -> { s with Engagements = s.Engagements |> List.filter ((<>) id) } |> Some
    | None, _ -> invalidOp "First event must create an the Troop"
    | Some _, Created _ -> invalidOp "Troop exists"





