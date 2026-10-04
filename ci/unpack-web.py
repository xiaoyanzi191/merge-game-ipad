#!/usr/bin/env python3
"""Unpack the reviewed static game without accepting links or escaping paths."""
import pathlib,stat,sys,zipfile
archive=pathlib.Path(sys.argv[1]); target=pathlib.Path(sys.argv[2]).resolve()
with zipfile.ZipFile(archive) as source:
    entries=source.infolist()
    names=[entry.filename for entry in entries]
    assert len(names)==len(set(names)), 'Duplicate archive paths'
    assert {'index.html','sw.js','manifest.webmanifest'} <= set(names), 'Incomplete web export'
    assert any(name.startswith('Build/') and name.endswith('.wasm') for name in names)
    for entry in entries:
        path=pathlib.PurePosixPath(entry.filename)
        assert not path.is_absolute() and '..' not in path.parts and '\\' not in entry.filename, 'Unsafe archive path'
        assert not stat.S_ISLNK(entry.external_attr>>16), 'Archive links are not allowed'
        assert path.suffix.lower() not in {'.ulf','.p12','.pfx','.key','.mobileprovision'}, 'Credential material is not web content'
    assert source.testzip() is None, 'Corrupt archive'
    for entry in entries:
        if entry.is_dir(): continue
        output=target/entry.filename
        output.parent.mkdir(parents=True,exist_ok=True)
        output.write_bytes(source.read(entry))
print('Reviewed static web archive unpacked')
