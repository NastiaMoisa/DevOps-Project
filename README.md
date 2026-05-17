# DevOps Project

- [DevOps Project](#devops-project)
  - [About](#about)
  - [Architecture](#architecture)
  - [Quick Start](#quick-start)
  - [VPS Deployment](#vps-deployment)
  - [Monitoring](#monitoring)
  - [Kubernetes](#kubernetes)
  - [Local Development](#local-development)
  - [Build](#build)
  - [CI/CD](#cicd)
  - [Lab Docs](#lab-docs)

## About
This repository contains a demo drone swarm control system used for the university DevOps course.

## Architecture

- `MapClient`: React + Vite UI for map-based control.
- `Communication Control`: ASP.NET Core API that stores hive state and exposes client endpoints.
- `Hive Mind`: ASP.NET Core API that simulates a drone swarm and periodically sends telemetry.
- `Redis`: transient storage and messaging support service.

## Quick Start

Run the full stack with Docker Compose:

```bash
docker compose up --build
```

Available services:
- Map UI: `http://localhost:3000`
- Communication Control Swagger: `http://localhost:8080/swagger`
- Hive Mind API: `http://localhost:5149/swagger`

## VPS Deployment

The VPS scenario uses prebuilt images from the GitLab container registry.

```bash
cp .env.example .env
docker compose -f docker-compose.prod.yml up -d
```

Before starting:
- update `CI_REGISTRY_IMAGE` and `IMAGE_TAG` in `.env`
- update `deployment/compose/map-client.config.json` with your public server IP or domain
- optionally bootstrap the server with `scripts/bootstrap-hetzner.sh`
- deploy manually with `scripts/deploy-vps.sh`

## Monitoring

Run the monitoring stack together with the application:

```bash
docker compose -f docker-compose.yml -f docker-compose.monitoring.yml up -d
```

Available services:
- Grafana: `http://localhost:3001`
- Loki: `http://localhost:3100`

## Kubernetes

Kubernetes manifests are stored in `deployment/app` and `deployment/infrastructure`.

Application workloads:
- `communication-control`
- `hive-mind`
- `map-client`
- `redis`

Before applying manifests:
- update image names to your GitLab registry path
- create the `gitlab-registry-secret`
- verify the client API URL in `deployment/app/map-client/map-client.config.yaml`

## Local Development

### Map Client

```bash
cd src/MapClient
npm install
npm run dev
```

### Communication Control

```bash
cd src/CommunicationControl
dotnet run --project DevOpsProject/DevOpsProject.CommunicationControl.API.csproj
```

### Hive Mind

```bash
cd src/CommunicationControl
dotnet run --project DevOpsProject.HiveMind.API/DevOpsProject.HiveMind.API.csproj
```

## Build

### Map Client

```bash
cd src/MapClient
npm install
npm run build
```

### Communiction Control

```bash
cd src/CommunicationControl
dotnet publish -c Release DevOpsProject/DevOpsProject.CommunicationControl.API.csproj
```

### Hive Mind

```bash
cd src/CommunicationControl
dotnet publish -c Release DevOpsProject.HiveMind.API/DevOpsProject.HiveMind.API.csproj
```

## CI/CD

GitLab CI validates and publishes three container images:
- `communication-control`
- `hive-mind`
- `map-client`

For branch builds, images are tagged with the branch slug. For the `main` branch, the pipeline also publishes the `latest` tag.

## Lab Docs

Supporting documentation for defense:
- [Deployment Strategy](./docs/deployment-strategy.md)
- [Lab Checklist](./docs/lab-checklist.md)
