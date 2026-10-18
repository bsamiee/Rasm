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
                   state.Altitude.Traverse(height => Model(height, scope.Units, GroundPlaneParameter.Height)).As(),
                   Model(state.TextureOffsetX, scope.Units, GroundPlaneParameter.Offset), Model(state.TextureOffsetY, scope.Units, GroundPlaneParameter.Offset),
                   Model(state.TextureSizeX, scope.Units, GroundPlaneParameter.Size), Model(state.TextureSizeY, scope.Units, GroundPlaneParameter.Size))
                   .Apply(static (altitude, x, y, width, height) => (Altitude: altitude, Offset: new Vector2d(x, y), Size: new Vector2d(width, height))).As())
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
                (Callbacks.Found(!plane.AutoAltitude, plane.Altitude).Traverse(height => Measured(height, units, GroundPlaneParameter.Height)).As(),
                 Measured(offset.X, units, GroundPlaneParameter.Offset), Measured(offset.Y, units, GroundPlaneParameter.Offset),
                 Measured(size.X, units, GroundPlaneParameter.Size), Measured(size.Y, units, GroundPlaneParameter.Size),
                 NorthAngle.Validated(plane.TextureRotation))
                    .Apply((altitude, offsetX, offsetY, sizeX, sizeY, rotation) => new GroundPlaneState(
                        plane.Enabled, plane.ShowUnderside, altitude, plane.ShadowOnly, plane.TextureOffsetLocked, offsetX, offsetY,
                        plane.TextureSizeLocked, sizeX, sizeY, rotation, Material(plane.MaterialInstanceId)))
                    .As(),
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

    private static Fin<Length> Measured(double value, LengthUnit units, NumericDomain<Length, double> domain) =>
        domain.Checked(Quantities.From(value, units));

    private static Option<Guid> Material(Guid id) => Conversions.Present(id).Filter(static held => held != ContentUuids.DefaultMaterialInstance);

    private static Fin<double> Model(Length length, LengthUnit units, NumericDomain<Length, double> domain) =>
        domain.Checked(length).Map(held => Quantities.As(held, units));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GroundPlaneParameter : IStateParameter<GroundPlaneState> {
    internal static readonly NumericDomain<Length, double> Height = NumericDomain.Meters((double.MinValue, double.MaxValue), new() { Form = NumberForm.Field, Origin = 0d });
    internal static readonly NumericDomain<Length, double> Offset = NumericDomain.Meters((double.MinValue, double.MaxValue), new() { Soft = (-1d, 1d), Step = 0.01d, Decimals = 2 });
    internal static readonly NumericDomain<Length, double> Size = NumericDomain.Meters((0d, double.MaxValue), new() { Soft = (0.01d, 10d), Step = 0.01d, Decimals = 2 });

    public static readonly GroundPlaneParameter Enabled = new("enabled", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })),
        new("Show ground plane", "Renders the ground plane"));
    public static readonly GroundPlaneParameter ShowUnderside = new("show-underside", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.ShowUnderside, static on => state => state with { ShowUnderside = on })),
        new("Show underside", "Renders the plane's underside, off renders it single-sided"));
    public static readonly GroundPlaneParameter Altitude = new("altitude", new StateParameter<GroundPlaneState>.OptionalNumeric<Length, double>(Lens<GroundPlaneState, Gated<Length>>.New(
        static state => state.Altitude.Match(Some: static height => new Gated<Length>(Enabled: true, height), None: static () => new Gated<Length>(Enabled: false, Length.Zero)),
        static gate => state => state with { Altitude = gate.Active }),
        Height),
        new("Height above world XY", "Fixed height of the plane, off places it at the automatic height below the lowest object"));
    public static readonly GroundPlaneParameter ShadowOnly = new("shadow-only", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.ShadowOnly, static on => state => state with { ShadowOnly = on })),
        new("Show shadows only", "Catches shadows alone, off renders the plane's material"));
    public static readonly GroundPlaneParameter TextureOffsetLocked = new("texture-offset-locked", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.TextureOffsetLocked, static locked => state => state with { TextureOffsetLocked = locked })),
        new("Lock offset", "Moves both offset axes together"));
    public static readonly GroundPlaneParameter TextureOffsetX = new("texture-offset-x", new StateParameter<GroundPlaneState>.Numeric<Length, double>(Lens<GroundPlaneState, Length>.New(static state => state.TextureOffsetX, static offset => state => state with { TextureOffsetX = offset }), Offset),
        new("Offset X", "Texture offset along world X"));
    public static readonly GroundPlaneParameter TextureOffsetY = new("texture-offset-y", new StateParameter<GroundPlaneState>.Numeric<Length, double>(Lens<GroundPlaneState, Length>.New(static state => state.TextureOffsetY, static offset => state => state with { TextureOffsetY = offset }), Offset),
        new("Offset Y", "Texture offset along world Y"));
    public static readonly GroundPlaneParameter TextureSizeLocked = new("texture-size-locked", new StateParameter<GroundPlaneState>.Toggle(
        Lens<GroundPlaneState, bool>.New(static state => state.TextureSizeLocked, static locked => state => state with { TextureSizeLocked = locked })),
        new("Lock size", "Scales both size axes together"));
    public static readonly GroundPlaneParameter TextureSizeX = new("texture-size-x", new StateParameter<GroundPlaneState>.Numeric<Length, double>(Lens<GroundPlaneState, Length>.New(static state => state.TextureSizeX, static size => state => state with { TextureSizeX = size }), Size),
        new("Size X", "Texture repeat size along world X"));
    public static readonly GroundPlaneParameter TextureSizeY = new("texture-size-y", new StateParameter<GroundPlaneState>.Numeric<Length, double>(Lens<GroundPlaneState, Length>.New(static state => state.TextureSizeY, static size => state => state with { TextureSizeY = size }), Size),
        new("Size Y", "Texture repeat size along world Y"));
    public static readonly GroundPlaneParameter TextureRotation = new("texture-rotation", StateParameter.Bounded(Lens<GroundPlaneState, NorthAngle>.New(static state => state.TextureRotation, static rotation => state => state with { TextureRotation = rotation }),
        new Presentation<double> { Unit = Quantity.GetUnitInfo(UnitsNet.Units.AngleUnit.Degree), Step = 1d, Decimals = 0 }),
        new("Rotation", "Texture rotation about the world origin plus the offset"));
    public static readonly GroundPlaneParameter MaterialInstanceId = new("material-instance-id", new StateParameter<GroundPlaneState>.OptionalRaw<Guid>(
        Lens<GroundPlaneState, Option<Guid>>.New(static state => state.MaterialInstanceId, static material => state => state with { MaterialInstanceId = material })),
        new("Material", "The render material instance assigned to the ground plane"));

    public StateParameter<GroundPlaneState> Kind { get; }

    public ParameterText Text { get; }
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
