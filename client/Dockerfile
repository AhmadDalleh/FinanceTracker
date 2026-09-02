FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/client/browser /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

# API_INTERNAL_URL should point at the API service via Railway's private network,
# e.g. http://api.railway.internal:8080 - see README "Deployment" section.
ENV API_INTERNAL_URL=http://localhost:8080
ENV PORT=8080
EXPOSE 8080
