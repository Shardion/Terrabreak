FROM mcr.microsoft.com/dotnet/nightly/sdk:9.0-preview AS build-env
WORKDIR /
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
COPY . ./
RUN dotnet restore
RUN dotnet publish Shardion.Terrabreak/ -c Release -o /out

FROM mcr.microsoft.com/dotnet/nightly/sdk:9.0-preview
WORKDIR /
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
USER $APP_UID
COPY --from=build-env --chown=$APP_UID --chmod=700 /out /terrabreak
WORKDIR /terrabreak
ENTRYPOINT ["dotnet", "Shardion.Terrabreak.dll"]
