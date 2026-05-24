// SCENARIO_NAME and CAPTURE_PATH are injected by scripts/rhino.sh verify.
#r "/Users/bardiasamiee/Documents/99.Github/Rasm/apps/grasshopper/Radyab/bin/Debug/net10.0/Rasm.dll"
#r "/Users/bardiasamiee/Documents/99.Github/Rasm/apps/grasshopper/Radyab/bin/Debug/net10.0/Rasm.Grasshopper.dll"
using System;
using Rasm.Grasshopper.UI;
using Rhino;

static void Require(bool condition, string message) =>
    _ = condition ? true : throw new InvalidOperationException(message);

int detached = 0;
Subscription.Atom(detach: () => detached++, marshalToUi: true).Dispose();
Require(detached == 1, $"marshal detach count={detached}");

int composite = 0;
(Subscription.Atom(detach: () => composite += 1, marshalToUi: true) | Subscription.Atom(detach: () => composite += 10, marshalToUi: true)).Dispose();
Require(composite == 11, $"composite detach sum={composite}");

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"capture={CAPTURE_PATH}");
Console.WriteLine($"rhino.mainThread={RhinoApp.IsOnMainThread}");
Console.WriteLine($"marshal.detached={detached}");
Console.WriteLine($"composite.sum={composite}");
