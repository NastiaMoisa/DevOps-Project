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

set -a
. "${APP_DIR}/.env"
set +a

PUBLIC_API_URL="${PUBLIC_API_URL:-http://188.245.181.186:8080/api/v1/client}"
MAP_CLIENT_CONFIG_DIR="${APP_DIR}/deployment/compose"
MAP_CLIENT_CONFIG_FILE="${MAP_CLIENT_CONFIG_DIR}/map-client.config.json"

mkdir -p "${MAP_CLIENT_CONFIG_DIR}"
printf '{ "API": "%s" }\n' "${PUBLIC_API_URL}" > "${MAP_CLIENT_CONFIG_FILE}"

echo "Using map-client config:"
cat "${MAP_CLIENT_CONFIG_FILE}"

docker compose --env-file .env -f "${COMPOSE_FILE}" pull
docker compose --env-file .env -f "${COMPOSE_FILE}" up -d
docker compose --env-file .env -f "${COMPOSE_FILE}" ps
