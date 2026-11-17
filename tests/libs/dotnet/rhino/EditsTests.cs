using CsCheck;
using Xunit;

namespace Rasm.Rhino.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class EditsTests {

    // --- [STEPS]

    [Fact]
    public Task StepsRunInOrderAndTheFirstDeclinedStepStopsAtItsPosition() =>
        Gen.Bool.Array[1, 8].SampleAsync(static async outcomes => {
            Atom<Seq<int>> ran = Atom(Seq<int>());
            using LineCurve line = new(Point3d.Origin, new Point3d(1, 0, 0));
            Seq<bool> accepted = toSeq(outcomes);
            Option<int> declined = accepted.Map(static (accepts, at) => (Accepts: accepts, At: at)).Find(static step => !step.Accepts).Map(static step => step.At);
            Fin<LineCurve> edited = await RhinoHost.RunSafeAsync(
                Edits.Apply(Stages.InPlace<LineCurve>(), line, accepted.Map((accepts, at) => Steps.Of<LineCurve>(_ => {
                    ran.Swap(held => held.Add(at));
                    return accepts;
                }))));
            Assert.Equal(toSeq(Enumerable.Range(0, declined.Map(static at => at + 1).IfNone(noneValue: outcomes.Length))), ran.Value);
            Assert.Equal(
                declined.Map(static at => (RhinoReason.Declined, Some(at))),
                edited.Match(Succ: static _ => Option<(RhinoReason, Option<int>)>.None, Fail: static error => Assert.IsType<RhinoFault>(error) switch { var fault => Some((fault.Reason, fault.Index)) }));
            Assert.True(edited.Map(held => ReferenceEquals(held, line)).IfFail(static _ => true));
        });

    [Fact]
    public async Task DerivedStepFailingWithARhinoFaultKeepsItsReasonAtTheStepPosition() {
        using LineCurve line = new(Point3d.Origin, new Point3d(1, 0, 0));
        RhinoFault fault = await Expect<RhinoFault>.FailAsync(Edits.Apply(Stages.InPlace<LineCurve>(), line, [
            Steps.Of<LineCurve>(static _ => true),
            Steps.Derived<LineCurve>(static held => IO.fail<LineCurve>(new RhinoFault(held.GetLength(), RhinoReason.Outside)))]));
        Assert.Equal((RhinoReason.Outside, Some(1), (object?)1d), (fault.Reason, fault.Index, fault.Subject));
    }

    [Fact]
    public async Task DerivedStepFailingWithAnotherErrorIsDeclinedAtItsPositionWithTheErrorInside() {
        using LineCurve line = new(Point3d.Origin, new Point3d(1, 0, 0));
        Error inner = Error.New(nameof(DerivedStepFailingWithAnotherErrorIsDeclinedAtItsPositionWithTheErrorInside));
        RhinoFault fault = await Expect<RhinoFault>.FailAsync(Edits.Apply(Stages.InPlace<LineCurve>(), line, [
            Steps.Of<LineCurve>(static _ => true),
            Steps.Of<LineCurve>(static _ => true),
            Steps.Derived<LineCurve>(_ => IO.fail<LineCurve>(inner))]));
        Assert.Equal((RhinoReason.Declined, Some(2), Some(inner)), (fault.Reason, fault.Index, fault.Inner));
    }

    [Fact]
    public async Task GeometryStageFailsAsInvalidOnAnInvalidResultAndLeavesTheLiveGeometry() {
        using LineCurve line = new(Point3d.Origin, new Point3d(1, 0, 0));
        RhinoFault fault = await Expect<RhinoFault>.FailAsync(Edits.Apply(Stages.Geometry<LineCurve>(), line, [Steps.Of<LineCurve>(static copy => copy.SetEndPoint(copy.PointAtStart))]));
        Assert.Equal((RhinoReason.Invalid, true), (fault.Reason, fault.Inner.IsSome));
        Assert.Equal((true, 1.0), (line.IsValid, line.GetLength()));
    }

    // --- [COMPONENTS]

    [Fact]
    public void ComponentsOutsideTheGeometryOrOfAnotherKindFailAtEachPosition() =>
        Gen.Bool.Select(Gen.OneOf(Kinds).Select(Gen.Int[-3, 15], static (kind, index) => new ComponentIndex(kind, index)).Array[1, 8]).Sample(static (solid, components) => {
            using Brep brep = new Box(new BoundingBox(0, 0, 0, 1, 1, 1)).ToBrep();
            using Mesh mesh = Mesh.CreateFromBox(new BoundingBox(0, 0, 0, 1, 1, 1), 1, 1, 1);
            GeometryBase geometry = solid ? brep : mesh;
            HashMap<ComponentIndexType, int> counts = solid ? BrepCounts : MeshCounts;
            Seq<(RhinoReason, Option<int>)> expected = toSeq(components).Map((component, at) => counts.Find(component.ComponentIndexType).Match(
                Some: count => component.Index >= 0 && component.Index < count ? Option<(RhinoReason, Option<int>)>.None : Some((RhinoReason.Outside, Some(at))),
                None: () => Some((RhinoReason.Mismatched, Some(at))))).Somes().Strict();
            Assert.Equal(
                expected,
                geometry.Within(toSeq(components)).Match(Succ: static _ => Seq<(RhinoReason, Option<int>)>(), Fail: static error => error.AsIterable().Map(static each => Assert.IsType<RhinoFault>(each) switch { var fault => (fault.Reason, fault.Index) }).ToSeq().Strict()));
        });

    // --- [GENERATORS]

    private static readonly HashMap<ComponentIndexType, int> BrepCounts = HashMap((ComponentIndexType.BrepFace, 6), (ComponentIndexType.BrepEdge, 12), (ComponentIndexType.BrepVertex, 8));

    private static readonly HashMap<ComponentIndexType, int> MeshCounts = HashMap((ComponentIndexType.MeshFace, 6), (ComponentIndexType.MeshTopologyEdge, 12), (ComponentIndexType.MeshTopologyVertex, 8));

    private static readonly ComponentIndexType[] Kinds = [.. BrepCounts.Keys, .. MeshCounts.Keys];
}
