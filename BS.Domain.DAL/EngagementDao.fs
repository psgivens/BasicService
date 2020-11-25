namespace BS.Domain.DAL

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.DataModel
open Amazon.DynamoDBv2.DocumentModel
open Amazon.DynamoDBv2.Model

open System.Collections.Generic
open BS.Domain.EngagementManagement

open System
open System.IO
open System.IO.Compression
open System.Net

open BS.Domain.DAL.DataAccess
open BS.Domain.EngagementManagement
open BS.Domain.DAL.EventEnvelopeDal

type EngagementDao (envDao:EventEnvelopeDao) =

    member dao.MakeSampleEngagement () = 
        {
            EngagementDetails.CustomerName = "Big Good Corporation"
            ProjectName = "A major project"
            SfdcProjectId = "PROJ-1234"
            SfdcProjectSlug = "abc10324kjlaskdfjhv"
            SecurityOwner = "Spider man" |> Some
            Team = "Justice league" |> Some
            Cti = {
                Category = "AWS"
                Type = "ProServe"
                Item = "EngSec"
            } |> Some
        }

    member dao.CreateEngagement (engagement:EngagementDetails) = 
        Created engagement 
        |> envDao.Envelop ((System.Guid.NewGuid ()).ToString()) "1" 
        |> envDao.InsertEventEnvelope 


    member dao.UpdateEngagement id version (engagement:EngagementDetails) = 
        let events = getItem

        Created engagement 
        |> envDao.Envelop ((System.Guid.NewGuid ()).ToString()) "1" 
        |> envDao.InsertEventEnvelope 


