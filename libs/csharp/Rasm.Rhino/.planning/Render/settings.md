# [RASM_RHINO_RENDER_SETTINGS]

Document render configuration (`Rasm.Rhino.Render`). `SettingsSource` carries the `DocumentOrFreeFloatingBase` duality as data: document-bound through a session, archive-attached through `File3dm`, or free-floating through the public constructor — one `Use` fold borrows for a read, one `Mutate` fold brackets a live mutation under an undo record and stamps a `SettingsReceipt`. Host `BeginChange`/`EndChange` methods are inert, non-obsolete no-ops; bound sub-owners write through their resolved native pointers, and `CopyFrom` is universal across all seven sub-owners. Each sub-owner projects to detached writable state, while derived sun evidence stays separate. `RenderState` aggregates the configuration, `SettingsEdit` closes mutation, `SunSolver` owns astronomical statics, and `AmbientWatch` projects all five real `Changed` broadcasts; `LinearWorkflow` and `Dithering` expose none.

## [01]-[INDEX]

- [02]-[SOURCE]: `SettingsSource` — the duality union with its `Use` read and `Mutate` undo-bracketed borrow folds.
- [03]-[STATE_RECORDS]: seven writable sub-owner records, derived sun evidence, and `RenderConfig`.
- [04]-[SUN_ASTRONOMY]: `SunSolver` — the static position, calendar, and twilight solvers.
- [05]-[EDIT_RAIL]: `SettingsEdit`, `RenderState`, and the `Settings` entry pair.
- [06]-[AMBIENT_WATCH]: `AmbientSlot` and the `Changed`-broadcast fold.
- [07]-[SURFACE_LEDGER]: page owner table.

## [02]-[SOURCE]

