using System;
using Rasm.Grasshopper.UI;
using Rasm.TestKit.Scenarios;
using Rhino;

Scenario.Run("gh-ui-subscription-marshal", CAPTURE_PATH, (key, facts) => {
    int detached = 0;
    Subscription.Atom(detach: () => detached++, marshalToUi: true).Dispose();
    Probe.Require(detached == 1, $"marshal detach count={detached}");

    int composite = 0;
    (Subscription.Atom(detach: () => composite += 1, marshalToUi: true) | Subscription.Atom(detach: () => composite += 10, marshalToUi: true)).Dispose();
    Probe.Require(composite == 11, $"composite detach sum={composite}");

    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);
    facts.Add("marshal.detached", detached);
    facts.Add("composite.sum", composite);
});
