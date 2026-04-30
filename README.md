# Poang

Ett webbprojekt för spelhantering med realtidsuppdateringar.
Frontend ligger i Vue och backend i ASP.NET Core, med SQL Server i Docker för lokal utveckling.

## Teknik

- Frontend: Vue 3 + TypeScript + Vite
- Backend: ASP.NET Core (`net10.0`)
- Databas: Microsoft SQL Server (Docker)
- Realtid: SignalR

## Krav

- Node.js och npm
- .NET SDK 10
- Docker Desktop

## Kom igang

Installera beroenden i root och klient:

```bash
npm install
npm run install:client
```

Starta hela utvecklingsmiljon (databas + server + klient):

```bash
npm run dev
```

Alternativt, skapa `.env` automatiskt och starta med:

```bash
npm run start
```

Vanliga databas-kommandon:

```bash
npm run db:up
npm run db:down
npm run db:logs
```

## Projektstruktur

- `client/` - Vue-applikationen
- `server/` - API, SignalR och affarslogik
- `docker-compose.yml` - lokal SQL Server
- `scripts/setup-env.js` - skapar `.env` fran `.env.example` vid behov
