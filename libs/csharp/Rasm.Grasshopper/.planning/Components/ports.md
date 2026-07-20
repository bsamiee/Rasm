# [RASM_GRASSHOPPER_PORTS]

`PortRow` is the data-driven pin catalogue. Each row owns its verified carrier, semantic family, capability axes, and one side-aware `PortBinding`; carrier admission therefore returns the unique semantic candidate instead of forcing duplicate carriers into a dictionary. `PinPlan` carries only host-writable policy, `PinTrim` mirrors the complete writable modifier columns, and `Ports` rejects every policy value outside the selected row's consumed axes before invoking an adder.

## [01]-[INDEX]

- [02]-[PIN_VOCABULARY]: host discriminants, semantic carrier families, and row capability axes
- [03]-[PIN_PLAN]: writable pin policy, complete trim columns, and persistent tree data
- [04]-[PORT_CATALOG]: verified carrier rows and the side-aware binding union
- [05]-[DECLARATION_FOLD]: semantic admission, capability validation, declaration, and maintenance realization

## [02]-[PIN_VOCABULARY]

- Owner: the generated vocabularies own host side, access, presence, and visibility with semantic carrier families; `PortAxes` states whether a row consumes access, presence, appearance, hidden registration, and one side-scoped trim family.
- Cases: `PinSide` mirrors `Side`, `PinAccess` mirrors `Access`, `PinPresence` mirrors `Requirement`, `PortSides` closes the side-capability set including the `Neither` row hidden-capability columns demand, and `PinVisibility` closes shown/hidden.
- Entry: `PinSide.Of(Side, Op?)` is the one reverse projection; unknown host values fail typed.
- Growth: a new host discriminant value is one row on the owning vocabulary.
- Boundary: `Side`, `Access`, and `Requirement` never travel past a binding delegate; interior code holds only the folder vocabulary.

```csharp signature
namespace Rasm.Grasshopper.Components;

// --- [TYPES] -----------------------------------------------------------------------------

[SmartEnum]
public sealed partial class PinSide {
    public static readonly PinSide Input = new(Side.Input);
    public static readonly PinSide Output = new(Side.Output);

    public Side Host { get; }

    public static Fin<PinSide> Of(Side host, Op? key = null) => host switch {
        Side.Input => Fin.Succ(Input),
        Side.Output => Fin.Succ(Output),
        _ => Fin.Fail<PinSide>(new GhFault.Refused(key.OrDefault(), $"{nameof(Side)}:{host}")),
    };
}

[SmartEnum]
public sealed partial class PinAccess {
    public static readonly PinAccess Item = new(Access.Item);
    public static readonly PinAccess Twig = new(Access.Twig);
    public static readonly PinAccess Tree = new(Access.Tree);

    public Access Host { get; }
}

[SmartEnum]
public sealed partial class PinPresence {
    public static readonly PinPresence MustExist = new(Requirement.MustExist);
    public static readonly PinPresence MayBeNull = new(Requirement.MayBeNull);
    public static readonly PinPresence MayBeMissing = new(Requirement.MayBeMissing);

    public Requirement Host { get; }
}

[SmartEnum]
public sealed partial class PortSides {
    public static readonly PortSides Both = new(input: true, output: true);
    public static readonly PortSides InputOnly = new(input: true, output: false);
    public static readonly PortSides OutputOnly = new(input: false, output: true);
    public static readonly PortSides Neither = new(input: false, output: false);

    public bool Input { get; }

    public bool Output { get; }

    public bool Accepts(PinSide side) => side.Switch(state: this, input: static held => held.Input, output: static held => held.Output);
}

[SmartEnum]
public sealed partial class PinVisibility {
    public static readonly PinVisibility Shown = new();
    public static readonly PinVisibility Hidden = new();
}

[SmartEnum]
public sealed partial class PortFamily {
    private static readonly Func<Type, Type, bool> Assignable = static (declared, candidate) => declared.IsAssignableFrom(candidate);

    public static readonly PortFamily Standard = new(Assignable);
    public static readonly PortFamily Generic = new(static (_, _) => true);
    public static readonly PortFamily Numeric = new(static (_, candidate) =>
        candidate == typeof(int)
        || candidate == typeof(double)
        || candidate == typeof(System.Numerics.BigInteger)
        || candidate == typeof(System.Numerics.Complex)
        || candidate == typeof(Grasshopper2.Types.Numeric.Angle));
    public static readonly PortFamily Index = new(Assignable);
    public static readonly PortFamily Pattern = new(Assignable);
    public static readonly PortFamily Topology = new(Assignable);

    [UseDelegateFromConstructor]
    public partial bool Accepts(Type declared, Type candidate);
}

public sealed record PortAxes(
    PortSides Access,
    PortSides Presence,
    PortSides Appearance,
    PortSides Hidden,
    Option<(Type Type, PortSides Sides)> Trim) {
    public static readonly PortAxes Modular = new(PortSides.Both, PortSides.InputOnly, PortSides.Both, PortSides.Both, None);
    public static readonly PortAxes Regular = new(PortSides.Both, PortSides.InputOnly, PortSides.Neither, PortSides.Neither, None);
    public static readonly PortAxes Identity = new(PortSides.Neither, PortSides.Neither, PortSides.Neither, PortSides.Neither, None);

    public PortAxes WithTrim<TTrim>(PortSides? sides = null) where TTrim : PinTrim =>
        this with { Trim = Some((Type: typeof(TTrim), Sides: sides ?? PortSides.Both)) };
}
```

