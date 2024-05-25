#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TestEngineX/TestEngineX/TestEngineX.csproj", "TestEngineX/TestEngineX/"]
COPY ["SharedX/SharedX.Models/SharedX.Core.csproj", "SharedX/SharedX.Models/"]
RUN dotnet restore "TestEngineX/TestEngineX/TestEngineX.csproj"
COPY . .
WORKDIR "/src/TestEngineX/TestEngineX"
RUN dotnet build "TestEngineX.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestEngineX.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestEngineX.dll"]