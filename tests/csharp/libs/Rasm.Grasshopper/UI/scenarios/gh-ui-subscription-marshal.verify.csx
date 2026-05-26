using System;
using Rasm.Grasshopper.UI;
using Rhino;

int detached = 0;
Subscription.Atom(detach: () => detached++, marshalToUi: true).Dispose();
Probe.Require(detached == 1, $"marshal detach count={detached}");

int composite = 0;
(Subscription.Atom(detach: () => composite += 1, marshalToUi: true) | Subscription.Atom(detach: () => composite += 10, marshalToUi: true)).Dispose();
Probe.Require(composite == 11, $"composite detach sum={composite}");

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("rhino.mainThread", RhinoApp.IsOnMainThread);
Evidence.Emit("marshal.detached", detached);
Evidence.Emit("composite.sum", composite);
