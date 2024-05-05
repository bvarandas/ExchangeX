#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OrderEntryX/OrderEntryX.ZeroMQApp/OrderEntryX.ServerApp.csproj", "OrderEntryX/OrderEntryX.ZeroMQApp/"]
COPY ["OrderEntryX/OrderEntryX.Application/OrderEntryX.Application.csproj", "OrderEntryX/OrderEntryX.Application/"]
COPY ["SharedX/SharedX.Models/SharedX.Core.csproj", "SharedX/SharedX.Models/"]
COPY ["OrderEntryX/OrderEntryX.Core/OrderEntryX.Core.csproj", "OrderEntryX/OrderEntryX.Core/"]
COPY ["Infrastructure/Common.logging/Common.logging.csproj", "Infrastructure/Common.logging/"]
COPY ["OrderEntryX/OrderEntryX.Infra/OrderEntryX.Infra.csproj", "OrderEntryX/OrderEntryX.Infra/"]
RUN dotnet restore "OrderEntryX/OrderEntryX.ZeroMQApp/OrderEntryX.ServerApp.csproj"
COPY . .
WORKDIR "/src/OrderEntryX/OrderEntryX.ZeroMQApp"
RUN dotnet build "OrderEntryX.ServerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderEntryX.ServerApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderEntryX.ServerApp.dll"]