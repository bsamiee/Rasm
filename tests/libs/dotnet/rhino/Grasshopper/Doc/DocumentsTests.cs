using CsCheck;
using Grasshopper2.Data;
using Grasshopper2.Diagnostics;
using Grasshopper2.Doc;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
using Rasm.Grasshopper.Doc;
using Rasm.Rhino.Tests.Grasshopper.Components;
using Rasm.Testing;
using Xunit;

namespace Rasm.Rhino.Tests.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------

[Collection(typeof(Grasshopper2PlugIn))]
public sealed class DocumentsTests(Fixtures fixtures) {

    // --- [PROGRAMS]

    [Fact]
    public async Task EvaluateRoutesEveryInputValueThroughTheDefinitionOnEachRun() {
        Grasshopper2.Doc.Document document = await fixtures.TakeAsync(Relayed(Seq("x")));
        await Values.SampleAsync(async values => Assert.Equal(values, await RhinoHost.RunAsync(Evaluated(document, "x", values))));
    }

    [Fact]
    public async Task EvaluateOfAnInputTheDefinitionLacksFailsAsMissing() {
        Grasshopper2.Doc.Document document = await fixtures.TakeAsync(Relayed(Seq("x")));
        RhinoFault fault = await Expect<RhinoFault>.FailAsync(Evaluated(document, "z", Seq(1.0)));
        Assert.Equal((RhinoReason.Missing, (object?)"z"), (fault.Reason, fault.Subject));
    }

    [Fact]
    public async Task EvaluateOfTwoInputsOfOneNameFailsAsRepeated() {
        Grasshopper2.Doc.Document document = await fixtures.TakeAsync(Relayed(Seq("x", "x")));
        RhinoFault fault = await Expect<RhinoFault>.FailAsync(Evaluated(document, "x", Seq(1.0)));
        Assert.Equal((RhinoReason.Repeated, "x"), (fault.Reason, fault.Subject is (string name, _) ? name : null));
    }

    // --- [FILES]

    [Fact]
    public async Task StoredComponentDocumentReadsBackAsTheComponentAndSolvesAlike() =>
        Assert.Equal(Seq(3.0, 9.25), await RhinoHost.RunAsync((
            from placed in Subtraction.Placed([5.0, 9.5], [2.0, 0.25])
            from bytes in DocumentFiles.Store(placed.Document, FileContents.Objects)
            from read in use(DocumentFiles.Read(bytes), static held => IO.lift(held.Close))
            from copy in IO.lift(Optional(read.Objects.Find(placed.Component.InstanceId) as Subtraction).ToFin(new RhinoFault(placed.Component.InstanceId, RhinoReason.Missing)))
            from solved in Solutions.Run(read, SolutionMode.Headless)
            select Subtraction.Solved(copy)).Bracket()));

    // --- [EDITS]

    [Fact]
    public async Task AddingAnObjectAnyDocumentHoldsFailsAsDeclinedWithTheHostCauseAndLeavesItWithItsOwner() {
        Grasshopper2.Doc.Document owner = await fixtures.TakeAsync(Inert);
        Grasshopper2.Doc.Document other = await fixtures.TakeAsync(Inert);
        Relay relay = await RhinoHost.RunAsync(DocumentEdits.Add(owner, new Relay()));
        RhinoFault[] faults = [await Expect<RhinoFault>.FailAsync(DocumentEdits.Add(owner, relay)), await Expect<RhinoFault>.FailAsync(DocumentEdits.Add(other, relay))];
        Assert.All(faults, static fault => Assert.Equal((RhinoReason.Declined, true), (fault.Reason, fault.Inner.Exists(static inner => inner.HasException<LoggedException>()))));
        Assert.Equal((true, false), await RhinoHost.RunAsync(IO.lift(() => (owner.Objects.Find(relay.InstanceId) is not null, other.Objects.Find(relay.InstanceId) is not null))));
    }

    // --- [GENERATORS]

    private static readonly Gen<Seq<double>> Values = Gen.Int[-1_000_000, 1_000_000].Array[1, 12].Select(static steps => toSeq(steps).Map(static step => step * ModelGrid.Model.Absolute).Strict());

    // --- [INPUTS]

    private static IO<Fixture<Grasshopper2.Doc.Document>> Inert { get; } =
        IO.lift(static () => Grasshopper2.Doc.Document.NewInertDocument()).Map(static document => new Fixture<Grasshopper2.Doc.Document>(document, _ => IO.lift(document.Close)));

    private static IO<Fixture<Grasshopper2.Doc.Document>> Relayed(Seq<string> inputs) =>
        from document in IO.lift(static () => Grasshopper2.Doc.Document.NewInertDocument())
        from shout in DocumentEdits.Add(document, new Shout { UserName = "y", ClusterOutput = true })
        from listens in inputs.TraverseM(name => DocumentEdits.Add(document, new Listen { UserName = name, ClusterInput = true }).Bind(listen => DocumentEdits.Connect(listen, shout, ActionList.Empty))).As()
        select new Fixture<Grasshopper2.Doc.Document>(document, _ => IO.lift(document.Close));

    private static IO<Seq<double>> Evaluated(Grasshopper2.Doc.Document document, string input, Seq<double> values) =>
        DocumentFiles.Evaluate(document, Map((input, Garden.ITreeFromList(values))))
            .Map(static outputs => toSeq(outputs["y"].TryConvert<double>(CancellationToken.None).AllItems).Strict());
}
