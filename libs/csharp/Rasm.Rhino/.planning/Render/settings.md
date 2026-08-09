# [RASM_RHINO_RENDER_SETTINGS]

`SettingsSource` admits live, archived, or owned detached `RenderSettings` once, while `Settings.Run` closes read, edit, and whole-state copy through one correlated request/result family. Generated owners close host classifications, `RenderState` carries replayable configuration plus derived evidence, `SunSolver.Solve` closes astronomy, and `AmbientWatch` retains a bounded latest-failure ledger beside its verified broadcasts.

## [01]-[INDEX]

- [02]-[SOURCE]: `SettingsSource` — the duality union with its `Use` read and `Mutate` undo-bracketed borrow folds.
- [03]-[STATE_RECORDS]: the `SubOwners` custody window, writable sub-owner states, derived evidence, and `RenderConfig`.
- [04]-[SUN_ASTRONOMY]: `SunSolver` — the static position, calendar, and twilight solvers beside the `SceneSun` wire band.
- [05]-[EDIT_RAIL]: `SettingsRequest`, `SettingsResult`, whole-state copy, and receipted edits.
- [06]-[AMBIENT_WATCH]: `AmbientSlot` and the `Changed`-broadcast fold.
- [07]-[SURFACE_LEDGER]: page owner table.

## [02]-[SOURCE]

