#!/bin/sh
set -eu
export GAME_HOST_PORT="${PORT:-${GAME_HOST_PORT:-8787}}"
export REPO_ROOT="${REPO_ROOT:-/app}"
cd /app
exec node game-host/server.mjs