## [03]-[PIN_PLAN]

- Owner: `PinPlan` carries identity, selected row, access, presence, writable appearance, one optional complete trim, visibility, and optional `ITree` persistence. `PinTrim` mirrors every verified writable modifier column; `PresetsWeak` and `TypeAssistantWeak` remain read-only host projections, and assistant reads live on `GardenData`.
- Cases: `PinTrim` closes over Boolean null/negation policy, connection collection, vector flags, angle enforcement and reduction, integer index policy and hint, number hint, numeric exotic filtering, curve parameterization and flip, surface mesh admission/parameterization/flip, text/file behavior, and text-pattern behavior.
- Entry: `PinPlan.Realize(IParameter)` admits one trim, projects its exact host property types, and assigns only carrier-compatible persistent tree data.
- Receipt: a refused trim-to-parameter pairing is a `GhFault.Refused` naming both shapes.
- Growth: a new writable parameter policy is one trim case and one row capability; a new adder shape is one `PortBinding` case.
- Boundary: policy assignment crosses through `Hosted.Bound`; an incompatible trim fails by exact case and host type before any property is written.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

[SmartEnum]
public sealed partial class AngleEnforcement {
    public static readonly AngleEnforcement None = new(0);
    public static readonly AngleEnforcement Degrees = new(1);
    public static readonly AngleEnforcement Radians = new(2);
    public static readonly AngleEnforcement Turns = new(3);

