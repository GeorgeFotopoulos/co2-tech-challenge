﻿# ==============================================
# Stage 1: RESTORE and BUILD
# ==============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files
COPY emissions-api/src/TechChallenge.Emissions.Api/TechChallenge.Emissions.Api.csproj emissions-api/
COPY shared/src/TechChallenge.Common/TechChallenge.Common.csproj /shared/src/TechChallenge.Common/
COPY shared/src/TechChallenge.DataSimulator/TechChallenge.DataSimulator.csproj /shared/src/TechChallenge.DataSimulator/
COPY shared/src/TechChallenge.ChaosMonkey/TechChallenge.ChaosMonkey.csproj /shared/src/TechChallenge.ChaosMonkey/

# Copy the full source for the referenced projects
COPY shared/src/TechChallenge.Common /shared/src/TechChallenge.Common
COPY shared/src/TechChallenge.DataSimulator /shared/src/TechChallenge.DataSimulator
COPY shared/src/TechChallenge.ChaosMonkey /shared/src/TechChallenge.ChaosMonkey

# Restore dependencies
RUN dotnet restore "emissions-api/TechChallenge.Emissions.Api.csproj"

# Copy the full source for the API project
COPY . .

# Set working directory to the API project
WORKDIR /src/emissions-api
RUN dotnet build "TechChallenge.Emissions.Api.csproj" -c Release -o /app/build

# ==============================================
# Stage 2: PUBLISH
# ==============================================
FROM build AS publish
RUN dotnet publish "TechChallenge.Emissions.Api.csproj" -c Release -o /app/publish

# ==============================================
# Stage 3: FINAL RUNTIME IMAGE
# ==============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80

# Copy published output
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TechChallenge.Emissions.Api.dll"]
