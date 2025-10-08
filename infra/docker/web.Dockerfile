FROM node:20-alpine

WORKDIR /app
COPY apps/web/package*.json apps/web/
WORKDIR /app/apps/web
RUN npm ci || npm install

COPY apps/web/ .

EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "5173"]