    public int Host { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PinTrim {
    private PinTrim() { }

    public sealed record Vector(bool Unitise, bool Reverse) : PinTrim;
    public sealed record Angle(AngleEnforcement Enforce, bool Reduce) : PinTrim;
    public sealed record Boolean(bool Negate, bool NullAsTrue, bool NullAsFalse) : PinTrim;
    public sealed record Connection(bool Collect) : PinTrim;
    public sealed record Integer(
        bool AsIndex,
        Grasshopper2.Parameters.Standard.IndexModifier Indexing,
        Option<UiInteger> Hint) : PinTrim;
    public sealed record Number(UiNumber Hint) : PinTrim;
    public sealed record Numeric(NumericFilter Exotic) : PinTrim;
    public sealed record Curve(Grasshopper2.Parameters.Standard.CurveParameter.NormalisationMethod Domains, bool Flip) : PinTrim;
    public sealed record Surface(
        bool AcceptMeshes,
        Grasshopper2.Parameters.Standard.CurveParameter.NormalisationMethod Domains,
        bool Flip) : PinTrim;
    public sealed record Text(
        TextFlavour Flavour,
        Seq<string> FileExtensions,
        bool WatchFiles,
        TextParameter.CasingBehaviour Casing,
        bool CleanWhitespace) : PinTrim;
    public sealed record TextPattern(TextPatternKind Kind, bool CaseSensitive) : PinTrim;

    public bool IsValid => Switch(
        vector: static _ => true,
        angle: static trim => trim.Enforce is not null,
        boolean: static trim => !(trim.NullAsTrue && trim.NullAsFalse),
        connection: static _ => true,
        integer: static trim => Enum.IsDefined(trim.Indexing) && trim.Hint.ForAll(static hint => hint is not null) &&
            (trim.AsIndex || trim.Indexing == Grasshopper2.Parameters.Standard.IndexModifier.None),
        number: static trim => trim.Hint is not null,
        numeric: static trim => (trim.Exotic & ~NumericFilter.All) == 0,
        curve: static trim => Enum.IsDefined(trim.Domains),
        surface: static trim => Enum.IsDefined(trim.Domains),
        text: static trim => trim.Flavour is TextFlavour.String or TextFlavour.File && Enum.IsDefined(trim.Casing) &&
            trim.FileExtensions.ForAll(static extension => !string.IsNullOrWhiteSpace(extension)) &&
            (trim.Flavour == TextFlavour.File || trim.FileExtensions.IsEmpty && !trim.WatchFiles),
        textPattern: static trim => Enum.IsDefined(trim.Kind));

    internal Fin<Unit> Apply(IParameter parameter, Op key) => (this, parameter) switch {
        (Vector trim, VectorParameter host) => Hosted.Bound(() => { host.UnitiseVectors = trim.Unitise; host.ReverseVectors = trim.Reverse; }, key),
        (Angle trim, AngleParameter host) => Hosted.Bound(() => { host.EnforceKind = trim.Enforce.Host; host.ReduceAngles = trim.Reduce; }, key),
        (Boolean trim, BooleanParameter host) => Hosted.Bound(() => {
            host.NegateValues = trim.Negate;
            host.ReplaceNullsWithTrue = trim.NullAsTrue;
            host.ReplaceNullsWithFalse = trim.NullAsFalse;
        }, key),
        (Connection trim, ConnectionParameter host) => Hosted.Bound(() => { host.DoCollect = trim.Collect; }, key),
        (Integer trim, _) when !trim.AsIndex && trim.Indexing != Grasshopper2.Parameters.Standard.IndexModifier.None =>
            Fin.Fail<Unit>(new GhFault.Refused(key, nameof(Grasshopper2.Parameters.Standard.IntegerParameter.Indexing))),
        (Integer trim, IntegerParameter host) => Hosted.Bound(() => {
            host.IsIndex = trim.AsIndex;
            host.Indexing = trim.Indexing;
            trim.Hint.Iter(hint => host.Hint = hint);
        }, key),
        (Number trim, NumberParameter host) => Hosted.Bound(() => { host.Hint = trim.Hint; }, key),
        (Numeric trim, NumericParameter host) => Hosted.Bound(() => { host.ExoticFilter = trim.Exotic; }, key),
        (Curve trim, CurveParameter host) => Hosted.Bound(() => { host.NormaliseDomains = trim.Domains; host.FlipCurves = trim.Flip; }, key),
        (Surface trim, SurfaceParameter host) => Hosted.Bound(() => {
            host.AcceptMeshes = trim.AcceptMeshes;
            host.NormaliseDomains = trim.Domains;
            host.FlipSurfaces = trim.Flip;
        }, key),
        (Text trim, TextParameter host) => Hosted.Bound(() => {
            host.Flavour = trim.Flavour;
            host.FileExtensions = trim.FileExtensions.ToArray();
            host.WatchFiles = trim.WatchFiles;
            host.Casing = trim.Casing;
            host.CleanWhitespace = trim.CleanWhitespace;
        }, key),
        (TextPattern trim, TextPatternParameter host) => Hosted.Bound(() => {
            host.PatternKind = trim.Kind;
            host.CaseSensitive = trim.CaseSensitive;
        }, key),
        _ => Fin.Fail<Unit>(new GhFault.Refused(key, $"{GetType().Name}:{parameter.GetType().Name}")),
    };
}

public sealed record PinPlan {
    public required string Name { get; init; }

    public required string Nick { get; init; }

    public required string Info { get; init; }

    public required PortRow Kind { get; init; }

    public PinAccess Access { get; init; } = PinAccess.Item;

    public PinPresence Presence { get; init; } = PinPresence.MustExist;

    public Option<PinTrim> Trim { get; init; } = default;

    public PinVisibility Visibility { get; init; } = PinVisibility.Shown;

    public Option<string> Category { get; init; } = default;

    public Option<Eto.Drawing.Color> Colour { get; init; } = default;

    public Option<ITree> Persistent { get; init; } = default;

    internal IParameter Mint(
        Func<string, string, string, string, Eto.Drawing.Color, Access, Requirement, IParameter> shown,
        Func<string, string, string, string, Eto.Drawing.Color, Access, Requirement, IParameter> hidden) =>
        (Visibility == PinVisibility.Shown ? shown : hidden)(
            Name, Nick, Info, Category.IfNone(""), Colour.IfNone(Eto.Drawing.Colors.Transparent), Access.Host, Presence.Host);

    internal IParameter Mint(
        Func<string, string, string, string, Eto.Drawing.Color, Access, IParameter> shown,
        Func<string, string, string, string, Eto.Drawing.Color, Access, IParameter> hidden) =>
        (Visibility == PinVisibility.Shown ? shown : hidden)(
            Name, Nick, Info, Category.IfNone(""), Colour.IfNone(Eto.Drawing.Colors.Transparent), Access.Host);

    internal IParameter Mint(Func<string, string, string, Access, Requirement, IParameter> bare) =>
        bare(Name, Nick, Info, Access.Host, Presence.Host);

    internal IParameter Mint(Func<string, string, string, Access, IParameter> bare) =>
        bare(Name, Nick, Info, Access.Host);

    public Fin<Unit> Realize(IParameter parameter, Op? key = null) =>
        Trim.Match(
                Some: trim => trim.IsValid
                    ? trim.Apply(parameter, key.OrDefault())
                    : Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), $"{nameof(PinTrim)}:{trim.GetType().Name}")),
                None: static () => Fin.Succ(unit))
            .Bind(_ => Persistent.Match(
                Some: held => Kind is not null && Kind.Family.Accepts(Kind.Carrier, held.Type)
                    ? Hosted.Bound(() => { parameter.PersistentDataWeak = held; }, key.OrDefault())
                    : Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), $"{nameof(Persistent)}:{held.Type.Name}")),
                None: static () => Fin.Succ(unit)));
}
```

## [04]-[PORT_CATALOG]

- Owner: `PortRow` is the `[SmartEnum<string>]` catalogue; every row carries its verified value carrier, semantic `PortFamily`, `PortAxes`, and one package-minted `PortBinding`. Its internal binding union owns both-sided, input-only, and output-only adder invocation without sentinel delegates or a public delegate-construction surface.
- Cases: modular rows consume appearance and hidden policy; regular rows consume access and input presence; mixed rows state their consumed axes per side; topology consumes only its input-side connection trim.
- Entry: `Candidates(Type, PortFamily)` performs assignable carrier matching inside one semantic family, and `Admit` requires that match to resolve exactly one row.
- Auto: `Accepts` rejects unsupported side, hidden, access, presence, appearance, trim, and persistent carrier policy before any adder call.
- Packages: `Thinktecture.Runtime.Extensions` generates the row vocabulary; carriers are `Rhino.Geometry` value and geometry types and the `Grasshopper2` data types.
- Growth: a new host pin kind is one row carrying its exact value type, family, axes, and binding case.
- Boundary: `AddTopological` carries identity text only; `Numeric`, `Generic`, `Index`, `TextPattern`, and connection semantics remain distinct families even where their CLR carrier overlaps another row.

```csharp signature
// --- [SERVICES] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PortBinding {
    private PortBinding() { }

    internal sealed record BothCase(
        Func<ModularInputAdder, PinPlan, IParameter> Input,
        Func<ModularOutputAdder, PinPlan, IParameter> Output) : PortBinding;
    internal sealed record InputCase(Func<ModularInputAdder, PinPlan, IParameter> Value) : PortBinding;
    internal sealed record OutputCase(Func<ModularOutputAdder, PinPlan, IParameter> Value) : PortBinding;

    public PortSides Sides => Switch(
        bothCase: static _ => PortSides.Both,
        inputCase: static _ => PortSides.InputOnly,
        outputCase: static _ => PortSides.OutputOnly);

    public Fin<IParameter> Bind(ModularInputAdder adder, PinPlan plan, Op key) => Switch(
        bothCase: row => Hosted.Bound(() => row.Input(adder, plan), key),
        inputCase: row => Hosted.Bound(() => row.Value(adder, plan), key),
        outputCase: _ => Fin.Fail<IParameter>(new GhFault.Refused(key, nameof(PinSide.Input))));

    public Fin<IParameter> Bind(ModularOutputAdder adder, PinPlan plan, Op key) => Switch(
        bothCase: row => Hosted.Bound(() => row.Output(adder, plan), key),
        inputCase: _ => Fin.Fail<IParameter>(new GhFault.Refused(key, nameof(PinSide.Output))),
        outputCase: row => Hosted.Bound(() => row.Value(adder, plan), key));
}

