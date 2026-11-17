using System.Globalization;
using CsCheck;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
using Rasm.Grasshopper;
using Rasm.Grasshopper.Components;
using Rasm.Grasshopper.Data;
using Rasm.Grasshopper.Doc;
using Rhino;
using Xunit;

namespace Rasm.Rhino.Tests.Grasshopper.Components;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class Subtraction : Component<Subtraction>, IComponentDefinition<Subtraction> {

    // --- [CONSTRUCTION]

    public Subtraction() { }

    public Subtraction(GrasshopperIO.IReader reader) : base(reader) { }

    // --- [DEFINITION]

    public static Nomen Face => new("Subtraction", "First value less the second", "Maths", "Tests");

    public static Func<IDataAccess, Fin<Unit>> Define(Ports<Subtraction> ports) {
        Func<IDataAccess, Fin<double>> minuend = ports.Item(new NumberParameter("Minuend", "A", "Value to subtract from", Access.Item));
        Func<IDataAccess, Fin<double>> subtrahend = ports.Item(new NumberParameter("Subtrahend", "B", "Value to subtract", Access.Item));
        Func<IDataAccess, double, Unit> difference = ports.Output(new NumberParameter("Subtraction", "D", "First value less the second", Access.Item));
        return access =>
            from a in minuend(access)
            from b in subtrahend(access)
            from kept in GrasshopperFault.Unless(a >= b, GrasshopperReason.Invalid, b)
            select difference(access, a - b);
    }

    // --- [INPUTS]

    public static IO<(Grasshopper2.Doc.Document Document, Subtraction Component)> Placed(Seq<double> minuends, Seq<double> subtrahends) =>
        from document in use(static () => Grasshopper2.Doc.Document.NewInertDocument(), static held => IO.lift(held.Close))
        from component in DocumentEdits.Add(document, new Subtraction())
        from fed in IO.lift(() => {
            component.Parameters.Input(0).PersistentDataWeak = Garden.ITreeFromList(minuends);
            component.Parameters.Input(1).PersistentDataWeak = Garden.ITreeFromList(subtrahends);
        })
        select (document, component);

    public static Seq<double> Solved(Subtraction component) => toSeq(component.Parameters.Output(0).State.Data.Tree().TryConvert<double>(CancellationToken.None).AllItems).Strict();
}

// --- [OPERATIONS] ----------------------------------------------------------------------

[Collection(typeof(Grasshopper2PlugIn))]
public sealed class ComponentTests {

    // --- [SOLUTIONS]

    [Fact]
    public Task ComponentWritesEachMinuendLessItsSubtrahendInInputOrder() =>
        Pairs.SampleAsync(static async pairs => {
            (Seq<double> outputs, Seq<MessageLevel> messages) = await RhinoHost.RunAsync((
                from placed in Subtraction.Placed(pairs.Map(static pair => pair.Minuend), pairs.Map(static pair => pair.Subtrahend))
                from solved in Solutions.Run(placed.Document, SolutionMode.Headless)
                select (Subtraction.Solved(placed.Component), toSeq(placed.Component.IterateOverAllMessages()).Map(static message => message.Level).Strict())).Bracket());
            Assert.Equal(pairs.Map(static pair => pair.Margin), outputs, static (want, got) => Math.Abs(want - got) <= RhinoMath.SqrtEpsilon * Math.Max(1d, Math.Abs(want)));
            Assert.Empty(messages);
        });

    [Fact]
    public Task SubtrahendAboveItsMinuendFailsTheSolutionWithTheFullComponentMessage() =>
        Gen.Int[-1_000_000, 1_000_000].Select(Gen.Int[1, 1_000_000]).SampleAsync(static async (minuend, excess) => {
            double subtrahend = (minuend + excess) * ModelGrid.Model.Absolute;
            GrasshopperFault fault = await Expect<GrasshopperFault>.FailAsync(
                Subtraction.Placed([minuend * ModelGrid.Model.Absolute], [subtrahend]).Bind(static placed => Solutions.Run(placed.Document, SolutionMode.Headless)).Bracket());
            Assert.Equal(GrasshopperReason.Unfinished, fault.Reason);
            Assert.Equal(
                [string.Create(CultureInfo.InvariantCulture, $"{GrasshopperReason.Unfinished.Message}: {nameof(Subtraction)}: {GrasshopperReason.Invalid.Message}: b = {subtrahend}")],
                fault.Inner.ToSeq().Bind(static inner => inner.AsIterable().ToSeq()).Map(static leaf => leaf.Message));
        });

    // --- [IDENTITY]

    [Fact]
    public async Task ComponentIdentityIsTheNameBasedUuidOfItsTypeInItsAssembly() =>
        Assert.Equal(new Guid(0x4a6a9ba9, 0x89bd, 0x55fc, 0xb7, 0x88, 0x2e, 0xea, 0x98, 0x40, 0x5c, 0x06), await RhinoHost.RunAsync(IO.lift(static () => new Subtraction().CustomIoId).OnUiThread()));

    [Fact]
    public async Task IdentityWithoutAnOwnerFailsAsMissing() =>
        Assert.Equal(GrasshopperReason.Missing, (await Expect<GrasshopperFault>.FailAsync(IO.lift(static () => new ValueParameter<double>().CustomIoId).OnUiThread())).Reason);

    // --- [GENERATORS]

    private static readonly Gen<Seq<(double Minuend, double Subtrahend, double Margin)>> Pairs =
        from count in Gen.Int[0, 12]
        from subtrahends in Gen.Int[-1_000_000, 1_000_000].Array[count]
        from margins in Gen.Int[0, 1_000_000].Array[count]
        select toSeq(subtrahends.Zip(margins, static (b, margin) => (Minuend: (b + margin) * ModelGrid.Model.Absolute, Subtrahend: b * ModelGrid.Model.Absolute, Margin: margin * ModelGrid.Model.Absolute))).Strict();
}
