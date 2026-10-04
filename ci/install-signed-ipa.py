#!/usr/bin/env python3
"""Install only an explicitly selected IPA to an explicitly selected USB device.
No pairing/reset/trust changes, app removal, or background device scans.
"""
import argparse,datetime,pathlib,plistlib,shutil,subprocess,tempfile,zipfile
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--ipa',type=pathlib.Path,required=True)
p.add_argument('--udid',required=True)
p.add_argument('--check-only',action='store_true',help='Check package and provisioning metadata without contacting the device')
a=p.parse_args()
if not a.ipa.is_file():p.error('IPA file does not exist')
with zipfile.ZipFile(a.ipa) as z:
 infos=[n for n in z.namelist() if n.startswith('Payload/') and n.count('/')==2 and n.endswith('/Info.plist')]
 if len(infos)!=1:p.error('Expected one iOS app')
 prefix=infos[0].rsplit('/',1)[0]+'/'
 if prefix+'embedded.mobileprovision' not in z.namelist() or not any(n.startswith(prefix+'_CodeSignature/') for n in z.namelist()):
  p.error('Unsigned IPA cannot be installed. Sign it with your own Apple identity first.')
 info=plistlib.loads(z.read(infos[0]))
 if info.get('CFBundleSupportedPlatforms')!=['iPhoneOS']:p.error('Not an iOS device build')
 profile=z.read(prefix+'embedded.mobileprovision')
if not shutil.which('openssl'):p.error('OpenSSL is required to read provisioning metadata')
# Verify CMS signature integrity, without representing this as an Apple trust-chain check.
result=subprocess.run(['openssl','cms','-verify','-noverify','-inform','DER'],input=profile,capture_output=True)
if result.returncode:p.error('Could not verify/read provisioning CMS content')
d=plistlib.loads(result.stdout)
if a.udid not in d.get('ProvisionedDevices',[]):p.error('This device is not listed in the provisioning profile')
if d['ExpirationDate']<=datetime.datetime.now(datetime.timezone.utc).replace(tzinfo=None):p.error('Provisioning profile has expired')
app_id=d['Entitlements']['application-identifier'].split('.',1)[1]
bundle=info['CFBundleIdentifier']
if not (app_id==bundle or (app_id.endswith('*') and bundle.startswith(app_id[:-1]))):p.error('Bundle ID does not match the profile')
print('Package/provisioning metadata checks passed. The device verifies Apple trust at installation.')
if a.check_only:raise SystemExit(0)
installer=shutil.which('ideviceinstaller')
if not installer:p.error('Install ideviceinstaller from your Linux distribution first; this script does not install system packages')
help_text=subprocess.run([installer,'--help'],capture_output=True,text=True).stdout
if '--install' in help_text:cmd=[installer,'--udid',a.udid,'--install',str(a.ipa.resolve())]
elif 'install' in help_text:cmd=[installer,'--udid',a.udid,'install',str(a.ipa.resolve())]
else:p.error('Unrecognized ideviceinstaller version')
subprocess.run(cmd,check=True)
