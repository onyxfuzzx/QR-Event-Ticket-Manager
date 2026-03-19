# QR Event Platform - Enterprise Event Management Solution

A sophisticated, high-performance event management system designed for industrial-scale deployment. This platform provides a seamless bridge between digital registration and physical entry validation using encrypted QR technology. Built on a modernized **.NET 8 Ecosystem** and a high-fidelity **Angular Dashboard**, it offers mission-critical reliability and premium aesthetics.

---

## Technical Overview

### Core Architecture
The system follows a decoupled, service-oriented architecture designed for scalability and maintainability:

*   **API Layer**: High-performance ASP.NET Core Web API with JWT-based stateless authentication.
*   **Data Persistence**: Optimized SQL Server implementation utilizing **Dapper ORM** for low-latency database interactions and custom mapping.
*   **Real-time Communication**: Persistent Hub connections using **SignalR** to push system events and scan logs to administrative dashboards instantly.
*   **Security Layer**: Enterprise-grade password hashing via **BCrypt.Net** and Role-Based Access Control (RBAC) enforced at both API and UI route levels.

---

## Detailed Feature Modules

### 1. Administrative Control Center
The Administrative portal provides a 360-degree view of the event's health.
*   **Event Lifecycle Management**: Complete CRUD operations for multi-day events with timezone-aware scheduling.
*   **Static & Dynamic Analytics**: Real-time counters showing Total Tickets, Scanned Entries, and Active Worker counts.
*   **Worker Provisioning**: Granular control over worker accounts, including assignment of scanners to specific event gates.

### 2. Dynamic Registration Form Builder
A powerful visual tool for creating custom attendee intake forms:
*   **Field Versatility**: Support for Text, Email, Phone, Number, TextArea, and Dropdown types.
*   **Validation Rules**: Set mandatory constraints per-field.
*   **Live Preview**: Instantly visualize the attendee's registration experience while building.
*   **Persistent Schema**: Dynamic JSON-based storage for flexible form structures.

### 3. Integrated Email Design Studio
Designing branded communication without external tools:
*   **Block-Based Layout**: Arrange Headers, Body Text, and Ticket segments using an intuitive UI.
*   **Live Rendering**: Real-time HTML generation with a dedicated preview panel.
*   **Branded Tickets**: Automatically embed unique QR codes and event details into the ticket block.
*   **Test Dispatch**: Built-in SMTP testing to verify email appearance in actual mail clients.

### 4. High-Performance Scanner Portal
Optimized for the front lines, the Worker portal transforms any mobile device into a professional scanner:
*   **Hardware Integration**: Utilizes the device camera for fast QR decoding.
*   **Feedback System**: Integrated sounds for success/error and haptic vibration for "eyes-free" validation.
*   **Manual Override**: Secure fallback for damaged QR codes or camera issues.
*   **Live Synchronization**: Scans are immediately pushed to the central dashboard for real-time tracking.

---

## Security & Auth Flow

### Password Recovery & Maintenance
The platform implements a secure, token-driven recovery flow:
1.  **Request**: User enters email on the portal.
2.  **Generation**: A cryptographically secure, time-limited token is generated and stored.
3.  **Delivery**: A professional reset email is sent via the platform's SMTP service.
4.  **Exchange**: User validates the token and sets a new password, automatically invalidating the token.

---

## Technology Stack

![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular_17-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=signalr&logoColor=white)

---

## Project Structure

```bash
QR-Event-Ticket-Manager/
├── QREventPlatform.Advanced/   # Backend Engine
│   ├── Controllers/             # RESTful Resource Handlers
│   ├── Services/                # Core Logic Pipelines (Auth, Email, QR)
│   ├── Hubs/                    # SignalR Real-time Channels
│   └── SQL/                     # Consolidated Migration Scripts
└── QREventPlatform.UI/         # Frontend Interface
    ├── src/app/auth/            # Login & Reset Workflows
    ├── src/app/dashboards/      # High-fidelity Portals
    └── src/app/core/            # Global State & API Services
```

---

## Deployment & Setup

### Requirements
- .NET 8.0 SDK
- Node.js 18+ & NPM
- SQL Server (standard instance name)

### 1. Database Initialization
Execute the consolidated script in `QREventPlatform.Advanced/SQL/createDb.sql`. This initializes the `PasswordResetTokens` table and seeds the default administrator.

### 2. Service Configuration
Update `appsettings.json` with your:
- **ConnectionStrings**: Primary SQL Server URI.
- **Email Settings**: SMTP Host, Port, and Credentials.
- **JWT Key**: Secure signing key for tokens.

### 3. Build & Run
**Backend:**
```bash
cd QREventPlatform.Advanced
dotnet run
```
**Frontend:**
```bash
cd QREventPlatform.UI
npm install
npm start
```

---

## Default Administrator Credentials
- **Email**: `superadmin@qrevent.com`
- **Password**: `12345678`

---

© 2026 QR Event Platform | Designed for Premium Performance
