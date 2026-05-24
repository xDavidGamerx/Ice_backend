# Runbook Local - ICE Backend

Este documento detalla los pasos obligatorios para configurar y ejecutar el entorno de desarrollo local. El proyecto utiliza variables de entorno estrictas y Docker para la infraestructura (PostgreSQL y Redis).

## 1. Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
- [Docker y Docker Compose](https://www.docker.com/products/docker-desktop) instalados y ejecutándose.
- Herramienta global de migraciones de Entity Framework Core (`dotnet-ef`):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## 2. Configurar Variables de Entorno

El proyecto implementa validación "Fail-Fast". **Si no configuras tus variables de entorno, la API no arrancará y arrojará un `InvalidOperationException` detallado.**

1. Copia el archivo de ejemplo en la raíz del repositorio:
   ```bash
   cp .env.example .env
   ```
2. Edita `.env` para incluir tus configuraciones, si es necesario. (La configuración de base de datos y Redis por defecto coincide con los contenedores locales de Docker).

## 3. Levantar Infraestructura (Postgres + Redis)

El proyecto incluye un `docker-compose.yml` que levanta los servicios mínimos:

```bash
# Inicia los servicios en segundo plano
docker-compose up -d

# Para verificar que estén corriendo:
docker-compose ps
```

Ejecutar con Docker Compose (API incluida):
```bash
# Asegúrate de tener .env en la raíz del proyecto
docker-compose up --build -d

# API disponible en: http://localhost:5000/swagger
# Health check: http://localhost:5000/health
```

> **Nota**: El `.env` debe existir antes de ejecutar `docker-compose up`. Las variables de Stripe, OAuth y CDN NO se definen en `docker-compose.yml`; se heredan del `.env`. Si alguna falta, la API fallará al arrancar con `ValidateOnStart()`.

## 4. Inicializar y Migrar la Base de Datos

Una vez que PostgreSQL está corriendo, aplica las migraciones de EF Core para generar el esquema:

```bash
# Ejecutar desde la raíz del repositorio
dotnet ef database update --project src/IceBackend.Infrastructure --startup-project src/IceBackend.Api
```

## 5. Compilar y Ejecutar

Verifica que el entorno funciona correctamente ejecutando el servidor de desarrollo:

```bash
# Ejecutar la API
dotnet run --project src/IceBackend.Api
```

La API buscará e inyectará automáticamente las variables de tu archivo `.env`. Puedes acceder a Swagger en: `http://localhost:5000/swagger` (el puerto puede variar según `launchSettings.json`).

## 6. Resolución de Problemas

- **Error: "CRITICAL ERROR: 'PostgresConnection' is missing or has a placeholder"**: Tu archivo `.env` no está bien configurado o el cargador no lo encontró en la raíz del proyecto.
- **Error conectando a Postgres**: Asegúrate de que `docker-compose up -d` se ejecutó correctamente y que no hay otro servicio usando el puerto `5432` en tu máquina.
