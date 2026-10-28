"""Translate Unity/hlslcc-generated Metal Shading Language back into Unity-compatible HLSL.

Only the subset of MSL that HLSLcc emits is supported. Raises TranslateError on anything
it does not understand so callers can fall back gracefully.
"""
import re


class TranslateError(Exception):
    pass


# --------------------------------------------------------------------------- helpers

def match_paren(s, i, open_ch="(", close_ch=")"):
    """s[i] == open_ch; return index of matching close."""
    depth = 0
    j = i
    n = len(s)
    while j < n:
        c = s[j]
        if c == open_ch:
            depth += 1
        elif c == close_ch:
            depth -= 1
            if depth == 0:
                return j
        j += 1
    raise TranslateError("unbalanced %s at %d" % (open_ch, i))


def split_args(s, angle=False):
    """split top-level comma separated args (handles (), [], {} and optionally <>)."""
    args, depth, cur = [], 0, []
    opens, closes = ("([{<", ")]}>") if angle else ("([{", ")]}")
    i = 0
    while i < len(s):
        c = s[i]
        if c in opens:
            depth += 1
        elif c in closes:
            depth -= 1
        if c == "," and depth == 0:
            args.append("".join(cur).strip())
            cur = []
        else:
            cur.append(c)
        i += 1
    if "".join(cur).strip():
        args.append("".join(cur).strip())
    return args


SCALARS = ("float", "half", "int", "uint", "bool")


def hlsl_type(t):
    t = t.strip()
    t = t.replace("uint32_t", "uint").replace("int32_t", "int")
    t = re.sub(r"\bushort\b", "uint", t)
    t = re.sub(r"\bshort\b", "int", t)
    return t


TEX_TYPES = {
    "texture2d": "Texture2D",
    "texture2d_array": "Texture2DArray",
    "texture3d": "Texture3D",
    "texturecube": "TextureCube",
    "texturecube_array": "TextureCubeArray",
    "depth2d": "Texture2D",
    "depth2d_array": "Texture2DArray",
    "depthcube": "TextureCube",
    "texture2d_ms": "Texture2DMS<float4>",
}

FUNC_RENAMES = [
    (r"\bfma\(", "mad("),
    (r"\bfract\(", "frac("),
    (r"\bmix\(", "lerp("),
    (r"\brint\(", "round("),
    (r"\bdfdx\(", "ddx("),
    (r"\bdfdy\(", "ddy("),
    (r"\bfmin\(", "min("),
    (r"\bfmax\(", "max("),
    (r"\bpopcount\(", "countbits("),
    (r"\breverse_bits\(", "reversebits("),
    (r"\bdiscard_fragment\(\)", "discard"),
    (r"\b(?:fast|precise|metal)::", ""),
    (r"\buint32_t\b", "uint"),
    (r"\bint32_t\b", "int"),
    (r"\bthread\s+", ""),
]


def rewrite_calls(code, name_regex, fn):
    """Find calls matching name_regex (regex ending right before '('), replace via fn(match, args_str)."""
    out = []
    pos = 0
    rx = re.compile(name_regex)
    while True:
        m = rx.search(code, pos)
        if not m:
            out.append(code[pos:])
            break
        p = m.end()
        if p >= len(code) or code[p] != "(":
            out.append(code[pos:p])
            pos = p
            continue
        q = match_paren(code, p)
        inner = code[p + 1:q]
        # recurse into inner first
        inner = rewrite_calls(inner, name_regex, fn)
        out.append(code[pos:m.start()])
        out.append(fn(m, inner))
        pos = q + 1
    return "".join(out)


def fix_constructors(code):
    def fn(m, inner):
        t = m.group(0)
        args = split_args(inner)
        if len(args) == 1:
            return "((%s)(%s))" % (t, args[0])
        return "%s(%s)" % (t, inner)
    return rewrite_calls(code, r"\b(?:float|half|int|uint|bool)[234]\b(?!x)", fn)


def fix_select(code):
    def fn(m, inner):
        a = split_args(inner)
        if len(a) != 3:
            raise TranslateError("select arity")
        return "((%s) ? (%s) : (%s))" % (a[2], a[1], a[0])
    return rewrite_calls(code, r"\bselect\b", fn)


def fix_astype(code):
    def fn(m, inner):
        t = m.group(1).strip()
        base = re.match(r"(float|half|int|uint|bool)", t)
        if not base:
            raise TranslateError("as_type " + t)
        b = base.group(1)
        f = {"float": "asfloat", "half": "asfloat", "int": "asint", "uint": "asuint"}.get(b)
        if not f:
            raise TranslateError("as_type " + t)
        return "%s(%s)" % (f, inner)
    return rewrite_calls(code, r"\bas_type<\s*([\w ]+?)\s*>", fn)


