#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


#Build/Push to registry
 #docker build -t lake_stats_api .  
 #docker image tag lake_stats_api 192.168.1.136:9005/lake_stats_api:latest 
 #docker image push 192.168.1.136:9005/lake_stats_api:latest 

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
ENV InfluxKey
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LakeStatsApi.csproj", "LakeStatsApi/"]
RUN dotnet restore "LakeStatsApi/LakeStatsApi.csproj"
WORKDIR "/src/LakeStatsApi"
COPY . .
RUN dotnet build "LakeStatsApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LakeStatsApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LakeStatsApi.dll"]