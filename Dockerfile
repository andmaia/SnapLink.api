# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar os arquivos de projeto necess�rios
COPY SnapLink.api/SnapLink.api.csproj ./SnapLink.api/
COPY Web/Web.csproj ./Web/

# Restaurar depend�ncias
RUN dotnet restore ./Web/Web.csproj

# Copiar todo o c�digo da solution
COPY . .

# Publicar a aplica��o Web
WORKDIR /app/Web
RUN dotnet publish -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar os arquivos publicados
COPY --from=build /app/Web/out .

# Definir vari�vel de ambiente para a URL da API
ENV ApiSettings__BaseUrl=http://localhost:5000

EXPOSE 5001
ENTRYPOINT ["dotnet", "Web.dll"]