def fix_matrix_ctor(code, warn):
    # MSL matrix constructors take columns, HLSL takes rows -> wrap with transpose.
    def fn(m, inner):
        t = m.group(0)
        args = split_args(inner)
        r, c = int(t[-3]), int(t[-1])
        if len(args) == c:  # column vectors
            warn.append("matrix ctor")
            ht = "%s%dx%d" % (t[:-3], c, r)
            return "transpose(%s(%s))" % (ht, inner)
        if len(args) == 1:
            return "((%s)(%s))" % (t, inner)
        raise TranslateError("matrix ctor form")
    return rewrite_calls(code, r"\b(?:float|half)[234]x[234]\b", fn)


class TexInfo:
    def __init__(self, name, kind, comp):
        self.name, self.kind, self.comp = name, kind, comp


def fix_texture_calls(code, textures, errors):
    names = "|".join(re.escape(n) for n in sorted(textures, key=len, reverse=True))
    if not names:
        return code
    rx = r"\b(%s)\.(sample_compare|sample|read|gather|get_width|get_height|get_num_mip_levels)\b" % names

    def fn(m, inner):
        tex = textures[m.group(1)]
        meth = m.group(2)
        a = split_args(inner)
        k = tex.kind
        is_arr = k in ("texture2d_array", "depth2d_array", "texturecube_array")
        name = tex.name
        if meth == "sample":
            samp, coord = a[0], a[1]
            rest = a[2:]
            if is_arr:
                if not rest:
                    raise TranslateError("array sample w/o index")
                idx = rest[0]
                rest = rest[1:]
                dim = 3 if k == "texturecube_array" else 2
                coord = "float%d(%s, %s)" % (dim + 1, coord, idx)
            if not rest:
                return "%s.Sample(%s, %s)" % (name, samp, coord)
            opt = rest[0]
            offset = rest[1] if len(rest) > 1 else None
            mm = re.match(r"(bias|level|gradient2d|gradientcube|gradient3d|min_lod_clamp)\((.*)\)$", opt, re.S)
            if not mm:
                # offset-only form
                return "%s.Sample(%s, %s, %s)" % (name, samp, coord, opt)
            kind, val = mm.group(1), mm.group(2)
            off = (", " + offset) if offset else ""
            if kind == "bias":
                return "%s.SampleBias(%s, %s, %s%s)" % (name, samp, coord, val, off)
            if kind == "level":
                return "%s.SampleLevel(%s, %s, %s%s)" % (name, samp, coord, val, off)
            if kind.startswith("gradient"):
                g = split_args(val)
                return "%s.SampleGrad(%s, %s, %s, %s%s)" % (name, samp, coord, g[0], g[1], off)
            raise TranslateError("sample opt " + kind)
        if meth == "sample_compare":
            samp, coord = a[0], a[1]
            rest = a[2:]
            if is_arr:
                coord = "float3(%s, %s)" % (coord, rest[0])
                rest = rest[1:]
            ref = rest[0]
            return "%s.SampleCmpLevelZero(%s, %s, %s)" % (name, samp, coord, ref)
        if meth == "read":
            if k == "texture2d_ms":
                return "%s.Load(int2(%s), %s)" % (name, a[0], a[1])
            if k in ("texture2d", "depth2d"):
                lod = a[1] if len(a) > 1 else "0"
                return "%s.Load(int3(int2(%s), %s))" % (name, a[0], lod)
            if is_arr:
                lod = a[2] if len(a) > 2 else "0"
                return "%s.Load(int4(int2(%s), %s, %s))" % (name, a[0], a[1], lod)
            if k == "texture3d":
                lod = a[1] if len(a) > 1 else "0"
                return "%s.Load(int4(int3(%s), %s))" % (name, a[0], lod)
            raise TranslateError("read on " + k)
        if meth == "gather":
            samp, coord = a[0], a[1]
            rest = a[2:]
            comp = "x"
            off = None
            for r in rest:
                cm = re.match(r"component::(\w)", r)
                if cm:
                    comp = cm.group(1)
                elif not re.match(r"int2\(0\)", r):
                    off = r
            fnm = {"x": "GatherRed", "y": "GatherGreen", "z": "GatherBlue", "w": "GatherAlpha"}[comp]
            return "%s.%s(%s, %s%s)" % (name, fnm, samp, coord, (", " + off) if off else "")
        if meth in ("get_width", "get_height"):
            lod = a[0] if a else "0"
            if k not in ("texture2d", "depth2d"):
                raise TranslateError("dims on " + k)
            return "_hlslcc_tex_%s(%s, %s)" % ("w" if meth == "get_width" else "h", name, lod)
        raise TranslateError("tex method " + meth)

    return rewrite_calls(code, rx, fn)


