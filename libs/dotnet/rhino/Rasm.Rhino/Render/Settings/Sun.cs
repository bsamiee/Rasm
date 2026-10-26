using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
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
        DisposalOps.Within(() => scope.Settings.Sun, static sun => IO.lift(() =>
            (SunIntensity.Validated(sun.Intensity),
             sun.ManualControlOn ? NorthAngle.Validated(sun.Azimuth) : Default.Azimuth,
             sun.ManualControlOn ? SunAltitude.Validated(sun.Altitude) : Default.Altitude,
             SunSite.Read(sun))
                .Apply((intensity, azimuth, altitude, site) => new SunState { Enabled = sun.Enabled, Intensity = intensity, ManualControlOn = sun.ManualControlOn, Azimuth = azimuth, Altitude = altitude, Site = site })
                .As()));

    public static IO<SunState> Write(RenderSettingsScope scope, SunState state) =>
        (state.ManualControlOn ? state : state with { Azimuth = Default.Azimuth, Altitude = Default.Altitude }) switch {
            var accepted => DisposalOps.Within(() => scope.Settings.Sun, sun => IO.lift(() => {
                SunMapper.Update(accepted, sun);
                return accepted;
            })),
        };

    public static RowRules Rules(RowSource<SunState> source, SunParameter parameter) =>
        (RowRule.When(source, static state => state.Enabled, SunParameter.Enabled), RowRule.When(source, static state => state.ManualControlOn, SunParameter.ManualControlOn)) switch {
            var (on, manual) => (on & manual, on & !manual) switch {
                var (placed, dated) => new RowRules(None, parameter.Map<Option<RowRule>>(
                    enabled: None, intensity: on, manualControlOn: on, azimuth: placed, altitude: placed, north: on,
                    placeLatitude: dated, placeLongitude: dated, timeZone: dated, saving: dated)),
            },
        };

    public static IO<SunAngles> Angles(RenderSettingsSource source) =>
        RenderSettingsSources.Read(source, static scope => DisposalOps.Within(() => scope.Settings.Sun, static sun =>
            DisposalOps.Within(static () => new Sun(), copy => IO.lift(() => {
                copy.CopyFrom(sun);
                copy.Accuracy = Sun.Accuracies.Maximum;
                return SunMapper.ToAngles(copy);
            }))));

    public static ValueStore<SunMoment> Moment(RenderSettingsSource source, TimeProvider clock) =>
        ValueStore.Of(
            RenderSettingsSources.Read(source, static scope => DisposalOps.Within(() => scope.Settings.Sun, static sun => IO.lift(() => SunMoment.Read(sun).Map(static moment => Some(moment))))),
            moment => moment.Traverse(held => RenderSettingsSources.Edit(source, scope => DisposalOps.Within(() => scope.Settings.Sun, sun => IO.lift(held.Write(sun, clock))))).As().Map(unit),
            Applied.Live,
            RenderSettingsSources.Signal(source, EventKind.SunChanged));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SunParameter : IStateParameter<SunState> {
    private static readonly Presentation<double> Degrees = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Degree), Step = 1d, Decimals = 1 };
    public static readonly SunParameter Enabled = new("enabled", new StateParameter<SunState>.Toggle(
        Lens<SunState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })),
        new("On", "Lights scene with sun"));
    public static readonly SunParameter Intensity = new("intensity", StateParameter.Bounded(Lens<SunState, SunIntensity>.New(static state => state.Intensity, static intensity => state => state with { Intensity = intensity }),
            new Presentation<double> { Soft = (0d, 5d), Step = 0.01d, Decimals = 2 }),
        new("Intensity", "Sun intensity multiplier"));
    public static readonly SunParameter ManualControlOn = new("manual-control", new StateParameter<SunState>.Toggle(
        Lens<SunState, bool>.New(static state => state.ManualControlOn, static on => state => state with { ManualControlOn = on })),
        new("Manual control", "Places sun by azimuth and altitude, off by date, time, and location"));
    public static readonly SunParameter Azimuth = new("azimuth", StateParameter.Bounded(Lens<SunState, NorthAngle>.New(static state => state.Azimuth, static azimuth => state => state with { Azimuth = azimuth }), Degrees),
        new("Azimuth", "Sun bearing east of true north"));
    public static readonly SunParameter Altitude = new("altitude", StateParameter.Bounded(Lens<SunState, SunAltitude>.New(static state => state.Altitude, static altitude => state => state with { Altitude = altitude }), Degrees),
        new("Altitude", "Sun angle above horizon"));
    public static readonly SunParameter North = new("north", StateParameter.Bounded(Lens<SunState, NorthAngle>.New(static state => state.Site.North, static north => state => state with { Site = state.Site with { North = north } }), Degrees),
        new("North", "Bearing of true north anticlockwise from world X"));
    public static readonly SunParameter PlaceLatitude = new("latitude", StateParameter.Bounded(Lens<SunState, Latitude>.New(
            static state => state.Site.Place.Latitude,
            static latitude => state => state with { Site = state.Site with { Place = state.Site.Place with { Latitude = latitude } } }),
            Degrees),
        new("Lat", "Site latitude"));
    public static readonly SunParameter PlaceLongitude = new("longitude", StateParameter.Bounded(Lens<SunState, Longitude>.New(
            static state => state.Site.Place.Longitude,
            static longitude => state => state with { Site = state.Site with { Place = state.Site.Place with { Longitude = longitude } } }),
            Degrees),
        new("Long", "Site longitude"));
    public static readonly SunParameter TimeZone = new("time-zone", StateParameter.Bounded(Lens<SunState, StandardOffset>.New(static state => state.Site.Standard, static standard => state => state with { Site = state.Site with { Standard = standard } }),
            new Presentation<double> { Unit = Quantity.GetUnitInfo(DurationUnit.Hour), Step = 0.25d, Decimals = 2 }),
        new("Time zone", "Site standard offset from UTC"));
    public static readonly SunParameter Saving = new("daylight-saving", StateParameter.OptionalBounded(Lens<SunState, Gated<DaylightSaving>>.New(
            static state => state.Site.Saving.Match(Some: static saving => new Gated<DaylightSaving>(Enabled: true, saving), None: static () => new Gated<DaylightSaving>(Enabled: false, DaylightSaving.MinValue)),
            static gate => state => state with { Site = state.Site with { Saving = gate.Active } }),
            new Presentation<int> { Unit = Quantity.GetUnitInfo(DurationUnit.Minute), Step = 60, Decimals = 0 }),
        new("Daylight saving", "Minutes added to standard time during daylight saving"));

    public StateParameter<SunState> Kind { get; }

    public ParameterText Text { get; }
}
