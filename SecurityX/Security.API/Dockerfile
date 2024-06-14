#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SecurityX/Security.API/Security.API.csproj", "SecurityX/Security.API/"]
COPY ["SharedX/SharedX.Models/SharedX.Core.csproj", "SharedX/SharedX.Models/"]
COPY ["SecurityX/Security.Application/Security.Application.csproj", "SecurityX/Security.Application/"]
COPY ["OrderEngineX/OrderEngineX.Application/OrderEngineX.Application.csproj", "OrderEngineX/OrderEngineX.Application/"]
COPY ["OrderEntryX/OrderEntryX.Infra/OrderEntryX.Infra.csproj", "OrderEntryX/OrderEntryX.Infra/"]
COPY ["OrderEntryX/OrderEntryX.Application/OrderEntryX.Application.csproj", "OrderEntryX/OrderEntryX.Application/"]
COPY ["OrderEntryX/OrderEntryX.Core/OrderEntryX.Core.csproj", "OrderEntryX/OrderEntryX.Core/"]
COPY ["Infrastructure/Common.logging/Common.logging.csproj", "Infrastructure/Common.logging/"]
COPY ["OrderEngineX/OrderEngineX.Core/OrderEngineX.Core.csproj", "OrderEngineX/OrderEngineX.Core/"]
COPY ["SecurityX/Security.Infra/Security.Infra/SecurityX.Infra.csproj", "SecurityX/Security.Infra/Security.Infra/"]
COPY ["SecurityX/SecurityX.Core/SecurityX.Core.csproj", "SecurityX/SecurityX.Core/"]
RUN dotnet restore "SecurityX/Security.API/Security.API.csproj"
COPY . .
WORKDIR "/src/SecurityX/Security.API"
RUN dotnet build "Security.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Security.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Security.API.dll"]