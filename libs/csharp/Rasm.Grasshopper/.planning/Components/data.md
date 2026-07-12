# [RASM_GRASSHOPPER_DATA]

`GardenData` is the GH2 data boundary: one transfer policy carries item, pear, twig, and tree topology across `IDataAccess`; typed ingress rows absorb array and assistant reads; and the `Garden` algebra owns promotion, metadata retagging, expression application, and tree-wise folds. `Coerce` resolves host objects through scope-ranked broker rows, the merit-scored conversion server, and the geometry brokers' native discriminants. Broker participation composes the canonical `Lease<T>` ownership rail, and `HostUnits` projects host tolerance evidence directly into the canonical domain `Context`.

## [01]-[INDEX]

- [02]-[FAULT_AND_NOTICE]: the `GhFault` family, the `Hosted` funnel, and the `Severity`/`Notice` diagnostic vocabulary
- [03]-[TRANSFER]: topology transfer, array and assistant ingress, metadata retagging, and typed `Garden` folds
- [04]-[CONVERSION]: scope-ranked broker rows, leased participation, conversion receipts, and geometry discriminants
- [05]-[HOST_CONTEXT]: tolerance and unit capture projected into the canonical domain context

## [02]-[FAULT_AND_NOTICE]

- Owner: `GhFault` is the closed Components boundary-fault family; every case carries its `Op`, detail, and stable code through the kernel `Expected` bridge, while `Aggregate` supplies `Semigroup<GhFault>` accumulation. `Hosted` is the exception funnel for host calls, and `Severity` plus `Notice` project failures onto `IDataAccess`.
- Cases: `Text`, `Absent`, `Refused`, `Conversion`, `Host`, `Registration`, and the `Aggregate` fold case.
- Entry: `Hosted.Bound` absorbs value-returning and void host calls by argument shape; every public entry in the sub-domain accepts `Op? key = null` and resolves through the kernel `OrDefault()`, threading the resolved key into every fault it mints; `Notice.Of(Error)` projects any rail failure into a document-visible error notice.
- Packages: `Rasm.Domain` carries `Expected`, `Op`, `OrDefault()`, and `Fault.Cancelled`; `LanguageExt.Core` carries `Fin`, `Validation`, and `Try`; `Thinktecture.Runtime.Extensions` generates the union and the severity rows.
- Growth: a new crossing cause is one fault case; a new document message channel is one `Severity` row.
- Boundary: a captured `OperationCanceledException` remains the kernel `Fault.Cancelled`; the `Try` capture inside `Hosted` is the platform exception seam.

