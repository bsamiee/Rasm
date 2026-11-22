using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI;
using Rhino;
using Xunit;

namespace Rasm.Rhino.Tests.UI;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class ProbePanel(RhinoDoc document) : DefinedPanel<ProbePanel>(document), IDefined<View.Panel> {
    private static readonly Atom<int> Built = Atom(0);

    public static int Opened => Built.Value;

    public static View.Panel Definition { get; } = new() {
        Identity = typeof(ProbePanel),
        Caption = "Probe",
        Store = new ViewStore.DocumentStores(new RedrawPolicy.Silent()) {
            Bindings = Some<Func<RhinoDoc, IO<BindingGroup>>>(static _ => IO.lift(static () => Built.Swap(static count => count + 1)).Map(static _ => new BindingGroup([]))),
        },
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class HostingTests {

    // --- [RELEASE]

    [Fact]
    public async Task ReleasingANeverShownViewBuildsNothing() =>
        Assert.Equal((true, 0), await RhinoHost.RunAsync(RhinoHost.Headless(static doc =>
            from released in IO.lift(() => {
                using ProbePanel panel = new(doc);
                return panel.Content is Panel { Content: null };
            })
            select (released, ProbePanel.Opened))));
}
