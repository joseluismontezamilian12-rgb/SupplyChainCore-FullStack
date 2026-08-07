# syntax=docker/dockerfile:1

# ---------- Etapa de compilación ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Los .csproj se copian primero y solos: mientras no cambien las dependencias,
# Docker reutiliza la capa de restore y no vuelve a descargar los paquetes.
COPY SupplyChainCore.Domain/*.csproj          SupplyChainCore.Domain/
COPY SupplyChainCore.Application/*.csproj     SupplyChainCore.Application/
COPY SupplyChainCore.Infrastructure/*.csproj  SupplyChainCore.Infrastructure/
COPY SupplyChainCore.WebApi/*.csproj          SupplyChainCore.WebApi/
RUN dotnet restore SupplyChainCore.WebApi/SupplyChainCore.WebApi.csproj

COPY SupplyChainCore.Domain/          SupplyChainCore.Domain/
COPY SupplyChainCore.Application/     SupplyChainCore.Application/
COPY SupplyChainCore.Infrastructure/  SupplyChainCore.Infrastructure/
COPY SupplyChainCore.WebApi/          SupplyChainCore.WebApi/

RUN dotnet publish SupplyChainCore.WebApi/SupplyChainCore.WebApi.csproj \
    -c Release -o /app/publish --no-restore

# ---------- Etapa de ejecución ----------
# Imagen sin SDK ni compilador: solo el runtime necesario para servir la API.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Usuario sin privilegios: si alguien logra ejecutar código dentro del
# contenedor, no lo hace como root.
USER $APP_UID

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SupplyChainCore.WebApi.dll"]
