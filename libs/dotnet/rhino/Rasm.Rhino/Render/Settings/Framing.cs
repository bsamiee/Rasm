using System.Drawing;
using PeterO.Numbers;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FrameFraction : IPresented<FrameFraction, double> {
    public static FrameFraction MinValue { get; } = new(0d);
    public static FrameFraction MaxValue { get; } = new(1d);
    public static FrameFraction ActionArea { get; } = new(0.9d);
    public static FrameFraction TitleArea { get; } = new(0.8d);
    public static Presentation<double> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record SafeFrameState(
    bool Enabled, bool PerspectiveOnly, bool FieldsOn, bool LiveFrameOn,
    bool ActionFrameOn, bool ActionFrameLinked, FrameFraction ActionFrameXScale, FrameFraction ActionFrameYScale,
    bool TitleFrameOn, bool TitleFrameLinked, FrameFraction TitleFrameXScale, FrameFraction TitleFrameYScale)
    : IRenderSettingsRecord<SafeFrameState, SafeFrameParameter> {
    public static SafeFrameState Default { get; } = new(
        Enabled: false, PerspectiveOnly: true, FieldsOn: false, LiveFrameOn: true,
        ActionFrameOn: true, ActionFrameLinked: true, ActionFrameXScale: FrameFraction.ActionArea, ActionFrameYScale: FrameFraction.ActionArea,
        TitleFrameOn: true, TitleFrameLinked: true, TitleFrameXScale: FrameFraction.TitleArea, TitleFrameYScale: FrameFraction.TitleArea);

    public static string Owner => "safe-frame";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.SafeFrameChanged;

    public static IO<SafeFrameState> Read(RenderSettingsScope scope) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.SafeFrame, static frame => IO.lift(() =>
            (FrameFraction.Validated(frame.ActionFrameXScale).ToValidation(),
             FrameFraction.Validated(frame.ActionFrameYScale).ToValidation(),
             FrameFraction.Validated(frame.TitleFrameXScale).ToValidation(),
             FrameFraction.Validated(frame.TitleFrameYScale).ToValidation())
                .Apply((actionX, actionY, titleX, titleY) => new SafeFrameState(
                    frame.Enabled, frame.PerspectiveOnly, frame.FieldsOn, frame.LiveFrameOn,
                    frame.ActionFrameOn, frame.ActionFrameLinked, actionX, actionY,
                    frame.TitleFrameOn, frame.TitleFrameLinked, titleX, titleY))
                .As()
                .ToFin()));

    public static IO<SafeFrameState> Write(RenderSettingsScope scope, SafeFrameState state) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.SafeFrame, frame => IO.lift(() => { SafeFrameMapper.Update(state, frame); return state; }));

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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SafeFrameParameter : IStateParameter<SafeFrameState> {
    public static readonly SafeFrameParameter Enabled = new("on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })),
        new("Show in active view", "Draws the safe frame in the active view"));
    public static readonly SafeFrameParameter PerspectiveOnly = new("perspective-only", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.PerspectiveOnly, static on => state => state with { PerspectiveOnly = on })),
        new("Perspective views only", "Draws the safe frame in perspective views alone"));
    public static readonly SafeFrameParameter FieldsOn = new("field-display-on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.FieldsOn, static on => state => state with { FieldsOn = on })),
        new("Show 4x3 field grid", "Draws the 4 by 3 field grid"));
    public static readonly SafeFrameParameter LiveFrameOn = new("live-frame-on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.LiveFrameOn, static on => state => state with { LiveFrameOn = on })),
        new("Live area", "Draws the live area the rendering covers"));
    public static readonly SafeFrameParameter ActionFrameOn = new("action-frame-on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.ActionFrameOn, static on => state => state with { ActionFrameOn = on })),
        new("Action area", "Draws the action safe area"));
    public static readonly SafeFrameParameter ActionFrameLinked = new("action-frame-link", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.ActionFrameLinked, static linked => state => state with { ActionFrameLinked = linked })),
        new("Action area linked", "Scales the action area alike on both axes"));
    public static readonly SafeFrameParameter ActionFrameXScale = new("action-frame-x-scale", StateParameter.Bounded(Lens<SafeFrameState, FrameFraction>.New(static state => state.ActionFrameXScale, static scale => state => state with { ActionFrameXScale = scale })),
        new("Action frame X", "Action area width as a share of the frame"));
    public static readonly SafeFrameParameter ActionFrameYScale = new("action-frame-y-scale", StateParameter.Bounded(Lens<SafeFrameState, FrameFraction>.New(static state => state.ActionFrameYScale, static scale => state => state with { ActionFrameYScale = scale })),
        new("Action frame Y", "Action area height as a share of the frame"));
    public static readonly SafeFrameParameter TitleFrameOn = new("title-frame-on", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.TitleFrameOn, static on => state => state with { TitleFrameOn = on })),
        new("Title area", "Draws the title safe area"));
    public static readonly SafeFrameParameter TitleFrameLinked = new("title-frame-link", new StateParameter<SafeFrameState>.Toggle(
        Lens<SafeFrameState, bool>.New(static state => state.TitleFrameLinked, static linked => state => state with { TitleFrameLinked = linked })),
        new("Title area linked", "Scales the title area alike on both axes"));
    public static readonly SafeFrameParameter TitleFrameXScale = new("title-frame-x-scale", StateParameter.Bounded(Lens<SafeFrameState, FrameFraction>.New(static state => state.TitleFrameXScale, static scale => state => state with { TitleFrameXScale = scale })),
        new("Title frame X", "Title area width as a share of the frame"));
    public static readonly SafeFrameParameter TitleFrameYScale = new("title-frame-y-scale", StateParameter.Bounded(Lens<SafeFrameState, FrameFraction>.New(static state => state.TitleFrameYScale, static scale => state => state with { TitleFrameYScale = scale })),
        new("Title frame Y", "Title area height as a share of the frame"));

    public StateParameter<SafeFrameState> Kind { get; }

    public ParameterText Text { get; }
}

