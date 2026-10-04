"""Tests of packaging guards with synthetic fixtures; not real game IPAs."""
import pathlib,plistlib,subprocess,sys,tempfile,unittest,zipfile
ROOT=pathlib.Path(__file__).resolve().parents[1]
class PackageChecks(unittest.TestCase):
 def fixture(self,folder,platform='iPhoneOS',executable=True):
  path=pathlib.Path(folder)/'fixture.ipa'
  with zipfile.ZipFile(path,'w') as z:
   z.writestr('Payload/Test.app/Info.plist',plistlib.dumps({'CFBundleExecutable':'Test','CFBundleSupportedPlatforms':[platform],'UIDeviceFamily':[1,2],'UISupportedInterfaceOrientations':['UIInterfaceOrientationPortrait']}))
   if executable:z.writestr('Payload/Test.app/Test',b'synthetic executable placeholder')
  return path
 def verify(self,path,mode='unsigned'):
  return subprocess.run([sys.executable,str(ROOT/'ci/verify-ipa.py'),str(path),mode],capture_output=True)
 def test_unsigned_structure(self):
  with tempfile.TemporaryDirectory() as d:self.assertEqual(self.verify(self.fixture(d)).returncode,0)
 def test_simulator_rejected(self):
  with tempfile.TemporaryDirectory() as d:self.assertNotEqual(self.verify(self.fixture(d,platform='iPhoneSimulator')).returncode,0)
 def test_missing_executable_rejected(self):
  with tempfile.TemporaryDirectory() as d:self.assertNotEqual(self.verify(self.fixture(d,executable=False)).returncode,0)
 def test_unsigned_not_accepted_as_signed(self):
  with tempfile.TemporaryDirectory() as d:self.assertNotEqual(self.verify(self.fixture(d),'signed').returncode,0)
 def test_installer_rejects_unsigned_before_device_access(self):
  with tempfile.TemporaryDirectory() as d:
   result=subprocess.run([sys.executable,str(ROOT/'ci/install-signed-ipa.py'),'--ipa',str(self.fixture(d)),'--udid','synthetic-test-device','--check-only'],capture_output=True,text=True)
   self.assertNotEqual(result.returncode,0);self.assertIn('Unsigned IPA',result.stderr)
if __name__=='__main__':unittest.main()