- Owner: `SettingsSource` `[Union]` — `Live` resolves `RhinoDoc.RenderSettings` inside a `Demand` window, `Archived` resolves the archive-bound `File3dm.Settings.RenderSettings`, and `Free` mints one owned free-floating `RenderSettings` retained until source disposal; `Use` borrows the selected aggregate for exactly one read callback, and `Mutate` borrows it for exactly one mutation callback — the live arm demanding `Mutate`+`Undo`, opening one named `UndoBracket`, and stamping the undo serial onto the `SettingsReceipt`.
- Law: the origin is the discriminant a consumer carries — the same `GroundPlane` type is document-bound, archive-attached, or free-floating by the host's internal pointer resolution, so no parallel type pair exists on this side of the seam and no live sub-owner leaves the borrow.
- Law: writes are in-place — a bound sub-owner's property write commits through its native pointer, the inert `BeginChange`/`EndChange` never appear, and a free-floating value enters a bound owner only through `CopyFrom` on the operation that needs the transfer.
- Law: only the document owns an undo record — archive and detached mutations apply without one (the archive commits at `File3dm.Write`, the detached value exists for `CopyFrom` transfer), so their receipts carry no serial.
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

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct SettingsReceipt(int Applied, uint UndoRecord = 0u) : IDetachedDocumentResult;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
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
            state: (Borrow: borrow, Op: key),
            live: static (ctx, source) => source.Session.Demand(
                use: document => Optional(document.RenderSettings).ToFin(Fail: ctx.Op.MissingContext()).Bind(ctx.Borrow),
                key: ctx.Op,
                needs: [SessionNeed.Read]),
            archived: static (ctx, source) => ctx.Op.Catch(() =>
                Optional(source.Archive.Settings.RenderSettings).ToFin(Fail: ctx.Op.MissingContext()).Bind(ctx.Borrow)),
            detached: static (ctx, source) => ctx.Op.Catch(() => ctx.Borrow(source.Settings.Resource)));

    internal Fin<SettingsReceipt> Mutate(string name, Func<RenderSettings, Fin<int>> borrow, Op key) =>
        Switch(
            state: (Name: name, Borrow: borrow, Op: key),
            live: static (ctx, source) => source.Session.Demand(
                use: document => Optional(document.RenderSettings).ToFin(Fail: ctx.Op.MissingContext()).Bind(settings =>
                    ctx.Op.Catch(() => {
                        using UndoBracket undo = UndoBracket.Begin(document: document, name: ctx.Name, recordsUndo: true);
                        Fin<SettingsReceipt> applied = guard(undo.Admitted, ctx.Op.InvalidResult()).ToFin()
                            .Bind(_ => ctx.Borrow(settings).Map(count => new SettingsReceipt(Applied: count)));
                        return undo.Seal(
                            outcome: applied,
                            stamp: static (receipt, serial) => receipt with { UndoRecord = serial },
                            key: ctx.Op);
                    })),
                key: ctx.Op,
                needs: [SessionNeed.Mutate, SessionNeed.Undo]),
            archived: static (ctx, source) => ctx.Op.Catch(() =>
                Optional(source.Archive.Settings.RenderSettings).ToFin(Fail: ctx.Op.MissingContext())
                    .Bind(settings => ctx.Borrow(settings).Map(static count => new SettingsReceipt(Applied: count)))),
            detached: static (ctx, source) => ctx.Op.Catch(() =>
                ctx.Borrow(source.Settings.Resource).Map(static count => new SettingsReceipt(Applied: count))));

    public void Dispose() {
        _ = Switch(
            live: static _ => unit,
            archived: static _ => unit,
            detached: static source => source.Settings.Dispose());
    }
}
```

## [03]-[STATE_RECORDS]

- Owner: seven total-state records, one per sub-owner, each with a one-pass `Of` read and whole-state `Apply`: `GroundPlaneState`, `SkylightState`, `SunState`, `WorkflowState`, `DitherState`, `SafeFrameState`, and `ChannelState`. `SunPosition` discriminates automatic geotemporal inputs from manual angles or a manual vector; `SunEvidence` carries derived vector and hash without pretending they are replay inputs. `RenderConfig` owns aggregate background, quality, element inclusion, image output, view source, and environment bindings.
- Law: applies are total state, never a patch — every `Apply` re-asserts its full field set, so an absent field cannot silently clear and a configuration travels as one replayable value between documents, archives, and free-floating carriers.
- Law: sun position follows host mode — automatic state writes geolocation, timezone, daylight saving, and moment before clearing manual control; manual state admits either the host angle pair or vector setter after enabling manual control. Readback canonicalizes manual state to angles, while vector and hash detach as evidence.
- Law: environment binding is per-usage rows — `RenderConfig` carries one `(usage, content, override)` row per `EnvironmentUsage`, read through `RenderEnvironmentId` under `Standard` purpose and written through `SetRenderEnvironmentId`/`SetRenderEnvironmentOverride`; the bound content itself is the registry page's territory.
- Growth: a new host sub-owner property is one record field read and asserted in the same pass; a new sub-owner is one record with its `Of`/`Apply` pair and one `SettingsEdit` case.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record GroundPlaneState(
    bool Enabled, bool ShadowOnly, bool ShowUnderside, double Altitude, bool AutoAltitude,
    Option<Guid> MaterialInstance, Vector2d TextureOffset, Vector2d TextureSize, double TextureRotation,
    bool TextureOffsetLocked, bool TextureSizeLocked) : IDetachedDocumentResult {
    internal static GroundPlaneState Of(GroundPlane ground) =>
        new(
            Enabled: ground.Enabled, ShadowOnly: ground.ShadowOnly, ShowUnderside: ground.ShowUnderside,
            Altitude: ground.Altitude, AutoAltitude: ground.AutoAltitude,
            MaterialInstance: Optional(ground.MaterialInstanceId).Filter(static id => id != Guid.Empty),
            TextureOffset: ground.TextureOffset, TextureSize: ground.TextureSize, TextureRotation: ground.TextureRotation,
            TextureOffsetLocked: ground.TextureOffsetLocked, TextureSizeLocked: ground.TextureSizeLocked);

    internal Fin<Unit> Apply(GroundPlane ground, Op key) {
        GroundPlaneState self = this;
        return key.Catch(() => {
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
        });
    }
}

public readonly record struct SkylightState(bool Enabled, double ShadowIntensity) : IDetachedDocumentResult {
    internal static SkylightState Of(Skylight sky) => new(Enabled: sky.Enabled, ShadowIntensity: sky.ShadowIntensity);

    internal Fin<Unit> Apply(Skylight sky, Op key) {
        SkylightState self = this;
        return key.Catch(() => {
            sky.Enabled = self.Enabled;
            sky.ShadowIntensity = self.ShadowIntensity;
            return Fin.Succ(value: unit);
        });
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
}

public readonly record struct SunEvidence(Vector3d Vector, uint Hash) : IDetachedDocumentResult {
    internal static SunEvidence Of(global::Rhino.Render.Sun sun) => new(Vector: sun.Vector, Hash: sun.Hash);
}

public sealed record SunState(
    bool Enabled,
    double Intensity,
    global::Rhino.Render.Sun.Accuracies Accuracy,
    double North,
    SunPosition Position) : IDetachedDocumentResult {
    internal static SunState Of(global::Rhino.Render.Sun sun) =>
        new(
            Enabled: sun.Enabled,
            Intensity: sun.Intensity,
            Accuracy: sun.Accuracy,
            North: sun.North,
            Position: sun.ManualControlOn
                ? new SunPosition.ManualAngles(Azimuth: sun.Azimuth, Altitude: sun.Altitude)
                : new SunPosition.Automatic(
                    Latitude: sun.Latitude, Longitude: sun.Longitude, TimeZone: sun.TimeZone,
                    DaylightSavingOn: sun.DaylightSavingOn, DaylightSavingMinutes: sun.DaylightSavingMinutes,
                    Moment: sun.GetDateTime(DateTimeKind.Local)));

    internal Fin<Unit> Apply(global::Rhino.Render.Sun sun, Op key) {
        SunState self = this;
        return key.Catch(() => {
            sun.Enabled = self.Enabled;
            sun.Intensity = self.Intensity;
            sun.Accuracy = self.Accuracy;
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

public readonly record struct WorkflowState(
    bool PreProcessColors, bool PreProcessTextures, bool PostProcessFrameBuffer,
    float PreProcessGamma, float PostProcessGamma, bool PostProcessGammaOn) : IDetachedDocumentResult {
    internal static WorkflowState Of(LinearWorkflow workflow) =>
        new(
            PreProcessColors: workflow.PreProcessColors, PreProcessTextures: workflow.PreProcessTextures,
            PostProcessFrameBuffer: workflow.PostProcessFrameBuffer,
            PreProcessGamma: workflow.PreProcessGamma, PostProcessGamma: workflow.PostProcessGamma,
            PostProcessGammaOn: workflow.PostProcessGammaOn);

    internal Fin<Unit> Apply(LinearWorkflow workflow, Op key) {
        WorkflowState self = this;
        return key.Catch(() => {
            workflow.PreProcessColors = self.PreProcessColors;
            workflow.PreProcessTextures = self.PreProcessTextures;
            workflow.PostProcessFrameBuffer = self.PostProcessFrameBuffer;
            workflow.PreProcessGamma = self.PreProcessGamma;
            workflow.PostProcessGamma = self.PostProcessGamma;
            workflow.PostProcessGammaOn = self.PostProcessGammaOn;
            return Fin.Succ(value: unit);
        });
    }
}

public readonly record struct DitherState(Dithering.Methods Method, bool Enabled) : IDetachedDocumentResult {
    internal static DitherState Of(Dithering dither) => new(Method: dither.Method, Enabled: dither.Enabled);

    internal Fin<Unit> Apply(Dithering dither, Op key) {
        DitherState self = this;
        return key.Catch(() => {
            dither.Method = self.Method;
            dither.Enabled = self.Enabled;
            return Fin.Succ(value: unit);
        });
    }
}

public sealed record SafeFrameState(
    bool Enabled, bool PerspectiveOnly, bool FieldsOn, bool LiveFrameOn,
    bool ActionFrameOn, bool ActionFrameLinked, double ActionFrameXScale, double ActionFrameYScale,
    bool TitleFrameOn, bool TitleFrameLinked, double TitleFrameXScale, double TitleFrameYScale) : IDetachedDocumentResult {
    internal static SafeFrameState Of(SafeFrame frame) =>
        new(
            Enabled: frame.Enabled, PerspectiveOnly: frame.PerspectiveOnly, FieldsOn: frame.FieldsOn, LiveFrameOn: frame.LiveFrameOn,
            ActionFrameOn: frame.ActionFrameOn, ActionFrameLinked: frame.ActionFrameLinked,
            ActionFrameXScale: frame.ActionFrameXScale, ActionFrameYScale: frame.ActionFrameYScale,
            TitleFrameOn: frame.TitleFrameOn, TitleFrameLinked: frame.TitleFrameLinked,
            TitleFrameXScale: frame.TitleFrameXScale, TitleFrameYScale: frame.TitleFrameYScale);

    internal Fin<Unit> Apply(SafeFrame frame, Op key) {
        SafeFrameState self = this;
        return key.Catch(() => {
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
        });
    }
}

public sealed record ChannelState(RenderChannels.Modes Mode, Seq<Guid> Custom) : IDetachedDocumentResult {
    internal static ChannelState Of(RenderChannels channels) =>
        new(Mode: channels.Mode, Custom: toSeq(channels.CustomList));

    internal Fin<Unit> Apply(RenderChannels channels, Op key) {
        ChannelState self = this;
        return key.Catch(() => {
            channels.CustomList = self.Custom.ToArray();
            channels.Mode = self.Mode;
            return Fin.Succ(value: unit);
        });
    }
}

public readonly record struct EnvironmentBinding(Option<Guid> Content, bool Override);

public sealed record EnvironmentBindingState {
    private readonly Seq<(RenderSettings.EnvironmentUsage Usage, EnvironmentBinding Binding)> rows;

    private EnvironmentBindingState(Seq<(RenderSettings.EnvironmentUsage Usage, EnvironmentBinding Binding)> rows) => this.rows = rows;

    public Seq<(RenderSettings.EnvironmentUsage Usage, EnvironmentBinding Binding)> Rows => rows;

    public static Fin<EnvironmentBindingState> Of(
        params ReadOnlySpan<(RenderSettings.EnvironmentUsage Usage, EnvironmentBinding Binding)> rows) {
        Op op = Op.Of(name: nameof(EnvironmentBindingState));
        Seq<(RenderSettings.EnvironmentUsage Usage, EnvironmentBinding Binding)> admitted = toSeq(rows.ToArray());
        Seq<RenderSettings.EnvironmentUsage> required = toSeq(Enum.GetValues<RenderSettings.EnvironmentUsage>());
        return guard(
                admitted.Count == required.Count
                && required.ForAll(usage => admitted.Count(row => row.Usage == usage) == 1)
                && admitted.ForAll(row => row.Binding.Content.Map(static id => id != Guid.Empty).IfNone(true)),
                op.InvalidInput())
            .ToFin()
            .Map(_ => new EnvironmentBindingState(rows: admitted));
    }

    internal static EnvironmentBindingState Of(RenderSettings settings) =>
        new(rows: toSeq(Enum.GetValues<RenderSettings.EnvironmentUsage>()).Map(usage => (
            usage,
            new EnvironmentBinding(
                Content: Optional(settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.Standard))
                    .Filter(static id => id != Guid.Empty),
                Override: settings.RenderEnvironmentOverride(usage)))));

    internal Fin<Unit> Apply(RenderSettings settings, Op key) {
        EnvironmentBindingState self = this;
        return key.Catch(() => {
            self.rows.Iter(row => {
                settings.SetRenderEnvironmentId(row.Usage, row.Binding.Content.IfNone(Guid.Empty));
                settings.SetRenderEnvironmentOverride(row.Usage, row.Binding.Override);
            });
            return Fin.Succ(value: unit);
        });
    }
}

public sealed record RenderConfig(
    PerceptualColor Ambient,
    PerceptualColor BackgroundTop,
    PerceptualColor BackgroundBottom,
    global::Rhino.Display.BackgroundStyle BackgroundStyle,
    bool TransparentBackground,
    AntialiasLevel Antialias,
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
    bool UseViewportSize,
    Size2i ImageSize,
    double ImageDpi,
    UnitSystem ImageUnits,
    bool ScaleBackgroundToFit,
    RenderSettings.RenderingSources Source,
    Option<string> NamedView,
    Option<string> SpecificViewport,
    Option<string> Snapshot,
    EnvironmentBindingState Environments) : IDetachedDocumentResult {

    internal static Fin<RenderConfig> Of(RenderSettings settings, Op key) =>
        key.Catch(() =>
            from ambient in PerceptualColor.OfRgb(settings.AmbientLight.R, settings.AmbientLight.G, settings.AmbientLight.B, settings.AmbientLight.A / 255.0)
            from top in PerceptualColor.OfRgb(settings.BackgroundColorTop.R, settings.BackgroundColorTop.G, settings.BackgroundColorTop.B, settings.BackgroundColorTop.A / 255.0)
            from bottom in PerceptualColor.OfRgb(settings.BackgroundColorBottom.R, settings.BackgroundColorBottom.G, settings.BackgroundColorBottom.B, settings.BackgroundColorBottom.A / 255.0)
            from imageSize in Size2i.Of(width: settings.ImageSize.Width, height: settings.ImageSize.Height, key: key)
            select new RenderConfig(
                Ambient: ambient, BackgroundTop: top, BackgroundBottom: bottom,
                BackgroundStyle: settings.BackgroundStyle, TransparentBackground: settings.TransparentBackground,
                Antialias: settings.AntialiasLevel, ShadowmapLevel: settings.ShadowmapLevel,
                RenderBackfaces: settings.RenderBackfaces, RenderCurves: settings.RenderCurves, RenderPoints: settings.RenderPoints,
                RenderMeshEdges: settings.RenderMeshEdges, RenderAnnotations: settings.RenderAnnotations, RenderIsoparams: settings.RenderIsoparams,
                UseHiddenLights: settings.UseHiddenLights, DepthCue: settings.DepthCue, FlatShade: settings.FlatShade,
                UseViewportSize: settings.UseViewportSize,
                ImageSize: imageSize,
                ImageDpi: settings.ImageDpi, ImageUnits: settings.ImageUnitSystem, ScaleBackgroundToFit: settings.ScaleBackgroundToFit,
                Source: settings.RenderSource,
                NamedView: Optional(settings.NamedView).Filter(static text => text.Length > 0),
                SpecificViewport: Optional(settings.SpecificViewport).Filter(static text => text.Length > 0),
                Snapshot: Optional(settings.Snapshot).Filter(static text => text.Length > 0),
                Environments: EnvironmentBindingState.Of(settings: settings)));

    internal Fin<Unit> Apply(RenderSettings settings, Op key) {
        RenderConfig self = this;
        return key.Catch(() => {
            settings.AmbientLight = Quantized(self.Ambient);
            settings.BackgroundColorTop = Quantized(self.BackgroundTop);
            settings.BackgroundColorBottom = Quantized(self.BackgroundBottom);
            settings.BackgroundStyle = self.BackgroundStyle;
            settings.TransparentBackground = self.TransparentBackground;
            settings.AntialiasLevel = self.Antialias;
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
            settings.ImageSize = self.ImageSize.Native;
            settings.ImageDpi = self.ImageDpi;
            settings.ImageUnitSystem = self.ImageUnits;
            settings.ScaleBackgroundToFit = self.ScaleBackgroundToFit;
            settings.NamedView = self.NamedView.IfNone(string.Empty);
            settings.SpecificViewport = self.SpecificViewport.IfNone(string.Empty);
            settings.Snapshot = self.Snapshot.IfNone(string.Empty);
            settings.RenderSource = self.Source;
            settings.UseViewportSize = self.UseViewportSize;
            return self.Environments.Apply(settings: settings, key: key);
        });
    }

    private static System.Drawing.Color Quantized(PerceptualColor color) {
        (byte r, byte g, byte b, double alpha) = color.ToRgb();
        return System.Drawing.Color.FromArgb(byte.CreateSaturating(Math.Round(alpha * byte.MaxValue)), r, g, b);
    }
}
```

