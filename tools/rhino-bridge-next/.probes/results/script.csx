#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/FSharp.Core.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/LanguageExt.Core.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/Thinktecture.Runtime.Extensions.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/CsCheck.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/Csp.Contracts.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/CSparse.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/FParsecCS.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/FParsec.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/MathNet.Numerics.FSharp.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/MathNet.Numerics.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/MathNet.Symbolics.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/Microsoft.Bcl.AsyncInterfaces.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/Microsoft.Win32.SystemEvents.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/System.Private.Windows.Core.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/System.Private.Windows.GdiPlus.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/xunit.v3.assert.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/xunit.v3.common.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/xunit.v3.core.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/Rasm.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/Rasm.RhinoBridge.Protocol.dll"
#r "tools/rhino-bridge-next/.probes/results/refs/4C8717EE43F7603BCE60CF962A3E8346/Rasm.TestKit.dll"
using System;

// PROBE 0d (rhino-bridge rebuild Phase 0): trivial scenario for the warm-loop benchmark. The wall
// clock around `bridge verify` runs of this file sizes SessionPolicy deadlines and the cutover
// criterion-4 denominator (corpus 10 §3, §5).
using LanguageExt;
using static LanguageExt.Prelude;
using Rasm.TestKit.Scenarios;

const string SCENARIO_NAME = "probe-0d-trivial";
const string CAPTURE_PATH = "tools/rhino-bridge-next/.probes/results/warm-0d.png";
global::LanguageExt.HashMap<string, int> __rasmBridgeLanguageExtBootstrap = global::LanguageExt.HashMap<string, int>.Empty;
global::LanguageExt.HashMap<(uint Serial, System.Guid DefId), int> __rasmBridgeLanguageExtTupleBootstrap = global::LanguageExt.HashMap<(uint Serial, System.Guid DefId), int>.Empty;
// --- [SCENARIO_BODY] ---
Scenario.Run("probe-0d-trivial", CAPTURE_PATH, (key, facts) => {
    Probe.Require(condition: 1 + 1 == 2, message: "arithmetic holds");
    facts.Add("trivial.ok", true);
});