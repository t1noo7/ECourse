# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution và project files
COPY Demo.sln .
COPY Demo.Application/*.csproj Demo.Application/
COPY Demo.Common/*.csproj Demo.Common/
COPY Demo.Core/*.csproj Demo.Core/
COPY Demo.Database/*.csproj Demo.Database/
COPY Demo.Infrastructure.File/*.csproj Demo.Infrastructure.File/
COPY Demo.Infrastructure.Mail/*.csproj Demo.Infrastructure.Mail/
COPY Demo.Web/*.csproj Demo.Web/

# Restore dependencies
RUN dotnet restore

# Copy everything else và build
COPY . .
WORKDIR /src/Demo.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files
COPY --from=build /app/publish .

# Thêm certificate cho HTTPS (nếu cần)
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates

# Entry point
ENTRYPOINT ["dotnet", "Demo.Web.dll"]