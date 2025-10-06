# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY SystemHealthAPI.csproj ./
RUN dotnet restore SystemHealthAPI.csproj

# Copy source code and build
COPY . ./
RUN dotnet publish SystemHealthAPI.csproj -c Release -o out

# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the built application
COPY --from=build /app/out .

# Expose port 8080
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "SystemHealthAPI.dll"]