## [04]-[SUN_ASTRONOMY]

- Owner: `SunSolver` — the astronomical statics as one owner: `Direction` the sun vector from geolocation and moment (UTC versus local inferred from `DateTime.Kind`), `AltitudeAt` the elevation solve over timezone and daylight-saving context, `Julian` the Julian-day value, `Twilight` the twilight-zone threshold, `ColorAt` the altitude-tinted light color across the kernel seam, `Here` the OS geolocation probe projected to absence on refusal.
- Law: no local astronomy — a re-derived solar position, Julian day, or twilight test beside these statics is the deleted form; both `Julian` and `Twilight` return bare `double` values, host truth this owner carries without a wrapper unit type.
- Boundary: the georeference invariant — `Sun.North`/`Latitude`/`Longitude` re-encoded from `EarthAnchorPoint` after an anchor write — is the Exchange rail's earth-sync owner; this page never writes the anchor.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SunSolver {
    public static Vector3d Direction(double latitude, double longitude, DateTime when) =>
        global::Rhino.Render.Sun.SunDirection(latitude: latitude, longitude: longitude, when: when);

    public static double AltitudeAt(
        double latitude, double longitude, double timezoneHours, int daylightMinutes, DateTime when, double hours, bool fast = false) =>
        global::Rhino.Render.Sun.AltitudeFromValues(
            latitude: latitude, longitude: longitude, timezoneHours: timezoneHours,
            daylightMinutes: daylightMinutes, when: when, hours: hours, fast: fast);

    public static double Julian(double timezoneHours, int daylightMinutes, DateTime when, double hours) =>
        global::Rhino.Render.Sun.JulianDay(timezoneHours: timezoneHours, daylightMinutes: daylightMinutes, when: when, hours: hours);

    public static double Twilight() => global::Rhino.Render.Sun.TwilightZone();

    public static Fin<PerceptualColor> ColorAt(double altitudeDegrees, Op? key = null) {
        System.Drawing.Color tint = global::Rhino.Render.Sun.ColorFromAltitude(altitudeDegrees);
        return PerceptualColor.OfRgb(tint.R, tint.G, tint.B, tint.A / 255.0, key);
    }

    public static Option<(double Latitude, double Longitude)> Here() =>
        global::Rhino.Render.Sun.Here(out double latitude, out double longitude)
            ? Some((latitude, longitude))
            : Option<(double, double)>.None;
}
```

## [05]-[EDIT_RAIL]

- Owner: `RenderState` — the whole-configuration read product: the aggregate `RenderConfig` plus every sub-owner state record; `SettingsEdit` `[Union]` — one case per state record, each dispatching its `Apply` against the borrowed aggregate's sub-owner accessor; `Settings` — the entry pair: `Ask` reads the whole state in one `Use` borrow, `Commit` applies an edit sequence in one `Mutate` borrow and returns the stamped `SettingsReceipt`.
- Law: one borrow per verb — `Ask` and `Commit` each enter the source exactly once, every sub-owner resolves off that one `RenderSettings`, and the sequence applies inside the window so a half-applied plan never spans two grants.
- Law: `CopySubOwners` captures all seven source sub-owners into one owned free-floating capsule, releases the source borrow, then transfers each universal `CopyFrom` inside one target `Mutate` borrow.
- Boundary: `RenderSettings.PostEffects : PostEffectCollection` is the eighth host sub-owner; its configuration rows are the Display render page's post-effect territory, so this page carries no eighth record.
- Growth: a new configuration axis is one state-record field; a new sub-owner is one record, one `RenderState` field, and one `SettingsEdit` case with every consumer untouched.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record RenderState(
    RenderConfig Config,
    GroundPlaneState Ground,
    SkylightState Sky,
    SunState Daylight,
    SunEvidence DaylightEvidence,
    WorkflowState Workflow,
    DitherState Dither,
    SafeFrameState SafeFrame,
    ChannelState Channels) : IDetachedDocumentResult;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsEdit {
    private SettingsEdit() { }
    public sealed record Frame(RenderConfig Config) : SettingsEdit;
    public sealed record Ground(GroundPlaneState State) : SettingsEdit;
    public sealed record Sky(SkylightState State) : SettingsEdit;
    public sealed record Daylight(SunState State) : SettingsEdit;
    public sealed record Workflow(WorkflowState State) : SettingsEdit;
    public sealed record Dither(DitherState State) : SettingsEdit;
    public sealed record Guides(SafeFrameState State) : SettingsEdit;
    public sealed record Channels(ChannelState State) : SettingsEdit;

    internal Fin<Unit> Apply(RenderSettings settings, Op op) =>
        Switch(
            (Settings: settings, Op: op),
            frame: static (context, edit) => edit.Config.Apply(settings: context.Settings, key: context.Op),
            ground: static (context, edit) => edit.State.Apply(ground: context.Settings.GroundPlane, key: context.Op),
            sky: static (context, edit) => edit.State.Apply(sky: context.Settings.Skylight, key: context.Op),
            daylight: static (context, edit) => edit.State.Apply(sun: context.Settings.Sun, key: context.Op),
            workflow: static (context, edit) => edit.State.Apply(workflow: context.Settings.LinearWorkflow, key: context.Op),
            dither: static (context, edit) => edit.State.Apply(dither: context.Settings.Dithering, key: context.Op),
            guides: static (context, edit) => edit.State.Apply(frame: context.Settings.SafeFrame, key: context.Op),
            channels: static (context, edit) => edit.State.Apply(channels: context.Settings.RenderChannels, key: context.Op));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Settings {
    public static Fin<RenderState> Ask(SettingsSource source) {
        Op op = Op.Of();
        return source.Use(
            borrow: settings => RenderConfig.Of(settings: settings, key: op).Map(config => new RenderState(
                Config: config,
                Ground: GroundPlaneState.Of(ground: settings.GroundPlane),
                Sky: SkylightState.Of(sky: settings.Skylight),
                Daylight: SunState.Of(sun: settings.Sun),
                DaylightEvidence: SunEvidence.Of(sun: settings.Sun),
                Workflow: WorkflowState.Of(workflow: settings.LinearWorkflow),
                Dither: DitherState.Of(dither: settings.Dithering),
                SafeFrame: SafeFrameState.Of(frame: settings.SafeFrame),
                Channels: ChannelState.Of(channels: settings.RenderChannels))),
            key: op);
    }

    public static Fin<SettingsReceipt> Commit(SettingsSource source, params ReadOnlySpan<SettingsEdit> edits) {
        Op op = Op.Of();
        Seq<SettingsEdit> plan = toSeq(edits.ToArray());
        return from _ in guard(!plan.IsEmpty, op.InvalidInput())
               from receipt in source.Mutate(
                   name: nameof(Commit),
                   borrow: settings => plan.TraverseM(edit => edit.Apply(settings: settings, op: op)).As().Map(_ => plan.Count),
                   key: op)
               select receipt;
    }

    public static Fin<SettingsReceipt> CopySubOwners(SettingsSource source, SettingsSource target) {
        Op op = Op.Of();
        return source.Use(
                borrow: settings => op.Catch(() => Fin.Succ(value: new SubOwnerCopies(source: settings))),
                key: op)
            .Bind(copies => {
                using (copies) {
                    return target.Mutate(
                        name: nameof(CopySubOwners),
                        borrow: settings => op.Catch(() => Fin.Succ(value: copies.CopyTo(target: settings))),
                        key: op);
                }
            });
    }

    private sealed class SubOwnerCopies : IDisposable, IDetachedDocumentResult {
        private GroundPlane? ground;
        private Skylight? sky;
        private global::Rhino.Render.Sun? daylight;
        private LinearWorkflow? workflow;
        private Dithering? dither;
        private SafeFrame? safeFrame;
        private RenderChannels? channels;

        internal SubOwnerCopies(RenderSettings source) {
            try {
                ground = new GroundPlane();
                sky = new Skylight();
                daylight = new global::Rhino.Render.Sun();
                workflow = new LinearWorkflow();
                dither = new Dithering();
                safeFrame = new SafeFrame();
                channels = new RenderChannels();
                Ground.CopyFrom(source.GroundPlane);
                Sky.CopyFrom(source.Skylight);
                Daylight.CopyFrom(source.Sun);
                Workflow.CopyFrom(source.LinearWorkflow);
                Dither.CopyFrom(source.Dithering);
                SafeFrame.CopyFrom(source.SafeFrame);
                Channels.CopyFrom(source.RenderChannels);
            } catch {
                Dispose();
                throw;
            }
        }

        internal GroundPlane Ground => ground ?? throw new ObjectDisposedException(nameof(SubOwnerCopies));
        internal Skylight Sky => sky ?? throw new ObjectDisposedException(nameof(SubOwnerCopies));
        internal global::Rhino.Render.Sun Daylight => daylight ?? throw new ObjectDisposedException(nameof(SubOwnerCopies));
        internal LinearWorkflow Workflow => workflow ?? throw new ObjectDisposedException(nameof(SubOwnerCopies));
        internal Dithering Dither => dither ?? throw new ObjectDisposedException(nameof(SubOwnerCopies));
        internal SafeFrame SafeFrame => safeFrame ?? throw new ObjectDisposedException(nameof(SubOwnerCopies));
        internal RenderChannels Channels => channels ?? throw new ObjectDisposedException(nameof(SubOwnerCopies));

        internal int CopyTo(RenderSettings target) {
            target.GroundPlane.CopyFrom(Ground);
            target.Skylight.CopyFrom(Sky);
            target.Sun.CopyFrom(Daylight);
            target.LinearWorkflow.CopyFrom(Workflow);
            target.Dithering.CopyFrom(Dither);
            target.SafeFrame.CopyFrom(SafeFrame);
            target.RenderChannels.CopyFrom(Channels);
            return 7;
        }

        public void Dispose() {
            Interlocked.Exchange(ref ground, null)?.Dispose();
            Interlocked.Exchange(ref sky, null)?.Dispose();
            Interlocked.Exchange(ref daylight, null)?.Dispose();
            Interlocked.Exchange(ref workflow, null)?.Dispose();
            Interlocked.Exchange(ref dither, null)?.Dispose();
            Interlocked.Exchange(ref safeFrame, null)?.Dispose();
            Interlocked.Exchange(ref channels, null)?.Dispose();
        }
    }
}
```