- Owner: `SettingsSource` `[Union]` — `Live` resolves `RhinoDoc.RenderSettings` inside a `Demand` window, `Archived` resolves the archive-bound `File3dm.Settings.RenderSettings`, and `Free` mints one owned free-floating `RenderSettings` retained until source disposal; `Use` borrows the selected aggregate for exactly one read callback, and `Mutate` borrows it for exactly one mutation callback — the live arm demanding `Mutate`+`Undo`, opening one named `UndoBracket`, and stamping the undo serial onto the `SettingsReceipt`.
- Law: the origin is the discriminant a consumer carries — the same `GroundPlane` type is document-bound, archive-attached, or free-floating by the host's internal pointer resolution, so no parallel type pair exists on this side of the seam and no live sub-owner leaves the borrow.
- Law: writes are in-place — a bound sub-owner commits through its native pointer, inert `BeginChange`/`EndChange` never appear, and cross-source copy replays one detached total state.
- Law: only the document owns an undo record — archive and detached mutations apply without one; archive persistence occurs at `File3dm.Write`, while detached values remain locally owned, so their receipts carry no serial.
- Boundary: the document and archive accessors are the document and file-IO catalogs' seam; this union names them once and every settings verb enters through it.

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
        return op.Catch(() => Fin.Succ<SettingsSource>(value: new Detached(
            Settings: new Lease<RenderSettings>.Owned(Value: new RenderSettings()))));
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

    internal Fin<SettingsReceipt> Mutate(string name, Func<RenderSettings, Fin<Seq<SettingsAxis>>> borrow, Op key) =>
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
                            from axes in ctx.Borrow(settings)
                            select new SettingsReceipt(Applied: axes, UndoRecord: None),
                        stamp: static (value, serial) => value with { UndoRecord = Some(serial) },
                        op: ctx.Op),
                    key: ctx.Op,
                    needs: SessionNeed.Mutation(undo: true, redraw: RedrawPolicy.None).ToArray())
                select receipt,
            archived: static (ctx, source) =>
                from archive in ctx.Op.Need(source.Archive)
                from receipt in ctx.Op.Catch(() =>
                    from settings in Optional(archive.Settings.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                    from axes in ctx.Borrow(settings)
                    select new SettingsReceipt(Applied: axes, UndoRecord: None))
                select receipt,
            detached: static (ctx, source) =>
                from settings in ctx.Op.Need(source.Settings)
                from axes in ctx.Op.Catch(() => ctx.Borrow(settings.Resource))
                select new SettingsReceipt(Applied: axes, UndoRecord: None));

    public void Dispose() {
        _ = Switch(
            live: static _ => unit,
            archived: static _ => unit,
            detached: static source => source.Settings.Dispose());
    }
}
```

## [03]-[STATE_RECORDS]

- Owner: `SubOwners` is the one custody window over the seven `RenderSettings` sub-owners — `Within` owns the bracket, so every read and every apply the body asks for borrows the same seven wrappers, the state and its evidence sample one instant, and a per-property re-read cannot tear the snapshot.
- Law: a borrow never releases the window — the bracket does. A read-then-write body is therefore ONE window, and a compensated edit reads its prior state and applies its plan against the same wrapper set instead of two unsynchronized opens. Each total-state owner carries one-pass `Of` and whole-state `Apply`; boundary guards reject invalid scalar, vector, key, and case combinations before host mutation. `SunEvidence` owns derived vector, hash, and light custody, while `WorkflowEvidence` owns reciprocal gamma and hash without treating either as replay input.
- Law: a sub-owner property answers a FRESH non-owning wrapper on every read and its `Dispose` is `GC.SuppressFinalize` alone, so custody is a finalizer retirement, never a native release — the seven-wrapper window is what makes the read coherent, and a free-floating sub-owner never enters it because disposing one suppresses its only delete path.
- Law: applies are total state, never a patch — every `Apply` re-asserts its full field set, so an absent field cannot silently clear and a configuration travels as one replayable value between documents, archives, and free-floating carriers.
- Law: sun position follows host mode — automatic state writes geolocation, timezone, daylight saving, and moment before clearing manual control; manual state admits either the host angle pair or vector setter after enabling manual control. Readback canonicalizes manual state to angles, while vector and hash detach as evidence.
- Law: `EnvironmentRole` and `EnvironmentView` close the usage-purpose product; `RenderConfig` writes one binding per role and `RenderState.EnvironmentResolution` reads both purposes without leaking host enums.
- Growth: a new host sub-owner property is one record field read and asserted in the same pass; a new sub-owner is one record with its `Of`/`Apply` pair and one `SettingsEdit` case.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record SubOwners : IDisposable {
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
        from result in Bracketed(owners: owners, borrow: activeBorrow, key: key)
        select result;

    // The window's release belongs to the BRACKET, never to a borrow. `Within` opens the seven wrappers once, runs whatever
    // sequence of borrows the body asks against that one instant, and retires the finalizer registrations on exit — so a
    // read-then-write body is expressible over ONE coherent wrapper set. A borrow disposing its own receiver made a second
    // consecutive borrow inexpressible and forced every read-then-write caller into two unsynchronized windows.
    private static Fin<TOut> Bracketed<TOut>(SubOwners owners, Func<SubOwners, Fin<TOut>> borrow, Op key) {
        try {
            return key.Catch(() => borrow(owners));
        } finally {
            owners.Dispose();
        }
    }

    // Host truth: every sub-owner read off a `RenderSettings` is a NON-OWNING wrapper — the private `Dispose(bool)` body is
    // empty and the public `Dispose` only runs `GC.SuppressFinalize`, so this release retires seven finalizer registrations
    // and frees no native. A genuinely free-floating sub-owner (the parameterless constructor) inverts that: disposing it
    // suppresses the finalizer that is its ONLY `DeleteCpp` path, so such a value never enters this window. One throwing
    // release never aborts the sweep.
    public void Dispose() => held.Iter(static owner => ignore(Try.lift(() => { owner.Dispose(); return unit; }).Run()));
}

public sealed record GroundPlaneState(
    bool Enabled, bool ShadowOnly, bool ShowUnderside, double Altitude, bool AutoAltitude,
    Option<Guid> MaterialInstance, Vector2d TextureOffset, Vector2d TextureSize, double TextureRotation,
    bool TextureOffsetLocked, bool TextureSizeLocked) : IDetachedDocumentResult {
    private bool IsValid => double.IsFinite(Altitude) && TextureOffset.IsValid && TextureSize.IsValid
        && double.IsFinite(TextureRotation)
        && MaterialInstance.Map(static id => id != Guid.Empty).IfNone(true);

    internal static GroundPlaneState Of(GroundPlane ground) =>
        new(
            Enabled: ground.Enabled, ShadowOnly: ground.ShadowOnly, ShowUnderside: ground.ShowUnderside,
            Altitude: ground.Altitude, AutoAltitude: ground.AutoAltitude,
            MaterialInstance: Optional(ground.MaterialInstanceId).Filter(static id => id != Guid.Empty),
            TextureOffset: ground.TextureOffset, TextureSize: ground.TextureSize, TextureRotation: ground.TextureRotation,
            TextureOffsetLocked: ground.TextureOffsetLocked, TextureSizeLocked: ground.TextureSizeLocked);

    internal Fin<Unit> Apply(GroundPlane ground, Op key) {
        GroundPlaneState self = this;
        return from _ in guard(self.IsValid, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => {
            ground.Enabled = self.Enabled;
            ground.ShadowOnly = self.ShadowOnly;
            ground.ShowUnderside = self.ShowUnderside;
            ground.Altitude = self.Altitude;
            ground.AutoAltitude = self.AutoAltitude;
            ground.MaterialInstanceId = self.MaterialInstance.IfNone(Guid.Empty);
            ground.TextureOffset = self.TextureOffset;
            ground.TextureSize = self.TextureSize;
            ground.TextureRotation = self.TextureRotation;
            ground.TextureOffsetLocked = self.TextureOffsetLocked;
            ground.TextureSizeLocked = self.TextureSizeLocked;
            return Fin.Succ(value: unit);
        })
               select applied;
    }
}

public readonly record struct SkylightState(bool Enabled, double ShadowIntensity) : IDetachedDocumentResult {
    internal static SkylightState Of(Skylight sky) => new(Enabled: sky.Enabled, ShadowIntensity: sky.ShadowIntensity);

    internal Fin<Unit> Apply(Skylight sky, Op key) {
        SkylightState self = this;
        return from _ in guard(double.IsFinite(self.ShadowIntensity) && self.ShadowIntensity >= 0d, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => {
            sky.Enabled = self.Enabled;
            sky.ShadowIntensity = self.ShadowIntensity;
            return Fin.Succ(value: unit);
        })
               select applied;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunPosition {
    private SunPosition() { }
    public sealed record Automatic(
        double Latitude, double Longitude, double TimeZone,
        bool DaylightSavingOn, int DaylightSavingMinutes, DateTime Moment) : SunPosition;
    public sealed record ManualAngles(double Azimuth, double Altitude) : SunPosition;
    public sealed record ManualVector(Vector3d Value) : SunPosition;

    internal bool IsValid => Switch(
        automatic: static position =>
            double.IsFinite(position.Latitude) && position.Latitude is >= -90d and <= 90d
            && double.IsFinite(position.Longitude) && position.Longitude is >= -180d and <= 180d
            && double.IsFinite(position.TimeZone) && position.TimeZone is >= -24d and <= 24d
            && position.DaylightSavingMinutes is >= 0 and <= 1440,
        manualAngles: static position =>
            double.IsFinite(position.Azimuth)
            && double.IsFinite(position.Altitude) && position.Altitude is >= -90d and <= 90d,
        // `Vector3d.IsValid` gates finiteness ALONE and admits the zero vector, which the host then reads back as a
        // due-south horizon sun — a plausible angle pair no consumer separates from a measured one. Refusing the
        // zero ray here keeps that reading off the wire; a denormal ray still unitizes and stays admitted.
        manualVector: static position => position.Value.IsValid && !position.Value.IsZero);
}

[SmartEnum<string>]
public sealed partial class SunAccuracy {
    public static readonly SunAccuracy Minimum = new("minimum", global::Rhino.Render.Sun.Accuracies.Minimum);
    public static readonly SunAccuracy Maximum = new("maximum", global::Rhino.Render.Sun.Accuracies.Maximum);

    internal global::Rhino.Render.Sun.Accuracies Native { get; }

    internal static Fin<SunAccuracy> Of(global::Rhino.Render.Sun.Accuracies native, Op key) =>
        key.Row(Items, native, static item => item.Native);
}

public sealed record SunEvidence(Vector3d Vector, uint Hash, Lease<Light> Light)
    : IDisposable, IDetachedDocumentResult {
    internal static Fin<SunEvidence> Of(global::Rhino.Render.Sun sun, Op key) => key.Catch(() =>
        Optional(sun.Light).ToFin(Fail: key.InvalidResult()).Map(light => new SunEvidence(
            Vector: sun.Vector,
            Hash: sun.Hash,
            Light: new Lease<Light>.Owned(Value: light))));

    public void Dispose() => Light.Dispose();
}

[ComplexValueObject]
public sealed partial class SunState : IDetachedDocumentResult {
    public bool Enabled { get; }
    public double Intensity { get; }
    public SunAccuracy Accuracy { get; }
    public double North { get; }
    public SunPosition Position { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref bool enabled,
        ref double intensity,
        ref SunAccuracy accuracy,
        ref double north,
        ref SunPosition position) {
        validationError = double.IsFinite(intensity) && intensity >= 0d
            && accuracy is not null && double.IsFinite(north) && position is { IsValid: true }
            ? validationError
            : new ValidationError(message: "sun state is invalid");
    }

    internal static Fin<SunState> Of(global::Rhino.Render.Sun sun, Op key) =>
        from accuracy in SunAccuracy.Of(sun.Accuracy, key)
        let position = sun.ManualControlOn
                ? (SunPosition)new SunPosition.ManualAngles(Azimuth: sun.Azimuth, Altitude: sun.Altitude)
                : new SunPosition.Automatic(
                    Latitude: sun.Latitude, Longitude: sun.Longitude, TimeZone: sun.TimeZone,
                    DaylightSavingOn: sun.DaylightSavingOn, DaylightSavingMinutes: sun.DaylightSavingMinutes,
                    Moment: sun.GetDateTime(DateTimeKind.Local))
        from state in key.AcceptValidated(Validate(sun.Enabled, sun.Intensity, accuracy, sun.North, position, out SunState? value), value)
        select state;

    internal Fin<Unit> Apply(global::Rhino.Render.Sun sun, Op key) {
        SunState self = this;
        return key.Catch(() => {
            sun.Enabled = self.Enabled;
            sun.Intensity = self.Intensity;
            sun.Accuracy = self.Accuracy.Native;
            sun.North = self.North;
            self.Position.Switch(
                automatic: state => {
                    sun.Latitude = state.Latitude;
                    sun.Longitude = state.Longitude;
                    sun.TimeZone = state.TimeZone;
                    sun.DaylightSavingOn = state.DaylightSavingOn;
                    sun.DaylightSavingMinutes = state.DaylightSavingMinutes;
                    sun.SetDateTime(state.Moment, state.Moment.Kind);
                    sun.ManualControlOn = false;
                },
                manualAngles: state => {
                    sun.ManualControlOn = true;
                    sun.Azimuth = state.Azimuth;
                    sun.Altitude = state.Altitude;
                },
                manualVector: state => {
                    sun.ManualControlOn = true;
                    sun.Vector = state.Value;
                });
            return Fin.Succ(value: unit);
        });
    }
}

[SmartEnum<bool>]
public sealed partial class GammaMode {
    public static readonly GammaMode Off = new(false);
    public static readonly GammaMode On = new(true);

    internal bool Enabled => Key;
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
        validationError = mode is not null && float.IsFinite(gamma) && gamma > 0f
            ? validationError
            : new ValidationError(message: "post-process gamma is invalid");
    }

    internal static Fin<PostGamma> Of(LinearWorkflow workflow, Op key) =>
        key.AcceptValidated(Validate(workflow.PostProcessGammaOn ? GammaMode.On : GammaMode.Off, workflow.PostProcessGamma, out PostGamma? value), value);

    internal void Apply(LinearWorkflow workflow) {
        workflow.PostProcessGamma = Gamma;
        workflow.PostProcessGammaOn = Mode.Enabled;
    }
}

public readonly record struct WorkflowState(
    bool PreProcessColors, bool PreProcessTextures, bool PostProcessFrameBuffer,
    float PreProcessGamma, PostGamma PostGamma) : IDetachedDocumentResult {
    internal static Fin<WorkflowState> Of(LinearWorkflow workflow, Op key) =>
        PostGamma.Of(workflow, key).Map(postGamma => new WorkflowState(
            PreProcessColors: workflow.PreProcessColors, PreProcessTextures: workflow.PreProcessTextures,
            PostProcessFrameBuffer: workflow.PostProcessFrameBuffer,
            PreProcessGamma: workflow.PreProcessGamma,
            PostGamma: postGamma));

    internal Fin<Unit> Apply(LinearWorkflow workflow, Op key) {
        WorkflowState self = this;
        return from _ in guard(
                   float.IsFinite(self.PreProcessGamma) && self.PreProcessGamma > 0f && self.PostGamma is not null,
                   key.InvalidInput()).ToFin()
               from applied in key.Catch(() => {
            workflow.PreProcessColors = self.PreProcessColors;
            workflow.PreProcessTextures = self.PreProcessTextures;
            workflow.PostProcessFrameBuffer = self.PostProcessFrameBuffer;
            workflow.PreProcessGamma = self.PreProcessGamma;
            self.PostGamma.Apply(workflow);
            return Fin.Succ(value: unit);
        })
               select applied;
    }
}

public readonly record struct WorkflowEvidence(float PostGammaReciprocal, uint Hash) : IDetachedDocumentResult {
    internal static WorkflowEvidence Of(LinearWorkflow workflow) =>
        new(PostGammaReciprocal: workflow.PostProcessGammaReciprocal, Hash: workflow.Hash);
}

// `DitherMethod` is the `Rasm.Rhino.Render` namespace's ONE dither vocabulary: the settings sub-owner and the Display render
// window both bind these rows, and a second owner keyed on the native enum beside it is the deleted form. The roster is the
// whole of `Dithering.Methods`.
//
// Host truth: `Dithering.Method` is a TWO-state native variant wearing a three-row enum. The getter answers
// `FloydSteinberg` for any non-zero and `SimpleNoise` otherwise — it never answers `None` — and the setter writes `1` for
// anything but `SimpleNoise`, so writing `None` reads back as `FloydSteinberg`. `None` is therefore an admissible INPUT row
// that does not round-trip, and `Dithering.Enabled` is the real off switch a consumer wanting no dithering writes.
[SmartEnum<string>]
public sealed partial class DitherMethod {
    public static readonly DitherMethod None = new("none", Dithering.Methods.None);
    public static readonly DitherMethod FloydSteinberg = new("floyd-steinberg", Dithering.Methods.FloydSteinberg);
    public static readonly DitherMethod SimpleNoise = new("simple-noise", Dithering.Methods.SimpleNoise);

    internal Dithering.Methods Native { get; }

    internal static Fin<DitherMethod> Of(Dithering.Methods native, Op key) => key.Row(Items, native, static item => item.Native);
}

public readonly record struct DitherState(DitherMethod Method, bool Enabled) : IDetachedDocumentResult {
    internal static Fin<DitherState> Of(Dithering dither, Op key) =>
        DitherMethod.Of(dither.Method, key).Map(method => new DitherState(Method: method, Enabled: dither.Enabled));

    internal Fin<Unit> Apply(Dithering dither, Op key) {
        DitherState self = this;
        return from _ in guard(self.Method is not null, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => {
            dither.Method = self.Method.Native;
            dither.Enabled = self.Enabled;
            return Fin.Succ(value: unit);
        })
               select applied;
    }
}

public sealed record SafeFrameState(
    bool Enabled, bool PerspectiveOnly, bool FieldsOn, bool LiveFrameOn,
    bool ActionFrameOn, bool ActionFrameLinked, double ActionFrameXScale, double ActionFrameYScale,
    bool TitleFrameOn, bool TitleFrameLinked, double TitleFrameXScale, double TitleFrameYScale) : IDetachedDocumentResult {
    private bool IsValid =>
        double.IsFinite(ActionFrameXScale) && ActionFrameXScale >= 0d
        && double.IsFinite(ActionFrameYScale) && ActionFrameYScale >= 0d
        && double.IsFinite(TitleFrameXScale) && TitleFrameXScale >= 0d
        && double.IsFinite(TitleFrameYScale) && TitleFrameYScale >= 0d;

    internal static SafeFrameState Of(SafeFrame frame) =>
        new(
            Enabled: frame.Enabled, PerspectiveOnly: frame.PerspectiveOnly, FieldsOn: frame.FieldsOn, LiveFrameOn: frame.LiveFrameOn,
            ActionFrameOn: frame.ActionFrameOn, ActionFrameLinked: frame.ActionFrameLinked,
            ActionFrameXScale: frame.ActionFrameXScale, ActionFrameYScale: frame.ActionFrameYScale,
            TitleFrameOn: frame.TitleFrameOn, TitleFrameLinked: frame.TitleFrameLinked,
            TitleFrameXScale: frame.TitleFrameXScale, TitleFrameYScale: frame.TitleFrameYScale);

    internal Fin<Unit> Apply(SafeFrame frame, Op key) {
        SafeFrameState self = this;
        return from _ in guard(self.IsValid, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => {
            frame.Enabled = self.Enabled;
            frame.PerspectiveOnly = self.PerspectiveOnly;
            frame.FieldsOn = self.FieldsOn;
            frame.LiveFrameOn = self.LiveFrameOn;
            frame.ActionFrameOn = self.ActionFrameOn;
            frame.ActionFrameXScale = self.ActionFrameXScale;
            frame.ActionFrameYScale = self.ActionFrameYScale;
            frame.ActionFrameLinked = self.ActionFrameLinked;
            frame.TitleFrameOn = self.TitleFrameOn;
            frame.TitleFrameXScale = self.TitleFrameXScale;
            frame.TitleFrameYScale = self.TitleFrameYScale;
            frame.TitleFrameLinked = self.TitleFrameLinked;
            return Fin.Succ(value: unit);
        })
               select applied;
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelState : IDetachedDocumentResult {
    private ChannelState() { }
    public sealed record Automatic : ChannelState;
    public sealed record Custom(Seq<Guid> Values) : ChannelState;

    private bool IsValid => Switch(
        automatic: static _ => true,
        custom: static value => !value.Values.IsEmpty
            && value.Values.ForAll(static id => id != Guid.Empty)
            && value.Values.Distinct().Count == value.Values.Count);

    internal static Fin<ChannelState> Of(RenderChannels channels, Op key) => channels.Mode switch {
        RenderChannels.Modes.Automatic => Fin.Succ<ChannelState>(new Automatic()),
        RenderChannels.Modes.Custom => new Custom(toSeq(channels.CustomList)) is { IsValid: true } state
            ? Fin.Succ<ChannelState>(state)
            : Fin.Fail<ChannelState>(key.InvalidResult()),
        _ => Fin.Fail<ChannelState>(key.InvalidResult()),
    };

    internal Fin<Unit> Apply(RenderChannels channels, Op key) {
        ChannelState self = this;
        return from _ in guard(self.IsValid, key.InvalidInput()).ToFin()
               from applied in key.Catch(() => self.Switch(
            context: channels,
            automatic: static (state, _) => {
                state.CustomList = [];
                state.Mode = RenderChannels.Modes.Automatic;
                return Fin.Succ(unit);
            },
            custom: static (state, value) => {
                state.CustomList = value.Values.Distinct().ToArray();
                state.Mode = RenderChannels.Modes.Custom;
                return Fin.Succ(unit);
            }))
               select applied;
    }
}

[SmartEnum<string>]
public sealed partial class EnvironmentRole {
    public static readonly EnvironmentRole Background = new("background", RenderSettings.EnvironmentUsage.Background);
    public static readonly EnvironmentRole Reflection = new("reflection", RenderSettings.EnvironmentUsage.Reflection);
    public static readonly EnvironmentRole Skylighting = new("skylighting", RenderSettings.EnvironmentUsage.Skylighting);

    internal RenderSettings.EnvironmentUsage Native { get; }
    internal static Seq<EnvironmentRole> All => [Background, Reflection, Skylighting];

    internal static Fin<EnvironmentRole> Of(RenderSettings.EnvironmentUsage native, Op key) =>
        key.Row(Items, native, static item => item.Native);
}

[SmartEnum<string>]
public sealed partial class EnvironmentView {
    public static readonly EnvironmentView Standard = new("standard", RenderSettings.EnvironmentPurpose.Standard);
    public static readonly EnvironmentView Rendering = new("rendering", RenderSettings.EnvironmentPurpose.ForRendering);

    internal RenderSettings.EnvironmentPurpose Native { get; }
    internal static Seq<EnvironmentView> All => [Standard, Rendering];
}

public readonly record struct EnvironmentBinding(Option<Guid> Content, bool Override);

public sealed record EnvironmentBindingState {
    private readonly Seq<(EnvironmentRole Role, EnvironmentBinding Binding)> rows;

    private EnvironmentBindingState(Seq<(EnvironmentRole Role, EnvironmentBinding Binding)> rows) => this.rows = rows;

    public Seq<(EnvironmentRole Role, EnvironmentBinding Binding)> Rows => rows;

    public static Fin<EnvironmentBindingState> Of(
        params ReadOnlySpan<(EnvironmentRole Role, EnvironmentBinding Binding)> rows) {
        Op op = Op.Of(name: nameof(EnvironmentBindingState));
        Seq<(EnvironmentRole Role, EnvironmentBinding Binding)> admitted = toSeq(rows.ToArray());
        return guard(
                admitted.Count == EnvironmentRole.All.Count
                && EnvironmentRole.All.ForAll(role => admitted.Count(row => row.Role == role) == 1)
                && admitted.ForAll(row => row.Binding.Content.Map(static id => id != Guid.Empty).IfNone(true)),
                op.InvalidInput())
            .ToFin()
            .Map(_ => new EnvironmentBindingState(rows: admitted));
    }

    internal static EnvironmentBindingState Of(RenderSettings settings) =>
        new(rows: EnvironmentRole.All.Map(role => (
            role,
            new EnvironmentBinding(
                Content: Optional(settings.RenderEnvironmentId(role.Native, EnvironmentView.Standard.Native))
                    .Filter(static id => id != Guid.Empty),
                Override: settings.RenderEnvironmentOverride(role.Native)))));

    internal Fin<Seq<(EnvironmentRole Role, EnvironmentView View, Option<Guid> Content)>> Resolve(
        RenderSettings settings,
        Op key) => key.Catch(() => Fin.Succ(EnvironmentRole.All.Bind(role => EnvironmentView.All.Map(view => (
            Role: role,
            View: view,
            Content: Optional(settings.RenderEnvironmentId(role.Native, view.Native))
                .Filter(static id => id != Guid.Empty))))));

    internal Fin<Unit> Apply(RenderSettings settings, Op key) {
        EnvironmentBindingState self = this;
        return key.Catch(() => {
            self.rows.Iter(row => {
                settings.SetRenderEnvironmentId(row.Role.Native, row.Binding.Content.IfNone(Guid.Empty));
                settings.SetRenderEnvironmentOverride(row.Role.Native, row.Binding.Override);
            });
            return Fin.Succ(value: unit);
        });
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderSource {
    private RenderSource() { }
    public sealed record ActiveViewport : RenderSource;
    public sealed record SpecificViewport(string Name) : RenderSource;
    public sealed record NamedView(string Name) : RenderSource;
    public sealed record Snapshot(string Name) : RenderSource;

    internal bool IsValid => Switch(
        activeViewport: static _ => true,
        specificViewport: static source => !string.IsNullOrWhiteSpace(source.Name),
        namedView: static source => !string.IsNullOrWhiteSpace(source.Name),
        snapshot: static source => !string.IsNullOrWhiteSpace(source.Name));

    internal static Fin<RenderSource> Of(RenderSettings settings, Op key) => settings.RenderSource switch {
        RenderSettings.RenderingSources.ActiveViewport => Fin.Succ<RenderSource>(new ActiveViewport()),
        RenderSettings.RenderingSources.SpecificViewport => Required(
            settings.SpecificViewport, key, static name => new SpecificViewport(name)),
        RenderSettings.RenderingSources.NamedView => Required(settings.NamedView, key, static name => new NamedView(name)),
        RenderSettings.RenderingSources.SnapShot => Required(settings.Snapshot, key, static name => new Snapshot(name)),
        _ => Fin.Fail<RenderSource>(key.InvalidResult()),
    };

    internal void Apply(RenderSettings settings) => Switch(
        context: settings,
        activeViewport: static (state, _) => state.RenderSource = RenderSettings.RenderingSources.ActiveViewport,
        specificViewport: static (state, source) => {
            state.SpecificViewport = source.Name;
            state.RenderSource = RenderSettings.RenderingSources.SpecificViewport;
        },
        namedView: static (state, source) => {
            state.NamedView = source.Name;
            state.RenderSource = RenderSettings.RenderingSources.NamedView;
        },
        snapshot: static (state, source) => {
            state.Snapshot = source.Name;
            state.RenderSource = RenderSettings.RenderingSources.SnapShot;
        });

    private static Fin<RenderSource> Required<T>(string value, Op key, Func<string, T> project) where T : RenderSource =>
        Optional(value).Filter(static text => !string.IsNullOrWhiteSpace(text))
            .ToFin(Fail: key.InvalidResult())
            .Map(text => (RenderSource)project(text));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderOutput {
    private RenderOutput() { }
    private sealed record ViewportCase(bool ScaleBackgroundToFit) : RenderOutput;
    private sealed record FixedCase(Size2i Size, double Dpi, ModelUnit Units, bool ScaleBackgroundToFit) : RenderOutput;

    public static RenderOutput Viewport(bool scaleBackgroundToFit) =>
        new ViewportCase(ScaleBackgroundToFit: scaleBackgroundToFit);

    public static Fin<RenderOutput> Fixed(
        Size2i size,
        double dpi,
        ModelUnit units,
        bool scaleBackgroundToFit,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admittedSize in Size2i.Of(width: size.Width, height: size.Height, key: op)
               from admittedDpi in op.Positive(value: dpi)
               from admittedUnits in op.Need(units)
               select (RenderOutput)new FixedCase(
                   Size: admittedSize,
                   Dpi: admittedDpi,
                   Units: admittedUnits,
                   ScaleBackgroundToFit: scaleBackgroundToFit);
    }

    internal Fin<RenderOutput> Admit(Op key) => Switch(
        context: key,
        viewportCase: static (_, output) => Fin.Succ<RenderOutput>(output),
        fixedCase: static (op, output) => Fixed(
            size: output.Size,
            dpi: output.Dpi,
            units: output.Units,
            scaleBackgroundToFit: output.ScaleBackgroundToFit,
            key: op));

    internal static Fin<RenderOutput> Of(RenderSettings settings, Op key) => settings.UseViewportSize
        ? Fin.Succ(Viewport(scaleBackgroundToFit: settings.ScaleBackgroundToFit))
        : from size in Size2i.Of(width: settings.ImageSize.Width, height: settings.ImageSize.Height, key: key)
          from units in ModelUnit.Of(value: settings.ImageUnitSystem, key: key)
          from output in Fixed(
              size: size,
              dpi: settings.ImageDpi,
              units: units,
              scaleBackgroundToFit: settings.ScaleBackgroundToFit,
              key: key)
          select output;

    internal void Apply(RenderSettings settings) => Switch(
        context: settings,
        viewportCase: static (state, output) => {
            state.UseViewportSize = true;
            state.ScaleBackgroundToFit = output.ScaleBackgroundToFit;
        },
        fixedCase: static (state, output) => {
            state.UseViewportSize = false;
            state.ImageSize = output.Size.Native;
            state.ImageDpi = output.Dpi;
            state.ImageUnitSystem = output.Units.System;
            state.ScaleBackgroundToFit = output.ScaleBackgroundToFit;
        });
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

public sealed record RenderConfig(
    PerceptualColor Ambient,
    PerceptualColor BackgroundTop,
    PerceptualColor BackgroundBottom,
    BackgroundMode BackgroundStyle,
    bool TransparentBackground,
    AntialiasPolicy Antialias,
    int ShadowmapLevel,
    bool RenderBackfaces,
    bool RenderCurves,
    bool RenderPoints,
    bool RenderMeshEdges,
    bool RenderAnnotations,
    bool RenderIsoparams,
    bool UseHiddenLights,
    bool DepthCue,
    bool FlatShade,
    RenderOutput Output,
    RenderSource Source,
    EnvironmentBindingState Environments) : IDetachedDocumentResult {

    internal static Fin<RenderConfig> Of(RenderSettings settings, Op key) =>
        key.Catch(() =>
            from ambient in PerceptualColor.OfRgb(settings.AmbientLight.R, settings.AmbientLight.G, settings.AmbientLight.B, settings.AmbientLight.A)
            from top in PerceptualColor.OfRgb(settings.BackgroundColorTop.R, settings.BackgroundColorTop.G, settings.BackgroundColorTop.B, settings.BackgroundColorTop.A)
            from bottom in PerceptualColor.OfRgb(settings.BackgroundColorBottom.R, settings.BackgroundColorBottom.G, settings.BackgroundColorBottom.B, settings.BackgroundColorBottom.A)
            from background in BackgroundMode.Of(settings.BackgroundStyle, key)
            from antialias in AntialiasPolicy.Of(settings.AntialiasLevel, key)
            from output in RenderOutput.Of(settings, key)
            from source in RenderSource.Of(settings, key)
            select new RenderConfig(
                Ambient: ambient, BackgroundTop: top, BackgroundBottom: bottom,
                BackgroundStyle: background, TransparentBackground: settings.TransparentBackground,
                Antialias: antialias, ShadowmapLevel: settings.ShadowmapLevel,
                RenderBackfaces: settings.RenderBackfaces, RenderCurves: settings.RenderCurves, RenderPoints: settings.RenderPoints,
                RenderMeshEdges: settings.RenderMeshEdges, RenderAnnotations: settings.RenderAnnotations, RenderIsoparams: settings.RenderIsoparams,
                UseHiddenLights: settings.UseHiddenLights, DepthCue: settings.DepthCue, FlatShade: settings.FlatShade,
                Output: output,
                Source: source,
                Environments: EnvironmentBindingState.Of(settings: settings)));

    internal Fin<Unit> Apply(RenderSettings settings, Op key) {
        RenderConfig self = this;
        return from output in key.Need(self.Output).Bind(value => value.Admit(key))
               from _ in guard(
                   self.Source is { IsValid: true }
                   && self.Environments is not null
                   && self.ShadowmapLevel >= 0,
                   key.InvalidInput()).ToFin()
               from applied in key.Catch(() => {
            settings.AmbientLight = self.Ambient.Quantized();
            settings.BackgroundColorTop = self.BackgroundTop.Quantized();
            settings.BackgroundColorBottom = self.BackgroundBottom.Quantized();
            settings.BackgroundStyle = self.BackgroundStyle.Native;
            settings.TransparentBackground = self.TransparentBackground;
            settings.AntialiasLevel = self.Antialias.Native;
            settings.ShadowmapLevel = self.ShadowmapLevel;
            settings.RenderBackfaces = self.RenderBackfaces;
            settings.RenderCurves = self.RenderCurves;
            settings.RenderPoints = self.RenderPoints;
            settings.RenderMeshEdges = self.RenderMeshEdges;
            settings.RenderAnnotations = self.RenderAnnotations;
            settings.RenderIsoparams = self.RenderIsoparams;
            settings.UseHiddenLights = self.UseHiddenLights;
            settings.DepthCue = self.DepthCue;
            settings.FlatShade = self.FlatShade;
            output.Apply(settings);
            self.Source.Apply(settings);
            return self.Environments.Apply(settings: settings, key: key);
        })
               select applied;
    }
}
```

