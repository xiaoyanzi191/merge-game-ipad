#!/usr/bin/env python3
"""Repository/source integrity checks. Not a substitute for Unity import/build."""
from pathlib import Path
import json,re,subprocess,xml.etree.ElementTree as ET
root=Path(__file__).resolve().parents[1]
checks=0

def check(condition,label):
 global checks
 assert condition,label
 checks+=1
 print('PASS',label)

for directory in ['Assets','Packages','ProjectSettings']:
 check((root/directory).is_dir(),f'Unity project directory: {directory}')
version=(root/'ProjectSettings/ProjectVersion.txt').read_text()
check('2022.3.62f3' in version and '96770f904ca7' in version,'Unity editor version and official revision pinned')
manifest=json.loads((root/'Packages/manifest.json').read_text())
lock=json.loads((root/'Packages/packages-lock.json').read_text())
uni=lock['dependencies']['com.cysharp.unitask']
check(manifest['dependencies']['com.cysharp.unitask']==uni['version'] and uni['version'].endswith('#'+uni['hash']),'UniTask manifest matches original lock commit')
scenes=re.findall(r'path: (Assets/Scenes/[^\n]+)',(root/'ProjectSettings/EditorBuildSettings.asset').read_text())
check(scenes==['Assets/Scenes/LoadScene.unity','Assets/Scenes/MainScene.unity','Assets/Scenes/MergeScene.unity'] and all((root/p).exists() for p in scenes),'bootstrap/main/merge scenes exist in build order')
guids={}
for p in (root/'Assets').rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(),re.M)
 if m:
  check(m[1] not in guids,f'unique GUID: {p.relative_to(root)}') if m[1] in guids else None
  guids[m[1]]=p
check(True,f'{len(guids)} unique Unity asset GUIDs')
new_scripts=[p for p in (root/'Assets/Scripts').rglob('*.cs') if not Path(str(p)+'.meta').exists()]
check(not new_scripts,'all scripts have committed .meta files')
producer=(root/'Assets/Scripts/Core/GridPawns/Producer.cs').read_text()
check('get => int.MaxValue' in producer and 'StartCoroutine' not in producer and 'WaitForSeconds' not in producer,'producer capacity fixed, no recovery coroutine')
merge=(root/'Assets/Scripts/MVP/Presenters/MergePresenter.cs').read_text()
check('ReplaceProducer(' not in merge and 'ShouldDestroy(' not in merge,'production does not recycle producer; highest level survives selection')
settings=(root/'ProjectSettings/ProjectSettings.asset').read_text()
check('defaultScreenOrientation: 1' in settings and 'iPhoneSdkVersion: 988' in settings and 'iOSTargetOSVersionString: 15.0' in settings,'portrait + iOS device SDK + iOS 15 minimum')
link=ET.parse(root/'Assets/link.xml')
check({a.attrib['fullname'] for a in link.findall('assembly')}=={'Assembly-CSharp','Game.DI'},'IL2CPP reflection constructors preserved')
assets=list((root/'Assets/Resources/Data/ApplianceData/ApplianceALevels').glob('*.asset'))
check(len(assets)==11,'all 11 existing appliance levels retained')
producer_assets=list((root/'Assets/Resources/Data/ProducerData/Producers').glob('*.asset'))
check(len(producer_assets)==1,'existing ProducerA series retained (no nonexistent producers promised)')
orders=list((root/'Assets/Resources/Tasks').glob('*.asset'))
ids=[]
for p in orders:
 text=p.read_text();ids+=list(map(int,re.findall(r'<TaskID>k__BackingField: (\d+)',text)))
 for n in re.findall(r'<Level>k__BackingField: (\d+)',text): check(1<=int(n)<=11,f'valid order goal: {p.name} level {n}')
check(sorted(ids)==list(range(1,13)),'all 12 original orders retained')
for file in ['ci/install-unity-macos.sh','ci/prepare-signing.sh','ci/package-ios.sh']:
 subprocess.run(['bash','-n',str(root/file)],check=True)
check(True,'three build shell scripts pass bash syntax checks')
check((root/'LICENSE').read_text().startswith('MIT License'),'upstream MIT license retained')
print(f'TOTAL {checks} repository checks PASS; Unity import, player rendering and iOS archive NOT verified here.')
