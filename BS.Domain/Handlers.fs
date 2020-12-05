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
        fun (cmdenv: CmdEnvelope) ->
            let buildState = EventEnvelopeDal.buildState EngagementManagement.evolve
            let version, state = buildState envDao cmdenv.Id
            let envelopEngagement (e:EngagementEvent) = envDao.EnvelopEvent cmdenv.Id version e

            let createEngagement id (details:EngagementCreatedDetails) = 
                let troopVersion, troopState = EventEnvelopeDal.buildState TroopManagement.evolve envDao cmdenv.Id
                let envelopTroop (e:TroopEvent) = envDao.EnvelopEvent cmdenv.Id troopVersion e
                [ EngagementEvent.Created details |> envelopEngagement
                  TroopEvent.EngagementAdded id |> envelopTroop ]

            let updateEngagement (details:EngagementUpdatedDetails) = 
                [ EngagementEvent.Updated details |> envelopEngagement ]

            task {
                return 
                    match state, cmdenv.Command :?> EngagementCommand with 
                    | None, EngagementCommand.Create details -> createEngagement cmdenv.Id details
                    | Some _, EngagementCommand.Update details -> updateEngagement details
                    | _,_ -> failwith "invalid command"
                }

module TroopHandlers =
    let createHandler (envDao:EventEnvelopeDao) : DomainHandler =         
        fun (cmdenv: CmdEnvelope) ->
            let buildState = EventEnvelopeDal.buildState TroopManagement.evolve
            let version, state = buildState envDao cmdenv.Id
            let envelopTroop (e:TroopEvent) = envDao.EnvelopEvent cmdenv.Id version e

            task {
                return 
                    match state, cmdenv.Command :?> TroopCommand with 
                    | None, TroopCommand.Create details -> [ TroopEvent.Created details |> envelopTroop ] 
                    | Some _, TroopCommand.Update details -> [ TroopEvent.Updated details |> envelopTroop ]
                    | _,_ -> failwith "invalid command"
                }