[SmartEnum<string>]
public sealed partial class PortRow {
    public static readonly PortRow Path = new("path", typeof(Grasshopper2.Data.Path), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddPath, a.AddHiddenPath), static (a, p) => p.Mint(a.AddPath, a.AddHiddenPath)));
    public static readonly PortRow Site = new("site", typeof(Grasshopper2.Data.Site), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddSite, a.AddHiddenSite), static (a, p) => p.Mint(a.AddSite, a.AddHiddenSite)));
    public static readonly PortRow Topological = new("topological", typeof(System.Guid), PortFamily.Topology,
        PortAxes.Identity.WithTrim<PinTrim.Connection>(PortSides.InputOnly),
        Input(static (a, p) => a.RegularAdder.AddTopological(p.Name, p.Nick, p.Info)));
    public static readonly PortRow Colour = new("colour", typeof(Grasshopper2.Types.Colour.Colour), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddColour, a.AddHiddenColour), static (a, p) => p.Mint(a.AddColour, a.AddHiddenColour)));
    public static readonly PortRow Point = new("point", typeof(Rhino.Geometry.Point3d), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddPoint, a.AddHiddenPoint), static (a, p) => p.Mint(a.AddPoint, a.AddHiddenPoint)));
    public static readonly PortRow Vector = new("vector", typeof(Rhino.Geometry.Vector3d), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Vector>(),
        Both(static (a, p) => p.Mint(a.AddVector, a.AddHiddenVector), static (a, p) => p.Mint(a.AddVector, a.AddHiddenVector)));
    public static readonly PortRow Line = new("line", typeof(Rhino.Geometry.Line), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddLine, a.AddHiddenLine), static (a, p) => p.Mint(a.AddLine, a.AddHiddenLine)));
    public static readonly PortRow Arc = new("arc", typeof(Rhino.Geometry.Arc), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddArc, a.AddHiddenArc), static (a, p) => p.Mint(a.AddArc, a.AddHiddenArc)));
    public static readonly PortRow Circle = new("circle", typeof(Rhino.Geometry.Circle), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddCircle, a.AddHiddenCircle), static (a, p) => p.Mint(a.AddCircle, a.AddHiddenCircle)));
    public static readonly PortRow Rectangle = new("rectangle", typeof(Rhino.Geometry.Rectangle3d), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddRectangle, a.AddHiddenRectangle), static (a, p) => p.Mint(a.AddRectangle, a.AddHiddenRectangle)));
    public static readonly PortRow Curve = new("curve", typeof(Rhino.Geometry.Curve), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Curve>(),
        Both(static (a, p) => p.Mint(a.AddCurve, a.AddHiddenCurve), static (a, p) => p.Mint(a.AddCurve, a.AddHiddenCurve)));
    public static readonly PortRow Surface = new("surface", typeof(Rhino.Geometry.Surface), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Surface>(),
        Both(static (a, p) => p.Mint(a.AddSurface, a.AddHiddenSurface), static (a, p) => p.Mint(a.AddSurface, a.AddHiddenSurface)));
    public static readonly PortRow Box = new("box", typeof(Rhino.Geometry.Box), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddBox, a.AddHiddenBox), static (a, p) => p.Mint(a.AddBox, a.AddHiddenBox)));
    public static readonly PortRow Cage = new("cage", typeof(Grasshopper2.Types.Shapes.Cage), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddCage), static (a, p) => p.Mint(a.RegularAdder.AddCage)));
    public static readonly PortRow Sphere = new("sphere", typeof(Rhino.Geometry.Sphere), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddSphere, a.AddHiddenSphere), static (a, p) => p.Mint(a.AddSphere, a.AddHiddenSphere)));
    public static readonly PortRow Plane = new("plane", typeof(Rhino.Geometry.Plane), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddPlane, a.AddHiddenPlane), static (a, p) => p.Mint(a.AddPlane, a.AddHiddenPlane)));
    public static readonly PortRow Dot = new("dot", typeof(Rhino.Geometry.TextDot), PortFamily.Standard,
        new PortAxes(PortSides.OutputOnly, PortSides.Neither, PortSides.OutputOnly, PortSides.OutputOnly, None),
        Output(static (a, p) => p.Mint(a.AddDot, a.AddHiddenDot)));
    public static readonly PortRow Transform = new("transform", typeof(Rhino.Geometry.Transform), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTransform, a.AddHiddenTransform), static (a, p) => p.Mint(a.AddTransform, a.AddHiddenTransform)));
    public static readonly PortRow View = new("view", typeof(Rhino.DocObjects.ViewportInfo), PortFamily.Standard,
        new PortAxes(PortSides.Both, PortSides.InputOnly, PortSides.OutputOnly, PortSides.OutputOnly, None),
        Both(static (a, p) => p.Mint(a.RegularAdder.AddView), static (a, p) => p.Mint(a.AddView, a.AddHiddenView)));
    public static readonly PortRow Graph = new("graph", typeof(Grasshopper2.Types.Graphs.Graph), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddGraph, a.AddHiddenGraph), static (a, p) => p.Mint(a.AddGraph, a.AddHiddenGraph)));
    public static readonly PortRow Field = new("field", typeof(Grasshopper2.Types.Fields.Field), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddField, a.AddHiddenField), static (a, p) => p.Mint(a.AddField, a.AddHiddenField)));
    public static readonly PortRow Function = new("function", typeof(Grasshopper2.Types.Functions.Function), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddFunction, a.AddHiddenFunction), static (a, p) => p.Mint(a.AddFunction, a.AddHiddenFunction)));
    public static readonly PortRow Tuple = new("tuple", typeof(Grasshopper2.Types.NTuple), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTuple, a.AddHiddenTuple), static (a, p) => p.Mint(a.AddTuple, a.AddHiddenTuple)));
    public static readonly PortRow Integer = new("integer", typeof(int), PortFamily.Standard,
        PortAxes.Modular.WithTrim<PinTrim.Integer>(PortSides.InputOnly),
        Both(static (a, p) => p.Mint(a.AddInteger, a.AddHiddenInteger), static (a, p) => p.Mint(a.AddInteger, a.AddHiddenInteger)));
    public static readonly PortRow Index = new("index", typeof(int), PortFamily.Index,
        PortAxes.Regular.WithTrim<PinTrim.Integer>(PortSides.InputOnly),
        Input(static (a, p) => p.Mint(a.RegularAdder.AddIndex)));
    public static readonly PortRow Interval = new("interval", typeof(Rhino.Geometry.Interval), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddInterval, a.AddHiddenInterval), static (a, p) => p.Mint(a.AddInterval, a.AddHiddenInterval)));
    public static readonly PortRow Angle = new("angle", typeof(Grasshopper2.Types.Numeric.Angle), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Angle>(),
        Both(static (a, p) => p.Mint(a.AddAngle, a.AddHiddenAngle), static (a, p) => p.Mint(a.AddAngle, a.AddHiddenAngle)));
    public static readonly PortRow Number = new("number", typeof(double), PortFamily.Standard,
        PortAxes.Modular.WithTrim<PinTrim.Number>(PortSides.InputOnly),
        Both(static (a, p) => p.Mint(a.AddNumber, a.AddHiddenNumber), static (a, p) => p.Mint(a.AddNumber, a.AddHiddenNumber)));
    public static readonly PortRow Complex = new("complex", typeof(System.Numerics.Complex), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddComplex, a.AddHiddenComplex), static (a, p) => p.Mint(a.AddComplex, a.AddHiddenComplex)));
    public static readonly PortRow Numeric = new("numeric", typeof(object), PortFamily.Numeric, PortAxes.Modular.WithTrim<PinTrim.Numeric>(),
        Both(static (a, p) => p.Mint(a.AddNumeric, a.AddHiddenNumeric), static (a, p) => p.Mint(a.AddNumeric, a.AddHiddenNumeric)));
    public static readonly PortRow Guid = new("guid", typeof(System.Guid), PortFamily.Standard,
        new PortAxes(PortSides.OutputOnly, PortSides.Neither, PortSides.OutputOnly, PortSides.OutputOnly, None),
        Output(static (a, p) => p.Mint(a.AddGuid, a.AddHiddenGuid)));
    public static readonly PortRow Random = new("random", typeof(Grasshopper2.Types.Random.RandomEngine), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddRandom), static (a, p) => p.Mint(a.RegularAdder.AddRandom)));
    public static readonly PortRow Continuous = new("continuous", typeof(Grasshopper2.Types.Random.ContinuousDistribution), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddContinuous), static (a, p) => p.Mint(a.RegularAdder.AddContinuous)));
    public static readonly PortRow Discrete = new("discrete", typeof(Grasshopper2.Types.Random.DiscreteDistribution), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddDiscrete), static (a, p) => p.Mint(a.RegularAdder.AddDiscrete)));
    public static readonly PortRow Boolean = new("boolean", typeof(bool), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Boolean>(),
        Both(static (a, p) => p.Mint(a.AddBoolean, a.AddHiddenBoolean), static (a, p) => p.Mint(a.AddBoolean, a.AddHiddenBoolean)));
    public static readonly PortRow Text = new("text", typeof(string), PortFamily.Standard, PortAxes.Modular.WithTrim<PinTrim.Text>(),
        Both(static (a, p) => p.Mint(a.AddText, a.AddHiddenText), static (a, p) => p.Mint(a.AddText, a.AddHiddenText)));
    public static readonly PortRow TextPattern = new("textPattern", typeof(string), PortFamily.Pattern,
        new PortAxes(PortSides.InputOnly, PortSides.InputOnly, PortSides.InputOnly, PortSides.InputOnly,
            Some((Type: typeof(PinTrim.TextPattern), Sides: PortSides.InputOnly))),
        Input(static (a, p) => p.Mint(a.AddTextPattern, a.AddHiddenTextPattern)));
    public static readonly PortRow Gradient = new("gradient", typeof(Grasshopper2.Types.Colour.Gradient), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddGradient, a.AddHiddenGradient), static (a, p) => p.Mint(a.AddGradient, a.AddHiddenGradient)));
    public static readonly PortRow DateTime = new("dateTime", typeof(System.DateTime), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddDateTime, a.AddHiddenDateTime), static (a, p) => p.Mint(a.AddDateTime, a.AddHiddenDateTime)));
    public static readonly PortRow TimeSpan = new("timeSpan", typeof(System.TimeSpan), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTimeSpan, a.AddHiddenTimeSpan), static (a, p) => p.Mint(a.AddTimeSpan, a.AddHiddenTimeSpan)));