```csharp signature
namespace Rasm.Grasshopper.Components;

// --- [ERRORS] ----------------------------------------------------------------------------

[Union]
public abstract partial record GhFault : Expected, Semigroup<GhFault> {
    private GhFault(Op key, string detail, int code) => (Key, Detail, Code) = (key, detail, code);

    public Op Key { get; }

    public string Detail { get; }

    public sealed override int Code { get; }

    public sealed override string Category => "Grasshopper";

    public override string Message => $"Grasshopper operation '{Key}': {Detail}";

    public sealed record Text : GhFault { public Text(Op key, string detail) : base(key, detail, 4600) { } }
    public sealed record Absent : GhFault { public Absent(Op key, string detail) : base(key, detail, 4601) { } }
    public sealed record Refused : GhFault { public Refused(Op key, string detail) : base(key, detail, 4602) { } }
    public sealed record Conversion : GhFault {
        public Conversion(Op key, string source, string target, string detail) : base(key, $"{source}->{target}: {detail}", 4603) =>
            (Source, Target) = (source, target);
        public string Source { get; }
        public string Target { get; }
    }
    public sealed record Host : GhFault { public Host(Op key, string detail) : base(key, detail, 4604) { } }
    public sealed record Registration : GhFault { public Registration(Op key, string detail) : base(key, detail, 4605) { } }
    public sealed record Aggregate : GhFault {
        public Aggregate(Op key, Seq<GhFault> faults) : base(key, $"{faults.Count} faults", 4699) => Faults = faults;
        public Seq<GhFault> Faults { get; }
    }

    public GhFault Combine(GhFault rhs) => (this, rhs) switch {
        (Aggregate left, Aggregate right) => new Aggregate(left.Key, left.Faults + right.Faults),
        (Aggregate left, _) => new Aggregate(left.Key, left.Faults.Add(rhs)),
        (_, Aggregate right) => new Aggregate(Key, this.Cons(right.Faults)),
        _ => new Aggregate(Key, Seq(this, rhs)),
    };
}

// --- [SERVICES] --------------------------------------------------------------------------

public static class Hosted {
    public static Fin<T> Bound<T>(Func<T> call, Op? key = null) =>
        Try.lift(() => Fin.Succ(call()))
            .Run()
            .MapFail(error => error.HasException<OperationCanceledException>()
                ? (Error)new Fault.Cancelled()
                : new GhFault.Host(key.OrDefault(), error.Message))
            .Bind(static result => result);

    public static Fin<Unit> Bound(Action call, Op? key = null) =>
        Bound(() => { call(); return unit; }, key);
}

[SmartEnum]
public sealed partial class Severity {
    public static readonly Severity Remark = new(static (a, t, d, x) => a.AddRemark(t, d, x));
    public static readonly Severity Warning = new(static (a, t, d, x) => a.AddWarning(t, d, x));
    public static readonly Severity Error = new(static (a, t, d, x) => a.AddError(t, d, x));

    [UseDelegateFromConstructor]
    public partial void Emit(IDataAccess access, string title, string detail, Grasshopper2.Doc.MessageAction[] actions);
}

public sealed record Notice(Severity Severity, string Title, string Detail, Seq<Grasshopper2.Doc.MessageAction> Actions) {
    public static Notice Of(Error fault) => new(Severity.Error, fault.GetType().Name, fault.Message, []);

    public Unit Report(IDataAccess access) => fun(() => Severity.Emit(access, Title, Detail, [.. Actions]))();
}
```

## [03]-[TRANSFER]

- Owner: `Transfer<T>` is the topology union for pin payloads; `Retention` owns metadata preservation or reprojection across every topology; `ArrayIngress<T>` and `AssistantIngress<TValue, TAssistant>` parameterize the verified `IDataAccess` array and assistant families without multiplying read entries.
- Cases: `Item` carries a bare value with its `MetaData`; `OfPear`, `OfTwig`, and `OfTree` carry the host carriers verbatim.
- Entry: `GardenData.Read<T>` discriminates depth by `PinAccess`; `ReadArray` and `ReadAssistant` consume typed ingress rows; `Write<T>` retags pear metadata before emission; `AsTree`, `Zip`, `Amend`, `Evaluate`, and `ConvertTwig` lift the host algebra onto `Fin`.
- Receipt: every fold lands on `Fin` with a `GhFault` cause — an absent pin is `Absent`, a host raise is `Host` through the funnel.
- Packages: `Grasshopper2` `Garden`, `Tree<T>`/`Twig<T>`/`Pear<T>`, and `MetaData` are the composed algebra; no local tree walker exists beside them.
- Growth: a new topology the host admits is one `Transfer<T>` case plus one arm per fold.
- Boundary: presence law remains the pin's declared host `Requirement`; a failed ingress becomes `GhFault.Absent`, and all metadata reconstruction uses the verified `Pear<T>.Create(T, MetaData)` mint.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

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

public delegate bool ArrayIngress<T>(IDataAccess access, int pin, out T[] values);

public readonly record struct ArrayRead<T>(ArrayIngress<T> Ingress);

public static class ArrayReads {
    public static ArrayRead<T> Items<T>() => new(static (IDataAccess access, int pin, out T[] values) =>
        access.GetItemArray(pin, out values));

    public static readonly ArrayRead<IPear> Pears = new(static (IDataAccess access, int pin, out IPear[] values) =>
        access.GetIPears(pin, out values));

    public static readonly ArrayRead<bool> Nulls = new(static (IDataAccess access, int pin, out bool[] values) =>
        access.GetNullArray(pin, out values));

