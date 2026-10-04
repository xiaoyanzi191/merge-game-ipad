#!/usr/bin/env python3
"""Unpack an explicitly selected export without escaping the build workspace."""
import pathlib,stat,sys,zipfile
checkout=pathlib.Path(sys.argv[1]).resolve()
archive=(checkout/sys.argv[2]).resolve()
if not archive.is_relative_to(checkout):raise ValueError('Export path escapes checkout')
destination=pathlib.Path(sys.argv[3]).resolve()
with zipfile.ZipFile(archive) as z:
 for entry in z.infolist():
  name=pathlib.PurePosixPath(entry.filename)
  if name.is_absolute() or '..' in name.parts or '\\' in entry.filename:
   raise ValueError('Unsafe ZIP path')
  mode=entry.external_attr>>16
  if stat.S_ISLNK(mode):raise ValueError('Export must contain copied files, not symlinks')
  output=destination.joinpath(*name.parts)
  if not output.resolve().is_relative_to(destination):raise ValueError('ZIP path escapes output')
  if entry.is_dir():output.mkdir(parents=True,exist_ok=True);continue
  output.parent.mkdir(parents=True,exist_ok=True)
  with z.open(entry) as src,output.open('wb') as dst:
   while chunk:=src.read(1024*1024):dst.write(chunk)
  if mode:output.chmod(mode&0o777)
if not (destination/'Unity-iPhone.xcodeproj/project.pbxproj').is_file():
 raise ValueError('Expected Unity-iPhone.xcodeproj at the ZIP root')
print('Reviewed iOS export unpacked; Unity is not required on the Xcode runner')
