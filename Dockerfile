# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o .csproj da subpasta
COPY TesteYardim.CSharp/TesteYardim.CSharp.csproj ./TesteYardim.CSharp/

# Restaura dentro da subpasta
WORKDIR /src/TesteYardim.CSharp
RUN dotnet restore

# Volta e copia todo o código da pasta TesteYardim.CSharp
WORKDIR /src
COPY TesteYardim.CSharp/ ./TesteYardim.CSharp/

# Publica
WORKDIR /src/TesteYardim.CSharp
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copia a pasta Data
COPY --from=build /src/TesteYardim.CSharp/Data ./Data/

# Copia a aplicação publicada
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TesteYardim.CSharp.dll"]