## [04]-[SUN_ASTRONOMY]

- Owner: `SunProblem` closes direction, altitude, Julian day, twilight, tint, ephemeris, and machine-location modalities; `SunCapability` is the grant a machine-facts read presents; `SunSolution` closes vector, scalar, color, angle, and optional location egress; `SunSolver.Solve` is the sole entry. `SolarFrame`, `SunDerivation`, and `SceneSun` are the daylighting descriptor's sun band, projected out of `SunState` and never read back in.
- Law: each host problem dispatches directly to its verified host static, and provider failure or invalid admission stays on the `Fin<SunSolution>` rail; `Ephemeris` alone carries no `Catch`, since the kernel almanac is a total effect-free fold over an admitted `SolarSite`.
- Law: every problem but `Here` is pure over its supplied arguments — `Here` reads the machine's own geolocation service, so it carries the `SunCapability.MachineLocation` grant and admission refuses the case without it; a machine-facts read reached implicitly through a coordinate solve is the deleted form.
- Law: host astronomy and wire astronomy answer two questions and stay two-formed — `Sun.SunDirection`/`AltitudeFromValues` report what the HOST believes and drive host-facing reads, while `Ephemeris` composes `Rasm.Numerics.SolarPosition.At` and is the only derivation a peer reproduces; a descriptor angle taken off a host static publishes an almanac no peer holds.
- Law: `SceneSun` narrows the georeference to what an annual engine run admits — time zone `[-12, 14]` hours and elevation `[-300, 8900)` metres — so a document outside those bounds refuses at the producer instead of writing a site an engine rejects, and `SolarSite`'s own wider gate stays the kernel's.
- Law: `Sited` and `Authored` are the whole discriminant — a manually controlled sun has no site derivation, so it carries angles alone and an annual run refuses it by name rather than back-solving coordinates from two numbers.
- Law: `Sun.Vector` points sun-toward-scene in the document world frame and `Sun.North` bears compass north counter-clockwise off `+X` — `90`, the host default, seating north on `+Y` — so `ManualVector` negates, unitizes, projects onto that bearing's east and north axes in the host `Vector3d` the almanac takes, and re-reads through the kernel `SunPosition.OfDirection`; `Authored` therefore carries the same east-of-North pair `Sited` does, the host frame stops at this projection, and a ray that cannot unitize refuses instead of crossing as the due-south horizon reading the host substitutes for it.
- Boundary: the georeference invariant — `Sun.North`/`Latitude`/`Longitude` re-encoded from `EarthAnchorPoint` after an anchor write — is the Exchange rail's earth-sync owner; this page never writes the anchor, `Here` only reads the machine, and `elevationMetres` arrives as the caller's `EarthAnchorPoint.EarthBasepointElevation` read.
- Boundary: sky irradiance is the consuming weather owner's — `SunState.Intensity` is a dimensionless render multiplier, so this band carries no `W/m2` column and a manufactured one fabricates radiation the document never held.

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
    public sealed record Ephemeris(SolarSite Site, Instant Moment) : SunProblem;
    public sealed record Here(SunCapability Grant) : SunProblem;

    internal bool IsValid => Switch(
        direction: static problem => Coordinate(problem.Latitude, problem.Longitude),
        altitude: static problem => Coordinate(problem.Latitude, problem.Longitude)
            && Time(problem.TimeZoneHours, problem.DaylightMinutes, problem.Hours)
            && problem.Mode is not null,
        julian: static problem => Time(problem.TimeZoneHours, problem.DaylightMinutes, problem.Hours),
        twilight: static _ => true,
        color: static problem => double.IsFinite(problem.AltitudeDegrees),
        ephemeris: static problem => problem.Site is not null,
        here: static problem => problem.Grant == SunCapability.MachineLocation);

    private static bool Coordinate(double latitude, double longitude) =>
        double.IsFinite(latitude) && latitude is >= -90d and <= 90d
        && double.IsFinite(longitude) && longitude is >= -180d and <= 180d;

    private static bool Time(double zone, int daylight, double hours) =>
        double.IsFinite(zone) && zone is >= -24d and <= 24d
        && daylight is >= 0 and <= 1440 && double.IsFinite(hours);
}

