import sys, json, UnityPy
env = UnityPy.load(sys.argv[1])
target = sys.argv[2]
def trim(o, depth=0, maxlist=3):
    if isinstance(o, dict):
        return {k: trim(v, depth + 1, maxlist) for k, v in o.items() if k not in ("compressedBlob",)}
    if isinstance(o, list):
        r = [trim(x, depth + 1, maxlist) for x in o[:maxlist]]
        if len(o) > maxlist:
            r.append(f"... ({len(o)} total)")
        return r
    if isinstance(o, (bytes, bytearray)):
        return f"<bytes {len(o)}>"
    return o
for obj in env.objects:
    if obj.type.name != "Shader":
        continue
    t = obj.read_typetree()
    pf = t["m_ParsedForm"]
    if pf["m_Name"] != target:
        continue
    print("TOP:", list(t.keys()))
    print("PF:", list(pf.keys()))
    print("keywords:", pf.get("m_KeywordNames"))
    print("flags:", pf.get("m_KeywordFlags"))
    ss = pf["m_SubShaders"][0]
    print("SS keys:", list(ss.keys()))
    p = ss["m_Passes"][int(sys.argv[3]) if len(sys.argv) > 3 else 0]
    for st in ("progVertex", "progFragment"):
        ps = p[st]["m_PlayerSubPrograms"]
        print(st, "player lists:", [len(x) for x in ps], "pbi:", [len(x) for x in p[st]["m_ParameterBlobIndices"]])
        for lst in ps:
            for sp in lst[:40]:
                print("   ", {k: v for k, v in sp.items()})
    print("nameIndices", p["m_NameIndices"])
    print("tags", p["m_Tags"], "ss tags", ss["m_Tags"])
    p = {k: v for k, v in p.items() if k not in ("m_State", "m_EditorDataHash")}
    print(json.dumps(trim(p, maxlist=int(sys.argv[4]) if len(sys.argv) > 4 else 3), indent=1, default=str)[:20000])
    break
