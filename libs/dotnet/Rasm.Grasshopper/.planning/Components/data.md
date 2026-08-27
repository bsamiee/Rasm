# [RASM_GRASSHOPPER_DATA]

`GardenData` is the GH2 data boundary: one transfer policy carries item, pear, twig, and tree topology across `IDataAccess`; typed ingress rows absorb array and assistant reads; and the `Garden` algebra owns promotion, metadata retagging, expression application, and tree-wise folds. `Coerce` resolves host objects through scope-ranked broker rows, the merit-scored conversion server, and the geometry brokers' native discriminants. Broker participation composes the canonical `Lease<T>` ownership contract, and `HostUnits` projects host tolerance evidence directly into the canonical domain `Context`.

## [01]-[INDEX]

- [02]-[FAULT_AND_NOTICE]: `GhFault` family and the `Severity`/`Notice` diagnostic vocabulary
- [03]-[TRANSFER]: topology transfer, array and assistant ingress, metadata retagging, and typed `Garden` folds
- [04]-[CONVERSION]: scope-ranked broker rows, leased participation, typed conversion, and geometry discriminants
- [05]-[HOST_CONTEXT]: tolerance and unit capture projected into the canonical domain context

## [02]-[FAULT_AND_NOTICE]

- Owner: `GhFault` is the direct Components boundary family, and `Notice` recursively projects standard `ManyErrors` onto `IDataAccess`.
- Cases: `Absent | ContractRefused | Conversion | Registration | Overdue`, carrying the compact `[FaultCase]` ordinals `0..4` on `FaultBand.Grasshopper`.
- Entry: `Op.Catch` absorbs value-returning and void host calls and threads the exact execution token when cancellation is possible; `Notice.Fan(Error)` emits each `ManyErrors` leaf with its optional generated code.
- Packages: `Rasm.Domain` (`Fault`, `KernelFault`, `Op`, `OrDefault()`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new crossing cause is one fault case; a new document message channel is one `Severity` row.
- Boundary: `Op.Catch` preserves unknown host exceptions and recognizes cancellation only from its requested execution token.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Components;
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GhContract {
    public static readonly GhContract Pin = new("pin");
    public static readonly GhContract Component = new("component");
    public static readonly GhContract Object = new("object");
}

public sealed record GhSubject(string Name);
public sealed record GhEvidence(string Detail);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GhFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Grasshopper;
    private GhFault(string message) => Message = message;

    public sealed override string Message { get; }

    [FaultCase(0)]
    public sealed partial record Absent(GhSubject Subject)
        : GhFault($"grasshopper subject absent during {Subject.Operation.Key}: {Subject.Name}");

    [FaultCase(1)]
    public sealed partial record ContractRefused(GhContract Contract, GhEvidence Evidence)
        : GhFault($"grasshopper {Contract.Key} contract refused during {Evidence.Operation.Key}: {Evidence.Detail}");

    [FaultCase(2)]
    public sealed partial record Conversion(string Source, string Target, string Detail)
        : GhFault($"grasshopper conversion refused during {Key.Key}: {Source} -> {Target}: {Detail}");

    [FaultCase(3)]
    public sealed partial record Registration(string Detail)
        : GhFault($"grasshopper registration refused during {Key.Key}: {Detail}");

    [FaultCase(4)]
    public sealed partial record Overdue(string Detail)
        : GhFault($"grasshopper operation overdue during {Key.Key}: {Detail}");
}

[SmartEnum]
public sealed partial class Severity {
    public static readonly Severity Remark = new(static (a, t, d, x) => a.AddRemark(t, d, x));
    public static readonly Severity Warning = new(static (a, t, d, x) => a.AddWarning(t, d, x));
    public static readonly Severity Error = new(static (a, t, d, x) => a.AddError(t, d, x));

    [UseDelegateFromConstructor]
    public partial void Emit(IDataAccess access, string title, string detail, Grasshopper2.Doc.MessageAction[] actions);
}

