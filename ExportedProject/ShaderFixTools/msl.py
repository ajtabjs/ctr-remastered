import sys, re
# usage: msl.py blob.bin  -> list sources with index, kind, length, keywords-ish
b = open(sys.argv[1], "rb").read()
srcs = []
i = 0
while True:
    j = b.find(b"#include <metal_stdlib>", i)
    if j < 0:
        break
    k = b.find(b"\x00", j)
    srcs.append(b[j:k].decode("utf-8", "replace"))
    i = k
if len(sys.argv) == 2:
    for n, s in enumerate(srcs):
        kind = "frag" if "fragment " in s else ("vert" if "vertex " in s else "?")
        h = hash(s) & 0xffff
        print(n, kind, len(s), h, re.findall(r"(?:constant|texture2d|sampler)\s*[^;]*?\b(_\w+)", s)[:12])
else:
    print(srcs[int(sys.argv[2])])
