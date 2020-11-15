#!/bin/bash


    ENDPOINT='http://ddb-local:8000'
    aws dynamodb list-tables --no-cli-pager --endpoint-url $ENDPOINT

    aws dynamodb delete-table --table-name "MysfitsTable" \
      --endpoint-url $ENDPOINT --no-cli-pager     

    aws dynamodb delete-table --table-name "EngagementEventsTable" \
      --endpoint-url $ENDPOINT --no-cli-pager      

    aws dynamodb list-tables --no-cli-pager --endpoint-url $ENDPOINT
