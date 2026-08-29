# conference-room-booking-api

REST API for managing conference rooms, services, bookings, room availability, and reports.

## Technologies

- C#
- .NET
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- xUnit
- Swagger

## Features

- Create, update and delete rooms
- Add and update room services
- Search for available rooms
- Create bookings
- Validate booking conflicts
- Calculate booking prices
- Generate hourly booking reports
- Generate room revenue reports

## API Endpoints

### Rooms

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/rooms` | Create a new room |
| PUT | `/api/rooms/{id}` | Update a room |
| DELETE | `/api/rooms/{id}` | Delete a room |
| DELETE | `/api/rooms/services/{id}` | Delete a room |
| GET | `/api/rooms/{id}` | Get a room by ID |
| GET | `/api/rooms/available` | Search for available rooms |

### Bookings

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/bookings` | Create a new booking |

### Reports

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/reports/hourly-bookings` | Get booking count by hour |
| GET | `/api/reports/room-revenue` | Get booking count and revenue per room |

## API Documentation

The API is documented using Swagger.

After starting the application, Swagger UI is available at:

`/swagger`

Swagger provides interactive documentation and allows API endpoints to be tested directly from the browser.

## Database Setup

The application uses PostgreSQL.

Configure the connection string locally:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ConferenceRoomBookingDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

The actual database password should not be committed to the repository.

## Database Migrations

The project uses Entity Framework Core migrations to manage the database schema.

Install the Entity Framework Core CLI tool if it is not already installed:

```bash
dotnet tool install --global dotnet-ef
```

Restore project dependencies:

```bash
dotnet restore
```

Apply the existing migrations:

```bash
dotnet ef database update --project ConferenceRoomBookingApi
```

## Running the Application

Build the project:

```bash
dotnet build
```

Run the application:

```bash
dotnet run --project ConferenceRoomBookingApi
```

The API can then be accessed through Swagger.

## Testing

The project contains unit tests covering room management, booking logic, room availability, and reporting functionality.

Run all tests with:

```bash
dotnet test
```

## Project Documentation

The `docs` directory contains the class diagram describing the main entities, services, and their relationships.

The diagram illustrates the relationships between:

- Rooms
- Services
- Bookings
- Application services
- AppDbContext

## Architecture

The application follows a service-based architecture:

- **Controllers** handle HTTP requests and responses.
- **Services** contain business logic.
- **DTOs** define request and response models.
- **Models** represent domain entities.
- **AppDbContext** provides access to the PostgreSQL database.
- **GlobalExceptionHandler** provides centralized exception handling.

## Error Handling

The API uses centralized exception handling through `GlobalExceptionHandler`.

Exceptions are converted into appropriate HTTP responses using the ASP.NET Core Problem Details mechanism.
