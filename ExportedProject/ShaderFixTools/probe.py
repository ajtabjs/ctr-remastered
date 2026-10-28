import sys, UnityPy
env = UnityPy.load(sys.argv[1])
for obj in env.objects:
    if obj.type.name != "Shader":
        continue
    d = obj.read()
    pf = d.m_ParsedForm
    print("SHADER", pf.m_Name, "platforms:", list(d.platforms))
    for si, ss in enumerate(pf.m_SubShaders):
        for pi, p in enumerate(ss.m_Passes):
            print("  ss", si, "pass", pi, p.m_Name if hasattr(p, "m_Name") else "", p.m_State.m_Name)
    if len(sys.argv) > 2:
        from UnityPy.export import ShaderConverter
        txt = ShaderConverter.export_shader(d)
        open(sys.argv[2] + "/" + pf.m_Name.replace("/", "_") + ".txt", "w").write(txt)
