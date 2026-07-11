# [RASM_GRASSHOPPER_PORTS]

`PortRow` is the data-driven pin catalog: the host adder rosters demote to seed rows, and one row carries carrier type, side capability, hidden-side capability, and both registration bindings over the modular adders — so every pin kind the host hides, categorizes, or colours declares through the same row, and the binding returns the minted `IParameter` for immediate realization. Pin depth, presence, side, and visibility are generated vocabularies over the host discriminants; parameter-property policy is one `PinTrim` family applied at declaration and re-applied through the `ComponentParameters` index accessors; and `Ports` is the one declaration fold every component pin crosses. A new host pin kind lands as one row with every consumer unchanged.

## [01]-[INDEX]

- [02]-[PIN_VOCABULARY]: the side, depth, presence, side-capability, and visibility vocabularies over the host discriminants
- [03]-[PIN_PLAN]: the per-pin declaration record, the polymorphic mint, and the `PinTrim` parameter-property family
- [04]-[PORT_CATALOG]: the `PortRow` seed-row catalog binding every verified adder member
- [05]-[DECLARATION_FOLD]: the `Ports` declare/realize fold and the generic enum entry
- [06]-[RESEARCH]

## [02]-[PIN_VOCABULARY]

- Owner: five keyless `[SmartEnum]` vocabularies own every pin discriminant; each row carries its host value as a column, so folder dispatch is exhaustive generated `Switch` while the host value crosses only inside a registration binding.
- Cases: `PinSide` mirrors `Side`, `PinAccess` mirrors `Access`, `PinPresence` mirrors `Requirement`, `PortSides` closes the side-capability set including the `Neither` row hidden-capability columns demand, and `PinVisibility` closes shown/hidden.
- Entry: `PinSide.Of(Side)` is the one reverse projection a host side admits through.
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

    public static PinSide Of(Side host) => host == Side.Input ? Input : Output;
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
```

## [03]-[PIN_PLAN]

- Owner: `PinPlan` is the one per-pin declaration value — identity text, catalog row, depth, presence, trims, visibility, category, colour, and persistent data ride one record; `Mint` is its polymorphic invocation over the four verified adder-member shapes, discriminating shown against hidden by the plan's own visibility; `PinTrim` is the closed parameter-property family over the `Grasshopper2.Parameters.Standard` projections.
- Cases: `PinTrim` closes at `Vector`, `Angle`, `Integer`, `Curve`, and `Surface` — each case carries the full verified property set of its host parameter.
- Entry: `PinTrim.Apply(IParameter, Op)` discriminates by the joint (trim, host parameter) shape in one flattened pattern; `PinPlan.Realize(IParameter)` folds trims and persistent data onto the minted pin.
- Receipt: a refused trim-to-parameter pairing is a `GhFault.Refused` naming both shapes.
- Growth: a new host parameter policy is one `PinTrim` case plus one joint-pattern arm; a new adder-member shape is one delegate row plus one `Mint` overload.
- Boundary: property assignment crosses through `data#FAULT_AND_NOTICE` `Hosted.Bound`; the joint pattern's `var` floor is the open host-parameter shape, never a swallowed closed case.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------

public delegate IParameter InputMint(string name, string code, string info, string category, Eto.Drawing.Color colour, Access access, Requirement requirement);
public delegate IParameter OutputMint(string name, string code, string info, string category, Eto.Drawing.Color colour, Access access);
public delegate IParameter InputBare(string name, string code, string info, Access access, Requirement requirement);
public delegate IParameter OutputBare(string name, string code, string info, Access access);

