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
    let createHandler (envDao:EventEnvelopeDao) : DomainHandler=         
        let postEnvelopesAsync = EventEnvelopeDal.postEnvelopesAsync envDao
        fun (cmdenv: CmdEnvelope) ->
            task {
                let buildState = EventEnvelopeDal.buildState EngagementManagement.evolve
                let! version, state = buildState envDao.GetEnvelopesAsync cmdenv.Id 
                let envelopEngagement (e:EngagementEvent) = envDao.EnvelopEvent cmdenv.Id version e

                let createEngagement id (details:EngagementCreatedDetails) = 
                    task {
                        let! troopVersion, _ = EventEnvelopeDal.buildState TroopManagement.evolve envDao.GetEnvelopesAsync cmdenv.Id
                        let envelopTroop (e:TroopEvent) = envDao.EnvelopEvent details.TroopId troopVersion e

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
    let createHandler (envDao:EventEnvelopeDao) : DomainHandler =         
        let postEnvelopes = EventEnvelopeDal.postEnvelopesAsync envDao
        fun (cmdenv: CmdEnvelope) ->
            task {
                let buildState = EventEnvelopeDal.buildState TroopManagement.evolve
                let! version, state = buildState envDao.GetEnvelopesAsync cmdenv.Id
                let envelopTroop (e:TroopEvent) = envDao.EnvelopEvent cmdenv.Id version e

                return! 
                    match state, cmdenv.Command :?> TroopCommand with 
                    | None, TroopCommand.Create details -> 
                        [ TroopEvent.Created details |> envelopTroop ] 
                        |> postEnvelopes
                    | Some _, TroopCommand.Update details -> 
                        [ TroopEvent.Updated details |> envelopTroop ]
                        |> postEnvelopes
                    | _,_ -> failwith "invalid command"
            }

