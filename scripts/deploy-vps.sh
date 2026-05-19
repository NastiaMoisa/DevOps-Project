#!/usr/bin/env bash

set -euo pipefail

APP_DIR="${APP_DIR:-$(pwd)}"
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.prod.yml}"

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker is required on the target host."
  exit 1
fi

if ! docker compose version >/dev/null 2>&1; then
  echo "Docker Compose plugin is required on the target host."
  exit 1
fi

if [ ! -f "${APP_DIR}/.env" ]; then
  echo "Missing ${APP_DIR}/.env"
  exit 1
fi

if [ ! -f "${APP_DIR}/${COMPOSE_FILE}" ]; then
  echo "Missing ${APP_DIR}/${COMPOSE_FILE}"
  exit 1
fi

cd "${APP_DIR}"

docker compose --env-file .env -f "${COMPOSE_FILE}" pull
docker compose --env-file .env -f "${COMPOSE_FILE}" up -d
docker compose --env-file .env -f "${COMPOSE_FILE}" ps