public sealed record Notice(
    Severity Severity, Option<int> Code, string Title, string Detail, Seq<Grasshopper2.Doc.MessageAction> Actions) {
    public static Notice Of(Error fault) => fault switch {
        Fault expected => new(Severity.Error, Some(expected.Identity.Code), expected.Identity.Case, expected.Message, []),
        _ => new(Severity.Error, None, fault.GetType().Name, fault.Message, []),
    };

    public static Seq<Notice> Fan(Error fault) => fault switch {
        ManyErrors held => toSeq(held.Errors).Bind(member => Fan(member)),
        _ => Seq1(Of(fault)),
    };

    public Unit Report(IDataAccess access) => fun(() => Severity.Emit(
        access,
        Code.Match(Some: code => $"{Title} ({code})", None: () => Title),
        Detail,
        [.. Actions]))();
}
```

## [03]-[TRANSFER]

- Owner: `Transfer<T>` is the topology union for pin payloads; `Retention` owns metadata preservation or reprojection across every topology; `HostRead<T>` is the ONE typed ingress row over every out-parameter `IDataAccess` read — the array family and the assistant family were two parallel delegate-plus-wrapper shapes over one fact (a host read that answers a value and a bool), so one `HostIngress<T>` delegate and one row type carry both, an assistant pair riding as `HostRead<Assisted<TValue, TAssistant>>` whose row lambda fuses the host's two out-parameters.
- Cases: `Item` carries a bare value with its `MetaData`; `OfPear`, `OfTwig`, and `OfTree` carry the host carriers verbatim.
- Entry: `GardenData.Read<T>` discriminates depth by `PinAccess`; `GardenData.Read<T>(access, pin, HostRead<T>)` consumes any typed ingress row — the `ReadArray`/`ReadAssistant` arity twins collapse onto it; `Write<T>` retags pear metadata before emission; `AsTree`, `Zip`, `Amend`, `Evaluate`, and `ConvertTwig` lift the host algebra onto `Fin`.
- Law: every fold lands on `Fin` — an absent pin is `GhFault.Absent`, while `Op.Catch` retains a raised host exception as its original `Error`.
- Packages: `Grasshopper2` `Garden`, `Tree<T>`/`Twig<T>`/`Pear<T>`, and `MetaData` are the composed algebra; no local tree walker exists beside them.
- Growth: a new topology the host admits is one `Transfer<T>` case with one arm per fold.
- Boundary: presence law remains the pin's declared host `Requirement`; a failed ingress becomes `GhFault.Absent`, and all metadata reconstruction uses the `Pear<T>.Create(T, MetaData)` mint.
- Law: `GetTransform(int, out Transform)`/`GetQuaternion(int, out Quaternion)` are the host's own dedicated typed reads and `Read<T>` composes them by preference where the target type matches — the host publishes them beside the generic path precisely because they own their conversion, so routing those two targets through `GetPear<T>` bets on an equivalence the host never states.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Components;
using Grasshopper2.Data;
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Transfer<T> {
    private Transfer() { }

    public sealed record Item(T Value, MetaData Meta) : Transfer<T>;
    public sealed record OfPear(Pear<T> Pear) : Transfer<T>;
    public sealed record OfTwig(Twig<T> Twig) : Transfer<T>;
    public sealed record OfTree(Tree<T> Tree) : Transfer<T>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Retention {
    private Retention() { }

    public sealed record Preserve : Retention;
    public sealed record Retag(Func<MetaData, MetaData> Project) : Retention;

    public MetaData Applied(MetaData incoming) => Switch(
        state: incoming,
        preserve: static (meta, _) => meta,
        retag: static (meta, row) => row.Project(meta));
}

public delegate bool HostIngress<T>(IDataAccess access, int pin, out T value);

public readonly record struct HostRead<T>(HostIngress<T> Ingress);

public sealed record Assisted<TValue, TAssistant>(TValue Value, TAssistant Assistant);

public static class HostReads {
    public static HostRead<T[]> Items<T>() => new(static (IDataAccess access, int pin, out T[] values) =>
        access.GetItemArray(pin, out values));

    public static readonly HostRead<IPear[]> Pears = new(static (IDataAccess access, int pin, out IPear[] values) =>
        access.GetIPears(pin, out values));

    public static readonly HostRead<bool[]> Nulls = new(static (IDataAccess access, int pin, out bool[] values) =>
        access.GetNullArray(pin, out values));

    public static readonly HostRead<MetaData[]> Meta = new(static (IDataAccess access, int pin, out MetaData[] values) =>
        access.GetMetaArray(pin, out values));

    public static readonly HostRead<Assisted<object, Grasshopper2.Types.Assistant.ITypeAssistant>> Item = new(
        static (IDataAccess access, int pin, out Assisted<object, Grasshopper2.Types.Assistant.ITypeAssistant> value) => {
            bool held = access.GetItemWithTypeAssistant(pin, out object item, out Grasshopper2.Types.Assistant.ITypeAssistant assistant);
            value = new(Value: item, Assistant: assistant);
            return held;
        });

    public static readonly HostRead<Assisted<IPear, Grasshopper2.Types.Assistant.ITypeAssistant>> Pear = new(
        static (IDataAccess access, int pin, out Assisted<IPear, Grasshopper2.Types.Assistant.ITypeAssistant> value) => {
            bool held = access.GetIPearWithTypeAssistant(pin, out IPear item, out Grasshopper2.Types.Assistant.ITypeAssistant assistant);
            value = new(Value: item, Assistant: assistant);
            return held;
        });

    public static readonly HostRead<Assisted<object, Grasshopper2.Types.Assistant.ICurveAssistant>> Curve = new(
        static (IDataAccess access, int pin, out Assisted<object, Grasshopper2.Types.Assistant.ICurveAssistant> value) => {
            bool held = access.GetItemWithCurveAssistant(pin, out object item, out Grasshopper2.Types.Assistant.ICurveAssistant assistant);
            value = new(Value: item, Assistant: assistant);
            return held;
        });

    public static readonly HostRead<Assisted<object, Grasshopper2.Types.Assistant.ISurfaceAssistant>> Surface = new(
        static (IDataAccess access, int pin, out Assisted<object, Grasshopper2.Types.Assistant.ISurfaceAssistant> value) => {
            bool held = access.GetItemWithSurfaceAssistant(pin, out object item, out Grasshopper2.Types.Assistant.ISurfaceAssistant assistant);
            value = new(Value: item, Assistant: assistant);
            return held;
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class GardenData {
    public static Fin<Transfer<T>> Read<T>(IDataAccess access, int pin, PinAccess depth) =>
        depth.Switch(
            state: (Access: access, Pin: pin),
            item: static held => held.Access.GetPear<T>(held.Pin, out Pear<T> pear)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfPear(pear))
                : Missing<Transfer<T>>(held.Pin, held.Key),
            twig: static held => held.Access.GetTwig<T>(held.Pin, out Twig<T> twig)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfTwig(twig))
                : Missing<Transfer<T>>(held.Pin, held.Key),
            tree: static held => held.Access.GetTree<T>(held.Pin, out Tree<T> tree)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfTree(tree))
                : Missing<Transfer<T>>(held.Pin, held.Key));

    public static Fin<T> Read<T>(IDataAccess access, int pin, HostRead<T> read) =>
        read.Ingress(access, pin, out T value)
            ? Fin.Succ(value)
            : Missing<T>(pin);

    public static Fin<Unit> Write<T>(IDataAccess access, int pin, Transfer<T> payload, Retention retention) =>
        payload.Switch(
            state: (Access: access, Pin: pin, Retention: retention),
            item: static (held, item) => Try.lift(() => held.Access.SetItem(held.Pin, item.Value!, held.Retention.Applied(item.Meta))).Run().Bind(static inner => inner),
            ofPear: static (held, row) => Try.lift(() => held.Access.SetPear(held.Pin, Retag(row.Pear, held.Retention))).Run().Bind(static inner => inner),
            ofTwig: static (held, row) => Try.lift(() => held.Access.SetTwig(
                held.Pin,
                held.Retention is Retention.Preserve
                    ? row.Twig
                    : Garden.TwigFromPears(row.Twig.Pears.Select(pear => Retag(pear, held.Retention))))).Run().Bind(static inner => inner),
            ofTree: static (held, row) => Try.lift(() => held.Access.SetTree(
                held.Pin,
                held.Retention is Retention.Preserve
                    ? row.Tree
                    : Garden.PearWiseOp(row.Tree, pear => Retag(pear, held.Retention), CancellationToken.None))).Run().Bind(static inner => inner));

    public static Fin<Tree<T>> AsTree<T>(Transfer<T> payload) =>
        payload.Switch(
            state: key.OrDefault(),
            item: static (row) => Try.lift(() => Fin.Succ(Garden.TreeFromPears([Pear<T>.Create(row.Value, row.Meta)]))).Run().Bind(static inner => inner),
            ofPear: static (row) => Try.lift(() => Fin.Succ(Garden.TreeFromPears([row.Pear]))).Run().Bind(static inner => inner),
            ofTwig: static (row) => Try.lift(() => Fin.Succ(Garden.TreeFromTwigs([row.Twig]))).Run().Bind(static inner => inner),
            ofTree: static (_, row) => Fin.Succ(row.Tree));

    public static Fin<Tree<TOut>> Zip<TLeft, TRight, TOut>(
        Tree<TLeft> left, Tree<TRight> right, Func<TLeft, TRight, TOut> merge, CancellationToken cancel) =>
        key.OrDefault().Catch(() => Fin.Succ(Garden.PairWiseOp(left, right, merge, cancel)), cancel);

    public static Fin<Tree<T>> Amend<T>(Tree<T> tree, Func<Pear<T>, Pear<T>> project, CancellationToken cancel) =>
        key.OrDefault().Catch(() => Fin.Succ(Garden.PearWiseOp(tree, project, cancel)), cancel);

    public static Fin<(Twig<T> Twig, Grasshopper2.Data.IExpressionReport Report)> Evaluate<T>(
        Twig<T> twig,
        Grasshopper2.Expressions.Expression expression,
        Grasshopper2.Expressions.Resolver resolver) =>
        key.OrDefault().Catch(() => Fin.Succ((
            Twig: twig.Apply(expression, resolver, out Grasshopper2.Data.IExpressionReport report),
            Report: report)));

    public static Fin<Twig<TOut>> ConvertTwig<TIn, TOut>(
        Twig<TIn> twig, Grasshopper2.Types.Conversion.ConversionDelegate<TIn, TOut> convert,
        CancellationToken cancel, Grasshopper2.Data.ConversionRecord record) =>
        key.OrDefault().Catch(() => Fin.Succ(twig.Convert(convert, cancel, record)), cancel);

    private static Pear<T> Retag<T>(Pear<T> pear, Retention retention) =>
        pear is null ? pear : Pear<T>.Create(pear.Item, retention.Applied(pear.Meta));

    private static Fin<T> Missing<T>(int pin) => Fin.Fail<T>(new GhFault.Absent(new GhSubject($"pin:{pin}")));
}
```

