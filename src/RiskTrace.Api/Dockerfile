FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["global.json", "./"] #Need for correct SDK version (lately added - when need CI/CD or coworking with other developers)

COPY ["RiskTrace.sln", "./"]
COPY ["src/RiskTrace.Api/RiskTrace.Api.csproj", "src/RiskTrace.Api/"]
COPY ["src/RiskTrace.Infrastructure/RiskTrace.Infrastructure.csproj", "src/RiskTrace.Infrastructure/"]
COPY ["src/RiskTrace.UseCases/RiskTrace.UseCases.csproj", "src/RiskTrace.UseCases/"]
COPY ["src/RiskTrace.Domain/RiskTrace.Domain.csproj", "src/RiskTrace.Domain/"]
COPY ["src/RiskTrace.Core/RiskTrace.Core.csproj", "src/RiskTrace.Core/"]

RUN dotnet restore "src/RiskTrace.Api/RiskTrace.Api.csproj"

COPY . .
RUN dotnet publish "src/RiskTrace.Api/RiskTrace.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

#Remove recommendations libkrb5-3 to avoid installing unnecessary packages and reduce image size
RUN apt-get update \
    && apt-get install -y --no-install-recommends libkrb5-3 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RiskTrace.Api.dll"]