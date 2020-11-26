namespace BS.Domain

open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open BS.Domain.EngagementManagement


module EngagementHandlers =
    let hanlde (state: EngagementState option) (cmd: EngagementCommand) : Task<IEventSourcingEvent list> =
        task {
            return 
                match state, cmd with 
                | None, Create details -> [ Created details ] 
                | Some _, Update details -> [ Updated details ]
                | _,_ -> failwith "invalid command"
            }

