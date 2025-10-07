# syntax=docker/dockerfile:1
FROM alpine:3.19
WORKDIR /app
CMD ["sh", "-c", "echo 'Worker service container is ready. Override WORKER_DOCKERFILE or WORKER_CMD to run the real worker.'; sleep infinity"]
