"""Collect every Shader object from the game build into a pickle cache.

usage: collect.py <game Data dir> <cache dir>
Each cache entry: {name, source, path_id, parsed (typetree of m_ParsedForm), platforms,
                   blobs: {platform_index: {"segments": [bytes...]}} }
"""
import sys, os, hashlib, pickle, traceback
import lz4.block
import UnityPy

data_dir, cache = sys.argv[1], sys.argv[2]
os.makedirs(cache, exist_ok=True)
index_path = os.path.join(cache, "index.tsv")
seen = set()
if os.path.exists(index_path):
    for line in open(index_path):
        seen.add(line.split("\t")[1])
idx = open(index_path, "a")


def decompress(t):
    cb = bytes(t["compressedBlob"])
    res = []
    for p in range(len(t["platforms"])):
        o, c, u = t["offsets"][p], t["compressedLengths"][p], t["decompressedLengths"][p]
        if not isinstance(o, list):
            o, c, u = [o], [c], [u]
        segs = []
        for oo, cc, uu in zip(o, c, u):
            segs.append(lz4.block.decompress(cb[oo:oo + cc], uncompressed_size=uu) if cc else b"")
        res.append(segs)
    return res


files = []
for root, dirs, fs in os.walk(data_dir):
    for f in fs:
        if f.endswith((".manifest", ".resS", ".resource", ".json", ".info", ".config", ".dat")) or "il2cpp_data" in root:
            continue
        files.append(os.path.join(root, f))
files.sort()
print("files:", len(files), flush=True)
count = 0
for fi, fp in enumerate(files):
    try:
        env = UnityPy.load(fp)
    except Exception as e:
        continue
    try:
        objs = list(env.objects)
    except Exception:
        continue
    for obj in objs:
        try:
            if obj.type.name != "Shader":
                continue
            t = obj.read_typetree()
            pf = t["m_ParsedForm"]
            h = hashlib.sha1(bytes(t["compressedBlob"]) + pf["m_Name"].encode()).hexdigest()[:16]
            if h in seen:
                idx.write(f"{pf['m_Name']}\t{h}\t{os.path.relpath(fp, data_dir)}\t{obj.path_id}\tdup\n")
                continue
            seen.add(h)
            entry = {
                "name": pf["m_Name"],
                "source": os.path.relpath(fp, data_dir),
                "path_id": obj.path_id,
                "parsed": pf,
                "platforms": list(t["platforms"]),
                "blobs": decompress(t),
            }
            with open(os.path.join(cache, h + ".pkl"), "wb") as f:
                pickle.dump(entry, f)
            idx.write(f"{pf['m_Name']}\t{h}\t{os.path.relpath(fp, data_dir)}\t{obj.path_id}\tnew\n")
            idx.flush()
            count += 1
        except Exception:
            traceback.print_exc()
    if fi % 50 == 0:
        print(f"{fi}/{len(files)} unique shaders so far: {count}", flush=True)
print("done unique:", count)