#pragma warning disable CS0618
    public static readonly PortRow Language = new("language", typeof(Grasshopper2.Types.Linguistic.Language), PortFamily.Standard, PortAxes.Regular,
        Both(static (a, p) => p.Mint(a.RegularAdder.AddLanguage), static (a, p) => p.Mint(a.RegularAdder.AddLanguage)));
#pragma warning restore CS0618
    public static readonly PortRow MetaName = new("metaName", typeof(Grasshopper2.Data.Meta.MetaName), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddMetaKey, a.AddHiddenMetaKey), static (a, p) => p.Mint(a.AddMetaKey, a.AddHiddenMetaKey)));
    public static readonly PortRow Meta = new("meta", typeof(MetaData), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddMetaData, a.AddHiddenMetaData), static (a, p) => p.Mint(a.AddMetaData, a.AddHiddenMetaData)));
    public static readonly PortRow Mesh = new("mesh", typeof(Rhino.Geometry.Mesh), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddMesh, a.AddHiddenMesh), static (a, p) => p.Mint(a.AddMesh, a.AddHiddenMesh)));
    public static readonly PortRow Polyline = new("polyline", typeof(Rhino.Geometry.Polyline), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddPolyline, a.AddHiddenPolyline), static (a, p) => p.Mint(a.AddPolyline, a.AddHiddenPolyline)));
    public static readonly PortRow Generic = new("generic", typeof(object), PortFamily.Generic, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddGeneric, a.AddHiddenGeneric), static (a, p) => p.Mint(a.AddGeneric, a.AddHiddenGeneric)));
    public static readonly PortRow Triangle = new("triangle", typeof(Grasshopper2.Types.Shapes.Triangle), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTriangle, a.AddHiddenTriangle), static (a, p) => p.Mint(a.AddTriangle, a.AddHiddenTriangle)));
    public static readonly PortRow Tube = new("tube", typeof(Grasshopper2.Types.Shapes.Tube), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddTube, a.AddHiddenTube), static (a, p) => p.Mint(a.AddTube, a.AddHiddenTube)));
    public static readonly PortRow Region = new("region", typeof(Grasshopper2.Types.Shapes.Region), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddRegion, a.AddHiddenRegion), static (a, p) => p.Mint(a.AddRegion, a.AddHiddenRegion)));
    public static readonly PortRow CurveLocus = new("curveLocus", typeof(Grasshopper2.Types.Shapes.CurveLocus), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddCurveLocus, a.AddHiddenCurveLocus), static (a, p) => p.Mint(a.AddCurveLocus, a.AddHiddenCurveLocus)));
    public static readonly PortRow SurfaceLocus = new("surfaceLocus", typeof(Grasshopper2.Types.Shapes.SurfaceLocus), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddSurfaceLocus, a.AddHiddenSurfaceLocus), static (a, p) => p.Mint(a.AddSurfaceLocus, a.AddHiddenSurfaceLocus)));
    public static readonly PortRow MeshFacet = new("meshFacet", typeof(Rhino.Geometry.MeshFace), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddMeshFacet, a.AddHiddenMeshFacet), static (a, p) => p.Mint(a.AddMeshFacet, a.AddHiddenMeshFacet)));
    public static readonly PortRow NPoint = new("nPoint", typeof(Grasshopper2.Types.Coordinates.NPoint), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddNPoint, a.AddHiddenNPoint), static (a, p) => p.Mint(a.AddNPoint, a.AddHiddenNPoint)));
    public static readonly PortRow UvPoint = new("uvPoint", typeof(Rhino.Geometry.Point2d), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddUvPoint, a.AddHiddenUvPoint), static (a, p) => p.Mint(a.AddUvPoint, a.AddHiddenUvPoint)));
    public static readonly PortRow Deform = new("deform", typeof(Grasshopper2.Types.Shapes.Deform), PortFamily.Standard, PortAxes.Modular,
        Both(static (a, p) => p.Mint(a.AddDeform, a.AddHiddenDeform), static (a, p) => p.Mint(a.AddDeform, a.AddHiddenDeform)));

    public Type Carrier { get; }

    public PortFamily Family { get; }

    public PortAxes Axes { get; }

    internal PortBinding Binding { get; }

    public PortSides Sides => Binding.Sides;

    public static Seq<PortRow> Candidates(Type carrier, PortFamily family) =>
        toSeq(Items).Filter(row => row.Family == family && family.Accepts(row.Carrier, carrier)).Strict();

    public static Validation<Error, PortRow> Admit(Type carrier, PortFamily family, Op? key = null) {
        Seq<PortRow> candidates = Candidates(carrier, family);
        return candidates.Count switch {
            1 => Success<Error, PortRow>(candidates[0]),
            _ => Fail<Error, PortRow>(new GhFault.Refused(key.OrDefault(), $"{carrier.Name}:{family}:{candidates.Count}")),
        };
    }

    public Fin<Unit> Accepts(PinPlan plan, PinSide side, Op key) =>
        (
            Side: Sides.Accepts(side),
            Hidden: plan.Visibility == PinVisibility.Shown || Axes.Hidden.Accepts(side),
            Access: plan.Access == PinAccess.Item || Axes.Access.Accepts(side),
            Presence: plan.Presence == PinPresence.MustExist || Axes.Presence.Accepts(side),
            Appearance: plan.Category.IsNone && plan.Colour.IsNone || Axes.Appearance.Accepts(side),
            Trim: plan.Trim.ForAll(trim => AdmitsTrim(trim, side)),
            Persistent: plan.Persistent.ForAll(tree => Family.Accepts(Carrier, tree.Type))
        ) switch {
            { Side: false } => Fin.Fail<Unit>(new GhFault.Refused(key, $"{Key}:{side}")),
            { Hidden: false } => Fin.Fail<Unit>(new GhFault.Refused(key, $"{Key}:{nameof(PinVisibility.Hidden)}:{side}")),
            { Access: false } => Fin.Fail<Unit>(new GhFault.Refused(key, $"{Key}:{nameof(PinPlan.Access)}:{side}")),
            { Presence: false } => Fin.Fail<Unit>(new GhFault.Refused(key, $"{Key}:{nameof(PinPlan.Presence)}:{side}")),
            { Appearance: false } => Fin.Fail<Unit>(new GhFault.Refused(key, $"{Key}:{nameof(PinPlan.Category)}:{side}")),
            { Trim: false } => Fin.Fail<Unit>(new GhFault.Refused(key, $"{Key}:{nameof(PinPlan.Trim)}:{side}")),
            { Persistent: false } => Fin.Fail<Unit>(new GhFault.Refused(key, $"{Key}:{nameof(PinPlan.Persistent)}:{side}")),
            _ => Fin.Succ(unit),
        };

    private bool AdmitsTrim(PinTrim trim, PinSide side) =>
        trim is { IsValid: true } && Axes.Trim.Map(axis => axis.Sides.Accepts(side) && axis.Type.IsInstanceOfType(trim)
            && (this != Index || trim is not PinTrim.Integer { AsIndex: false })
            && (this != Integer || trim is not PinTrim.Integer { AsIndex: true })).IfNone(false);

    private static PortBinding Both(
        Func<ModularInputAdder, PinPlan, IParameter> input,
        Func<ModularOutputAdder, PinPlan, IParameter> output) => new PortBinding.BothCase(input, output);

    private static PortBinding Input(Func<ModularInputAdder, PinPlan, IParameter> input) => new PortBinding.InputCase(input);

    private static PortBinding Output(Func<ModularOutputAdder, PinPlan, IParameter> output) => new PortBinding.OutputCase(output);
}
```

## [05]-[DECLARATION_FOLD]

- Owner: `Ports` is the one declaration fold — side selection is the adder argument's static type, every plan folds through the accumulating carrier so a malformed pin roster reports every violation at once, and each minted `IParameter` realizes its trim and persistent data on the same fault rail.
- Entry: `Declare` admits every row policy, invokes the binding union, and realizes the minted parameter; `Realize` re-applies the trim and persistent tree through `ComponentParameters.Input(int)` and `Output(int)`; the `DeclareEnum<T>` input/output pair admits only non-flags, `Int32`-backed enums and retains the integer row's applicable policy.
- Receipt: declaration returns `Validation<Error, Seq<IParameter>>` carrying row-policy, host, trim, and persistence failures.
- Growth: a new maintenance projection is one fold over the returned parameter seq; enum pins remain integer carriers, while `T` supplies presets and the input seed.
- Boundary: a rejected policy never reaches the host; presets and assistants are observed through their get-only host contracts rather than projected as plan setters.

```csharp signature
// --- [OPERATIONS] ------------------------------------------------------------------------

