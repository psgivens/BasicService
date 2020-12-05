namespace BS.API.Controllers

open Giraffe

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextSensitive
open BS.Domain.DAL.EventEnvelopeDal
open BS.Domain.DAL.EngagementEventDal
open Amazon.DynamoDBv2
open BS.Domain.DAL
open BS.Domain.EngagementManagement

type Message = {
    Text : string
}

[<CLIMutable>]
type CtiDto = {
    Category: string
    Type: string
    Item: string
}

module CtiDto =
    let toCti dto = {
        CTI.Category = dto.Category
        Type = dto.Type
        Item = dto.Item
        }


[<CLIMutable>]
type EngagementDto = {
    CustomerName: string 
    ProjectName: string 
    SfdcProjectId: string 
    SfdcProjectSlug: string 
    SecurityOwner: string 
    Team: string  
    Cti: CtiDto
    TroopId: string
    }

type EngagementController 
    (   createClient:unit -> AmazonDynamoDBClient,
        createEnvelopeDao: EventEnvelopeDaoFactory
    ) = 
    let client = createClient ()
    member _.SubmitEngagement : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<EngagementDto>()
                // let envDao = EventEnvelopeDao ([EngagementEventConverter ()], client, "sample_user")
                let envDao = createEnvelopeDao client "sample_user"
                let dao = EngagementDao (envDao, client)
                
                let optString = function
                    | s when System.String.IsNullOrWhiteSpace(s) -> None
                    | value -> Some value

                let optCti cti =
                    let { CtiDto.Category=c; Type=t; Item=i } = cti
                    match (optString c, optString t, optString i) with 
                    | None, None, None -> None
                    | Some c', Some t', Some i' -> Some {
                            CTI.Category = c'
                            Type = t'
                            Item = i'
                        }
                    | _ -> invalidArg "Cti" "Optional parameter incomplete"

                let (|?!) (value:string) (name:string) = 
                    optString value |> Option.defaultWith (fun () -> invalidArg name "String cannot be empty")

                try
                    // let item = dao.MakeSampleEngagement ()
                    let id = dao.CreateEngagement {
                        CreateEngagementRequest.CustomerName =  dto.CustomerName |?! "CustomerName"
                        ProjectName = dto.ProjectName |?! "ProjectName"
                        SfdcProjectId = dto.SfdcProjectId |?! "SfdcProjectId"
                        SfdcProjectSlug = dto.SfdcProjectSlug |?! "SfdcProjectSlug"
                        SecurityOwner = dto.SecurityOwner
                        Team = dto.Team
                        Cti = CtiDto.toCti dto.Cti
                        TroopId = dto.TroopId
                    } 
                
                    // Sends the object back to the client
                    return! Successful.OK id next ctx
                with 
                    | _ -> return! ServerErrors.SERVICE_UNAVAILABLE "Something went wrong" next ctx
            }
