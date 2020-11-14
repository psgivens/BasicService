ARG VARIANT="3"
FROM mcr.microsoft.com/vscode/devcontainers/python:0-${VARIANT}

# [Optional] Allow the vscode user to pip install globally w/o sudo
ENV PIP_TARGET=/usr/local/pip-global
ENV PYTHONPATH=${PIP_TARGET}:${PYTHONPATH}
ENV PATH=${PIP_TARGET}/bin:${PATH}
RUN mkdir -p ${PIP_TARGET} \
    && chown vscode:root ${PIP_TARGET} \
    && echo "if [ \"\$(stat -c '%U' ${PIP_TARGET})\" != \"vscode\" ]; then chown -R vscode:root ${PIP_TARGET}; fi" \
        | tee -a /root/.bashrc /home/vscode/.bashrc /root/.zshrc >> /home/vscode/.zshrc 

RUN apt-get update \
    && apt-get install -y default-jdk

WORKDIR /Downloads

RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" \
    && unzip awscliv2.zip \
    && ./aws/install 

RUN wget https://s3.us-west-2.amazonaws.com/dynamodb-local/dynamodb_local_latest.tar.gz \
    && tar xzf dynamodb_local_latest.tar.gz \
    && chown vscode:root DynamoDBLocal.jar \
    && chmod ug+x DynamoDBLocal.jar

COPY ./config /workspaces/ddb/config

COPY ./scripts /home/vscode/.local/bin
RUN chown vscode:root /home/vscode/.local/bin/* \
    && chmod ug+x /home/vscode/.local/bin/*

RUN mkdir /data \
    && chown vscode:root /data \
    && chmod ug+x /data

USER vscode
RUN aws configure set aws_access_key_id default_access_key \
    && aws configure set aws_secret_access_key default_secret_key \
    && aws configure set default.region us-west-1 

WORKDIR /data
CMD ["/home/vscode/.local/bin/LocalDdb.sh"]
# CMD ["/bin/sleep", "10000000"]