public static class Ports {
    public static Validation<Error, Seq<IParameter>> Declare(ModularInputAdder adder, Seq<PinPlan> plans, Op? key = null) {
        Op op = key.OrDefault();
        return plans.Traverse(plan => Minted(plan, PinSide.Input, () => plan.Kind.Binding.Bind(adder, plan, op), op).ToValidation()).As();
    }

    public static Validation<Error, Seq<IParameter>> Declare(ModularOutputAdder adder, Seq<PinPlan> plans, Op? key = null) {
        Op op = key.OrDefault();
        return plans.Traverse(plan => Minted(plan, PinSide.Output, () => plan.Kind.Binding.Bind(adder, plan, op), op).ToValidation()).As();
    }

    public static Fin<IParameter> DeclareEnum<T>(ModularInputAdder adder, PinPlan plan, T seed, Op? key = null) where T : struct, Enum {
        Op op = key.OrDefault();
        string category = plan.Category.IfNone("");
        Eto.Drawing.Color colour = plan.Colour.IfNone(Eto.Drawing.Colors.Transparent);
        return EnumType<T>(op).Bind(_ => plan.Kind == PortRow.Integer
            ? plan.Kind.Accepts(plan, PinSide.Input, op).Bind(_ => Hosted.Bound<IParameter>(() => plan.Visibility == PinVisibility.Shown
                    ? adder.AddEnum(plan.Name, plan.Nick, plan.Info, category, colour, seed, plan.Access.Host, plan.Presence.Host)
                    : adder.AddHiddenEnum(plan.Name, plan.Nick, plan.Info, category, colour, seed, plan.Access.Host, plan.Presence.Host), op))
                .Bind(parameter => plan.Realize(parameter, op).Map(_ => parameter))
            : Fin.Fail<IParameter>(new GhFault.Refused(op, $"{plan.Kind.Key}:{nameof(DeclareEnum)}")));
    }