// --- [MODELS] ----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PinTrim {
    private PinTrim() { }

    public sealed record Vector(bool Unitise, bool Reverse) : PinTrim;
    public sealed record Angle(bool Reduce) : PinTrim;
    public sealed record Integer(bool AsIndex) : PinTrim;
    public sealed record Curve(bool NormaliseDomains, bool Flip) : PinTrim;
    public sealed record Surface(bool AcceptMeshes, bool NormaliseDomains, bool Flip) : PinTrim;

    public Fin<Unit> Apply(IParameter parameter, Op key) => (this, parameter) switch {
        (Vector trim, VectorParameter host) => Hosted.Bound(() => { host.UnitiseVectors = trim.Unitise; host.ReverseVectors = trim.Reverse; }, key),
        (Angle trim, AngleParameter host) => Hosted.Bound(() => { host.ReduceAngles = trim.Reduce; }, key),
        (Integer trim, IntegerParameter host) => Hosted.Bound(() => { host.IsIndex = trim.AsIndex; }, key),
        (Curve trim, CurveParameter host) => Hosted.Bound(() => { host.NormaliseDomains = trim.NormaliseDomains; host.FlipCurves = trim.Flip; }, key),
        (Surface trim, SurfaceParameter host) => Hosted.Bound(() => {
            host.AcceptMeshes = trim.AcceptMeshes;
            host.NormaliseDomains = trim.NormaliseDomains;
            host.FlipSurfaces = trim.Flip;
        }, key),
        var (trim, host) => Fin.Fail<Unit>(new GhFault.Refused(key, $"{trim.GetType().Name}:{host.GetType().Name}")),
    };
}

public sealed record PinPlan {
    public required string Name { get; init; }

    public required string Nick { get; init; }

    public required string Info { get; init; }

    public required PortRow Kind { get; init; }

    public PinAccess Access { get; init; } = PinAccess.Item;

    public PinPresence Presence { get; init; } = PinPresence.MustExist;

    public Seq<PinTrim> Trims { get; init; } = [];

    public PinVisibility Visibility { get; init; } = PinVisibility.Shown;

    public Option<string> Category { get; init; } = default;

    public Option<Eto.Drawing.Color> Colour { get; init; } = default;

    public Option<object> Persistent { get; init; } = default;

    public IParameter Mint(InputMint shown, InputMint hidden) =>
        (Visibility == PinVisibility.Shown ? shown : hidden)(
            Name, Nick, Info, Category.IfNone(""), Colour.IfNone(Eto.Drawing.Colors.Transparent), Access.Host, Presence.Host);

    public IParameter Mint(OutputMint shown, OutputMint hidden) =>
        (Visibility == PinVisibility.Shown ? shown : hidden)(
            Name, Nick, Info, Category.IfNone(""), Colour.IfNone(Eto.Drawing.Colors.Transparent), Access.Host);

    public IParameter Mint(InputBare bare) => bare(Name, Nick, Info, Access.Host, Presence.Host);

    public IParameter Mint(OutputBare bare) => bare(Name, Nick, Info, Access.Host);

