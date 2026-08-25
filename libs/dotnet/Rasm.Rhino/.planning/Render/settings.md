# [RASM_RHINO_RENDER_SETTINGS]

`SettingsSource` admits live, archived, or detached `RenderSettings` once, and `Settings.Run` closes read, edit, and copy through one correlated request/result family. `SettingsSlot` is the ONE axis roster — its rows carry the host read, the `SettingsBody` union carries the host write, and the receipt is the spine's fact stream over the pair — so an axis is one row and every projection, gate, and undo stamp derives. `SunSolver.Solve` closes host astronomy, `SceneSun` projects the `rasm.contracts.scene` sun band off the kernel almanac, and `AmbientWatch` parks broadcast failures in one ring.

## [01]-[INDEX]

- [02]-[SOURCE]: `SettingsSource` — the origin union with its `Use` read and `Mutate` undo-bracketed borrow folds.
- [03]-[STATE_RECORDS]: the `SubOwners` custody window, the capability vocabularies, the writable sub-owner states, derived evidence, and `RenderConfig`.
- [04]-[SUN_ASTRONOMY]: `SunProblem`/`SunSolution`/`SunSolver` over the host statics, beside the `SolarFrame`/`SunDerivation`/`SceneSun` descriptor band.
- [05]-[EDIT_RAIL]: `SettingsSlot`, `SettingsBody`, `SettingsReceipt`, `RenderState`, and the `Settings.Run` request/result rail.
- [06]-[AMBIENT_WATCH]: `AmbientPulse` and the `Changed`-broadcast fold over a bounded ring.
- [07]-[SURFACE_LEDGER]: page owner table.

## [02]-[SOURCE]

