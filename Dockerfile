#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5224

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BankingSolution.sln", "."]
COPY ["Banking.Api/Banking.Api.csproj", "Banking.Api/"]
COPY ["Banking.ApiTests/Banking.ApiTests.csproj", "Banking.ApiTests/"]
COPY ["Banking.Core/Banking.Core.csproj", "Banking.Core/"]
COPY ["Banking.Infrastructure/Banking.Infrastructure.csproj", "Banking.Infrastructure/"]
COPY ["Banking.Specs/Banking.Specs.csproj", "Banking.Specs/"]
COPY ["Banking.UnitTests/Banking.UnitTests.csproj", "Banking.UnitTests/"]
RUN dotnet restore "./BankingSolution.sln"
COPY . .
WORKDIR "/src/."
RUN dotnet build "BankingSolution.sln" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Banking.Api/Banking.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Banking.Api.dll"]
