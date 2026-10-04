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
class ExportUnpackChecks(unittest.TestCase):
 def invoke(self,folder,name='Unity-iPhone.xcodeproj/project.pbxproj',mode=0):
  root=pathlib.Path(folder);archive=root/'export.zip';output=root/'unpacked'
  with zipfile.ZipFile(archive,'w') as z:
   item=zipfile.ZipInfo(name);item.external_attr=mode<<16
   z.writestr(item,b'synthetic export fixture')
  return subprocess.run([sys.executable,str(ROOT/'ci/unpack-ios-export.py'),str(root),'export.zip',str(output)],capture_output=True),output
 def test_expected_xcode_root(self):
  with tempfile.TemporaryDirectory() as d:
   result,output=self.invoke(d);self.assertEqual(result.returncode,0)
   self.assertTrue((output/'Unity-iPhone.xcodeproj/project.pbxproj').exists())
 def test_zip_parent_traversal_rejected(self):
  with tempfile.TemporaryDirectory() as d:
   result,_=self.invoke(d,'../escaped');self.assertNotEqual(result.returncode,0)
   self.assertFalse((pathlib.Path(d)/'escaped').exists())
 def test_symlink_rejected(self):
  with tempfile.TemporaryDirectory() as d:
   result,_=self.invoke(d,mode=0o120777);self.assertNotEqual(result.returncode,0)
 def test_outside_checkout_archive_rejected(self):
  with tempfile.TemporaryDirectory() as d:
   result=subprocess.run([sys.executable,str(ROOT/'ci/unpack-ios-export.py'),d,'../outside.zip',str(pathlib.Path(d)/'out')],capture_output=True)
   self.assertNotEqual(result.returncode,0);self.assertIn(b'escapes checkout',result.stderr)
if __name__=='__main__':unittest.main()
