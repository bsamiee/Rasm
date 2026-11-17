using System.Globalization;
using CsCheck;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino.DocObjects;
using Xunit;

namespace Rasm.Rhino.Tests.Blocks;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PlansTests {

    // --- [RECONCILE]

    [Fact]
    public Task EachDefinitionIsCreatedAfterEveryDefinitionItPlacesAndASecondReconcilePlansNothing() =>
        Nesting.SampleAsync(static async nesting => {
            using Point mark = new(Point3d.Origin);
            Seq<Seq<int>> placed = toSeq(Enumerable.Range(0, nesting.Count)).Map(at => toSeq(Enumerable.Range(0, at)).Filter(below => nesting.Edges[at, below]).Strict()).Strict();
            Seq<PlannedDefinition> plans = toSeq(nesting.Order)
                .Map(at => new PlannedDefinition(Metadata(Named(at)), nesting.BasePoint, placed[at].Map(member => (PlannedMember)new PlannedMember.Instance(Named(member), nesting.Xform)).Add(new PlannedMember.Shape(mark))))
                .Strict();
            (string[] created, int again, (bool Valid, string Log) audit) = await RhinoHost.RunAsync(RhinoHost.Headless(doc =>
                from first in Plans.Reconcile(doc, plans, Prune.Keep, new RedrawPolicy.Silent())
                from second in Plans.Plan(doc, plans, Prune.Keep)
                select (first.Value.Choose(static step => step is RowPlan<InstanceDefinition, PlannedDefinition>.Create create ? Some(create.Spec.Metadata.Name) : None).ToArray(), second.Count, RhinoHost.Audit(doc))));
            Assert.Equal(nesting.Count, created.Length);
            Assert.All(Enumerable.Range(0, nesting.Count), at =>
                Assert.All(placed[at], member => Assert.True(System.Array.IndexOf(created, Named(member)) < System.Array.IndexOf(created, Named(at)), $"{Named(at)} places {Named(member)}")));
            Assert.Equal(0, again);
            Assert.True(audit.Valid, audit.Log);
        });

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task RingOfDefinitionsBehindAnAcyclicTailFailsAsOneRepeatedRing(int count) {
        Seq<string> ring = toSeq(Enumerable.Range(0, count)).Map(static at => string.Create(CultureInfo.InvariantCulture, $"R{at}")).Strict();
        Seq<PlannedDefinition> plans = ring.Map((name, at) => new PlannedDefinition(Metadata(name), Point3d.Origin, [new PlannedMember.Instance(ring[(at + 1) % count], Transform.Identity)]))
            .Add(new PlannedDefinition(Metadata("T"), Point3d.Origin, [new PlannedMember.Instance(ring[0], Transform.Identity)]))
            .Add(new PlannedDefinition(Metadata("U"), Point3d.Origin, [new PlannedMember.Instance("T", Transform.Identity)]))
            .Strict();
        Error error = await Expect<Error>.FailAsync(RhinoHost.Headless(doc => Plans.Plan(doc, plans, Prune.Keep)));
        RhinoFault fault = Assert.IsType<RhinoFault>(Assert.Single(error.AsIterable().ToSeq()));
        Assert.Equal(RhinoReason.Repeated, fault.Reason);
        Assert.Equal(ring.Order(StringComparer.Ordinal), Assert.IsType<Seq<string>>(fault.Subject).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ChangedDefinitionIsUpdatedInPlace() {
        using Point first = new(new Point3d(1, 2, 3));
        using Point moved = new(new Point3d(4, 5, 6));
        (Seq<RowPlan<InstanceDefinition, PlannedDefinition>> steps, Guid before, Guid after, Seq<Point3d> locations) = await RhinoHost.RunAsync(RhinoHost.Headless(doc =>
            from added in Plans.Reconcile(doc, [new PlannedDefinition(Metadata("D"), Point3d.Origin, [new PlannedMember.Shape(first)])], Prune.Keep, new RedrawPolicy.Silent())
            let original = doc.InstanceDefinitions.Find("D").Id
            from changed in Plans.Reconcile(doc, [new PlannedDefinition(Metadata("D"), Point3d.Origin, [new PlannedMember.Shape(moved)])], Prune.Keep, new RedrawPolicy.Silent())
            let definition = doc.InstanceDefinitions.Find("D")
            select (
                changed.Value,
                original,
                definition.Id,
                toSeq(definition.GetObjects()).Map(static held => ((Point)held.Geometry).Location).Strict())));
        Assert.IsType<RowPlan<InstanceDefinition, PlannedDefinition>.Change>(Assert.Single(steps));
        Assert.Equal((before, moved.Location), (after, Assert.Single(locations)));
    }

    // --- [PRUNE]

    [Fact]
    public async Task PurgedStaleDefinitionLeavesTheTable() {
        using Point mark = new(Point3d.Origin);
        Assert.Equal(0, await RhinoHost.RunAsync(RhinoHost.Headless(doc =>
            from added in Plans.Reconcile(doc, [new PlannedDefinition(Metadata("Stale"), Point3d.Origin, [new PlannedMember.Shape(mark)])], Prune.Keep, new RedrawPolicy.Silent())
            from purged in Plans.Reconcile(doc, [], Prune.Purge, new RedrawPolicy.Silent())
            select doc.InstanceDefinitions.GetList(ignoreDeleted: false).Length)));
    }

    // --- [GENERATORS]

    private static readonly Gen<(int Count, bool[,] Edges, int[] Order, Point3d BasePoint, Transform Xform)> Nesting =
        from count in Gen.Int[1, 6]
        from edges in Gen.Bool.Array2D[count, count]
        from order in Gen.Shuffle(Enumerable.Range(0, count).ToArray())
        from basePoint in ModelGrid.Point
        from offset in ModelGrid.Offset
        from turn in ModelGrid.Turn
        select (count, edges, order, basePoint, Transform.Translation(offset) * Transform.Rotation(turn.Angle, turn.Axis, Point3d.Origin));

    // --- [INPUTS]

    private static string Named(int at) => string.Create(CultureInfo.InvariantCulture, $"B{at}");

    private static DefinitionMetadata Metadata(string name) => new(name, None, None);
}
