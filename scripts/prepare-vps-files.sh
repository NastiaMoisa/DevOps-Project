#!/usr/bin/env bash

set -euo pipefail

TARGET_DIR="${1:-./deploy-package}"

rm -rf "${TARGET_DIR}"
mkdir -p "${TARGET_DIR}/deployment/compose"

cp docker-compose.prod.yml "${TARGET_DIR}/docker-compose.prod.yml"
cp .env.example "${TARGET_DIR}/.env.example"
cp deployment/compose/map-client.config.json "${TARGET_DIR}/deployment/compose/map-client.config.json"
cp scripts/deploy-vps.sh "${TARGET_DIR}/deploy-vps.sh"

echo "Prepared deployment bundle in ${TARGET_DIR}"
