# syntax=docker/dockerfile:1
FROM alpine:3.19
WORKDIR /app
CMD ["sh", "-c", "echo 'Web service container is ready. Override WEB_DOCKERFILE or WEB_CMD to run the real frontend.'; sleep infinity"]
