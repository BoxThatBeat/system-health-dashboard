# system-health-dashboard

A full-stack "Hello World" application with an Angular 19 frontend and .NET 8 backend.

## Project Structure

- `frontend/` - Angular 19 TypeScript application
- `backend/` - .NET 8 C# REST API
- `backend.tests/` - NUnit test project for the backend

## Prerequisites

- Node.js (v20 or higher)
- Yarn package manager
- .NET 8 SDK

## Frontend Setup

The frontend is an Angular 19 application using TypeScript, with unit tests configured using Jasmine and Karma.

### Install Dependencies
```bash
cd frontend
yarn install
```

### Run Development Server
```bash
cd frontend
yarn start
```
The application will be available at `http://localhost:4200`

### Build
```bash
cd frontend
yarn build
```
Build artifacts will be stored in the `frontend/dist/` directory.

### Run Tests
```bash
cd frontend
yarn test
```

### Run Tests (Headless)
```bash
cd frontend
yarn test --browsers=ChromeHeadless --watch=false
```

## Backend Setup

The backend is a .NET 8 Web API with REST endpoints, tested using NUnit.

### Restore Dependencies
```bash
cd backend
dotnet restore
```

### Run Development Server
```bash
cd backend
dotnet run
```
The API will be available at `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS)

### Build
```bash
cd backend
dotnet build
```

### Run Tests
```bash
cd backend.tests
dotnet test
```

## API Endpoints

- `GET /api/hello` - Returns a "Hello World" message with timestamp
- `GET /weatherforecast` - Returns sample weather forecast data (demo endpoint)

### Example Response from `/api/hello`:
```json
{
  "message": "Hello World",
  "timestamp": "2025-10-02T23:00:00.000Z"
}
```

## Technology Stack

### Frontend
- Angular 19
- TypeScript
- Yarn (package manager)
- Jasmine & Karma (testing)

### Backend
- .NET 8
- C#
- ASP.NET Core Web API
- NUnit (testing)
- Microsoft.AspNetCore.Mvc.Testing (integration testing)