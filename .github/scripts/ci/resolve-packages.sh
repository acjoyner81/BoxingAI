#!/usr/bin/env bash
set -euo pipefail

# Resolve Unity packages for CI runs so the Package Manager is ready before
# compilation/build steps. This script tries a sensible default Unity binary
# path for macOS Hub installs. Override by setting UNITY_BIN in the environment.

PROJECT_PATH="$PWD"
LOG_DIR="$PROJECT_PATH/logs"
mkdir -p "$LOG_DIR"
LOG_FILE="$LOG_DIR/unity-resolve.log"

UNITY_BIN_DEFAULT="/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity"
UNITY_BIN="${UNITY_BIN:-$UNITY_BIN_DEFAULT}"

echo "Using Unity binary: $UNITY_BIN"
echo "Project path: $PROJECT_PATH"
echo "Writing log: $LOG_FILE"

if [ ! -x "$UNITY_BIN" ]; then
  echo "Unity binary not found or not executable: $UNITY_BIN" | tee "$LOG_FILE"
  echo "If you run this on GitHub Actions use game-ci/unity-builder or configure a self-hosted runner with Unity installed." | tee -a "$LOG_FILE"
  exit 2
fi

"$UNITY_BIN" -projectPath "$PROJECT_PATH" -batchmode -nographics -quit -logFile "$LOG_FILE" -executeMethod PackageResolver.ResolvePackages

EXIT_CODE=$?
echo "Unity exit code: $EXIT_CODE" >> "$LOG_FILE"
exit $EXIT_CODE