    public static readonly ArrayRead<MetaData> Meta = new(static (IDataAccess access, int pin, out MetaData[] values) =>
        access.GetMetaArray(pin, out values));
}

public delegate bool AssistantIngress<TValue, TAssistant>(
    IDataAccess access,
    int pin,
    out TValue value,
    out TAssistant assistant);

public readonly record struct AssistantRead<TValue, TAssistant>(AssistantIngress<TValue, TAssistant> Ingress);

public sealed record Assisted<TValue, TAssistant>(TValue Value, TAssistant Assistant);

public static class AssistantReads {
    public static readonly AssistantRead<object, Grasshopper2.Types.Assistant.ITypeAssistant> Item = new(
        static (IDataAccess access, int pin, out object value, out Grasshopper2.Types.Assistant.ITypeAssistant assistant) =>
            access.GetItemWithTypeAssistant(pin, out value, out assistant));

    public static readonly AssistantRead<IPear, Grasshopper2.Types.Assistant.ITypeAssistant> Pear = new(
        static (IDataAccess access, int pin, out IPear value, out Grasshopper2.Types.Assistant.ITypeAssistant assistant) =>
            access.GetIPearWithTypeAssistant(pin, out value, out assistant));

    public static readonly AssistantRead<object, Grasshopper2.Types.Assistant.ICurveAssistant> Curve = new(
        static (IDataAccess access, int pin, out object value, out Grasshopper2.Types.Assistant.ICurveAssistant assistant) =>
            access.GetItemWithCurveAssistant(pin, out value, out assistant));

