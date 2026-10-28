"""Rebuild a ShaderLab .shader file from a cached game Shader object (Metal platform)."""
import re, struct
from msl2hlsl import translate, TranslateError

METAL = 14
VERT_TYPES = {23}       # kShaderGpuProgramMetalVS
FRAG_TYPES = {24}       # kShaderGpuProgramMetalFS

# keywords that are driven by the engine / pipeline and should be treated as "off"
GLOBAL_KW_PREFIX = (
    "STEREO_", "UNITY_SINGLE_PASS_STEREO", "INSTANCING_ON", "DOTS_INSTANCING_ON", "PROCEDURAL_INSTANCING_ON",
    "LIGHTMAP_ON", "DIRLIGHTMAP_COMBINED", "DYNAMICLIGHTMAP_ON", "LIGHTMAP_SHADOW_MIXING", "SHADOWS_SHADOWMASK",
    "_SAMPLE_GI", "FOG_", "_DBUFFER_", "DEBUG_DISPLAY", "_SCREEN_SPACE_OCCLUSION", "_WRITE_RENDERING_LAYERS",
    "_GBUFFER_NORMALS_OCT", "_CASTING_PUNCTUAL_LIGHT_SHADOW", "LOD_FADE_CROSSFADE", "_MAIN_LIGHT_SHADOWS",
    "_ADDITIONAL_LIGHTS", "_ADDITIONAL_LIGHT_SHADOWS", "_SHADOWS_SOFT", "_LIGHT_LAYERS", "_LIGHT_COOKIES",
    "_FORWARD_PLUS", "_REFLECTION_PROBE_", "_MIXED_LIGHTING_SUBTRACTIVE", "EVALUATE_SH_", "_CLUSTERED_RENDERING",
    "_RENDER_PASS_ENABLED", "_USE_DRAW_PROCEDURAL", "_DEFERRED_", "_SPOT", "_POINT", "_DIRECTIONAL",
    "SHADOWS_", "UNITY_HDR_ON", "_LIGHT_COOKIE", "_ENABLE_", "VERTEXLIGHT_ON", "DIRECTIONAL", "POINT", "SPOT",
    "LIGHTPROBE_SH", "UNITY_UI_ALPHACLIP", "_USE_FAST_SRGB_LINEAR_CONVERSION", "_LINEAR_TO_SRGB_CONVERSION",
    "_FOVEATED_RENDERING", "_SURFACE_TYPE_TRANSPARENT_SHADOWS",
)
GLOBAL_KW_EXACT = {"POINT", "SPOT", "DIRECTIONAL", "POINT_COOKIE", "DIRECTIONAL_COOKIE"}

MAX_BRANCHES = 48

CULL = {0: "Off", 1: "Front", 2: "Back"}
CMP = {0: None, 1: "Never", 2: "Less", 3: "Equal", 4: "LEqual", 5: "Greater", 6: "NotEqual", 7: "GEqual", 8: "Always"}
BLEND = {0: "Zero", 1: "One", 2: "DstColor", 3: "SrcColor", 4: "OneMinusDstColor", 5: "SrcAlpha",
         6: "OneMinusSrcColor", 7: "DstAlpha", 8: "OneMinusDstAlpha", 9: "SrcAlphaSaturate", 10: "OneMinusSrcAlpha"}
BLENDOP = {0: "Add", 1: "Sub", 2: "RevSub", 3: "Min", 4: "Max", 5: "LogicalClear", 6: "LogicalSet",
           7: "LogicalCopy", 8: "LogicalCopyInverted", 9: "LogicalNoop", 10: "LogicalInvert", 11: "LogicalAnd",
           12: "LogicalNand", 13: "LogicalOr", 14: "LogicalNor", 15: "LogicalXor", 16: "LogicalEquiv",
           17: "LogicalAndReverse", 18: "LogicalAndInverted", 19: "LogicalOrReverse", 20: "LogicalOrInverted",
           21: "Multiply", 22: "Screen", 23: "Overlay", 24: "Darken", 25: "Lighten", 26: "ColorDodge",
           27: "ColorBurn", 28: "HardLight", 29: "SoftLight", 30: "Difference", 31: "Exclusion",
           32: "HSLHue", 33: "HSLSaturation", 34: "HSLColor", 35: "HSLLuminosity"}