    public static Fin<IParameter> DeclareEnum<T>(ModularOutputAdder adder, PinPlan plan, Op? key = null) where T : struct, Enum {
        Op op = key.OrDefault();
        return EnumType<T>(op)
            .Bind(_ => AcceptsOutputEnum(plan, op))
            .Bind(_ => Hosted.Bound<IParameter>(() => adder.RegularAdder.AddEnum<T>(
                plan.Name, plan.Nick, plan.Info, plan.Access.Host), op))
            .Bind(parameter => plan.Realize(parameter, op).Map(_ => parameter));
    }

    public static Validation<Error, Unit> Realize(ComponentParameters parameters, Seq<PinPlan> inputs, Seq<PinPlan> outputs, Op? key = null) {
        Op op = key.OrDefault();
        return (inputs.Map(static (plan, index) => (Plan: plan, Index: index))
                .Traverse(row => Hosted.Bound<IParameter>(() => parameters.Input(row.Index), op)
                    .Bind(parameter => row.Plan.Realize(parameter, op)).ToValidation()).As(),
            outputs.Map(static (plan, index) => (Plan: plan, Index: index))
                .Traverse(row => Hosted.Bound<IParameter>(() => parameters.Output(row.Index), op)
                    .Bind(parameter => row.Plan.Realize(parameter, op)).ToValidation()).As())
            .Apply(static (_, _) => unit)
            .As();
    }

    private static Fin<IParameter> Minted(PinPlan plan, PinSide side, Func<Fin<IParameter>> bind, Op key) =>
        plan.Kind.Accepts(plan, side, key)
            .Bind(_ => bind())
            .Bind(parameter => plan.Realize(parameter, key).Map(_ => parameter));

    private static Fin<Unit> EnumType<T>(Op key) where T : struct, Enum =>
        Enum.GetUnderlyingType(typeof(T)) == typeof(int) && !typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GhFault.Refused(key, $"{typeof(T).Name}:{nameof(DeclareEnum)}"));

    private static Fin<Unit> AcceptsOutputEnum(PinPlan plan, Op key) =>
        plan.Kind == PortRow.Integer &&
        plan.Visibility == PinVisibility.Shown &&
        plan.Presence == PinPresence.MustExist &&
        plan.Category.IsNone &&
        plan.Colour.IsNone &&
        plan.Trim.IsNone
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GhFault.Refused(key, $"{plan.Kind.Key}:{nameof(DeclareEnum)}:{nameof(PinSide.Output)}"));
}
```
