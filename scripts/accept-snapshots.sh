#!/usr/bin/env bash
# Accepts pending snapshots by renaming every *.received.* file to *.verified.*.
# Usage: scripts/accept-snapshots.sh [directory]   (defaults to the current directory)
set -euo pipefail

root="${1:-.}"
count=0
while IFS= read -r -d '' received; do
  name="$(basename -- "$received")"
  verified="$(dirname -- "$received")/${name/.received./.verified.}"
  mv -f -- "$received" "$verified"
  echo "accepted ${verified#"$root"/}"
  count=$((count + 1))
done < <(find "$root" -type f -name '*.received.*' -not -path '*/bin/*' -not -path '*/obj/*' -print0)

echo "$count snapshot(s) accepted."
