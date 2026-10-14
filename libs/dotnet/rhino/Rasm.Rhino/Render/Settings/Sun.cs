using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Rows;
using Rhino.Render;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SunState : SunLight, IRenderSettingsRecord<SunState, SunParameter> {
    public static SunState Default { get; } = new() {
        Enabled = false, Intensity = SunIntensity.Default, ManualControlOn = false, Azimuth = NorthAngle.MinValue, Altitude = SunAltitude.Horizon, Site = SunSite.Default,
    };

    public static string Owner => "sun";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.SunChanged;

    public static IO<SunState> Read(RenderSettingsScope scope) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.Sun, static sun => IO.lift(() =>
            (Conversions.Validated<SunIntensity, double, InvalidRhinoValue>(sun.Intensity).ToValidation(),
             sun.ManualControlOn ? Conversions.Validated<NorthAngle, double, InvalidRhinoValue>(sun.Azimuth).ToValidation() : Default.Azimuth,
             sun.ManualControlOn ? Conversions.Validated<SunAltitude, double, InvalidRhinoValue>(sun.Altitude).ToValidation() : Default.Altitude,
             SunSite.Read(sun).ToValidation())
                .Apply((intensity, azimuth, altitude, site) => new SunState { Enabled = sun.Enabled, Intensity = intensity, ManualControlOn = sun.ManualControlOn, Azimuth = azimuth, Altitude = altitude, Site = site })
                .As()
                .ToFin()));

    public static IO<SunState> Write(RenderSettingsScope scope, SunState state) =>
        (state.ManualControlOn ? state : state with { Azimuth = Default.Azimuth, Altitude = Default.Altitude }) switch {
            var accepted => RenderSettingsSources.SubOwner(scope, static settings => settings.Sun, sun => IO.lift(() => {
                SunMapper.Update(accepted, sun);
                return accepted;
            })),
        };

    public static ParameterText Text(SunParameter parameter) =>
        parameter.Map(
            enabled: ParameterText.Of("On", "Lights the scene with the sun"),
            intensity: ParameterText.Of("Intensity", "Sun intensity multiplier"),
            manualControlOn: ParameterText.Of("Manual control", "Places the sun by azimuth and altitude, off places it by date, time, and location"),
            azimuth: ParameterText.Of("Azimuth", "Bearing of the sun east of true north"),
            altitude: ParameterText.Of("Altitude", "Angle of the sun above the horizon"),
            north: ParameterText.Of("North", "Bearing of true north anticlockwise from world X"),
            placeLatitude: ParameterText.Of("Lat", "Latitude of the site"),
            placeLongitude: ParameterText.Of("Long", "Longitude of the site"),
            timeZone: ParameterText.Of("Time zone", "Standard offset of the site from UTC"),
            saving: ParameterText.Of("Daylight saving", "Minutes added to the standard time while daylight saving holds"));

    public static RowRules Rules(RowSource<SunState> source, SunParameter parameter) =>
        (RowRule.When(source, static state => state.Enabled, SunParameter.Enabled), RowRule.When(source, static state => state.ManualControlOn, SunParameter.ManualControlOn)) switch {
            var (on, manual) => (on & manual, on & !manual) switch {
                var (placed, dated) => new RowRules(None, parameter.Map<Option<RowRule>>(
                    enabled: None, intensity: on, manualControlOn: on, azimuth: placed, altitude: placed, north: on,
                    placeLatitude: dated, placeLongitude: dated, timeZone: dated, saving: dated)),
            },
        };

    public static IO<SunAngles> Angles(RenderSettingsSource source) =>
        RenderSettingsSources.Read(source, static scope => RenderSettingsSources.SubOwner(scope, static settings => settings.Sun, static sun =>
            IO.lift(() => { sun.Accuracy = Sun.Accuracies.Maximum; }).Map(_ => SunMapper.ToAngles(sun))));

    public static ValueStore<SunMoment> Moment(RenderSettingsSource source, TimeProvider clock) =>
        ValueStore.Of(
            RenderSettingsSources.Read(source, static scope => RenderSettingsSources.SubOwner(scope, static settings => settings.Sun, static sun => IO.lift(() => SunMoment.Read(sun).Map(static moment => Some(moment))))),
            moment => moment.Traverse(held => RenderSettingsSources.Edit(source, scope => RenderSettingsSources.SubOwner(scope, static settings => settings.Sun, sun => IO.lift(held.Write(sun, clock))))).As().Map(static _ => unit),
            Applied.Live,
            RenderSettingsSources.Signal(source, EventKind.SunChanged));
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SunParameter : IStateParameter<SunState> {
    private static readonly Presentation<double> Degrees = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Degree), Step = 1d, Decimals = 1 };
    public static readonly SunParameter Enabled = new("enabled", new StateParameter<SunState>.Toggle(
        Lens<SunState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })));
    public static readonly SunParameter Intensity = new("intensity", StateParameter.Bounded<SunState, SunIntensity, double, InvalidRhinoValue>(Lens<SunState, SunIntensity>.New(static state => state.Intensity, static intensity => state => state with { Intensity = intensity }),
    new() { Soft = (0d, 5d), Step = 0.01d, Decimals = 2 }));
    public static readonly SunParameter ManualControlOn = new("manual-control", new StateParameter<SunState>.Toggle(
        Lens<SunState, bool>.New(static state => state.ManualControlOn, static on => state => state with { ManualControlOn = on })));
    public static readonly SunParameter Azimuth = new("azimuth", StateParameter.Bounded<SunState, NorthAngle, double, InvalidRhinoValue>(Lens<SunState, NorthAngle>.New(static state => state.Azimuth, static azimuth => state => state with { Azimuth = azimuth }), Degrees));
    public static readonly SunParameter Altitude = new("altitude", StateParameter.Bounded<SunState, SunAltitude, double, InvalidRhinoValue>(Lens<SunState, SunAltitude>.New(static state => state.Altitude, static altitude => state => state with { Altitude = altitude }), Degrees));
    public static readonly SunParameter North = new("north", StateParameter.Bounded<SunState, NorthAngle, double, InvalidRhinoValue>(Lens<SunState, NorthAngle>.New(static state => state.Site.North, static north => state => state with { Site = state.Site with { North = north } }), Degrees));
    public static readonly SunParameter PlaceLatitude = new("latitude", StateParameter.Bounded<SunState, Latitude, double, InvalidRhinoValue>(Lens<SunState, Latitude>.New(
        static state => state.Site.Place.Latitude,
        static latitude => state => state with { Site = state.Site with { Place = state.Site.Place with { Latitude = latitude } } }),
    Degrees));
    public static readonly SunParameter PlaceLongitude = new("longitude", StateParameter.Bounded<SunState, Longitude, double, InvalidRhinoValue>(Lens<SunState, Longitude>.New(
        static state => state.Site.Place.Longitude,
        static longitude => state => state with { Site = state.Site with { Place = state.Site.Place with { Longitude = longitude } } }),
    Degrees));
    public static readonly SunParameter TimeZone = new("time-zone", StateParameter.Bounded<SunState, StandardOffset, double, InvalidRhinoValue>(Lens<SunState, StandardOffset>.New(static state => state.Site.Standard, static standard => state => state with { Site = state.Site with { Standard = standard } }),
    new() { Unit = Quantity.GetUnitInfo(DurationUnit.Hour), Step = 0.25d, Decimals = 2 }));
    public static readonly SunParameter Saving = new("daylight-saving", StateParameter.OptionalBounded<SunState, DaylightSaving, int, InvalidRhinoValue>(Lens<SunState, Gated<DaylightSaving>>.New(
        static state => state.Site.Saving.Match(Some: static saving => new Gated<DaylightSaving>(Enabled: true, saving), None: static () => new Gated<DaylightSaving>(Enabled: false, DaylightSaving.MinValue)),
        static gate => state => state with { Site = state.Site with { Saving = gate.Active } }),
    new() { Unit = Quantity.GetUnitInfo(DurationUnit.Minute), Step = 60, Decimals = 0 }));

    public StateParameter<SunState> Kind { get; }
}