[Union]
public abstract partial record RenderSize {
    public abstract Fin<ImageOutputState> Applied(ImageOutputState held);

    public sealed record ViewportSize(Size ScreenPort) : RenderSize {
        public override Fin<ImageOutputState> Applied(ImageOutputState held) => Framing.Sized(held with { UseViewportSize = true }, ScreenPort);
    }

    public sealed record CustomSize : RenderSize {
        public override Fin<ImageOutputState> Applied(ImageOutputState held) => held with { UseViewportSize = false };
    }

    public sealed record Listed(Size Size) : RenderSize {
        public override Fin<ImageOutputState> Applied(ImageOutputState held) => Framing.Sized(held with { UseViewportSize = false }, Size);
    }
}

[ComplexValueObject]
public sealed partial class RenderRegion {
    public FrameFraction Left { get; }
    public FrameFraction Top { get; }
    public FrameFraction Right { get; }
    public FrameFraction Bottom { get; }

    public Rectangle Pixels(PixelExtent frame) =>
        Rectangle.FromLTRB(
            Framing.Edge(frame.Width, Left), Framing.Edge(frame.Height, Top),
            Framing.Edge(frame.Width, Right), Framing.Edge(frame.Height, Bottom));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FrameFraction left, ref FrameFraction top, ref FrameFraction right, ref FrameFraction bottom) =>
        validationError = left < right && top < bottom ? null : new ValidationError("frame left lies at or right of its right, or top at or below its bottom");
}

public sealed class OverscanFrame {
    private OverscanFrame(PixelExtent frame, PixelExtent padded) => (Frame, Padded) = (frame, padded);

    public PixelExtent Frame { get; }
    public PixelExtent Padded { get; }
    public Size Pad => new((Padded.Width - Frame.Width) / 2, (Padded.Height - Frame.Height) / 2);

