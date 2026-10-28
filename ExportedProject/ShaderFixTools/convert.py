"""Replace AssetRipper dummy shaders in the project with HLSL reconstructed from the game's Metal programs.

usage: convert.py <project root> <cache dir> [--write] [--only SUBSTR]
"""
import sys, os, re, pickle, glob, traceback
sys.path.insert(0, os.path.dirname(__file__))
from build_shader import build_shader
from msl2hlsl import TranslateError

root, cache = sys.argv[1], sys.argv[2]
write = "--write" in sys.argv
only = sys.argv[sys.argv.index("--only") + 1] if "--only" in sys.argv else None

# ---- material keywords
allowed = set()
for mp in glob.glob(os.path.join(root, "Assets", "**", "*.mat"), recursive=True):
    txt = open(mp, encoding="utf-8", errors="replace").read()
    for key in ("m_ValidKeywords", "m_InvalidKeywords"):
        m = re.search(r"%s:(?: \[\])?\n((?:  - .*\n)*)" % key, txt)
        if m:
            allowed |= {x.strip()[2:].strip() for x in m.group(1).splitlines() if x.strip()}
    m = re.search(r"m_ShaderKeywords: (.*)", txt)
    if m:
        allowed |= set(m.group(1).split())
print("material keywords:", len(allowed))

# ---- cache index
by_name = {}
for line in open(os.path.join(cache, "index.tsv")):
    parts = line.rstrip("\n").split("\t")
    if len(parts) >= 5 and parts[4] == "new":
        by_name.setdefault(parts[0], []).append(parts[1])

_loaded = {}


def load(h):
    if h not in _loaded:
        with open(os.path.join(cache, h + ".pkl"), "rb") as f:
            _loaded[h] = pickle.load(f)
    return _loaded[h]


def n_subprograms(e):
    n = 0
    for ss in e["parsed"]["m_SubShaders"]:
        for p in ss["m_Passes"]:
            for st in ("progVertex", "progFragment"):
                for lst in p[st].get("m_PlayerSubPrograms") or []:
                    n += len(lst)
    return n


def prop_names_from_entry(e):
    return {p["m_Name"] for p in e["parsed"]["m_PropInfo"]["m_Props"]}


def extract_block(txt, start_kw):
    i = txt.find(start_kw)
    if i < 0:
        return None
    j = txt.index("{", i)
    depth = 0
    k = j
    while k < len(txt):
        if txt[k] == "{":
            depth += 1
        elif txt[k] == "}":
            depth -= 1
            if depth == 0:
                break
        k += 1
    # include leading whitespace of the line
    ls = txt.rfind("\n", 0, i) + 1
    return txt[ls:k + 1]


files = sorted(glob.glob(os.path.join(root, "Assets", "**", "*.shader"), recursive=True))
report = []
stats = {"ok": 0, "partial": 0, "failed": 0, "skipped": 0}
for fp in files:
    rel = os.path.relpath(fp, root)
    if only and only not in rel:
        continue
    txt = open(fp, encoding="utf-8", errors="replace").read()
    if "//DummyShaderTextExporter" not in txt:
        report.append("SKIP (not a dummy) " + rel)
        stats["skipped"] += 1
        continue
    m = re.match(r'\s*Shader\s+"([^"]+)"', txt)
    name = m.group(1)
    props = extract_block(txt, "Properties {") or "\tProperties {\n\t}"
    props_in_file = set(re.findall(r"^\s*(?:\[[^\]]*\]\s*)*(\w+)\s*\(\"", props, re.M))
    fb = re.search(r"^\s*Fallback\s+.*$", txt, re.M)
    ce = re.search(r"^\s*//\s*CustomEditor\s+.*$", txt, re.M)
    cands = by_name.get(name, [])
    if not cands:
        report.append("FAILED (no source in build) %s [%s]" % (rel, name))
        stats["failed"] += 1
        continue
    best = max(cands, key=lambda h: (len(prop_names_from_entry(load(h)) & props_in_file) -
                                     len(prop_names_from_entry(load(h)) ^ props_in_file), n_subprograms(load(h))))
    log = []
    try:
        out, ok = build_shader(load(best), props, fb.group(0).strip() if fb else None,
                               ce.group(0).strip() if ce else None, allowed, log)
    except TranslateError as e:
        report.append("FAILED %s [%s]: %s" % (rel, name, e))
        stats["failed"] += 1
        continue
    except Exception as e:
        report.append("FAILED %s [%s]: %s" % (rel, name, traceback.format_exc().splitlines()[-1]))
        stats["failed"] += 1
        continue
    status = "OK" if ok and not any("FAILED" in l or "placeholder" in l for l in log) else ("PARTIAL" if ok else "FAILED")
    stats[{"OK": "ok", "PARTIAL": "partial", "FAILED": "failed"}[status]] += 1
    report.append("%s %s [%s] src=%s" % (status, rel, name, best))
    for l in log:
        report.append("    " + l)
    if write and ok:
        with open(fp, "w", encoding="utf-8", newline="\n") as f:
            f.write(out)
    elif not write:
        os.makedirs("/tmp/shaderfix_out", exist_ok=True)
        with open(os.path.join("/tmp/shaderfix_out", rel.replace("/", "__")), "w") as f:
            f.write(out)

open(os.path.join(os.path.dirname(__file__), "report.txt"), "w").write("\n".join(report) + "\n")
print(stats)