## [04]-[CONVERSION]

- Owner: `Coerce` is the conversion fold: direct assignment admits first, the ROOT-OWNED `BrokerLedger` evaluates scope-ranked rows, and `ConversionServer` is the terminal generic route. `BrokerLedger` is an INSTANCE the composition root constructs and holds (`Platform/composition.md` row `[03]` — a process-global static registry on a library page was the seat defect), its row store an `AtomHashMap` whose per-key transitions replace the whole-map swap. `CurveShape` and `SurfaceShape` project the geometry brokers' returned `CurveType` and `SurfaceLikeType` discriminants without inferring a case from out-parameter defaults.
- Cases: `BrokerScope` closes at `Document` and `Plugin`; `BrokerRank` states that lower values run first; the enrolment ordinal orders rows stably within an equal scope and rank.
- Entry: `Coerce.To<TOut>(raw, ledger, scope, key)` is the one typed conversion entry; broker candidates settle by first success and LanguageExt combines plural failures without a package-local aggregate case.
- Auto: enrolment returns an owned `Lease<BrokerRegistration>`; disposing the lease revokes the row exactly once through the canonical resource contract.
- Growth: a new conversion route is one broker row; a new geometry family member is one union case with one probe arm.
- Boundary: the `BrokerLedger` per-key transitions and the cast-or-convert out-probes are the named boundary-kernel statement forms; interior code receives the typed value on `Fin`, and the ledger's ONE instance lives on `PlatformRoot.Brokers` — no page constructs a second.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BrokerScope {
    private BrokerScope() { }

    public sealed record Document(Guid DocumentId) : BrokerScope;
    public sealed record Plugin(Guid PluginId) : BrokerScope;

    public int Precedence => Switch(document: static _ => 0, plugin: static _ => 1);
}

