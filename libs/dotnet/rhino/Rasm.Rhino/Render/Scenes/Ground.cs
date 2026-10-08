using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Geolocation;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Events;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using UnitsNet;

namespace Rasm.Rhino.Render.Scenes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct GroundLength : System.Numerics.IMinMaxValue<GroundLength> {
    public static GroundLength MinValue { get; } = new(double.MinValue);
    public static GroundLength MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TextureSpan : System.Numerics.IMinMaxValue<TextureSpan> {
    public static TextureSpan MinValue { get; } = new(double.BitIncrement(0d));
    public static TextureSpan MaxValue { get; } = new(double.MaxValue);
    public static TextureSpan Default { get; } = new(1d);
    public static Presentation<TextureSpan, double> Presentation { get; } =
        new() { Unit = Quantity.GetUnitInfo(UnitsNet.Units.LengthUnit.Meter), Soft = (0.01d, 10d), Step = 0.01d, Decimals = 2 };

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record GroundPlaneState(
    bool Enabled, bool ShowUnderside, Option<GroundLength> Altitude, bool ShadowOnly,
    bool TextureOffsetLocked, GroundLength TextureOffsetX, GroundLength TextureOffsetY,
    bool TextureSizeLocked, TextureSpan TextureSizeX, TextureSpan TextureSizeY, NorthAngle TextureRotation, Option<Guid> MaterialInstanceId)
    : ISceneRecord<GroundPlaneState, GroundPlaneParameter> {
    public static GroundPlaneState Default { get; } = new(
        Enabled: true, ShowUnderside: false, Altitude: None, ShadowOnly: true,
        TextureOffsetLocked: false, TextureOffsetX: GroundLength.Zero, TextureOffsetY: GroundLength.Zero,
        TextureSizeLocked: true, TextureSizeX: TextureSpan.Default, TextureSizeY: TextureSpan.Default, TextureRotation: NorthAngle.MinValue, MaterialInstanceId: None);

    public static string Owner => "ground-plane";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.GroundPlaneChanged;

    public static IO<GroundPlaneState> Read(SceneWindow window) =>
        Sources.SubOwner(window, static settings => settings.GroundPlane, plane => IO.lift(() =>
            (Callbacks.Found(!plane.AutoAltitude, plane.Altitude).Traverse(height => Measured<GroundLength>(height, window.Units)).As(),
             Measured<GroundLength>(plane.TextureOffset.X, window.Units), Measured<GroundLength>(plane.TextureOffset.Y, window.Units),
             Measured<TextureSpan>(plane.TextureSize.X, window.Units), Measured<TextureSpan>(plane.TextureSize.Y, window.Units),
             Conversions.Validated<NorthAngle, double, InvalidRhinoValue>(plane.TextureRotation).ToValidation())
                .Apply((altitude, offsetX, offsetY, sizeX, sizeY, rotation) => new GroundPlaneState(
                    plane.Enabled, plane.ShowUnderside, altitude, plane.ShadowOnly, plane.TextureOffsetLocked, offsetX, offsetY, plane.TextureSizeLocked, sizeX, sizeY, rotation,
                    Conversions.Present(plane.MaterialInstanceId).Filter(static id => id != ContentUuids.DefaultMaterialInstance)))
                .As()
                .ToFin()));

    public static IO<Unit> Write(SceneWindow window, GroundPlaneState state) =>
        Sources.SubOwner(window, static settings => settings.GroundPlane, plane => IO.lift(() => {
            GroundMapper.Update(state, plane);
            (plane.TextureOffset, plane.TextureSize) =
                (new Vector2d(Model(state.TextureOffsetX, window.Units), Model(state.TextureOffsetY, window.Units)),
                 new Vector2d(Model(state.TextureSizeX, window.Units), Model(state.TextureSizeY, window.Units)));
            _ = state.Altitude.IfSome(height => plane.Altitude = Model(height, window.Units));
        }));

    public static ParameterText Text(GroundPlaneParameter parameter) =>
        parameter.Map(
            enabled: ParameterText.Of("Show Ground Plane", "Renders the ground plane"),
            showUnderside: ParameterText.Of("Show underside", "Renders the plane's underside, off renders it single-sided"),
            altitude: ParameterText.Of("Height above world XY", "Fixed height of the plane, off places it at the automatic height below the lowest object"),
            shadowOnly: ParameterText.Of("Show shadows only", "Catches shadows alone, off renders the plane's material"),
            textureOffsetLocked: ParameterText.Of("Lock offset", "Moves both offset axes together"),
            textureOffsetX: ParameterText.Of("Offset X", "Texture offset along world X"),
            textureOffsetY: ParameterText.Of("Offset Y", "Texture offset along world Y"),
            textureSizeLocked: ParameterText.Of("Lock size", "Scales both size axes together"),
            textureSizeX: ParameterText.Of("Size X", "Texture repeat size along world X"),
            textureSizeY: ParameterText.Of("Size Y", "Texture repeat size along world Y"),
            textureRotation: ParameterText.Of("Rotation", "Texture rotation about the world origin plus the offset"));

    public static RowRules Rules(RowSource<GroundPlaneState> source, GroundPlaneParameter parameter) =>
        RowRule.When(source, static state => state.Enabled, GroundPlaneParameter.Enabled) switch {
            var on => (on & !RowRule.When(source, static state => state.ShadowOnly, GroundPlaneParameter.ShadowOnly)) switch {
                var textured => new RowRules(None, parameter.Map<Option<RowRule>>(
                    enabled: None, showUnderside: on, altitude: on, shadowOnly: on,
                    textureOffsetLocked: textured, textureOffsetX: textured, textureOffsetY: textured,
                    textureSizeLocked: textured, textureSizeX: textured, textureSizeY: textured, textureRotation: textured)),
            },
        };

    private static Validation<Error, T> Measured<T>(double value, LengthUnit units) where T : IObjectFactory<T, double, InvalidRhinoValue> =>
        Conversions.Validated<T, double, InvalidRhinoValue>(Quantities.From(value, units).Meters.ToDouble()).ToValidation();

    private static double Model(double metres, LengthUnit units) => Quantities.As(Length.FromMeters(metres), units);
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GroundPlaneParameter : IStateParameter<GroundPlaneState> {
    private static readonly Presentation<GroundLength, double> Offset =
        new() { Unit = Quantity.GetUnitInfo(UnitsNet.Units.LengthUnit.Meter), Soft = (-1d, 1d), Step = 0.01d, Decimals = 2 };

    public static readonly GroundPlaneParameter Enabled = new("enabled", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })));
    public static readonly GroundPlaneParameter ShowUnderside = new("show-underside", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.ShowUnderside, static on => state => state with { ShowUnderside = on })));
    public static readonly GroundPlaneParameter Altitude = new("altitude", new StateParameter<GroundPlaneState>.OptionalBounded<GroundLength, double, InvalidRhinoValue>(
        Lens<GroundPlaneState, Gated<GroundLength>>.New(
            static state => state.Altitude.Match(Some: static height => new Gated<GroundLength>(Enabled: true, height), None: static () => new Gated<GroundLength>(Enabled: false, GroundLength.Zero)),
            static gate => state => state with { Altitude = gate.Active }),
        new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(UnitsNet.Units.LengthUnit.Meter), Origin = 0d }));
    public static readonly GroundPlaneParameter ShadowOnly = new("shadow-only", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.ShadowOnly, static on => state => state with { ShadowOnly = on })));
    public static readonly GroundPlaneParameter TextureOffsetLocked = new("texture-offset-locked", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.TextureOffsetLocked, static locked => state => state with { TextureOffsetLocked = locked })));
    public static readonly GroundPlaneParameter TextureOffsetX = new("texture-offset-x", new StateParameter<GroundPlaneState>.Bounded<GroundLength, double, InvalidRhinoValue>(
        Lens<GroundPlaneState, GroundLength>.New(static state => state.TextureOffsetX, static offset => state => state with { TextureOffsetX = offset }), Offset));
    public static readonly GroundPlaneParameter TextureOffsetY = new("texture-offset-y", new StateParameter<GroundPlaneState>.Bounded<GroundLength, double, InvalidRhinoValue>(
        Lens<GroundPlaneState, GroundLength>.New(static state => state.TextureOffsetY, static offset => state => state with { TextureOffsetY = offset }), Offset));
    public static readonly GroundPlaneParameter TextureSizeLocked = new("texture-size-locked", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.TextureSizeLocked, static locked => state => state with { TextureSizeLocked = locked })));
    public static readonly GroundPlaneParameter TextureSizeX = new("texture-size-x", new StateParameter<GroundPlaneState>.Bounded<TextureSpan, double, InvalidRhinoValue>(
        Lens<GroundPlaneState, TextureSpan>.New(static state => state.TextureSizeX, static size => state => state with { TextureSizeX = size }), TextureSpan.Presentation));
    public static readonly GroundPlaneParameter TextureSizeY = new("texture-size-y", new StateParameter<GroundPlaneState>.Bounded<TextureSpan, double, InvalidRhinoValue>(
        Lens<GroundPlaneState, TextureSpan>.New(static state => state.TextureSizeY, static size => state => state with { TextureSizeY = size }), TextureSpan.Presentation));
    public static readonly GroundPlaneParameter TextureRotation = new("texture-rotation", new StateParameter<GroundPlaneState>.Bounded<NorthAngle, double, InvalidRhinoValue>(
        Lens<GroundPlaneState, NorthAngle>.New(static state => state.TextureRotation, static rotation => state => state with { TextureRotation = rotation }),
        new() { Unit = Quantity.GetUnitInfo(UnitsNet.Units.AngleUnit.Degree), Step = 1d, Decimals = 0 }));

    public StateParameter<GroundPlaneState> Kind { get; }
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
    [MapperIgnoreTarget(nameof(GroundPlane.TextureOffset), Justification = "Written in model units by GroundPlaneState.Write")]
    [MapperIgnoreTarget(nameof(GroundPlane.TextureSize), Justification = "Written in model units by GroundPlaneState.Write")]
    internal static partial void Update(GroundPlaneState state, GroundPlane plane);

    [UserMapping]
    private static double Degrees(NorthAngle rotation) => rotation;
}
