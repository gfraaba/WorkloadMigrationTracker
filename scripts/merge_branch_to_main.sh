#!/usr/bin/env bash
# scripts/merge_branch_to_main.sh
# Safely merge a feature branch into `main` with optional push.
# Usage:
#   ./scripts/merge_branch_to_main.sh <branch-name> [--push] [--fetch]
#
# By default this script will:
#  - fetch from `origin` if `--fetch` is provided
#  - ensure a local `main` exists (create from `origin/main` if available)
#  - pull `origin/main` into local `main` (if available)
#  - merge the specified branch into local `main` (no fast-forward)
#  - if conflicts occur, the script exits with instructions to resolve them
#  - only pushes `main` when `--push` is passed

set -euo pipefail
IFS=$'\n\t'

if [ "$#" -lt 1 ]; then
  echo "Usage: $0 <branch-to-merge> [--push] [--fetch]"
  exit 2
fi

BRANCH="$1"
shift || true
DO_PUSH=false
DO_FETCH=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --push) DO_PUSH=true; shift ;;
    --fetch) DO_FETCH=true; shift ;;
    -h|--help) echo "Usage: $0 <branch-to-merge> [--push] [--fetch]"; exit 0 ;;
    *) echo "Unknown option: $1"; echo "Usage: $0 <branch-to-merge> [--push] [--fetch]"; exit 2 ;;
  esac
done

ROOT_DIR="$(git rev-parse --show-toplevel 2>/dev/null || true)"
if [ -z "$ROOT_DIR" ]; then
  echo "Not in a git repository. Run this from the repo working tree." >&2
  exit 3
fi
cd "$ROOT_DIR"

if [ "$DO_FETCH" = true ]; then
  echo "Fetching origin..."
  git fetch origin --prune
fi

# Check branch exists locally or remotely
if git rev-parse --verify --quiet "refs/heads/$BRANCH" >/dev/null; then
  echo "Found local branch $BRANCH"
elif git ls-remote --exit-code --heads origin "$BRANCH" >/dev/null 2>&1; then
  echo "Branch $BRANCH only exists on origin; fetching it..."
  git fetch origin "$BRANCH":"refs/heads/$BRANCH"
else
  echo "Branch '$BRANCH' not found locally or on origin." >&2
  exit 4
fi

# Ensure local main exists
if git show-ref --verify --quiet refs/heads/main; then
  echo "Using existing local 'main' branch"
  git checkout main
else
  if git ls-remote --exit-code --heads origin main >/dev/null 2>&1; then
    echo "Creating local 'main' from origin/main"
    git checkout -b main origin/main
  else
    echo "Creating new local 'main' branch (no origin/main found)"
    git checkout -b main
  fi
fi

# Pull origin/main if present
if git rev-parse --verify --quiet origin/main >/dev/null; then
  echo "Pulling origin/main into local main (fast-forward or merge)..."
  git pull origin main --ff-only || true
fi

echo "Merging branch '$BRANCH' into 'main' (no-ff)..."
set +e
git merge --no-ff "$BRANCH" -m "Merge branch '$BRANCH' into main"
MERGE_STATUS=$?
set -e

if [ $MERGE_STATUS -ne 0 ]; then
  echo "Merge finished with conflicts or errors (exit code $MERGE_STATUS)."
  echo "Resolve conflicts, then run: git commit -m 'Merge resolved'" 
  echo "To abort merge: git merge --abort"
  exit $MERGE_STATUS
fi

echo "Merge successful."
if [ "$DO_PUSH" = true ]; then
  echo "Pushing 'main' to origin..."
  git push origin main
  echo "Push completed."
else
  echo "Local 'main' updated. Run 'git push origin main' to push changes." 
fi

echo "Done."
exit 0
