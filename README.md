# QREventPlatform

QR-based event management platform with a .NET backend and Angular frontend.

## Project Type

**Full-stack web application (Monorepo)**

- Backend API: ASP.NET Core (.NET 8)
- Frontend UI: Angular + Tailwind CSS
- Database: SQL Server
- Realtime updates: SignalR

## Repository Structure

```
QREventPlatform/
├─ QREventPlatform.Advanced/   # Backend API
└─ QREventPlatform.UI/         # Frontend app
```

## Features

- Role-based auth (Admin, Super Admin, workers)
- Event creation and management
- Ticket creation and validation
- QR scan flows for event entry
- Event forms and audit-related endpoints
- Notification and realtime hub support

## Tech Stack

### Backend (`QREventPlatform.Advanced`)
- ASP.NET Core Web API (.NET 8)
- Dapper
- SQL Server
- JWT authentication
- SignalR

### Frontend (`QREventPlatform.UI`)
- Angular
- TypeScript
- Tailwind CSS

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js (LTS) and npm
- SQL Server
- Angular CLI (optional, recommended)

### 1) Clone

```bash
git clone <your-repository-url>
cd QREventPlatform
```

### 2) Backend Setup

```bash
cd QREventPlatform.Advanced
dotnet restore
dotnet run
```

Backend runs using settings in `appsettings.json` and environment-specific overrides.

### 3) Frontend Setup

```bash
cd ../QREventPlatform.UI
npm install
npm start
```

Frontend default URL is typically `http://localhost:4200`.

## Configuration

- Do not commit secrets.
- Use local/environment overrides for:
  - database connection string
  - JWT key
  - SMTP credentials

## API and Development Notes

- Main backend entry point: `QREventPlatform.Advanced/Program.cs`
- Main frontend entry point: `QREventPlatform.UI/src/main.ts`
- SQL scripts are in `QREventPlatform.Advanced/SQL/`

## Git Hygiene

This repository includes a root `.gitignore` for:

- build outputs (`bin/`, `obj/`, `dist/`)
- caches (`.vs/`, `node_modules/`, Angular cache)
- logs, temp files, and local secret files


