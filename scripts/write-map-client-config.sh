#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="${ROOT_DIR}/.env"
TARGET_FILE="${1:-${ROOT_DIR}/deployment/compose/map-client.config.json}"
DEFAULT_API_URL="http://localhost:8080/api/v1/client"

if [ -f "${ENV_FILE}" ]; then
  set -a
  # shellcheck disable=SC1090
  . "${ENV_FILE}"
  set +a
fi

API_URL="${PUBLIC_API_URL:-${DEFAULT_API_URL}}"

mkdir -p "$(dirname "${TARGET_FILE}")"
printf '{ "API": "%s" }\n' "${API_URL}" > "${TARGET_FILE}"

echo "Wrote ${TARGET_FILE} with API=${API_URL}"
