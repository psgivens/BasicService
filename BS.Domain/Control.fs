namespace BS.Domain

open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open BS.Domain.EngagementManagement


module Say =
    let hello name =
        printfn "Hello %s" name


module EngagementManagement =
    let hanlde (state: EngagementState option) (cmd: EngagementCommand) : Task<IEventSourcingEvent list> =
        task {
            return 
                match state, cmd with 
                | None, EngagementCommand.Create details -> [ EngagementEvent.Created details ] 
                | Some state', EngagementCommand.Update details -> [ EngagementEvent.Updated details ]
                | _,_ -> failwith "invalid command"
            }

module EngagmentControl =

    let control () =
        EventEnvelopeDal.putEnvelope () |> ignore
