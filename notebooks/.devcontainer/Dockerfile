# [Choice] .NET Core version: 3.1, 2.1
ARG VARIANT=3.1
FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:${VARIANT}

# [Option] Install Node.js
ARG INSTALL_NODE="true"
ARG NODE_VERSION="lts/*"
RUN if [ "${INSTALL_NODE}" = "true" ]; then su vscode -c "source /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# # [Option] Install Azure CLI
# ARG INSTALL_AZURE_CLI="false"
# COPY .devcontainer/library-scripts/azcli-debian.sh /tmp/library-scripts/
# RUN if [ "$INSTALL_AZURE_CLI" = "true" ]; then bash /tmp/library-scripts/azcli-debian.sh; fi \
#     && apt-get clean -y && rm -rf /var/lib/apt/lists/* /tmp/library-scripts

# [Optional] Uncomment this section to install additional OS packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1

RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" \
    && unzip awscliv2.zip \
    && ./aws/install 

USER vscode
RUN aws configure set aws_access_key_id default_access_key \
    && aws configure set aws_secret_access_key default_secret_key \
    && aws configure set default.region us-west-1     

# Install Powershell
RUN sudo apt-get update \
    && sudo apt-get install -y wget apt-transport-https software-properties-common 

WORKDIR /tmp
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb \
    && sudo dpkg -i packages-microsoft-prod.deb \
    && sudo apt-get update \
    && sudo add-apt-repository universe \
    && sudo apt-get install -y powershell

RUN sudo apt-get install -y python3-pip

RUN dotnet tool install -g Microsoft.dotnet-interactive --version 1.0.136005 --add-source "https://dotnet.myget.org/F/dotnet-try/api/v3/index.json" \
    && pip3 install jupyterlab 

WORKDIR /home/vscode
ENV PATH="$PATH:/home/vscode/.dotnet/tools"    
# RUN dotnet interactive jupyter install

# RUN dotnet interactive jupyter install

# jupyter notebook

CMD ["/bin/sleep", "10000000"]
