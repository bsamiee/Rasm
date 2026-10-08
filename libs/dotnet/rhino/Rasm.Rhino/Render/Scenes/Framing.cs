using System.Drawing;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Views;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Scenes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct FrameFraction : System.Numerics.IMinMaxValue<FrameFraction> {
    public static FrameFraction MinValue { get; } = new(0d);
    public static FrameFraction MaxValue { get; } = new(1d);
    public static FrameFraction ActionArea { get; } = new(0.9d);
    public static FrameFraction TitleArea { get; } = new(0.8d);
    public static Presentation<FrameFraction, double> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record SafeFrameState(
    bool Enabled, bool PerspectiveOnly, bool FieldsOn, bool LiveFrameOn,
    bool ActionFrameOn, bool ActionFrameLinked, FrameFraction ActionFrameXScale, FrameFraction ActionFrameYScale,
    bool TitleFrameOn, bool TitleFrameLinked, FrameFraction TitleFrameXScale, FrameFraction TitleFrameYScale)
    : ISceneRecord<SafeFrameState, SafeFrameParameter> {
    public static SafeFrameState Default { get; } = new(
        Enabled: false, PerspectiveOnly: true, FieldsOn: false, LiveFrameOn: true,
        ActionFrameOn: true, ActionFrameLinked: true, ActionFrameXScale: FrameFraction.ActionArea, ActionFrameYScale: FrameFraction.ActionArea,
        TitleFrameOn: true, TitleFrameLinked: true, TitleFrameXScale: FrameFraction.TitleArea, TitleFrameYScale: FrameFraction.TitleArea);

    public static string Owner => "safe-frame";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.SafeFrameChanged;

    public static IO<SafeFrameState> Read(SceneWindow window) =>
        Sources.SubOwner(window, static settings => settings.SafeFrame, static frame => IO.lift(() =>
            (Conversions.Validated<FrameFraction, double, InvalidRhinoValue>(frame.ActionFrameXScale).ToValidation(),
             Conversions.Validated<FrameFraction, double, InvalidRhinoValue>(frame.ActionFrameYScale).ToValidation(),
             Conversions.Validated<FrameFraction, double, InvalidRhinoValue>(frame.TitleFrameXScale).ToValidation(),
             Conversions.Validated<FrameFraction, double, InvalidRhinoValue>(frame.TitleFrameYScale).ToValidation())
                .Apply((actionX, actionY, titleX, titleY) => new SafeFrameState(
                    frame.Enabled, frame.PerspectiveOnly, frame.FieldsOn, frame.LiveFrameOn,
                    frame.ActionFrameOn, frame.ActionFrameLinked, actionX, actionY,
                    frame.TitleFrameOn, frame.TitleFrameLinked, titleX, titleY))
                .As()
                .ToFin()));

    public static IO<Unit> Write(SceneWindow window, SafeFrameState state) =>
        Sources.SubOwner(window, static settings => settings.SafeFrame, frame => IO.lift(() => SafeFrameMapper.Update(state, frame)));

    public static ParameterText Text(SafeFrameParameter parameter) =>
        parameter.Map(
            enabled: ParameterText.Of("Show in active view", "Draws the safe frame in the active view"),
            perspectiveOnly: ParameterText.Of("Perspective views only", "Draws the safe frame in perspective views alone"),
            fieldsOn: ParameterText.Of("Show 4x3 field grid", "Draws the 4 by 3 field grid"),
            liveFrameOn: ParameterText.Of("Live area", "Draws the live area the rendering covers"),
            actionFrameOn: ParameterText.Of("Action area", "Draws the action safe area"),
            actionFrameLinked: ParameterText.Of("Action area linked", "Scales the action area alike on both axes"),
            actionFrameXScale: ParameterText.Of("Action frame X", "Action area width as a share of the frame"),
            actionFrameYScale: ParameterText.Of("Action frame Y", "Action area height as a share of the frame"),
            titleFrameOn: ParameterText.Of("Title area", "Draws the title safe area"),
            titleFrameLinked: ParameterText.Of("Title area linked", "Scales the title area alike on both axes"),
            titleFrameXScale: ParameterText.Of("Title frame X", "Title area width as a share of the frame"),
            titleFrameYScale: ParameterText.Of("Title frame Y", "Title area height as a share of the frame"));

    public static RowRules Rules(RowSource<SafeFrameState> source, SafeFrameParameter parameter) =>
        RowRule.When(source, static state => state.Enabled, SafeFrameParameter.Enabled) switch {
            var on => (on & RowRule.When(source, static state => state.ActionFrameOn, SafeFrameParameter.ActionFrameOn),
                       on & RowRule.When(source, static state => state.TitleFrameOn, SafeFrameParameter.TitleFrameOn)) switch {
                           var (action, title) => new RowRules(parameter.Map<Option<RowRule>>(
                               enabled: None, perspectiveOnly: on, fieldsOn: on, liveFrameOn: on,
                               actionFrameOn: on, actionFrameLinked: action, actionFrameXScale: action,
                               actionFrameYScale: action & RowRule.When(source, static state => !state.ActionFrameLinked, SafeFrameParameter.ActionFrameLinked),
                               titleFrameOn: on, titleFrameLinked: title, titleFrameXScale: title,
                               titleFrameYScale: title & RowRule.When(source, static state => !state.TitleFrameLinked, SafeFrameParameter.TitleFrameLinked)), None),
                       },
        };

    public Option<Rectangle> Action(PixelExtent frame) =>
        Guide(frame, ActionFrameOn, ActionFrameXScale, ActionFrameLinked ? ActionFrameXScale : ActionFrameYScale);

    public Option<Rectangle> Title(PixelExtent frame) =>
        Guide(frame, TitleFrameOn, TitleFrameXScale, TitleFrameLinked ? TitleFrameXScale : TitleFrameYScale);

    private Option<Rectangle> Guide(PixelExtent frame, bool on, FrameFraction x, FrameFraction y) =>
        Enabled && on
            ? Some(Rectangle.FromLTRB(
                Framing.Edge(frame.Width, (1d - x) / 2d), Framing.Edge(frame.Height, (1d - y) / 2d),
                Framing.Edge(frame.Width, (1d + x) / 2d), Framing.Edge(frame.Height, (1d + y) / 2d)))
            : None;
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SafeFrameParameter : IStateParameter<SafeFrameState> {
    public static readonly SafeFrameParameter Enabled = new("on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })));
    public static readonly SafeFrameParameter PerspectiveOnly = new("perspective-only", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.PerspectiveOnly, static on => state => state with { PerspectiveOnly = on })));
    public static readonly SafeFrameParameter FieldsOn = new("field-display-on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.FieldsOn, static on => state => state with { FieldsOn = on })));
    public static readonly SafeFrameParameter LiveFrameOn = new("live-frame-on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.LiveFrameOn, static on => state => state with { LiveFrameOn = on })));
    public static readonly SafeFrameParameter ActionFrameOn = new("action-frame-on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.ActionFrameOn, static on => state => state with { ActionFrameOn = on })));
    public static readonly SafeFrameParameter ActionFrameLinked = new("action-frame-link", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.ActionFrameLinked, static linked => state => state with { ActionFrameLinked = linked })));
    public static readonly SafeFrameParameter ActionFrameXScale = new("action-frame-x-scale", new StateParameter<SafeFrameState>.Bounded<FrameFraction, double, InvalidRhinoValue>(
        Lens<SafeFrameState, FrameFraction>.New(static state => state.ActionFrameXScale, static scale => state => state with { ActionFrameXScale = scale }), FrameFraction.Presentation));
    public static readonly SafeFrameParameter ActionFrameYScale = new("action-frame-y-scale", new StateParameter<SafeFrameState>.Bounded<FrameFraction, double, InvalidRhinoValue>(
        Lens<SafeFrameState, FrameFraction>.New(static state => state.ActionFrameYScale, static scale => state => state with { ActionFrameYScale = scale }), FrameFraction.Presentation));
    public static readonly SafeFrameParameter TitleFrameOn = new("title-frame-on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.TitleFrameOn, static on => state => state with { TitleFrameOn = on })));
    public static readonly SafeFrameParameter TitleFrameLinked = new("title-frame-link", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.TitleFrameLinked, static linked => state => state with { TitleFrameLinked = linked })));
    public static readonly SafeFrameParameter TitleFrameXScale = new("title-frame-x-scale", new StateParameter<SafeFrameState>.Bounded<FrameFraction, double, InvalidRhinoValue>(
        Lens<SafeFrameState, FrameFraction>.New(static state => state.TitleFrameXScale, static scale => state => state with { TitleFrameXScale = scale }), FrameFraction.Presentation));
    public static readonly SafeFrameParameter TitleFrameYScale = new("title-frame-y-scale", new StateParameter<SafeFrameState>.Bounded<FrameFraction, double, InvalidRhinoValue>(
        Lens<SafeFrameState, FrameFraction>.New(static state => state.TitleFrameYScale, static scale => state => state with { TitleFrameYScale = scale }), FrameFraction.Presentation));

    public StateParameter<SafeFrameState> Kind { get; }
}

[Union]
public abstract partial record RenderSize {
    public abstract Fin<ImageOutputState> Applied(ImageOutputState held);

    public sealed record Viewport(Size ScreenPort) : RenderSize {
        public override Fin<ImageOutputState> Applied(ImageOutputState held) => Framing.Sized(held with { UseViewportSize = true }, ScreenPort);
    }

    public sealed record Custom : RenderSize {
        public override Fin<ImageOutputState> Applied(ImageOutputState held) => held with { UseViewportSize = false };
    }

    public sealed record Listed(Size Size) : RenderSize {
        public override Fin<ImageOutputState> Applied(ImageOutputState held) => Framing.Sized(held with { UseViewportSize = false }, Size);
    }
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class RenderRegion {
    public FrameFraction Left { get; }
    public FrameFraction Top { get; }
    public FrameFraction Right { get; }
    public FrameFraction Bottom { get; }

    public Rectangle Pixels(PixelExtent frame) =>
        Rectangle.FromLTRB(
            Framing.Edge(frame.Width, Left), Framing.Edge(frame.Height, Top),
            Framing.Edge(frame.Width, Right), Framing.Edge(frame.Height, Bottom));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref FrameFraction left, ref FrameFraction top, ref FrameFraction right, ref FrameFraction bottom) =>
        validationError = left < right && top < bottom ? null : new InvalidRhinoValue();
}

public sealed record OverscanFrame(PixelExtent Frame, Size Pad) {
    public Size Padded => new(Frame.Width + (2 * Pad.Width), Frame.Height + (2 * Pad.Height));

    public Rectangle DisplayWindow => new(0, 0, Frame.Width, Frame.Height);

    public Rectangle DataWindow => new(-Pad.Width, -Pad.Height, Padded.Width, Padded.Height);

    public double FrustumScale => Frame.Width >= Frame.Height ? (double)Padded.Width / Frame.Width : (double)Padded.Height / Frame.Height;

    public IO<Unit> Widen(ViewportInfo view) =>
        IO.lift(() =>
            Refused.Unless(view.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far), nameof(ViewportInfo.GetFrustum))
                .Bind(_ => Refused.Unless(
                    view.SetFrustum(left * FrustumScale, right * FrustumScale, bottom * FrustumScale, top * FrustumScale, near, far),
                    nameof(ViewportInfo.SetFrustum))));
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Overscan : System.Numerics.IMinMaxValue<Overscan> {
    public static Overscan MinValue { get; } = new(0d);
    public static Overscan MaxValue { get; } = new(0.5d);

    public OverscanFrame Around(PixelExtent frame) =>
        new(frame, new Size(Pad(frame.Width), Pad(frame.Height)));

    private int Pad(int side) => (int)Math.Round(side * _value, MidpointRounding.ToEven);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(EnabledConversions = MappingConversionType.Queryable | MappingConversionType.Enumerable | MappingConversionType.Dictionary
    | MappingConversionType.Span | MappingConversionType.Memory | MappingConversionType.EnumToEnum | MappingConversionType.ImplicitCast)]
internal static partial class SafeFrameMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Both)]
    internal static partial void Update(SafeFrameState state, SafeFrame frame);
}

