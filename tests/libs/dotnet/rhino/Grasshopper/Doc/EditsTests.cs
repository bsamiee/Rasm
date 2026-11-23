using Grasshopper2.Diagnostics;
using Grasshopper2.Parameters.Special;
using Rasm.Grasshopper.Doc;
using Xunit;

namespace Rasm.Rhino.Tests.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------

[Collection(typeof(Grasshopper2PlugIn))]
public sealed class EditsTests {

    // --- [OBJECTS]

    [Fact]
    public async Task AddedObjectJoinsTheDocumentAndIsReturned() {
        (bool same, bool found) = await RhinoHost.RunAsync((
            from document in use(static () => Grasshopper2.Doc.Document.NewInertDocument(), static held => IO.lift(held.Close))
            let relay = new Relay()
            from added in DocumentEdits.Add(document, relay)
            select (ReferenceEquals(added, relay), document.Objects.Find(relay.InstanceId) is not null)).Bracket());
        Assert.Equal((true, true), (same, found));
    }

    [Fact]
    public async Task AddingAnObjectTheDocumentAlreadyHoldsFailsAsDeclined() {
        RhinoFault fault = await Expect<RhinoFault>.FailAsync((
            from document in use(static () => Grasshopper2.Doc.Document.NewInertDocument(), static held => IO.lift(held.Close))
            let relay = new Relay()
            from added in DocumentEdits.Add(document, relay)
            from again in DocumentEdits.Add(document, relay)
            select again).Bracket());
        Assert.Equal(RhinoReason.Declined, fault.Reason);
        Assert.True(fault.Inner.Exists(static inner => inner.HasException<LoggedException>()), fault.ToString());
    }

    [Fact]
    public async Task AddingAnObjectAnotherDocumentHoldsFailsAsDeclined() {
        RhinoFault fault = await Expect<RhinoFault>.FailAsync((
            from owner in use(static () => Grasshopper2.Doc.Document.NewInertDocument(), static held => IO.lift(held.Close))
            from other in use(static () => Grasshopper2.Doc.Document.NewInertDocument(), static held => IO.lift(held.Close))
            let relay = new Relay()
            from added in DocumentEdits.Add(owner, relay)
            from moved in DocumentEdits.Add(other, relay)
            select moved).Bracket());
        Assert.Equal(RhinoReason.Declined, fault.Reason);
        Assert.True(fault.Inner.Exists(static inner => inner.HasException<LoggedException>()), fault.ToString());
    }
}
