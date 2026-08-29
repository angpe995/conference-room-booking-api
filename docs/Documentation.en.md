# Project Documentation

## 1. Business Overview

### Purpose
This system is designed to automate the rental and reservation of conference rooms for business use. It helps companies manage available spaces, control bookings, calculate service costs, and obtain analytics about room utilization.

### Core business features
- Management of conference rooms: create, update, delete, and check availability.
- Management of additional services: projector, Wi-Fi, sound, and other room-related services.
- Search for available rooms based on capacity, time, and booking requirements.
- Booking of rooms with conflict validation across time intervals.
- Automatic cost calculation based on time and selected services.
- Report generation for business users and managers.

### Pricing logic
The system applies a time-based pricing model to support flexible and efficient resource management.

- Standard hours: 09:00–18:00 — base price.
- Evening hours: 18:00–23:00 — 20% discount.
- Morning hours: 06:00–09:00 — 10% discount.
- Peak hours: 12:00–14:00 — 15% surcharge.

This approach allows the system to reflect different levels of room demand throughout the day and helps businesses optimize their pricing policy.

### Booking pricing calculation
The price of a booking is calculated using the selected room rate, the duration of the reservation, and any additional services included in the booking.

The booking process supports only full-hour time slots. This means that reservations must start and end at a full hour, for example:
- valid: 10:00–12:00
- valid: 09:00–10:00
- invalid: 11:20–12:20

This rule ensures consistency in billing, room availability checks, and reporting. If a reservation spans multiple time periods, the price is calculated separately for each segment and then combined into the final total.

### Reports and analytics
The system generates analytical reports for business users, including:
- hourly booking reports;
- room revenue reports;
- room occupancy analysis by time period.

This allows management to monitor room utilization, optimize occupancy, and make informed decisions about pricing and resource distribution.

---

## 2. Technical Overview

### Architecture
The project follows a service-based architecture with a clear separation of responsibilities:

- Controllers handle HTTP requests and responses.
- Services contain business logic and validation.
- DTOs are used for data transfer between layers.
- Models represent database entities and business domain objects.

This structure improves maintainability, simplifies testing, and supports future scaling without tightly coupling components.

### Error handling
The project implements centralized exception handling through GlobalExceptionHandler. This ensures:
- consistent conversion of runtime errors into HTTP responses;
- unified processing of conflicts and not-found scenarios;
- use of the Problem Details standard for predictable API behavior.

### Data layer
The application uses Entity Framework Core with PostgreSQL. This provides:
- a clear object-relational mapping;
- schema versioning through migrations;
- reliable integration with the project’s business logic.

### Testing
The project includes xUnit unit tests covering the main business logic:
- booking creation and validation;
- conflict detection for overlapping reservations;
- price calculation and time-based rules;
- room availability checks;
- report generation.

This testing approach helps maintain a stable and reliable API during business logic changes.

---

## Summary
The project implements a complete conference room booking system that combines business logic, resource utilization analytics, and clean software architecture. It meets the needs of automated booking management, availability control, pricing, and reporting for business users.