using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

// Batch-mode helper: compiles every pass of every shader in the project for a few
// graphics APIs (base variant + each declared keyword on its own) and writes a report.
public static class ShaderCompileCheck
{
    public static void Run()
    {
        var sb = new StringBuilder();
        var platforms = new[] { ShaderCompilerPlatform.Vulkan, ShaderCompilerPlatform.OpenGLCore, ShaderCompilerPlatform.Metal };
        int failures = 0, total = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Shader", new[] { "Assets" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null) continue;
            var msgs = ShaderUtil.GetShaderMessages(shader);
            foreach (var m in msgs)
                if (m.severity == ShaderCompilerMessageSeverity.Error)
                {
                    sb.AppendLine($"PARSE-ERROR\t{path}\t{m.message} (line {m.line})");
                    failures++;
                }
            var data = ShaderUtil.GetShaderData(shader);
            if (data == null) continue;
            for (int s = 0; s < data.SubshaderCount; s++)
            {
                var ss = data.GetSubshader(s);
                for (int p = 0; p < ss.PassCount; p++)
                {
                    var pass = ss.GetPass(p);
                    if (!pass.HasShaderStage(ShaderType.Vertex)) continue;
                    var kwsets = new List<string[]> { new string[0] };
                    string src = pass.SourceCode ?? "";
                    foreach (var line in src.Split('\n'))
                    {
                        var t = line.Trim();
                        if (t.StartsWith("#pragma shader_feature_local __ ") || t.StartsWith("#pragma multi_compile_local __ "))
                            kwsets.Add(new[] { t.Split(' ').Last() });
                    }
                    foreach (var plat in platforms)
                        foreach (var kws in kwsets)
                            foreach (var stage in new[] { ShaderType.Vertex, ShaderType.Fragment })
                            {
                                total++;
                                var r = pass.CompileVariant(stage, kws, plat, BuildTarget.StandaloneLinux64);
                                if (!r.Success)
                                {
                                    failures++;
                                    var first = r.Messages.Where(x => x.severity == ShaderCompilerMessageSeverity.Error).Take(3)
                                        .Select(x => $"{x.message} (line {x.line})");
                                    sb.AppendLine($"COMPILE-ERROR\t{path}\tss{s} pass{p} '{pass.Name}'\t{plat}\t{stage}\t[{string.Join(" ", kws)}]\t{string.Join(" | ", first)}");
                                }
                            }
                }
            }
        }
        sb.AppendLine($"TOTAL {total} compiles, {failures} failures");
        File.WriteAllText("shader_compile_report.txt", sb.ToString());
        Debug.Log("ShaderCompileCheck done: " + failures + " failures");
    }
}
