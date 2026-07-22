# [RASM_RHINO_RENDER_SETTINGS]

`SettingsSource` admits live, archived, or owned detached `RenderSettings` once, while `Settings.Run` closes read, edit, and whole-state copy through one correlated request/result family. Generated owners close host classifications, `RenderState` carries replayable configuration plus derived evidence, `SunSolver.Solve` closes astronomy, and `AmbientWatch` retains a bounded latest-failure ledger beside its verified broadcasts.

## [01]-[INDEX]

- [02]-[SOURCE]: `SettingsSource` — the duality union with its `Use` read and `Mutate` undo-bracketed borrow folds.
- [03]-[STATE_RECORDS]: writable sub-owner states, derived evidence, and `RenderConfig`.
- [04]-[SUN_ASTRONOMY]: `SunSolver` — the static position, calendar, and twilight solvers.
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
                from session in Optional(source.Session).ToFin(Fail: ctx.Op.InvalidInput())
                from result in session.Demand(
                    use: document =>
                        from settings in Optional(document.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                        from output in ctx.Borrow(settings)
                        select output,
                    key: ctx.Op,
                    needs: [SessionNeed.Read])
                select result,
            archived: static (ctx, source) =>
                from archive in Optional(source.Archive).ToFin(Fail: ctx.Op.InvalidInput())
                from result in ctx.Op.Catch(() =>
                    from settings in Optional(archive.Settings.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                    from output in ctx.Borrow(settings)
                    select output)
                select result,
            detached: static (ctx, source) =>
                from settings in Optional(source.Settings).ToFin(Fail: ctx.Op.InvalidInput())
                from result in ctx.Op.Catch(() => ctx.Borrow(settings.Resource))
                select result);

    internal Fin<SettingsReceipt> Mutate(string name, Func<RenderSettings, Fin<Seq<SettingsAxis>>> borrow, Op key) =>
        Switch(
            context: (Name: name, Borrow: borrow, Op: key),
            live: static (ctx, source) =>
                from session in Optional(source.Session).ToFin(Fail: ctx.Op.InvalidInput())
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
                from archive in Optional(source.Archive).ToFin(Fail: ctx.Op.InvalidInput())
                from receipt in ctx.Op.Catch(() =>
                    from settings in Optional(archive.Settings.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                    from axes in ctx.Borrow(settings)
                    select new SettingsReceipt(Applied: axes, UndoRecord: None))
                select receipt,
            detached: static (ctx, source) =>
                from settings in Optional(source.Settings).ToFin(Fail: ctx.Op.InvalidInput())
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

- Owner: each total-state owner carries one-pass `Of` and whole-state `Apply`; boundary guards reject invalid scalar, vector, key, and case combinations before host mutation. `SunEvidence` owns derived vector, hash, and light custody, while `WorkflowEvidence` owns reciprocal gamma and hash without treating either as replay input.
- Law: applies are total state, never a patch — every `Apply` re-asserts its full field set, so an absent field cannot silently clear and a configuration travels as one replayable value between documents, archives, and free-floating carriers.
- Law: sun position follows host mode — automatic state writes geolocation, timezone, daylight saving, and moment before clearing manual control; manual state admits either the host angle pair or vector setter after enabling manual control. Readback canonicalizes manual state to angles, while vector and hash detach as evidence.
- Law: `EnvironmentRole` and `EnvironmentView` close the usage-purpose product; `RenderConfig` writes one binding per role and `RenderState.EnvironmentResolution` reads both purposes without leaking host enums.
- Growth: a new host sub-owner property is one record field read and asserted in the same pass; a new sub-owner is one record with its `Of`/`Apply` pair and one `SettingsEdit` case.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
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
        manualVector: static position => position.Value.IsValid);
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
                ? new SunPosition.ManualAngles(Azimuth: sun.Azimuth, Altitude: sun.Altitude)
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
               from admittedUnits in Optional(units).ToFin(Fail: op.InvalidInput())
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
        return from output in Optional(self.Output).ToFin(Fail: key.InvalidInput()).Bind(value => value.Admit(key))
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

- Owner: `SunProblem` closes direction, altitude, Julian day, twilight, tint, and host-location modalities; `SunSolution` closes vector, scalar, color, and optional location egress; `SunSolver.Solve` is the sole entry.
- Law: each problem dispatches directly to its verified host static, and provider failure or invalid admission stays on the `Fin<SunSolution>` rail.
- Boundary: the georeference invariant — `Sun.North`/`Latitude`/`Longitude` re-encoded from `EarthAnchorPoint` after an anchor write — is the Exchange rail's earth-sync owner; this page never writes the anchor.

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
    public sealed record Location : SunProblem;

    internal bool IsValid => Switch(
        direction: static problem => Coordinate(problem.Latitude, problem.Longitude),
        altitude: static problem => Coordinate(problem.Latitude, problem.Longitude)
            && Time(problem.TimeZoneHours, problem.DaylightMinutes, problem.Hours)
            && problem.Mode is not null,
        julian: static problem => Time(problem.TimeZoneHours, problem.DaylightMinutes, problem.Hours),
        twilight: static _ => true,
        color: static problem => double.IsFinite(problem.AltitudeDegrees),
        location: static _ => true);

    private static bool Coordinate(double latitude, double longitude) =>
        double.IsFinite(latitude) && latitude is >= -90d and <= 90d
        && double.IsFinite(longitude) && longitude is >= -180d and <= 180d;

    private static bool Time(double zone, int daylight, double hours) =>
        double.IsFinite(zone) && zone is >= -24d and <= 24d
        && daylight is >= 0 and <= 1440 && double.IsFinite(hours);
}

[SmartEnum<bool>]
public sealed partial class SolarSolveMode {
    public static readonly SolarSolveMode Precise = new(false);
    public static readonly SolarSolveMode Fast = new(true);

    internal bool FastPath => Key;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunSolution : IDetachedDocumentResult {
    private SunSolution() { }
    public sealed record Vector(Vector3d Value) : SunSolution;
    public sealed record Scalar(double Value) : SunSolution;
    public sealed record Color(PerceptualColor Value) : SunSolution;
    public sealed record Location(Option<(double Latitude, double Longitude)> Value) : SunSolution;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SunSolver {
    public static Fin<SunSolution> Solve(SunProblem problem, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(problem).ToFin(Fail: op.InvalidInput())
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
            location: static (state, _) => state.Catch(() => Fin.Succ<SunSolution>(new SunSolution.Location(
                global::Rhino.Render.Sun.Here(out double latitude, out double longitude)
                    ? Some((latitude, longitude))
                    : Option<(double, double)>.None))))
               select solution;
    }
}
```

## [05]-[EDIT_RAIL]

- Owner: `SettingsRequest` closes read, edit, and copy; `SettingsResult` keeps state and receipt egress explicit; `Settings.Run` is the sole entry over every `SettingsSource` origin.
- Law: each request enters its source once; edit and whole-state replay lower through one `SettingsEdit` program inside one compensated mutation grant, and copy crosses sources only through an owned `RenderSettings.Duplicate()` lease.
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

    internal Fin<Unit> Apply(RenderSettings settings, Op op) =>
        Switch(
            (Settings: settings, Op: op),
            frame: static (context, edit) => Optional(edit.Config).ToFin(Fail: context.Op.InvalidInput())
                .Bind(config => config.Apply(settings: context.Settings, key: context.Op)),
            ground: static (context, edit) => Optional(edit.State).ToFin(Fail: context.Op.InvalidInput())
                .Bind(state => state.Apply(ground: context.Settings.GroundPlane, key: context.Op)),
            sky: static (context, edit) => edit.State.Apply(sky: context.Settings.Skylight, key: context.Op),
            daylight: static (context, edit) => Optional(edit.State).ToFin(Fail: context.Op.InvalidInput())
                .Bind(state => state.Apply(sun: context.Settings.Sun, key: context.Op)),
            workflow: static (context, edit) => edit.State.Apply(workflow: context.Settings.LinearWorkflow, key: context.Op),
            dither: static (context, edit) => edit.State.Apply(dither: context.Settings.Dithering, key: context.Op),
            guides: static (context, edit) => Optional(edit.State).ToFin(Fail: context.Op.InvalidInput())
                .Bind(state => state.Apply(frame: context.Settings.SafeFrame, key: context.Op)),
            channels: static (context, edit) => Optional(edit.State).ToFin(Fail: context.Op.InvalidInput())
                .Bind(state => state.Apply(channels: context.Settings.RenderChannels, key: context.Op)));
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
    public static Fin<SettingsResult> Run(SettingsSource source, SettingsRequest request) {
        Op op = Op.Of();
        return from activeSource in Optional(source).ToFin(Fail: op.InvalidInput())
               from activeRequest in Optional(request).ToFin(Fail: op.InvalidInput())
               from result in activeRequest.Switch(
                   context: (Source: activeSource, Op: op),
                   read: static (state, _) => state.Source.Use(
                       borrow: settings => ReadState(settings, state.Op)
                           .Map(static value => (SettingsResult)new SettingsResult.State(value)),
                       key: state.Op),
                   edit: static (state, command) => Commit(state.Source, command.Changes, state.Op)
                       .Map(static receipt => (SettingsResult)new SettingsResult.Changed(receipt)),
                   copyTo: static (state, command) => Copy(state.Source, command.Target, state.Op)
                       .Map(static receipt => (SettingsResult)new SettingsResult.Changed(receipt)))
               select result;
    }

    private static Fin<RenderState> ReadState(RenderSettings settings, Op op) =>
        from config in RenderConfig.Of(settings: settings, key: op)
        from daylight in SunState.Of(sun: settings.Sun, key: op)
        from workflow in WorkflowState.Of(workflow: settings.LinearWorkflow, key: op)
        from dither in DitherState.Of(dither: settings.Dithering, key: op)
        from channels in ChannelState.Of(channels: settings.RenderChannels, key: op)
        from environments in config.Environments.Resolve(settings: settings, key: op)
        from evidence in SunEvidence.Of(sun: settings.Sun, key: op)
        select new RenderState(
            Config: config,
            Ground: GroundPlaneState.Of(ground: settings.GroundPlane),
            Sky: SkylightState.Of(sky: settings.Skylight),
            Daylight: daylight,
            DaylightEvidence: evidence,
            Workflow: workflow,
            WorkflowEvidence: WorkflowEvidence.Of(workflow: settings.LinearWorkflow),
            Dither: dither,
            SafeFrame: SafeFrameState.Of(frame: settings.SafeFrame),
            Channels: channels,
            EnvironmentResolution: environments);

    private static Fin<SettingsReceipt> Commit(SettingsSource source, Seq<SettingsEdit> plan, Op op) {
        return from _ in guard(!plan.IsEmpty && plan.ForAll(static edit => edit is not null), op.InvalidInput())
               from receipt in source.Mutate(
                   name: nameof(SettingsRequest.Edit),
                   borrow: settings => ReadState(settings, op).Bind(prior => Compensated(settings, prior, plan, op)),
                   key: op)
               select receipt;
    }

    private static Fin<Seq<SettingsAxis>> Compensated(RenderSettings settings, RenderState prior, Seq<SettingsEdit> plan, Op op) {
        using (prior) {
            return ApplyPlan(settings: settings, plan: plan, op: op)
                .BindFail(fault => ApplyPlan(settings: settings, plan: prior.Replay(), op: op).Match(
                    Succ: static _ => Fin.Fail<Seq<SettingsAxis>>(error: fault),
                    Fail: restore => Fin.Fail<Seq<SettingsAxis>>(error: fault + restore)));
        }
    }

    private static Fin<SettingsReceipt> Copy(SettingsSource source, SettingsSource target, Op op) =>
        from activeTarget in Optional(target).ToFin(Fail: op.InvalidInput())
        from duplicate in source.Use(
            borrow: settings => op.Catch(() => Fin.Succ(
                (Lease<RenderSettings>)new Lease<RenderSettings>.Owned(Value: settings.Duplicate()))),
            key: op)
        from receipt in duplicate.Use(detached =>
            from state in ReadState(detached, op)
            from changed in state.Use(value => activeTarget.Mutate(
                name: nameof(SettingsRequest.CopyTo),
                borrow: settings => ReadState(settings, op)
                    .Bind(prior => Compensated(settings: settings, prior: prior, plan: value.Replay(), op: op)),
                key: op))
            select changed)
        select receipt;

    private static Fin<Seq<SettingsAxis>> ApplyPlan(RenderSettings settings, Seq<SettingsEdit> plan, Op op) =>
        plan.TraverseM(edit => edit.Apply(settings: settings, op: op)).As()
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
        return from activeRetention in Optional(retention).ToFin(Fail: op.InvalidInput())
               from activeSink in Optional(sink).ToFin(Fail: op.InvalidInput())
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

| [INDEX] | [CONCERN]        | [OWNER]                              | [FORM]                    | [ENTRY]           |
| :-----: | :--------------- | :----------------------------------- | :------------------------ | :---------------- |
|  [01]   | live origin      | `SettingsSource.Live`                | document borrow           | `Use` / `Mutate`  |
|  [02]   | archive origin   | `SettingsSource.Archived`            | archive borrow            | `Use` / `Mutate`  |
|  [03]   | detached origin  | `SettingsSource.Detached`            | owned borrow              | `Use` / `Mutate`  |
|  [04]   | state            | state owners                         | total projection          | `Of` / `Apply`    |
|  [05]   | aggregate config | `RenderConfig`                       | correlated configuration  | `Of` / `Apply`    |
|  [06]   | astronomy        | `SunProblem` / `SunSolution`         | closed request/result     | `SunSolver.Solve` |
|  [07]   | settings rail    | `SettingsRequest` / `SettingsResult` | correlated request/result | `Settings.Run`    |
|  [08]   | mutation receipt | `SettingsAxis` / `SettingsReceipt`   | changed axes with undo    | `Settings.Run`    |
|  [09]   | broadcasts       | `AmbientSlot` / `AmbientFailure`     | verified failure ledger   | `AmbientWatch.Of` |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
