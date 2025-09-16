# Get Base Image

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Copy csproj and restore

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Infrastructure/Infrastructure.csproj"

# Copy everything else and build

COPY . ./ 
WORKDIR "/src/Infrastructure"
RUN dotnet build "Infrastructure.csproj" -c Release -o /app/build

# Publish

FROM build AS publish
RUN dotnet publish "Infrastructure.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Infrastructure.dll"]