## [06]-[AMBIENT_WATCH]

- Owner: `AmbientSlot` `[SmartEnum<int>]` — the five sub-owners carrying a static `Changed` broadcast (`GroundPlane`, `Skylight`, `Sun`, `SafeFrame`, `RenderChannels`) as rows with one bind column each. `AmbientFact` detaches the slot, optional document key, and host property context. `AmbientWatch` owns transactional attach and symmetric release through the document `Subscription` capsule.
- Law: the roster is host truth — `LinearWorkflow` and `Dithering` carry no `Changed` event by decompile, so workflow and dither staleness is polled through `Ask`, never a phantom subscription.
- Law: `RenderPropertyChangedEvent.Document` and `Context` project inside the callback; `Context` remains the host's opaque integer discriminant, and a missing document yields `None` rather than a live handle.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
public readonly record struct AmbientFact(AmbientSlot Slot, Option<DocKey> Key, int Context) : IDetachedDocumentResult;

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

    private AmbientWatch(Subscription subscription) => this.subscription = subscription;

    public void Dispose() {
        Subscription? captured = Interlocked.Exchange(location1: ref subscription, value: null);
        captured?.Dispose();
    }

    public static Fin<AmbientWatch> Of(Seq<AmbientSlot> slots, Func<AmbientFact, Fin<Unit>> sink) {
        Op op = Op.Of(name: nameof(AmbientWatch));
        return from _ in guard(!slots.IsEmpty, op.InvalidInput())
               from attached in Subscription.AttachAll(
                   slots.Distinct().Map(slot => (Func<Fin<Subscription>>)(() =>
                       slot.Bind(handler: (_, args) => ignore(sink(new AmbientFact(
                           Slot: slot,
                           Key: Optional(args.Document).Bind(document => DocKey.Of(document: document, key: op).ToOption()),
                           Context: args.Context)))))))
               select new AmbientWatch(subscription: attached);
    }
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]                     | [FORM]                                            | [ENTRY]                         |
| :-----: | :---------------- | :-------------------------- | :------------------------------------------------ | :------------------------------ |
|  [01]   | settings origin   | `SettingsSource`            | one union: live session, archive, free-floating   | `Use` / `Mutate`                |
|  [02]   | sub-owner states  | the seven state records     | total-state read/apply pairs, kernel color seam   | `Of` / `Apply`                  |
|  [03]   | aggregate config  | `RenderConfig`              | background, quality, output, source, environments | `Of` / `Apply`                  |
|  [04]   | astronomy         | `SunSolver`                 | statics over position, calendar, twilight, tint   | `Direction` / `Julian` / `Here` |
|  [05]   | whole-state read  | `RenderState`               | one borrow, aggregate plus every sub-owner        | `Settings.Ask(source)`          |
|  [06]   | mutation rail     | `SettingsEdit` / `Settings` | one case per record, one receipted borrow per commit | `Commit(source, edits)`      |
|  [07]   | change broadcasts | `AmbientSlot`               | five rows over the verified `Changed` roster      | `AmbientWatch.Of(slots, sink)`  |
