using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using UnitsNet;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GroundPlaneState(
    bool Enabled, bool ShowUnderside, Option<Length> Altitude, bool ShadowOnly,
    bool TextureOffsetLocked, Length TextureOffsetX, Length TextureOffsetY,
    bool TextureSizeLocked, Length TextureSizeX, Length TextureSizeY, NorthAngle TextureRotation, Option<Guid> MaterialInstanceId)
    : IRenderSettingsRecord<GroundPlaneState, GroundPlaneParameter> {
    public static GroundPlaneState Default { get; } = new(
        Enabled: true, ShowUnderside: false, Altitude: None, ShadowOnly: true,
        TextureOffsetLocked: false, TextureOffsetX: Length.Zero, TextureOffsetY: Length.Zero,
        TextureSizeLocked: true, TextureSizeX: Length.FromMeters(1d), TextureSizeY: Length.FromMeters(1d), TextureRotation: NorthAngle.MinValue, MaterialInstanceId: None);

    public static string Owner => "ground-plane";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.GroundPlaneChanged;

    public static IO<GroundPlaneState> Read(RenderSettingsScope scope) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.GroundPlane, plane => IO.lift(() => Capture(plane, scope.Units)));

    public static IO<GroundPlaneState> Write(RenderSettingsScope scope, GroundPlaneState state) =>
        from values in IO.lift((
            state.Altitude.Traverse(height => Model(height, scope.Units, double.MinValue)).As(),
            Model(state.TextureOffsetX, scope.Units, double.MinValue), Model(state.TextureOffsetY, scope.Units, double.MinValue),
            Model(state.TextureSizeX, scope.Units, 0d), Model(state.TextureSizeY, scope.Units, 0d))
            .Apply(static (altitude, x, y, width, height) => (Altitude: altitude, Offset: new Vector2d(x, y), Size: new Vector2d(width, height))).As().ToFin())
        let accepted = state with { MaterialInstanceId = Material(Conversions.Unset(state.MaterialInstanceId)) }
        from written in RenderSettingsSources.SubOwner(scope, static settings => settings.GroundPlane, plane => IO.lift(() => {
            GroundMapper.Update(accepted, plane, values.Offset, values.Size);
            _ = values.Altitude.IfSome(height => plane.Altitude = height);
            return Capture(plane, scope.Units);
        }))
        select written;

    private static Fin<GroundPlaneState> Capture(GroundPlane plane, LengthUnit units) =>
        (plane.TextureOffset, plane.TextureSize) switch {
            var (offset, size) =>
                (Callbacks.Found(!plane.AutoAltitude, plane.Altitude).Traverse(height => Measured(height, units, double.MinValue)).As(),
                 Measured(offset.X, units, double.MinValue), Measured(offset.Y, units, double.MinValue),
                 Measured(size.X, units, 0d), Measured(size.Y, units, 0d),
                 Conversions.Validated<NorthAngle, double, InvalidRhinoValue>(plane.TextureRotation).ToValidation())
                    .Apply((altitude, offsetX, offsetY, sizeX, sizeY, rotation) => new GroundPlaneState(
                        plane.Enabled, plane.ShowUnderside, altitude, plane.ShadowOnly, plane.TextureOffsetLocked, offsetX, offsetY,
                        plane.TextureSizeLocked, sizeX, sizeY, rotation, Material(plane.MaterialInstanceId)))
                    .As().ToFin(),
        };

    public static RowRules Rules(RowSource<GroundPlaneState> source, GroundPlaneParameter parameter) =>
        RowRule.When(source, static state => state.Enabled, GroundPlaneParameter.Enabled) switch {
            var on => (on & !RowRule.When(source, static state => state.ShadowOnly, GroundPlaneParameter.ShadowOnly)) switch {
                var textured => new RowRules(None, parameter.Map<Option<RowRule>>(
                    enabled: None, showUnderside: on, altitude: on, shadowOnly: on,
                    textureOffsetLocked: textured, textureOffsetX: textured, textureOffsetY: textured,
                    textureSizeLocked: textured, textureSizeX: textured, textureSizeY: textured, textureRotation: textured, materialInstanceId: textured)),
            },
        };

    private static Validation<Error, Length> Measured(double value, LengthUnit units, double minimum) =>
        value >= minimum && value <= double.MaxValue
            ? Success<Error, Length>(Quantities.From(value, units))
            : Fail<Error, Length>(new InvalidRhinoValue());

    private static Option<Guid> Material(Guid id) => Conversions.Present(id).Filter(static held => held != ContentUuids.DefaultMaterialInstance);

    private static Validation<Error, double> Model(Length length, LengthUnit units, double minimum) =>
        Quantities.As(length, units) switch {
            var model when model >= minimum && model <= double.MaxValue => Success<Error, double>(model),
            _ => Fail<Error, double>(new InvalidRhinoValue()),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GroundPlaneParameter : IRowParameter<GroundPlaneState> {
    private static readonly (double Low, double High) SignedBounds = (double.MinValue, double.MaxValue);
    private static readonly (double Low, double High) SizeBounds = (0d, double.MaxValue);
    private static readonly Func<Length, double> MetresKey = static length => length.Meters.ToDouble();
    private static readonly Presentation<double> Size =
        new() { Unit = Quantity.GetUnitInfo(UnitsNet.Units.LengthUnit.Meter), Soft = (0.01d, 10d), Step = 0.01d, Decimals = 2 };
    private static readonly Presentation<double> Offset =
        new() { Unit = Quantity.GetUnitInfo(UnitsNet.Units.LengthUnit.Meter), Soft = (-1d, 1d), Step = 0.01d, Decimals = 2 };

    public static readonly GroundPlaneParameter Enabled = new("enabled", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })),
        ParameterText.Of("Show Ground Plane", "Renders the ground plane"));
    public static readonly GroundPlaneParameter ShowUnderside = new("show-underside", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.ShowUnderside, static on => state => state with { ShowUnderside = on })),
        ParameterText.Of("Show underside", "Renders the plane's underside, off renders it single-sided"));
    public static readonly GroundPlaneParameter Altitude = new("altitude", new StateParameter<GroundPlaneState>.OptionalNumeric<Length, double>(Lens<GroundPlaneState, Gated<Length>>.New(
        static state => state.Altitude.Match(Some: static height => new Gated<Length>(Enabled: true, height), None: static () => new Gated<Length>(Enabled: false, Length.Zero)),
        static gate => state => state with { Altitude = gate.Active }),
        Metres(SignedBounds.Low), MetresKey, SignedBounds, new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(UnitsNet.Units.LengthUnit.Meter), Origin = 0d }),
        ParameterText.Of("Height above world XY", "Fixed height of the plane, off places it at the automatic height below the lowest object"));
    public static readonly GroundPlaneParameter ShadowOnly = new("shadow-only", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.ShadowOnly, static on => state => state with { ShadowOnly = on })),
        ParameterText.Of("Show shadows only", "Catches shadows alone, off renders the plane's material"));
    public static readonly GroundPlaneParameter TextureOffsetLocked = new("texture-offset-locked", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.TextureOffsetLocked, static locked => state => state with { TextureOffsetLocked = locked })),
        ParameterText.Of("Lock offset", "Moves both offset axes together"));
    public static readonly GroundPlaneParameter TextureOffsetX = new("texture-offset-x", new StateParameter<GroundPlaneState>.Numeric<Length, double>(Lens<GroundPlaneState, Length>.New(static state => state.TextureOffsetX, static offset => state => state with { TextureOffsetX = offset }), Metres(SignedBounds.Low), MetresKey, SignedBounds, Offset),
        ParameterText.Of("Offset X", "Texture offset along world X"));
    public static readonly GroundPlaneParameter TextureOffsetY = new("texture-offset-y", new StateParameter<GroundPlaneState>.Numeric<Length, double>(Lens<GroundPlaneState, Length>.New(static state => state.TextureOffsetY, static offset => state => state with { TextureOffsetY = offset }), Metres(SignedBounds.Low), MetresKey, SignedBounds, Offset),
        ParameterText.Of("Offset Y", "Texture offset along world Y"));
    public static readonly GroundPlaneParameter TextureSizeLocked = new("texture-size-locked", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.TextureSizeLocked, static locked => state => state with { TextureSizeLocked = locked })),
        ParameterText.Of("Lock size", "Scales both size axes together"));
    public static readonly GroundPlaneParameter TextureSizeX = new("texture-size-x", new StateParameter<GroundPlaneState>.Numeric<Length, double>(Lens<GroundPlaneState, Length>.New(static state => state.TextureSizeX, static size => state => state with { TextureSizeX = size }), Metres(SizeBounds.Low), MetresKey, SizeBounds, Size),
        ParameterText.Of("Size X", "Texture repeat size along world X"));
    public static readonly GroundPlaneParameter TextureSizeY = new("texture-size-y", new StateParameter<GroundPlaneState>.Numeric<Length, double>(Lens<GroundPlaneState, Length>.New(static state => state.TextureSizeY, static size => state => state with { TextureSizeY = size }), Metres(SizeBounds.Low), MetresKey, SizeBounds, Size),
        ParameterText.Of("Size Y", "Texture repeat size along world Y"));
    public static readonly GroundPlaneParameter TextureRotation = new("texture-rotation", StateParameter.Bounded<GroundPlaneState, NorthAngle, double, InvalidRhinoValue>(Lens<GroundPlaneState, NorthAngle>.New(static state => state.TextureRotation, static rotation => state => state with { TextureRotation = rotation }),
        new() { Unit = Quantity.GetUnitInfo(UnitsNet.Units.AngleUnit.Degree), Step = 1d, Decimals = 0 }),
        ParameterText.Of("Rotation", "Texture rotation about the world origin plus the offset"));
    public static readonly GroundPlaneParameter MaterialInstanceId = new("material-instance-id", new StateParameter<GroundPlaneState>.OptionalRaw<Guid>(
        Lens<GroundPlaneState, Option<Guid>>.New(static state => state.MaterialInstanceId, static material => state => state with { MaterialInstanceId = material })),
        ParameterText.Of("Material", "The render material instance assigned to the ground plane"));

    public StateParameter<GroundPlaneState> Kind { get; }

    public ParameterText Text { get; }

    private static Func<double, Fin<Length>> Metres(double minimum) =>
        value => value >= minimum && value <= double.MaxValue ? Fin.Succ(Length.FromMeters(value)) : new InvalidRhinoValue();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
internal static partial class GroundMapper {
    [MapProperty(nameof(@GroundPlaneState.Altitude.IsNone), nameof(GroundPlane.AutoAltitude))]
    [MapperIgnoreTarget(nameof(GroundPlane.Altitude), Justification = "Written in model units by GroundPlaneState.Write while AutoAltitude is off")]
    [MapperIgnoreSource(nameof(GroundPlaneState.TextureOffsetX), Justification = "Written through TextureOffset in model units")]
    [MapperIgnoreSource(nameof(GroundPlaneState.TextureOffsetY), Justification = "Written through TextureOffset in model units")]
    [MapperIgnoreSource(nameof(GroundPlaneState.TextureSizeX), Justification = "Written through TextureSize in model units")]
    [MapperIgnoreSource(nameof(GroundPlaneState.TextureSizeY), Justification = "Written through TextureSize in model units")]
    internal static partial void Update(GroundPlaneState state, GroundPlane plane, Vector2d textureOffset, Vector2d textureSize);
}
