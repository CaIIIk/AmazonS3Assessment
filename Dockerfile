FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine AS build
WORKDIR /app

# Copy csproj and restore
COPY s3assessment/*.csproj ./s3tool/
WORKDIR /app/s3tool
RUN dotnet restore

# Copy and publish app and libraries
WORKDIR /app/
COPY s3assessment/. ./s3tool/
WORKDIR /app/s3tool
RUN dotnet publish -c Release -o out

# Run the app
FROM mcr.microsoft.com/dotnet/core/runtime:2.2-alpine AS runtime
WORKDIR /app
COPY --from=build /app/s3tool/out ./
CMD ["dotnet", "S3assessment.dll"]
