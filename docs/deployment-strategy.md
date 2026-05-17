# Deployment Strategy

This project is designed so one codebase can cover every DevOps lab and the Kubernetes course project.

## What We Deploy and Where

### Lab 1: IaaS

Deploy the full stack to a VPS:
- `communication-control`
- `hive-mind`
- `map-client`
- `redis`

Recommended approach:
- Use GitLab CI to publish images.
- Bootstrap the Hetzner VPS with `scripts/bootstrap-hetzner.sh`.
- Copy `.env.example` to `.env` on the VPS or let GitLab deploy generate `.env`.
- Copy `deployment/compose/map-client.config.json.example` to `deployment/compose/map-client.config.json`.
- Start the stack with `docker compose --env-file .env -f docker-compose.prod.yml up -d`.
- Optionally use `scripts/deploy-vps.sh` for repeated deploys.

This lab demonstrates:
- manual server provisioning
- SSH access
- firewall rules
- manual deployment without platform automation

### Lab 2: PaaS

Deploy one or two public-facing services to a PaaS platform.

Recommended scope for fast defense:
- `map-client` as a static/web service
- `communication-control` as a web service

Notes:
- `redis` can stay external or be replaced with a managed Redis offering if the platform provides one.
- `hive-mind` can be omitted from the minimal PaaS demo if the platform makes multi-service deployment inconvenient.

This lab demonstrates:
- simplified deployment
- reduced server management
- comparison against IaaS

### Lab 3: Docker

Use the service Dockerfiles:
- `src/CommunicationControl/DevOpsProject/Dockerfile`
- `src/CommunicationControl/DevOpsProject.HiveMind.API/Dockerfile`
- `src/MapClient/Dockerfile`

### Lab 4: Docker Compose

Use:
- `docker-compose.yml`

### Lab 5: CI/CD

Use:
- `.gitlab-ci.yml`

The pipeline validates:
- .NET solution build
- frontend production build
- Docker image build and push for all services

### Lab 6: Monitoring

Use:
- `docker-compose.monitoring.yml`

Monitoring stack:
- Grafana
- Loki
- Promtail

### Course Project / RGR: Kubernetes

Use Kubernetes manifests from:
- `deployment/app`
- `deployment/infrastructure`

Deploy:
- `communication-control`
- `hive-mind`
- `map-client`
- `redis`
- ingress

## Recommended Final Defense Flow

1. Show local Docker Compose stack.
2. Show GitLab CI pipeline building images.
3. Show monitoring stack in Grafana/Loki.
4. Show VPS deployment plan with `docker-compose.prod.yml`.
5. Show PaaS deployment scope and comparison with IaaS.
6. Show Kubernetes manifests and local or cloud cluster deployment.
