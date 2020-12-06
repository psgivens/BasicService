namespace BS.Domain.DAL

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.DataModel
open Amazon.DynamoDBv2.DocumentModel
open Amazon.DynamoDBv2.Model

open System.Collections.Generic

open System
open System.IO
open System.IO.Compression
open System.Net
open System.Threading.Tasks
open FSharp.Control.Tasks.V2

open BS.Domain
open BS.Domain.EngagementManagement
open BS.Domain.DAL.DataAccess
open BS.Domain.DAL.EventEnvelopeDal
open BS.Domain.DAL.EngagementEventDal
open BS.Domain.Handlers

type CreateEngagementRequest = EngagementCreatedDetails

type EngagementDao (envDao:EventEnvelopeDao, client:AmazonDynamoDBClient) =
    let engagementsTable = "EngagementsTable"
    let handle = EngagementHandlers.createHandler envDao

    let engagementToAttributes id (e:EngagementState)=
        [ ("EngagementId", ScalarString id)
          ("TeamsName", ScalarString e.Team)
          ("Segment", ScalarString "fict")
          ("SFDCID", ScalarString e.SfdcProjectId)
          ("ProjectManager", ScalarString e.SecurityOwner) 
          ("CustomerName", ScalarString e.CustomerName)
          ("SfdcProjectId", ScalarString e.SfdcProjectId)
          ("SfdcProjectSlug", ScalarString e.SfdcProjectSlug)
          ("SecurityOwner", ScalarString e.SecurityOwner)
          ("CTI", DocMap <| ctiToAttributes e.Cti) 
          ("TimeStamp", ScalarString (DateTime.Now.ToString ()))]

    member _.MakeSampleEngagement (troopId: string) = 
        {
            EngagementCreatedDetails.CustomerName = "Big Good Corporation"
            ProjectName = "A major project"
            SfdcProjectId = "PROJ-1234"
            SfdcProjectSlug = "abc10324kjlaskdfjhv"
            SecurityOwner = "Spider man" 
            Team = "Justice league" 
            Cti = {
                Category = "AWS"
                Type = "ProServe"
                Item = "EngSec"
            } 
            TroopId = troopId
        }

    member _.CreateEngagement (engagement:CreateEngagementRequest) = 
        task {
            let id = ((Guid.NewGuid ()).ToString())
            let! results = 
                EngagementCommand.Create engagement 
                |> envDao.EnvelopCommand id 
                |> handle

            return results
            // TODO Check for errors and raise them. 
            // TODO Retrieve ids of objects, build state, and store state. 

            // evts
            // |> envDao.InsertEventEnvelope 
            // |> fun result ->
            //    match result with 
            //    | Ok () -> id
            //    | Error message -> failwith message
            // |> envDao.GetEnvelopes 
            // |> List.map (fun env -> env.Event :?> EngagementEvent)
            // |> List.fold evolve None        
            // |> Option.get
            // |> engagementToAttributes id
            // |> putItem client engagementsTable 
        }

    member _.UpdateEngagement id version (engagement:EngagementCreatedDetails) = 
        let events = getItem

        Created engagement 
        |> envDao.EnvelopEvent ((System.Guid.NewGuid ()).ToString()) "1" 
        |> envDao.InsertEventEnvelope 


