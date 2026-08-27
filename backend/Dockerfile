# Multi-stage Dockerfile for ASP.NET Core 10 Web API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/Biplott.Core/Biplott.Core.csproj", "src/Biplott.Core/"]
COPY ["src/Biplott.Application/Biplott.Application.csproj", "src/Biplott.Application/"]
COPY ["src/Biplott.Infrastructure/Biplott.Infrastructure.csproj", "src/Biplott.Infrastructure/"]
COPY ["src/Biplott.Api/Biplott.Api.csproj", "src/Biplott.Api/"]

RUN dotnet restore "src/Biplott.Api/Biplott.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/Biplott.Api"
RUN dotnet publish "Biplott.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:8080;http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Biplott.Api.dll"]