STENCILOP = {0: "Keep", 1: "Zero", 2: "Replace", 3: "IncrSat", 4: "DecrSat", 5: "Invert", 6: "IncrWrap", 7: "DecrWrap"}


def is_global_kw(k):
    return k in GLOBAL_KW_EXACT or any(k.startswith(p) for p in GLOBAL_KW_PREFIX)


# --------------------------------------------------------------------------- blob access

class BlobReader:
    def __init__(self, segments):
        self.segments = segments
        s0 = segments[0]
        n = struct.unpack_from("<I", s0, 0)[0]
        self.entries = []
        for i in range(n):
            off, ln, seg = struct.unpack_from("<III", s0, 4 + i * 12)
            self.entries.append((off, ln, seg))
        self._cache = {}

    def msl(self, idx):
        if idx in self._cache:
            return self._cache[idx]
        off, ln, seg = self.entries[idx]
        data = self.segments[seg][off:off + ln]
        j = data.find(b"#include <metal_stdlib>")
        if j < 0:
            src = None
        else:
            k = data.find(b"\x00", j)
            if k < 0:
                k = len(data)
            src = data[j:k].decode("utf-8", "replace")
        self._cache[idx] = src
        return src


# --------------------------------------------------------------------------- state

def sv(x):
    """serialized float value (val, name) -> (number, propname or None)"""
    name = x.get("name", "<noninit>")
    if name and name != "<noninit>":
        return None, name
    return x["val"], None


def fmt_enum(x, table):
    v, n = sv(x)
    if n:
        return "[%s]" % n
    return table.get(int(v))


def fmt_num(x):
    v, n = sv(x)
    if n:
        return "[%s]" % n
    if float(v).is_integer():
        return str(int(v))
    return repr(float(v))


def state_lines(stt, ind):
    L = []
    c = fmt_enum(stt["culling"], CULL)
    L.append("Cull " + (c or "Back"))
    zw_v, zw_n = sv(stt["zWrite"])
    L.append("ZWrite " + ("[%s]" % zw_n if zw_n else ("On" if zw_v else "Off")))
    zt = fmt_enum(stt["zTest"], CMP)
    if zt:
        L.append("ZTest " + zt)
    zc_v, zc_n = sv(stt.get("zClip", {"val": 1.0}))
    if zc_n:
        L.append("ZClip [%s]" % zc_n)
    elif not zc_v:
        L.append("ZClip False")
    sep = stt.get("rtSeparateBlend", False)
    nrt = 8 if sep else 1
    for i in range(nrt):
        b = stt["rtBlend%d" % i]
        sfx = (" %d" % i) if sep else ""
        src, dst = fmt_enum(b["srcBlend"], BLEND), fmt_enum(b["destBlend"], BLEND)
        srca, dsta = fmt_enum(b["srcBlendAlpha"], BLEND), fmt_enum(b["destBlendAlpha"], BLEND)
        if (src, dst, srca, dsta) == ("One", "Zero", "One", "Zero"):
            if sep:
                L.append("Blend%s Off" % sfx)
        elif (srca, dsta) == (src, dst):
            L.append("Blend%s %s %s" % (sfx, src, dst))
        else:
            L.append("Blend%s %s %s, %s %s" % (sfx, src, dst, srca, dsta))
        op, opa = fmt_enum(b["blendOp"], BLENDOP), fmt_enum(b["blendOpAlpha"], BLENDOP)
        if op != "Add" or opa != "Add":
            L.append("BlendOp%s %s%s" % (sfx, op, (", " + opa) if opa != op else ""))
        cmv, cmn = sv(b["colMask"])
        if cmn:
            L.append("ColorMask [%s]%s" % (cmn, sfx))
        else:
            m = int(cmv)
            if m != 15:
                s = "".join(ch for bit, ch in ((8, "R"), (4, "G"), (2, "B"), (1, "A")) if m & bit) or "0"
                L.append("ColorMask %s%s" % (s, sfx))
    of, ou = stt["offsetFactor"], stt["offsetUnits"]
    if sv(of) != (0.0, None) or sv(ou) != (0.0, None):
        L.append("Offset %s, %s" % (fmt_num(of), fmt_num(ou)))
    a2m_v, a2m_n = sv(stt["alphaToMask"])
    if a2m_n:
        L.append("AlphaToMask [%s]" % a2m_n)
    elif a2m_v:
        L.append("AlphaToMask On")
    # stencil
    ref, rm, wm = stt["stencilRef"], stt["stencilReadMask"], stt["stencilWriteMask"]
    sf, sb = stt["stencilOpFront"], stt["stencilOpBack"]

    def op_default(o):
        return all(sv(o[k]) == (0.0, None) for k in ("pass", "fail", "zFail")) and sv(o["comp"]) == (8.0, None)

    stencil_used = not (op_default(sf) and op_default(sb) and op_default(stt["stencilOp"])) or \
        sv(ref)[1] or sv(rm)[1] or sv(wm)[1]
    if stencil_used:
        S = ["Stencil {", "    Ref " + fmt_num(ref), "    ReadMask " + fmt_num(rm), "    WriteMask " + fmt_num(wm)]
        same = all(sv(sf[k]) == sv(sb[k]) for k in ("pass", "fail", "zFail", "comp"))
        if same:
            S += ["    Comp " + (fmt_enum(sf["comp"], CMP) or "Always"), "    Pass " + fmt_enum(sf["pass"], STENCILOP),
                  "    Fail " + fmt_enum(sf["fail"], STENCILOP), "    ZFail " + fmt_enum(sf["zFail"], STENCILOP)]
        else:
            for nm, o in (("Front", sf), ("Back", sb)):
                S += ["    Comp%s %s" % (nm, fmt_enum(o["comp"], CMP) or "Always"),
                      "    Pass%s %s" % (nm, fmt_enum(o["pass"], STENCILOP)),
                      "    Fail%s %s" % (nm, fmt_enum(o["fail"], STENCILOP)),
                      "    ZFail%s %s" % (nm, fmt_enum(o["zFail"], STENCILOP))]
        S.append("}")
        L += S
    return [ind + x for x in L]