public static class Framing {
    // --- [OUTPUT]
    public static Fin<PixelExtent> Final(ImageOutputState output, Size viewport) =>
        EffectPipeline.Extent(output.UseViewportSize ? viewport : new Size(output.ImageWidth, output.ImageHeight));

    public static (Length Width, Length Height) Print(ImageOutputState output, PixelExtent final) =>
        (Length.FromInches(final.Width / (double)output.ImageDpi), Length.FromInches(final.Height / (double)output.ImageDpi));

    public static Fin<ImageOutputState> Sized(ImageOutputState held, Size size) =>
        (Conversions.Validated<PixelDimension, int, InvalidRhinoValue>(size.Width).ToValidation(),
         Conversions.Validated<PixelDimension, int, InvalidRhinoValue>(size.Height).ToValidation())
            .Apply((width, height) => held with { ImageWidth = width, ImageHeight = height })
            .As()
            .ToFin();

    public static Fin<PixelDimension> AspectHeight(PixelDimension width, PixelExtent viewport) =>
        Conversions.Validated<PixelDimension, int, InvalidRhinoValue>((int)((width / (viewport.Width / (double)viewport.Height)) + 0.5d));

    // --- [EXTENT]
    public static ReadModel<PixelExtent> Extent { get; } = new(
        [ReadModel.Marks(EventKind.DocumentPropertiesChanged), ReadModel.Marks(EventKind.UndoRedo)],
        static doc => IO.lift(() => EffectPipeline.Extent(RenderPipeline.RenderSize(doc, fromRenderSources: true))));

    // --- [SIZES]
    public static Seq<RenderSize> Sizes(EditorState editor, Size screenPort) =>
        [new RenderSize.Viewport(screenPort), new RenderSize.Custom(), .. editor.CustomRenderSizes.Map(static size => new RenderSize.Listed(size))];

    public static IO<Unit> Choose(SceneSource.Section section, RenderSize size) =>
        from sized in Sources.Edit(section, window => ImageOutputState.Read(window).Bind(held => IO.lift(size.Applied(held))).Bind(next => ImageOutputState.Write(window, next)))
        from preset in ViewModels.Write(section.Host, Provider.RhinoSettings, data => IO.lift(() => data.CustomImageSizeIsPreset = size.Map(viewport: false, custom: false, listed: true)))
        select unit;

    // --- [ROUNDING]
    internal static int Edge(int side, double fraction) => (int)((float)fraction * side);
}
