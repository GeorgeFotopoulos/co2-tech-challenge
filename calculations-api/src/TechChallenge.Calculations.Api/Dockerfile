# ==============================================
# Stage 1: RESTORE and BUILD
# ==============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files for calculations-api and its dependencies
COPY calculations-api/src/TechChallenge.Calculations.Api/TechChallenge.Calculations.Api.csproj calculations-api/
COPY shared/src/TechChallenge.Common/TechChallenge.Common.csproj /shared/src/TechChallenge.Common/
COPY shared/src/TechChallenge.DataSimulator/TechChallenge.DataSimulator.csproj /shared/src/TechChallenge.DataSimulator/
COPY shared/src/TechChallenge.ChaosMonkey/TechChallenge.ChaosMonkey.csproj /shared/src/TechChallenge.ChaosMonkey/

# Copy the Emissions and Measurements projects
COPY emissions-api/src/TechChallenge.Emissions.Api/TechChallenge.Emissions.Api.csproj /emissions-api/src/TechChallenge.Emissions.Api/
COPY measurements-api/src/TechChallenge.Measurements.Api/TechChallenge.Measurements.Api.csproj /measurements-api/src/TechChallenge.Measurements.Api/

# Copy the full source for the referenced projects
COPY shared/src/TechChallenge.Common /shared/src/TechChallenge.Common
COPY shared/src/TechChallenge.DataSimulator /shared/src/TechChallenge.DataSimulator
COPY shared/src/TechChallenge.ChaosMonkey /shared/src/TechChallenge.ChaosMonkey

# Copy the full source for Emissions and Measurements projects
COPY emissions-api/src/TechChallenge.Emissions.Api /emissions-api/src/TechChallenge.Emissions.Api
COPY measurements-api/src/TechChallenge.Measurements.Api /measurements-api/src/TechChallenge.Measurements.Api

# Restore dependencies
RUN dotnet restore "calculations-api/TechChallenge.Calculations.Api.csproj"

# Copy the full source for the API project
COPY . . 

# Set working directory to the API project
WORKDIR /src/calculations-api
RUN dotnet build "TechChallenge.Calculations.Api.csproj" -c Release -o /app/build

# ==============================================
# Stage 2: PUBLISH
# ==============================================
FROM build AS publish
RUN dotnet publish "TechChallenge.Calculations.Api.csproj" -c Release -o /app/publish

# ==============================================
# Stage 3: FINAL RUNTIME IMAGE
# ==============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80

COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "TechChallenge.Calculations.Api.dll"]