def tags_str(tags):
    if not tags:
        return ""
    items = tags if isinstance(tags, list) else list(tags.items())
    return "Tags { " + " ".join('"%s" = "%s"' % (k, v) for k, v in items) + " }"


def get_tags(t):
    if t is None:
        return []
    if isinstance(t, dict):
        t = t.get("tags", t)
    if isinstance(t, dict):
        return list(t.items())
    return [tuple(x) for x in t]


# --------------------------------------------------------------------------- programs

def subprograms(prog, types):
    lists = prog.get("m_PlayerSubPrograms") or []
    out = []
    for lst in lists:
        if lst:
            out = [sp for sp in lst if sp["m_GpuProgramType"] in types]
            if out:
                break
    if not out:
        out = [sp for sp in (prog.get("m_SubPrograms") or []) if sp["m_GpuProgramType"] in types]
    return out


def choose(sps, S, kwnames):
    """pick subprogram with keyword set == S restricted to keywords that this stage knows; else best subset."""
    known = set()
    for sp in sps:
        known |= {kwnames[i] for i in sp["m_KeywordIndices"]}
    want = frozenset(S & known)
    best, bestlen = None, -1
    for sp in sps:
        ks = frozenset(kwnames[i] for i in sp["m_KeywordIndices"])
        if ks == want:
            return sp
        if ks <= want and not any(is_global_kw(k) for k in ks) and len(ks) > bestlen:
            best, bestlen = sp, len(ks)
    return best


def emit_struct(name, members):
    L = ["struct %s" % name, "{"]
    for m in members:
        L.append("    " + m + ";")
    L.append("};")
    return "\n".join(L)


