using System.Drawing;
using Darkroom.Chain;
using PeterO.Numbers;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.DocObjects;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Overscan : System.Numerics.IMinMaxValue<Overscan> {
    public static Overscan MinValue => Off;
    public static Overscan MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
public sealed partial class RenderRegion {
    public FrameFraction Left { get; }
    public FrameFraction Top { get; }
    public FrameFraction Right { get; }
    public FrameFraction Bottom { get; }

    public Rectangle Pixels(PixelExtent frame) =>
        Rectangle.FromLTRB((int)(Left * frame.Width), (int)(Top * frame.Height), (int)(Right * frame.Width), (int)(Bottom * frame.Height));

    public static Fin<RenderRegion> Of(double left, double top, double right, double bottom) =>
        (FrameFraction.Validated(left), FrameFraction.Validated(top), FrameFraction.Validated(right), FrameFraction.Validated(bottom))
            .Apply(static (l, t, r, b) => (Left: l, Top: t, Right: r, Bottom: b))
            .As()
            .Bind(static edges => ValidationResults.ToFin(Validate(edges.Left, edges.Top, edges.Right, edges.Bottom, out RenderRegion? region), region));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FrameFraction left, ref FrameFraction top, ref FrameFraction right, ref FrameFraction bottom) =>
        validationError = left < right && top < bottom ? null : new ValidationError("region needs left before right and top above bottom");
}

public sealed class OverscanFrame {
    private OverscanFrame(PixelExtent frame, PixelExtent padded) => (Frame, Padded) = (frame, padded);

    public PixelExtent Frame { get; }
    public PixelExtent Padded { get; }
    public Size Pad => new((Padded.Width - Frame.Width) / 2, (Padded.Height - Frame.Height) / 2);
    public Rectangle DataWindow => new(-Pad.Width, -Pad.Height, Padded.Width, Padded.Height);

    public static Fin<OverscanFrame> Around(PixelExtent frame, Overscan overscan) =>
        EFloat.FromDouble(overscan) switch {
            var fraction => PixelExtent.Validated(Grown(frame.Width, fraction), Grown(frame.Height, fraction)).Map(padded => new OverscanFrame(frame, padded)),
        };

    public IO<Unit> Widen(ViewportInfo view) =>
        IO.lift(() => {
            view.FrustumAspect = (double)Frame.Width / Frame.Height;
            return Refused.Unless(view.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far), nameof(ViewportInfo.GetFrustum))
                .Bind(_ => ((right - left) * Pad.Width / Frame.Width, (top - bottom) * Pad.Height / Frame.Height) switch {
                    var (x, y) => Refused.Unless(view.SetFrustum(left - x, right + x, bottom - y, top + y, near, far), nameof(ViewportInfo.SetFrustum)),
                });
        });

    private static int Grown(int side, EFloat fraction) =>
        fraction.Multiply(side).RoundToIntegerNoRoundedFlag(EContext.UnlimitedHalfEven).ToEInteger().Multiply(2).Add(side) switch {
            var held => held.CanFitInInt32() ? held.ToInt32Unchecked() : int.MaxValue,
        };
}

public sealed record RenderOverride(ValueSet Values, Option<Albedo> Clay) {
    // --- [SCENE]
    public static IO<BindingGroup> Bindings(RhinoDoc doc) =>
        CyclesSetting.Bindings.Map(cycles => new BindingGroup(
            RenderSettingsSources.Bindings(new RenderSettingsSource.Live(doc)).Bindings + ChainState.Bindings(doc).Bindings + cycles.Bindings));

    // --- [SCOPE]
    public IO<T> Around<T>(RhinoDoc doc, IPlugInViews views, IO<T> launch) =>
        Bindings(doc).Bind(bindings => DisposalOps.Within(
            bindings.Capture.Map(prior => new ValueSet(prior.Entries.Intersect(Values.Entries.Keys))),
            _ => DisposalOps.Within<IDisposable, T>(retain =>
                    from applied in bindings.Apply(Values, [])
                    from clay in Clay.Traverse(albedo => ClayMeshProvider.Register(views, doc, albedo, retain)).As()
                    from launched in launch
                    select launched,
                DisposalOps.Release),
            prior => bindings.Apply(prior, []).Map(static _ => unit)));
}
