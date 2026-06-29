# [MATERIALS_GRAPH]

The node-graph appearance engine and the polymorphic library. One `AppearanceNode` `[Union]` closes the node-kind family — `Input`, `Texture`, `Math`, `Mix`, `Normal`, `BsdfOutput` — over a typed `PortValue` channel set; one `MaterialGraph.Evaluate` fold topologically orders the node DAG and folds each node's arm into a `PortEnvironment`, terminating at the single `BsdfOutput` node that assembles a `SurfaceShade`. One `MaterialParameters` record is the canonical parameter vector (base color, metalness, roughness, IOR, transmission, sheen, clearcoat, subsurface, anisotropy, emission), and one `MaterialLibrary` `FrozenDictionary<MaterialId, MaterialParameters>` is the seed of the 100-material catalog — metal, glass, plastic, skin, fabric, car paint, wax, ceramic, liquid, gas — as DATA ROWS; the 32 transcribed rows are the seed and the remaining rows are pure data additions, not new types. A new material is `MaterialLibrary.Rows[MaterialId.Of("metal.titanium")] = new MaterialParameters(...)`, a row of values, never a `TitaniumMaterial` type. The page composes the Rasm/Vectors `Direction`/`VectorFrame` shading frame (never re-minting a vector), Wacton.Unicolour directly as the scene-linear/spectral color owner for every base-color and emission conversion (never re-minting a `ColourSpace`), and the `bsdf#LOBE_FAMILY` lobe family for the terminal shade (never re-deriving lobe math here). The masonry-assignment consumer generalizes through the `MaterialId` row: a masonry `Profile` maps to a `MaterialId`, never to a profile-specific material type.

## [01]-[INDEX]

- [01]-[MATERIAL_GRAPH]: the `AppearanceNode` node union, the `PortValue` channel set, the topological evaluation fold, and the `SurfaceShade` sink.
- [02]-[MATERIAL_LIBRARY]: the `MaterialParameters` canonical row, the `MaterialId` key, the 100-row catalog table, the profile-assignment generalization, the `NearestChecker`/`Named` Datasets validation seam, the three physical-reproducibility gates by domain — display RGB (`SurfaceShade.InGamut`), Pointer real-surface (`PointerAdmit`/`MapToPointer`), and MacAdam spectral-limit (`SpectralAdmit`/`MapToSpectral`) — plus the `CvdPreview` accessibility projection.

## [02]-[MATERIAL_GRAPH]

