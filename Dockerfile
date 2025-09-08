# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar solução e projeto da API
COPY *.sln .
COPY SnapLink.api/*.csproj ./SnapLink.api/
RUN dotnet restore

# Copiar tudo e publicar
COPY . .
WORKDIR /app/SnapLink.api
RUN dotnet publish -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/SnapLink.api/out . 
EXPOSE 5000
ENTRYPOINT ["dotnet", "SnapLink.api.dll"]