// `Sun.Here(out double, out double)` reads the MACHINE's geolocation service — where the running computer is — not the
// document, not the earth anchor, and not the astronomy model every other problem evaluates over supplied coordinates. That
// is a host-facts capability rather than a solve input, so it enters only as the grant a caller names, and an implicit
// machine read inside an otherwise-pure solve is the deleted form.
[SmartEnum<string>]
public sealed partial class SunCapability {
    public static readonly SunCapability MachineLocation = new("machine-location");
}

[SmartEnum<bool>]
public sealed partial class SolarSolveMode {
    public static readonly SolarSolveMode Precise = new(false);
    public static readonly SolarSolveMode Fast = new(true);

    internal bool FastPath => Key;
}

// `Rasm.Numerics.SunPosition` spells in full on every mention: `Rasm.Numerics` is in the page prelude and this
// namespace declares its own `SunPosition` union, so the bare name binds to the host-state carrier and the kernel
// almanac's angle pair would silently resolve to a different concept.
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunSolution : IDetachedDocumentResult {
    private SunSolution() { }
    public sealed record Vector(Vector3d Value) : SunSolution;
    public sealed record Scalar(double Value) : SunSolution;
    public sealed record Color(PerceptualColor Value) : SunSolution;
    public sealed record Angles(Rasm.Numerics.SunPosition Value) : SunSolution;
    public sealed record Location(Option<(double Latitude, double Longitude)> Value) : SunSolution;

    internal Fin<Rasm.Numerics.SunPosition> Angular(Op key) =>
        Switch(
            context: key,
            vector: static (op, _) => Fin.Fail<Rasm.Numerics.SunPosition>(error: op.InvalidResult()),
            scalar: static (op, _) => Fin.Fail<Rasm.Numerics.SunPosition>(error: op.InvalidResult()),
            color: static (op, _) => Fin.Fail<Rasm.Numerics.SunPosition>(error: op.InvalidResult()),
            angles: static (_, solution) => Fin.Succ(value: solution.Value),
            location: static (op, _) => Fin.Fail<Rasm.Numerics.SunPosition>(error: op.InvalidResult()));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunDerivation {
    private SunDerivation() { }
    public sealed record Sited(SolarFrame Frame, Rasm.Numerics.SunPosition Angles) : SunDerivation;
    public sealed record Authored(Rasm.Numerics.SunPosition Angles) : SunDerivation;
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
        validationError = site is not null
            && site.TimezoneHours is >= -12d and <= 14d
            && site.ElevationM is >= -300d and < 8900d
            && double.IsFinite(northAxisDegrees)
            && daylightSavingMinutes is >= 0 and <= 1440
            ? validationError
            : new ValidationError(message: "<solar-frame-outside-engine-bounds>");
    }
}