    public Fin<Unit> Realize(IParameter parameter, Op? key = null) =>
        Trims.TraverseM(trim => trim.Apply(parameter, key.OrDefault())).As()
            .Bind(_ => Persistent.Match(
                Some: held => Hosted.Bound(() => { parameter.PersistentDataWeak = held; }, key.OrDefault()),
                None: static () => Fin.Succ(unit)));
}
```

## [04]-[PORT_CATALOG]

- Owner: `PortRow` is the `[SmartEnum<string>]` seed catalog — one row per verified host pin kind carrying its typed carrier where the carrier is a verified type, its side capability, its hidden-side capability, and both registration bindings; every binding names only verified adder members and returns the minted `IParameter`.
- Cases: a row whose kind carries the modular `Add*`/`AddHidden*` pair binds through `PinPlan.Mint` with category and colour applied; a kind the modular adders omit binds through `RegularAdder` with `PortSides.Neither` hidden capability; `MetaName` binds the host `AddMetaKey` spelling.
- Entry: `PortRow.ForCarrier(Type)` is the `Items`-derived frozen default-row index; `Get`/`TryGet` key lookup arrives generated.
- Auto: a side a row does not carry is unreachable by the `Sides` gate, so its binding column throws `UnreachableException` by construction, never a silent no-op.
- Packages: `Thinktecture.Runtime.Extensions` generates the row vocabulary; carriers are `Rhino.Geometry` value and geometry types plus the verified `Grasshopper2` data types.
- Growth: a new host pin kind is one three-line row; a carrier resolved later flips `Option.None` to `Some(typeof(...))` on its row; a kind gaining modular members flips its hidden column and rebinds through `Mint`.
- Boundary: `AddTopological` carries no depth or presence arguments and its row binds identity text only; unresolved carriers stay `None` until the value-type census lands them.

```csharp signature
// --- [SERVICES] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class PortRow {
    public static readonly PortRow Path = new("path", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddPath, a.AddHiddenPath),
        static (a, p) => p.Mint(a.AddPath, a.AddHiddenPath));
    public static readonly PortRow Site = new("site", Some(typeof(Grasshopper2.Data.Site)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddSite, a.AddHiddenSite),
        static (a, p) => p.Mint(a.AddSite, a.AddHiddenSite));
    public static readonly PortRow Topological = new("topological", None, PortSides.InputOnly, PortSides.Neither,
        static (a, p) => a.RegularAdder.AddTopological(p.Name, p.Nick, p.Info),
        static (_, _) => throw new System.Diagnostics.UnreachableException());
    public static readonly PortRow Colour = new("colour", Some(typeof(Grasshopper2.Types.Colour.Colour)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddColour, a.AddHiddenColour),
        static (a, p) => p.Mint(a.AddColour, a.AddHiddenColour));
    public static readonly PortRow Point = new("point", Some(typeof(Rhino.Geometry.Point3d)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddPoint, a.AddHiddenPoint),
        static (a, p) => p.Mint(a.AddPoint, a.AddHiddenPoint));
    public static readonly PortRow Vector = new("vector", Some(typeof(Rhino.Geometry.Vector3d)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddVector, a.AddHiddenVector),
        static (a, p) => p.Mint(a.AddVector, a.AddHiddenVector));
    public static readonly PortRow Line = new("line", Some(typeof(Rhino.Geometry.Line)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddLine, a.AddHiddenLine),
        static (a, p) => p.Mint(a.AddLine, a.AddHiddenLine));
    public static readonly PortRow Arc = new("arc", Some(typeof(Rhino.Geometry.Arc)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddArc, a.AddHiddenArc),
        static (a, p) => p.Mint(a.AddArc, a.AddHiddenArc));
    public static readonly PortRow Circle = new("circle", Some(typeof(Rhino.Geometry.Circle)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddCircle, a.AddHiddenCircle),
        static (a, p) => p.Mint(a.AddCircle, a.AddHiddenCircle));
    public static readonly PortRow Rectangle = new("rectangle", Some(typeof(Rhino.Geometry.Rectangle3d)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddRectangle, a.AddHiddenRectangle),
        static (a, p) => p.Mint(a.AddRectangle, a.AddHiddenRectangle));
    public static readonly PortRow Curve = new("curve", Some(typeof(Rhino.Geometry.Curve)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddCurve, a.AddHiddenCurve),
        static (a, p) => p.Mint(a.AddCurve, a.AddHiddenCurve));
    public static readonly PortRow Surface = new("surface", Some(typeof(Rhino.Geometry.Surface)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddSurface, a.AddHiddenSurface),
        static (a, p) => p.Mint(a.AddSurface, a.AddHiddenSurface));
    public static readonly PortRow Box = new("box", Some(typeof(Rhino.Geometry.Box)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddBox, a.AddHiddenBox),
        static (a, p) => p.Mint(a.AddBox, a.AddHiddenBox));
    public static readonly PortRow Cage = new("cage", None, PortSides.Both, PortSides.Neither,
        static (a, p) => p.Mint(a.RegularAdder.AddCage),
        static (a, p) => p.Mint(a.RegularAdder.AddCage));
    public static readonly PortRow Sphere = new("sphere", Some(typeof(Rhino.Geometry.Sphere)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddSphere, a.AddHiddenSphere),
        static (a, p) => p.Mint(a.AddSphere, a.AddHiddenSphere));
    public static readonly PortRow Plane = new("plane", Some(typeof(Rhino.Geometry.Plane)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddPlane, a.AddHiddenPlane),
        static (a, p) => p.Mint(a.AddPlane, a.AddHiddenPlane));
    public static readonly PortRow Dot = new("dot", None, PortSides.OutputOnly, PortSides.OutputOnly,
        static (_, _) => throw new System.Diagnostics.UnreachableException(),
        static (a, p) => p.Mint(a.AddDot, a.AddHiddenDot));
    public static readonly PortRow Transform = new("transform", Some(typeof(Rhino.Geometry.Transform)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddTransform, a.AddHiddenTransform),
        static (a, p) => p.Mint(a.AddTransform, a.AddHiddenTransform));
    public static readonly PortRow View = new("view", None, PortSides.Both, PortSides.OutputOnly,
        static (a, p) => p.Mint(a.RegularAdder.AddView),
        static (a, p) => p.Mint(a.AddView, a.AddHiddenView));
    public static readonly PortRow Graph = new("graph", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddGraph, a.AddHiddenGraph),
        static (a, p) => p.Mint(a.AddGraph, a.AddHiddenGraph));
    public static readonly PortRow Field = new("field", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddField, a.AddHiddenField),
        static (a, p) => p.Mint(a.AddField, a.AddHiddenField));
    public static readonly PortRow Function = new("function", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddFunction, a.AddHiddenFunction),
        static (a, p) => p.Mint(a.AddFunction, a.AddHiddenFunction));
    public static readonly PortRow Tuple = new("tuple", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddTuple, a.AddHiddenTuple),
        static (a, p) => p.Mint(a.AddTuple, a.AddHiddenTuple));
    public static readonly PortRow Integer = new("integer", Some(typeof(int)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddInteger, a.AddHiddenInteger),
        static (a, p) => p.Mint(a.AddInteger, a.AddHiddenInteger));
    public static readonly PortRow Index = new("index", Some(typeof(int)), PortSides.InputOnly, PortSides.Neither,
        static (a, p) => p.Mint(a.RegularAdder.AddIndex),
        static (_, _) => throw new System.Diagnostics.UnreachableException());
    public static readonly PortRow Interval = new("interval", Some(typeof(Rhino.Geometry.Interval)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddInterval, a.AddHiddenInterval),
        static (a, p) => p.Mint(a.AddInterval, a.AddHiddenInterval));
    public static readonly PortRow Angle = new("angle", Some(typeof(Grasshopper2.Types.Numeric.Angle)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddAngle, a.AddHiddenAngle),
        static (a, p) => p.Mint(a.AddAngle, a.AddHiddenAngle));
    public static readonly PortRow Number = new("number", Some(typeof(double)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddNumber, a.AddHiddenNumber),
        static (a, p) => p.Mint(a.AddNumber, a.AddHiddenNumber));
    public static readonly PortRow Complex = new("complex", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddComplex, a.AddHiddenComplex),
        static (a, p) => p.Mint(a.AddComplex, a.AddHiddenComplex));
    public static readonly PortRow Numeric = new("numeric", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddNumeric, a.AddHiddenNumeric),
        static (a, p) => p.Mint(a.AddNumeric, a.AddHiddenNumeric));
    public static readonly PortRow Guid = new("guid", Some(typeof(System.Guid)), PortSides.OutputOnly, PortSides.OutputOnly,
        static (_, _) => throw new System.Diagnostics.UnreachableException(),
        static (a, p) => p.Mint(a.AddGuid, a.AddHiddenGuid));
    public static readonly PortRow Random = new("random", None, PortSides.Both, PortSides.Neither,
        static (a, p) => p.Mint(a.RegularAdder.AddRandom),
        static (a, p) => p.Mint(a.RegularAdder.AddRandom));
    public static readonly PortRow Continuous = new("continuous", None, PortSides.Both, PortSides.Neither,
        static (a, p) => p.Mint(a.RegularAdder.AddContinuous),
        static (a, p) => p.Mint(a.RegularAdder.AddContinuous));
    public static readonly PortRow Discrete = new("discrete", None, PortSides.Both, PortSides.Neither,
        static (a, p) => p.Mint(a.RegularAdder.AddDiscrete),
        static (a, p) => p.Mint(a.RegularAdder.AddDiscrete));
    public static readonly PortRow Boolean = new("boolean", Some(typeof(bool)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddBoolean, a.AddHiddenBoolean),
        static (a, p) => p.Mint(a.AddBoolean, a.AddHiddenBoolean));
    public static readonly PortRow Text = new("text", Some(typeof(string)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddText, a.AddHiddenText),
        static (a, p) => p.Mint(a.AddText, a.AddHiddenText));
    public static readonly PortRow TextPattern = new("textPattern", Some(typeof(string)), PortSides.InputOnly, PortSides.InputOnly,
        static (a, p) => p.Mint(a.AddTextPattern, a.AddHiddenTextPattern),
        static (_, _) => throw new System.Diagnostics.UnreachableException());
    public static readonly PortRow Gradient = new("gradient", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddGradient, a.AddHiddenGradient),
        static (a, p) => p.Mint(a.AddGradient, a.AddHiddenGradient));
    public static readonly PortRow DateTime = new("dateTime", Some(typeof(System.DateTime)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddDateTime, a.AddHiddenDateTime),
        static (a, p) => p.Mint(a.AddDateTime, a.AddHiddenDateTime));
    public static readonly PortRow TimeSpan = new("timeSpan", Some(typeof(System.TimeSpan)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddTimeSpan, a.AddHiddenTimeSpan),
        static (a, p) => p.Mint(a.AddTimeSpan, a.AddHiddenTimeSpan));
    public static readonly PortRow Language = new("language", None, PortSides.Both, PortSides.Neither,
        static (a, p) => p.Mint(a.RegularAdder.AddLanguage),
        static (a, p) => p.Mint(a.RegularAdder.AddLanguage));
    public static readonly PortRow MetaName = new("metaName", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddMetaKey, a.AddHiddenMetaKey),
        static (a, p) => p.Mint(a.AddMetaKey, a.AddHiddenMetaKey));
    public static readonly PortRow Meta = new("meta", Some(typeof(MetaData)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddMetaData, a.AddHiddenMetaData),
        static (a, p) => p.Mint(a.AddMetaData, a.AddHiddenMetaData));
    public static readonly PortRow Mesh = new("mesh", Some(typeof(Rhino.Geometry.Mesh)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddMesh, a.AddHiddenMesh),
        static (a, p) => p.Mint(a.AddMesh, a.AddHiddenMesh));
    public static readonly PortRow Polyline = new("polyline", Some(typeof(Rhino.Geometry.Polyline)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddPolyline, a.AddHiddenPolyline),
        static (a, p) => p.Mint(a.AddPolyline, a.AddHiddenPolyline));
    public static readonly PortRow Generic = new("generic", Some(typeof(object)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddGeneric, a.AddHiddenGeneric),
        static (a, p) => p.Mint(a.AddGeneric, a.AddHiddenGeneric));
    public static readonly PortRow Triangle = new("triangle", Some(typeof(Grasshopper2.Types.Shapes.Triangle)), PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddTriangle, a.AddHiddenTriangle),
        static (a, p) => p.Mint(a.AddTriangle, a.AddHiddenTriangle));
    public static readonly PortRow Tube = new("tube", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddTube, a.AddHiddenTube),
        static (a, p) => p.Mint(a.AddTube, a.AddHiddenTube));
    public static readonly PortRow Region = new("region", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddRegion, a.AddHiddenRegion),
        static (a, p) => p.Mint(a.AddRegion, a.AddHiddenRegion));
    public static readonly PortRow CurveLocus = new("curveLocus", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddCurveLocus, a.AddHiddenCurveLocus),
        static (a, p) => p.Mint(a.AddCurveLocus, a.AddHiddenCurveLocus));
    public static readonly PortRow SurfaceLocus = new("surfaceLocus", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddSurfaceLocus, a.AddHiddenSurfaceLocus),
        static (a, p) => p.Mint(a.AddSurfaceLocus, a.AddHiddenSurfaceLocus));
    public static readonly PortRow MeshFacet = new("meshFacet", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddMeshFacet, a.AddHiddenMeshFacet),
        static (a, p) => p.Mint(a.AddMeshFacet, a.AddHiddenMeshFacet));
    public static readonly PortRow NPoint = new("nPoint", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddNPoint, a.AddHiddenNPoint),
        static (a, p) => p.Mint(a.AddNPoint, a.AddHiddenNPoint));
    public static readonly PortRow UvPoint = new("uvPoint", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddUvPoint, a.AddHiddenUvPoint),
        static (a, p) => p.Mint(a.AddUvPoint, a.AddHiddenUvPoint));
    public static readonly PortRow Deform = new("deform", None, PortSides.Both, PortSides.Both,
        static (a, p) => p.Mint(a.AddDeform, a.AddHiddenDeform),
        static (a, p) => p.Mint(a.AddDeform, a.AddHiddenDeform));

    public Option<Type> Carrier { get; }

    public PortSides Sides { get; }

    public PortSides HiddenSides { get; }

    [UseDelegateFromConstructor]
    public partial IParameter BindInput(ModularInputAdder adder, PinPlan plan);

    [UseDelegateFromConstructor]
    public partial IParameter BindOutput(ModularOutputAdder adder, PinPlan plan);

    private static readonly Lazy<FrozenDictionary<Type, PortRow>> ByCarrier = new(static () =>
        Items.Choose(static row => row.Carrier.Map(carrier => (Carrier: carrier, Row: row)))
            .ToFrozenDictionary(static pair => pair.Carrier, static pair => pair.Row));

    public static Option<PortRow> ForCarrier(Type carrier) =>
        ByCarrier.Value.TryGetValue(carrier, out PortRow? row) ? Optional(row) : None;
}
```

## [05]-[DECLARATION_FOLD]

- Owner: `Ports` is the one declaration fold — side selection is the adder argument's static type, every plan folds through the accumulating carrier so a malformed pin roster reports every violation at once, and each minted `IParameter` realizes its trims and persistent data in the same pass, so declaration and realization are one act.
- Entry: `Declare` gates side and hidden capability on the row columns before any binding runs, binds through the row, and returns the realized parameters in declaration order; `Realize` re-applies trims and persistent data through the verified `ComponentParameters.Input(int)`/`Output(int)` accessors at maintenance passes; `DeclareEnum<T>` is the generic enum-pin entry over the verified modular `AddEnum<T>`/`AddHiddenEnum<T>` pair.
- Receipt: declaration returns `Validation<Error, Seq<IParameter>>` carrying every refused side, refused hidden emission, host rejection, and trim refusal at once.
- Growth: a new maintenance projection is one fold over the returned parameter seq; enum wire representation stays the seed value's own type.
- Boundary: a plan whose row refuses the declared side or hidden emission never reaches the adder — the gate fires before the binding delegate, so the throwing columns are structurally unreachable.

```csharp signature
// --- [OPERATIONS] ------------------------------------------------------------------------

