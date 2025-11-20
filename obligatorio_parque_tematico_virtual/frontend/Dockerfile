# Utilizar una imagen base que incluya Node.js y npm
FROM node:20 AS builder

# Establecer el directorio de trabajo dentro del contenedor
WORKDIR /app

# Copiar los archivos de package.json y package-lock.json primero para aprovechar el cache de Docker
COPY package*.json ./

# Instalar las dependencias
RUN npm ci

# Copiar el resto de los archivos de la aplicación
COPY . .

# Construir la aplicación para producción
RUN npm run build

# Etapa de producción
FROM nginx:alpine

# Copiar configuración personalizada de nginx para Angular (manejar rutas)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copiar los archivos compilados de Angular desde la etapa anterior
COPY --from=builder /app/dist/frontend/browser /usr/share/nginx/html/

# El puerto que escucha NGINX por defecto
EXPOSE 80

# Comando para iniciar NGINX
CMD ["nginx", "-g", "daemon off;"]
