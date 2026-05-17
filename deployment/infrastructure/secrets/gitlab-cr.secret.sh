kubectl create secret docker-registry gitlab-registry-secret \
--docker-server=registry.gitlab.com \
--docker-username="$CI_REGISTRY_USER" \
--docker-password="$CI_REGISTRY_PASSWORD" \
--docker-email="your-email@example.com" \
--namespace=default