public sealed record SceneSun(SunDerivation Derivation, bool Enabled, double IntensityScale)
    : IDetachedDocumentResult {
    internal static Fin<SceneSun> Of(SunState state, double elevationMetres, Op key) =>
        from active in key.Need(state)
        from derivation in Derive(state: active, elevationMetres: elevationMetres, key: key)
        select new SceneSun(Derivation: derivation, Enabled: active.Enabled, IntensityScale: active.Intensity);

    private static Fin<SunDerivation> Derive(SunState state, double elevationMetres, Op key) =>
        state.Position.Switch(
            context: (State: state, Elevation: elevationMetres, Op: key),
            automatic: static (context, position) =>
                from site in context.Op.AcceptValidated(SolarSite.Validate(
                    latitudeDeg: position.Latitude,
                    longitudeDeg: position.Longitude,
                    timezoneHours: position.TimeZone,
                    elevationM: context.Elevation,
                    out SolarSite? admitted), admitted)
                from frame in context.Op.AcceptValidated(SolarFrame.Validate(
                    site: site,
                    northAxisDegrees: context.State.North,
                    daylightSavingMinutes: Saving(position),
                    moment: Utc(position),
                    out SolarFrame? framed), framed)
                from solution in SunSolver.Solve(
                    problem: new SunProblem.Ephemeris(Site: site, Moment: frame.Moment), key: context.Op)
                from angles in solution.Angular(key: context.Op)
                select (SunDerivation)new SunDerivation.Sited(Frame: frame, Angles: angles),
            manualAngles: static (context, position) => Fin.Succ<SunDerivation>(value: new SunDerivation.Authored(
                Angles: new Rasm.Numerics.SunPosition(
                    AzimuthDeg: position.Azimuth, AltitudeDeg: position.Altitude))),
            manualVector: static (context, position) =>
                Surveyed(hostVector: position.Value, northDegrees: context.State.North)
                    .Bind(Rasm.Numerics.SunPosition.OfDirection)
                    .Map(static angles => (SunDerivation)new SunDerivation.Authored(Angles: angles))
                    .ToFin(Fail: context.Op.InvalidInput()));

    // `Sun.Vector` points sun-TOWARD-scene — the direction light travels — so the scene-toward-sun ray the survey
    // frame speaks is its negation. `Sun.North` carries the document's compass north as a counter-clockwise angle
    // off `+X`, `90` (the host default) seating north on `+Y` and making the world frame the survey frame outright,
    // so the turn that derotates a document is the bearing's OFFSET from that default. Taking the offset rather
    // than the bearing keeps the default exact — a rotation built on `cos(90°)` instead carries its round-off into
    // every reading and lands a due-north sun a few ulps BELOW `360`, in the last compass bucket rather than the
    // first. Absence answers a ray that cannot unitize, which the host collapses to a due-south horizon reading.
    // Projection closes in the host coordinate the almanac itself speaks, so the bearing keeps its whole tail into
    // `OfDirection` rather than rounding through a single-precision hop no reader downstream can recover.
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
    private static Instant Utc(SunPosition.Automatic position) =>
        Instant.FromDateTimeUtc(DateTime.SpecifyKind(
            position.Moment - TimeSpan.FromHours(position.TimeZone) - TimeSpan.FromMinutes(Saving(position)),
            DateTimeKind.Utc));

    private static int Saving(SunPosition.Automatic position) =>
        position.DaylightSavingOn ? position.DaylightSavingMinutes : 0;
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
                    fast: query.Mode.FastPath)))),
            julian: static (state, query) => state.Catch(() => Fin.Succ<SunSolution>(new SunSolution.Scalar(
                global::Rhino.Render.Sun.JulianDay(
                    timezoneHours: query.TimeZoneHours,
                    daylightMinutes: query.DaylightMinutes,
                    when: query.Moment,
                    hours: query.Hours)))),
            twilight: static (state, _) => state.Catch(() => Fin.Succ<SunSolution>(
                new SunSolution.Scalar(global::Rhino.Render.Sun.TwilightZone()))),
            color: static (state, query) => state.Catch(() => {
                System.Drawing.Color tint = global::Rhino.Render.Sun.ColorFromAltitude(query.AltitudeDegrees);
                return PerceptualColor.OfRgb(tint.R, tint.G, tint.B, tint.A, state)
                    .Map(static value => (SunSolution)new SunSolution.Color(value));
            }),
            // Kernel almanac, not a host static: `SolarPosition.At` is total and effect-free over an admitted site,
            // so this arm carries no `Catch` and every peer reproducing the pair reads one derivation.
            ephemeris: static (_, query) => Fin.Succ<SunSolution>(value: new SunSolution.Angles(
                Rasm.Numerics.SolarPosition.At(site: query.Site, instant: query.Moment))),
            here: static (state, _) => state.Catch(() => Fin.Succ<SunSolution>(new SunSolution.Location(
                global::Rhino.Render.Sun.Here(out double latitude, out double longitude)
                    ? Some((latitude, longitude))
                    : Option<(double, double)>.None))))
               select solution;
    }
}
```

## [05]-[EDIT_RAIL]

- Owner: `SettingsRequest` closes read, edit, and copy; `SettingsResult` keeps state and receipt egress explicit; `Settings.Run` is the sole entry over every `SettingsSource` origin.
- Law: each request enters its source once; edit and whole-state replay lower through one `SettingsEdit` program inside one compensated mutation grant over a single `SubOwners` window, and copy crosses sources as exactly one source read-window plus one target write-window — the total-state record IS the replayable carrier, so no duplicate aggregate is minted and no live aggregate outlives its window.
- Law: a failed edit sequence restores the pre-borrow total state before the fault leaves — the prior `RenderState` is the compensation record for every source, archive and detached included, with the live bracket's undo rollback layered above it; a restore failure appends onto the primary fault, never replaces it.
- Law: `SettingsReceipt.Applied` names changed axes, and live mutations stamp the same receipt through `UndoBracket`.
- Boundary: `RenderSettings.PostEffects : PostEffectCollection` is a separate host sub-owner whose configuration rows belong to the Display render page.
- Growth: a new configuration axis is one state-record field; a new sub-owner is one record, one `RenderState` field, and one `SettingsEdit` case with every consumer untouched.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SettingsAxis {
    public static readonly SettingsAxis Frame = new("frame");
    public static readonly SettingsAxis Ground = new("ground");
    public static readonly SettingsAxis Sky = new("sky");
    public static readonly SettingsAxis Daylight = new("daylight");
    public static readonly SettingsAxis Workflow = new("workflow");
    public static readonly SettingsAxis Dither = new("dither");
    public static readonly SettingsAxis Guides = new("guides");
    public static readonly SettingsAxis Channels = new("channels");
}

public sealed record SettingsReceipt(Seq<SettingsAxis> Applied, Option<uint> UndoRecord) : IDetachedDocumentResult;

public sealed record RenderState(
    RenderConfig Config,
    GroundPlaneState Ground,
    SkylightState Sky,
    SunState Daylight,
    SunEvidence DaylightEvidence,
    WorkflowState Workflow,
    WorkflowEvidence WorkflowEvidence,
    DitherState Dither,
    SafeFrameState SafeFrame,
    ChannelState Channels,
    Seq<(EnvironmentRole Role, EnvironmentView View, Option<Guid> Content)> EnvironmentResolution)
    : IDisposable, IDetachedDocumentResult {
    internal Fin<T> Use<T>(Func<RenderState, Fin<T>> borrow) where T : IDetachedDocumentResult {
        using (this) return borrow(this);
    }

    internal Seq<SettingsEdit> Replay() => Seq<SettingsEdit>(
        new SettingsEdit.Frame(Config),
        new SettingsEdit.Ground(Ground),
        new SettingsEdit.Sky(Sky),
        new SettingsEdit.Daylight(Daylight),
        new SettingsEdit.Workflow(Workflow),
        new SettingsEdit.Dither(Dither),
        new SettingsEdit.Guides(SafeFrame),
        new SettingsEdit.Channels(Channels));

    public void Dispose() => DaylightEvidence.Dispose();
}

// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsEdit {
    private SettingsEdit(SettingsAxis axis) => Axis = axis;

    internal SettingsAxis Axis { get; }

    public sealed record Frame(RenderConfig Config) : SettingsEdit(SettingsAxis.Frame);
    public sealed record Ground(GroundPlaneState State) : SettingsEdit(SettingsAxis.Ground);
    public sealed record Sky(SkylightState State) : SettingsEdit(SettingsAxis.Sky);
    public sealed record Daylight(SunState State) : SettingsEdit(SettingsAxis.Daylight);
    public sealed record Workflow(WorkflowState State) : SettingsEdit(SettingsAxis.Workflow);
    public sealed record Dither(DitherState State) : SettingsEdit(SettingsAxis.Dither);
    public sealed record Guides(SafeFrameState State) : SettingsEdit(SettingsAxis.Guides);
    public sealed record Channels(ChannelState State) : SettingsEdit(SettingsAxis.Channels);

    internal Fin<Unit> Apply(SubOwners owners, Op op) =>
        Switch(
            (Owners: owners, Op: op),
            frame: static (context, edit) => context.Op.Need(edit.Config)
                .Bind(config => config.Apply(settings: context.Owners.Settings, key: context.Op)),
            ground: static (context, edit) => context.Op.Need(edit.State)
                .Bind(state => state.Apply(ground: context.Owners.Ground, key: context.Op)),
            sky: static (context, edit) => edit.State.Apply(sky: context.Owners.Sky, key: context.Op),
            daylight: static (context, edit) => context.Op.Need(edit.State)
                .Bind(state => state.Apply(sun: context.Owners.Daylight, key: context.Op)),
            workflow: static (context, edit) => edit.State.Apply(workflow: context.Owners.Workflow, key: context.Op),
            dither: static (context, edit) => edit.State.Apply(dither: context.Owners.Dither, key: context.Op),
            guides: static (context, edit) => context.Op.Need(edit.State)
                .Bind(state => state.Apply(frame: context.Owners.Guides, key: context.Op)),
            channels: static (context, edit) => context.Op.Need(edit.State)
                .Bind(state => state.Apply(channels: context.Owners.Channels, key: context.Op)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsRequest {
    private SettingsRequest() { }
    public sealed record Read : SettingsRequest;
    public sealed record Edit(Seq<SettingsEdit> Changes) : SettingsRequest;
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
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
                           borrow: owners => ReadState(owners, state.Op)
                               .Map(static value => (SettingsResult)new SettingsResult.State(value)),
                           key: state.Op),
                       key: state.Op),
                   edit: static (state, command) => Commit(state.Source, command.Changes, state.Op)
                       .Map(static receipt => (SettingsResult)new SettingsResult.Changed(receipt)),
                   copyTo: static (state, command) => Copy(state.Source, command.Target, state.Op)
                       .Map(static receipt => (SettingsResult)new SettingsResult.Changed(receipt)))
               select result;
    }

    // `Sun` and `LinearWorkflow` each answer a FRESH wrapper per property read, so the state and its evidence read one
    // borrowed instance apiece — two reads of one sub-owner are two unsynchronized samples of live host state.
    private static Fin<RenderState> ReadState(SubOwners owners, Op op) =>
        from config in RenderConfig.Of(settings: owners.Settings, key: op)
        from daylight in SunState.Of(sun: owners.Daylight, key: op)
        from workflow in WorkflowState.Of(workflow: owners.Workflow, key: op)
        from dither in DitherState.Of(dither: owners.Dither, key: op)
        from channels in ChannelState.Of(channels: owners.Channels, key: op)
        from environments in config.Environments.Resolve(settings: owners.Settings, key: op)
        from evidence in SunEvidence.Of(sun: owners.Daylight, key: op)
        select new RenderState(
            Config: config,
            Ground: GroundPlaneState.Of(ground: owners.Ground),
            Sky: SkylightState.Of(sky: owners.Sky),
            Daylight: daylight,
            DaylightEvidence: evidence,
            Workflow: workflow,
            WorkflowEvidence: WorkflowEvidence.Of(workflow: owners.Workflow),
            Dither: dither,
            SafeFrame: SafeFrameState.Of(frame: owners.Guides),
            Channels: channels,
            EnvironmentResolution: environments);

    private static Fin<SettingsReceipt> Commit(SettingsSource source, Seq<SettingsEdit> plan, Op op) {
        return from _ in guard(!plan.IsEmpty && plan.ForAll(static edit => edit is not null), op.InvalidInput())
               from receipt in source.Mutate(
                   name: nameof(SettingsRequest.Edit),
                   borrow: settings => SubOwners.Within(
                       settings: settings,
                       borrow: owners => ReadState(owners, op).Bind(prior => Compensated(owners, prior, plan, op)),
                       key: op),
                   key: op)
               select receipt;
    }

    private static Fin<Seq<SettingsAxis>> Compensated(SubOwners owners, RenderState prior, Seq<SettingsEdit> plan, Op op) {
        using (prior) {
            return ApplyPlan(owners: owners, plan: plan, op: op)
                .BindFail(fault => ApplyPlan(owners: owners, plan: prior.Replay(), op: op).Match(
                    Succ: static _ => Fin.Fail<Seq<SettingsAxis>>(error: fault),
                    Fail: restore => Fin.Fail<Seq<SettingsAxis>>(error: fault + restore)));
        }
    }

    // `RenderState` IS the detached replayable carrier, so the source borrow yields it directly; a `Duplicate()` lease would
    // mint a second native, re-read the same total state off it, and carry a live aggregate the detached marker cannot type.
    // Two sub-owner windows total and no more: ONE read window over the source, ONE write window over the target whose prior
    // read and whose apply are two borrows of the same seven wrappers, so the compensation record and the state it restores
    // sample one instant. `RenderState.Use` between them is the detached value's own disposal bracket, not a third window.
    private static Fin<SettingsReceipt> Copy(SettingsSource source, SettingsSource target, Op op) =>
        from activeTarget in op.Need(target)
        from state in source.Use(
            borrow: settings => SubOwners.Within(
                settings: settings, borrow: owners => ReadState(owners, op), key: op),
            key: op)
        from receipt in state.Use(value => activeTarget.Mutate(
            name: nameof(SettingsRequest.CopyTo),
            borrow: settings => SubOwners.Within(
                settings: settings,
                borrow: owners => ReadState(owners, op)
                    .Bind(prior => Compensated(owners: owners, prior: prior, plan: value.Replay(), op: op)),
                key: op),
            key: op))
        select receipt;

    private static Fin<Seq<SettingsAxis>> ApplyPlan(SubOwners owners, Seq<SettingsEdit> plan, Op op) =>
        plan.TraverseM(edit => edit.Apply(owners: owners, op: op)).As()
            .Map(_ => plan.Map(static edit => edit.Axis).Distinct());
}
```

