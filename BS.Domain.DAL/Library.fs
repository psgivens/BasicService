namespace BS.Domain.DAL

// open System
// open System.Net
// open System.Net.NetworkInformation
open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.DataModel
open Amazon.DynamoDBv2.DocumentModel
open Amazon.DynamoDBv2.Model

open System.Collections.Generic
open BS.Domain.EngagementManagement

open System.IO
open System.IO.Compression



module Say =
    let hello name =
        printfn "Hello %s" name
