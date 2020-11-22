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

open BS.Domain.DAL.ReadWrite
open BS.Domain.DAL.EngagementEventDal
open BS.Domain.DAL.EventEnvelopeDal

type EngagementDao (client:AmazonDynamoDBClient, tableName:string, userName:string) =
    let envelop' = envelop userName
    member dao.MakeSampleEngagement () = 
        {
            EngagementDetails2.Owner = "Fun Guy"
            ProjectName = "A major project"
            TeamsName = "flytrap and co"
            Region = "NARNIA"
            SfdcId = "PROJ-1234"
        }

    member dao.CreateEngagement (engagement:EngagementDetails2) = 
        FakeEvent2.Created engagement 
        |> envelop' ((System.Guid.NewGuid ()).ToString()) "1" 
        |> insertEventEnvelope client tableName

        // TODO Store a copy of the engagement

    member dao.UpdateEngagement id version (engagement:EngagementDetails2) = 
        let events = getItem

        FakeEvent2.Created engagement 
        |> envelop' ((System.Guid.NewGuid ()).ToString()) "1" 
        |> insertEventEnvelope client tableName

        // TODO Store a copy of the engagement