def build_variant(vsrc, fsrc, tag):
    vs = translate(vsrc, "vertex", "v%s_" % tag)
    fs = translate(fsrc, "fragment", "f%s_" % tag)
    # varyings
    vout = []
    k = 0
    vnames = set()
    for t, n, sem, interp in vs.vout:
        if sem is None:
            sem = "TEXCOORD%d" % k
            k += 1
        vout.append("%s%s %s : %s" % (interp, t, n, sem))
        vnames.add(n)
    for n in fs.frag_inputs:
        if n not in vnames:
            raise TranslateError("fragment input %s not produced by vertex" % n)
    if fs.frag_coord and "mtl_Position" not in vnames:
        raise TranslateError("frag coord without position")
    vin = ["%s %s : %s" % (t, n, sem) for t, n, sem in vs.vin]
    parts = []
    parts.append(emit_struct("VIn", vin) if vin else "struct VIn { float4 dummy : POSITION; };")
    parts.append(emit_struct("V2F", vout))
    if fs.ret != "void":
        parts.append(emit_struct("FOut", ["%s %s : %s" % f for f in fs.fout]))
    parts += vs.statics + fs.statics
    vparams = ["VIn input"] + vs.params
    parts.append("V2F vert(%s)\n{%s}" % (", ".join(vparams), vs.body))
    fparams = ["V2F input"] + fs.params
    parts.append("%s frag(%s)\n{%s}" % ("FOut" if fs.ret != "void" else "void", ", ".join(fparams), fs.body))
    decls = {}
    for d in (vs.uniforms, fs.uniforms, vs.textures, fs.textures, vs.samplers, fs.samplers):
        for kname, v in d.items():
            decls.setdefault(kname, v)
    return "\n".join(parts), decls, vs.uses_tex_dims or fs.uses_tex_dims, vs.warn + fs.warn


HEADER_HELPERS = """#define HLSLCC_COL(m, i) (transpose(m)[(i)])
#define HLSLCC_COLA(m, i, c) (transpose(m[(i) / (c)])[(i) % (c)])"""

TEXDIM_HELPERS = """float _hlslcc_tex_w(Texture2D t, uint lod) { uint w, h, l; t.GetDimensions(lod, w, h, l); return w; }
float _hlslcc_tex_h(Texture2D t, uint lod) { uint w, h, l; t.GetDimensions(lod, w, h, l); return h; }"""


def build_pass(p, pf, blob, allowed_kw, use_multi_compile, log, ind):
    kwnames = pf["m_KeywordNames"]
    stt = p["m_State"]
    L = []
    ptype = p.get("m_Type", 0)
    if ptype == 1:
        return [ind + 'UsePass "%s"' % p["m_UseName"]], True
    if ptype == 2:
        tn = p.get("m_TextureName") or ""
        return [ind + ("GrabPass { \"%s\" }" % tn if tn else "GrabPass { }")], True
    L.append(ind + "Pass {")
    i2 = ind + "    "
    if stt.get("m_Name"):
        L.append(i2 + 'Name "%s"' % stt["m_Name"])
    tg = get_tags(stt.get("m_Tags"))
    if tg:
        L.append(i2 + tags_str(tg))
    if stt.get("m_LOD"):
        L.append(i2 + "LOD %d" % stt["m_LOD"])
    L += state_lines(stt, i2)

    vsps = subprograms(p["progVertex"], VERT_TYPES)
    fsps = subprograms(p["progFragment"], FRAG_TYPES)
    if not vsps or not fsps:
        log.append("pass '%s': no Metal vertex/fragment programs (placeholder emitted)" % stt.get("m_Name"))
        L.append(i2 + "// no program data found in build")
        L.append(i2 + "HLSLPROGRAM")
        L.append(PLACEHOLDER)
        L.append(i2 + "ENDHLSL")
        L.append(ind + "}")
        return L, False

    # candidate keyword sets
    cand = set()
    for sp in vsps + fsps:
        ks = frozenset(kwnames[i] for i in sp["m_KeywordIndices"])
        if all(k in allowed_kw and not is_global_kw(k) for k in ks):
            cand.add(ks)
    if not cand:
        cand = {frozenset()}
    cand = sorted(cand, key=lambda s: (len(s), sorted(s)))
    if frozenset() not in cand:
        cand.insert(0, frozenset())

    branches = []  # (S, vsrc, fsrc)
    for S in cand:
        v = choose(vsps, S, kwnames)
        f = choose(fsps, S, kwnames)
        if v is None or f is None:
            continue
        vsrc, fsrc = blob.msl(v["m_BlobIndex"]), blob.msl(f["m_BlobIndex"])
        if vsrc is None or fsrc is None:
            continue
        # what would the #if chain pick without this branch?
        fb = None
        for T, tv, tf in sorted(branches, key=lambda b: -len(b[0])):
            if T <= S:
                fb = (tv, tf)
                break
        if fb == (vsrc, fsrc):
            continue
        branches.append((S, vsrc, fsrc))
        if len(branches) >= MAX_BRANCHES:
            log.append("pass '%s': variant cap reached" % stt.get("m_Name"))
            break
    if not branches:
        # fall back to the smallest keyword set of any kind
        v = min(vsps, key=lambda sp: len(sp["m_KeywordIndices"]))
        f = min(fsps, key=lambda sp: len(sp["m_KeywordIndices"]))
        branches.append((frozenset(), blob.msl(v["m_BlobIndex"]), blob.msl(f["m_BlobIndex"])))

    code_blocks = []
    decls = {}
    need_texdim = False
    ok_branches = []
    for bi, (S, vsrc, fsrc) in enumerate(branches):
        try:
            code, d, td, warn = build_variant(vsrc, fsrc, str(bi))
        except TranslateError as e:
            log.append("pass '%s' variant [%s]: %s" % (stt.get("m_Name"), " ".join(sorted(S)), e))
            continue
        for kname, v in d.items():
            if kname in decls and decls[kname] != v:
                # same name, different declaration: keep the wider one (first seen) but log
                log.append("decl conflict %s: %s vs %s" % (kname, decls[kname], v))
                continue
            decls.setdefault(kname, v)
        need_texdim |= td
        ok_branches.append((S, code))
    if not ok_branches:
        log.append("pass '%s': FAILED (placeholder emitted)" % stt.get("m_Name"))
        L.append(i2 + "// program translation failed - placeholder keeps pass indices stable")
        L.append(i2 + "HLSLPROGRAM")
        L.append(PLACEHOLDER)
        L.append(i2 + "ENDHLSL")
        L.append(ind + "}")
        return L, False

    kws = sorted(set().union(*[S for S, _ in ok_branches]))
    L.append(i2 + "HLSLPROGRAM")
    L.append("#pragma vertex vert")
    L.append("#pragma fragment frag")
    L.append("#pragma target 3.5")
    for k in kws:
        L.append("#pragma %s __ %s" % ("multi_compile_local" if use_multi_compile else "shader_feature_local", k))
    L.append(HEADER_HELPERS)
    # uniforms first, then textures/samplers
    for kname in sorted(decls, key=lambda x: (not decls[x].split()[0].startswith(("float", "half", "int", "uint", "bool")), x)):
        L.append(decls[kname])
    if need_texdim:
        L.append(TEXDIM_HELPERS)
    ordered = sorted(ok_branches, key=lambda b: -len(b[0]))
    if len(ordered) == 1:
        L.append(ordered[0][1])
    else:
        first = True
        for S, code in ordered:
            if not S:
                L.append("#else")
            else:
                cond = " && ".join("defined(%s)" % k for k in sorted(S))
                L.append(("#if " if first else "#elif ") + cond)
            first = False
            L.append(code)
        if ordered[-1][0]:
            # no empty-set branch (should not happen) -> error
            L.append("#else")
            L.append(ordered[-1][1])
        L.append("#endif")
    L.append(i2 + "ENDHLSL")
    L.append(ind + "}")
    return L, True


