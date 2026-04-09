#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if ! command -v cloc >/dev/null 2>&1; then
  echo "cloc is not installed or not in PATH. Install it, then retry:" >&2
  echo "  macOS:         brew install cloc" >&2
  echo "  Debian/Ubuntu: sudo apt install cloc" >&2
  echo "  Other:         https://github.com/AlDanial/cloc#installing-cloc" >&2
  exit 127
fi

cloc . --vcs git \
  --exclude-dir=node_modules,dist,.nuxt,.output,.turbo,.vite,coverage,.nitro,.data,.cache,out,generated
