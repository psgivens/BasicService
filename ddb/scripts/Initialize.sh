#!/bin/bash

    ENDPOINT='http://ddb-local:8000'

# Original mysfits table
    aws dynamodb create-table --cli-input-json file://config/dynamodb-table.json \
      --endpoint-url $ENDPOINT --no-cli-pager
    aws dynamodb batch-write-item --request-items file://config/populate-dynamodb.json \
      --endpoint-url $ENDPOINT --no-cli-pager

# Event sourcing for engagements
    aws dynamodb create-table --cli-input-json file://config/dynamodb-table-engagement-events.json \
      --endpoint-url $ENDPOINT --no-cli-pager
    aws dynamodb batch-write-item --request-items file://config/populate-dynamodb-engagement-events.json \
      --endpoint-url $ENDPOINT --no-cli-pager


    aws dynamodb list-tables --no-cli-pager --endpoint-url $ENDPOINT

# Engagement state tables
# TODO



