# pip-modern-application-flask dynamodb
Flask app that will be deployed in the modern application lab.

This readme is for the dynamodb. 

# Setup instructions
1. Install Visual Studio Code
2. Install Visual Studio Code - Remote-Container extension
3. Open this project in Visual Studio Code. When it asks, open in a remote container. This will build the environment and provide all the tools you need. 

Once you can get to a bash shell in the container, 
1. In a fresh Bash shell, run LocalDdb.sh. It is in your path, but located at `/home/vscode/.local/bin/LocalDdb.sh`. This runs DynamoDB locally on port 8000. 
2. In a fresh Bash Shell, `python -m http.server 3001 -d web`. This created a static web server for the html front end. 
3. F5 - Start Debugging or run. This will run `./app/service/mythicalMysfitsService.py`.

**Disclaimer**: This is not completely local. It authenticates against cognito, but the tokens are ignored by the API server locally. They are checked by API Gateway when this is deployed to the cloud. 

# Structure
The service lives under `./app`, and the static web site lives under `./web`. 

## Post start-up / build
You'll have to populate the dynamodb database with the instructions further down in this README. Otherwise, you'll have errors as this app trys to scan non-existent tables.

# Disclaimer
**This README is misleading.** I have merged the sample from these two projects. 
https://github.com/microsoft/vscode-remote-try-python
from https://github.com/aws-samples/aws-modern-application-workshop/tree/python

https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.DownloadingAndRunning.html

https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2-linux.html

**If we want to create aws-cli in a docker like aws does**
https://github.com/aws/aws-cli/blob/v2/docker/Dockerfile

**If we want to add accounts to AWS organizations**
https://docs.aws.amazon.com/organizations/latest/APIReference/API_InviteAccountToOrganization.html
https://docs.aws.amazon.com/organizations/latest/APIReference/API_AcceptHandshake.html

**If we want to change the launch.json**
https://code.visualstudio.com/docs/cpp/launch-json-reference

# Installed for you
LocalDdb.sh - This will start DynamodDB. It lives in /home/vscode/.local/bin .

    aws dynamodb list-tables --endpoint-url http://localhost:8000

# Module 3: Store Mysfit Information
https://aws.amazon.com/getting-started/hands-on/build-modern-app-fargate-lambda-dynamodb-python/module-three/

    ENDPOINT='http://ddb-local:8000'
    aws dynamodb list-tables --no-cli-pager --endpoint-url $ENDPOINT

    aws dynamodb create-table --cli-input-json file://schema/dynamodb-table.json \
      --endpoint-url $ENDPOINT

    aws dynamodb describe-table --table-name MysfitsTable --endpoint-url $ENDPOINT

    aws dynamodb scan --table-name MysfitsTable --endpoint-url $ENDPOINT

    aws dynamodb batch-write-item --request-items file://aws-cli/populate-dynamodb.json \
      --endpoint-url $ENDPOINT

    aws dynamodb update-table --cli-input-json file://config/dynamodb-table-engagement-events.json \    
      --endpoint-url $ENDPOINT

    aws dynamodb delete-table --table-name "EngagementEventsTable" \
      --endpoint-url $ENDPOINT      

    aws dynamodb create-table --cli-input-json file://config/dynamodb-table-engagement-events.json \
      --endpoint-url $ENDPOINT

    aws dynamodb batch-write-item --request-items file://config/populate-dynamodb-engagement-events.json \
      --endpoint-url $ENDPOINT

    aws dynamodb scan --table-name EngagementEventsTable --endpoint-url $ENDPOINT

    aws dynamodb create-table --cli-input-json file://config/dynamodb-table-engagement.json \
      --endpoint-url $ENDPOINT

      