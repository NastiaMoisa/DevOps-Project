# Lab Checklist

## Lab 1: IaaS
- Create VPS
- Run `scripts/bootstrap-hetzner.sh`
- Configure firewall
- Create `.env` or configure GitLab deploy variables
- Prepare `deployment/compose/map-client.config.json`
- Run `docker compose --env-file .env -f docker-compose.prod.yml up -d`

## Lab 2: PaaS
- Choose Render, Railway, Azure App Service, or DigitalOcean App Platform
- Deploy `communication-control`
- Optionally deploy `map-client`
- Compare PaaS vs IaaS

## Lab 3: Docker
- Build backend image
- Build hive-mind image
- Build frontend image
- Run at least one container manually

## Lab 4: Docker Compose
- Run `docker compose up --build`
- Show all four services working together
- Explain networks, ports, and dependencies

## Lab 5: CI/CD
- Push repository to GitLab
- Configure registry variables
- Run pipeline
- Show images published to registry

## Lab 6: Monitoring
- Run `docker compose -f docker-compose.yml -f docker-compose.monitoring.yml up -d`
- Open Grafana
- Show Loki datasource
- Show logs from app containers

## RGR: Kubernetes
- Create registry secret
- Apply redis manifests
- Apply communication-control manifests
- Apply hive-mind manifests
- Apply map-client manifests
- Apply ingress
- Show pods, services, ingress
