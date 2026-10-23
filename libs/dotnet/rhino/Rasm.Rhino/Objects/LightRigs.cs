using System.Globalization;
using LanguageExt.ClassInstances;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Wacton.Unicolour;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Front", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Azimuth : System.Numerics.IMinMaxValue<Azimuth> {
    public static Azimuth MinValue { get; } = new(-Math.PI);
    public static Azimuth MaxValue { get; } = new(Math.PI);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Horizon", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Elevation : System.Numerics.IMinMaxValue<Elevation> {
    public static Elevation MinValue { get; } = new(-Math.PI / 2d);
    public static Elevation MaxValue { get; } = new(Math.PI / 2d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RadiusMultiple : System.Numerics.IMinMaxValue<RadiusMultiple> {
    public static RadiusMultiple MinValue { get; } = new(double.BitIncrement(0d));
    public static RadiusMultiple MaxValue { get; } = new(double.MaxValue);
    public static RadiusMultiple Default { get; } = new(2.5);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct AngularDiameter : System.Numerics.IMinMaxValue<AngularDiameter> {
    public static AngularDiameter MinValue { get; } = new(0d);
    public static AngularDiameter MaxValue { get; } = new(Math.PI / 2d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "AtKey", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LightStops : System.Numerics.IMinMaxValue<LightStops> {
    public static LightStops MinValue { get; } = new(-double.MaxValue);
    public static LightStops MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TargetRadius : System.Numerics.IMinMaxValue<TargetRadius> {
    public static TargetRadius MinValue { get; } = new(double.BitIncrement(0d));
    public static TargetRadius MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct KeySoftness : System.Numerics.IMinMaxValue<KeySoftness> {
    public static KeySoftness MinValue { get; } = new(0d);
    public static KeySoftness MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ColorSaturation : System.Numerics.IMinMaxValue<ColorSaturation> {
    public static ColorSaturation MinValue { get; } = new(0d);
    public static ColorSaturation MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>(KeyMemberName = "Name", SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightRole {
    public static readonly LightRole Key = new("key");
    public static readonly LightRole Fill = new("fill");
    public static readonly LightRole Rim = new("rim");
    public static readonly LightRole Accent = new("accent");
    public static readonly LightRole Top = new("top");
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigEmitter {
    public sealed record PointLight() : RigEmitter;

    public sealed record SpotLight(SpotAngle Angle, HotSpot HotSpot) : RigEmitter;

    public sealed record DirectionalLight() : RigEmitter;

    public sealed record RectangularLight() : RigEmitter;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigColor {
    public sealed record Blackbody(ColorTemperature Kelvin) : RigColor;

    public sealed record Linear(ColorFilter Filter) : RigColor;
}

public sealed record RigLight(
    LightRole Role, RigEmitter Emitter, Azimuth Azimuth, Elevation Elevation, RadiusMultiple Distance,
    AngularDiameter Size, LightStops Stops, RigColor Color, Light.Attenuation Falloff) {
    public (Vector3d Toward, Vector3d Across, Vector3d Rise) Axes =>
        (Math.SinCos(Azimuth), Math.SinCos(Elevation)) switch {
            var ((sinA, cosA), (sinE, cosE)) => (new Vector3d(cosE * sinA, sinE, cosE * cosA), new Vector3d(cosA, 0d, -sinA), new Vector3d(-sinE * sinA, cosE, -sinE * cosA)),
        };

    public DistantLight Distant {
        get {
            Vector3d toward = Axes.Toward;
            Unicolour color = Color.Switch(
            blackbody: static blackbody => Adaptation.Blackbody(Gamut.StandardRgb, blackbody.Kelvin),
            linear: static linear => linear.Filter.Unicolour);
            Unicolour normalized = Adaptation.Peaked(color).MapToRgbGamut();
            (double red, double green, double blue) = normalized.RgbLinear;
            double luminance = normalized.Xyz.Y;
            return new DistantLight(
                new System.Numerics.Vector3((float)toward.X, (float)toward.Y, (float)toward.Z),
                new System.Numerics.Vector3((float)(red / luminance), (float)(green / luminance), (float)(blue / luminance)), Stops);
        }
    }
}

public sealed record MatchedLook(
    int Count, RigEmitter Key, KeySoftness Softness, AngularDiameter FillSize, Option<LightStops> Fill,
    Option<LightStops> Rim, ColorFilter Tint, ColorSaturation Saturation, RadiusMultiple Distance) {
    public Fin<Seq<RigLight>> Derived(LightingReading reading) {
        RgbLinear Color(System.Numerics.Vector3 color) =>
            new Unicolour(reading.Gamut.Configuration, ColourSpace.RgbLinear, color.X, color.Y, color.Z)
                .ConvertToConfiguration(Gamut.StandardRgb.Configuration) switch {
                    { RgbLinear: { R: 0d, G: 0d, B: 0d } } => new Unicolour(Gamut.StandardRgb.Configuration, ColourSpace.RgbLinear, 1d, 1d, 1d).RgbLinear,
                    var converted => Adaptation.Peaked(converted).MapToRgbGamut().RgbLinear,
                };
        double Luminance(System.Numerics.Vector3 color) =>
            Math.Max(LuminanceSamples.Luminance(color.X, color.Y, color.Z, reading.Gamut.Luminance), LuminanceSamples.Floor);
        return
        from azimuth in Count > 0 ? Fin.Succ(reading.Direction.X * Math.PI / 2d) : Error.New(ErrorCodes.ValidationFailed, "matched look needs lights")
        let elevation = reading.Direction.Y * Math.PI / 2d
        let size = reading.Hardness > 0f ? Math.Atan2(Softness * reading.Ratio, reading.Hardness) : (double)AngularDiameter.MaxValue
        let behind = reading.Backlit ? Math.PI : Math.IEEERemainder(azimuth + Math.PI, Math.Tau)
        let working = reading.Gels.Match(
            Some: static gels => (Key: gels.Key.Color, Fill: gels.Accent.Color, Rim: gels.Accent.Color),
            None: () => (Key: reading.KeyColor, Fill: reading.FillColor, Rim: reading.KeyColor))
        let colors = (Key: Color(working.Key), Fill: Color(working.Fill), Rim: Color(working.Rim))
        let keyed = reading.Gels.Map(static gels => gels.Key) | reading.Palette.Head
        from lights in (Seq(Row(LightRole.Key, Key, azimuth, elevation, size, LightStops.AtKey, colors.Key),
                    Row(LightRole.Fill, new RigEmitter.RectangularLight(), -azimuth, Elevation.Horizon, FillSize, Fill.Map(static stops => (double)stops).IfNone(-reading.Ratio), colors.Fill))
                + Rim.Map(rim => Row(LightRole.Rim, Key, behind, elevation, size, rim, colors.Rim)).ToSeq()
                + reading.Palette.Filter(entry => keyed != Some(entry)).Map(entry => Row(
                    LightRole.Accent, Key, ((2d * entry.Position.X) - 1d) * Math.PI / 2d, (1d - (2d * entry.Position.Y)) * Math.PI / 2d, size,
                    Math.Log2(Luminance(entry.Color)) - Math.Log2(Luminance(working.Key)), Color(entry.Color))))
            .Take(Count)
            .Traverse(static light => light)
            .As()
        select lights;
    }

    private Fin<RigLight> Row(LightRole role, RigEmitter emitter, double azimuth, double elevation, double size, double stops, RgbLinear color) =>
        (Azimuth.Validated(azimuth),
         Elevation.Validated(elevation),
         AngularDiameter.Validated(size),
         LightStops.Validated(stops),
         ValidationResults.ToFin(ColorFilter.Validate(
             Tint.Red * (1d + (Saturation * (color.R - 1d))),
             Tint.Green * (1d + (Saturation * (color.G - 1d))),
             Tint.Blue * (1d + (Saturation * (color.B - 1d))), out ColorFilter? filter), filter))
            .Apply((a, e, s, st, filter) => new RigLight(role, emitter, a, e, Distance, s, st, filter, Light.Attenuation.InverseSquared))
            .As();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigLayout {
    public sealed record Arranged(Seq<RigLight> Rows) : RigLayout;

    public sealed record Matched(MatchedLook Look) : RigLayout;

    public Fin<Seq<RigLight>> Lights(Option<LightingReading> reading) =>
        Switch(
            reading,
            arranged: static (_, arranged) => arranged.Rows,
            matched: static (held, matched) => held.ToFin(new Refused(nameof(LightingReading), Refused.Absent)).Bind(matched.Look.Derived));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigTarget {
    public sealed record Subjects(IterableNE<Guid> Ids) : RigTarget;

    public sealed record Fixed(Point3d Focus, TargetRadius Radius) : RigTarget;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigBasis {
    public sealed record World() : RigBasis;

    public sealed record View(ViewportTarget Target) : RigBasis;

    public sealed record Frame(Guid Object) : RigBasis;
}

public sealed record LightRig(
    Guid Id, RigTarget Target, RigBasis Basis, LightIntensity Level, Azimuth Rotation, Elevation Elevation, RigLayout Layout, Option<string> Preset);

public sealed record RigCommit(Func<LightRole, string> Caption, string Name, RedrawPolicy Redraw);

public sealed record PlacedLight(LightRole Role, LightShape Shape, LightColor Color, LightIntensity Intensity, Vector3d AttenuationVector) {
    public LightSpec Spec(string name, bool enabled, LightPower watts, ShadowIntensity shadow) =>
        new(Conversions.Present(name), enabled, Shape, Color, Intensity, watts, shadow, AttenuationVector);
}

public sealed record RigLabel(Guid Rig, int Slot, LightRole Role, Option<string> Preset) {
    public static HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> Strings(Option<RigLabel> label) =>
        HashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(
            (LightKey.Rig.Key, label.Map(static row => row.Rig.ToString("D"))),
            (LightKey.Slot.Key, label.Map(static row => row.Slot.ToString(CultureInfo.InvariantCulture))),
            (LightKey.Role.Key, label.Map(static row => row.Role.Name)),
            (LightKey.Preset.Key, label.Bind(static row => row.Preset)));
}

public sealed record HeldLight(Guid Id, uint Serial, HashMap<EqStringOrdinalIgnoreCase, string, string> Strings) {
    public Option<int> Slot =>
        Strings.Find(LightKey.Slot.Key).Bind(static text => Invariant.Number<int>(text, NumberStyles.None));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigChange {
    public sealed record Create(RigLabel Label, PlacedLight Placed, Option<Guid> Source) : RigChange;

    public sealed record Place(Guid Light, PlacedLight Placed, Option<RigLabel> Label) : RigChange;

    public sealed record Release(Guid Light) : RigChange;

    public sealed record Remove(Guid Light) : RigChange;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LightRigs {
    // --- [TARGET]
    public static IO<RigTarget.Fixed> Target(RhinoDoc doc, RigTarget target) =>
        target.Switch(
            doc,
            subjects: static (document, subjects) => from found in new ObjectTarget.Ids(subjects.Ids.AsIterable().ToSeq()).Objects(document)
                                                     let bounds = Measurements.Bounds(found.Map(static held => held.Geometry.GetBoundingBox(accurate: true)))
                                                     from box in IO.lift(bounds.ToFin(new Refused(nameof(GeometryBase.GetBoundingBox), Refused.Absent)))
                                                     from radius in IO.lift(TargetRadius.Validated(box.Diagonal.Length / 2d))
                                                     select new RigTarget.Fixed(box.Center, radius),
            @fixed: static (_, held) => IO.pure(held));

    public static IO<Plane> Basis(RhinoDoc doc, RigBasis basis, Point3d focus) =>
        basis.Switch(
            (Document: doc, Focus: focus),
            world: static (state, _) => IO.pure(new Plane(state.Focus, Vector3d.XAxis, Vector3d.ZAxis)),
            view: static (state, view) =>
                DisposalOps.Within(
                    (Action<IDisposable> held) => Viewports.ResolveViewport(state.Document, view.Target, held).Bind(static row => Cameras.ReadPose(row.Viewport)),
                    DisposalOps.Release)
                    .Map(pose => new Plane(state.Focus, pose.Frame.XAxis, pose.Frame.YAxis)),
            frame: static (state, frame) => IO.lift(() => Refused.UnlessFound(state.Document.Objects.FindId(frame.Object), nameof(ObjectTable.FindId)))
                .Bind(static found => IO.lift(() => Conversions.Present(found.ObjectFrame()).ToFin(new Refused(nameof(RhinoObject.ObjectFrame), Refused.Invalid))))
                .Map(axes => new Plane(state.Focus, axes.XAxis, axes.ZAxis)));

    // --- [PLACEMENT]
    public static Fin<Seq<PlacedLight>> Placed(LightRig rig, Seq<RigLight> lights, RigTarget.Fixed target, Plane basis, LengthUnit modelUnit) =>
        from placing in Fin.Succ(Transform.Rotation(rig.Rotation, basis.YAxis, basis.Origin)
            * Transform.Rotation(-(double)rig.Elevation, basis.XAxis, basis.Origin)
            * Transform.PlaneToPlane(Plane.WorldXY, basis))
        from placed in lights.Traverse(light => Positioned(rig, light, target, placing, modelUnit)).As()
        select placed;

    private static Fin<PlacedLight> Positioned(LightRig rig, RigLight light, RigTarget.Fixed target, Transform placing, LengthUnit modelUnit) =>
        from reach in TargetRadius.Validated(light.Distance * target.Radius)
        let axes = light.Axes
        let radius = reach * Math.Tan(light.Size / 2d)
        let side = 2d * radius
        let at = placing * new Point3d(reach * axes.Toward)
        let meters = Quantities.From(reach, modelUnit).Meters.ToDouble()
        let falloff = light.Falloff switch {
            Light.Attenuation.Constant => (Gain: 1d, Vector: Light.ConstantAttenuationVector),
            Light.Attenuation.Linear => (Gain: meters, Vector: Light.LinearAttenuationVector),
            Light.Attenuation.InverseSquared => (Gain: meters * meters, Vector: Light.InverseSquaredAttenuationVector),
        }
        let shaped = light.Emitter.Switch(
            (At: at, Aim: target.Focus - at, Radius: radius, Side: side, Size: (double)light.Size, falloff.Gain, Across: side * (placing * axes.Across), Rise: side * (placing * axes.Rise)),
            pointLight: static (state, _) => (
                Shape: EmitterRadius.Validated(state.Radius)
                    .Map(emitter => (LightShape)new LightShape.PointLight(state.At, emitter, CameraRelative: false)),
                 state.Gain),
            spotLight: static (state, spot) => (
                Shape: EmitterRadius.Validated(state.Radius)
                    .Map(emitter => (LightShape)new LightShape.SpotLight(state.At, state.Aim, spot.Angle, spot.HotSpot, emitter, CameraRelative: false)),
                 state.Gain),
            directionalLight: static (state, _) => (
                Shape: SunAngle.Validated(state.Size)
                    .Map(angle => (LightShape)new LightShape.DirectionalLight(state.At, state.Aim, angle, CameraRelative: false)),
                Gain: 1d),
            rectangularLight: static (state, _) => (
                Shape: double.IsFinite(state.Side)
                    ? new LightShape.RectangularLight(state.At - (state.Across / 2d) - (state.Rise / 2d), state.Across, state.Rise, state.Aim)
                    : Error.New(ErrorCodes.ValidationFailed, "rectangular light side is not finite"),
                 state.Gain))
        from result in (shaped.Shape, LightIntensity.Validated(rig.Level == LightIntensity.MinValue ? 0d : rig.Level * double.Exp2(light.Stops) * shaped.Gain))
            .Apply((shape, intensity) => new PlacedLight(
                light.Role, shape, light.Color.Switch(blackbody: static blackbody => (LightColor)blackbody.Kelvin, linear: static linear => (LightColor)linear.Filter), intensity, falloff.Vector))
            .As()
        select result;

    // --- [RECONCILE]
    public static IO<Committed<Seq<RigChange>>> Reconcile(RhinoDoc doc, LightRig rig, Option<LightingReading> reading, RigCommit commit, IPlugInSink sink) =>
        Commits.Commit(doc, RowText.Localize(commit.Name, sink), commit.Redraw,
            from lights in IO.lift(rig.Layout.Lights(reading))
            from target in Target(doc, rig.Target)
            from basis in Basis(doc, rig.Basis, target.Focus)
            from placed in IO.lift(() => Placed(rig, lights, target, basis, doc.ModelUnits))
            from held in Held(doc, rig.Id)
            let changes = Plan(rig, held, placed)
            from applied in Apply(doc, changes, role => RowText.Localize(commit.Caption(role), sink))
            select changes);

    public static IO<Seq<HeldLight>> Held(RhinoDoc doc, Guid rig) =>
        IO.lift(() => Conversions.Rows(doc.Lights).Filter(static light => !light.IsDeleted)
            .Map(static row => new HeldLight(row.Id, row.RuntimeSerialNumber, UserStrings.Held(row.Attributes.GetUserStrings())))
            .Filter(light => light.Strings.Find(LightKey.Rig.Key) == Some(rig.ToString("D"))).Strict());

    public static Seq<RigChange> Plan(LightRig rig, Seq<HeldLight> held, Seq<PlacedLight> placed) =>
        toSeq(held.OrderBy(static light => light.Serial)).Fold(
            (Members: Map<int, HeldLight>(), Released: Seq<HeldLight>()),
            static (state, light) => light.Slot.Filter(slot => !state.Members.ContainsKey(slot)).Match(
                Some: slot => (state.Members.Add(slot, light), state.Released),
                None: () => (state.Members, state.Released.Add(light)))) switch {
                    var (members, released) =>
                        (from row in placed.Map((placement, slot) => (Placement: placement, Label: new RigLabel(rig.Id, slot, placement.Role, rig.Preset)))
                         let member = members.Find(row.Label.Slot)
                         let labelled = member.Exists(light => RigLabel.Strings(Some(row.Label)).ForAll((key, value) => light.Strings.Find(key) == value))
                         select member.Match(
                             Some: light => (RigChange)new RigChange.Place(light.Id, row.Placement, Callbacks.Found(!labelled, row.Label)),
                             None: () => new RigChange.Create(row.Label, row.Placement, members.Min.Map(static lowest => lowest.Value.Id))))
                        + toSeq(members.Filter((slot, _) => slot >= placed.Count).Values).Map(static member => (RigChange)new RigChange.Remove(member.Id))
                        + released.Map(static light => (RigChange)new RigChange.Release(light.Id)),
                };

    private static IO<Unit> Apply(RhinoDoc doc, Seq<RigChange> changes, Func<LightRole, string> caption) =>
        from created in changes.TraverseM(change => change.Switch(
                (Document: doc, Caption: caption),
                create: static (state, create) => Created(state.Document, create, state.Caption),
                place: static (state, place) => Moved(state.Document, place, state.Caption),
                release: static (state, release) => Labelled(state.Document, release.Light, None).Map(Seq<Guid>()),
                remove: static (state, remove) =>
                    Lights.Apply(state.Document, new LightOp.Delete(new ComponentRef<LightObject>.ById(remove.Light), Quiet: true)).Map(Seq<Guid>()))).As()
        let fresh = created.Flatten()
        from grouped in unless(fresh.IsEmpty, Lights.Apply(doc, new LightOp.Grouping(
            fresh.Map<ComponentRef<LightObject>>(static id => new ComponentRef<LightObject>.ById(id)), On: true)).Map(unit)).As()
        select unit;

    private static IO<Seq<Guid>> Created(RhinoDoc doc, RigChange.Create create, Func<LightRole, string> caption) =>
        from source in create.Source.Traverse(member => IO.lift(() => Refused.UnlessFound(doc.Objects.FindId(member), nameof(ObjectTable.FindId)))).As()
        let joined = source.ToSeq().Bind(static found => Seq(
            AttributeEdits.LayerIndex(new LayerRef.Row(new ComponentRef<Layer>.ByIndex(found.Attributes.LayerIndex))),
            AttributeEdits.Groups(new RowsEdit<ComponentRef<Group>, ComponentRef<Group>>.Add(
                Conversions.Rows(found.Attributes.GetGroupList()).Map<ComponentRef<Group>>(static index => new ComponentRef<Group>.ByIndex(index))))))
        from added in TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes => from edited in AttributeOps.Apply(doc, attributes, joined.Add(AttributeEdits.SetUserStrings(RigLabel.Strings(Some(create.Label)))))
                                                                                               from light in Lights.Apply(doc, new LightOp.Add(
                                                                                                   create.Placed.Spec(caption(create.Placed.Role), enabled: true, LightPower.Default, ShadowIntensity.Default), Some(attributes)))
                                                                                               select light)
        select source.IsNone ? added : Seq<Guid>();

    private static IO<Seq<Guid>> Moved(RhinoDoc doc, RigChange.Place place, Func<LightRole, string> caption) =>
        new ComponentRef<LightObject>.ById(place.Light) switch {
            var address =>
                from held in Lights.Read(doc, address)
                from moved in Lights.Apply(doc, new LightOp.Modify(
                    address, place.Placed.Spec(caption(place.Placed.Role), held.Spec.Enabled, held.Spec.Watts, held.Spec.ShadowIntensity)))
                from labelled in place.Label.Traverse(label => Labelled(doc, place.Light, Some(label))).As()
                select Seq<Guid>(),
        };

    private static IO<Seq<Guid>> Labelled(RhinoDoc doc, Guid light, Option<RigLabel> label) =>
        TableOps.Apply(doc, AttributeOps.Modify(doc, new ObjectTarget.Ids(Seq(light)), Seq(AttributeEdits.SetUserStrings(RigLabel.Strings(label))), quiet: true));

    // --- [FOLLOW]
    public static IO<Unit> Follow(RhinoDoc doc, LightRig rig, Option<LightingReading> reading, RigCommit commit, IPlugInSink sink, Action<IDisposable> retain) =>
        rig.Target.Switch(
            (Document: doc, Rig: rig, Reading: reading, Commit: commit, Sink: sink, Retain: retain),
            subjects: static (state, subjects) => from initial in Target(state.Document, subjects)
                                                  from acquisition in IO.lift(() => Watching(state, subjects, initial))
                                                  from followed in acquisition
                                                  select followed,
            @fixed: static (_, _) => IO.pure(unit));

    private static IO<Unit> Watching(
        (RhinoDoc Document, LightRig Rig, Option<LightingReading> Reading, RigCommit Commit, IPlugInSink Sink, Action<IDisposable> Retain) state, RigTarget.Subjects subjects, RigTarget.Fixed initial) {
        Atom<RigTarget.Fixed> previous = Atom(initial);
        LanguageExt.HashSet<Guid> ids = toHashSet(subjects.Ids);
        uint serial = state.Document.RuntimeSerialNumber;
        CallbackSite site = new(state.Sink, typeof(LightRigs), nameof(Follow));
        Seq<HostEvent<(Unit, Unit)>> events = Seq(
            EventKind.ReplaceRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
            EventKind.DeleteRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
            EventKind.UndeleteRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
            EventKind.TransformObjects.In(serial).Choose(args => Callbacks.Found(args.ObjectIds.Exists(ids.Contains) || args.GripOwnerIds.Exists(ids.Contains), (unit, unit))));
        IO<Unit> Resync(HashMap<Unit, Unit> _) =>
            from target in Target(state.Document, state.Rig.Target)
            from moved in IO.lift(() => previous.Value != target)
            from changed in when(moved, Reconcile(state.Document, state.Rig with { Target = target }, state.Reading, state.Commit, state.Sink)
                .Bind(_ => previous.SwapIO(_ => target)).Map(unit)).As()
            select unit;
        return DisposalOps.Acquire(
            owned => from mailbox in Subscriptions.Idle<Unit, Unit>(Resync)(site, owned)
                     from watched in events.TraverseM(row => row.Inline(mailbox, state.Sink, owned)).As()
                     select unit,
            state.Retain);
    }
}
