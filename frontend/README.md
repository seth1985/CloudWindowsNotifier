# Windows Notifier Cloud Frontend

## Overview

React + TypeScript admin portal for Cloud Windows Notifier.

This frontend is integrated with the backend API and supports day-to-day module administration workflows.

## Stack

- React 18
- TypeScript
- Vite 5
- Tailwind CSS
- Recharts

## Workspace Layout

- `src/App.tsx` - app composition and tab routing
- `src/features/auth` - login API and auth state
- `src/features/modules` - module APIs, hooks, forms, CRUD/export flow
- `src/features/templates` - template APIs and modals
- `src/features/telemetry` - reporting APIs and telemetry dashboard
- `src/features/users` - user management hooks/components
- `src/components/layout` - sidebar and app shell
- `src/core` - API client helpers and shared utilities
- `src/styles/global.css` - global styles and tokens

## Prerequisites

- Node.js 18+
- npm

## Quick Start

Install dependencies:

```bash
cd frontend
npm install
```

Run development server:

```bash
npm run dev
```

Default dev URL: `http://localhost:5173`

## Configuration

- Default API target in code: `http://localhost:5210`
- API base URL is editable in the login panel
- API base URL is persisted in `localStorage` as `wnc_api_base`
- Auth provider is discovered from `GET /api/config/auth`

Development auth defaults in the UI:

- Username: `admin`
- Password: `P@ssw0rd!`

When backend provider is switched to `Entra`, the login panel switches to Microsoft sign-in automatically.

## Implemented Capabilities

- Login against `/api/auth/login`
- Role-aware UI behavior for `Standard`, `Advanced`, and `Admin`
- Module creation flow with type-specific forms
- Icon and hero image uploads
- Export to Dev Core from module actions
- Module delete (single and multi-select)
- Template gallery CRUD for conditional/dynamic scripts
- Telemetry dashboard (`summary` and `per module` views)
- Admin users page

## Backend Endpoints Used

- `POST /api/auth/login`
- `GET/POST/PUT/DELETE /api/modules`
- `POST /api/modules/{id}/icon`
- `POST /api/modules/{id}/hero`
- `POST /api/export/{id}/devcore`
- `GET/POST/PUT/DELETE /api/templates`
- `GET /api/reporting/summary`
- `GET /api/reporting/modules`
- `GET/POST/PUT/DELETE /api/users`

## Common Commands

Run dev server:

```bash
npm run dev
```

Build production bundle:

```bash
npm run build
```

Preview production build:

```bash
npm run preview
```

## Notes

- Current logout action reloads the page; there is no backend token revocation flow.
- Static assets are under `public/icons` and `public/Hero`.
