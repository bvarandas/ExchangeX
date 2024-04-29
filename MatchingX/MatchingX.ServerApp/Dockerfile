#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MatchingX/Matching.FixApp/Matching.FixApp.csproj", "MatchingX/Matching.FixApp/"]
COPY ["SharedX/SharedX.Models/SharedX.Core.csproj", "SharedX/SharedX.Models/"]
COPY ["MatchingX/MacthingX.Application/MacthingX.Application.csproj", "MatchingX/MacthingX.Application/"]
COPY ["MatchingX/MatchingX.Core/MatchingX.Core.csproj", "MatchingX/MatchingX.Core/"]
COPY ["MatchingX/MatchingX.Infra/MatchingX.Infra.csproj", "MatchingX/MatchingX.Infra/"]
RUN dotnet restore "MatchingX/Matching.FixApp/Matching.FixApp.csproj"
COPY . .
WORKDIR "/src/MatchingX/Matching.FixApp"
RUN dotnet build "Matching.FixApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Matching.FixApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Matching.FixApp.dll"]