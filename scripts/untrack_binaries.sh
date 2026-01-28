#!/usr/bin/env bash
# scripts/untrack_binaries.sh
# Safe helper to find tracked compiled binaries (DLLs, PDBs, exes, shared libs),
# untrack them from the git index, and optionally commit the cleanup.
#
# Usage:
#   # Dry-run (default) - lists files that would be untracked
#   /opt/homebrew/bin/bash scripts/untrack_binaries.sh --dry-run
#
#   # Actually untrack and commit (prompts for confirmation unless --yes)
#   /opt/homebrew/bin/bash scripts/untrack_binaries.sh --apply
#   /opt/homebrew/bin/bash scripts/untrack_binaries.sh --apply --yes
#
# Notes:
# - This script only removes files from the git index (`git rm --cached`),
#   it will NOT delete the files from disk. Files will remain on disk but will
#   be ignored by git after `.gitignore` contains matching rules.
# - Make sure you have committed or stashed any important local changes before
#   running with `--apply`.
# - Designed for macOS with Homebrew bash, but works with any POSIX bash.

set -euo pipefail
IFS=$'\n\t'

ROOT_DIR="$(git rev-parse --show-toplevel 2>/dev/null || true)"
if [ -z "$ROOT_DIR" ]; then
  echo "Error: not inside a git repository. Run this from a repo working tree." >&2
  exit 2
fi
cd "$ROOT_DIR"

DRY_RUN=true
ASSUME_YES=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --apply) DRY_RUN=false; shift ;;
    --yes) ASSUME_YES=true; shift ;;
    --dry-run) DRY_RUN=true; shift ;;
    -h|--help) echo "Usage: $0 [--dry-run] [--apply] [--yes]"; exit 0 ;;
    *) echo "Unknown arg: $1"; echo "Usage: $0 [--dry-run] [--apply] [--yes]"; exit 2 ;;
  esac
done

EXTENSIONS=(dll pdb exe so dylib)
PATTERNS=("\\.dll$" "\\.pdb$" "\\.exe$" "\\.so$" "\\.dylib$")

echo "Scanning repository for tracked binary files..."
tracked_files=()

for pat in "${PATTERNS[@]}"; do
  while IFS= read -r f; do
    # avoid empty
    if [ -n "$f" ]; then
      tracked_files+=("$f")
    fi
  done < <(git ls-files | grep -Ei "$pat" || true)
done

# Also include files inside any committed bin/ or obj/ directories
while IFS= read -r f; do
  if [ -n "$f" ]; then
    tracked_files+=("$f")
  fi
done < <(git ls-files | grep -Ei '(^|/)bin/|(^|/)obj/' || true)

# Deduplicate
if [ ${#tracked_files[@]} -gt 0 ]; then
  # use awk to dedupe while preserving newline safe handling
  mapfile -t tracked_files < <(printf "%s\n" "${tracked_files[@]}" | awk '!x[$0]++')
fi

if [ ${#tracked_files[@]} -eq 0 ]; then
  echo "No tracked binary files (dll/pdb/exe/so/dylib) or bin/obj files found in git index." 
  exit 0
fi

echo "Found ${#tracked_files[@]} tracked files:" 
for f in "${tracked_files[@]}"; do
  echo "  $f"
done

if [ "$DRY_RUN" = true ]; then
  echo "\nDry run: no changes will be made. To untrack and commit, run with --apply".
  exit 0
fi

if [ "$ASSUME_YES" = false ]; then
  echo -n "Proceed to untrack these files from git index and commit changes? [y/N] "; read -r resp
  case "$resp" in
    [yY]|[yY][eE][sS]) ;;
    *) echo "Aborted by user."; exit 0 ;;
  esac
fi

# Untrack files
echo "Untracking files from git index..."
for f in "${tracked_files[@]}"; do
  git rm --cached --ignore-unmatch -- "$f" || true
done

# Stage .gitignore in case it's been updated
if git check-ignore -q .gitignore; then
  # .gitignore itself ignored? unlikely; still stage if modified
  :
fi

git add .gitignore || true

# Commit staged changes if any
if git diff --staged --quiet; then
  echo "No staged changes to commit (nothing to do)."
else
  git commit -m "chore: untrack committed binaries and ensure they are ignored by .gitignore"
  echo "Committed removal of tracked binaries."
fi

echo "Cleanup complete. Files remain on disk but are no longer tracked by git."

echo "Tip: verify by running: git ls-files | grep -Ei '\\.(dll|pdb|exe|so|dylib)$' || true"

exit 0