- Owner: `MaterialGraph` over `AppearanceNode`
- Cases: `Input` (constant/parameter source) · `Texture` (UV-sampled source via `texture#TEXTURE_UV`) · `Math` (closed scalar/vector op over upstream ports) · `Mix` (parameterized blend of two ports) · `Normal` (tangent-space perturbation of the shading frame) · `BsdfOutput` (the single sink assembling the closed lobe set into a `SurfaceShade`)
- Entry: `public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key)` — `Fin<T>` aborts on a cyclic DAG (`MaterialFault.Graph`, key-correlated), an unbound port reference, or a port-type mismatch; `Compile` resolves the topological order once and freezes it, `Shade` is the per-sample re-entry over a compiled order; `MaterialGraph.Default` is the canonical Disney-principled wiring every library row drives through.
- Packages: Rasm (project — `Direction`/`VectorFrame`/`Vector3d`), Wacton.Unicolour (color/spectral compose), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`)
- Growth: a new appearance operation is one `AppearanceNode` case (or one `MathOp`/`MixOp` row on the existing `Math`/`Mix` arms); a new port channel is one `PortValue` case carrying its CLR carrier; a new lobe assembled at the sink is one `BsdfLobe` `[Union]` case on the `bsdf` page — never a per-effect graph variant and never a sibling node type. The `AppearanceNode` union and `PortValue` set are the MaterialX 1.39 node-graph alignment target framed at `[4]-[RESEARCH]`.
- Boundary: the node DAG is the only appearance-program shape — a per-material hand-written shade function is the deleted form; `PortValue` is the only inter-node channel and carries scalar/`Vector3d`/color/`Direction` polarities so a node arm reads typed ports and never `object`; the `Color`→`Scalar` projection is the Rec709 scene-linear luminance dot `0.2126 R + 0.7152 G + 0.0722 B`, never a red-channel read, so a mask pulled from a color is photometrically weighted and cannot silently bias to red; the `Texture` arm composes `texture#TEXTURE_UV` and never re-implements sampling; the `Normal` arm perturbs the composed Rasm/Vectors `VectorFrame` (tangent·bitangent·normal) and never re-mints a basis; the `BsdfOutput` arm composes the `bsdf#LOBE_FAMILY` closed lobe family for the final scattering and the directly-consumed Wacton.Unicolour `RgbConfiguration.Acescg` scene-linear color owner for color resolution — the lobe math lives on the `bsdf` page and is never re-derived here; the `BsdfOutput` sink resolves through `Assemble`, never a port write, so the environment carries no dead entry under the sink id and a downstream node cannot read a phantom `Scalar(1.0)`; the `Math`/`Mix` arms fold over their `MathOp`/`MixOp` SmartEnum by data so a new operation is a row, never a new arm; the `MathOp.Fresnel` row supplies only the Schlick angular weight `(1−cosθ)⁵` for a `Mix` lobe blend and the full Fresnel term lives on `bsdf#MICROFACET_KERNEL`, never re-derived here; the `Mix` color arms blend in scene-linear `Acescg` through `Unicolour.Mix` and the channel composites, never a hand-rolled raw-double channel lerp; `Compile` runs Kahn topological ordering once and `Shade` re-enters over the frozen order so per-sample evaluation never re-sorts — the Kahn sort is the page's one `[EXPRESSION_SPINE]` kernel exemption, the indegree `Dictionary`, ready `Queue`, and order `List` mutated by index through `CollectionsMarshal.GetValueRefOrNullRef` over the bounded once-per-compile pass, every other surface on the page expression-bodied and the per-sample `Shade` fold immutable; the indegree seed `Distinct()`-deduplicates per-node dependencies so a `Math` node authored with `Lhs == Rhs` counts one edge and a multi-edge DAG sorts correctly instead of stranding a node above zero; the `PortEnvironment` fold projects a new frozen `HashMap` per compiled order and never mutates a dictionary; `MaterialGraph.Default` carries the geometric frame unperturbed through one `Normal` node at `Strength 0` whose identity tangent-space sample `(0.5,0.5,1.0)` decodes to `+Z`, so a library row is parameters evaluated through this one standard graph, never a per-row graph type; a cycle, a dangling port, or a duplicate sink rails `Fin.Fail` and never propagates a NaN shade outward.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<int>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, int>]
public readonly partial struct PortId;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MathOp {
    public static readonly MathOp Add = new("add", static (l, r) => new PortValue.Vector(l.AsVector + r.AsVector));
    public static readonly MathOp Multiply = new("multiply", static (l, r) => new PortValue.Scalar(l.AsScalar * r.AsScalar));
    public static readonly MathOp Scale = new("scale", static (l, r) => new PortValue.Vector(l.AsVector * r.AsScalar));
    public static readonly MathOp Power = new("power", static (l, r) => new PortValue.Scalar(System.Math.Pow(l.AsScalar, r.AsScalar)));
    public static readonly MathOp DotProduct = new("dot", static (l, r) => new PortValue.Scalar(l.AsVector * r.AsVector));
    public static readonly MathOp Clamp01 = new("clamp01", static (l, _) => new PortValue.Scalar(System.Math.Clamp(l.AsScalar, 0.0, 1.0)));
    public static readonly MathOp OneMinus = new("one-minus", static (l, _) => new PortValue.Scalar(1.0 - l.AsScalar));
    public static readonly MathOp Fresnel = new("fresnel-weight", static (l, r) => new PortValue.Scalar(NodeEvaluator.SchlickWeight(System.Math.Clamp(l.AsVector * r.AsVector, 0.0, 1.0))));

    [UseDelegateFromConstructor]
    public partial PortValue Apply(PortValue lhs, PortValue rhs);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MixOp {
    public static readonly MixOp Lerp = new("lerp", static (a, b, t) => new PortValue.Color(a.AsColor.Mix(b.AsColor, ColourSpace.RgbLinear, t, premultiplyAlpha: false)));
    public static readonly MixOp Multiply = new("multiply", static (a, b, _) => new PortValue.Color(NodeEvaluator.ChannelCompose(a.AsColor, b.AsColor, static (p, q) => p * q)));
    public static readonly MixOp Screen = new("screen", static (a, b, _) => new PortValue.Color(NodeEvaluator.ChannelCompose(a.AsColor, b.AsColor, static (p, q) => 1.0 - (1.0 - p) * (1.0 - q))));
    public static readonly MixOp Overlay = new("overlay", static (a, b, _) => new PortValue.Color(NodeEvaluator.ChannelCompose(a.AsColor, b.AsColor, static (p, q) => p < 0.5 ? 2.0 * p * q : 1.0 - 2.0 * (1.0 - p) * (1.0 - q))));

    [UseDelegateFromConstructor]
    public partial PortValue Apply(PortValue a, PortValue b, double t);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PortValue {
    private PortValue() { }

    public sealed record Scalar(double Value) : PortValue;
    public sealed record Color(Unicolour Linear) : PortValue;
    public sealed record Vector(Vector3d Value) : PortValue;
    public sealed record Frame(VectorFrame Value) : PortValue;

    public double AsScalar => Switch(scalar: static s => s.Value, color: static c => Luminance(c.Linear), vector: static v => v.Value.Length, frame: static _ => 1.0);

    static double Luminance(Unicolour c) => 0.2126 * c.RgbLinear.R + 0.7152 * c.RgbLinear.G + 0.0722 * c.RgbLinear.B;
    public Vector3d AsVector => Switch(scalar: static s => new Vector3d(s.Value, s.Value, s.Value), color: static c => RgbLinearVector(c.Linear), vector: static v => v.Value, frame: static f => f.Value.ZAxis);
    public Unicolour AsColor => Switch(scalar: static s => GreyLinear(s.Value), color: static c => c.Linear, vector: static v => VectorLinear(v.Value), frame: static f => VectorLinear(f.Value.ZAxis));

    static Vector3d RgbLinearVector(Unicolour c) => new(c.RgbLinear.R, c.RgbLinear.G, c.RgbLinear.B);
    static Unicolour GreyLinear(double g) => new(SceneLinear, ColourSpace.RgbLinear, g, g, g);
    static Unicolour VectorLinear(Vector3d v) => new(SceneLinear, ColourSpace.RgbLinear, v.X, v.Y, v.Z);
    internal static readonly Configuration SceneLinear = new(RgbConfiguration.Acescg);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ShadePoint(Point3d Position, VectorFrame Frame, Vector3d ViewDirection, double U, double V) {
    public static Fin<ShadePoint> Of(Point3d position, Vector3d normal, Vector3d view, Option<Vector3d> tangentHint, double u, double v, Context context, Op key) =>
        from frame in VectorFrame.Of(origin: position, normal: normal, xHint: tangentHint, context: context, key: key)
        from outgoing in Direction.Of(value: view, context: context, key: key)
        select new ShadePoint(position, frame, outgoing.Value, u, v);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppearanceNode(PortId Id) {
    private protected AppearanceNode(PortId id) : this(Id: id) { }

    public sealed record Input(PortId Id, Func<MaterialParameters, PortValue> Pull) : AppearanceNode(Id);
    public sealed record Texture(PortId Id, Func<double, double, PortValue> Sample) : AppearanceNode(Id);
    public sealed record Math(PortId Id, MathOp Op, PortId Lhs, Option<PortId> Rhs) : AppearanceNode(Id);
    public sealed record Mix(PortId Id, MixOp Op, PortId A, PortId B, PortId Factor) : AppearanceNode(Id);
    public sealed record Normal(PortId Id, PortId Source, double Strength) : AppearanceNode(Id);
    public sealed record BsdfOutput(PortId Id, PortId BaseColor, PortId Metalness, PortId Roughness, PortId NormalFrame, PortId Emission) : AppearanceNode(Id);

    public Seq<PortId> Dependencies =>
        Switch(
            input: static _ => Seq<PortId>(),
            texture: static _ => Seq<PortId>(),
            math: static m => m.Rhs.Match(Some: r => Seq(m.Lhs, r), None: () => Seq1(m.Lhs)),
            mix: static x => Seq(x.A, x.B, x.Factor),
            normal: static n => Seq1(n.Source),
            bsdfOutput: static o => Seq(o.BaseColor, o.Metalness, o.Roughness, o.NormalFrame, o.Emission));
}

public sealed record SurfaceShade(Unicolour BaseColorLinear, double Metalness, double Roughness, VectorFrame ShadingFrame, Unicolour EmissionLinear) {
    public bool InGamut => BaseColorLinear.IsInRgbGamut && EmissionLinear.IsInRgbGamut;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed record PortEnvironment(LanguageExt.HashMap<PortId, PortValue> Resolved) {
    public static readonly PortEnvironment Empty = new(LanguageExt.HashMap<PortId, PortValue>.Empty);
    public Fin<PortValue> Read(PortId id, Op key) => Resolved.Find(id).ToFin(MaterialFault.Graph(key, $"<unbound-port:{id.Value}>"));
    public PortEnvironment With(PortId id, PortValue value) => this with { Resolved = Resolved.AddOrUpdate(id, value) };
}

public static class NodeEvaluator {
    public static Fin<Option<PortValue>> Evaluate(AppearanceNode node, ShadePoint point, MaterialParameters parameters, PortEnvironment env, Op key) =>
        node.Switch(
            input: i => Fin.Succ(Some(i.Pull(parameters))),
            texture: t => Fin.Succ(Some(t.Sample(point.U, point.V))),
            math: m => EvaluateMath(m, env, key).Map(Some),
            mix: x => EvaluateMix(x, env, key).Map(Some),
            normal: n => EvaluateNormal(n, point, env, key).Map(Some),
            bsdfOutput: _ => Fin.Succ(Option<PortValue>.None));

    static Fin<PortValue> EvaluateMath(AppearanceNode.Math m, PortEnvironment env, Op key) =>
        from lhs in env.Read(m.Lhs, key)
        from rhs in m.Rhs.Match(Some: r => env.Read(r, key), None: () => Fin.Succ<PortValue>(new PortValue.Scalar(0.0)))
        select m.Op.Apply(lhs, rhs);

    internal static double SchlickWeight(double cosTheta) { double m = System.Math.Clamp(1.0 - cosTheta, 0.0, 1.0); double m2 = m * m; return m2 * m2 * m; }

    static Fin<PortValue> EvaluateMix(AppearanceNode.Mix x, PortEnvironment env, Op key) =>
        from a in env.Read(x.A, key)
        from b in env.Read(x.B, key)
        from f in env.Read(x.Factor, key)
        select x.Op.Apply(a, b, System.Math.Clamp(f.AsScalar, 0.0, 1.0));

    internal static Unicolour ChannelCompose(Unicolour a, Unicolour b, Func<double, double, double> f) =>
        new(PortValue.SceneLinear, ColourSpace.RgbLinear, f(a.RgbLinear.R, b.RgbLinear.R), f(a.RgbLinear.G, b.RgbLinear.G), f(a.RgbLinear.B, b.RgbLinear.B));

    static Fin<PortValue> EvaluateNormal(AppearanceNode.Normal n, ShadePoint point, PortEnvironment env, Op key) =>
        from sample in env.Read(n.Source, key)
        let raw = sample.AsVector
        let tangentSpace = new Vector3d((2.0 * raw.X - 1.0) * n.Strength, (2.0 * raw.Y - 1.0) * n.Strength, 2.0 * raw.Z - 1.0)
        let basis = point.Frame.Value
        let world = (basis.XAxis * tangentSpace.X) + (basis.YAxis * tangentSpace.Y) + (basis.ZAxis * tangentSpace.Z)
        from perturbed in VectorFrame.Of(origin: basis.Origin, normal: world, xHint: Some(basis.XAxis), context: GraphContext.Tolerant, key: key)
        select (PortValue)new PortValue.Frame(perturbed);
}

public sealed record MaterialGraph(Seq<AppearanceNode> Nodes, PortId Sink) {
    public Fin<CompiledGraph> Compile(Op key) {
        LanguageExt.HashMap<PortId, AppearanceNode> byId = Nodes.Fold(LanguageExt.HashMap<PortId, AppearanceNode>.Empty, static (m, n) => m.AddOrUpdate(n.Id, n));
        System.Collections.Generic.Dictionary<PortId, int> indegree = Nodes.ToDictionary(static n => n.Id, n => n.Dependencies.Distinct().Count(d => byId.ContainsKey(d)));
        System.Collections.Generic.Queue<PortId> ready = new(indegree.Where(static kv => kv.Value == 0).Select(static kv => kv.Key));
        System.Collections.Generic.List<AppearanceNode> order = new(Nodes.Count);
        while (ready.Count > 0) {
            PortId id = ready.Dequeue();
            byId.Find(id).IfSome(order.Add);
            foreach (AppearanceNode dependent in Nodes.Filter(n => n.Dependencies.Contains(id)))
                if (--System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrNullRef(indegree, dependent.Id) == 0) ready.Enqueue(dependent.Id);
        }
        return order.Count == Nodes.Count && byId.Find(Sink).Map(static n => n is AppearanceNode.BsdfOutput).IfNone(false)
            ? Fin.Succ(new CompiledGraph(toSeq(order), Sink, byId))
            : Fin.Fail<CompiledGraph>(MaterialFault.Graph(key, order.Count == Nodes.Count ? "<sink-not-bsdf-output>" : "<cyclic-appearance-graph>"));
    }

    public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key) =>
        Compile(key).Bind(compiled => compiled.Shade(point, parameters, key));

    public static readonly MaterialGraph Default = BuildDefault();

    static MaterialGraph BuildDefault() {
        PortId baseColor = PortId.Of(1), metalness = PortId.Of(2), roughness = PortId.Of(3), normalSrc = PortId.Of(4), normal = PortId.Of(5), emission = PortId.Of(6), sink = PortId.Of(7);
        return new MaterialGraph(Seq<AppearanceNode>(
            new AppearanceNode.Input(baseColor, static p => new PortValue.Color(p.BaseColor)),
            new AppearanceNode.Input(metalness, static p => new PortValue.Scalar(p.Metalness)),
            new AppearanceNode.Input(roughness, static p => new PortValue.Scalar(p.Roughness)),
            new AppearanceNode.Input(normalSrc, static _ => new PortValue.Vector(new Vector3d(0.5, 0.5, 1.0))),
            new AppearanceNode.Input(emission, static p => new PortValue.Color(p.Emission)),
            new AppearanceNode.Normal(normal, normalSrc, Strength: 0.0),
            new AppearanceNode.BsdfOutput(sink, baseColor, metalness, roughness, normal, emission)), sink);
    }
}

public sealed record CompiledGraph(Seq<AppearanceNode> Order, PortId Sink, LanguageExt.HashMap<PortId, AppearanceNode> ById) {
    public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Op key) =>
        Order.Fold(Fin.Succ(PortEnvironment.Empty), (acc, node) => acc.Bind(env => NodeEvaluator.Evaluate(node, point, parameters, env, key).Map(value => value.Match(Some: v => env.With(node.Id, v), None: () => env))))
            .Bind(env => ById.Find(Sink).ToFin(MaterialFault.Graph(key, "<sink-missing>")).Bind(sink => Assemble((AppearanceNode.BsdfOutput)sink, point, env, key)));

    static Fin<SurfaceShade> Assemble(AppearanceNode.BsdfOutput sink, ShadePoint point, PortEnvironment env, Op key) =>
        from baseColor in env.Read(sink.BaseColor, key)
        from metalness in env.Read(sink.Metalness, key)
        from roughness in env.Read(sink.Roughness, key)
        from normalFrame in env.Read(sink.NormalFrame, key)
        from emission in env.Read(sink.Emission, key)
        let frame = normalFrame switch { PortValue.Frame f => f.Value, _ => point.Frame }
        let shade = new SurfaceShade(baseColor.AsColor, System.Math.Clamp(metalness.AsScalar, 0.0, 1.0), System.Math.Clamp(roughness.AsScalar, 0.0, 1.0), frame, emission.AsColor)
        from valid in guard(shade.InGamut, MaterialFault.Gamut(key, $"<shade-out-of-gamut:{shade.BaseColorLinear.Hex}>"))
        select shade;
}
```

## [03]-[MATERIAL_LIBRARY]

- Owner: `MaterialLibrary` over `MaterialParameters` keyed by `MaterialId`
- Cases: 100 rows — `metal.gold`/`metal.copper`/`metal.aluminum`/`metal.titanium`/`metal.iron`/`metal.silver`/`metal.chrome`/`metal.brass`, `glass.crown`/`glass.flint`/`liquid.water`/`liquid.oil`/`gas.cavity`, `plastic.abs`/`plastic.pvc`/`rubber.matte`, `skin.caucasian`/`skin.deep`, `fabric.velvet`/`fabric.silk`/`fabric.denim`, `paint.car-metallic`/`paint.clearcoat`/`ceramic.glazed`/`ceramic.porcelain`, `wax.beeswax`/`wax.candle`, `stone.jade`/`stone.marble`, `wood.oak`, `coat.gold-leaf`, `gem.diamond` — each a row of `MaterialParameters` values, ZERO per-material types
- Entry: `public static Fin<MaterialParameters> Lookup(MaterialId id, Op key)` — `Fin<T>` aborts on an unregistered id (`MaterialFault.Parameter`, key-correlated); the `Of` overload admits an ad-hoc parameter vector through the same row validation a registered row passes; `Assign` is the profile-generalization seam mapping a masonry `Profile` to a `MaterialId` row.
- Packages: Wacton.Unicolour (base-color/emission construction; the `IsInPointerGamut`/`MapToPointerGamut` Pointer real-surface gamut accessors, the `IsInMacAdamLimits`/`MapToMacAdamLimits`/`IsImaginary` MacAdam spectral-limit accessors, and the `Simulate(Cvd, double severity)` colour-vision-deficiency projection over the `Cvd` 8-member selector, the catalogued-but-uncalled gamut/CVD surface mined here at its deepest accessor), Wacton.Unicolour.Datasets (composed for ColorChecker/Macbeth `Macbeth.All` validation and `Css`/`Xkcd`/`Nord` named-colour resolution, validation/reference only), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`)
- Growth: a new material is one `MaterialLibrary.Rows` entry — a `MaterialId` key and a `MaterialParameters` value. A new appearance parameter shared by ALL materials is one column on `MaterialParameters` (defaulted so existing rows are unaffected). A new gamut domain (the display RGB gate, the Pointer real-surface gate, the MacAdam spectral-limit gate) is one accessor predicate by domain, never a collapse of the three into one gate; a new accessibility-preview is one read-only projection over the `Cvd` selector, never a stored row. There is NO per-material type, NO `GoldMaterial`/`GlassMaterial` class, NO `MetalFactory`/`PlasticFactory`, and NO per-family graph variant — the named defect is a second material surface; the cure is a row. The conductor and dielectric rows are the measured-spectral grounding target framed at `[4]-[RESEARCH]`.
- Boundary: `MaterialParameters` is the single material concept — its columns are the closed Disney-principled parameter set (base color, metalness, roughness, specular tint, anisotropy, IOR, transmission, transmission roughness, sheen, sheen tint, clearcoat, clearcoat roughness, subsurface weight, subsurface radius, emission color, emission luminance) that every measured material parameterizes; base color and emission are constructed once through Wacton.Unicolour scene-linear `Acescg` so the table carries spectrally-grounded colors, never raw byte triples; the metalness/IOR pairing is the conductor/dielectric discriminant the `bsdf#LOBE_FAMILY` lobe weights read (metalness=1 selects the conductor lobe with the base color as F0, metalness=0 the dielectric lobe with IOR-derived F0), so a "metal" and a "plastic" differ only by the metalness, IOR, and roughness columns — never by type; transmission>0 with IOR selects the dielectric-transmission lobe so glass, water, the sealed IGU cavity gas (`gas.cavity`, IOR 1.0 so its transmissive interface carries no Fresnel and the `Profiles/glazing#GLAZING_FAMILY` cavity layers shade as a clear non-refracting fill rather than the `liquid.water` proxy), and gems are rows differing only in IOR and transmission roughness; subsurface weight>0 routes the subsurface lobe so skin, wax, jade, and marble are rows differing only in subsurface radius (the per-channel mean-free-path carried as the validated three-band `SubsurfaceRadius` `[ComplexValueObject]`, a negative or non-finite millimetre band unrepresentable at `Create` so the inline negative-mfp guard `MaterialParameters.Of` once carried is gone); sheen>0 routes the sheen lobe so velvet, silk, and denim are rows differing only in sheen and roughness; clearcoat>0 layers the clearcoat lobe so car paint and glazed ceramic are rows differing only in clearcoat and clearcoat roughness; the profile consumer generalizes through `Assign`, which maps a masonry `Profile` to a `MaterialId` row and NEVER mints a profile-specific material — `Profiles/masonry#PROFILE_FAMILY` is the cross-section owner the engine reads, never modifies, and an unmapped key falls back to the neutral `ceramic.porcelain` row rather than a fault so the profile consumer always shades; the Wacton.Unicolour.Datasets composition is validation/reference only — `NearestChecker` gates a row's base color against the nearest `Macbeth.All` ColorChecker patch by `Unicolour.Difference(DeltaE.Ciede2000)` (a drift beyond tolerance rails `MaterialFault.Gamut`) and `Named` resolves a row's base color from a passed `Css`/`Xkcd`/`Nord` named `Unicolour` instead of a hand-keyed triple, the observer CMFs/illuminant SPDs/reflectance staying on the main Wacton.Unicolour owner the Datasets package does not carry; there are THREE gamut gates BY DOMAIN, never one collapse — `SurfaceShade.InGamut` reads the display `IsInRgbGamut` (the preview-reproducibility gate every row evaluates through), `PointerAdmit` reads the Pointer real-surface `IsInPointerGamut` (the physical-reproducibility gate a pigment-mixed reflectance must pass, the predicate `Appearance/finish#FINISH` imports for its admission), and `SpectralAdmit` reads the MacAdam optimal-colour `IsInMacAdamLimits` (the absolute spectral-locus bound a reflectance physically reachable at its luminance must satisfy, a reflectance beyond the spectral locus first caught by `IsImaginary` so an imaginary colour rails before the MacAdam test), each domain-gate railing the SAME `MaterialFault.Gamut` case with its own domain reason string (the case is reused across all three, never a second fault) and `MapToPointer`/`MapToSpectral` returning the nearest in-gamut Pointer/MacAdam `Unicolour` for the recoverable path — the gate ladder runs display RGB inside Pointer real-surface inside the MacAdam spectral-locus bound, the three never collapsed because each names a distinct physical reproducibility domain; the `CvdPreview` accessibility projection reads `Unicolour.Simulate(Cvd, double severity)` over the `Cvd` 8-member deficiency selector as a READ-ONLY preview the color-specification seam consumes, never a stored library column; every row evaluates to an in-gamut `SurfaceShade` through the same `MaterialGraph`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Rasm.Element;                  // MaterialId — the SEAM material-identity owner this page references, never re-declares

// --- [TYPES] -------------------------------------------------------------------------------
// MaterialId is a CROSS-PACKAGE identity owned by the seam (`Rasm.Element` `Composition/material#MATERIAL_COMPOSITION`
// declares the one `[ValueObject<string>]` material key, ordinal-ignore-case). The `Material`/`MaterialComposition`/
// `MaterialLayer`/`MaterialConstituent` seam types and every Materials catalogue key on it, so this page composes the
// seam type rather than declaring a parallel `family.name` identity — the prior local `MaterialId` is RETIRED.
// The seam key comparer travels with the type (the seam owns it); the shipped `ComparerAccessors.StringOrdinal` remains the
// ordinal string policy the local `PortId`/`MathOp`/`MixOp` keys compose, NOT the material identity.

[ComplexValueObject]
public readonly partial struct SubsurfaceRadius {
    public double R { get; }
    public double G { get; }
    public double B { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double r, ref double g, ref double b) {
        if (!double.IsFinite(r) || !double.IsFinite(g) || !double.IsFinite(b) || r < 0.0 || g < 0.0 || b < 0.0)
            validationError = new ValidationError($"<subsurface-radius-negative-mfp:{r:R},{g:R},{b:R}>");
    }

    public static readonly SubsurfaceRadius None = Create(0.0, 0.0, 0.0);
    public double Magnitude => Math.Sqrt(R * R + G * G + B * B);
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record MaterialParameters(
    Unicolour BaseColor,
    double Metalness,
    double Roughness,
    double SpecularTint,
    double Anisotropy,
    double Ior,
    double Transmission,
    double TransmissionRoughness,
    double Sheen,
    double SheenTint,
    double Clearcoat,
    double ClearcoatRoughness,
    double Subsurface,
    SubsurfaceRadius SubsurfaceRadius,
    Unicolour Emission,
    double EmissionLuminance) {

    public static Fin<MaterialParameters> Of(MaterialParameters candidate, Op key) =>
        guard(InUnit(candidate.Metalness) && InUnit(candidate.Roughness) && InUnit(candidate.Transmission) && InUnit(candidate.Sheen) && InUnit(candidate.Clearcoat) && InUnit(candidate.Subsurface), MaterialFault.Parameter(key, "<weight-out-of-unit>"))
            .Bind(_ => guard(InIorRange(candidate.Ior, candidate.Metalness), MaterialFault.Parameter(key, $"<ior-out-of-range:{candidate.Ior}@metalness={candidate.Metalness}>")))
            .Bind(_ => guard(candidate.BaseColor.IsInRgbGamut && candidate.Emission.IsInRgbGamut, MaterialFault.Gamut(key, "<row-color-out-of-gamut>")))
            .Map(_ => candidate);

    static bool InUnit(double v) => double.IsFinite(v) && v is >= 0.0 and <= 1.0;
    static bool InIorRange(double ior, double metalness) => double.IsFinite(ior) && (metalness >= 1.0 ? ior is >= 0.1 and <= 3.0 : ior is >= 1.0 and <= 2.5);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialLibrary {
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
    static readonly Unicolour Black = Linear(0.0, 0.0, 0.0);
    static readonly SubsurfaceRadius NoScatter = SubsurfaceRadius.None;
    static SubsurfaceRadius Scatter(double r, double g, double b) => SubsurfaceRadius.Create(r, g, b);

    public static readonly FrozenDictionary<MaterialId, MaterialParameters> Rows = new (MaterialId Id, MaterialParameters Row)[] {
        (MaterialId.Of("metal.gold"),      new(Linear(1.000, 0.766, 0.336), 1.0, 0.12, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.copper"),    new(Linear(0.955, 0.638, 0.538), 1.0, 0.18, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.aluminum"),  new(Linear(0.913, 0.922, 0.924), 1.0, 0.08, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.silver"),    new(Linear(0.972, 0.960, 0.915), 1.0, 0.05, 0.0,  0.0,  0.155, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.iron"),      new(Linear(0.560, 0.570, 0.580), 1.0, 0.35, 0.0,  0.0,  2.950, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.titanium"),  new(Linear(0.542, 0.497, 0.449), 1.0, 0.28, 0.0,  0.0,  2.740, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.chrome"),    new(Linear(0.550, 0.556, 0.554), 1.0, 0.02, 0.0,  0.0,  3.000, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.brass"),     new(Linear(0.887, 0.789, 0.434), 1.0, 0.22, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("glass.crown"),     new(Linear(0.960, 0.970, 0.980), 0.0, 0.02, 0.0,  0.0,  1.520, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("glass.flint"),     new(Linear(0.950, 0.945, 0.960), 0.0, 0.03, 0.0,  0.0,  1.620, 1.0, 0.05, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("liquid.water"),    new(Linear(0.980, 0.990, 0.995), 0.0, 0.0,  0.0,  0.0,  1.333, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("liquid.oil"),      new(Linear(0.920, 0.880, 0.620), 0.0, 0.04, 0.0,  0.0,  1.470, 0.9, 0.08, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("gas.cavity"),      new(Linear(0.998, 0.998, 0.998), 0.0, 0.0,  0.0,  0.0,  1.000, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("gem.diamond"),     new(Linear(0.990, 0.990, 0.995), 0.0, 0.0,  0.0,  0.0,  2.417, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("stone.jade"),      new(Linear(0.270, 0.560, 0.380), 0.0, 0.35, 0.0,  0.0,  1.660, 0.4, 0.30, 0.0, 0.0, 0.0, 0.0, 0.6, Scatter(4.0, 8.0, 5.0), Black, 0.0)),
        (MaterialId.Of("plastic.abs"),     new(Linear(0.800, 0.050, 0.050), 0.0, 0.30, 0.5,  0.0,  1.460, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("plastic.pvc"),     new(Linear(0.180, 0.380, 0.760), 0.0, 0.45, 0.4,  0.0,  1.520, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("rubber.matte"),    new(Linear(0.040, 0.040, 0.040), 0.0, 0.85, 0.0,  0.0,  1.519, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("skin.caucasian"), new(Linear(0.640, 0.430, 0.370), 0.0, 0.45, 0.0,  0.0,  1.400, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, Scatter(3.67, 1.37, 0.68), Black, 0.0)),
        (MaterialId.Of("skin.deep"),       new(Linear(0.330, 0.180, 0.130), 0.0, 0.50, 0.0,  0.0,  1.400, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, Scatter(2.10, 0.80, 0.40), Black, 0.0)),
        (MaterialId.Of("fabric.velvet"),   new(Linear(0.380, 0.030, 0.080), 0.0, 0.90, 0.0,  0.0,  1.460, 0.0, 0.0, 1.0, 0.8, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("fabric.silk"),     new(Linear(0.700, 0.620, 0.480), 0.0, 0.35, 0.2,  0.6,  1.460, 0.0, 0.0, 0.6, 0.3, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("fabric.denim"),    new(Linear(0.150, 0.230, 0.380), 0.0, 0.80, 0.0,  0.0,  1.460, 0.0, 0.0, 0.4, 0.5, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("paint.car-metallic"), new(Linear(0.090, 0.020, 0.220), 0.85, 0.30, 0.0, 0.0, 1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("paint.clearcoat"), new(Linear(0.700, 0.700, 0.700), 0.0, 0.40, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.03, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("ceramic.glazed"),  new(Linear(0.880, 0.850, 0.780), 0.0, 0.10, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.9, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("ceramic.porcelain"), new(Linear(0.930, 0.920, 0.900), 0.0, 0.20, 0.0, 0.0, 1.504, 0.0, 0.0, 0.0, 0.0, 0.3, 0.10, 0.4, Scatter(5.0, 5.0, 5.0), Black, 0.0)),
        (MaterialId.Of("wax.beeswax"),     new(Linear(0.870, 0.700, 0.330), 0.0, 0.55, 0.0,  0.0,  1.443, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.8, Scatter(6.0, 4.0, 1.5), Black, 0.0)),
        (MaterialId.Of("wax.candle"),      new(Linear(0.940, 0.920, 0.850), 0.0, 0.60, 0.0,  0.0,  1.430, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.9, Scatter(8.0, 6.0, 4.0), Black, 0.0)),
        (MaterialId.Of("stone.marble"),    new(Linear(0.870, 0.860, 0.840), 0.0, 0.30, 0.0,  0.0,  1.486, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.5, Scatter(2.19, 2.62, 3.00), Black, 0.0)),
        (MaterialId.Of("wood.oak"),        new(Linear(0.430, 0.270, 0.140), 0.0, 0.55, 0.3,  0.4,  1.530, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("coat.gold-leaf"),  new(Linear(1.000, 0.766, 0.336), 1.0, 0.06, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 1.0, 0.02, 0.0, NoScatter, Black, 0.0)),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);   // seam MaterialId's generated equality (ordinal-ignore-case) keys the row table

    public static Fin<MaterialParameters> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out MaterialParameters? row) ? Fin.Succ(row!) : Fin.Fail<MaterialParameters>(MaterialFault.Parameter(key, $"<unregistered-material:{id.Value}>"));

    public static Fin<MaterialParameters> Of(MaterialParameters candidate, Op key) => MaterialParameters.Of(candidate, key);

    public static Fin<MaterialParameters> Assign(MaterialId appearanceId, Op key) =>
        Rows.ContainsKey(appearanceId) ? Lookup(appearanceId, key) : Lookup(MaterialId.Of("ceramic.porcelain"), key);

    public static MaterialParameters Named(Unicolour reference, MaterialParameters template) =>
        template with { BaseColor = new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, reference.RgbLinear.R, reference.RgbLinear.G, reference.RgbLinear.B) };

    public static Fin<(Unicolour Patch, double DeltaE)> NearestChecker(Unicolour candidate, double tolerance, Op key) =>
        toSeq(Wacton.Unicolour.Datasets.Macbeth.All)
            .Map(patch => (Patch: patch, DeltaE: candidate.Difference(patch, DeltaE.Ciede2000)))
            .Fold(Option<(Unicolour Patch, double DeltaE)>.None, static (best, row) => best.Filter(b => b.DeltaE <= row.DeltaE).IsSome ? best : Some(row))
            .ToFin(MaterialFault.Parameter(key, "<colorchecker-set-empty>"))
            .Bind(nearest => nearest.DeltaE <= tolerance
                ? Fin.Succ(nearest)
                : MaterialFault.Gamut(key, $"<colorchecker-drift:deltaE={nearest.DeltaE:R}>"));

    public static Fin<Unicolour> PointerAdmit(Unicolour reflectance, Op key) =>
        reflectance.IsInPointerGamut
            ? Fin.Succ(reflectance)
            : MaterialFault.Gamut(key, $"<pointer-unreproducible-reflectance:{reflectance.Hex}>");

    public static Unicolour MapToPointer(Unicolour reflectance) => reflectance.MapToPointerGamut();

    public static Fin<Unicolour> SpectralAdmit(Unicolour reflectance, Op key) =>
        reflectance.IsImaginary
            ? MaterialFault.Gamut(key, $"<imaginary-reflectance:{reflectance.Hex}>")
            : reflectance.IsInMacAdamLimits
                ? Fin.Succ(reflectance)
                : MaterialFault.Gamut(key, $"<macadam-unreproducible-reflectance:{reflectance.Hex}>");

    public static Unicolour MapToSpectral(Unicolour reflectance) => reflectance.MapToMacAdamLimits();

    public static Unicolour CvdPreview(Unicolour color, Cvd deficiency, double severity) => color.Simulate(deficiency, System.Math.Clamp(severity, 0.0, 1.0));
}
```

## [04]-[RESEARCH]

- [MATERIALX_GRAPH_INTERCHANGE]: REALIZED at `interchange#MATERIALX_DOCUMENT` — the `AppearanceNode` union and `PortValue` set project onto the MaterialX 1.39 node-graph schema through the `NodeCategory` map so a `MaterialGraph` serializes to and from `.mtlx` (root `<materialx version="1.39">`, `<nodegraph>` of `<node>` with `<input>`/`<output>`), the six node cases mapping to the MaterialX categories (`constant`/`image`/`multiply`/`mix`/`normalmap`/`open_pbr_surface`), the `PortValue` polarities onto the MaterialX typed ports (`float`/`color3`/`vector3`), and the Standard-Surface-to-OpenPBR translation riding the `surface#OPENPBR_SLAB` lowering. The remaining probe is the per-input port-name alignment per node against a reference MaterialX standard-node library and the `System.Xml.Linq` `.mtlx` serialize at the host boundary, not a re-architecture of the evaluation fold.
- [MEASURED_SPECTRAL_LIBRARY]: the conductor grounding is REALIZED at `surface#CONDUCTOR_IOR` — the `ConductorIor` table carries the measured complex refractive index `(η, k)` per RGB band (gold/copper/aluminum/silver/iron/chromium/titanium/brass from the Johnson-Christy / `refractiveindex.info` tables) so a metal row's `bsdf#LOBE_FAMILY` `Conductor` lobe grounds from the measured Fresnel keyed by the row's `metal.<name>` `MaterialId`, the `BaseColor` the perceptual preview seed and the `(η, k)` the shading truth; `ConductorIor.Resolve(family, name)` maps a `metal.*` id to its `ConductorMetal`. The remaining [UPSTREAM-BLOCKED] leg is the measured isotropic 195-wavelength spectral BRDF (EPFL RGL goniophotometer, brdf-loader `.bsdf` format) admitted through the `surface#SPECTRAL_UPSAMPLE` `ToSpd` per band, blocked on a vendored managed `.bsdf` reader at `acquisition#EPFL_RGL_BRDF_LOADER`; the per-band `(η, k)` table is the INTERNAL leg now landed, the spectral curve the host-edge extension.
- [SUBSURFACE_RADIUS_UNITS]: REALIZED — the `SubsurfaceRadius` mean-free-path is the validated three-band `[ComplexValueObject]` carrier over per-channel non-negative finite millimetre bands (`R`/`G`/`B`), authored from the published skin/marble/wax diffusion profiles, gating a negative or non-finite mean-free-path once at `Create` so a degenerate radius is unrepresentable at admission rather than caught by an inline `MaterialParameters.Of` guard. The `bsdf#LOBE_FAMILY` `Subsurface` lobe reads the carrier's `Magnitude` for the Burley diffusion radius and the `interchange#MATERIAL_WIRE` projection reads the `R`/`G`/`B` bands rather than a `Vector3d.X`/`Y`/`Z` decomposition; the `photometric#PHOTOMETRIC` admission coerces emission luminance through the in-folder `MaterialUnits` UnitsNet boundary (the illuminance row gated by `UnitsNet.Illuminance`, never a reach DOWN to the app-platform `Rasm.Compute` units owner) — no unit type crosses the interior signature, the band carrier the millimetre owner.
- [GRAPH_CONTEXT]: the `Normal`/`ShadePoint` arms construct the Rasm/Vectors `VectorFrame` through a tolerant `Context` (`GraphContext.Tolerant`) so a near-degenerate perturbation re-seeds a perpendicular tangent through the Vectors owner rather than faulting mid-shade; the frame owner's own `Fin` rail is the boundary, never a re-minted basis.
