using CsCheck;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
using Rasm.Grasshopper;
using Rasm.Grasshopper.Doc;
using Rasm.Rhino.Tests.Grasshopper.Components;
using Xunit;

namespace Rasm.Rhino.Tests.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------

[Collection(typeof(Grasshopper2PlugIn))]
public sealed class DocumentsTests {

    // --- [SOLUTIONS]

    [Fact]
    public async Task DocumentWithoutInputsSolvesToCompleted() =>
        Assert.Equal(SolutionPhase.Completed, await RhinoHost.RunAsync(Relayed([]).Bind(static document => Solutions.Run(document, SolutionMode.Headless)).Map(static solution => solution.Phase).Bracket()));

    // --- [PROGRAMS]

    [Fact]
    public Task EvaluateRoutesEveryInputValueThroughTheDefinitionOnEachRun() =>
        Values.SampleAsync(static async values =>
            Assert.Equal((values, values), await RhinoHost.RunAsync((
                from document in Relayed(Seq("x"))
                from first in Evaluated(document, "x", values)
                from second in Evaluated(document, "x", values)
                select (first, second)).Bracket())));

    [Fact]
    public async Task EvaluateOfAnInputTheDefinitionLacksFailsAsMissing() {
        GrasshopperFault fault = await Expect<GrasshopperFault>.FailAsync(Relayed(Seq("x")).Bind(static document => Evaluated(document, "z", Seq(1.0))).Bracket());
        Assert.Equal((GrasshopperReason.Missing, (object?)"z"), (fault.Reason, fault.Subject));
    }

    [Fact]
    public async Task EvaluateOfTwoInputsOfOneNameFailsAsInvalid() {
        GrasshopperFault fault = await Expect<GrasshopperFault>.FailAsync(Relayed(Seq("x", "x")).Bind(static document => Evaluated(document, "x", Seq(1.0))).Bracket());
        Assert.Equal((GrasshopperReason.Invalid, (object?)"x"), (fault.Reason, fault.Subject));
    }

    // --- [FILES]

    [Fact]
    public Task StoredDocumentReadsBackAndEvaluatesAlike() =>
        Values.SampleAsync(static async values =>
            Assert.Equal(values, await RhinoHost.RunAsync(
                (from document in Relayed(Seq("x"))
                 from bytes in DocumentFiles.Store(document, FileContents.Objects)
                 from read in use(DocumentFiles.Read(bytes), static held => IO.lift(held.Close))
                 from outputs in Evaluated(read, "x", values)
                 select outputs).Bracket())));

    [Fact]
    public async Task StoredComponentDocumentReadsBackAsTheComponentAndSolvesAlike() =>
        Assert.Equal(Seq(3.0, 9.25), await RhinoHost.RunAsync((
            from placed in Subtraction.Placed([5.0, 9.5], [2.0, 0.25])
            from bytes in DocumentFiles.Store(placed.Document, FileContents.Objects)
            from read in use(DocumentFiles.Read(bytes), static held => IO.lift(held.Close))
            from copy in IO.lift(Optional(read.Objects.Find(placed.Component.InstanceId) as Subtraction).ToFin(new GrasshopperFault(placed.Component.InstanceId, GrasshopperReason.Missing)))
            from solved in Solutions.Run(read, SolutionMode.Headless)
            select Subtraction.Solved(copy)).Bracket()));

    // --- [GENERATORS]

    private static readonly Gen<Seq<double>> Values = Gen.Int[-1_000_000, 1_000_000].Array[1, 12].Select(static steps => toSeq(steps).Map(static step => step * ModelGrid.Model.Absolute).Strict());

    // --- [INPUTS]

    private static IO<Grasshopper2.Doc.Document> Relayed(Seq<string> inputs) =>
        from document in use(static () => Grasshopper2.Doc.Document.NewInertDocument(), static held => IO.lift(held.Close))
        from shout in DocumentEdits.Add(document, new Shout { UserName = "y", ClusterOutput = true })
        from listens in inputs.TraverseM(name => DocumentEdits.Add(document, new Listen { UserName = name, ClusterInput = true }).Bind(listen => DocumentEdits.Connect(listen, shout, ActionList.Empty))).As()
        select document;

    private static IO<Seq<double>> Evaluated(Grasshopper2.Doc.Document document, string input, Seq<double> values) =>
        DocumentFiles.Evaluate(document, Map((input, Garden.ITreeFromList(values))))
            .Map(static outputs => toSeq(outputs["y"].TryConvert<double>(CancellationToken.None).AllItems).Strict());
}
