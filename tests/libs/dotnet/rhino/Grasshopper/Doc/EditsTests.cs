using Grasshopper2.Parameters.Special;
using Rasm.Grasshopper.Doc;
using Xunit;

namespace Rasm.Rhino.Tests.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------

[Collection(typeof(Grasshopper2PlugIn))]
public sealed class EditsTests {

    [Fact]
    public async Task AddedObjectJoinsTheDocumentAndIsReturned() {
        (bool same, bool found) = await RhinoHost.RunAsync((
            from document in use(static () => Grasshopper2.Doc.Document.NewInertDocument(), static held => IO.lift(held.Close))
            let relay = new Relay()
            from added in DocumentEdits.Add(document, relay)
            select (ReferenceEquals(added, relay), document.Objects.Find(relay.InstanceId) is not null)).Bracket());
        Assert.Equal((true, true), (same, found));
    }
}