PLACEHOLDER = """#pragma vertex vert
#pragma fragment frag
float4x4 unity_ObjectToWorld;
float4x4 unity_MatrixVP;
float4 vert(float4 pos : POSITION) : SV_POSITION { return mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(pos.xyz, 1.0))); }
float4 frag() : SV_Target { return float4(1, 0, 1, 1); }"""


def build_shader(entry, props_block, fallback_line, custom_editor_line, allowed_kw, log):
    pf = entry["parsed"]
    plat = entry["platforms"]
    if METAL not in plat:
        raise TranslateError("no Metal platform in build")
    blob = BlobReader(entry["blobs"][plat.index(METAL)])
    name = pf["m_Name"]
    use_mc = name.startswith("Hidden/")
    out = ['Shader "%s" {' % name]
    out.append(props_block.rstrip())
    any_ok = False
    for ss in pf["m_SubShaders"]:
        out.append("\tSubShader {")
        tg = get_tags(ss.get("m_Tags"))
        if tg:
            out.append("\t\t" + tags_str(tg))
        if ss.get("m_LOD"):
            out.append("\t\tLOD %d" % ss["m_LOD"])
        for p in ss["m_Passes"]:
            lines, ok = build_pass(p, pf, blob, allowed_kw, use_mc, log, "\t\t")
            any_ok |= ok
            out += lines
        out.append("\t}")
    if fallback_line:
        out.append(fallback_line)
    if custom_editor_line:
        out.append(custom_editor_line)
    out.append("}")
    return "\n".join(out) + "\n", any_ok