public readonly record struct BrokerRank(uint Value) : IComparable<BrokerRank> {
    public int CompareTo(BrokerRank other) => Value.CompareTo(other.Value);
}

public sealed record BrokerRow(BrokerScope Scope, BrokerRank Rank, Type Source, Type Target, Func<object, Fin<object>> Convert);

public sealed record ConversionScope(Option<Guid> Document, Option<Guid> Plugin) {
    public static readonly ConversionScope Unscoped = new(None, None);

    public bool Admits(BrokerScope scope) => scope.Switch(
        state: this,
        document: static (held, row) => held.Document.Exists(id => id == row.DocumentId),
        plugin: static (held, row) => held.Plugin.Exists(id => id == row.PluginId));
}

public sealed class BrokerRegistration : IDisposable {
    private readonly BrokerLedger ledger;
    private readonly Atom<Option<Guid>> row;

    internal BrokerRegistration(BrokerLedger ledger, Guid row) =>
        (this.ledger, this.row) = (ledger, Atom(Some(row)));

    public void Dispose() => Cell.Take(row).Current.Iter(ledger.Revoke);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveShape {
    private CurveShape() { }

    public sealed record OfLine(Rhino.Geometry.Line Value) : CurveShape;
    public sealed record OfTriangle(Grasshopper2.Types.Shapes.Triangle Value) : CurveShape;
    public sealed record OfRectangle(Rhino.Geometry.Rectangle3d Value) : CurveShape;
    public sealed record OfPolyline(Rhino.Geometry.Polyline Value) : CurveShape;
    public sealed record OfCircle(Rhino.Geometry.Circle Value) : CurveShape;
    public sealed record OfArc(Rhino.Geometry.Arc Value) : CurveShape;
    public sealed record OfCurve(Rhino.Geometry.Curve Value) : CurveShape;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceShape {
    private SurfaceShape() { }

    public sealed record OfSurface(Rhino.Geometry.Surface Value) : SurfaceShape;
    public sealed record OfBrep(Rhino.Geometry.Brep Value) : SurfaceShape;
    public sealed record OfSubD(Rhino.Geometry.SubD Value) : SurfaceShape;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public sealed class BrokerLedger {
    private readonly AtomHashMap<Guid, (BrokerRow Row, long Ordinal)> rows = AtomHashMap<Guid, (BrokerRow, long)>();
    private readonly Atom<long> next = Atom(0L);

    public Lease<BrokerRegistration> Enroll(BrokerRow row) {
        Guid id = Guid.NewGuid();
        long ordinal = next.Swap(static held => held + 1);
        rows.Add(id, (row, ordinal));
        return new Lease<BrokerRegistration>.Owned(new BrokerRegistration(ledger: this, row: id));
    }

    internal Unit Revoke(Guid row) => ignore(rows.Remove(row));

    public Seq<BrokerRow> Resolved(Type source, Type target, ConversionScope scope) =>
        toSeq(rows.ToSeq()
            .Filter(entry => entry.Row.Source.IsAssignableFrom(source) && target.IsAssignableFrom(entry.Row.Target) && scope.Admits(entry.Row.Scope))
            .OrderBy(static entry => (entry.Row.Scope.Precedence, entry.Row.Rank.Value, entry.Ordinal)))
            .Map(static entry => entry.Row);
}

public static class Coerce {
    public static Fin<TOut> To<TOut>(object? raw, BrokerLedger ledger, ConversionScope scope) => raw switch {
        null => Fin.Fail<TOut>(new GhFault.Absent(new GhSubject(typeof(TOut).Name))),
        TOut direct => Fin.Succ(direct),
        _ => ledger.Resolved(raw.GetType(), typeof(TOut), scope)
            .Fold(Fin.Fail<TOut>(new GhFault.Conversion(raw.GetType().Name, typeof(TOut).Name, nameof(BrokerLedger))),
                (state, row) => state | Projected<TOut>(raw, row))
            .BindFail(brokerFault => Served<TOut>(raw)
                .MapFail(serveFault => Error.Many([brokerFault, serveFault]))),
    };

    public static Fin<CurveShape> CurveOf(object? raw) {
        return Optional(raw).ToFin(new GhFault.Absent(new GhSubject(nameof(CurveShape))))
            .Bind(held => Try.lift(() => CurveProbe(held)).Run().Bind(static inner => inner));
    }

    public static Fin<SurfaceShape> SurfaceOf(object? raw) {
        return Optional(raw).ToFin(new GhFault.Absent(new GhSubject(nameof(SurfaceShape))))
            .Bind(held => Try.lift(() => SurfaceProbe(held)).Run().Bind(static inner => inner));
    }

    private static Fin<TOut> Served<TOut>(object raw) =>
        Try.lift(() =>
            Grasshopper2.Types.Conversion.ConversionServer.Convert(raw, typeof(TOut), out object converted, out _, out string detail)
                ? converted is TOut value
                    ? Fin.Succ(value)
                    : Fin.Fail<TOut>(new GhFault.Conversion(converted?.GetType().Name ?? "<null>", typeof(TOut).Name, detail))
                : Fin.Fail<TOut>(new GhFault.Conversion(raw.GetType().Name, typeof(TOut).Name, detail))).Run().Bind(static inner => inner);

    private static Fin<TOut> Projected<TOut>(object raw, BrokerRow row) =>
        typeof(TOut).IsAssignableFrom(row.Target)
            ? row.Convert(raw).Bind(value => value is TOut projected
                ? Fin.Succ(projected)
                : Fin.Fail<TOut>(new GhFault.Conversion(value?.GetType().Name ?? "<null>",
                    typeof(TOut).Name,
                    row.Target.Name)))
            : Fin.Fail<TOut>(new GhFault.Conversion(row.Target.Name,
                typeof(TOut).Name,
                nameof(BrokerRow.Target)));

    private static Fin<CurveShape> CurveProbe(object raw) =>
        Grasshopper2.Parameters.Standard.CurveBroker.CastOrConvert(
            raw, out Rhino.Geometry.Line line, out Grasshopper2.Types.Shapes.Triangle triangle, out Rhino.Geometry.Rectangle3d rectangle,
            out Rhino.Geometry.Polyline polyline, out Rhino.Geometry.Circle circle, out Rhino.Geometry.Arc arc, out Rhino.Geometry.Curve curve) switch {
            Grasshopper2.Parameters.Standard.CurveType.Line => new CurveShape.OfLine(line),
            Grasshopper2.Parameters.Standard.CurveType.Triangle => new CurveShape.OfTriangle(triangle),
            Grasshopper2.Parameters.Standard.CurveType.Rectangle => new CurveShape.OfRectangle(rectangle),
            Grasshopper2.Parameters.Standard.CurveType.Polyline => new CurveShape.OfPolyline(polyline),
            Grasshopper2.Parameters.Standard.CurveType.Circle => new CurveShape.OfCircle(circle),
            Grasshopper2.Parameters.Standard.CurveType.Arc => new CurveShape.OfArc(arc),
            Grasshopper2.Parameters.Standard.CurveType.Curve => new CurveShape.OfCurve(curve),
            _ => Fin.Fail<CurveShape>(new GhFault.Conversion(raw.GetType().Name,
                nameof(CurveShape),
                nameof(Grasshopper2.Parameters.Standard.CurveBroker))),
        };

    private static Fin<SurfaceShape> SurfaceProbe(object raw) =>
        Grasshopper2.Parameters.Standard.SurfaceBroker.CastOrConvert(
            raw, out Rhino.Geometry.Surface surface, out Rhino.Geometry.Brep brep, out Rhino.Geometry.SubD subd) switch {
            Grasshopper2.Parameters.Standard.SurfaceLikeType.Surf => new SurfaceShape.OfSurface(surface),
            Grasshopper2.Parameters.Standard.SurfaceLikeType.Brep => new SurfaceShape.OfBrep(brep),
            Grasshopper2.Parameters.Standard.SurfaceLikeType.SubD => new SurfaceShape.OfSubD(subd),
            _ => Fin.Fail<SurfaceShape>(new GhFault.Conversion(raw.GetType().Name,
                nameof(SurfaceShape),
                nameof(Grasshopper2.Parameters.Standard.SurfaceBroker))),
        };

}
```

## [05]-[HOST_CONTEXT]

- Owner: `HostUnits` captures the live tolerance triad and unit system once, then projects those scalars through `Rasm.Domain.Context.Of`; the host record never owns an independent tolerance policy.
- Entry: `HostUnits.Of(IDataAccess)` captures host evidence; `Context` performs canonical domain admission; `ScalingTo` exposes the host's live scale query for boundary-only conversions.
- Law: the unit carrier is whatever the host publishes and nothing richer — `IDataAccess.GetUnitSystem` answers a bare `UnitSystem`, so the projection takes the kernel's `UnitSystem` admission arm and a CUSTOM-unit document refuses at that gate, because a custom regime's scale and name live only on a `LengthUnit` this access surface never yields. Synthesizing a meters-per-unit factor from the host's scale query to force the admission mints a unit identity no host fact carries, so the refusal is the honest terminal until GH2 publishes the length unit.
- Boundary: every kernel call consumes the admitted `Context`, so raw GH2 tolerance values stop at this projection. `ScalingTo` is the HOST's answer to a host question and never a second cross-context scale owner — a kernel-space rescale is `ModelUnit.ScaleTo` off the admitted `Context.Unit`, and a call that reaches for the host factor to convert kernel measures forks the one scale owner.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Components;
using Rasm.Domain;
using Rhino;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record HostUnits(double Absolute, double Relative, Grasshopper2.Types.Numeric.Angle Angle, UnitSystem Units) {
    public static Fin<HostUnits> Of(IDataAccess access) =>
        access.GetTolerance(out double absolute, out double relative)
        && access.GetTolerance(out Grasshopper2.Types.Numeric.Angle angle)
        && access.GetUnitSystem(out UnitSystem units)
            ? Fin.Succ(new HostUnits(absolute, relative, angle, units))
            : Fin.Fail<HostUnits>(new GhFault.Absent(new GhSubject(nameof(HostUnits))));

    public Validation<Error, Context> Context =>
        Rasm.Domain.Context.Of(Absolute, Relative, Angle.Radians, Units);

    public Fin<double> ScalingTo(IDataAccess access, Rhino.UnitSystem target) =>
        access.GetUnitScaling(target, out double factor)
            ? Fin.Succ(factor)
            : Fin.Fail<double>(new GhFault.Absent(new GhSubject(nameof(ScalingTo))));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