    public static readonly AssistantRead<object, Grasshopper2.Types.Assistant.ISurfaceAssistant> Surface = new(
        static (IDataAccess access, int pin, out object value, out Grasshopper2.Types.Assistant.ISurfaceAssistant assistant) =>
            access.GetItemWithSurfaceAssistant(pin, out value, out assistant));
}

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class GardenData {
    public static Fin<Transfer<T>> Read<T>(IDataAccess access, int pin, PinAccess depth, Op? key = null) =>
        depth.Switch(
            state: (Access: access, Pin: pin, Key: key.OrDefault()),
            item: static held => held.Access.GetPear<T>(held.Pin, out Pear<T> pear)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfPear(pear))
                : Missing<Transfer<T>>(held.Pin, held.Key),
            twig: static held => held.Access.GetTwig<T>(held.Pin, out Twig<T> twig)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfTwig(twig))
                : Missing<Transfer<T>>(held.Pin, held.Key),
            tree: static held => held.Access.GetTree<T>(held.Pin, out Tree<T> tree)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfTree(tree))
                : Missing<Transfer<T>>(held.Pin, held.Key));

    public static Fin<Arr<T>> ReadArray<T>(IDataAccess access, int pin, ArrayRead<T> read, Op? key = null) =>
        read.Ingress(access, pin, out T[] values)
            ? Fin.Succ(new Arr<T>(values))
            : Missing<Arr<T>>(pin, key.OrDefault());

    public static Fin<Assisted<TValue, TAssistant>> ReadAssistant<TValue, TAssistant>(
        IDataAccess access,
        int pin,
        AssistantRead<TValue, TAssistant> read,
        Op? key = null) =>
        read.Ingress(access, pin, out TValue value, out TAssistant assistant)
            ? Fin.Succ(new Assisted<TValue, TAssistant>(value, assistant))
            : Missing<Assisted<TValue, TAssistant>>(pin, key.OrDefault());

    public static Fin<Unit> Write<T>(IDataAccess access, int pin, Transfer<T> payload, Retention retention, Op? key = null) =>
        payload.Switch(
            state: (Access: access, Pin: pin, Retention: retention, Key: key.OrDefault()),
            item: static (held, item) => Hosted.Bound(() => held.Access.SetItem(held.Pin, item.Value!, held.Retention.Applied(item.Meta)), held.Key),
            ofPear: static (held, row) => Hosted.Bound(() => held.Access.SetPear(held.Pin, Retag(row.Pear, held.Retention)), held.Key),
            ofTwig: static (held, row) => Hosted.Bound(() => held.Access.SetTwig(
                held.Pin,
                held.Retention is Retention.Preserve
                    ? row.Twig
                    : Garden.TwigFromPears(row.Twig.Pears.Select(pear => Retag(pear, held.Retention)))), held.Key),
            ofTree: static (held, row) => Hosted.Bound(() => held.Access.SetTree(
                held.Pin,
                held.Retention is Retention.Preserve
                    ? row.Tree
                    : Garden.PearWiseOp(row.Tree, pear => Retag(pear, held.Retention))), held.Key));

    public static Fin<Tree<T>> AsTree<T>(Transfer<T> payload, Op? key = null) =>
        payload.Switch(
            state: key.OrDefault(),
            item: static (op, row) => Hosted.Bound(() => Garden.TreeFromPears([Pear<T>.Create(row.Value, row.Meta)]), op),
            ofPear: static (op, row) => Hosted.Bound(() => Garden.TreeFromPears([row.Pear]), op),
            ofTwig: static (op, row) => Hosted.Bound(() => Garden.TreeFromTwigs([row.Twig]), op),
            ofTree: static (_, row) => Fin.Succ(row.Tree));

    public static Fin<Tree<TOut>> Zip<TLeft, TRight, TOut>(
        Tree<TLeft> left, Tree<TRight> right, Func<TLeft, TRight, TOut> merge, CancellationToken cancel, Op? key = null) =>
        Hosted.Bound(() => Garden.PairWiseOp(left, right, merge, cancel), key.OrDefault());

    public static Fin<Tree<T>> Amend<T>(Tree<T> tree, Func<Pear<T>, Pear<T>> project, CancellationToken cancel, Op? key = null) =>
        Hosted.Bound(() => Garden.PearWiseOp(tree, project, cancel), key.OrDefault());

    public static Fin<(Twig<T> Twig, Grasshopper2.Data.IExpressionReport Report)> Evaluate<T>(
        Twig<T> twig,
        Grasshopper2.Expressions.Expression expression,
        Grasshopper2.Expressions.Resolver resolver,
        Op? key = null) =>
        Hosted.Bound(() => (
            Twig: twig.Apply(expression, resolver, out Grasshopper2.Data.IExpressionReport report),
            Report: report), key.OrDefault());

    public static Fin<Twig<TOut>> ConvertTwig<TIn, TOut>(
        Twig<TIn> twig, Grasshopper2.Types.Conversion.ConversionDelegate<TIn, TOut> convert,
        CancellationToken cancel, Grasshopper2.Data.ConversionRecord record, Op? key = null) =>
        Hosted.Bound(() => twig.Convert(convert, cancel, record), key.OrDefault());

    private static Pear<T> Retag<T>(Pear<T> pear, Retention retention) =>
        pear is null ? pear : Pear<T>.Create(pear.Item, retention.Applied(pear.Meta));

    private static Fin<T> Missing<T>(int pin, Op key) => Fin.Fail<T>(new GhFault.Absent(key, $"pin:{pin}"));
}
```

## [04]-[CONVERSION]

- Owner: `Coerce` is the conversion fold: direct assignment admits first, `BrokerLedger` evaluates scope-ranked rows, and `ConversionServer` is the terminal generic route. `CurveShape` and `SurfaceShape` project the geometry brokers' returned `CurveType` and `SurfaceLikeType` discriminants without inferring a case from out-parameter defaults.
- Cases: `BrokerScope` closes at `Document` and `Plugin`; `BrokerRank` states that lower values run first; the enrolment ordinal orders rows stably within an equal scope and rank.
- Entry: `Coerce.To<TOut>` is the one typed conversion entry over any raw host object with a null admitted as `GhFault.Absent` before any probe runs; `CurveOf` and `SurfaceOf` are the geometry-family probes under the same null gate.
- Receipt: every success carries a `ConversionReceipt` naming source, target, route, and the host `Merit` where the server resolved it.
- Auto: enrolment returns an owned `Lease<BrokerRegistration>`; disposing the lease revokes the row exactly once through the canonical resource rail.
- Growth: a new conversion route is one broker row; a new geometry family member is one union case plus one probe arm.
- Boundary: the `BrokerLedger` swap bodies and the cast-or-convert out-probes are the named boundary-kernel statement seam; interior code receives typed carriers and receipts only.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

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
        document: static (held, row) => held.Document.Map(id => id == row.DocumentId).IfNone(false),
        plugin: static (held, row) => held.Plugin.Map(id => id == row.PluginId).IfNone(false));
}

public sealed record ConversionReceipt(string Source, string Target, string Route, Option<Grasshopper2.Types.Conversion.Merit> Merit);

public sealed class BrokerRegistration : IDisposable {
    private readonly Guid row;
    private int active = 1;

    internal BrokerRegistration(Guid row) => this.row = row;

    public void Dispose() {
        if (Interlocked.Exchange(ref active, 0) == 1) { BrokerLedger.Revoke(row); }
    }
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

// --- [COMPOSITION] -----------------------------------------------------------------------

public static class BrokerLedger {
    private sealed record Ledger(HashMap<Guid, (BrokerRow Row, long Ordinal)> Rows, long Next);

    private static readonly Atom<Ledger> Cell = Atom(new Ledger(HashMap<Guid, (BrokerRow, long)>(), 0));

    public static Lease<BrokerRegistration> Enroll(BrokerRow row) {
        Guid id = Guid.NewGuid();
        Cell.Swap(ledger => new Ledger(ledger.Rows.Add(id, (row, ledger.Next)), ledger.Next + 1));
        return new Lease<BrokerRegistration>.Owned(new BrokerRegistration(id));
    }

    internal static Unit Revoke(Guid row) =>
        ignore(Cell.Swap(ledger => ledger with { Rows = ledger.Rows.Remove(row) }));

    public static Seq<BrokerRow> Resolved(Type source, Type target, ConversionScope scope) =>
        toSeq(Cell.Value.Rows.Values
            .Filter(entry => entry.Row.Source.IsAssignableFrom(source) && target.IsAssignableFrom(entry.Row.Target) && scope.Admits(entry.Row.Scope))
            .OrderBy(static entry => (entry.Row.Scope.Precedence, entry.Row.Rank.Value, entry.Ordinal)))
            .Map(static entry => entry.Row);
}

public static class Coerce {
    public static Fin<(TOut Value, ConversionReceipt Receipt)> To<TOut>(object? raw, ConversionScope scope, Op? key = null) => raw switch {
        null => Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Absent(key.OrDefault(), typeof(TOut).Name)),
        TOut direct => Fin.Succ((direct, Receipt<TOut>(raw, nameof(Type), None))),
        _ => BrokerLedger.Resolved(raw.GetType(), typeof(TOut), scope)
            .Fold(Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Conversion(key.OrDefault(), raw.GetType().Name, typeof(TOut).Name, nameof(BrokerLedger))),
                (state, row) => state.IsSucc ? state : Projected<TOut>(raw, row, key.OrDefault()))
            .BindFail(_ => Served<TOut>(raw, key.OrDefault())),
    };

    public static Fin<CurveShape> CurveOf(object? raw, Op? key = null) =>
        Optional(raw).ToFin(new GhFault.Absent(key.OrDefault(), nameof(CurveShape)))
            .Bind(held => Hosted.Bound(() => CurveProbe(held, key.OrDefault()), key.OrDefault()).Bind(identity));

    public static Fin<SurfaceShape> SurfaceOf(object? raw, Op? key = null) =>
        Optional(raw).ToFin(new GhFault.Absent(key.OrDefault(), nameof(SurfaceShape)))
            .Bind(held => Hosted.Bound(() => SurfaceProbe(held, key.OrDefault()), key.OrDefault()).Bind(identity));

    private static Fin<(TOut Value, ConversionReceipt Receipt)> Served<TOut>(object raw, Op key) =>
        Hosted.Bound(() =>
            Grasshopper2.Types.Conversion.ConversionServer.Convert(raw, typeof(TOut), out object converted, out Grasshopper2.Types.Conversion.Merit merit, out string detail)
                ? converted is TOut value
                    ? Fin.Succ((value, Receipt<TOut>(raw, nameof(Grasshopper2.Types.Conversion.ConversionServer), Optional(merit))))
                    : Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Conversion(key, converted?.GetType().Name ?? "<null>", typeof(TOut).Name, detail))
                : Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Conversion(key, raw.GetType().Name, typeof(TOut).Name, detail)), key)
            .Bind(identity);

    private static Fin<(TOut Value, ConversionReceipt Receipt)> Projected<TOut>(object raw, BrokerRow row, Op key) =>
        typeof(TOut).IsAssignableFrom(row.Target)
            ? row.Convert(raw).Bind(value => value is TOut projected
                ? Fin.Succ((projected, Receipt<TOut>(raw, nameof(BrokerRow), None)))
                : Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Conversion(
                    key,
                    value?.GetType().Name ?? "<null>",
                    typeof(TOut).Name,
                    row.Target.Name)))
            : Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Conversion(
                key,
                row.Target.Name,
                typeof(TOut).Name,
                nameof(BrokerRow.Target)));

    private static Fin<CurveShape> CurveProbe(object raw, Op key) =>
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
            _ => Fin.Fail<CurveShape>(new GhFault.Conversion(
                key,
                raw.GetType().Name,
                nameof(CurveShape),
                nameof(Grasshopper2.Parameters.Standard.CurveBroker))),
        };

    private static Fin<SurfaceShape> SurfaceProbe(object raw, Op key) =>
        Grasshopper2.Parameters.Standard.SurfaceBroker.CastOrConvert(
            raw, out Rhino.Geometry.Surface surface, out Rhino.Geometry.Brep brep, out Rhino.Geometry.SubD subd) switch {
            Grasshopper2.Parameters.Standard.SurfaceLikeType.Surf => new SurfaceShape.OfSurface(surface),
            Grasshopper2.Parameters.Standard.SurfaceLikeType.Brep => new SurfaceShape.OfBrep(brep),
            Grasshopper2.Parameters.Standard.SurfaceLikeType.SubD => new SurfaceShape.OfSubD(subd),
            _ => Fin.Fail<SurfaceShape>(new GhFault.Conversion(
                key,
                raw.GetType().Name,
                nameof(SurfaceShape),
                nameof(Grasshopper2.Parameters.Standard.SurfaceBroker))),
        };

    private static ConversionReceipt Receipt<TOut>(object raw, string route, Option<Grasshopper2.Types.Conversion.Merit> merit) =>
        new(raw.GetType().Name, typeof(TOut).Name, route, merit);
}
```

