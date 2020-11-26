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


type EngagementController (createClient:unit -> AmazonDynamoDBClient, tableName: string) = 
    let client = createClient ()
    member _.SubmitEngagement : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                // Binds a JSON payload to a Car object
                let! dto = ctx.BindJsonAsync<EngagementDto>()

                let envDao = EventEnvelopeDao ([EngagementEventConverter ()], client, tableName, "sample_user")

                let dao = EngagementDao envDao

                let verify = function
                    | "" | null -> None
                    | value -> Some value

                let (|HasValue|_|) value = verify value

                let (|?!) (value:string) (name:string) = 
                    match verify value with
                    | None -> invalidArg name "String cannot be empty"
                    | Some v -> v

                let optString (value:string) = verify value

                let optCti cti =
                    let { CtiDto.Category=c; Type=t; Item=i } = cti
                    match (verify c, verify t, verify i) with 
                    | None, None, None -> None
                    | Some c', Some t', Some i' -> Some {
                            CTI.Category = c'
                            Type = t'
                            Item = i'
                        }
                    | _ -> invalidArg "Cti" "Optional parameter incomplete"

                // let item = dao.MakeSampleEngagement ()
                dao.CreateEngagement {
                    CreateEngagementRequest.CustomerName =  dto.CustomerName |?! "CustomerName"
                    ProjectName = dto.ProjectName |?! "ProjectName"
                    SfdcProjectId = dto.SfdcProjectId |?! "SfdcProjectId"
                    SfdcProjectSlug = dto.SfdcProjectSlug |?! "SfdcProjectSlug"
                    SecurityOwner = optString dto.SecurityOwner
                    Team = optString dto.Team
                    Cti = optCti dto.Cti
                } |> ignore
                
                // EventEnvelopeDal.putEnvelope () |> ignore




                // Sends the object back to the client
                return! Successful.OK dto next ctx
            }
