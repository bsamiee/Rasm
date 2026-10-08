using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Geolocation;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Scenes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SunIntensity : System.Numerics.IMinMaxValue<SunIntensity> {
    public static SunIntensity MinValue { get; } = new(0d);
    public static SunIntensity MaxValue { get; } = new(double.MaxValue);
    public static SunIntensity Default { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Horizon", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SunAltitude : System.Numerics.IMinMaxValue<SunAltitude> {
    public static SunAltitude MinValue { get; } = new(-90d);
    public static SunAltitude MaxValue { get; } = new(90d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record SunState(bool Enabled, SunIntensity Intensity, bool ManualControlOn, NorthAngle Azimuth, SunAltitude Altitude, SunSite Site)
    : ISceneRecord<SunState, SunParameter> {
    public static SunState Default { get; } = new(
        Enabled: false, Intensity: SunIntensity.Default, ManualControlOn: false, Azimuth: NorthAngle.MinValue, Altitude: SunAltitude.Horizon, Site: SunSite.Default);

    public static string Owner => "sun";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.SunChanged;

    public static IO<SunState> Read(SceneWindow window) =>
        Sources.SubOwner(window, static settings => settings.Sun, static sun => IO.lift(() =>
            (Conversions.Validated<SunIntensity, double, InvalidRhinoValue>(sun.Intensity).ToValidation(),
             sun.ManualControlOn ? Conversions.Validated<NorthAngle, double, InvalidRhinoValue>(sun.Azimuth).ToValidation() : Default.Azimuth,
             sun.ManualControlOn ? Conversions.Validated<SunAltitude, double, InvalidRhinoValue>(sun.Altitude).ToValidation() : Default.Altitude,
             SunSite.Read(sun).ToValidation())
                .Apply((intensity, azimuth, altitude, site) => new SunState(sun.Enabled, intensity, sun.ManualControlOn, azimuth, altitude, site))
                .As()
                .ToFin()));

    public static IO<Unit> Write(SceneWindow window, SunState state) =>
        Sources.SubOwner(window, static settings => settings.Sun, sun => IO.lift(() => {
            SunMapper.Update(state, sun);
            SunSiteMapper.Update(state.Site, sun);
            if (state.ManualControlOn)
                SunMapper.Place(state, sun);
        }));

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

    public static IO<SunAngles> Angles(SceneSource source) =>
        Sources.Read(source, static window => Sources.SubOwner(window, static settings => settings.Sun, static sun =>
            IO.lift(() => { sun.Accuracy = Sun.Accuracies.Maximum; }).Map(_ => SunSiteMapper.ToAngles(sun))));

    public static ValueStore<SunMoment> Moment(SceneSource source, TimeProvider clock) =>
        ValueStore.Of(
            Sources.Read(source, static window => Sources.SubOwner(window, static settings => settings.Sun, static sun => IO.lift(() => SunMoment.Read(sun).Map(static moment => Some(moment))))),
            moment => moment.Match(
                Some: held => Sources.Edit(source, window => Sources.SubOwner(window, static settings => settings.Sun, sun => IO.lift(held.Write(sun, clock)))),
                None: static () => IO.pure(unit)),
            Applied.Live,
            Sources.Signal(source, EventKind.SunChanged));

    public static (RowSource<Option<SunMoment>> Source, RowField<Option<SunMoment>> Field) MomentRow(SceneSource source, TimeProvider clock) =>
        RowSource.OptionalKeyed<SunMoment, string, InvalidRhinoValue>(Owner, "moment", "Date and time", "Local date and time the sun is placed at", _ => Moment(source, clock));
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SunParameter : IStateParameter<SunState> {
    public static readonly SunParameter Enabled = new("enabled", new StateParameter<SunState>.Toggle(
        Lens<SunState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })));
    public static readonly SunParameter Intensity = new("intensity", new StateParameter<SunState>.Bounded<SunIntensity, double, InvalidRhinoValue>(
        Lens<SunState, SunIntensity>.New(static state => state.Intensity, static intensity => state => state with { Intensity = intensity }),
        new() { Soft = (0d, 5d), Step = 0.01d, Decimals = 2 }));
    public static readonly SunParameter ManualControlOn = new("manual-control", new StateParameter<SunState>.Toggle(
        Lens<SunState, bool>.New(static state => state.ManualControlOn, static on => state => state with { ManualControlOn = on })));
    public static readonly SunParameter Azimuth = new("azimuth", new StateParameter<SunState>.Bounded<NorthAngle, double, InvalidRhinoValue>(
        Lens<SunState, NorthAngle>.New(static state => state.Azimuth, static azimuth => state => state with { Azimuth = azimuth }), Degrees<NorthAngle>()));
    public static readonly SunParameter Altitude = new("altitude", new StateParameter<SunState>.Bounded<SunAltitude, double, InvalidRhinoValue>(
        Lens<SunState, SunAltitude>.New(static state => state.Altitude, static altitude => state => state with { Altitude = altitude }), Degrees<SunAltitude>()));
    public static readonly SunParameter North = new("north", new StateParameter<SunState>.Bounded<NorthAngle, double, InvalidRhinoValue>(
        Lens<SunState, NorthAngle>.New(static state => state.Site.North, static north => state => state with { Site = state.Site with { North = north } }), Degrees<NorthAngle>()));
    public static readonly SunParameter PlaceLatitude = new("latitude", new StateParameter<SunState>.Bounded<Latitude, double, InvalidRhinoValue>(
        Lens<SunState, Latitude>.New(
            static state => state.Site.Place.Latitude,
            static latitude => state => state with { Site = state.Site with { Place = state.Site.Place with { Latitude = latitude } } }),
        Degrees<Latitude>()));
    public static readonly SunParameter PlaceLongitude = new("longitude", new StateParameter<SunState>.Bounded<Longitude, double, InvalidRhinoValue>(
        Lens<SunState, Longitude>.New(
            static state => state.Site.Place.Longitude,
            static longitude => state => state with { Site = state.Site with { Place = state.Site.Place with { Longitude = longitude } } }),
        Degrees<Longitude>()));
    public static readonly SunParameter TimeZone = new("time-zone", new StateParameter<SunState>.Bounded<StandardOffset, double, InvalidRhinoValue>(
        Lens<SunState, StandardOffset>.New(static state => state.Site.Standard, static standard => state => state with { Site = state.Site with { Standard = standard } }),
        new() { Unit = Quantity.GetUnitInfo(DurationUnit.Hour), Step = 0.25d, Decimals = 2 }));
    public static readonly SunParameter Saving = new("daylight-saving", new StateParameter<SunState>.OptionalBounded<DaylightSaving, int, InvalidRhinoValue>(
        Lens<SunState, Gated<DaylightSaving>>.New(
            static state => state.Site.Saving.Match(Some: static saving => new Gated<DaylightSaving>(Enabled: true, saving), None: static () => new Gated<DaylightSaving>(Enabled: false, DaylightSaving.MinValue)),
            static gate => state => state with { Site = state.Site with { Saving = gate.Active } }),
        new() { Unit = Quantity.GetUnitInfo(DurationUnit.Minute), Step = 60, Decimals = 0 }));

    public StateParameter<SunState> Kind { get; }

    private static Presentation<TValue, double> Degrees<TValue>() where TValue : System.Numerics.IMinMaxValue<TValue>, IConvertible<double> =>
        new() { Unit = Quantity.GetUnitInfo(AngleUnit.Degree), Step = 1d, Decimals = 1 };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both, IgnoreObsoleteMembersStrategy = IgnoreObsoleteMembersStrategy.Target)]
internal static partial class SunMapper {
    [MapperIgnoreSource(nameof(SunState.Azimuth), Justification = "Written by SunMapper.Place while ManualControlOn is on")]
    [MapperIgnoreSource(nameof(SunState.Altitude), Justification = "Written by SunMapper.Place while ManualControlOn is on")]
    [MapperIgnoreSource(nameof(SunState.Site), Justification = "Written through SunSiteMapper.Update(SunSite, Sun)")]
    [MapperIgnoreTarget(nameof(Sun.Azimuth), Justification = "Written by SunMapper.Place while ManualControlOn is on")]
    [MapperIgnoreTarget(nameof(Sun.Altitude), Justification = "Written by SunMapper.Place while ManualControlOn is on")]
    [MapperIgnoreTarget(nameof(Sun.North), Justification = "Written by SunSiteMapper.Update(SunSite, Sun)")]
    [MapperIgnoreTarget(nameof(Sun.Latitude), Justification = "Written by SunSiteMapper.Update(SunSite, Sun)")]
    [MapperIgnoreTarget(nameof(Sun.Longitude), Justification = "Written by SunSiteMapper.Update(SunSite, Sun)")]
    [MapperIgnoreTarget(nameof(Sun.TimeZone), Justification = "Written by SunSiteMapper.Update(SunSite, Sun)")]
    [MapperIgnoreTarget(nameof(Sun.DaylightSavingOn), Justification = "Written by SunSiteMapper.Update(SunSite, Sun)")]
    [MapperIgnoreTarget(nameof(Sun.DaylightSavingMinutes), Justification = "Written by SunSiteMapper.Update(SunSite, Sun)")]
    [MapperIgnoreTarget(nameof(Sun.Accuracy), Justification = "Session value SunState.Angles sets")]
    [MapperIgnoreTarget(nameof(Sun.Vector), Justification = "Derived from the angles the host holds")]
    internal static partial void Update(SunState state, Sun sun);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(SunState.Enabled), Justification = "Written by SunMapper.Update")]
    [MapperIgnoreSource(nameof(SunState.Intensity), Justification = "Written by SunMapper.Update")]
    [MapperIgnoreSource(nameof(SunState.ManualControlOn), Justification = "Written by SunMapper.Update before the angles")]
    [MapperIgnoreSource(nameof(SunState.Site), Justification = "Written through SunSiteMapper.Update(SunSite, Sun) before the angles")]
    internal static partial void Place(SunState state, Sun sun);

    [UserMapping]
    private static double Multiplier(SunIntensity intensity) => intensity;

    [UserMapping]
    private static double Degrees(NorthAngle azimuth) => azimuth;

    [UserMapping]
    private static double Degrees(SunAltitude altitude) => altitude;
}
