# SpaceAge game-host — Mono + Game.exe + visual tool (Railway-ready; GM laptop docker compose today).
# Player-agent + Ollama stay on the GM host (not in this image).

FROM ubuntu:22.04 AS game-build
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update \
  && apt-get install -y --no-install-recommends mono-complete ca-certificates \
  && rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY Game/ Game/
RUN xbuild /p:Configuration=Debug /verbosity:minimal Game/Game.csproj

FROM node:20-bookworm-slim AS visual-build
WORKDIR /src
COPY visual-tool/package.json visual-tool/package-lock.json ./
RUN npm ci
COPY visual-tool/ ./
RUN npm run build

FROM ubuntu:22.04 AS runtime
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update \
  && apt-get install -y --no-install-recommends mono-complete ca-certificates curl \
  && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

WORKDIR /app
ENV REPO_ROOT=/app
ENV GAME_USE_MONO=1
ENV GAME_HOST_PORT=8787

COPY --from=game-build /src/Game/bin/Debug/Game.exe Game/bin/Debug/Game.exe
COPY --from=visual-build /src/dist visual-tool/dist/
COPY game-host/package.json game-host/
RUN cd game-host && npm install --omit=dev
COPY game-host/ game-host/
COPY campaign/ campaign/

EXPOSE 8787
COPY docker/entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]
