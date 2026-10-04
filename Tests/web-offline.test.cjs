const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
let checks=0;
function pass(label) { checks++; console.log('PASS '+label); }
const storage=new Map(), memory=new Map(), notices=[];
let library, failStorage=false, pointer=0;
vm.runInNewContext(fs.readFileSync('Assets/Plugins/WebGL/LocalMergeSave.jslib','utf8'),{
  LibraryManager:{library:{}}, mergeInto:(target,value)=>{library=value;},
  UTF8ToString:value=>value, lengthBytesUTF8:Buffer.byteLength,
  _malloc:()=>++pointer, stringToUTF8:(value,target)=>memory.set(target,value),
  localStorage:{getItem:key=>storage.has(key)?storage.get(key):null,setItem:(key,value)=>{if(failStorage)throw Error('quota');storage.set(key,value);}},
  CustomEvent:class{constructor(type,options){this.type=type;this.detail=options.detail;}},
  window:{dispatchEvent:event=>notices.push(event.detail.ok)}
});
assert.equal(library.LocalMergeReadSave('grid_data.json'),0); pass('missing web save remains absent');
library.LocalMergeWriteSave('grid_data.json','{"grid":[1]}');
library.LocalMergeWriteSave('grid_data.json','{"grid":[2]}');
assert.equal(memory.get(library.LocalMergeReadSave('grid_data.json')),'{"grid":[2]}'); pass('rapid writes restore the latest complete board');
library.LocalMergeWriteSave('CompletedTasks','');
assert.equal(memory.get(library.LocalMergeReadSave('CompletedTasks')),''); pass('empty completed-order save differs from a missing save');
failStorage=true; library.LocalMergeWriteSave('inventory.json','{}');
assert.equal(notices.at(-1),false); pass('storage failure is reported instead of claiming a durable save');

async function worker(failDownload=false) {
  const handlers={}, stores=new Map(), scope='https://example.invalid/game/';
  let fetched=0, offline=false, claimed=false, skipped=false;
  stores.set('merge-sandbox:/game/:old',new Map()); stores.set('unrelated-app',new Map());
  stores.set('merge-sandbox:/other-game/:old',new Map());
  const key=value=>typeof value==='string'?value:value.url;
  const cache=name=>({put:async(url,response)=>stores.get(name).set(key(url),response.clone()),
    match:async(request,options)=>{
      let url=key(request); if(options?.ignoreSearch)url=url.split('?')[0];
      return stores.get(name).get(url)?.clone();
    }});
  const self={location:{origin:'https://example.invalid'},registration:{scope},
    addEventListener:(name,handler)=>handlers[name]=handler,
    skipWaiting:async()=>{skipped=true;},
    clients:{matchAll:async()=>[],claim:async()=>{claimed=true;}}};
  const source=fs.readFileSync('Assets/WebGLTemplates/OfflineMerge/sw.js','utf8')
    .replace('__CACHE_VERSION__','fixture').replace('__PRECACHE__',JSON.stringify(['./index.html','./Build/game.wasm']));
  vm.runInNewContext(source,{self,URL,Request,
    caches:{open:async name=>{if(!stores.has(name))stores.set(name,new Map());return cache(name);},keys:async()=>[...stores.keys()],delete:async name=>stores.delete(name)},
    fetch:async request=>{fetched++;if(offline)throw Error('offline');return new Response('fixture',{status:failDownload&&key(request).endsWith('.wasm')?503:200});}
  });
  let pending; handlers.install({waitUntil:value=>pending=value});
  if(failDownload) {
    await assert.rejects(pending);
    assert.ok(stores.has('merge-sandbox:/game/:old')); assert.ok(!stores.has('merge-sandbox:/game/:fixture'));
    pass('incomplete download discards only the new cache and preserves the old game'); return;
  }
  await pending; assert.equal(stores.get('merge-sandbox:/game/:fixture').size,2); pass('all game assets are cached before installation succeeds');
  assert.equal(skipped,false); pass('updates do not replace resources underneath a running game');
  handlers.activate({waitUntil:value=>pending=value}); await pending;
  assert.ok(claimed&&stores.has('unrelated-app')&&stores.has('merge-sandbox:/other-game/:old')&&!stores.has('merge-sandbox:/game/:old')); pass('activation preserves unrelated site and other game-scope caches');
  offline=true; const before=fetched;
  let response;
  handlers.fetch({request:{method:'GET',url:scope+'Build/game.wasm',mode:'cors'},respondWith:value=>response=value});
  assert.equal(await (await response).text(),'fixture'); assert.equal(fetched,before); pass('cached game binary loads without any network request');
  handlers.fetch({request:{method:'GET',url:scope+'?homescreen',mode:'navigate'},respondWith:value=>response=value});
  assert.equal(await (await response).text(),'fixture'); assert.equal(fetched,before); pass('home-screen navigation works offline with query parameters');
}
(async()=>{await worker();await worker(true);console.log('TOTAL '+checks+' web persistence/cache checks PASS');})().catch(error=>{console.error(error);process.exitCode=1;});
