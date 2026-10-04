#!/usr/bin/env bash
set -euo pipefail
[[ "${RUNNER_ENVIRONMENT:-}" == github-hosted ]] || { echo "Signing setup is restricted to ephemeral hosted runners." >&2; exit 2; }
: "${IOS_CERTIFICATE_P12_BASE64:?Missing your Apple Development certificate}"
: "${IOS_CERTIFICATE_PASSWORD:?Missing certificate password}"
: "${IOS_PROFILE_BASE64:?Missing device provisioning profile}"
: "${APPLE_TEAM_ID:?Missing Apple team ID}"
signing="$RUNNER_TEMP/local-merge-signing"
mkdir -p "$signing"
chmod 700 "$signing"
export SIGNING_DIR="$signing"
python3 - <<'SCRIPT'
import os,base64,pathlib
p=pathlib.Path(os.environ['SIGNING_DIR'])
for key,name in [('IOS_CERTIFICATE_P12_BASE64','certificate.p12'),('IOS_PROFILE_BASE64','profile.mobileprovision')]:
 (p/name).write_bytes(base64.b64decode(os.environ[key],validate=True))
SCRIPT
security cms -D -i "$signing/profile.mobileprovision" > "$signing/profile.plist"
python3 - <<'SCRIPT'
import plistlib,os,pathlib,datetime
p=pathlib.Path(os.environ['SIGNING_DIR'])
d=plistlib.loads((p/'profile.plist').read_bytes())
assert os.environ['APPLE_TEAM_ID'] in d['TeamIdentifier'], 'Profile team does not match'
assert d['ExpirationDate'] > datetime.datetime.now(datetime.timezone.utc).replace(tzinfo=None), 'Profile expired'
assert d.get('ProvisionedDevices'), 'A development profile containing your target device is required'
assert d['Entitlements'].get('get-task-allow'), 'Use an Apple Development profile'
app=d['Entitlements']['application-identifier'].split('.',1)[1]
bundle=os.environ.get('IOS_BUNDLE_ID','com.localmerge.sandbox')
assert app==bundle or (app.endswith('*') and bundle.startswith(app[:-1])), 'Profile does not match the bundle ID'
with open(os.environ['GITHUB_ENV'],'a') as f:
 f.write('IOS_PROFILE_UUID='+d['UUID']+'\n')
 f.write('IOS_KEYCHAIN_PATH='+str(p/'build.keychain-db')+'\n')
with open(os.environ['GITHUB_OUTPUT'],'a') as f: f.write('uuid='+d['UUID']+'\n')
SCRIPT
uuid=$(/usr/libexec/PlistBuddy -c 'Print UUID' "$signing/profile.plist")
keychain="$signing/build.keychain-db"
keypass=$(openssl rand -hex 32)
security create-keychain -p "$keypass" "$keychain"
security set-keychain-settings -lut 21600 "$keychain"
security unlock-keychain -p "$keypass" "$keychain"
security import "$signing/certificate.p12" -P "$IOS_CERTIFICATE_PASSWORD" -T /usr/bin/codesign -T /usr/bin/security -t cert -f pkcs12 -k "$keychain"
security set-key-partition-list -S apple-tool:,apple:,codesign: -s -k "$keypass" "$keychain" > /dev/null
security list-keychains -d user -s "$keychain" "$HOME/Library/Keychains/login.keychain-db"
mkdir -p "$HOME/Library/MobileDevice/Provisioning Profiles"
cp "$signing/profile.mobileprovision" "$HOME/Library/MobileDevice/Provisioning Profiles/$uuid.mobileprovision"
