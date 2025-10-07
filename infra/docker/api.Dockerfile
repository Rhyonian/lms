# syntax=docker/dockerfile:1
FROM alpine:3.19
WORKDIR /app
CMD ["sh", "-c", "echo 'API service container is ready. Override API_DOCKERFILE or API_CMD to run the real service.'; sleep infinity"]
