namespace BS.Domain.Handlers

open FSharp.Control.Tasks.V2
open System.Threading.Tasks

open BS.Domain
open BS.Domain.EngagementManagement
open BS.Domain.TroopManagement
open BS.Domain.Common
open BS.Domain.DAL
open BS.Domain.DAL.EventEnvelopeDal

module EngagementHandlers =
    type HandlerDependencies = {
        InsertEventEnvelopesAsync:InsertEventEnvelopesAsync
        GetEnvelopesAsync:EnvelopesFetcher
        EnvelopEngagement:EnvelopEvent<EngagementEvent>
        EnvelopTroop:EnvelopEvent<TroopEvent>
    }
    let createHandler (ext:HandlerDependencies) : DomainHandler=         
        let postEnvelopesAsync = EventEnvelopeDal.postEnvelopesAsync ext.InsertEventEnvelopesAsync
        fun (cmdenv: CmdEnvelope) ->
            task {
                let buildState = EventEnvelopeDal.buildState EngagementManagement.evolve ext.GetEnvelopesAsync
                let! version, state = buildState cmdenv.Id 
                let envelopEngagement (e:EngagementEvent) = ext.EnvelopEngagement cmdenv.Id version e

                let createEngagement id (details:EngagementCreatedDetails) = 
                    task {
                        let buildState = EventEnvelopeDal.buildState TroopManagement.evolve ext.GetEnvelopesAsync
                        let! troopVersion, _ = buildState cmdenv.Id
                        let envelopTroop (e:TroopEvent) = ext.EnvelopTroop details.TroopId troopVersion e

                        return! 
                            [ EngagementEvent.Created details |> envelopEngagement
                              TroopEvent.EngagementAdded id |> envelopTroop ]
                            |> postEnvelopesAsync
                    }

                let updateEngagement (details:EngagementUpdatedDetails) = 
                    [ EngagementEvent.Updated details |> envelopEngagement ] 
                    |> postEnvelopesAsync
                    
                return! 
                    match state, cmdenv.Command :?> EngagementCommand with 
                    | None, EngagementCommand.Create details -> createEngagement cmdenv.Id details
                    | Some _, EngagementCommand.Update details -> updateEngagement details
                    | _,_ -> failwith "invalid command"
            }

module TroopHandlers =
    type HandlerDependencies = {
        InsertEventEnvelopesAsync:InsertEventEnvelopesAsync
        GetEnvelopesAsync:EnvelopesFetcher
        EnvelopEvent:EnvelopEvent<TroopEvent>
    }
    let createHandler (ext:HandlerDependencies) : DomainHandler =         
        let postEnvelopesAsync = EventEnvelopeDal.postEnvelopesAsync ext.InsertEventEnvelopesAsync
        fun (cmdenv: CmdEnvelope) ->
            task {
                let buildState = EventEnvelopeDal.buildState TroopManagement.evolve ext.GetEnvelopesAsync
                let! version, state = buildState cmdenv.Id
                let envelopTroop (e:TroopEvent) = ext.EnvelopEvent cmdenv.Id version e

                return! 
                    match state, cmdenv.Command :?> TroopCommand with 
                    | None, TroopCommand.Create details -> 
                        [ TroopEvent.Created details |> envelopTroop ] 
                        |> postEnvelopesAsync
                    | Some _, TroopCommand.Update details -> 
                        [ TroopEvent.Updated details |> envelopTroop ]
                        |> postEnvelopesAsync
                    | _,_ -> failwith "invalid command"
            }

