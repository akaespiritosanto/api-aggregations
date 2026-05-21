# api-aggregations

ASP.NET Core Web API for reservations, reserved products, monthly totals, and Excel report export.

The project uses SQL Server with Entity Framework Core. The Excel export uses the local Expedita DLLs stored in the `excel/` folder.

## Project Structure

- `Controllers/` receives HTTP requests and returns API responses.
- `Services/` contains the business logic and database queries.
- `Models/` maps database tables/views to C# classes.
- `Dtos/` contains request/response shapes used by the API.
- `Data/AppDbContext.cs` configures Entity Framework Core.
- `Filters/` handles API key authorization and consistent error responses.
- `Utils/DateStringHelper.cs` keeps date parsing/formatting consistent.
- `DbScripts/` contains SQL scripts used to create/update the database.
- `excel/` contains the local Expedita Excel export dependencies.

## Requirements

- .NET SDK 10
- SQL Server
- A database created with the scripts in `DbScripts/`

## Configuration

Create a `.env` file in the project root:

```env
SECRET=Server=YOUR_SERVER;Database=api_aggregations;Trusted_Connection=true;TrustServerCertificate=true;
API_KEY=your-local-api-key
```

`SECRET` is mandatory. It is the SQL Server connection string.

`API_KEY` is optional. If it is set, every request must send:

```text
X-API-KEY: your-local-api-key
```

If `API_KEY` is not set, the API key filter is disabled so local development is easier.

## Run Locally

```powershell
dotnet restore
dotnet run
```

Development URLs:

- `http://localhost:5092`
- `https://localhost:7167`

Swagger is available in development:

- `https://localhost:7167/swagger`

## Run Tests

```powershell
dotnet test
```

## Docker

```powershell
docker compose up --build
```

Docker URL:

- `http://localhost:8080`

## Main Endpoints

### Reservations

Base route: `/reserva`

- `GET /reserva` lists reservations with pagination and filters.
- `GET /reserva/{id}` returns one reservation.
- `POST /reserva` creates a reservation.
- `PUT /reserva/{id}` updates a reservation.
- `DELETE /reserva/{id}` deletes a reservation.
- `GET /reserva/totais` returns grouped reservation totals.

### Reserved Products

Base route: `/produtoreservado`

- `GET /produtoreservado` lists reserved products with pagination and filters.
- `GET /produtoreservado/{id}` returns one reserved product.
- `POST /produtoreservado` creates a reserved product.
- `PUT /produtoreservado/{id}` updates a reserved product.
- `DELETE /produtoreservado/{id}` deletes a reserved product.
- `GET /produtoreservado/totais` returns grouped reserved product totals.

### Reservation Value/Duration Reports

Base route: `/relatoriovaloreseduracaoreservas`

- `GET /totaisProduto` returns monthly totals grouped by product.
- `GET /totaisLugar` returns monthly totals grouped by place.
- `GET /listDisponibilidadesBase?idServico=...` returns available DispBase references.
- `GET /exportar` downloads an Excel report.

Excel export query parameters:

```text
agruparPor=produto|lugar
mostrar=valor|duracao
idServico=optional
idDispBase=optional
dataInicio=optional
dataFim=optional
```

Example:

```text
GET /relatoriovaloreseduracaoreservas/exportar?agruparPor=produto&mostrar=valor&idServico=5
```

## Error Handling

Errors are returned as `ProblemDetails` JSON.

- `400` for invalid input.
- `401` for missing/invalid API key when `API_KEY` is configured.
- `404` when a record does not exist.
- `409` for conflicts such as duplicate keys.
- `500` for unexpected errors.

## Notes for Developers

- Keep generated folders such as `bin/`, `obj/`, and `temp-obj/` out of source control.
- Keep `.env` out of source control because it can contain secrets.
- The project references local Expedita DLLs for Excel export, so do not delete the `excel/` folder.
- Dates are stored/read as strings in some database columns. Use `DateStringHelper` instead of parsing dates manually.
- The Excel export has a small XML merge-cell fix after Expedita creates the file. This is intentional and keeps the downloaded workbook from having the wrong merged header cells.
