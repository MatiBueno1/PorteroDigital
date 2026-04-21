# Versión de SDK y Runtime (Usando .NET 10 Preview)
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build-env
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY src/PorteroDigital.Domain/*.csproj ./src/PorteroDigital.Domain/
COPY src/PorteroDigital.Application/*.csproj ./src/PorteroDigital.Application/
COPY src/PorteroDigital.Infrastructure/*.csproj ./src/PorteroDigital.Infrastructure/
COPY src/PorteroDigital.WebAPI/*.csproj ./src/PorteroDigital.WebAPI/
RUN dotnet restore src/PorteroDigital.WebAPI/PorteroDigital.WebAPI.csproj

# Copiar el resto del código y compilar
COPY . ./
RUN dotnet publish src/PorteroDigital.WebAPI/PorteroDigital.WebAPI.csproj -c Release -o out

# Generar la imagen final
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build-env /app/out .

# El puerto se configura dinámicamente vía Program.cs y variable PORT
ENTRYPOINT ["dotnet", "PorteroDigital.WebAPI.dll"]
