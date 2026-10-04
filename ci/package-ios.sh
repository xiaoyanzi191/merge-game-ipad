#!/usr/bin/env bash
set -euo pipefail
[[ "$(uname -s)" == Darwin ]] || { echo "Xcode on macOS is required." >&2; exit 2; }
mode="${1:-unsigned}"
[[ "$mode" == unsigned || "$mode" == signed ]] || exit 2
mkdir -p Builds/artifacts
archive="$PWD/Builds/MergeSandbox.xcarchive"
args=(-project Builds/iOS/Unity-iPhone.xcodeproj -scheme Unity-iPhone -configuration Release
      -sdk iphoneos -destination 'generic/platform=iOS' -archivePath "$archive" archive)
if [[ "$mode" == unsigned ]]; then args+=(CODE_SIGNING_ALLOWED=NO CODE_SIGNING_REQUIRED=NO); fi
xcodebuild "${args[@]}" > Builds/xcode-archive.log 2>&1 || { tail -100 Builds/xcode-archive.log; exit 1; }
if [[ "$mode" == signed ]]; then
  signed_app=$(find "$archive/Products/Applications" -maxdepth 1 -name '*.app' -type d -print -quit)
  codesign --verify --deep --strict "$signed_app"
  export IOS_PROFILE_UUID APPLE_TEAM_ID IOS_BUNDLE_ID
  python3 - <<'SCRIPT'
import os,plistlib,pathlib
options={'method':'debugging','signingStyle':'manual','teamID':os.environ['APPLE_TEAM_ID'],
         'signingCertificate':'Apple Development','provisioningProfiles':{os.environ['IOS_BUNDLE_ID']:os.environ['IOS_PROFILE_UUID']}}
pathlib.Path('Builds/ExportOptions.plist').write_bytes(plistlib.dumps(options))
SCRIPT
  xcodebuild -exportArchive -archivePath "$archive" -exportOptionsPlist Builds/ExportOptions.plist     -exportPath Builds/artifacts > Builds/xcode-export.log 2>&1 || { tail -100 Builds/xcode-export.log; exit 1; }
else
  mkdir -p Builds/unsigned-package/Payload
  app=$(find "$archive/Products/Applications" -maxdepth 1 -name '*.app' -type d -print -quit)
  test -n "$app"
  ditto "$app" "Builds/unsigned-package/Payload/MergeSandbox.app"
  (cd Builds/unsigned-package && /usr/bin/zip -qry ../artifacts/MergeSandbox-UNSIGNED.ipa Payload)
  printf '%s\n' 'UNSIGNED: re-sign with your own Apple identity before installation.' > Builds/artifacts/SIGNING_REQUIRED.txt
fi
python3 ci/verify-ipa.py Builds/artifacts/*.ipa "$mode"
ditto -c -k --keepParent Builds/iOS Builds/artifacts/XcodeProject.zip
