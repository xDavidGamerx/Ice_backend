# ICE Backend API

> Backend del ICE Launcher — autenticación, inventario de cosméticos, suscripción ICE+ y webhooks Stripe.

![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat)
![Tests](https://img.shields.io/badge/tests-57%2F57-blue?style=flat)
![License](https://img.shields.io/badge/license-MIT-yellow?style=flat)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)

---

## Tabla de Contenidos

- [Caracteristicas](#caracteristicas)
- [Stack Tecnologico](#stack-tecnologico)
- [Quick Start (local)](#quick-start-local)
- [Despliegue en servidor real](#despliegue-en-servidor-real)
- [Variables de Entorno](#variables-de-entorno)
- [Endpoints](#endpoints)
- [Tests](#tests)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Auditoria de Logs (secretos)](#auditoria-de-logs-secretos)
- [Contribuir](#contribuir)
- [Licencia](#licencia)

---

## Caracteristicas

- Autenticacion por sesiones Redis + OAuth2 PKCE (Microsoft, Google)
- Inventario de cosmeticos con equipamiento y desequipamiento
- Suscripcion ICE+ con cache Redis (Cache-Aside + anti-penetration)
- Webhooks Stripe idempotentes (`invoice.paid`, `invoice.payment_failed`, `customer.subscription.deleted`)
- URLs prefirmadas para CDN con entrega JIT
- Cache de sesiones con Lua scripting y purga atomica
- Endpoints de desarrollo con DevLogin y DevSeed
- Docker Compose ready (PostgreSQL 15 + Redis 7 + API)
- Swagger/OpenAPI integrado
- Logs estructurados con Serilog (JSON)
- 57 tests automatizados (40 unit + 17 integracion)

---

## Stack Tecnologico

| Capa           | Tecnologia                                                    |
|----------------|---------------------------------------------------------------|
| Runtime        | .NET 8 (ASP.NET Core)                                        |
| Arquitectura   | Clean Architecture (Domain, Application, Infrastructure, API) |
| ORM            | Entity Framework Core + Npgsql                               |
| Base de datos  | PostgreSQL 15                                                |
| Cache          | Redis 7 (StackExchange.Redis)                                |
| Logs           | Serilog (formato JSON compacto)                              |
| Tests          | xUnit + Testcontainers (Postgres + Redis reales)             |
| Autenticacion  | Session tokens en Redis + OAuth2 PKCE                        |
| Pagos          | Stripe Webhooks                                               |

---

## Quick Start (local)

### Requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (WSL2 backend recomendado)
- [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### Pasos

```bash
git clone https://github.com/xDavidGamerx/Ice_backend.git
cd Ice_backend

# 1. Configurar variables de entorno
cp .env.example .env
# Editar .env segun sea necesario (los valores por defecto funcionan localmente)

# 2. Levantar infraestructura (Postgres + Redis)
docker-compose up -d postgres redis

# 3. Aplicar migraciones de EF Core
dotnet ef database update --project src/IceBackend.Infrastructure --startup-project src/IceBackend.Api

# 4. Ejecutar la API
dotnet run --project src/IceBackend.Api

# 5. Abrir Swagger
open http://localhost:5000/swagger
```

### Con Docker Compose (API incluida)

```bash
docker-compose up --build -d
# API en http://localhost:5000/swagger
# Health check en http://localhost:5000/health
```

> **Nota**: El archivo `.env` debe existir antes de ejecutar `docker-compose up`. Las variables de Stripe, OAuth y CDN se heredan del `.env`. Si alguna falta, la API fallara al arrancar con `ValidateOnStart()`.

---

## Despliegue en servidor real

### Requisitos del servidor

- Linux (Ubuntu 22.04+ o Debian 12+ recomendado)
- Docker Engine 24+ y Docker Compose plugin
- Dominio con SSL (Let's Encrypt via Certbot)
- (Opcional) Nginx como reverse proxy

### Pasos

```bash
# 1. Conectarse al servidor y clonar
git clone https://github.com/xDavidGamerx/Ice_backend.git
cd Ice_backend

# 2. Configurar entorno de produccion
cp .env.example .env
# Editar .env con valores reales de produccion:
#   - ConnectionStrings__PostgresConnection (password fuerte)
#   - Stripe__SecretKey y Stripe__WebhookSecret
#   - OAuth__* (ClientId y ClientSecret reales)
#   - Cdn__SigningSecret (token criptograficamente seguro)
#   - CORS__AllowedOrigins (https://tu-dominio.com)
#   - ASPNETCORE_ENVIRONMENT=Production

# 3. Levantar todo
docker-compose up --build -d

# 4. Verificar salud
curl http://localhost:5000/health

# 5. Configurar Nginx como reverse proxy (ejemplo abajo)
```

### Ejemplo de configuracion Nginx

Crea `/etc/nginx/sites-available/ice-api`:

```nginx
server {
    listen 80;
    server_name api.ice.gg;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/ice-api /etc/nginx/sites-enabled/
sudo certbot --nginx -d api.ice.gg
sudo nginx -t && sudo systemctl reload nginx
```

### Backup de volumenes

```bash
# PostgreSQL
docker run --rm -v ice_postgres_data:/source -v /backup:/dest alpine tar czf /dest/postgres-backup-$(date +%F).tar.gz -C /source .

# Redis (RDB persistente)
docker run --rm -v ice_redis_data:/source -v /backup:/dest alpine tar czf /dest/redis-backup-$(date +%F).tar.gz -C /source .
```

### Logs en produccion

```bash
# Ver logs de la API
docker logs -f ice_api

# Solo estructurados (JSON)
docker logs ice_api 2>&1 | Select-String -Pattern "level"  # Linux: grep '"level"'
```

---

## Variables de Entorno

| Variable                                        | Obligatoria | Descripcion                            |
|-------------------------------------------------|-------------|----------------------------------------|
| `ConnectionStrings__PostgresConnection`         | Si          | Cadena de conexion PostgreSQL          |
| `ConnectionStrings__RedisConnection`            | Si          | Cadena de conexion Redis               |
| `DEV_MASTER_TOKEN`                              | No*         | Token maestro para endpoints dev       |
| `Stripe__SecretKey`                             | Si          | Stripe Secret Key                      |
| `Stripe__WebhookSecret`                         | Si          | Stripe Webhook Secret                  |
| `OAuth__Microsoft__ClientId`                   | Si          | Microsoft OAuth Client ID              |
| `OAuth__Microsoft__ClientSecret`               | Si          | Microsoft OAuth Client Secret          |
| `OAuth__Microsoft__TenantId`                   | No          | Tenant ID (default: consumers)         |
| `OAuth__Google__ClientId`                      | Si          | Google OAuth Client ID                 |
| `OAuth__Google__ClientSecret`                  | Si          | Google OAuth Client Secret             |
| `Cdn__BaseUrl`                                  | Si          | URL base del CDN                       |
| `Cdn__SigningSecret`                            | Si          | Secreto para firmar URLs de CDN        |
| `Cdn__UrlExpirationMinutes`                     | No          | TTL de URLs prefirmadas (default: 15)  |
| `CORS__AllowedOrigins`                          | Si          | Origenes CORS separados por coma       |
| `ASPNETCORE_ENVIRONMENT`                        | No          | `Development` o `Production`           |

> \* Solo necesario para Development. En produccion los endpoints dev estan bloqueados por entorno.

---

## Endpoints

| Metodo | Ruta                                   | Auth        | Descripcion                    |
|--------|----------------------------------------|-------------|--------------------------------|
| GET    | `/health`                              | No          | Health check                   |
| POST   | `/api/v1/dev/login`                    | Dev         | Login de desarrollo            |
| POST   | `/api/v1/dev/seed`                     | Dev         | Sembrar datos de prueba        |
| POST   | `/api/v1/inventory/equip`              | Sesion      | Equipar cosmetico              |
| POST   | `/api/v1/inventory/unequip`            | Sesion      | Desequipar cosmetico           |
| GET    | `/api/v1/assets/info/{id}`             | Sesion      | Informacion de cosmetico       |
| POST   | `/api/v1/assets/request-delivery`      | Sesion      | Solicitar entrega de activo    |
| GET    | `/api/v1/assets/deliver/{hash}`        | Token       | Descargar activo cosmético     |
| GET    | `/api/auth/external/microsoft/login`   | No          | Iniciar login Microsoft        |
| GET    | `/api/auth/external/google/login`      | No          | Iniciar login Google           |
| POST   | `/api/auth/external/callback`          | No          | Callback OAuth                 |
| POST   | `/api/webhooks/stripe`                 | Stripe      | Webhook de Stripe              |
| GET    | `/api/v1/subscription/me`              | Sesion      | Estado de mi suscripcion ICE+  |

> Documentacion interactiva completa en Swagger: `http://localhost:5000/swagger`

---

## Tests

```bash
# Tests unitarios (40) — no requieren Docker
dotnet test tests/IceBackend.UnitTests

# Tests de integracion (17) — requieren Docker Desktop corriendo
dotnet test tests/IceBackend.IntegrationTests

# Todos (57)
dotnet test
```

Los tests de integracion usan Testcontainers para levantar PostgreSQL 15 y Redis 7 reales en contenedores Docker, asegurando fidelidad con el entorno de produccion.

---

## Estructura del Proyecto

```
src/
├── IceBackend.Api/              # API layer (Controllers, Middleware, Program.cs)
├── IceBackend.Application/      # Use cases, interfaces, DTOs, Options
├── IceBackend.Domain/           # Entities, Value Objects, Domain Services
└── IceBackend.Infrastructure/   # EF Core, Redis, Stripe, CDN, Repositories
tests/
├── IceBackend.UnitTests/        # Unit tests (40)
└── IceBackend.IntegrationTests/ # Integration tests (17)
docs/
├── database.sql                 # Esquema de base de datos
├── local-runbook.md             # Guia detallada de ejecucion local
└── Basedatos.png                # Diagrama de base de datos
```

---

## Auditoria de Logs (secretos)

A partir de la revision de seguridad (Task 19), se confirma que **ningun secreto se imprime en logs**:

- `Log.Information` solo contiene mensajes fijos sin interpolacion de variables sensibles
- Los mensajes de error de validacion Fail-Fast mencionan "missing o placeholder", nunca el valor real
- No existen `Log.X` que incluyan claves, tokens o passwords en ningun archivo `.cs`
- El File sink de Serilog esta configurado exclusivamente en `appsettings.Development.json` — no afecta a produccion

---

## Contribuir

1. Fork el repositorio
2. Crea una rama: `git checkout -b feat/mi-feature`
3. Haz commit: `git commit -m "feat(scope): descripcion"`
4. Haz push: `git push origin feat/mi-feature`
5. Abre un Pull Request

Los mensajes de commit siguen [Conventional Commits](https://www.conventionalcommits.org/):
- `feat(scope):` — nueva funcionalidad
- `fix(scope):` — correccion de bug
- `docs(scope):` — documentacion
- `refactor(scope):` — cambio sin alterar comportamiento
- `test(scope):` — pruebas

---

## Licencia

MIT