public static class Ports {
    public static Validation<Error, Seq<IParameter>> Declare(ModularInputAdder adder, Seq<PinPlan> plans, Op? key = null) =>
        plans.Traverse(plan => Minted(plan, PinSide.Input, () => plan.Kind.BindInput(adder, plan), key.OrDefault()).ToValidation()).As();

    public static Validation<Error, Seq<IParameter>> Declare(ModularOutputAdder adder, Seq<PinPlan> plans, Op? key = null) =>
        plans.Traverse(plan => Minted(plan, PinSide.Output, () => plan.Kind.BindOutput(adder, plan), key.OrDefault()).ToValidation()).As();

    public static Fin<IParameter> DeclareEnum<T>(ModularInputAdder adder, PinPlan plan, T seed, Op? key = null) where T : struct, Enum {
        Op op = key.OrDefault();
        string category = plan.Category.IfNone("");
        Eto.Drawing.Color colour = plan.Colour.IfNone(Eto.Drawing.Colors.Transparent);
        return Hosted.Bound<IParameter>(() => plan.Visibility == PinVisibility.Shown
                ? adder.AddEnum(plan.Name, plan.Nick, plan.Info, category, colour, seed, plan.Access.Host, plan.Presence.Host)
                : adder.AddHiddenEnum(plan.Name, plan.Nick, plan.Info, category, colour, seed, plan.Access.Host, plan.Presence.Host), op)
            .Bind(parameter => plan.Realize(parameter, op).Map(_ => parameter));
    }

