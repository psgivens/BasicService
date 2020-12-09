module BS.Domain.Common

open FSharp.Control.Tasks.V2
open System.Threading.Tasks

type IEventSourcingCommand = interface end
type IEventSourcingEvent = interface end


type CmdEnvelope = {
    Id:string
    UserName:string
    TimeStamp:string
    Command: IEventSourcingCommand
}

type EvtEnvelope = {
    Id:string
    Version:int
    UserName:string
    TimeStamp:string
    Event: IEventSourcingEvent
}

type DomainHandler = CmdEnvelope -> Task<string list> 
type DomainEvolver<'Event, 'State when 'Event :> IEventSourcingEvent> = 'State option -> 'Event -> 'State option



