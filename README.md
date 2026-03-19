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

---

## Technical Stack & Indicators

![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular_17-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=signalr&logoColor=white)

---

## Full Tutorial: System Implementation

This guide provides a comprehensive path to deploying the QR Event Platform from source.

### Phase 1: Environment Preparation

Ensure the following system dependencies are installed and accessible:
- **Programming Language**: .NET 8.0 SDK (verify with `dotnet --version`)
- **Runtime Environment**: Node.js 18.x or later (verify with `node -v`)
- **Database Engine**: Microsoft SQL Server (2019+ or LocalDB)
- **Tooling**: Angular CLI (optional but recommended: `npm install -g @angular/cli`)

### Phase 2: Database Layer Initialization

The platform requires a pre-provisioned schema. Follow these steps:

1.  Open **SQL Server Management Studio (SSMS)** or your preferred SQL tool.
2.  Connect to your server instance.
3.  Navigate to the repository folder: `QREventPlatform.Advanced/SQL/`.
4.  Open and execute the **`createDb.sql`** script.
    - This script performs three critical actions:
        - Creates all core tables (Users, Events, Tickets, etc.).
        - Initializes the `PasswordResetTokens` table for security recovery.
        - Seeds the initial **SuperAdmin** user into the database.

---

### Phase 3: Backend Configuration

1.  Navigate to the backend project directory:
    ```bash
    cd QREventPlatform.Advanced
    ```

2.  Locate `appsettings.json` (Important: This file is excluded from Git by default for security). If it does not exist, create it following this template:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=prism;Trusted_Connection=True;TrustServerCertificate=True"
      },
      "Jwt": {
        "Key": "YOUR_SUPER_SECRET_KEY_MIN_32_CHARS",
        "Issuer": "QREventPlatform",
        "Audience": "QREventPlatformUser"
      },
      "Email": {
        "SmtpHost": "smtp.gmail.com",
        "SmtpPort": 587,
        "SmtpUser": "your-email@gmail.com",
        "SmtpPass": "your-app-password"
      },
      "AllowedHosts": "*"
    }
    ```

3.  **Authentication Security**: Ensure the `Jwt:Key` is a robust, unique string.
4.  **Email Integration**: Configure a valid SMTP provider (like Gmail App Passwords) to enable the "Forgot Password" feature.
5.  Restore dependencies and launch:
    ```bash
    dotnet restore
    dotnet run
    ```

---

### Phase 4: Frontend Implementation

1.  Navigate to the UI project directory:
    ```bash
    cd QREventPlatform.UI
    ```

2.  **Dependency Installation**:
    ```bash
    npm install
    ```

3.  **Environment Sync**: Ensure `src/environments/environment.ts` correctly points to your backend URL (usually `http://localhost:5036/api`).

4.  **Launch the Application**:
    ```bash
    npm start
    ```
    - The portal will be accessible at **`http://localhost:4200`**.

---

### Phase 5: Initial Access & Roles

Identify the system role hierarchy to begin development:

- **SuperAdmin**: Full system control.
    - Default Email: `superadmin@qrevent.com`
    - Default Password: `12345678`
- **Admin**: Event-specific management. (Created by SuperAdmin)
- **Worker**: Scanner-only access. (Created by Admins)

---

## Security & Maintenance Workflows

### Authentication Recovery
If a user loses access, the platform utilizes a secure, multi-stage recovery:
1.  User enters email on the login page via the "Forgot Password" link.
2.  The backend generates a cryptographically secure token via the `AuthService`.
3.  An automated email is sent with a unique reset link pointing to the `/reset-password` frontend route.
4.  Upon validation, the user securely updates their password in the `PasswordResetTokens` table.

---

## 🏗️ Project Structure Reference

```bash
QR-Event-Ticket-Manager/
├── QREventPlatform.Advanced/   # Backend Engine (.NET 8)
│   ├── Controllers/             # Auth, User, Event, Ticket API Endpoints
│   ├── Services/                # EmailService, AuthService, TokenGenerator
│   ├── Hubs/                    # LiveScanHub (Real-time events)
│   └── SQL/                     # Unified Migration & Seed Scripts
└── QREventPlatform.UI/         # Frontend Interface (Angular + Tailwind)
    ├── src/app/auth/            # Login, Forgot & Reset Password pages
    ├── src/app/dashboards/      # SuperAdmin, Admin, and Worker portals
    └── src/app/core/            # Interceptors, Guards, and API clients
```

---

## Troubleshooting Guide

- **CORS Errors**: Verify the `AllowedOrigins` in `Program.cs` matches the frontend URL.
- **SQL Connection Failed**: Ensure SQL Server is running and the connection string in `appsettings.json` is accurate.
- **Email Not Sending**: Verify SMTP credentials and ensure "Less Secure Apps" or "App Passwords" are enabled for your provider.
- **SignalR Connection Issues**: Ensure the `Hubs` are correctly mapped in `Program.cs` and the frontend imports `@microsoft/signalr`.

---

© 2026 QR Event Platform | Designed for Premium Performance
