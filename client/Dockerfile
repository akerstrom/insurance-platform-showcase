# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm ci

# Build application
COPY . .
RUN npm run build

# Production stage
FROM nginx:alpine AS runtime

LABEL org.opencontainers.image.title="ips-web" \
      org.opencontainers.image.description="ThreadPilot Web UI - Insurance management interface" \
      org.opencontainers.image.source="https://github.com/peterakerstrom/insurance-platform-showcase"

# Copy built application and nginx config
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
COPY config.template.json /etc/nginx/config.template.json

# Default API URL (override via environment variable)
ENV API_BASE_URL=""

EXPOSE 8080

# Generate config.json from template at startup, then run nginx
CMD ["/bin/sh", "-c", "envsubst < /etc/nginx/config.template.json > /usr/share/nginx/html/config.json && nginx -g 'daemon off;'"]