def strip_comments(s):
    s = re.sub(r"/\*.*?\*/", "", s, flags=re.S)
    s = re.sub(r"//[^\n]*", "", s)
    return s


# --------------------------------------------------------------------------- stage translation

class Stage:
    def __init__(self):
        self.uniforms = {}      # name -> decl
        self.textures = {}      # name -> decl
        self.samplers = {}      # name -> decl
        self.structs = []       # extra struct decls (HLSL)
        self.statics = []       # static const decls
        self.vin = []           # (type, name, semantic)
        self.vout = []          # (type, name, semantic, interp)
        self.fout = []          # (type, name, semantic)
        self.params = []        # extra params
        self.body = ""
        self.ret = ""
        self.warn = []
        self.uses_tex_dims = False


VIN_SEM = re.compile(r"^([A-Z]+?)(\d+)$")


def vin_semantic(name):
    m = VIN_SEM.match(name)
    if not m:
        return name
    base, idx = m.group(1), m.group(2)
    if base == "TEXCOORD":
        return "TEXCOORD" + idx
    if base == "POSITION" and idx == "0":
        return "POSITION"
    if idx == "0":
        return base
    return base + idx


def parse_struct_members(body):
    mems = []
    for line in body.split(";"):
        line = line.strip()
        if not line:
            continue
        attr = None
        am = re.search(r"\[\[(.*?)\]\]", line)
        if am:
            attr = am.group(1).strip()
            line = (line[:am.start()] + line[am.end():]).strip()
        m = re.match(r"^([\w:<>, ]+?)\s+(\w+)\s*(\[\s*(\d+)\s*\])?$", line)
        if not m:
            raise TranslateError("struct member: " + line)
        mems.append((hlsl_type(m.group(1)), m.group(2), int(m.group(4)) if m.group(4) else None, attr))
    return mems


