# Stage 1: Build
FROM node:24-alpine AS build

# Set working directory inside container
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json ./
RUN npm ci

# Copy all source code (including src/app)
COPY . .

# Build the Next.js app
RUN npm run build

# Stage 2: Run
FROM node:24-alpine AS run

WORKDIR /app

# Copy built files and dependencies from build stage
COPY --from=build /app ./

# Expose default Next.js port
EXPOSE 3000

# Start the Next.js server
CMD ["npm", "start"]
