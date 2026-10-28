import sys, os, re, lz4.block, UnityPy

def blobs(d):
    out = []
    cb = bytes(d.compressedBlob)
    offs = d.offsets; clen = d.compressedLengths; dlen = d.decompressedLengths
    for p in range(len(d.platforms)):
        # 2022.3: offsets[p] is list of chunks
        o = offs[p]; c = clen[p]; u = dlen[p]
        if not isinstance(o, (list, tuple)):
            o, c, u = [o], [c], [u]
        data = b""
        for oo, cc, uu in zip(o, c, u):
            data += lz4.block.decompress(cb[oo:oo + cc], uncompressed_size=uu)
        out.append(data)
    return out

env = UnityPy.load(sys.argv[1])
outdir = sys.argv[2]
os.makedirs(outdir, exist_ok=True)
for obj in env.objects:
    if obj.type.name != "Shader":
        continue
    d = obj.read()
    name = d.m_ParsedForm.m_Name
    for i, b in enumerate(blobs(d)):
        fn = os.path.join(outdir, name.replace("/", "_") + f".p{i}.bin")
        open(fn, "wb").write(b)
        n = b.count(b"#include <metal_stdlib>")
        print(name, len(b), "msl sources:", n)