- Owner: `SettingsSource` `[Union]` — `Live` resolves `RhinoDoc.RenderSettings` inside a `Demand` window, `Archived` resolves the archive-bound `File3dm.Settings.RenderSettings`, and `Free` mints one owned free-floating `RenderSettings` retained until source disposal; `Use` borrows the selected aggregate for exactly one read callback, and `Mutate` borrows it for exactly one mutation callback — the live arm demanding `Mutate`+`Undo`, opening one named `UndoBracket`, and stamping the undo serial onto the `SettingsReceipt`.
- Law: the origin is the discriminant a consumer carries — the same `GroundPlane` type is document-bound, archive-attached, or free-floating by the host's internal pointer resolution, so no parallel type pair exists on this side of the seam and no live sub-owner leaves the borrow.
- Law: writes are in-place — a bound sub-owner commits through its native pointer, inert `BeginChange`/`EndChange` never appear, and cross-source copy replays one detached total state.
- Law: only the document owns an undo record — archive and detached mutations apply without one; archive persistence occurs at `File3dm.Write`, while detached values remain locally owned, so their receipts carry no serial. The stamp is the stream's own projection, so an unrecorded program contributes no fact instead of one claiming record zero.
- Law: `RhinoDoc.RenderSettings` answers a FRESH document-bound wrapper on every read, so the aggregate enters the borrow once and threads — two reads of one property are two wrappers over one native and two instants the `Changed` broadcast can move between.
- Boundary: the document and archive accessors are the document and file-IO catalogs' seam; this union names them once and every settings verb enters through it.
- Packages: `api-rhinocommon-rendersettings.md` (`RenderSettings`, `DocumentOrFreeFloatingBase`, `RhinoDoc.RenderSettings`); `api-rhinocommon-fileio.md` (`File3dm.Settings.RenderSettings`); kernel `Domain/rails` (`Op`, `Op.Catch`, `Op.Need`, `Lease<T>.Acquire`); `Document/session.md` (`DocumentSession.Demand`, `SessionNeed`, `RedrawPolicy`, `IDetachedDocumentResult`), `Document/commit.md` (`DocumentCommit.Sealed`), `Document/facts.md` (`FactStream.Stamped`); LanguageExt.Core (`Fin`); Thinktecture.Runtime.Extensions (`[Union]`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
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

// --- [TYPES] --------------------------------------------------------------------------------
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

    internal Fin<SettingsReceipt> Mutate(string name, Func<RenderSettings, Fin<SettingsReceipt>> borrow, Op key) =>
        Switch(
            context: (Name: name, Borrow: borrow, Op: key),
            live: static (ctx, source) =>
                from session in ctx.Op.Need(source.Session)
                from receipt in session.Demand(
                    use: document => DocumentCommit.Sealed(
                        document: document,
                        name: ctx.Name,
                        recordsUndo: true,
                        redraw: RedrawPolicy.None,
                        run: () =>
                            from settings in Optional(document.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                            from receipt in ctx.Borrow(settings)
                            select receipt,
                        stamp: static (receipt, serial) => receipt.Stamped(
                            slot: SettingsSlot.Undo,
                            record: static value => new SettingsBody.Record(Serial: value),
                            serial: serial),
                        project: Fin.Succ,
                        op: ctx.Op),
                    key: ctx.Op,
                    needs: SessionNeed.Mutation(undo: true, redraw: RedrawPolicy.None).ToArray())
                select receipt,
            archived: static (ctx, source) =>
                from archive in ctx.Op.Need(source.Archive)
                from receipt in ctx.Op.Catch(() =>
                    from settings in Optional(archive.Settings.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                    from applied in ctx.Borrow(settings)
                    select applied)
                select receipt,
            detached: static (ctx, source) =>
                from settings in ctx.Op.Need(source.Settings)
                from receipt in ctx.Op.Catch(() => ctx.Borrow(settings.Resource))
                select receipt);

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
- Growth: a new host switch is one vocabulary row; a new sub-owner property is one record field read and asserted in the same pass; a new sub-owner is one record, one `SettingsSlot` row, and one `SettingsBody` case.
- Packages: `api-rhinocommon-rendersettings.md` (`GroundPlane`, `Skylight`, `Sun`, `Sun.Accuracies`, `Sun.SetPosition`, `Sun.SetDateTime`/`GetDateTime`, `Sun.Light`/`Vector`/`Hash`, `LinearWorkflow`, `Dithering`, `Dithering.Methods`, `SafeFrame`, `RenderChannels`, `RenderChannels.Modes`, `RenderSettings.EnvironmentUsage`/`EnvironmentPurpose`/`RenderingSources`, `RenderEnvironmentId`/`SetRenderEnvironmentId`/`RenderEnvironmentOverride`/`SetRenderEnvironmentOverride`, `BackgroundStyle`, `AntialiasLevel`); `api-rhinocommon-document.md` (`LengthUnit`); kernel `Domain/rails` (`Op.Row`, `Op.Catch`, `Op.Confirm`, `Op.Side`, `ValidityClaim`, `Lease<T>`), `Domain/validation` (`ICapability`, `CapabilitySet`), `Domain/context` (`ModelUnit`), `Numerics/atoms` (`PerceptualColor.OfHost`/`ToDrawing`, `Size2i`); `Document/tables.md` (`ResourceId`), kernel `Domain/rails` (`Custody.Settled`); LanguageExt.Core (`Fin`, `Seq`, `Option`); Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`, `[ComplexValueObject]`, `[ValueObject]`, `[UseDelegateFromConstructor]`).

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
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

    // The window's release belongs to the BRACKET, never to a borrow: `Within` opens the seven wrappers once, runs
    // whatever sequence of borrows the body asks against that one instant, and retires the finalizer registrations
    // on exit through the package's both-arms release fold — so a release refusal APPENDS to the body's fault where
    // a `finally` replaced it, and a read-then-write body stays expressible over ONE coherent wrapper set.
    internal static Fin<TOut> Within<TOut>(RenderSettings settings, Func<SubOwners, Fin<TOut>> borrow, Op key) =>
        from active in key.Need(settings)
        from activeBorrow in key.Need(borrow)
        from owners in key.Catch(() => Fin.Succ(value: new SubOwners(settings: active)))
        from result in key.Catch(() => activeBorrow(owners))
            .Settled(held: Seq(owners), release: window => window.Release(key), key: key)
        select result;

    // Host truth: every sub-owner read off a `RenderSettings` is a NON-OWNING wrapper — the private `Dispose(bool)`
    // body is empty and the public `Dispose` only runs `GC.SuppressFinalize`, so this release retires seven
    // finalizer registrations and frees no native. A genuinely free-floating sub-owner inverts that: disposing it
    // suppresses the finalizer that is its ONLY `DeleteCpp` path, so such a value never enters this window.
    private Fin<Unit> Release(Op key) => Custody.Release(
        held: held,
        release: owner => key.Catch(() => Fin.Succ(value: Op.Side(owner.Dispose))),
        key: key);
}

// --- [TYPES] --------------------------------------------------------------------------------
// Each row carries BOTH directions of its own host switch, so `Of` is one `Where` over `Items` and `Apply` one
// `Iter`: the read and the seat cannot drift, and a new switch is one row rather than a record field, an `Of`
// argument, and an `Apply` statement. `Rasm.Rhino.Display`'s two-row local vocabulary of this name composes here.
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

// The host publishes ONE band shape twice — on, linked, and two scales for the action frame and again for the
// title frame — so the quadruple is `GuideBand` once and the row names which host properties it reads and seats.
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

// `DitherMethod` is the `Rasm.Rhino.Render` namespace's ONE dither vocabulary: the settings sub-owner and the
// Display render window both bind these rows. Host truth: `Dithering.Method` is a TWO-state native variant wearing
// a three-row enum — the getter answers `FloydSteinberg` for any non-zero and `SimpleNoise` otherwise and never
// answers `None`, and the setter writes `1` for anything but `SimpleNoise` — so `None` is an admissible INPUT row
// that does not round-trip, and `Dithering.Enabled` is the real off switch a consumer wanting no dithering writes.
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
    // The host publishes an on switch beside a minutes column and reads the minutes as zero when the switch is
    // off, so the pair is ONE optional column and no consumer re-derives the pairing.
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
        // `Vector3d.IsValid` gates finiteness ALONE and admits the zero vector, which the host then reads back as
        // a due-south horizon sun — a plausible angle pair no consumer separates from a measured one. The kernel
        // direction claim refuses that ray here; a denormal ray still unitizes and stays admitted.
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

// --- [MODELS] -------------------------------------------------------------------------------
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

    // `SetPosition` seats `Azimuth`, `Altitude`, and the derived `Vector` together; two property writes leave
    // `Vector` stale between them, and the host reads that stale ray back for any consumer sampling mid-apply.
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

// --- [TYPES] --------------------------------------------------------------------------------
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

// `ScaleBackgroundToFit` holds on BOTH cases, so it is a base column rather than a per-case field a construction
// site could set on one arm and forget on the other.
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

// --- [MODELS] -------------------------------------------------------------------------------
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

    // The full usage-purpose product, read off the aggregate rather than the admitted rows: a purpose the standard
    // read never carried is exactly what this projection exists to publish.
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
- Boundary: `SceneSun` is a BAND, not a descriptor — `Objects/lights.md`'s `Lights.Capture` is the declared whole-descriptor emitter and takes this value beside its photometric rows, and its `[Mapper] SceneMap` is the ONE transcription onto `rasm.contracts.scene`; a second mapper here forks the wire.
- Packages: `api-rhinocommon-rendersettings.md` (`Sun.SunDirection`, `Sun.AltitudeFromValues`, `Sun.JulianDay`, `Sun.TwilightZone`, `Sun.ColorFromAltitude`, `Sun.Here`); kernel `Numerics/calculus` (`SolarSite`, `SolarPosition.At`, `SunPosition`, `SunPosition.OfDirection`), `Numerics/atoms` (`PerceptualColor.OfHost`), `Domain/rails` (`Op.Catch`, `Op.AcceptValidated`, `ValidityClaim`); NodaTime (`Instant`, `Instant.FromDateTimeUtc`); LanguageExt.Core (`Fin`, `Option`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[ComplexValueObject]`).

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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

// `Sun.Here(out double, out double)` reads the MACHINE's geolocation service — where the running computer is — not
// the document, not the earth anchor, and not the astronomy model every other problem evaluates over supplied
// coordinates. That is a host-facts capability rather than a solve input, so it enters only as the grant a caller
// names, and an implicit machine read inside an otherwise-pure solve is the deleted form.
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

// --- [MODELS] --------------------------------------------------------------------------------
// Engine bounds sit BELOW the kernel site's own gate: `SolarSite` admits time zone `[-14, 14]` and elevation
// `(-500, 10000]` because astronomy holds there, while an annual building run reads `[-12, 14]` hours and
// `[-300, 8900)` metres. Admitting the wider pair and letting the consumer refuse moves the refusal past the only
// point that still knows which document produced it.
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

    // The almanac is the kernel's and it is TOTAL over an admitted site, so the sited arm reads it directly: the
    // site and the frame admit, and the angle pair is one call. A peer holding the same almanac reproduces the pair
    // from the frame alone, which is the whole reason the descriptor carries both.
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

    // `Sun.Vector` points sun-TOWARD-scene — the direction light travels — so the scene-toward-sun ray the survey
    // frame speaks is its negation. `Sun.North` carries the document's compass north as a counter-clockwise angle
    // off `+X`, `90` (the host default) seating north on `+Y` and making the world frame the survey frame outright,
    // so the turn that derotates a document is the bearing's OFFSET from that default. Taking the offset rather
    // than the bearing keeps the default exact — a rotation built on `cos(90°)` instead carries its round-off into
    // every reading and lands a due-north sun a few ulps BELOW `360`, in the last compass bucket rather than the
    // first. Absence answers a ray that cannot unitize, which the host collapses to a due-south horizon reading.
    private static Option<Vector3d> Surveyed(Vector3d hostVector, double northDegrees) {
        Vector3d ray = -hostVector;
        double turn = (northDegrees - 90.0) * Math.PI / 180.0;
        double cos = Math.Cos(turn), sin = Math.Sin(turn);
        return ray.Unitize()
            ? Some(new Vector3d((ray.X * cos) + (ray.Y * sin), (ray.Y * cos) - (ray.X * sin), ray.Z))
            : None;
    }

    // `Sun.GetDateTime(DateTimeKind.Local)` hands back the host's WALL clock, so the instant the almanac reads
    // subtracts the zone and whatever saving offset the document had armed; folding only the zone shifts every
    // summer capture by its saving minutes and moves the solved altitude with it.
    private static Instant Utc(SunPlacement.Automatic placement) =>
        Instant.FromDateTimeUtc(DateTime.SpecifyKind(
            placement.Moment
                - TimeSpan.FromHours(placement.TimeZone)
                - TimeSpan.FromMinutes(placement.DaylightSaving.IfNone(0)),
            DateTimeKind.Utc));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
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

- Owner: `SettingsBodyKind`, `SettingsSlot`, and `SettingsBody` are this page's whole contribution to the Document spine's fact stream — a kind vocabulary, a keyed slot roster carrying the host read as a delegate column, and a payload union carrying the host write as one total fold; `SettingsReceipt` is the closed instantiation and `SettingsReceipts` the mint surface. `SettingsRequest`/`SettingsResult` correlate the rail and `Settings.Run` is the sole entry over every `SettingsSource` origin.
- Entry: `SettingsReceipt.Edit(body)` mints one fact — the body's own kind selects its slot through the stream's gate, so a caller names the payload and never the axis; `SettingsSlot.State(owners, key)` is the whole-state read and `SettingsBody.Apply` the whole-state write.
- Law: the axis roster has ONE spelling. It stood in six — a slot enum, an edit union with its axis column, that union's apply switch, a total-state record's fields, that record's replay list, and the read fold's constructor — so a new sub-owner touched six places and any two disagreed about which axes exist. The row now carries the read, the body case carries the write, and every other spelling DERIVES. NAMED LOSS: `RenderState`'s eleven typed accessors, so an internal caller wanting one axis projects it off the stream by slot; bought back by the stream's slot-keyed readers, its monoid, and its gate, none of which the record had.
- Law: the stream MACHINERY is not this page's. The accumulation, the cross-product gate, the undo projection, and the slot-keyed readers live once on `Document/facts.md`; a page-local receipt, fact, gate, or projection beside that owner is the deleted form, and the same two declarations are all a third mutation folder needs to join.
- Law: the undo serial refuses zero. `DocumentCommit.Sealed` stamps every sealed receipt including a program that opened no record, and that serial is `0u` — the prior receipt wrapped it in `Some` and published a fact claiming record zero, indistinguishable from a real record. `UndoSerial.Maybe` refuses it, so an unrecorded program contributes no fact at all.
- Law: the undo slot has NO read column. Its body is minted by the commit envelope's stamp rather than sampled off a sub-owner, so its read is absent by type and the whole-state fold skips it without a predicate; replaying a stamped receipt refuses at that body's own write arm, because an undo record is evidence, never an edit.
- Law: derived evidence is a `RenderState` column, not a slot. `SunEvidence`, `WorkflowEvidence`, and the environment resolution are host projections a replay must never re-assert, so they sit beside the fact stream rather than inside it — a slot carrying one puts a non-replayable body into every replay plan.
- Law: each request enters its source once; edit and whole-state replay lower through one receipt inside one compensated mutation grant over a single `SubOwners` window, and copy crosses sources as exactly one source read-window and one target write-window — the receipt IS the replayable carrier, so no duplicate aggregate is minted and no live aggregate outlives its window.
- Law: a failed edit sequence restores the pre-borrow total state before the fault leaves — the prior `RenderState` is the compensation record for every source, archive and detached included, with the live bracket's undo rollback layered above it; a restore failure appends onto the primary fault, never replaces it. The disposal bracket is the package's both-arms release fold, so a compensation record whose lease refuses to release reports that refusal rather than swallowing it in a `using` exit.
- Boundary: `RenderSettings.PostEffects : PostEffectCollection` is a separate host sub-owner whose configuration rows belong to the Display render page.
- Growth: a new configuration axis is one `SettingsSlot` row, one `SettingsBody` case, and one `SettingsBodyKind` row with every consumer untouched.
- Packages: `Document/facts.md` (`IFactSlot<TBody, TKind>`, `IFactBody<TKind>`, `Fact`, `FactStream`, `UndoSerial`), kernel `Domain/rails` (`Custody.Settled`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`), `Domain/rails` (`Op`, `Op.Side`); LanguageExt.Core (`Fin`, `Seq`, `Traverse`, `TraverseM`); Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`).

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SettingsBodyKind : ICapability<SettingsBodyKind> {
    public static readonly SettingsBodyKind Frame = new(key: "frame");
    public static readonly SettingsBodyKind Ground = new(key: "ground");
    public static readonly SettingsBodyKind Sky = new(key: "sky");
    public static readonly SettingsBodyKind Daylight = new(key: "daylight");
    public static readonly SettingsBodyKind Workflow = new(key: "workflow");
    public static readonly SettingsBodyKind Dither = new(key: "dither");
    public static readonly SettingsBodyKind Guides = new(key: "guides");
    public static readonly SettingsBodyKind Channels = new(key: "channels");
    public static readonly SettingsBodyKind Record = new(key: "record");
}

// The ONE axis roster: the key orders the receipt, `Bodies` is the readable admission set the kinded contract
// derives `Admits` from, and `Read` is the host sample. The undo row carries no read because the commit envelope
// mints its body — absence is the type, so the whole-state fold needs no predicate beside it.
[SmartEnum<int>]
public sealed partial class SettingsSlot : IFactSlot<SettingsBody, SettingsBodyKind> {
    // Read-before-use: the row initializers consume these sets, so static construction order decides declaration
    // order here rather than the public-before-private one.
    private static readonly CapabilitySet<SettingsBodyKind> Framed = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Frame);
    private static readonly CapabilitySet<SettingsBodyKind> Grounded = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Ground);
    private static readonly CapabilitySet<SettingsBodyKind> Skied = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Sky);
    private static readonly CapabilitySet<SettingsBodyKind> Lit = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Daylight);
    private static readonly CapabilitySet<SettingsBodyKind> Piped = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Workflow);
    private static readonly CapabilitySet<SettingsBodyKind> Dithered = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Dither);
    private static readonly CapabilitySet<SettingsBodyKind> Guided = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Guides);
    private static readonly CapabilitySet<SettingsBodyKind> Channelled = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Channels);
    private static readonly CapabilitySet<SettingsBodyKind> Stamped = CapabilitySet<SettingsBodyKind>.Of(SettingsBodyKind.Record);

    public static readonly SettingsSlot Frame = new(key: 0, bodies: Framed, read: Some<Sampler>(
        static (owners, op) => RenderConfig.Of(settings: owners.Settings, key: op)
            .Map(static config => (SettingsBody)new SettingsBody.Frame(Config: config))));
    public static readonly SettingsSlot Ground = new(key: 1, bodies: Grounded, read: Some<Sampler>(
        static (owners, op) => GroundPlaneState.Of(ground: owners.Ground, key: op)
            .Map(static state => (SettingsBody)new SettingsBody.Ground(State: state))));
    public static readonly SettingsSlot Sky = new(key: 2, bodies: Skied, read: Some<Sampler>(
        static (owners, op) => op.Catch(() => Fin.Succ(
            value: (SettingsBody)new SettingsBody.Sky(State: SkylightState.Of(sky: owners.Sky))))));
    public static readonly SettingsSlot Daylight = new(key: 3, bodies: Lit, read: Some<Sampler>(
        static (owners, op) => SunState.Of(sun: owners.Daylight, key: op)
            .Map(static state => (SettingsBody)new SettingsBody.Daylight(State: state))));
    public static readonly SettingsSlot Workflow = new(key: 4, bodies: Piped, read: Some<Sampler>(
        static (owners, op) => WorkflowState.Of(workflow: owners.Workflow, key: op)
            .Map(static state => (SettingsBody)new SettingsBody.Workflow(State: state))));
    public static readonly SettingsSlot Dither = new(key: 5, bodies: Dithered, read: Some<Sampler>(
        static (owners, op) => DitherState.Of(dither: owners.Dither, key: op)
            .Map(static state => (SettingsBody)new SettingsBody.Dither(State: state))));
    public static readonly SettingsSlot Guides = new(key: 6, bodies: Guided, read: Some<Sampler>(
        static (owners, op) => op.Catch(() => Fin.Succ(
            value: (SettingsBody)new SettingsBody.Guides(State: SafeFrameState.Of(frame: owners.Guides))))));
    public static readonly SettingsSlot Channels = new(key: 7, bodies: Channelled, read: Some<Sampler>(
        static (owners, op) => ChannelState.Of(channels: owners.Channels, key: op)
            .Map(static state => (SettingsBody)new SettingsBody.Channels(State: state))));
    public static readonly SettingsSlot Undo = new(key: 8, bodies: Stamped, read: None);

    internal delegate Fin<SettingsBody> Sampler(SubOwners owners, Op key);

    public CapabilitySet<SettingsBodyKind> Bodies { get; }

    private Option<Sampler> Read { get; }

    internal Fin<SettingsReceipt> Sample(SubOwners owners, Op key) =>
        Read.Match(
            Some: read => read(owners, key).Bind(body => SettingsReceipt.Of(slot: this, body: body, key: key)),
            None: () => Fin.Succ(value: SettingsReceipt.Empty));

    // The whole-state read IS the roster fold: every sampling row contributes one fact, the undo row contributes
    // none, and the stream's own monoid assembles the result.
    internal static Fin<SettingsReceipt> State(SubOwners owners, Op key) =>
        toSeq(Items)
            .Traverse(slot => slot.Sample(owners: owners, key: key).ToValidation())
            .As()
            .ToFin()
            .Map(static facts => facts.Fold(SettingsReceipt.Empty, static (state, next) => state + next));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsBody : IFactBody<SettingsBodyKind> {
    private SettingsBody() { }
    public sealed record Frame(RenderConfig Config) : SettingsBody;
    public sealed record Ground(GroundPlaneState State) : SettingsBody;
    public sealed record Sky(SkylightState State) : SettingsBody;
    public sealed record Daylight(SunState State) : SettingsBody;
    public sealed record Workflow(WorkflowState State) : SettingsBody;
    public sealed record Dither(DitherState State) : SettingsBody;
    public sealed record Guides(SafeFrameState State) : SettingsBody;
    public sealed record Channels(ChannelState State) : SettingsBody;
    public sealed record Record(UndoSerial Serial) : SettingsBody;

    public SettingsBodyKind Kind => Map(
        frame: SettingsBodyKind.Frame,
        ground: SettingsBodyKind.Ground,
        sky: SettingsBodyKind.Sky,
        daylight: SettingsBodyKind.Daylight,
        workflow: SettingsBodyKind.Workflow,
        dither: SettingsBodyKind.Dither,
        guides: SettingsBodyKind.Guides,
        channels: SettingsBodyKind.Channels,
        record: SettingsBodyKind.Record);

    // The write half of the roster: one total fold, one arm per payload, and the undo record REFUSING because a
    // stamp is evidence rather than an edit — a silently succeeding arm would let a stamped receipt replay clean.
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
                .Bind(state => state.Apply(channels: context.Owners.Channels, key: context.Op)),
            record: static (context, _) => Fin.Fail<Unit>(error: context.Op.InvalidInput()));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsRequest {
    private SettingsRequest() { }
    public sealed record Read : SettingsRequest;
    public sealed record Edit(SettingsReceipt Plan) : SettingsRequest;
    public sealed record CopyTo(SettingsSource Target) : SettingsRequest;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsResult : IDetachedDocumentResult {
    private SettingsResult() { }
    public sealed record State(RenderState Value) : SettingsResult, IDisposable {
        public void Dispose() => Value.Dispose();
    }
    public sealed record Changed(SettingsReceipt Receipt) : SettingsResult;
}

// --- [EXPORTS] -------------------------------------------------------------------------------
// The page's receipt IS the spine's stream closed over this page's two vocabularies. These are `.cs` `global using`
// rows in a namespace-scoped file of their own — a file-scoped namespace forecloses them — so no consumer spells
// the instantiation and no page-local receipt type exists to drift from the owner.
global using SettingsFact = Rasm.Rhino.Document.Fact<Rasm.Rhino.Render.SettingsSlot, Rasm.Rhino.Render.SettingsBody>;
global using SettingsReceipt = Rasm.Rhino.Document.FactStream<Rasm.Rhino.Render.SettingsSlot, Rasm.Rhino.Render.SettingsBody>;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record RenderState(
    SettingsReceipt Facts,
    SunEvidence DaylightEvidence,
    WorkflowEvidence WorkflowEvidence,
    Seq<(EnvironmentRole Role, EnvironmentView View, Option<Guid> Content)> EnvironmentResolution)
    : IDisposable, IDetachedDocumentResult {
    // `Sun` and `LinearWorkflow` each answer a FRESH wrapper per property read, so the state and its evidence read
    // one borrowed instance apiece — two reads of one sub-owner are two unsynchronized samples of live host state.
    internal static Fin<RenderState> Of(SubOwners owners, Op key) =>
        from facts in SettingsSlot.State(owners: owners, key: key)
        from environments in EnvironmentBindingState.Resolve(settings: owners.Settings, key: key)
        from evidence in SunEvidence.Of(sun: owners.Daylight, key: key)
        select new RenderState(
            Facts: facts,
            DaylightEvidence: evidence,
            WorkflowEvidence: WorkflowEvidence.Of(workflow: owners.Workflow),
            EnvironmentResolution: environments);

    internal Fin<T> Use<T>(Func<RenderState, Fin<T>> borrow, Op key) where T : IDetachedDocumentResult {
        RenderState self = this;
        return key.Need(borrow)
            .Bind(active => key.Catch(() => active(self)))
            .Settled(held: Seq(self), release: static state => Fin.Succ(value: Op.Side(state.Dispose)), key: key);
    }

    public void Dispose() => DaylightEvidence.Dispose();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The mint surface rides an extension block over the closed instantiation, so every `SettingsReceipt.*` call site
// reads as this page's while the accumulation and the gate stay on the one owner.
public static class SettingsReceipts {
    extension(SettingsReceipt) {
        // The body's own kind SELECTS its slot through the stream's gate, so a caller names the payload and the
        // axis derives — the last site that could pair an axis with the wrong state.
        public static Fin<SettingsReceipt> Edit(SettingsBody body, Op? key = null) {
            Op op = key.OrDefault();
            return from active in op.Need(body)
                   from slot in toSeq(SettingsSlot.Items)
                       .Find(row => row.Admits(body: active))
                       .ToFin(Fail: op.InvalidInput())
                   from receipt in SettingsReceipt.Of(slot: slot, body: active, key: op)
                   select receipt;
        }
    }
}

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
                   edit: static (state, command) => Commit(state.Source, command.Plan, state.Op)
                       .Map(static receipt => (SettingsResult)new SettingsResult.Changed(receipt)),
                   copyTo: static (state, command) => Copy(state.Source, command.Target, state.Op)
                       .Map(static receipt => (SettingsResult)new SettingsResult.Changed(receipt)))
               select result;
    }

    private static Fin<SettingsReceipt> Commit(SettingsSource source, SettingsReceipt plan, Op op) =>
        from _ in guard(!plan.Facts.IsEmpty, op.InvalidInput()).ToFin()
        from receipt in source.Mutate(
            name: nameof(SettingsRequest.Edit),
            borrow: settings => SubOwners.Within(
                settings: settings,
                borrow: owners => RenderState.Of(owners: owners, key: op)
                    .Bind(prior => Compensated(owners, prior, plan, op)),
                key: op),
            key: op)
        select receipt;

    private static Fin<SettingsReceipt> Compensated(
        SubOwners owners, RenderState prior, SettingsReceipt plan, Op op) =>
        prior.Use(
            borrow: record => ApplyPlan(owners: owners, plan: plan, op: op)
                .BindFail(fault => ApplyPlan(owners: owners, plan: record.Facts, op: op).Match(
                    Succ: _ => Fin.Fail<SettingsReceipt>(error: fault),
                    Fail: restore => Fin.Fail<SettingsReceipt>(error: fault + restore))),
            key: op);

    // `RenderState` IS the detached replayable carrier, so the source borrow yields it directly; a `Duplicate()`
    // lease would mint a second native, re-read the same total state off it, and carry a live aggregate the
    // detached marker cannot type. Two sub-owner windows total and no more: ONE read window over the source, ONE
    // write window over the target whose prior read and whose apply are two borrows of the same seven wrappers.
    private static Fin<SettingsReceipt> Copy(SettingsSource source, SettingsSource target, Op op) =>
        from activeTarget in op.Need(target)
        from state in source.Use(
            borrow: settings => SubOwners.Within(
                settings: settings, borrow: owners => RenderState.Of(owners: owners, key: op), key: op),
            key: op)
        from receipt in state.Use(
            borrow: value => activeTarget.Mutate(
                name: nameof(SettingsRequest.CopyTo),
                borrow: settings => SubOwners.Within(
                    settings: settings,
                    borrow: owners => RenderState.Of(owners: owners, key: op)
                        .Bind(prior => Compensated(owners: owners, prior: prior, plan: value.Facts, op: op)),
                    key: op),
                key: op),
            key: op)
        select receipt;

    private static Fin<SettingsReceipt> ApplyPlan(SubOwners owners, SettingsReceipt plan, Op op) =>
        plan.Facts.TraverseM(fact => fact.Body.Apply(owners: owners, op: op)).As().Map(_ => plan);
}
```

## [06]-[AMBIENT_WATCH]

- Owner: `AmbientPulse` `[SmartEnum<int>]` carries each catalogued static `Changed` broadcast as one bind row; `AmbientFact` detaches the pulse, optional document key, and host property context; `AmbientWatch` owns transactional attach, symmetric release, and one bounded ring of delivery failures.
- Law: a broadcast row is a PULSE, never a slot. The word `Slot` names a mutation-consequence vocabulary on this boundary — the fact-stream contract at `Document/facts.md` and its `[05]` instantiation here — so an event-bind roster wearing it read as a receipt axis a reader hands to `Settings.Run`. `Render/registry.md`'s `ContentPulse` is the same regime under the same name.
- Law: `LinearWorkflow` and `Dithering` carry no `Changed` event, so their staleness is polled through `Settings.Run(SettingsRequest.Read)`.
- Law: the failure journal IS the kernel bounded ring. A cap, oldest-first eviction, and a drop counter were a page-local retention policy and ledger pair; `Ring<AmbientFailure>` is that shape once for the estate, its `Park` verdict is COUNTED rather than discarded, and a declined park reads as `Lost` where the prior ledger conflated a shed row with a contended write. NAMED LOSS: the accumulated `Error` over every dropped failure; that accumulator grew without bound beside a capped roster, so the cap now bounds what it claimed to.
- Law: `RenderPropertyChangedEvent.Document`, `Context`, `DocKey` projection, sink delivery, and failure retention share one guarded callback rail. `Context` remains the host's opaque integer discriminant, a missing document yields `None`, and projection failure parks a pulse-keyed fallback fact.
- Law: `AmbientWatch.Of` takes the caller's `Op`, because a key minted inside the owner names the owner at every refusal and erases which composition attached the watch.
- Boundary: the parked rows and the shed count read at the plug-in load root (`Plugin/lifecycle.md`'s `OnLoad`), which OWES the census wire-up; until it lands the ring bounds memory and nothing reads its evidence.
- Growth: a new host broadcast is one `AmbientPulse` row with its bind column.
- Packages: `api-rhinocommon-rendersettings.md` (`GroundPlane.Changed`, `Skylight.Changed`, `Sun.Changed`, `SafeFrame.Changed`, `RenderChannels.Changed`, `RenderPropertyChangedEvent.Document`/`Context`); kernel `Domain/hooks` (`Ring<T>`, `Transition`), `Domain/rails` (`Op`, `Op.Catch`, `Cell`); `Document/lifetime.md` (`Subscription.Attach`/`AttachAll`), `Document/session.md` (`DocKey`, `IDetachedDocumentResult`); `Numerics/atoms` (`Dimension`); LanguageExt.Core (`Fin`, `Seq`, `Option`); Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[UseDelegateFromConstructor]`).

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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

// --- [SERVICES] -----------------------------------------------------------------------------
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

    // `args.Context` is a plain integer property, so the fallback fact carries it directly; the projection and the
    // sink are the two rails that can refuse, and each parks the fact it was holding before the fault leaves.
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

    // A declined park is the ring's own `Lost` count, so the delivery reports the ORIGINAL fault either way and no
    // retention refusal masquerades as the reason the sink failed.
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
|  [13]   | axis roster       | `SettingsSlot` / `SettingsBodyKind`  | keyed slots with the host read     | `SettingsSlot.State`   |
|  [14]   | mutation receipt  | `SettingsReceipt`                    | the spine's stream, undo-stamped   | `SettingsReceipt.Edit` |
|  [15]   | broadcasts        | `AmbientPulse` / `AmbientFailure`    | bound ring over verified pulses    | `AmbientWatch.Of`      |
|  [16]   | engine-bound site | `SolarFrame`                         | annual-run georeference gate       | `SolarFrame.Validate`  |
|  [17]   | descriptor sun    | `SunDerivation` / `SceneSun`         | sited-or-authored wire band        | `SceneSun.Of`          |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
