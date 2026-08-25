# [RASM_RHINO_RENDER_SETTINGS]


## [01]-[INDEX]

- [02]-[SOURCE]: `SettingsSource` — the origin union with its `Use` read and `Mutate` undo-bracketed borrow folds.
- [03]-[STATE_RECORDS]: the `SubOwners` custody window, the capability vocabularies, the writable sub-owner states, derived evidence, and `RenderConfig`.
- [04]-[SUN_ASTRONOMY]: `SunProblem`/`SunSolution`/`SunSolver` over the host statics, beside the `SolarFrame`/`SunDerivation`/`SceneSun` descriptor band.
- [05]-[EDIT_RAIL]: `SettingsBody`, `RenderState`, and the `Settings.Run` request/result rail.
- [06]-[AMBIENT_WATCH]: `AmbientPulse` and the `Changed`-broadcast fold over a bounded ring.
- [07]-[SURFACE_LEDGER]: page owner table.

## [02]-[SOURCE]

- Owner: `SettingsSource` `[Union]` — `Live` resolves `RhinoDoc.RenderSettings` inside a `Demand` window, `Archived` resolves the archive-bound `File3dm.Settings.RenderSettings`, and `Free` mints one owned free-floating `RenderSettings` retained until source disposal; `Use` borrows the selected aggregate for exactly one read callback, and `Mutate` borrows it for exactly one mutation callback — the live arm demanding `Mutate`+`Undo` and opening one named `UndoBracket`.
- Law: the origin is the discriminant a consumer carries — the same `GroundPlane` type is document-bound, archive-attached, or free-floating by the host's internal pointer resolution, so no parallel type pair exists on this side of the seam and no live sub-owner leaves the borrow.
- Law: writes are in-place — a bound sub-owner commits through its native pointer, inert `BeginChange`/`EndChange` never appear, and cross-source copy replays one detached total state.
- Law: only the document owns an undo record — archive and detached mutations apply without one; archive persistence occurs at `File3dm.Write`, while detached values remain locally owned.
- Law: `RhinoDoc.RenderSettings` answers a FRESH document-bound wrapper on every read, so the aggregate enters the borrow once and threads — two reads of one property are two wrappers over one native and two instants the `Changed` broadcast can move between.
- Boundary: the document and archive accessors are the document and file-IO catalogs' seam; this union names them once and every settings verb enters through it.
- Packages: `api-rhinocommon-rendersettings.md` (`RenderSettings`, `DocumentOrFreeFloatingBase`, `RhinoDoc.RenderSettings`); `api-rhinocommon-fileio.md` (`File3dm.Settings.RenderSettings`); kernel `Domain/rails` (`Op`, `Op.Catch`, `Op.Need`, `Lease<T>.Acquire`); `Document/session.md` (`DocumentSession.Demand`, `SessionNeed`, `RedrawPolicy`, `IDetachedDocumentResult`), `Document/commit.md` (`DocumentCommit.Sealed`); LanguageExt.Core (`Fin`); Thinktecture.Runtime.Extensions (`[Union]`).

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using NodaTime;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Render;
using Thinktecture;

namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsSource : IDisposable {
    private SettingsSource() { }
    public sealed record Live(DocumentSession Session) : SettingsSource;
    public sealed record Archived(File3dm Archive) : SettingsSource;
    private sealed record Detached(Lease<RenderSettings> Settings) : SettingsSource;

    public static Fin<SettingsSource> Free(Op? key = null) {
        Op op = key.OrDefault();
        return Lease<RenderSettings>.Acquire(mint: static () => new RenderSettings(), key: op)
            .Map(static lease => (SettingsSource)new Detached(Settings: lease));
    }

    internal Fin<TOut> Use<TOut>(Func<RenderSettings, Fin<TOut>> borrow, Op key)
        where TOut : IDetachedDocumentResult =>
        Switch(
            context: (Borrow: borrow, Op: key),
            live: static (ctx, source) =>
                from session in ctx.Op.Need(source.Session)
                from result in session.Demand(
                    use: document =>
                        from settings in Optional(document.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                        from output in ctx.Borrow(settings)
                        select output,
                    key: ctx.Op,
                    needs: [SessionNeed.Read])
                select result,
            archived: static (ctx, source) =>
                from archive in ctx.Op.Need(source.Archive)
                from result in ctx.Op.Catch(() =>
                    from settings in Optional(archive.Settings.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                    from output in ctx.Borrow(settings)
                    select output)
                select result,
            detached: static (ctx, source) =>
                from settings in ctx.Op.Need(source.Settings)
                from result in ctx.Op.Catch(() => ctx.Borrow(settings.Resource))
                select result);

    internal Fin<Unit> Mutate(string name, Func<RenderSettings, Fin<Unit>> borrow, Op key) =>
        Switch(
            context: (Name: name, Borrow: borrow, Op: key),
            live: static (ctx, source) =>
                from session in ctx.Op.Need(source.Session)
                from changed in session.Demand(
                    use: document => DocumentCommit.Sealed(
                        document: document,
                        name: ctx.Name,
                        recordsUndo: true,
                        redraw: RedrawPolicy.None,
                        run: () =>
                            from settings in Optional(document.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                            from applied in ctx.Borrow(settings)
                            select applied,
                        project: Fin.Succ,
                        op: ctx.Op),
                    key: ctx.Op,
                    needs: SessionNeed.Mutation(undo: true, redraw: RedrawPolicy.None).ToArray())
                select changed,
            archived: static (ctx, source) =>
                from archive in ctx.Op.Need(source.Archive)
                from changed in ctx.Op.Catch(() =>
                    from settings in Optional(archive.Settings.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                    from applied in ctx.Borrow(settings)
                    select applied)
                select changed,
            detached: static (ctx, source) =>
                from settings in ctx.Op.Need(source.Settings)
                from changed in ctx.Op.Catch(() => ctx.Borrow(settings.Resource))
                select changed);

    public void Dispose() =>
        ignore(Switch(
            live: static _ => unit,
            archived: static _ => unit,
            detached: static source => source.Settings.Dispose()));
}
```

## [03]-[STATE_RECORDS]

- Owner: `SubOwners` is the one custody window over the seven `RenderSettings` sub-owners — `Within` owns the bracket, so every read and every apply the body asks for borrows the same seven wrappers, the state and its evidence sample one instant, and a per-property re-read cannot tear the snapshot.
- Owner: `GroundTrait`, `WorkflowStage`, `GuideTrait`, `GuideBandTrait`, and `RenderTrait` are the capability vocabularies whose rows carry the host read and the host seat as delegate columns; `GuideZone` is the two-row table over `SafeFrame`'s repeated band shape; each state record holds one `CapabilitySet` column where it held a bool run.
- Law: a switch is a ROW, not a field. Thirty-four host `bool` columns across seven records read and wrote as thirty-four named property pairs, so a swapped argument at a construction site inverted a meaning silently and a new host switch touched a record, its `Of`, and its `Apply`. Each vocabulary row now carries its own `Reads`/`Seats` pair, `Of` is one `Where` over `Items` and `Apply` one `Iter`, and a new switch is one row. NAMED LOSS: per-switch compile-time exhaustiveness at the record; bought back by the row's own delegate pair, which cannot be declared without both directions.
- Law: a borrow never releases the window — the bracket does. A read-then-write body is therefore ONE window, and a compensated edit reads its prior state and applies its plan against the same wrapper set instead of two unsynchronized opens. The release is the package's `Custody.Settled` both-arms bracket, so a wrapper-release refusal APPENDS to the body's fault; a `try`/`finally` around the same pair replaced the body's fault with the release's.
- Law: a sub-owner property answers a FRESH non-owning wrapper on every read and its `Dispose` is `GC.SuppressFinalize` alone, so custody is a finalizer retirement, never a native release — the seven-wrapper window is what makes the read coherent, and a free-floating sub-owner never enters it because disposing one suppresses its only delete path.
- Law: applies are total state, never a patch — every `Apply` re-asserts its full field set, so an absent field cannot silently clear and a configuration travels as one replayable value between documents, archives, and free-floating carriers.
- Law: `SafeFrame` publishes ONE band shape twice — an on switch, a link switch, and two scales for the action frame and again for the title frame — so `GuideBand` is that shape once and `GuideZone` carries which host quadruple a band reads and seats. Two records with eight properties between them re-asserted the same invariant twice and drifted on either half.
- Law: sun position follows host mode — automatic state writes geolocation, timezone, daylight saving, and moment before clearing manual control; manual state admits either the host angle pair or the vector setter after enabling manual control. `Sun.SetPosition` is the angle write, because it seats `Azimuth`, `Altitude`, and the derived `Vector` together where two property writes leave `Vector` stale between them.
- Law: daylight saving is ONE fact — the host publishes an on switch beside a minutes column and reads the minutes as zero when the switch is off, so `Option<int>` carries both and no consumer re-derives the pairing. A record holding the pair states twenty minutes of saving that no host read honours.
- Law: an identity column admits at construction — a ground plane's material instance and a channel's custom rows are `ResourceId`, so the empty guid the host answers for "no material" refuses here instead of travelling as a state a replay asserts.
- Law: `EnvironmentRole` and `EnvironmentView` close the usage-purpose product; `RenderConfig` writes one binding per role and `EnvironmentBindingState.Resolve` reads both purposes without leaking host enums.
- Law: a host-identity roster keys on its OWN host ordinal — `SunAccuracy`, `DitherMethod`, `EnvironmentRole`, and `EnvironmentView` are one-to-one renamings of a host enum, so the ordinal IS the key, `Native` derives from it, and the read is the kernel host-enum row arm. A string key beside a stored native column was two authorities for one value and the read it forced has no landed arity.
- Growth: a new host switch is one vocabulary row; a new sub-owner property is one record field read and asserted in the same pass; a new sub-owner is one state record, one `SettingsBody` case, and one `RenderState` column.
- Packages: `api-rhinocommon-rendersettings.md` (`GroundPlane`, `Skylight`, `Sun`, `Sun.Accuracies`, `Sun.SetPosition`, `Sun.SetDateTime`/`GetDateTime`, `Sun.Light`/`Vector`/`Hash`, `LinearWorkflow`, `Dithering`, `Dithering.Methods`, `SafeFrame`, `RenderChannels`, `RenderChannels.Modes`, `RenderSettings.EnvironmentUsage`/`EnvironmentPurpose`/`RenderingSources`, `RenderEnvironmentId`/`SetRenderEnvironmentId`/`RenderEnvironmentOverride`/`SetRenderEnvironmentOverride`, `BackgroundStyle`, `AntialiasLevel`); `api-rhinocommon-document.md` (`LengthUnit`); kernel `Domain/rails` (`Op.Row`, `Op.Catch`, `Op.Confirm`, `Op.Side`, `ValidityClaim`, `Lease<T>`), `Domain/validation` (`ICapability`, `CapabilitySet`), `Domain/context` (`ModelUnit`), `Numerics/atoms` (`PerceptualColor.OfHost`/`ToDrawing`, `Size2i`); `Document/tables.md` (`ResourceId`), kernel `Domain/rails` (`Custody.Settled`); LanguageExt.Core (`Fin`, `Seq`, `Option`); Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`, `[ComplexValueObject]`, `[ValueObject]`, `[UseDelegateFromConstructor]`).

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record SubOwners {
    private readonly Seq<IDisposable> held;

    private SubOwners(RenderSettings settings) {
        Settings = settings;
        Ground = settings.GroundPlane;
        Sky = settings.Skylight;
        Daylight = settings.Sun;
        Workflow = settings.LinearWorkflow;
        Dither = settings.Dithering;
        Guides = settings.SafeFrame;
        Channels = settings.RenderChannels;
        held = Seq<IDisposable>(Ground, Sky, Daylight, Workflow, Dither, Guides, Channels);
    }

    internal RenderSettings Settings { get; }
    internal GroundPlane Ground { get; }
    internal Skylight Sky { get; }
    internal global::Rhino.Render.Sun Daylight { get; }
    internal LinearWorkflow Workflow { get; }
    internal Dithering Dither { get; }
    internal SafeFrame Guides { get; }
    internal RenderChannels Channels { get; }

    internal static Fin<TOut> Within<TOut>(RenderSettings settings, Func<SubOwners, Fin<TOut>> borrow, Op key) =>
        from active in key.Need(settings)
        from activeBorrow in key.Need(borrow)
        from owners in key.Catch(() => Fin.Succ(value: new SubOwners(settings: active)))
        from result in key.Catch(() => activeBorrow(owners))
            .Settled(held: Seq(owners), release: window => window.Release(key), key: key)
        select result;

    private Fin<Unit> Release(Op key) => Custody.Release(
        held: held,
        release: owner => key.Catch(() => Fin.Succ(value: Op.Side(owner.Dispose))),
        key: key);
}

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GroundTrait : ICapability<GroundTrait> {
    public static readonly GroundTrait Enabled = new(key: "enabled",
        reads: static ground => ground.Enabled, seats: static (ground, held) => ground.Enabled = held);
    public static readonly GroundTrait ShadowOnly = new(key: "shadow-only",
        reads: static ground => ground.ShadowOnly, seats: static (ground, held) => ground.ShadowOnly = held);
    public static readonly GroundTrait Underside = new(key: "underside",
        reads: static ground => ground.ShowUnderside, seats: static (ground, held) => ground.ShowUnderside = held);
    public static readonly GroundTrait AutoAltitude = new(key: "auto-altitude",
        reads: static ground => ground.AutoAltitude, seats: static (ground, held) => ground.AutoAltitude = held);
    public static readonly GroundTrait OffsetLocked = new(key: "offset-locked",
        reads: static ground => ground.TextureOffsetLocked, seats: static (ground, held) => ground.TextureOffsetLocked = held);
    public static readonly GroundTrait SizeLocked = new(key: "size-locked",
        reads: static ground => ground.TextureSizeLocked, seats: static (ground, held) => ground.TextureSizeLocked = held);

    [UseDelegateFromConstructor]
    private partial bool Reads(GroundPlane ground);

    [UseDelegateFromConstructor]
    private partial void Seats(GroundPlane ground, bool held);

    internal static CapabilitySet<GroundTrait> Of(GroundPlane ground) =>
        CapabilitySet<GroundTrait>.Of(Items.Where(row => row.Reads(ground: ground)).ToArray());

    internal static Unit Apply(GroundPlane ground, CapabilitySet<GroundTrait> traits) =>
        toSeq(Items).Iter(row => row.Seats(ground: ground, held: traits.Admits(capability: row)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WorkflowStage : ICapability<WorkflowStage> {
    public static readonly WorkflowStage PreColors = new(key: "pre-colors",
        reads: static workflow => workflow.PreProcessColors, seats: static (workflow, held) => workflow.PreProcessColors = held);
    public static readonly WorkflowStage PreTextures = new(key: "pre-textures",
        reads: static workflow => workflow.PreProcessTextures, seats: static (workflow, held) => workflow.PreProcessTextures = held);
    public static readonly WorkflowStage PostFrameBuffer = new(key: "post-frame-buffer",
        reads: static workflow => workflow.PostProcessFrameBuffer, seats: static (workflow, held) => workflow.PostProcessFrameBuffer = held);

    [UseDelegateFromConstructor]
    private partial bool Reads(LinearWorkflow workflow);

    [UseDelegateFromConstructor]
    private partial void Seats(LinearWorkflow workflow, bool held);

    internal static CapabilitySet<WorkflowStage> Of(LinearWorkflow workflow) =>
        CapabilitySet<WorkflowStage>.Of(Items.Where(row => row.Reads(workflow: workflow)).ToArray());

    internal static Unit Apply(LinearWorkflow workflow, CapabilitySet<WorkflowStage> stages) =>
        toSeq(Items).Iter(row => row.Seats(workflow: workflow, held: stages.Admits(capability: row)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GuideTrait : ICapability<GuideTrait> {
    public static readonly GuideTrait Enabled = new(key: "enabled",
        reads: static frame => frame.Enabled, seats: static (frame, held) => frame.Enabled = held);
    public static readonly GuideTrait PerspectiveOnly = new(key: "perspective-only",
        reads: static frame => frame.PerspectiveOnly, seats: static (frame, held) => frame.PerspectiveOnly = held);
    public static readonly GuideTrait Fields = new(key: "fields",
        reads: static frame => frame.FieldsOn, seats: static (frame, held) => frame.FieldsOn = held);
    public static readonly GuideTrait LiveFrame = new(key: "live-frame",
        reads: static frame => frame.LiveFrameOn, seats: static (frame, held) => frame.LiveFrameOn = held);

    [UseDelegateFromConstructor]
    private partial bool Reads(SafeFrame frame);

    [UseDelegateFromConstructor]
    private partial void Seats(SafeFrame frame, bool held);

    internal static CapabilitySet<GuideTrait> Of(SafeFrame frame) =>
        CapabilitySet<GuideTrait>.Of(Items.Where(row => row.Reads(frame: frame)).ToArray());

    internal static Unit Apply(SafeFrame frame, CapabilitySet<GuideTrait> traits) =>
        toSeq(Items).Iter(row => row.Seats(frame: frame, held: traits.Admits(capability: row)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GuideBandTrait : ICapability<GuideBandTrait> {
    public static readonly GuideBandTrait Shown = new(key: "shown");
    public static readonly GuideBandTrait Linked = new(key: "linked");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RenderTrait : ICapability<RenderTrait> {
    public static readonly RenderTrait Transparent = new(key: "transparent",
        reads: static settings => settings.TransparentBackground, seats: static (settings, held) => settings.TransparentBackground = held);
    public static readonly RenderTrait Backfaces = new(key: "backfaces",
        reads: static settings => settings.RenderBackfaces, seats: static (settings, held) => settings.RenderBackfaces = held);
    public static readonly RenderTrait Curves = new(key: "curves",
        reads: static settings => settings.RenderCurves, seats: static (settings, held) => settings.RenderCurves = held);
    public static readonly RenderTrait Points = new(key: "points",
        reads: static settings => settings.RenderPoints, seats: static (settings, held) => settings.RenderPoints = held);
    public static readonly RenderTrait MeshEdges = new(key: "mesh-edges",
        reads: static settings => settings.RenderMeshEdges, seats: static (settings, held) => settings.RenderMeshEdges = held);
    public static readonly RenderTrait Annotations = new(key: "annotations",
        reads: static settings => settings.RenderAnnotations, seats: static (settings, held) => settings.RenderAnnotations = held);
    public static readonly RenderTrait Isoparams = new(key: "isoparams",
        reads: static settings => settings.RenderIsoparams, seats: static (settings, held) => settings.RenderIsoparams = held);
    public static readonly RenderTrait HiddenLights = new(key: "hidden-lights",
        reads: static settings => settings.UseHiddenLights, seats: static (settings, held) => settings.UseHiddenLights = held);
    public static readonly RenderTrait DepthCue = new(key: "depth-cue",
        reads: static settings => settings.DepthCue, seats: static (settings, held) => settings.DepthCue = held);
    public static readonly RenderTrait FlatShade = new(key: "flat-shade",
        reads: static settings => settings.FlatShade, seats: static (settings, held) => settings.FlatShade = held);

    [UseDelegateFromConstructor]
    private partial bool Reads(RenderSettings settings);

    [UseDelegateFromConstructor]
    private partial void Seats(RenderSettings settings, bool held);

    internal static CapabilitySet<RenderTrait> Of(RenderSettings settings) =>
        CapabilitySet<RenderTrait>.Of(Items.Where(row => row.Reads(settings: settings)).ToArray());

    internal static Unit Apply(RenderSettings settings, CapabilitySet<RenderTrait> traits) =>
        toSeq(Items).Iter(row => row.Seats(settings: settings, held: traits.Admits(capability: row)));
}

[SmartEnum<int>]
public sealed partial class GuideZone {
    public static readonly GuideZone Action = new(
        key: 0,
        reads: static frame => new GuideBand(
            Traits: Band(shown: frame.ActionFrameOn, linked: frame.ActionFrameLinked),
            XScale: frame.ActionFrameXScale,
            YScale: frame.ActionFrameYScale),
        seats: static (frame, band) => Op.Side(() => {
            frame.ActionFrameOn = band.Traits.Admits(capability: GuideBandTrait.Shown);
            frame.ActionFrameLinked = band.Traits.Admits(capability: GuideBandTrait.Linked);
            frame.ActionFrameXScale = band.XScale;
            frame.ActionFrameYScale = band.YScale;
        }));
    public static readonly GuideZone Title = new(
        key: 1,
        reads: static frame => new GuideBand(
            Traits: Band(shown: frame.TitleFrameOn, linked: frame.TitleFrameLinked),
            XScale: frame.TitleFrameXScale,
            YScale: frame.TitleFrameYScale),
        seats: static (frame, band) => Op.Side(() => {
            frame.TitleFrameOn = band.Traits.Admits(capability: GuideBandTrait.Shown);
            frame.TitleFrameLinked = band.Traits.Admits(capability: GuideBandTrait.Linked);
            frame.TitleFrameXScale = band.XScale;
            frame.TitleFrameYScale = band.YScale;
        }));

    [UseDelegateFromConstructor]
    internal partial GuideBand Reads(SafeFrame frame);

    [UseDelegateFromConstructor]
    internal partial Unit Seats(SafeFrame frame, GuideBand band);

    private static CapabilitySet<GuideBandTrait> Band(bool shown, bool linked) =>
        CapabilitySet<GuideBandTrait>.Of(
            Seq((Trait: GuideBandTrait.Shown, Held: shown), (Trait: GuideBandTrait.Linked, Held: linked))
                .Filter(static row => row.Held)
                .Map(static row => row.Trait)
                .ToArray());
}

[SmartEnum<int>]
public sealed partial class SunAccuracy {
    public static readonly SunAccuracy Minimum = new(key: (int)global::Rhino.Render.Sun.Accuracies.Minimum);
    public static readonly SunAccuracy Maximum = new(key: (int)global::Rhino.Render.Sun.Accuracies.Maximum);

    internal global::Rhino.Render.Sun.Accuracies Native => (global::Rhino.Render.Sun.Accuracies)Key;

    internal static Fin<SunAccuracy> Of(global::Rhino.Render.Sun.Accuracies native, Op key) =>
        key.Row<global::Rhino.Render.Sun.Accuracies, SunAccuracy>(native, static value => (int)value);
}

[SmartEnum<int>]
public sealed partial class DitherMethod {
    public static readonly DitherMethod None = new(key: (int)Dithering.Methods.None);
    public static readonly DitherMethod FloydSteinberg = new(key: (int)Dithering.Methods.FloydSteinberg);
    public static readonly DitherMethod SimpleNoise = new(key: (int)Dithering.Methods.SimpleNoise);

    internal Dithering.Methods Native => (Dithering.Methods)Key;

    internal static Fin<DitherMethod> Of(Dithering.Methods native, Op key) =>
        key.Row<Dithering.Methods, DitherMethod>(native, static value => (int)value);
}

[SmartEnum<int>]
public sealed partial class EnvironmentRole {
    public static readonly EnvironmentRole Background = new(key: (int)RenderSettings.EnvironmentUsage.Background);
    public static readonly EnvironmentRole Reflection = new(key: (int)RenderSettings.EnvironmentUsage.Reflection);
    public static readonly EnvironmentRole Skylighting = new(key: (int)RenderSettings.EnvironmentUsage.Skylighting);

    internal RenderSettings.EnvironmentUsage Native => (RenderSettings.EnvironmentUsage)Key;

    internal static Fin<EnvironmentRole> Of(RenderSettings.EnvironmentUsage native, Op key) =>
        key.Row<RenderSettings.EnvironmentUsage, EnvironmentRole>(native, static value => (int)value);
}

[SmartEnum<int>]
public sealed partial class EnvironmentView {
    public static readonly EnvironmentView Standard = new(key: (int)RenderSettings.EnvironmentPurpose.Standard);
    public static readonly EnvironmentView Rendering = new(key: (int)RenderSettings.EnvironmentPurpose.ForRendering);

    internal RenderSettings.EnvironmentPurpose Native => (RenderSettings.EnvironmentPurpose)Key;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunPlacement {
    private SunPlacement() { }
    public sealed record Automatic(
        double Latitude, double Longitude, double TimeZone,
        Option<int> DaylightSaving, DateTime Moment) : SunPlacement;
    public sealed record ManualAngles(double Azimuth, double Altitude) : SunPlacement;
    public sealed record ManualVector(Vector3d Value) : SunPlacement;

    internal ValidityClaim IsValid => Switch(
        automatic: static placement => ValidityClaim.All(
            Coordinate(latitude: placement.Latitude, longitude: placement.Longitude),
            ValidityClaim.Ordered(lower: -24d, upper: placement.TimeZone),
            ValidityClaim.Ordered(lower: placement.TimeZone, upper: 24d),
            ValidityClaim.WhenPresent(placement.DaylightSaving, static minutes => (ValidityClaim)(minutes is >= 0 and <= 1440))),
        manualAngles: static placement => ValidityClaim.All(
            ValidityClaim.Finite(value: placement.Azimuth),
            ValidityClaim.Ordered(lower: -90d, upper: placement.Altitude),
            ValidityClaim.Ordered(lower: placement.Altitude, upper: 90d)),
        manualVector: static placement => ValidityClaim.Direction(value: placement.Value));

    private static ValidityClaim Coordinate(double latitude, double longitude) => ValidityClaim.All(
        ValidityClaim.Ordered(lower: -90d, upper: latitude),
        ValidityClaim.Ordered(lower: latitude, upper: 90d),
        ValidityClaim.Ordered(lower: -180d, upper: longitude),
        ValidityClaim.Ordered(lower: longitude, upper: 180d));
}

[SmartEnum<bool>]
public sealed partial class GammaMode {
    public static readonly GammaMode Off = new(false);
    public static readonly GammaMode On = new(true);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct GuideBand(CapabilitySet<GuideBandTrait> Traits, double XScale, double YScale) {
    internal ValidityClaim IsValid => ValidityClaim.All(
        ValidityClaim.Nonnegative(value: XScale),
        ValidityClaim.Nonnegative(value: YScale));
}

public sealed record GroundPlaneState(
    CapabilitySet<GroundTrait> Traits,
    double Altitude,
    Option<ResourceId> Material,
    Vector2d TextureOffset,
    Vector2d TextureSize,
    double TextureRotation) : IDetachedDocumentResult {
    internal ValidityClaim IsValid => ValidityClaim.All(
        ValidityClaim.Finite(value: Altitude),
        TextureOffset.IsValid,
        TextureSize.IsValid,
        ValidityClaim.Finite(value: TextureRotation));

    internal static Fin<GroundPlaneState> Of(GroundPlane ground, Op key) =>
        from material in Optional(ground.MaterialInstanceId)
            .Filter(static id => id != Guid.Empty)
            .Traverse(id => ResourceId.Admit(value: id, key: key))
            .As()
        select new GroundPlaneState(
            Traits: GroundTrait.Of(ground: ground),
            Altitude: ground.Altitude,
            Material: material,
            TextureOffset: ground.TextureOffset,
            TextureSize: ground.TextureSize,
            TextureRotation: ground.TextureRotation);

    internal Fin<Unit> Apply(GroundPlane ground, Op key) {
        GroundPlaneState self = this;
        return from _ in guard(self.IsValid, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => Fin.Succ(value: Op.Side(() => {
                   ignore(GroundTrait.Apply(ground: ground, traits: self.Traits));
                   ground.Altitude = self.Altitude;
                   ground.MaterialInstanceId = self.Material.Map(static id => id.ToValue()).IfNone(Guid.Empty);
                   ground.TextureOffset = self.TextureOffset;
                   ground.TextureSize = self.TextureSize;
                   ground.TextureRotation = self.TextureRotation;
               })))
               select applied;
    }
}

public readonly record struct SkylightState(bool Enabled, double ShadowIntensity) : IDetachedDocumentResult {
    internal static SkylightState Of(Skylight sky) => new(Enabled: sky.Enabled, ShadowIntensity: sky.ShadowIntensity);

    internal Fin<Unit> Apply(Skylight sky, Op key) {
        SkylightState self = this;
        return from _ in guard(ValidityClaim.Nonnegative(value: self.ShadowIntensity), key.InvalidInput()).ToFin()
               from applied in key.Catch(() => Fin.Succ(value: Op.Side(() => {
                   sky.Enabled = self.Enabled;
                   sky.ShadowIntensity = self.ShadowIntensity;
               })))
               select applied;
    }
}

public sealed record SunEvidence(Vector3d Vector, uint Hash, Lease<Light> Light)
    : IDisposable, IDetachedDocumentResult {
    internal static Fin<SunEvidence> Of(global::Rhino.Render.Sun sun, Op key) => key.Catch(() =>
        Optional(sun.Light).ToFin(Fail: key.InvalidResult()).Map(light => new SunEvidence(
            Vector: sun.Vector,
            Hash: sun.Hash,
            Light: new Lease<Light>.Owned(Value: light))));

    public void Dispose() => ignore(Light.Dispose());
}

[ComplexValueObject]
public sealed partial class SunState : IDetachedDocumentResult {
    public bool Enabled { get; }
    public double Intensity { get; }
    public SunAccuracy Accuracy { get; }
    public double North { get; }
    public SunPlacement Placement { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref bool enabled,
        ref double intensity,
        ref SunAccuracy accuracy,
        ref double north,
        ref SunPlacement placement) {
        validationError = ValidityClaim.All(
            ValidityClaim.Nonnegative(value: intensity),
            accuracy is not null,
            ValidityClaim.Finite(value: north),
            placement is not null && placement.IsValid)
            ? validationError
            : new ValidationError(message: "sun state is invalid");
    }

    internal static Fin<SunState> Of(global::Rhino.Render.Sun sun, Op key) =>
        from accuracy in SunAccuracy.Of(sun.Accuracy, key)
        let placement = sun.ManualControlOn
                ? (SunPlacement)new SunPlacement.ManualAngles(Azimuth: sun.Azimuth, Altitude: sun.Altitude)
                : new SunPlacement.Automatic(
                    Latitude: sun.Latitude, Longitude: sun.Longitude, TimeZone: sun.TimeZone,
                    DaylightSaving: sun.DaylightSavingOn ? Some(sun.DaylightSavingMinutes) : None,
                    Moment: sun.GetDateTime(DateTimeKind.Local))
        from state in key.AcceptValidated(
            Validate(sun.Enabled, sun.Intensity, accuracy, sun.North, placement, out SunState? value), value)
        select state;

    internal Fin<Unit> Apply(global::Rhino.Render.Sun sun, Op key) {
        SunState self = this;
        return key.Catch(() => Fin.Succ(value: Op.Side(() => {
            sun.Enabled = self.Enabled;
            sun.Intensity = self.Intensity;
            sun.Accuracy = self.Accuracy.Native;
            sun.North = self.North;
            ignore(self.Placement.Switch(
                automatic: placement => Op.Side(() => {
                    sun.Latitude = placement.Latitude;
                    sun.Longitude = placement.Longitude;
                    sun.TimeZone = placement.TimeZone;
                    sun.DaylightSavingOn = placement.DaylightSaving.IsSome;
                    sun.DaylightSavingMinutes = placement.DaylightSaving.IfNone(0);
                    sun.SetDateTime(placement.Moment, placement.Moment.Kind);
                    sun.ManualControlOn = false;
                }),
                manualAngles: placement => Op.Side(() => {
                    sun.ManualControlOn = true;
                    sun.SetPosition(azimuthDegrees: placement.Azimuth, altitudeDegrees: placement.Altitude);
                }),
                manualVector: placement => Op.Side(() => {
                    sun.ManualControlOn = true;
                    sun.Vector = placement.Value;
                })));
        })));
    }
}

[ComplexValueObject]
public sealed partial class PostGamma {
    public GammaMode Mode { get; }
    public float Gamma { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref GammaMode mode,
        ref float gamma) {
        validationError = mode is not null && ValidityClaim.Positive(value: gamma)
            ? validationError
            : new ValidationError(message: "post-process gamma is invalid");
    }

    internal static Fin<PostGamma> Of(LinearWorkflow workflow, Op key) =>
        key.AcceptValidated(
            Validate(workflow.PostProcessGammaOn ? GammaMode.On : GammaMode.Off, workflow.PostProcessGamma,
                out PostGamma? value),
            value);

    internal Unit Apply(LinearWorkflow workflow) => Op.Side(() => {
        workflow.PostProcessGamma = Gamma;
        workflow.PostProcessGammaOn = Mode.Key;
    });
}

public readonly record struct WorkflowState(
    CapabilitySet<WorkflowStage> Stages, float PreProcessGamma, PostGamma PostGamma) : IDetachedDocumentResult {
    internal static Fin<WorkflowState> Of(LinearWorkflow workflow, Op key) =>
        PostGamma.Of(workflow, key).Map(postGamma => new WorkflowState(
            Stages: WorkflowStage.Of(workflow: workflow),
            PreProcessGamma: workflow.PreProcessGamma,
            PostGamma: postGamma));

    internal Fin<Unit> Apply(LinearWorkflow workflow, Op key) {
        WorkflowState self = this;
        return from _ in guard(
                   ValidityClaim.All(ValidityClaim.Positive(value: self.PreProcessGamma), self.PostGamma is not null),
                   key.InvalidInput()).ToFin()
               from applied in key.Catch(() => Fin.Succ(value: Op.Side(() => {
                   ignore(WorkflowStage.Apply(workflow: workflow, stages: self.Stages));
                   workflow.PreProcessGamma = self.PreProcessGamma;
                   ignore(self.PostGamma.Apply(workflow));
               })))
               select applied;
    }
}

public readonly record struct WorkflowEvidence(float PostGammaReciprocal, uint Hash) : IDetachedDocumentResult {
    internal static WorkflowEvidence Of(LinearWorkflow workflow) =>
        new(PostGammaReciprocal: workflow.PostProcessGammaReciprocal, Hash: workflow.Hash);
}

public readonly record struct DitherState(DitherMethod Method, bool Enabled) : IDetachedDocumentResult {
    internal static Fin<DitherState> Of(Dithering dither, Op key) =>
        DitherMethod.Of(dither.Method, key).Map(method => new DitherState(Method: method, Enabled: dither.Enabled));

    internal Fin<Unit> Apply(Dithering dither, Op key) {
        DitherState self = this;
        return from _ in guard(self.Method is not null, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => Fin.Succ(value: Op.Side(() => {
                   dither.Method = self.Method.Native;
                   dither.Enabled = self.Enabled;
               })))
               select applied;
    }
}

public sealed record SafeFrameState(
    CapabilitySet<GuideTrait> Traits, GuideBand Action, GuideBand Title) : IDetachedDocumentResult {
    internal ValidityClaim IsValid => ValidityClaim.All(Action.IsValid, Title.IsValid);

    internal static SafeFrameState Of(SafeFrame frame) => new(
        Traits: GuideTrait.Of(frame: frame),
        Action: GuideZone.Action.Reads(frame: frame),
        Title: GuideZone.Title.Reads(frame: frame));

    internal Fin<Unit> Apply(SafeFrame frame, Op key) {
        SafeFrameState self = this;
        return from _ in guard(self.IsValid, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => Fin.Succ(value: Op.Side(() => {
                   ignore(GuideTrait.Apply(frame: frame, traits: self.Traits));
                   ignore(GuideZone.Action.Seats(frame: frame, band: self.Action));
                   ignore(GuideZone.Title.Seats(frame: frame, band: self.Title));
               })))
               select applied;
    }
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelState : IDetachedDocumentResult {
    private ChannelState() { }
    public sealed record Automatic : ChannelState;
    public sealed record Custom(Seq<ResourceId> Values) : ChannelState;

    private ValidityClaim IsValid => Switch(
        automatic: static _ => ValidityClaim.All(),
        custom: static value => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: value.Values.Count, floor: 1),
            ValidityClaim.CountExactly(count: value.Values.Distinct().Count, expected: value.Values.Count)));

    internal static Fin<ChannelState> Of(RenderChannels channels, Op key) => channels.Mode switch {
        RenderChannels.Modes.Automatic => Fin.Succ<ChannelState>(new Automatic()),
        RenderChannels.Modes.Custom =>
            from admitted in toSeq(channels.CustomList)
                .Traverse(id => ResourceId.Admit(value: id, key: key).ToValidation())
                .As()
                .ToFin()
            from state in Fin.Succ<ChannelState>(new Custom(Values: admitted))
            from _ in guard(state.IsValid, key.InvalidResult()).ToFin()
            select state,
        _ => Fin.Fail<ChannelState>(key.InvalidResult()),
    };

    internal Fin<Unit> Apply(RenderChannels channels, Op key) {
        ChannelState self = this;
        return from _ in guard(self.IsValid, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => self.Switch(
            context: channels,
            automatic: static (state, _) => Fin.Succ(value: Op.Side(() => {
                state.CustomList = [];
                state.Mode = RenderChannels.Modes.Automatic;
            })),
            custom: static (state, value) => Fin.Succ(value: Op.Side(() => {
                state.CustomList = value.Values.Distinct().Map(static id => id.ToValue()).ToArray();
                state.Mode = RenderChannels.Modes.Custom;
            }))))
               select applied;
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderSource {
    private RenderSource() { }
    public sealed record ActiveViewport : RenderSource;
    public sealed record SpecificViewport(string Name) : RenderSource;
    public sealed record NamedView(string Name) : RenderSource;
    public sealed record Snapshot(string Name) : RenderSource;

    internal ValidityClaim IsValid => Switch(
        activeViewport: static _ => ValidityClaim.All(),
        specificViewport: static source => !string.IsNullOrWhiteSpace(source.Name),
        namedView: static source => !string.IsNullOrWhiteSpace(source.Name),
        snapshot: static source => !string.IsNullOrWhiteSpace(source.Name));

    internal static Fin<RenderSource> Of(RenderSettings settings, Op key) => settings.RenderSource switch {
        RenderSettings.RenderingSources.ActiveViewport => Fin.Succ<RenderSource>(new ActiveViewport()),
        RenderSettings.RenderingSources.SpecificViewport => Named(
            settings.SpecificViewport, key, static name => new SpecificViewport(name)),
        RenderSettings.RenderingSources.NamedView => Named(settings.NamedView, key, static name => new NamedView(name)),
        RenderSettings.RenderingSources.SnapShot => Named(settings.Snapshot, key, static name => new Snapshot(name)),
        _ => Fin.Fail<RenderSource>(key.InvalidResult()),
    };

    internal Unit Apply(RenderSettings settings) => Switch(
        context: settings,
        activeViewport: static (state, _) => Op.Side(() => state.RenderSource = RenderSettings.RenderingSources.ActiveViewport),
        specificViewport: static (state, source) => Op.Side(() => {
            state.SpecificViewport = source.Name;
            state.RenderSource = RenderSettings.RenderingSources.SpecificViewport;
        }),
        namedView: static (state, source) => Op.Side(() => {
            state.NamedView = source.Name;
            state.RenderSource = RenderSettings.RenderingSources.NamedView;
        }),
        snapshot: static (state, source) => Op.Side(() => {
            state.Snapshot = source.Name;
            state.RenderSource = RenderSettings.RenderingSources.SnapShot;
        }));

    private static Fin<RenderSource> Named<T>(string value, Op key, Func<string, T> project) where T : RenderSource =>
        key.AcceptText(value: value).Map(text => (RenderSource)project(text));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderOutput {
    private RenderOutput(bool scaleBackgroundToFit) => ScaleBackgroundToFit = scaleBackgroundToFit;

    internal bool ScaleBackgroundToFit { get; }

    private sealed record ViewportCase(bool Scaled) : RenderOutput(Scaled);
    private sealed record FixedCase(Size2i Size, double Dpi, ModelUnit Units, bool Scaled) : RenderOutput(Scaled);

    public static RenderOutput Viewport(bool scaleBackgroundToFit) => new ViewportCase(Scaled: scaleBackgroundToFit);

    public static Fin<RenderOutput> Fixed(
        Size2i size, double dpi, ModelUnit units, bool scaleBackgroundToFit, Op? key = null) {
        Op op = key.OrDefault();
        return from admittedSize in Size2i.Of(width: size.Width, height: size.Height, key: op)
               from admittedDpi in op.Positive(value: dpi)
               from admittedUnits in op.Need(units)
               select (RenderOutput)new FixedCase(
                   Size: admittedSize, Dpi: admittedDpi, Units: admittedUnits, Scaled: scaleBackgroundToFit);
    }

    internal Fin<RenderOutput> Admit(Op key) => Switch(
        context: key,
        viewportCase: static (_, output) => Fin.Succ<RenderOutput>(output),
        fixedCase: static (op, output) => Fixed(
            size: output.Size, dpi: output.Dpi, units: output.Units,
            scaleBackgroundToFit: output.ScaleBackgroundToFit, key: op));

    internal static Fin<RenderOutput> Of(RenderSettings settings, Op key) => settings.UseViewportSize
        ? Fin.Succ(Viewport(scaleBackgroundToFit: settings.ScaleBackgroundToFit))
        : from size in Size2i.Of(width: settings.ImageSize.Width, height: settings.ImageSize.Height, key: key)
          from units in ModelUnit.Of(value: settings.ImageUnitSystem, key: key)
          from output in Fixed(
              size: size, dpi: settings.ImageDpi, units: units,
              scaleBackgroundToFit: settings.ScaleBackgroundToFit, key: key)
          select output;

    internal Unit Apply(RenderSettings settings) => Switch(
        context: settings,
        viewportCase: static (state, output) => Op.Side(() => {
            state.UseViewportSize = true;
            state.ScaleBackgroundToFit = output.ScaleBackgroundToFit;
        }),
        fixedCase: static (state, output) => Op.Side(() => {
            state.UseViewportSize = false;
            state.ImageSize = output.Size.Native;
            state.ImageDpi = output.Dpi;
            state.ImageUnitSystem = output.Units.System;
            state.ScaleBackgroundToFit = output.ScaleBackgroundToFit;
        }));
}

[ValueObject<int>]
public readonly partial struct BackgroundMode {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        validationError = Enum.IsDefined((BackgroundStyle)value)
            ? validationError
            : new ValidationError("background mode is undefined");
    }

    internal BackgroundStyle Native => (BackgroundStyle)Value;

    internal static Fin<BackgroundMode> Of(BackgroundStyle value, Op key) => key.AcceptValidated<BackgroundMode>((int)value);
}

[ValueObject<int>]
public readonly partial struct AntialiasPolicy {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        validationError = Enum.IsDefined((AntialiasLevel)value)
            ? validationError
            : new ValidationError("antialias policy is undefined");
    }

    internal AntialiasLevel Native => (AntialiasLevel)Value;

    internal static Fin<AntialiasPolicy> Of(AntialiasLevel value, Op key) => key.AcceptValidated<AntialiasPolicy>((int)value);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct EnvironmentBinding(Option<ResourceId> Content, bool Override);

public sealed record EnvironmentBindingState {
    private readonly Seq<(EnvironmentRole Role, EnvironmentBinding Binding)> rows;

    private EnvironmentBindingState(Seq<(EnvironmentRole Role, EnvironmentBinding Binding)> rows) => this.rows = rows;

    public Seq<(EnvironmentRole Role, EnvironmentBinding Binding)> Rows => rows;

    public static Fin<EnvironmentBindingState> Of(
        Op? key = null, params ReadOnlySpan<(EnvironmentRole Role, EnvironmentBinding Binding)> rows) {
        Op op = key.OrDefault();
        Seq<(EnvironmentRole Role, EnvironmentBinding Binding)> admitted = toSeq(rows.ToArray());
        return guard(
                ValidityClaim.All(
                    ValidityClaim.CountExactly(count: admitted.Count, expected: EnvironmentRole.Items.Count),
                    toSeq(EnvironmentRole.Items).ForAll(role => admitted.Count(row => row.Role == role) == 1)),
                op.InvalidInput())
            .ToFin()
            .Map(_ => new EnvironmentBindingState(rows: admitted));
    }

    internal static Fin<EnvironmentBindingState> Of(RenderSettings settings, Op key) =>
        toSeq(EnvironmentRole.Items)
            .Traverse(role =>
                (from content in Optional(settings.RenderEnvironmentId(role.Native, EnvironmentView.Standard.Native))
                    .Filter(static id => id != Guid.Empty)
                    .Traverse(id => ResourceId.Admit(value: id, key: key))
                    .As()
                 select (Role: role, Binding: new EnvironmentBinding(
                     Content: content, Override: settings.RenderEnvironmentOverride(role.Native)))).ToValidation())
            .As()
            .ToFin()
            .Map(static admitted => new EnvironmentBindingState(rows: admitted));

    internal static Fin<Seq<(EnvironmentRole Role, EnvironmentView View, Option<Guid> Content)>> Resolve(
        RenderSettings settings, Op key) =>
        key.Catch(() => Fin.Succ(toSeq(EnvironmentRole.Items).Bind(role => toSeq(EnvironmentView.Items).Map(view => (
            Role: role,
            View: view,
            Content: Optional(settings.RenderEnvironmentId(role.Native, view.Native))
                .Filter(static id => id != Guid.Empty))))));

    internal Fin<Unit> Apply(RenderSettings settings, Op key) {
        EnvironmentBindingState self = this;
        return key.Catch(() => Fin.Succ(value: self.rows.Iter(row => {
            settings.SetRenderEnvironmentId(row.Role.Native, row.Binding.Content.Map(static id => id.ToValue()).IfNone(Guid.Empty));
            settings.SetRenderEnvironmentOverride(row.Role.Native, row.Binding.Override);
        })));
    }
}

public sealed record RenderConfig(
    PerceptualColor Ambient,
    PerceptualColor BackgroundTop,
    PerceptualColor BackgroundBottom,
    BackgroundMode BackgroundStyle,
    AntialiasPolicy Antialias,
    int ShadowmapLevel,
    CapabilitySet<RenderTrait> Traits,
    RenderOutput Output,
    RenderSource Source,
    EnvironmentBindingState Environments) : IDetachedDocumentResult {

    internal static Fin<RenderConfig> Of(RenderSettings settings, Op key) =>
        key.Catch(() =>
            from ambient in PerceptualColor.OfHost(host: settings.AmbientLight, key: key)
            from top in PerceptualColor.OfHost(host: settings.BackgroundColorTop, key: key)
            from bottom in PerceptualColor.OfHost(host: settings.BackgroundColorBottom, key: key)
            from background in BackgroundMode.Of(settings.BackgroundStyle, key)
            from antialias in AntialiasPolicy.Of(settings.AntialiasLevel, key)
            from output in RenderOutput.Of(settings, key)
            from source in RenderSource.Of(settings, key)
            from environments in EnvironmentBindingState.Of(settings: settings, key: key)
            select new RenderConfig(
                Ambient: ambient, BackgroundTop: top, BackgroundBottom: bottom,
                BackgroundStyle: background, Antialias: antialias, ShadowmapLevel: settings.ShadowmapLevel,
                Traits: RenderTrait.Of(settings: settings),
                Output: output, Source: source, Environments: environments));

    internal Fin<Unit> Apply(RenderSettings settings, Op key) {
        RenderConfig self = this;
        return from output in key.Need(self.Output).Bind(value => value.Admit(key))
               from _ in guard(
                   ValidityClaim.All(
                       self.Source is { IsValid.Holds: true },
                       self.Environments is not null,
                       ValidityClaim.CountAtLeast(count: self.ShadowmapLevel, floor: 0)),
                   key.InvalidInput()).ToFin()
               from ambient in self.Ambient.ToDrawing(key: key)
               from top in self.BackgroundTop.ToDrawing(key: key)
               from bottom in self.BackgroundBottom.ToDrawing(key: key)
               from applied in key.Catch(() => {
                   settings.AmbientLight = ambient;
                   settings.BackgroundColorTop = top;
                   settings.BackgroundColorBottom = bottom;
                   settings.BackgroundStyle = self.BackgroundStyle.Native;
                   settings.AntialiasLevel = self.Antialias.Native;
                   settings.ShadowmapLevel = self.ShadowmapLevel;
                   ignore(RenderTrait.Apply(settings: settings, traits: self.Traits));
                   ignore(output.Apply(settings));
                   ignore(self.Source.Apply(settings));
                   return self.Environments.Apply(settings: settings, key: key);
               })
               select applied;
    }
}
```

## [04]-[SUN_ASTRONOMY]

- Owner: `SunProblem` closes the HOST astronomy questions — direction, altitude, Julian day, twilight, tint, and machine location; `SunCapability` is the grant a machine-facts read presents; `SunSolution` closes vector, scalar, colour, and optional-location egress; `SunSolver.Solve` is the sole entry. `SolarFrame`, `SunDerivation`, and `SceneSun` are the daylighting descriptor's sun band, projected out of `SunState` and never read back in.
- Law: each host problem dispatches directly to its verified host static and provider failure or invalid admission stays on the `Fin<SunSolution>` rail.
- Law: every problem but `Here` is pure over its supplied arguments — `Here` reads the machine's own geolocation service, so it carries the `SunCapability.MachineLocation` grant and admission refuses the case without it; a machine-facts read reached implicitly through a coordinate solve is the deleted form.
- Law: host astronomy and wire astronomy answer two questions and stay two-formed — `Sun.SunDirection`/`AltitudeFromValues` report what the HOST believes and drive host-facing reads, while the descriptor's angles are the kernel `SolarPosition.At` almanac a peer reproduces. The SOLVE is the kernel's, so `SceneSun.Derive` composes it in ONE hop; routing it back through a host-problem case wrapped a total effect-free fold in a request union, a solution union, and a partial projection off both.
- Law: `SolarFrame` narrows the georeference to what an annual engine run admits — time zone `[-12, 14]` hours and elevation `[-300, 8900)` metres — so a document outside those bounds refuses at the producer instead of writing a site an engine rejects, and `SolarSite`'s own wider gate stays the kernel's.
- Law: `Sited` and `Authored` are the whole discriminant — a manually controlled sun has no site derivation, so it carries angles alone and an annual run refuses it by name rather than back-solving coordinates from two numbers.
- Law: `Sun.Vector` points sun-toward-scene in the document world frame and `Sun.North` bears compass north counter-clockwise off `+X` — `90`, the host default, seating north on `+Y` — so `ManualVector` negates, unitizes, projects onto that bearing's east and north axes in the host `Vector3d` the almanac takes, and re-reads through the kernel `SunPosition.OfDirection`; `Authored` therefore carries the same east-of-North pair `Sited` does, the host frame stops at this projection, and a ray that cannot unitize refuses instead of crossing as the due-south horizon reading the host substitutes for it.
- Boundary: the georeference invariant — `Sun.North`/`Latitude`/`Longitude` re-encoded from `EarthAnchorPoint` after an anchor write — is the Exchange rail's earth-sync owner; this page never writes the anchor, `Here` only reads the machine, and `elevationMetres` arrives as the caller's `EarthAnchorPoint.EarthBasepointElevation` read.
- Boundary: sky irradiance is the consuming weather owner's — `SunState.Intensity` is a dimensionless render multiplier, so this band carries no `W/m2` column and a manufactured one fabricates radiation the document never held.
- Packages: `api-rhinocommon-rendersettings.md` (`Sun.SunDirection`, `Sun.AltitudeFromValues`, `Sun.JulianDay`, `Sun.TwilightZone`, `Sun.ColorFromAltitude`, `Sun.Here`); kernel `Numerics/calculus` (`SolarSite`, `SolarPosition.At`, `SunPosition`, `SunPosition.OfDirection`), `Numerics/atoms` (`PerceptualColor.OfHost`), `Domain/rails` (`Op.Catch`, `Op.AcceptValidated`, `ValidityClaim`); NodaTime (`Instant`, `Instant.FromDateTimeUtc`); LanguageExt.Core (`Fin`, `Option`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[ComplexValueObject]`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunProblem {
    private SunProblem() { }
    public sealed record Direction(double Latitude, double Longitude, DateTime Moment) : SunProblem;
    public sealed record Altitude(
        double Latitude, double Longitude, double TimeZoneHours,
        int DaylightMinutes, DateTime Moment, double Hours, SolarSolveMode Mode) : SunProblem;
    public sealed record Julian(double TimeZoneHours, int DaylightMinutes, DateTime Moment, double Hours) : SunProblem;
    public sealed record Twilight : SunProblem;
    public sealed record Color(double AltitudeDegrees) : SunProblem;
    public sealed record Here(SunCapability Grant) : SunProblem;

    internal ValidityClaim IsValid => Switch(
        direction: static problem => Coordinate(problem.Latitude, problem.Longitude),
        altitude: static problem => ValidityClaim.All(
            Coordinate(problem.Latitude, problem.Longitude),
            Time(problem.TimeZoneHours, problem.DaylightMinutes, problem.Hours),
            problem.Mode is not null),
        julian: static problem => Time(problem.TimeZoneHours, problem.DaylightMinutes, problem.Hours),
        twilight: static _ => ValidityClaim.All(),
        color: static problem => ValidityClaim.Finite(value: problem.AltitudeDegrees),
        here: static problem => problem.Grant == SunCapability.MachineLocation);

    private static ValidityClaim Coordinate(double latitude, double longitude) => ValidityClaim.All(
        ValidityClaim.Ordered(lower: -90d, upper: latitude),
        ValidityClaim.Ordered(lower: latitude, upper: 90d),
        ValidityClaim.Ordered(lower: -180d, upper: longitude),
        ValidityClaim.Ordered(lower: longitude, upper: 180d));

    private static ValidityClaim Time(double zone, int daylight, double hours) => ValidityClaim.All(
        ValidityClaim.Ordered(lower: -24d, upper: zone),
        ValidityClaim.Ordered(lower: zone, upper: 24d),
        daylight is >= 0 and <= 1440,
        ValidityClaim.Finite(value: hours));
}

[SmartEnum<string>]
public sealed partial class SunCapability {
    public static readonly SunCapability MachineLocation = new("machine-location");
}

[SmartEnum<bool>]
public sealed partial class SolarSolveMode {
    public static readonly SolarSolveMode Precise = new(false);
    public static readonly SolarSolveMode Fast = new(true);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunSolution : IDetachedDocumentResult {
    private SunSolution() { }
    public sealed record Vector(Vector3d Value) : SunSolution;
    public sealed record Scalar(double Value) : SunSolution;
    public sealed record Color(PerceptualColor Value) : SunSolution;
    public sealed record Location(Option<(double Latitude, double Longitude)> Value) : SunSolution;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunDerivation {
    private SunDerivation() { }
    public sealed record Sited(SolarFrame Frame, SunPosition Angles) : SunDerivation;
    public sealed record Authored(SunPosition Angles) : SunDerivation;
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SolarFrame {
    public SolarSite Site { get; }
    public double NorthAxisDegrees { get; }
    public int DaylightSavingMinutes { get; }
    public Instant Moment { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SolarSite site,
        ref double northAxisDegrees,
        ref int daylightSavingMinutes,
        ref Instant moment) {
        validationError = ValidityClaim.All(
            site is not null,
            site is not null && ValidityClaim.Ordered(lower: -12d, upper: site.TimezoneHours),
            site is not null && ValidityClaim.Ordered(lower: site.TimezoneHours, upper: 14d),
            site is not null && ValidityClaim.Ordered(lower: -300d, upper: site.ElevationM),
            site is not null && site.ElevationM < 8900d,
            ValidityClaim.Finite(value: northAxisDegrees),
            daylightSavingMinutes is >= 0 and <= 1440)
            ? validationError
            : new ValidationError(message: "<solar-frame-outside-engine-bounds>");
    }
}

public sealed record SceneSun(SunDerivation Derivation, bool Enabled, double IntensityScale)
    : IDetachedDocumentResult {
    public static Fin<SceneSun> Of(SunState state, double elevationMetres, Op? key = null) {
        Op op = key.OrDefault();
        return from active in op.Need(state)
               from derivation in Derive(state: active, elevationMetres: elevationMetres, key: op)
               select new SceneSun(Derivation: derivation, Enabled: active.Enabled, IntensityScale: active.Intensity);
    }

    private static Fin<SunDerivation> Derive(SunState state, double elevationMetres, Op key) =>
        state.Placement.Switch(
            context: (State: state, Elevation: elevationMetres, Op: key),
            automatic: static (context, placement) =>
                from site in context.Op.AcceptValidated(SolarSite.Validate(
                    latitudeDeg: placement.Latitude,
                    longitudeDeg: placement.Longitude,
                    timezoneHours: placement.TimeZone,
                    elevationM: context.Elevation,
                    out SolarSite? admitted), admitted)
                from frame in context.Op.AcceptValidated(SolarFrame.Validate(
                    site: site,
                    northAxisDegrees: context.State.North,
                    daylightSavingMinutes: placement.DaylightSaving.IfNone(0),
                    moment: Utc(placement),
                    out SolarFrame? framed), framed)
                select (SunDerivation)new SunDerivation.Sited(
                    Frame: frame,
                    Angles: SolarPosition.At(site: site, instant: frame.Moment)),
            manualAngles: static (context, placement) => Fin.Succ<SunDerivation>(value: new SunDerivation.Authored(
                Angles: new SunPosition(AzimuthDeg: placement.Azimuth, AltitudeDeg: placement.Altitude))),
            manualVector: static (context, placement) =>
                Surveyed(hostVector: placement.Value, northDegrees: context.State.North)
                    .Bind(SunPosition.OfDirection)
                    .Map(static angles => (SunDerivation)new SunDerivation.Authored(Angles: angles))
                    .ToFin(Fail: context.Op.InvalidInput()));

    private static Option<Vector3d> Surveyed(Vector3d hostVector, double northDegrees) {
        Vector3d ray = -hostVector;
        double turn = (northDegrees - 90.0) * Math.PI / 180.0;
        double cos = Math.Cos(turn), sin = Math.Sin(turn);
        return ray.Unitize()
            ? Some(new Vector3d((ray.X * cos) + (ray.Y * sin), (ray.Y * cos) - (ray.X * sin), ray.Z))
            : None;
    }

    private static Instant Utc(SunPlacement.Automatic placement) =>
        Instant.FromDateTimeUtc(DateTime.SpecifyKind(
            placement.Moment
                - TimeSpan.FromHours(placement.TimeZone)
                - TimeSpan.FromMinutes(placement.DaylightSaving.IfNone(0)),
            DateTimeKind.Utc));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SunSolver {
    public static Fin<SunSolution> Solve(SunProblem problem, Op? key = null) {
        Op op = key.OrDefault();
        return from active in op.Need(problem)
               from _ in guard(active.IsValid, op.InvalidInput()).ToFin()
               from solution in active.Switch(
            context: op,
            direction: static (state, query) => state.Catch(() => Fin.Succ<SunSolution>(new SunSolution.Vector(
                global::Rhino.Render.Sun.SunDirection(
                    latitude: query.Latitude, longitude: query.Longitude, when: query.Moment)))),
            altitude: static (state, query) => state.Catch(() => Fin.Succ<SunSolution>(new SunSolution.Scalar(
                global::Rhino.Render.Sun.AltitudeFromValues(
                    latitude: query.Latitude,
                    longitude: query.Longitude,
                    timezoneHours: query.TimeZoneHours,
                    daylightMinutes: query.DaylightMinutes,
                    when: query.Moment,
                    hours: query.Hours,
                    fast: query.Mode.Key)))),
            julian: static (state, query) => state.Catch(() => Fin.Succ<SunSolution>(new SunSolution.Scalar(
                global::Rhino.Render.Sun.JulianDay(
                    timezoneHours: query.TimeZoneHours,
                    daylightMinutes: query.DaylightMinutes,
                    when: query.Moment,
                    hours: query.Hours)))),
            twilight: static (state, _) => state.Catch(() => Fin.Succ<SunSolution>(
                new SunSolution.Scalar(global::Rhino.Render.Sun.TwilightZone()))),
            color: static (state, query) => state.Catch(() =>
                PerceptualColor.OfHost(host: global::Rhino.Render.Sun.ColorFromAltitude(query.AltitudeDegrees), key: state)
                    .Map(static value => (SunSolution)new SunSolution.Color(value))),
            here: static (state, _) => state.Catch(() => Fin.Succ<SunSolution>(new SunSolution.Location(
                global::Rhino.Render.Sun.Here(out double latitude, out double longitude)
                    ? Some((latitude, longitude))
                    : Option<(double, double)>.None))))
               select solution;
    }
}
```

## [05]-[EDIT_RAIL]

- Owner: `SettingsBody` closes the writable sub-owner family; `RenderState` carries the complete detached state with derived evidence; `SettingsRequest`/`SettingsResult` correlate the rail and `Settings.Run` is the sole entry over every `SettingsSource` origin.
- Entry: `Settings.Run` reads the canonical state, applies one `SettingsBody`, or copies one complete `RenderState`.
- Law: the result carries every writable sub-owner as its existing typed state. Copy and compensation apply that state directly; no slot roster, body-kind mirror, accumulator, or projection stands between the producer and caller.
- Law: derived evidence remains read-only. `SunEvidence`, `WorkflowEvidence`, and environment resolution are host projections a replay must never assert, so `RenderState.Apply` touches only the writable state columns.
- Law: each request enters its source once; edit lowers one body inside one compensated mutation grant over a single `SubOwners` window, and copy crosses sources as exactly one source read-window and one target write-window.
- Law: a failed mutation restores the pre-borrow `RenderState` before the fault leaves, with the live bracket's undo rollback layered above it; a restore failure appends onto the primary fault, never replaces it.
- Boundary: `RenderSettings.PostEffects : PostEffectCollection` is a separate host sub-owner whose configuration rows belong to the Display render page.
- Growth: a new writable axis is one `SettingsBody` case and one typed `RenderState` column with its capture and apply paths.
- Packages: kernel `Domain/rails` (`Custody.Settled`, `Op`, `Op.Side`); LanguageExt.Core (`Fin`, `Seq`); Thinktecture.Runtime.Extensions (`[Union]`).

```csharp
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsBody {
    private SettingsBody() { }
    public sealed record Frame(RenderConfig Config) : SettingsBody;
    public sealed record Ground(GroundPlaneState State) : SettingsBody;
    public sealed record Sky(SkylightState State) : SettingsBody;
    public sealed record Daylight(SunState State) : SettingsBody;
    public sealed record Workflow(WorkflowState State) : SettingsBody;
    public sealed record Dither(DitherState State) : SettingsBody;
    public sealed record Guides(SafeFrameState State) : SettingsBody;
    public sealed record Channels(ChannelState State) : SettingsBody;

    internal Fin<Unit> Apply(SubOwners owners, Op op) =>
        Switch(
            context: (Owners: owners, Op: op),
            frame: static (context, body) => context.Op.Need(body.Config)
                .Bind(config => config.Apply(settings: context.Owners.Settings, key: context.Op)),
            ground: static (context, body) => context.Op.Need(body.State)
                .Bind(state => state.Apply(ground: context.Owners.Ground, key: context.Op)),
            sky: static (context, body) => body.State.Apply(sky: context.Owners.Sky, key: context.Op),
            daylight: static (context, body) => context.Op.Need(body.State)
                .Bind(state => state.Apply(sun: context.Owners.Daylight, key: context.Op)),
            workflow: static (context, body) => body.State.Apply(workflow: context.Owners.Workflow, key: context.Op),
            dither: static (context, body) => body.State.Apply(dither: context.Owners.Dither, key: context.Op),
            guides: static (context, body) => context.Op.Need(body.State)
                .Bind(state => state.Apply(frame: context.Owners.Guides, key: context.Op)),
            channels: static (context, body) => context.Op.Need(body.State)
                .Bind(state => state.Apply(channels: context.Owners.Channels, key: context.Op)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsRequest {
    private SettingsRequest() { }
    public sealed record Read : SettingsRequest;
    public sealed record Edit(SettingsBody Change) : SettingsRequest;
    public sealed record CopyTo(SettingsSource Target) : SettingsRequest;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsResult : IDetachedDocumentResult {
    private SettingsResult() { }
    public sealed record State(RenderState Value) : SettingsResult, IDisposable {
        public void Dispose() => Value.Dispose();
    }
    public sealed record Changed : SettingsResult;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderState(
    RenderConfig Frame,
    GroundPlaneState Ground,
    SkylightState Sky,
    SunState Daylight,
    WorkflowState Workflow,
    DitherState Dither,
    SafeFrameState Guides,
    ChannelState Channels,
    SunEvidence DaylightEvidence,
    WorkflowEvidence WorkflowEvidence,
    Seq<(EnvironmentRole Role, EnvironmentView View, Option<Guid> Content)> EnvironmentResolution)
    : IDisposable, IDetachedDocumentResult {
    internal static Fin<RenderState> Of(SubOwners owners, Op key) =>
        from frame in RenderConfig.Of(settings: owners.Settings, key: key)
        from ground in GroundPlaneState.Of(ground: owners.Ground, key: key)
        from sky in key.Catch(() => Fin.Succ(value: SkylightState.Of(sky: owners.Sky)))
        from daylight in SunState.Of(sun: owners.Daylight, key: key)
        from workflow in WorkflowState.Of(workflow: owners.Workflow, key: key)
        from dither in DitherState.Of(dither: owners.Dither, key: key)
        from guides in key.Catch(() => Fin.Succ(value: SafeFrameState.Of(frame: owners.Guides)))
        from channels in ChannelState.Of(channels: owners.Channels, key: key)
        from environments in EnvironmentBindingState.Resolve(settings: owners.Settings, key: key)
        from evidence in SunEvidence.Of(sun: owners.Daylight, key: key)
        select new RenderState(
            Frame: frame,
            Ground: ground,
            Sky: sky,
            Daylight: daylight,
            Workflow: workflow,
            Dither: dither,
            Guides: guides,
            Channels: channels,
            DaylightEvidence: evidence,
            WorkflowEvidence: WorkflowEvidence.Of(workflow: owners.Workflow),
            EnvironmentResolution: environments);

    internal Fin<Unit> Apply(SubOwners owners, Op key) =>
        from frame in key.Need(Frame).Bind(value => value.Apply(settings: owners.Settings, key: key))
        from ground in key.Need(Ground).Bind(value => value.Apply(ground: owners.Ground, key: key))
        from sky in Sky.Apply(sky: owners.Sky, key: key)
        from daylight in key.Need(Daylight).Bind(value => value.Apply(sun: owners.Daylight, key: key))
        from workflow in Workflow.Apply(workflow: owners.Workflow, key: key)
        from dither in Dither.Apply(dither: owners.Dither, key: key)
        from guides in key.Need(Guides).Bind(value => value.Apply(frame: owners.Guides, key: key))
        from channels in key.Need(Channels).Bind(value => value.Apply(channels: owners.Channels, key: key))
        select unit;

    internal Fin<T> Use<T>(Func<RenderState, Fin<T>> borrow, Op key) {
        RenderState self = this;
        return key.Need(borrow)
            .Bind(active => key.Catch(() => active(self)))
            .Settled(held: Seq(self), release: static state => Fin.Succ(value: Op.Side(state.Dispose)), key: key);
    }

    public void Dispose() => DaylightEvidence.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Settings {
    public static Fin<SettingsResult> Run(SettingsSource source, SettingsRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return from activeSource in op.Need(source)
               from activeRequest in op.Need(request)
               from result in activeRequest.Switch(
                   context: (Source: activeSource, Op: op),
                   read: static (state, _) => state.Source.Use(
                       borrow: settings => SubOwners.Within(
                           settings: settings,
                           borrow: owners => RenderState.Of(owners: owners, key: state.Op)
                               .Map(static value => (SettingsResult)new SettingsResult.State(value)),
                           key: state.Op),
                       key: state.Op),
                   edit: static (state, command) => Commit(state.Source, command.Change, state.Op)
                       .Map(static _ => (SettingsResult)new SettingsResult.Changed()),
                   copyTo: static (state, command) => Copy(state.Source, command.Target, state.Op)
                       .Map(static _ => (SettingsResult)new SettingsResult.Changed()))
               select result;
    }

    private static Fin<Unit> Commit(SettingsSource source, SettingsBody change, Op op) =>
        from active in op.Need(change)
        from changed in source.Mutate(
            name: nameof(SettingsRequest.Edit),
            borrow: settings => SubOwners.Within(
                settings: settings,
                borrow: owners => RenderState.Of(owners: owners, key: op)
                    .Bind(prior => Compensated(
                        owners, prior, (state, key) => active.Apply(owners: state, op: key), op)),
                key: op),
            key: op)
        select changed;

    private static Fin<Unit> Compensated(
        SubOwners owners, RenderState prior, Func<SubOwners, Op, Fin<Unit>> apply, Op op) =>
        prior.Use(
            borrow: record => apply(owners, op)
                .BindFail(fault => record.Apply(owners: owners, key: op).Match(
                    Succ: _ => Fin.Fail<Unit>(error: fault),
                    Fail: restore => Fin.Fail<Unit>(error: fault + restore))),
            key: op);

    private static Fin<Unit> Copy(SettingsSource source, SettingsSource target, Op op) =>
        from activeTarget in op.Need(target)
        from state in source.Use(
            borrow: settings => SubOwners.Within(
                settings: settings, borrow: owners => RenderState.Of(owners: owners, key: op), key: op),
            key: op)
        from changed in state.Use(
            borrow: value => activeTarget.Mutate(
                name: nameof(SettingsRequest.CopyTo),
                borrow: settings => SubOwners.Within(
                    settings: settings,
                    borrow: owners => RenderState.Of(owners: owners, key: op)
                        .Bind(prior => Compensated(
                            owners, prior, (active, key) => value.Apply(owners: active, key: key), op)),
                    key: op),
                key: op),
            key: op)
        select changed;
}
```

## [06]-[AMBIENT_WATCH]

- Owner: `AmbientPulse` `[SmartEnum<int>]` carries each catalogued static `Changed` broadcast as one bind row; `AmbientFact` detaches the pulse, optional document key, and host property context; `AmbientWatch` owns transactional attach, symmetric release, and one bounded ring of delivery failures.
- Law: a broadcast row is a PULSE, never a mutation axis; `Render/registry.md`'s `ContentPulse` is the same regime under the same name.
- Law: `LinearWorkflow` and `Dithering` carry no `Changed` event, so their staleness is polled through `Settings.Run(SettingsRequest.Read)`.
- Law: the failure journal IS the kernel bounded ring. A cap, oldest-first eviction, and a drop counter were a page-local retention policy and ledger pair; `Ring<AmbientFailure>` is that shape once for the estate, its `Park` verdict is COUNTED rather than discarded, and a declined park reads as `Lost` where the prior ledger conflated a shed row with a contended write. NAMED LOSS: the accumulated `Error` over every dropped failure; that accumulator grew without bound beside a capped roster, so the cap now bounds what it claimed to.
- Law: `RenderPropertyChangedEvent.Document`, `Context`, `DocKey` projection, sink delivery, and failure retention share one guarded callback rail. `Context` remains the host's opaque integer discriminant, a missing document yields `None`, and projection failure parks a pulse-keyed fallback fact.
- Law: `AmbientWatch.Of` takes the caller's `Op`, because a key minted inside the owner names the owner at every refusal and erases which composition attached the watch.
- Boundary: the parked rows and the shed count read at the plug-in load root (`Plugin/lifecycle.md`'s `OnLoad`), which OWES the census wire-up; until it lands the ring bounds memory and nothing reads its evidence.
- Growth: a new host broadcast is one `AmbientPulse` row with its bind column.
- Packages: `api-rhinocommon-rendersettings.md` (`GroundPlane.Changed`, `Skylight.Changed`, `Sun.Changed`, `SafeFrame.Changed`, `RenderChannels.Changed`, `RenderPropertyChangedEvent.Document`/`Context`); kernel `Domain/hooks` (`Ring<T>`, `Transition`), `Domain/rails` (`Op`, `Op.Catch`, `Cell`); `Document/lifetime.md` (`Subscription.Attach`/`AttachAll`), `Document/session.md` (`DocKey`, `IDetachedDocumentResult`); `Numerics/atoms` (`Dimension`); LanguageExt.Core (`Fin`, `Seq`, `Option`); Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[UseDelegateFromConstructor]`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct AmbientFact(AmbientPulse Pulse, Option<DocKey> Key, int Context) : IDetachedDocumentResult;
public sealed record AmbientFailure(AmbientFact Fact, Error Fault) : IDetachedDocumentResult;

[SmartEnum<int>]
public sealed partial class AmbientPulse {
    public static readonly AmbientPulse Ground = new(key: 0, bind: static handler => Subscription.Attach(
        subscribe: static h => GroundPlane.Changed += h, unsubscribe: static h => GroundPlane.Changed -= h, handler: handler));
    public static readonly AmbientPulse Sky = new(key: 1, bind: static handler => Subscription.Attach(
        subscribe: static h => Skylight.Changed += h, unsubscribe: static h => Skylight.Changed -= h, handler: handler));
    public static readonly AmbientPulse Daylight = new(key: 2, bind: static handler => Subscription.Attach(
        subscribe: static h => global::Rhino.Render.Sun.Changed += h, unsubscribe: static h => global::Rhino.Render.Sun.Changed -= h, handler: handler));
    public static readonly AmbientPulse Guides = new(key: 3, bind: static handler => Subscription.Attach(
        subscribe: static h => SafeFrame.Changed += h, unsubscribe: static h => SafeFrame.Changed -= h, handler: handler));
    public static readonly AmbientPulse Channels = new(key: 4, bind: static handler => Subscription.Attach(
        subscribe: static h => RenderChannels.Changed += h, unsubscribe: static h => RenderChannels.Changed -= h, handler: handler));

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Bind(EventHandler<RenderPropertyChangedEvent> handler);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class AmbientWatch : IDisposable {
    private readonly Atom<Option<Subscription>> subscription;
    private readonly Ring<AmbientFailure> failures;

    private AmbientWatch(Subscription attached, Ring<AmbientFailure> failures) {
        subscription = Atom(Some(attached));
        this.failures = failures;
    }

    public Seq<AmbientFailure> Parked => failures.Parked;
    public long Shed => failures.Shed;
    public long Lost => failures.Lost;

    public void Dispose() => ignore(Cell.Take(cell: subscription).Current.Iter(static held => held.Dispose()));

    public static Fin<AmbientWatch> Of(
        Seq<AmbientPulse> pulses,
        Rasm.Numerics.Dimension cap,
        Func<AmbientFact, Fin<Unit>> sink,
        Op? key = null) {
        Op op = key.OrDefault();
        Ring<AmbientFailure> failures = new(cap: cap);
        return from activeSink in op.Need(sink)
               from _ in guard(
                   ValidityClaim.All(
                       ValidityClaim.CountAtLeast(count: pulses.Count, floor: 1),
                       pulses.ForAll(static pulse => pulse is not null)),
                   op.InvalidInput()).ToFin()
               from attached in Subscription.AttachAll(
                   pulses.Distinct().Map(pulse => (Func<Fin<Subscription>>)(() =>
                       pulse.Bind(handler: (_, args) => ignore(Deliver(
                           pulse: pulse, args: args, sink: activeSink, failures: failures, op: op))))))
               select new AmbientWatch(attached: attached, failures: failures);
    }

    private static Fin<Unit> Deliver(
        AmbientPulse pulse,
        RenderPropertyChangedEvent args,
        Func<AmbientFact, Fin<Unit>> sink,
        Ring<AmbientFailure> failures,
        Op op) {
        AmbientFact fallback = new(Pulse: pulse, Key: None, Context: args.Context);
        return Project(args: args, contextual: fallback, op: op)
            .BindFail(fault => Park(fact: fallback, fault: fault, failures: failures, op: op))
            .Bind(fact => op.Catch(() => sink(fact))
                .BindFail(fault => Park(fact: fact, fault: fault, failures: failures, op: op)));
    }

    private static Fin<AmbientFact> Project(RenderPropertyChangedEvent args, AmbientFact contextual, Op op) =>
        op.Catch(() => Optional(args.Document).Match(
            Some: document => DocKey.Of(document: document, key: op).Map(key => contextual with { Key = Some(key) }),
            None: () => Fin.Succ(value: contextual)));

    private static Fin<Unit> Park(AmbientFact fact, Error fault, Ring<AmbientFailure> failures, Op op) =>
        op.Catch(() => Fin.Succ(value: ignore(failures.Park(item: new AmbientFailure(Fact: fact, Fault: fault)))))
            .Match(
                Succ: _ => Fin.Fail<Unit>(error: fault),
                Fail: retention => Fin.Fail<Unit>(error: fault + retention));
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]                              | [FORM]                             | [ENTRY]                |
| :-----: | :---------------- | :----------------------------------- | :--------------------------------- | :--------------------- |
|  [01]   | live origin       | `SettingsSource.Live`                | document borrow                    | `Use` / `Mutate`       |
|  [02]   | archive origin    | `SettingsSource.Archived`            | archive borrow                     | `Use` / `Mutate`       |
|  [03]   | detached origin   | `SettingsSource.Detached`            | owned borrow                       | `Use` / `Mutate`       |
|  [04]   | sub-owner window  | `SubOwners`                          | bracket-owned seven-wrapper borrow | `Within`               |
|  [05]   | host switches     | `GroundTrait` … `RenderTrait`        | read-and-seat capability rows      | `Of` / `Apply`         |
|  [06]   | guide bands       | `GuideZone` / `GuideBand`            | one band shape over two quadruples | `Reads` / `Seats`      |
|  [07]   | state payloads    | `SettingsBody`                       | one case per axis, one write fold  | `Apply`                |
|  [08]   | aggregate config  | `RenderConfig`                       | correlated configuration           | `Of` / `Apply`         |
|  [09]   | dither vocabulary | `DitherMethod`                       | the one `Dithering.Methods` owner  | `Of(native, key)`      |
|  [10]   | host astronomy    | `SunProblem` / `SunSolution`         | closed request/result              | `SunSolver.Solve`      |
|  [11]   | machine location  | `SunCapability`                      | grant the `Here` case names        | `SunSolver.Solve`      |
|  [12]   | settings rail     | `SettingsRequest` / `SettingsResult` | correlated request/result          | `Settings.Run`         |
|  [13]   | broadcasts        | `AmbientPulse` / `AmbientFailure`    | bound ring over verified pulses    | `AmbientWatch.Of`      |
|  [14]   | engine-bound site | `SolarFrame`                         | annual-run georeference gate       | `SolarFrame.Validate`  |
|  [15]   | descriptor sun    | `SunDerivation` / `SceneSun`         | sited-or-authored wire band        | `SceneSun.Of`          |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