def translate(src, stage_kind, prefix):
    """stage_kind: 'vertex' or 'fragment'. prefix: unique prefix for struct names/static consts."""
    st = Stage()
    s = strip_comments(src)
    s = re.sub(r"^\s*#include[^\n]*\n", "", s, flags=re.M)
    s = re.sub(r"^\s*using namespace metal;\s*\n", "", s, flags=re.M)
    s = re.sub(r"^\s*#pragma[^\n]*\n", "", s, flags=re.M)

    # ---- structs
    structs = {}
    for m in re.finditer(r"struct\s+(\w+)\s*\{(.*?)\n\};", s, re.S):
        structs[m.group(1)] = m.group(2)
    s_nostruct = re.sub(r"struct\s+(\w+)\s*\{(.*?)\n\};", "", s, flags=re.S)

    # ---- entry point
    em = re.search(r"\b(vertex|fragment)\s+(\w+)\s+xlatMtlMain\s*\(", s_nostruct)
    if not em:
        raise TranslateError("no entry point")
    if em.group(1) != stage_kind:
        raise TranslateError("stage mismatch")
    ret_type = em.group(2)
    p0 = em.end() - 1
    p1 = match_paren(s_nostruct, p0)
    params = split_args(s_nostruct[p0 + 1:p1], angle=True)
    b0 = s_nostruct.index("{", p1)
    b1 = match_paren(s_nostruct, b0, "{", "}")
    body = s_nostruct[b0 + 1:b1]
    globals_part = s_nostruct[:em.start()]

    # ---- global scope constants
    for line in globals_part.split("\n"):
        line = line.strip()
        if not line:
            continue
        if "rp_output_remap" in line or "xlt_remap" in line:
            continue
        fm = re.match(r"constant\s+([\w ]+?)\s+(\w+)\s*\[\[\s*function_constant\(\d+\)\s*\]\]\s*;", line)
        if fm:
            st.statics.append("static const %s %s = (%s)0;" % (hlsl_type(fm.group(1)), fm.group(2), hlsl_type(fm.group(1))))
            continue
        st.statics.append(line)  # handled below as a block
    gtext = "\n".join(x for x in st.statics if not x.startswith("static const"))
    st.statics = [x for x in st.statics if x.startswith("static const")]
    if gtext.strip():
        gtext = re.sub(r"\bconstant\s+", "static const ", gtext)
        gtext = gtext.replace("constexpr ", "")
        st.statics.append(gtext)

    cb_names = []
    matrices = {}  # name -> (rows, cols, count)
    textures = {}
    input_struct = None
    frag_coord = False

    def add_cb(struct_name):
        if struct_name not in structs:
            raise TranslateError("missing CB struct " + struct_name)
        for t, n, arr, attr in parse_struct_members(structs[struct_name]):
            mm = re.match(r"hlslcc_mtx(\d)x(\d)(\w+)$", n)
            if mm:
                R, C, real = int(mm.group(1)), int(mm.group(2)), mm.group(3)
                vec = re.match(r"(float|half)(\d)$", t)
                if not vec:
                    raise TranslateError("matrix element type " + t)
                cnt = arr or C
                nmat = cnt // C
                base = vec.group(1)
                R2 = int(vec.group(2))
                mtype = "%s%dx%d" % (base, R2, C)
                matrices[real] = (R2, C, nmat)
                if nmat == 1:
                    st.uniforms[real] = "%s %s;" % (mtype, real)
                else:
                    st.uniforms[real] = "%s %s[%d];" % (mtype, real, nmat)
                continue
            if t not in structs and re.match(r"^(float|half|int|uint|bool)([234](x[234])?)?$", t) is None:
                raise TranslateError("CB member type " + t)
            if t in structs:
                raise TranslateError("struct CB member (instancing) " + t)
            st.uniforms[n] = "%s %s%s;" % (t, n, "[%d]" % arr if arr else "")

    for p in params:
        p = p.strip()
        m = re.match(r"constant\s+(\w+)\s*&\s*(\w+)\s*\[\[\s*buffer\(\d+\)\s*\]\]", p)
        if m:
            add_cb(m.group(1))
            cb_names.append(m.group(2))
            continue
        m = re.match(r"sampler\s+(\w+)\s*\[\[\s*sampler\s*\(\d+\)\s*\]\]", p)
        if m:
            st.samplers[m.group(1)] = "SamplerState %s;" % m.group(1)
            continue
        m = re.match(r"(\w+)<\s*(\w+)\s*(?:,\s*access::(\w+)\s*)?>\s*(\w+)\s*\[\[\s*texture\(\d+\)\s*\]\]", p)
        if m:
            kind, comp, access, name = m.group(1), m.group(2), m.group(3), m.group(4)
            if kind not in TEX_TYPES:
                raise TranslateError("texture kind " + kind)
            if access in ("write", "read_write"):
                raise TranslateError("UAV texture")
            textures[name] = TexInfo(name, kind, comp)
            ht = TEX_TYPES[kind]
            if kind.startswith("depth") and "<" not in ht:
                ht = ht + "<float>"
            st.textures[name] = "%s %s;" % (ht, name)
            continue
        m = re.match(r"(Mtl_\w+)\s+input\s*\[\[\s*stage_in\s*\]\]", p)
        if m:
            input_struct = m.group(1)
            continue
        m = re.match(r"float4\s+mtl_FragCoord\s*\[\[\s*position\s*\]\]", p)
        if m:
            frag_coord = True
            continue
        m = re.match(r"bool\s+(\w+)\s*\[\[\s*front_facing\s*\]\]", p)
        if m:
            st.params.append("bool %s : SV_IsFrontFace" % m.group(1))
            continue
        m = re.match(r"uint\s+(\w+)\s*\[\[\s*(vertex_id|instance_id|base_instance|base_vertex|sample_id)\s*\]\]", p)
        if m:
            sem = {"vertex_id": "SV_VertexID", "instance_id": "SV_InstanceID", "sample_id": "SV_SampleIndex"}.get(m.group(2))
            if sem:
                st.params.append("uint %s : %s" % (m.group(1), sem))
            else:
                body = "    uint %s = 0;\n" % m.group(1) + body
            continue
        raise TranslateError("param: " + p[:120])

    # ---- IO structs
    if stage_kind == "vertex":
        if input_struct:
            for t, n, arr, attr in parse_struct_members(structs[input_struct]):
                st.vin.append((t, n, vin_semantic(n)))
        outname = ret_type
        if outname not in structs:
            raise TranslateError("no vertex out struct")
        for t, n, arr, attr in parse_struct_members(structs[outname]):
            if arr:
                raise TranslateError("array varying")
            a = attr or ""
            if "position" in a:
                st.vout.append((t, n, "SV_POSITION", ""))
            elif a.startswith("user("):
                interp = ""
                if "flat" in a or re.match(r"u?int|bool", t):
                    interp = "nointerpolation "
                elif "no_perspective" in a:
                    interp = "noperspective "
                elif "centroid" in a:
                    interp = "centroid "
                st.vout.append((t, n, None, interp))
            elif "point_size" in a:
                st.vout.append((t, n, "PSIZE", ""))
            else:
                raise TranslateError("vout attr " + a)
        body = re.sub(r"\b%s\s+output\s*;" % re.escape(outname), "V2F output = (V2F)0;", body)
    else:
        st.frag_inputs = []
        if input_struct:
            for t, n, arr, attr in parse_struct_members(structs[input_struct]):
                st.frag_inputs.append(n)
        st.frag_coord = frag_coord
        if ret_type != "void":
            if ret_type not in structs:
                raise TranslateError("no frag out struct")
            for t, n, arr, attr in parse_struct_members(structs[ret_type]):
                a = attr or ""
                cm = re.match(r"color\((?:rp_output_remap_|xlt_remap_o\[)?(\d+)", a)
                if cm:
                    st.fout.append((t, n, "SV_Target%s" % cm.group(1)))
                elif a.startswith("depth"):
                    st.fout.append((t, n, "SV_Depth"))
                elif "sample_mask" in a:
                    st.fout.append((t, n, "SV_Coverage"))
                else:
                    raise TranslateError("fout attr " + a)
            body = re.sub(r"\b%s\s+output\s*;" % re.escape(ret_type), "FOut output = (FOut)0;", body)
        if frag_coord:
            body = re.sub(r"\bmtl_FragCoord\b", "input.mtl_Position", body)
    st.ret = ret_type

    # ---- constexpr samplers in body
    def cs(m):
        name, args = m.group(1), m.group(2)
        if "compare_func" in args:
            st.samplers["sampler_LinearClampCompare"] = "SamplerComparisonState sampler_LinearClampCompare;"
            st.renames = getattr(st, "renames", {})
            st.renames[name] = "sampler_LinearClampCompare"
        else:
            st.samplers[name] = "SamplerState %s;" % name
        return ""
    body = re.sub(r"constexpr\s+sampler\s+(\w+)\s*\((.*?)\)\s*;", cs, body)
    for old, new in getattr(st, "renames", {}).items():
        body = re.sub(r"\b%s\b" % re.escape(old), new, body)

    # ---- CB member access
    for cb in cb_names:
        def mrep(code, cb=cb):
            rx = re.compile(r"\b%s\.hlslcc_mtx(\d)x(\d)(\w+)\[" % re.escape(cb))
            out, pos = [], 0
            while True:
                m = rx.search(code, pos)
                if not m:
                    out.append(code[pos:])
                    break
                q = match_paren(code, m.end() - 1, "[", "]")
                idx = code[m.end():q]
                real = m.group(3)
                R, C, n = matrices[real]
                out.append(code[pos:m.start()])
                if n == 1:
                    out.append("HLSLCC_COL(%s, %s)" % (real, idx))
                else:
                    out.append("HLSLCC_COLA(%s, %s, %d)" % (real, idx, C))
                pos = q + 1
            return "".join(out)
        body = mrep(body)
        body = re.sub(r"\b%s\.(\w+)" % re.escape(cb), r"\1", body)

    # ---- generic rewrites
    for a, b in FUNC_RENAMES:
        body = re.sub(a, b, body)
    body = fix_astype(body)
    body = fix_select(body)
    body = fix_texture_calls(body, textures, st.warn)
    if "_hlslcc_tex_" in body:
        st.uses_tex_dims = True
    body = fix_matrix_ctor(body, st.warn)
    body = fix_constructors(body)
    body = re.sub(r"(\d\.?\d*(?:e[+-]?\d+)?)h\b", r"\1", body)
    body = re.sub(r"\bconstant\s+", "const ", body)
    body = body.replace("constexpr ", "")

    # leftovers that indicate unsupported constructs
    for bad in ("device ", "threadgroup", "simd_", "atomic_", "[[", "access::", "::"):
        if bad in body:
            raise TranslateError("unsupported construct '%s'" % bad.strip())
    # statics: also generic rewrites
    st.statics = [fix_constructors(fix_astype(x)) for x in st.statics]
    # rename static const symbols to avoid vertex/fragment clashes
    for decl in list(st.statics):
        for sm in re.finditer(r"static const [\w ]+?\s+(\w+)\s*(\[|=)", decl):
            old = sm.group(1)
            new = prefix + old
            body = re.sub(r"\b%s\b" % re.escape(old), new, body)
            st.statics = [re.sub(r"\b%s\b" % re.escape(old), new, d) for d in st.statics]
    st.body = body
    return st
