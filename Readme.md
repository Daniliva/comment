# Comments System

A modern, real-time comment system with **nested replies**, **file attachments (images & text)**, **CAPTCHA**, **dark/light theme**, and **SignalR** for instant updates.

## Features
- Add, reply, and delete comments (up to 5 levels deep)
- Attach images (JPG, PNG, GIF) or text files (TXT)
- Real-time updates via **SignalR**
- CAPTCHA protection
- Dark / Light theme toggle
- Responsive design

## Tech Stack
- **Frontend**: React, Bootstrap 5, Axios, SignalR Client
- **Backend**: ASP.NET Core, SignalR, MassTransit (RabbitMQ), Redis Cache
- **Deployment**: Docker + Nginx

## Quick Start

```bash
# Clone & run backend
dotnet run --project Backend

# Run frontend
cd frontend
npm start
```

Open: [http://localhost:3000](http://localhost:3000)

