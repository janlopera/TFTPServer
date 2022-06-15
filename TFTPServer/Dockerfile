FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TFTPServer/TFTPServer.csproj", "TFTPServer/"]
RUN dotnet restore "TFTPServer/TFTPServer.csproj"
COPY . .
WORKDIR "/src/TFTPServer"
RUN dotnet build "TFTPServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TFTPServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TFTPServer.dll"]
