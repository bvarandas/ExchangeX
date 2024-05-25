#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OrderEngineX/OrderX.API/OrderEngineX.API.csproj", "OrderEngineX/OrderX.API/"]
COPY ["OrderEngineX/OrderEngineX.Application/OrderEngineX.Application.csproj", "OrderEngineX/OrderEngineX.Application/"]
COPY ["OrderEntryX/OrderEntryX.Infra/OrderEntryX.Infra.csproj", "OrderEntryX/OrderEntryX.Infra/"]
COPY ["SharedX/SharedX.Models/SharedX.Core.csproj", "SharedX/SharedX.Models/"]
COPY ["OrderEntryX/OrderEntryX.Application/OrderEntryX.Application.csproj", "OrderEntryX/OrderEntryX.Application/"]
COPY ["OrderEntryX/OrderEntryX.Core/OrderEntryX.Core.csproj", "OrderEntryX/OrderEntryX.Core/"]
COPY ["Infrastructure/Common.logging/Common.logging.csproj", "Infrastructure/Common.logging/"]
COPY ["OrderEngineX/OrderEngineX.Core/OrderEngineX.Core.csproj", "OrderEngineX/OrderEngineX.Core/"]
COPY ["OrderEngineX/OrderX.Infra/OrderEngineX.Infra.csproj", "OrderEngineX/OrderX.Infra/"]
COPY ["SharedX/Sharex.Infra/Sharedx.Infra.csproj", "SharedX/Sharex.Infra/"]
RUN dotnet restore "OrderEngineX/OrderX.API/OrderEngineX.API.csproj"
COPY . .
WORKDIR "/src/OrderEngineX/OrderX.API"
RUN dotnet build "OrderEngineX.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderEngineX.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderEngineX.API.dll"]