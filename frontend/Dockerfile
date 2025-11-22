# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Copy package files
COPY package*.json ./

# Clean npm cache and install dependencies
# Try with legacy peer deps for React 19 compatibility
RUN npm cache clean --force && \
    npm ci --legacy-peer-deps --no-fund --no-audit || \
    npm install --legacy-peer-deps --no-fund --no-audit

# Copy source code
COPY . .

# Build the app
RUN npm run build

# Stage 2: Production with nginx
FROM nginx:alpine AS production

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built files from build stage
COPY --from=build /app/build /usr/share/nginx/html

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]


