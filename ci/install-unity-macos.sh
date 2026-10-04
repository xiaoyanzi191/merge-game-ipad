#!/usr/bin/env bash
set -euo pipefail
[[ "$(uname -s)" == Darwin ]] || { echo "This installer is only for an ephemeral macOS build runner." >&2; exit 2; }
[[ "${GITHUB_ACTIONS:-}" == true && "${RUNNER_ENVIRONMENT:-}" == github-hosted ]] || {
  echo "Do not run the CI installer on a personal Mac. Install Unity through Unity Hub there." >&2; exit 2;
}
version=2022.3.62f3
revision=96770f904ca7
base="https://download.unity3d.com/download_unity/$revision"
for part in editor ios; do
  package="$RUNNER_TEMP/unity-$part.pkg"
  if [[ "$part" == editor ]]; then
    url="$base/MacEditorInstaller/Unity.pkg"
  else
    url="$base/MacEditorTargetInstaller/UnitySetup-iOS-Support-for-Editor-$version.pkg"
  fi
  curl --fail --location --retry 3 "$url" --output "$package"
  pkgutil --check-signature "$package" > "$RUNNER_TEMP/package-signature.txt"
  # Official Unity Developer ID, in addition to TLS and macOS package signature validation.
  grep -q 'Developer ID Installer: Unity Technologies' "$RUNNER_TEMP/package-signature.txt"
  sudo installer -pkg "$package" -target /
  rm -f "$package"
done
test -x /Applications/Unity/Unity.app/Contents/MacOS/Unity
echo 'UNITY_EDITOR=/Applications/Unity/Unity.app/Contents/MacOS/Unity' >> "$GITHUB_ENV"
