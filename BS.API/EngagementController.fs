namespace BS.API.Controllers

open Giraffe

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextSensitive
open BS.Domain.DAL.EventEnvelopeDal
open BS.Domain.DAL.EngagementEventDal
open Amazon.DynamoDBv2
open BS.Domain.DAL
open BS.Domain.EngagementManagement

type Message =
    {
        Text : string
    }

[<CLIMutable>]
type CtiDto = {
    Category: string
    Type: string
    Item: string
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
    }


type EngagementController (createClient:unit -> AmazonDynamoDBClient) = 
    let client = createClient ()
    member _.SubmitEngagement : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                // Binds a JSON payload to a Car object
                let! dto = ctx.BindJsonAsync<EngagementDto>()

                let envDao = EventEnvelopeDao ([EngagementEventConverter ()], client, "sample_user")

                let dao = EngagementDao (envDao, client)
                
                let optString = function
                    | s when System.String.IsNullOrWhiteSpace(s) -> None
                    | value -> Some value

                let (|?!) (value:string) (name:string) = 
                    optString value |> Option.defaultWith (fun () -> invalidArg name "String cannot be empty")

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

                try
                    // let item = dao.MakeSampleEngagement ()
                    let id = dao.CreateEngagement {
                        CreateEngagementRequest.CustomerName =  dto.CustomerName |?! "CustomerName"
                        ProjectName = dto.ProjectName |?! "ProjectName"
                        SfdcProjectId = dto.SfdcProjectId |?! "SfdcProjectId"
                        SfdcProjectSlug = dto.SfdcProjectSlug |?! "SfdcProjectSlug"
                        SecurityOwner = optString dto.SecurityOwner
                        Team = optString dto.Team
                        Cti = optCti dto.Cti
                    } 
                
                    // Sends the object back to the client
                    return! Successful.OK id next ctx
                with 
                    | _ -> return! ServerErrors.SERVICE_UNAVAILABLE "Something went wrong" next ctx
            }