## [06]-[AMBIENT_WATCH]

- Owner: `AmbientSlot` `[SmartEnum<int>]` carries each catalogued static `Changed` broadcast as one bind row. `AmbientFact` detaches the slot, optional document key, and host property context. `AmbientWatch` owns transactional attach and symmetric release through the document `Subscription` capsule.
- Law: `LinearWorkflow` and `Dithering` carry no `Changed` event, so their staleness is polled through `Settings.Run(SettingsRequest.Read)`; callback failure appends one `AmbientFailure` through the shared `RetentionPolicy` ledger, and overflow retains count-and-fault evidence.
- Law: `RenderPropertyChangedEvent.Document`, `Context`, `DocKey` projection, sink delivery, and failure retention share one guarded callback rail. `Context` remains the host's opaque integer discriminant, a missing document yields `None`, and projection failure retains a slot-keyed fallback fact.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
public readonly record struct AmbientFact(AmbientSlot Slot, Option<DocKey> Key, int Context) : IDetachedDocumentResult;
public sealed record AmbientFailure(AmbientFact Fact, Error Fault) : IDetachedDocumentResult;

[SmartEnum<int>]
public sealed partial class AmbientSlot {
    public static readonly AmbientSlot Ground = new(key: 0, bind: static handler => Subscription.Attach(
        subscribe: static h => GroundPlane.Changed += h, unsubscribe: static h => GroundPlane.Changed -= h, handler: handler));
    public static readonly AmbientSlot Sky = new(key: 1, bind: static handler => Subscription.Attach(
        subscribe: static h => Skylight.Changed += h, unsubscribe: static h => Skylight.Changed -= h, handler: handler));
    public static readonly AmbientSlot Daylight = new(key: 2, bind: static handler => Subscription.Attach(
        subscribe: static h => global::Rhino.Render.Sun.Changed += h, unsubscribe: static h => global::Rhino.Render.Sun.Changed -= h, handler: handler));
    public static readonly AmbientSlot Guides = new(key: 3, bind: static handler => Subscription.Attach(
        subscribe: static h => SafeFrame.Changed += h, unsubscribe: static h => SafeFrame.Changed -= h, handler: handler));
    public static readonly AmbientSlot Channels = new(key: 4, bind: static handler => Subscription.Attach(
        subscribe: static h => RenderChannels.Changed += h, unsubscribe: static h => RenderChannels.Changed -= h, handler: handler));

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Bind(EventHandler<RenderPropertyChangedEvent> handler);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class AmbientWatch : IDisposable {
    private Subscription? subscription;
    private readonly Atom<FailureLedger<AmbientFailure>> ledger;

    private AmbientWatch(Subscription subscription, Atom<FailureLedger<AmbientFailure>> ledger) {
        this.subscription = subscription;
        this.ledger = ledger;
    }

    public Seq<AmbientFailure> Failures => ledger.Value.Retained;
    public RetentionOverflow Overflow => ledger.Value.Overflow;

    public void Dispose() {
        Subscription? captured = Interlocked.Exchange(location1: ref subscription, value: null);
        captured?.Dispose();
    }

    public static Fin<AmbientWatch> Of(
        Seq<AmbientSlot> slots,
        RetentionPolicy retention,
        Func<AmbientFact, Fin<Unit>> sink) {
        Op op = Op.Of(name: nameof(AmbientWatch));
        Atom<FailureLedger<AmbientFailure>> ledger = Atom(FailureLedger<AmbientFailure>.Empty);
        return from activeRetention in op.Need(retention)
               from activeSink in op.Need(sink)
               from _ in guard(
                   !slots.IsEmpty
                   && slots.ForAll(static slot => slot is not null),
                   op.InvalidInput())
               from attached in Subscription.AttachAll(
                   slots.Distinct().Map(slot => (Func<Fin<Subscription>>)(() =>
                       slot.Bind(handler: (_, args) => ignore(Deliver(
                           slot: slot,
                           args: args,
                           retention: activeRetention,
                           sink: activeSink,
                           ledger: ledger,
                           op: op))))))
               select new AmbientWatch(subscription: attached, ledger: ledger);
    }

    private static Fin<Unit> Deliver(
        AmbientSlot slot,
        RenderPropertyChangedEvent args,
        RetentionPolicy retention,
        Func<AmbientFact, Fin<Unit>> sink,
        Atom<FailureLedger<AmbientFailure>> ledger,
        Op op) {
        AmbientFact fallback = new(Slot: slot, Key: None, Context: 0);
        return Contextual(args: args, fallback: fallback, op: op)
            .BindFail(fault => Retain(
                fact: fallback, fault: fault, retention: retention, ledger: ledger, op: op))
            .Bind(contextual => Project(args: args, contextual: contextual, op: op)
                .BindFail(fault => Retain(
                    fact: contextual, fault: fault, retention: retention, ledger: ledger, op: op)))
            .Bind(fact => op.Catch(() => sink(fact)).BindFail(fault => Retain(
                fact: fact, fault: fault, retention: retention, ledger: ledger, op: op)));
    }

    private static Fin<AmbientFact> Contextual(RenderPropertyChangedEvent args, AmbientFact fallback, Op op) =>
        op.Catch(() => Fin.Succ(value: fallback with { Context = args.Context }));

    private static Fin<AmbientFact> Project(RenderPropertyChangedEvent args, AmbientFact contextual, Op op) =>
        op.Catch(() => {
            return Optional(args.Document).Match(
                Some: document => DocKey.Of(document: document, key: op)
                    .Map(key => contextual with { Key = Some(key) }),
                None: () => Fin.Succ(value: contextual));
        });

    private static Fin<Unit> Retain(
        AmbientFact fact,
        Error fault,
        RetentionPolicy retention,
        Atom<FailureLedger<AmbientFailure>> ledger,
        Op op) => op.Catch(() => {
            _ = ledger.Swap(held => held.Admit(
                policy: retention,
                incoming: new AmbientFailure(Fact: fact, Fault: fault),
                fault: static failure => failure.Fault).Ledger);
            return Fin.Succ(value: unit);
        }).Match(
            Succ: _ => Fin.Fail<Unit>(error: fault),
            Fail: retention => Fin.Fail<Unit>(error: fault + retention));
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]                              | [FORM]                             | [ENTRY]               |
| :-----: | :---------------- | :----------------------------------- | :--------------------------------- | :-------------------- |
|  [01]   | live origin       | `SettingsSource.Live`                | document borrow                    | `Use` / `Mutate`      |
|  [02]   | archive origin    | `SettingsSource.Archived`            | archive borrow                     | `Use` / `Mutate`      |
|  [03]   | detached origin   | `SettingsSource.Detached`            | owned borrow                       | `Use` / `Mutate`      |
|  [04]   | sub-owner window  | `SubOwners`                          | bracket-owned seven-wrapper borrow | `Within`              |
|  [05]   | state             | state owners                         | total projection                   | `Of` / `Apply`        |
|  [06]   | aggregate config  | `RenderConfig`                       | correlated configuration           | `Of` / `Apply`        |
|  [07]   | dither vocabulary | `DitherMethod`                       | the one `Dithering.Methods` owner  | `Of(native, key)`     |
|  [08]   | astronomy         | `SunProblem` / `SunSolution`         | closed request/result              | `SunSolver.Solve`     |
|  [09]   | machine location  | `SunCapability`                      | grant the `Here` case names        | `SunSolver.Solve`     |
|  [10]   | settings rail     | `SettingsRequest` / `SettingsResult` | correlated request/result          | `Settings.Run`        |
|  [11]   | mutation receipt  | `SettingsAxis` / `SettingsReceipt`   | changed axes with undo             | `Settings.Run`        |
|  [12]   | broadcasts        | `AmbientSlot` / `AmbientFailure`     | verified failure ledger            | `AmbientWatch.Of`     |
|  [13]   | engine-bound site | `SolarFrame`                         | annual-run georeference gate       | `SolarFrame.Validate` |
|  [14]   | descriptor sun    | `SunDerivation` / `SceneSun`         | sited-or-authored wire band        | `SceneSun.Of`         |

## [08]-[RESEARCH]

(none)
