#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MatchingX/MatchinX.API/MatchinX.API.csproj", "MatchingX/MatchinX.API/"]
COPY ["SharedX/SharedX.Models/SharedX.Core.csproj", "SharedX/SharedX.Models/"]
COPY ["MatchingX/MatchingX.Infra/MatchingX.Infra.csproj", "MatchingX/MatchingX.Infra/"]
COPY ["MatchingX/MatchingX.Core/MatchingX.Core.csproj", "MatchingX/MatchingX.Core/"]
RUN dotnet restore "MatchingX/MatchinX.API/MatchinX.API.csproj"
COPY . .
WORKDIR "/src/MatchingX/MatchinX.API"
RUN dotnet build "MatchinX.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatchinX.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatchinX.API.dll"]