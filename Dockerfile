# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["OnlineRest/OnlineRest.csproj", "OnlineRest/"]
RUN dotnet restore "OnlineRest/OnlineRest.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/OnlineRest"
RUN dotnet build "OnlineRest.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "OnlineRest.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "OnlineRest.dll"]
