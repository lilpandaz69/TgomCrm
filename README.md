# TGOMCRM

[![last commit](https://img.shields.io/github/last-commit/lilpandaz69/TgomCrm)](https://github.com/lilpandaz69/TgomCrm/commits)
[![language](https://img.shields.io/github/languages/top/lilpandaz69/TgomCrm)](https://github.com/lilpandaz69/TgomCrm)
[![repo size](https://img.shields.io/github/repo-size/lilpandaz69/TgomCrm)](https://github.com/lilpandaz69/TgomCrm)
[![license](https://img.shields.io/github/license/lilpandaz69/TgomCrm)](LICENSE)

> Empowering Growth Through Seamless Customer Connections — a modular, developer-friendly CRM built with a clean architecture.

---

## Table of Contents
- [Overview](#overview)  
- [Why TgomCrm?](#why-tgomcrm)  
- [Features](#features)  
- [Tech Stack](#tech-stack)  
- [Getting Started](#getting-started)  
  - [Prerequisites](#prerequisites)  
  - [Run Backend](#run-backend)  
  - [Run Frontend](#run-frontend)  
- [Configuration](#configuration)  
- [Contributing](#contributing)  
- [License](#license)  
- [Contact](#contact)

---

## Overview

TgomCrm is a comprehensive, modular CRM platform designed to accelerate enterprise application development with a clean separation of concerns and scalable architecture.  
It combines a robust backend API (C# .NET), secure JWT authentication, and a modern Angular frontend with server-side rendering support.

---

## Why TgomCrm?

This project aims to streamline CRM workflows by providing a flexible, developer-friendly foundation. The core goals are:
- Fast API-first development
- Clean domain-driven structure
- Easy integration with frontends and third-party services
- Production-ready patterns (EF Core, JWT, DI, layered projects)

---

## Features

- 🧩 **API-Driven Architecture** — REST endpoints for customers, products, suppliers, sales, and inventory.  
- 🚀 **Modern Frontend** — Angular with server-side rendering (optional) for SEO and performance.  
- 🔐 **Secure Authentication** — JWT-based auth + role/claims.  
- 💾 **Reliable Persistence** — Entity Framework Core for data access and migrations.  
- 🛠️ **Modular Design** — Domain / Application / Infrastructure separation for maintainability.  
- 🧑‍💻 **Developer Focused** — Well-structured codebase and clear starting points for features.

---

## Tech Stack

- Backend: C# / .NET (multi-project solution: `Tagom.Domain`, `Tagom.Application`, `Tagom.Infrastructure`, `TgomCrm`)
- Frontend: Angular (TypeScript) — SSR support
- ORM: Entity Framework Core
- Auth: JWT
- CI/CD / Packaging: (suggested) GitHub Actions, Docker

---

## Getting Started

### Prerequisites
- .NET SDK (6.0 or later)  
- Node.js & npm (for Angular)  
- SQL Server / PostgreSQL / SQLite (configurable via connection string)  
- (Optional) Docker

### Run Backend (example)
1. Clone the repo:
```bash
git clone https://github.com/lilpandaz69/TgomCrm.git
cd TgomCrm
