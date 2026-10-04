#!/usr/bin/env python3
import pathlib,sys,zipfile,plistlib
path=pathlib.Path(sys.argv[1]); mode=sys.argv[2]
with zipfile.ZipFile(path) as archive:
 names=archive.namelist()
 infos=[n for n in names if n.startswith('Payload/') and n.count('/')==2 and n.endswith('/Info.plist')]
 assert len(infos)==1, 'Expected exactly one iOS application'
 prefix=infos[0].rsplit('/',1)[0]+'/'
 info=plistlib.loads(archive.read(infos[0]))
 assert prefix+info['CFBundleExecutable'] in names, 'Executable missing'
 assert info.get('CFBundleSupportedPlatforms')==['iPhoneOS'], 'Not a device build'
 assert sorted(info['UIDeviceFamily'])==[1,2], 'Expected iPhone and iPad support'
 assert info['UISupportedInterfaceOrientations']==['UIInterfaceOrientationPortrait']
 if mode=='signed':
  assert prefix+'embedded.mobileprovision' in names, 'Missing provisioning profile'
  assert any(n.startswith(prefix+'_CodeSignature/') for n in names), 'Missing app code signature'
 print(f'IPA structure PASS ({mode}); device installation has not been tested')