    public Rectangle DisplayWindow => new(0, 0, Frame.Width, Frame.Height);

    public Rectangle DataWindow => new(-Pad.Width, -Pad.Height, Padded.Width, Padded.Height);

    public static Fin<OverscanFrame> Around(PixelExtent frame, Overscan overscan) {
        EFloat fraction = EFloat.FromDouble(overscan);
        return PixelExtent.Validated(Dimension(frame.Width), Dimension(frame.Height)).Map(padded => new OverscanFrame(frame, padded));

        int Dimension(int side) =>
            fraction.Multiply(side).RoundToIntegerNoRoundedFlag(EContext.UnlimitedHalfEven).ToEInteger().Multiply(2).Add(side) switch {
                var held => held.CanFitInInt32() ? held.ToInt32Unchecked() : int.MaxValue,
            };
    }

    public IO<Unit> Widen(ViewportInfo view) =>
        IO.lift(() => {
            view.FrustumAspect = (float)Frame.Width / Frame.Height;
            return Refused.Unless(view.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far), nameof(ViewportInfo.GetFrustum))
                .Bind(_ => {
                    double x = (right - left) * (Pad.Width / (double)Frame.Width);
                    double y = (top - bottom) * (Pad.Height / (double)Frame.Height);
                    return Refused.Unless(view.SetFrustum(left - x, right + x, bottom - y, top + y, near, far), nameof(ViewportInfo.SetFrustum));
                });
        });
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Overscan : System.Numerics.IMinMaxValue<Overscan> {
    public static Overscan MinValue { get; } = new(0d);
    public static Overscan MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SafeFrameMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Both)]
    internal static partial void Update(SafeFrameState state, SafeFrame frame);

    [UserMapping]
    private static double Scalar(FrameFraction fraction) => fraction;
}

public static class Framing {
    // --- [OUTPUT]
    public static Fin<PixelExtent> Final(ImageOutputState output, Size viewport) =>
        EffectPipeline.Extent(output.UseViewportSize ? viewport : new Size(output.ImageWidth, output.ImageHeight));

    public static (Length Width, Length Height) Print(ImageOutputState output, PixelExtent final) =>
        (Length.FromInches(final.Width / (double)output.ImageDpi), Length.FromInches(final.Height / (double)output.ImageDpi));

    public static Fin<ImageOutputState> Sized(ImageOutputState held, Size size) =>
        (PixelDimension.Validated(size.Width).ToValidation(),
         PixelDimension.Validated(size.Height).ToValidation())
            .Apply((width, height) => held with { ImageWidth = width, ImageHeight = height })
            .As()
            .ToFin();

    // --- [EXTENT]
    public static ReadModel<Eto.Drawing.Size> Extent { get; } = new(
        [ReadModel.Marks(EventKind.DocumentPropertiesChanged), ReadModel.Marks(EventKind.UndoRedo)],
        static doc => IO.lift(() => RenderPipeline.RenderSize(doc, fromRenderSources: true)).Map(static size => new Eto.Drawing.Size(size.Width, size.Height)));

    // --- [SIZES]
    public static Seq<RenderSize> Sizes(EditorState editor, Size screenPort) =>
        [new RenderSize.ViewportSize(screenPort), new RenderSize.CustomSize(), .. editor.CustomRenderSizes.Map(static size => new RenderSize.Listed(size))];

    public static IO<Unit> Choose(RenderSettingsSource.Section section, RenderSize size) =>
        from sized in RenderSettingsSources.Edit(section, scope => ImageOutputState.Read(scope).Bind(held => IO.lift(size.Applied(held))).Bind(next => ImageOutputState.Write(scope, next)))
        from preset in SectionModels.Write(section.Host, Provider.RhinoSettings, data => IO.lift(() => data.CustomImageSizeIsPreset = size.Map(viewportSize: false, customSize: false, listed: true)))
        select unit;

    // --- [ROUNDING]
    internal static int Edge(int side, double fraction) => (int)((float)fraction * side);
}
