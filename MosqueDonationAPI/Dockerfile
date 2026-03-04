FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj from subfolder
COPY MosqueDonationAPI/*.csproj ./MosqueDonationAPI/

# Restore using path
RUN dotnet restore MosqueDonationAPI/MosqueDonationAPI.csproj

# Copy everything
COPY . .

# Publish
RUN dotnet publish MosqueDonationAPI/MosqueDonationAPI.csproj -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "MosqueDonationAPI.dll"]