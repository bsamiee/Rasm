using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

// PARITY DRIVER (U11) — executes the rebuilt [RhinoScenario] corpus in-host through the OLD
// bridge execute lane: hook a bin-directory probe onto the Default ALC (host + GH2 assemblies
// keep one type identity), load the compiled *.Scenarios.dll into Default, reflect the
// entrypoints, construct the SDK ScenarioContext against the live document, invoke each
// Fin<Unit> rail, and emit one verdict fact per scenario. Fact values are stringified at this
// boundary because the OLD marker pipe JSON-serializes facts and chokes on live host objects.
// Throwaway harness; the rebuilt supervisor owns this lane later.
Scenario.Run("nb-gh-ui", CAPTURE_PATH, (key, facts) => {
    string root;
    int idx = CAPTURE_PATH.IndexOf("/.artifacts/", StringComparison.Ordinal);
    if (idx > 0) { root = CAPTURE_PATH.Substring(0, idx); }
    else { root = Environment.GetEnvironmentVariable("PWD") ?? System.IO.Directory.GetCurrentDirectory(); }
    facts.Add("corpus.root", root);
    Guid gh2 = new Guid("8307876d-a461-4daa-bb77-eb3715925513");
    facts.Add("gh2.loaded", Rhino.PlugIns.PlugIn.LoadPlugIn(gh2));
    string[] expected = new[] { "MotionLayout" };
    RunCorpus(root, "tests/csharp/libs/Rasm.Grasshopper.Scenarios/bin/Release/net10.0/Rasm.Grasshopper.Scenarios.dll", expected, facts);
});

static void RunCorpus(string root, string relativeDll, string[] expected, FactBag facts) {
    string dll = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, relativeDll));
    Probe.Require(System.IO.File.Exists(dll), "corpus dll missing: " + dll);
    string binDir = System.IO.Path.GetDirectoryName(dll) ?? root;
    AssemblyLoadContext.Default.Resolving += delegate (AssemblyLoadContext context, AssemblyName requested) {
        string candidate = System.IO.Path.Combine(binDir, (requested.Name ?? "") + ".dll");
        return System.IO.File.Exists(candidate) ? context.LoadFromAssemblyPath(candidate) : null;
    };
    Assembly corpus = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
    Dictionary<string, MethodInfo> entries = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);
    foreach (Type type in corpus.GetTypes()) {
        foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
            foreach (object attr in method.GetCustomAttributes(false)) {
                if (attr.GetType().FullName == "Rasm.Bridge.Scenarios.RhinoScenarioAttribute") {
                    entries[method.Name] = method;
                }
            }
        }
    }
    facts.Add("corpus.discovered", entries.Count);
    int ok = 0;
    foreach (string name in expected) {
        Probe.Require(entries.ContainsKey(name), "scenario entrypoint missing: " + name);
        MethodInfo method = entries[name];
        Type ctxType = method.GetParameters()[0].ParameterType;
        string prefix = name;
        Action<string, object> sink = delegate (string factKey, object factValue) {
            facts.Add(prefix + "." + factKey, Plain(factValue));
        };
        object ctx = Activator.CreateInstance(ctxType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { Rhino.RhinoDoc.ActiveDoc, sink }, null);
        Probe.Require(ctx != null, "ScenarioContext construction failed for " + name);
        long started = Environment.TickCount64;
        object outcome = null;
        string thrown = null;
        try { outcome = method.Invoke(null, new object[] { ctx }); }
        catch (TargetInvocationException error) { thrown = (error.InnerException ?? error).ToString(); }
        long elapsed = Environment.TickCount64 - started;
        bool succ = false;
        if (thrown == null && outcome != null) {
            PropertyInfo isSucc = outcome.GetType().GetProperty("IsSucc");
            if (isSucc != null) { succ = (bool)isSucc.GetValue(outcome); }
        }
        MethodInfo drain = ctxType.GetMethod("DrainScopes", BindingFlags.Instance | BindingFlags.NonPublic);
        if (drain != null) {
            object leaked = drain.Invoke(ctx, new object[0]);
            facts.Add("scenario." + name + ".leakedScopes", Plain(leaked));
        }
        facts.Add("scenario." + name + ".status", succ ? "ok" : "failed");
        facts.Add("scenario." + name + ".ms", elapsed);
        if (!succ) { facts.Add("scenario." + name + ".fault", thrown != null ? thrown : (outcome == null ? "no outcome" : outcome.ToString())); }
        if (succ) { ok = ok + 1; }
    }
    facts.Add("corpus.expected", expected.Length);
    facts.Add("corpus.ok", ok);
    Probe.Require(ok == expected.Length, "corpus verdicts " + ok + "/" + expected.Length);
}

static object Plain(object value) {
    if (value == null) { return "<null>"; }
    if (value is string || value is bool || value is int || value is long || value is uint
        || value is double || value is float || value is decimal || value is Guid) { return value; }
    return value.ToString() ?? value.GetType().Name;
}
