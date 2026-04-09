# api-aggregations

API REST em ASP.NET Core para gerir **Reservas** e **Produtos Reservados**, com **paginação** e **filtros**, persistidos em **SQL Server** via **Entity Framework Core**.

## Tecnologias

- .NET (`net10.0`)
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- Swagger (apenas em `Development`)

## Pré-requisitos

- .NET SDK 10
- SQL Server (ex.: Express/LocalDB) com uma base de dados acessível pela connection string

## Configuração

Esta API lê a connection string do SQL Server a partir da variável de ambiente `SECRET` (carregada via ficheiro `.env` com `DotNetEnv`).

Cria/edita o ficheiro `.env` na raiz do projecto:

```env
SECRET="Server=SEU_SERVIDOR;Database=api_aggregations;Trusted_Connection=true;TrustServerCertificate=true;"
```

## Executar

```powershell
dotnet restore
dotnet run
```

Por omissão (perfil `Development`), os URLs locais são:

- HTTP: `http://localhost:5092` (redirecciona para HTTPS)
- HTTPS: `https://localhost:7167`

Swagger (em `Development`):

- `https://localhost:7167/swagger`

## Testes

```powershell
dotnet test
```

## Endpoints

### Reservas (`/reserva`)

- `GET /reserva` — lista paginada
  - Query: `PageNumber` (>=1), `PageSize` (1..100), `numero`, `tipo`, `estado`, `id_externo`
- `GET /reserva/{id}`
- `POST /reserva`
- `PUT /reserva/{id}`
- `DELETE /reserva/{id}`

### Produtos Reservados (`/produtoreservado`)

- `GET /produtoreservado` — lista paginada
  - Query: `PageNumber` (>=1), `PageSize` (1..100), `id_reserva`, `id_produto`, `estado`, `referencia`, `agregado`
- `GET /produtoreservado/{id}`
- `POST /produtoreservado`
- `PUT /produtoreservado/{id}`
- `DELETE /produtoreservado/{id}`

## Respostas de erro

A API devolve erros no formato `ProblemDetails` (JSON) com:

- `400` para paginação inválida
- `404` quando o recurso não existe
- `500` para erros inesperados

## Notas

- O ficheiro `api-agregations.http` tem um exemplo de request para testes rápidos.
- O nome do projecto/solução usa a grafia `api-agregations` (sem o segundo “g”), mas o namespace é `api_aggregations`.