    public static Validation<Error, Unit> Realize(ComponentParameters parameters, Seq<PinPlan> inputs, Seq<PinPlan> outputs, Op? key = null) =>
        (inputs.Map(static (plan, index) => (Plan: plan, Index: index))
                .Traverse(row => row.Plan.Realize(parameters.Input(row.Index), key.OrDefault()).ToValidation())
                .As(),
            outputs.Map(static (plan, index) => (Plan: plan, Index: index))
                .Traverse(row => row.Plan.Realize(parameters.Output(row.Index), key.OrDefault()).ToValidation())
                .As())
            .Apply(static (_, _) => unit)
            .As();

    private static Fin<IParameter> Minted(PinPlan plan, PinSide side, Func<IParameter> bind, Op key) =>
        (plan.Kind.Sides.Accepts(side), plan.Visibility == PinVisibility.Shown || plan.Kind.HiddenSides.Accepts(side)) switch {
            (false, _) => Fin.Fail<IParameter>(new GhFault.Refused(key, $"{plan.Kind.Key}:{side}")),
            (_, false) => Fin.Fail<IParameter>(new GhFault.Refused(key, $"{plan.Kind.Key}:{nameof(PinVisibility.Hidden)}:{side}")),
            _ => Hosted.Bound(bind, key).Bind(parameter => plan.Realize(parameter, key).Map(_ => parameter)),
        };
}
```

## [06]-[RESEARCH]

- [CARRIER_CENSUS]-[OPEN]: the pin VALUE types behind the `None` carrier rows — the minted parameter types are verified (`AddPath` returns `PathParameter`, `AddView` returns `ViewParameter`, `AddDot` returns `TextDotParameter`, `AddTopological` returns `ConnectionParameter`), so the census resolves each parameter's carried value type through `uv run python -m tools.assay api query <Parameter> --key gh2` and flips the row's `Option.None`.
- [PRESET_SETTER]-[OPEN]: whether `IParameter.PresetsWeak` admits assignment for preset seeding; verify the setter through the decompile rail before `PinPlan` grows a presets column.
- [PERSISTENT_SHAPE]-[OPEN]: the concrete payload contract behind `IParameter.PersistentDataWeak` and whether the property admits assignment; verify whether it accepts a bare value, an `IPear`, or an `ITree` so `PinPlan.Persistent` narrows from `object`.
- [TYPE_ASSISTANT]-[OPEN]: the `IParameter.TypeAssistantWeak` assignment contract and the `ITypeAssistant` shape — `IDataAccess.GetItemWithTypeAssistant`/`GetItemWithCurveAssistant`/`GetItemWithSurfaceAssistant` are verified reads; a `PinPlan` assistant column and a `GardenData` assistant-read arm land together once the shapes verify.
- [HINT_OVERLOADS]-[OPEN]: the regular-adder hint overloads — `AddNumber(..., UiNumber, ...)`, `AddInteger(..., UiInteger, ...)`, `AddBoolean(..., (string, string), (string, string), ...)`, `AddRandom(string usage = null)` — are verified members; a `PinPlan` hint column pends the `UiNumber`/`UiInteger` namespaces and a modular counterpart census.
