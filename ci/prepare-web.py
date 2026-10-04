#!/usr/bin/env python3
"""Freeze the whole exported game in an atomic, versioned offline cache."""
import hashlib,json,pathlib,sys
root=pathlib.Path(sys.argv[1]).resolve()
worker=root/'sw.js'
source=(pathlib.Path(__file__).resolve().parents[1]/'Assets/WebGLTemplates/OfflineMerge/sw.js').read_text()
assert '__CACHE_VERSION__' in source and '__PRECACHE__' in source, 'Expected an unprocessed Unity web export'
files=sorted(p for p in root.rglob('*') if p.is_file() and p != worker and not p.name.endswith('.meta'))
assert (root/'index.html') in files and any(p.suffix=='.wasm' for p in files)
digest=hashlib.sha256(source.encode())
for file in files:
    digest.update(file.relative_to(root).as_posix().encode())
    digest.update(hashlib.sha256(file.read_bytes()).digest())
version=digest.hexdigest()[:20]
assets=['./'+p.relative_to(root).as_posix() for p in files]
worker.write_text(source.replace('__CACHE_VERSION__',version).replace('__PRECACHE__',json.dumps(assets)))
print(f'Offline bundle {version}: {len(files)} files, {sum(p.stat().st_size for p in files)/1024**2:.1f} MiB')