## [05]-[HOST_CONTEXT]

- Owner: `HostUnits` captures the live tolerance triad and unit system once, then projects those scalars through `Rasm.Domain.Context.Of`; the host record never owns an independent tolerance policy.
- Entry: `HostUnits.Of(IDataAccess)` captures host evidence; `Context` performs canonical domain admission; `ScalingTo` exposes the host's live scale query for boundary-only conversions.
- Boundary: every kernel call consumes the admitted `Context`, so raw GH2 tolerance values stop at this projection.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record HostUnits(double Absolute, double Relative, Grasshopper2.Types.Numeric.Angle Angle, UnitSystem Units) {
    public static Fin<HostUnits> Of(IDataAccess access, Op? key = null) =>
        access.GetTolerance(out double absolute, out double relative)
        && access.GetTolerance(out Grasshopper2.Types.Numeric.Angle angle)
        && access.GetUnitSystem(out UnitSystem units)
            ? Fin.Succ(new HostUnits(absolute, relative, angle, units))
            : Fin.Fail<HostUnits>(new GhFault.Absent(key.OrDefault(), nameof(HostUnits)));

    public Validation<Error, Context> Context =>
        Rasm.Domain.Context.Of(Absolute, Relative, Angle.Radians, Units);

    public Fin<double> ScalingTo(IDataAccess access, Rhino.UnitSystem target, Op? key = null) =>
        access.GetUnitScaling(target, out double factor)
            ? Fin.Succ(factor)
            : Fin.Fail<double>(new GhFault.Absent(key.OrDefault(), nameof(ScalingTo)));
}
```
