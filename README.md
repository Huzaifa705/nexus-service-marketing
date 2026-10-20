# NEXUS Service Marketing System

Web application for Nexus Communication System: orders, feasibility, connections and account IDs,
equipment and vendors, billing and payments, a customer portal, and search.

**Status:** Node 2 baseline. Tooling, layout and a health endpoint exist; there are no business
features yet. The plan and every decision are in [`docs/`](docs/).

## Prerequisites

- Node.js 22 or newer (see `.nvmrc`) and Git
- A code editor such as VS Code

## First run (Windows PowerShell)

```powershell
npm ci
Copy-Item .env.example .env.local
npm run dev
```

Open http://localhost:3000. The health check is http://localhost:3000/health.

## Commands

| Command | What it does |
|---|---|
| `npm run dev` | Start the development server |
| `npm run lint` | ESLint |
| `npm run typecheck` | Generate route types, then TypeScript with no emit |
| `npm test` | Vitest (unit and document tests) |
| `npm run docs:check` | Consistency check of the five documents in `docs/` |
| `npm run audit:prod` | Fails on high or critical vulnerabilities in production dependencies |
| `npm run build` | Production build |
| `npm run verify` | docs:check, lint, typecheck, test and build in one go |

## Layout

```
app/                 routes and layouts
components/          shared UI
lib/domain/          pure business logic (identifiers, billing, state machines, permissions)
lib/db/              data access and transactions
supabase/migrations/ committed SQL migrations
tests/               unit, integration and end-to-end tests
docs/                requirements, decisions, architecture, test plan, known issues
scripts/             tooling (check-docs.mjs)
```

Next.js collects anonymous usage telemetry by default; opt out with `npx next telemetry disable`.

## Environment variables

Names only are listed in `.env.example`. Real values go in `.env.local` (ignored by Git) and in the
hosting dashboards. Never put secrets in code, commits, chat or screenshots.

## Hosting

Free plans are for local development and non-commercial demos only; live business use needs paid
plans (decision D-26 in `docs/decision-log.md`).
