# Stage 1: Build DocFX site
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS docfx-build

# Create non-root user
RUN addgroup -S docfx && adduser -S docfx -G docfx

# Switch early
USER docfx
ENV HOME=/home/docfx
ENV PATH="$PATH:/home/docfx/.dotnet/tools"

WORKDIR /workspace

# Install DocFX as docfx user
RUN dotnet tool install -g docfx

# Copy sources
COPY --chown=docfx:docfx docfx ./docfx
COPY --chown=docfx:docfx src ./src

WORKDIR /workspace/docfx
RUN docfx docfx_project/docfx.json

# Stage 2: Runtime (Nginx)
FROM nginx:alpine

# Create non-root user
RUN addgroup -S nginx-user && adduser -S nginx-user -G nginx-user

# Prepare runtime directories
RUN mkdir -p /var/cache/nginx /var/run \
    && chown -R nginx-user:nginx-user /var/cache/nginx /var/run

# Remove default content and config
RUN rm -rf /usr/share/nginx/html/* \
    && rm /etc/nginx/conf.d/default.conf

# Copy static site
COPY --from=docfx-build \
     --chown=nginx-user:nginx-user \
     /workspace/docfx/docfx_project/_site \
     /usr/share/nginx/html

# Provide nginx config
COPY docfx/nginx.conf /etc/nginx/nginx.conf

USER nginx-user

EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]