using Eto.Forms;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class NumberScale {
    public static readonly NumberScale Linear = new(
        static (range, value) => (value - range.Low) / (range.High - range.Low),
        static (range, fraction) => range.Low + (fraction * (range.High - range.Low)));

    public static readonly NumberScale Logarithmic = new(
        static (range, value) => Math.Log(value / range.Low) / Math.Log(range.High / range.Low),
        static (range, fraction) => range.Low * Math.Pow(range.High / range.Low, fraction));

    [UseDelegateFromConstructor]
    public partial double Fraction(NumberRange range, double value);

    [UseDelegateFromConstructor]
    public partial double Value(NumberRange range, double fraction);
}

public sealed record NumberRange(double Low, double High) {
    private const double Divisions = 100d;

    public Option<double> Step { get; init; }
    public Option<int> Decimals { get; init; }
    public Option<string> Suffix { get; init; }
    public NumberScale Scale { get; init; } = NumberScale.Linear;

    public double Clamped(double value) => double.Clamp(value, Low, High);

    public double Stepped(double value, double steps) =>
        Clamped(Step.Match(
            Some: step => value + (steps * step),
            None: () => Scale.Value(this, Scale.Fraction(this, value) + (steps / Divisions))));

    public double Snapped(double value) =>
        Clamped(Step.Match(
            Some: step => Low + (Math.Round((value - Low) / step, MidpointRounding.ToEven) * step),
            None: () => Scale.Value(this, Math.Round(Scale.Fraction(this, value) * Divisions, MidpointRounding.ToEven) / Divisions)));

    internal Unit Applied(NumericUpDownWithUnitParsing field) {
        (field.MinValue, field.MaxValue, field.Suffix) = (Low, High, Conversions.Unset(Suffix.Map(static suffix => $" {suffix}")));
        _ = Step.Iter(step => field.Increment = step);
        _ = Decimals.Iter(decimals => (field.DecimalPlaces, field.MaximumDecimalPlaces) = (decimals, decimals));
        return unit;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ScaledField(NumberRange range) : NumericUpDownWithUnitParsing {
    private const double Fine = 0.1d;

    protected override double UpdateValueForStepWheelOrDrag(double current, bool up, int dragDistance = -1) =>
        (Keyboard.Modifiers & (Keys.Shift | Application.Instance.CommonModifier)) switch {
            Keys.None when range.Step.IsSome => base.UpdateValueForStepWheelOrDrag(current, up, dragDistance),
            var modifiers => range.Stepped(current, (dragDistance < 0 ? 1d : dragDistance) * (up ? 1d : -1d) * (modifiers.HasFlag(Application.Instance.CommonModifier) ? Fine : 1d)) switch {
                var stepped => modifiers.HasFlag(Keys.Shift) ? range.Snapped(stepped) : stepped,
            },
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NumberFields {
    // --- [SCALARS]
    public static IO<(NumericUpDownWithUnitParsing Control, Func<double, IO<Unit>> Show)> Scalar(NumberRange range, Func<Edit<double>, IO<Unit>> edited, IPlugInLifetime owner) =>
        from field in Field(() => new ScaledField(range), owner)
        from ranged in IO.lift(() => range.Applied(field))
        from change in Changes(field, edited, static field => field.Value, owner)
        select (field, (Func<double, IO<Unit>>)(value => change.Detached(IO.lift(() => { field.Value = value; }))));

    // --- [DISTANCES]
    public static IO<(NumericUpDownWithUnitParsing Control, Func<Length, IO<Unit>> Show)> Distance(
        RhinoDoc doc, (Length Low, Length High) domain, bool modelUnits, Func<Edit<Length>, IO<Unit>> edited, IPlugInLifetime owner) =>
        from field in Field(static () => new NumericUpDownWithUnitParsing(), owner)
        from units in IO.lift(() => modelUnits ? doc.ModelUnits : doc.PageUnits)
        let measured = IO.lift(() => {
            NumericUpDownWithUnitParsing.GetFormatLengthUnits(doc, out LengthUnit lengthUnits, out DistanceDisplayMode mode, !modelUnits);
            field.SetFormatLengthUnits(lengthUnits, mode);
            (field.UseDistanceDisplayMode, field.MinValue, field.MaxValue) = (true, Quantities.As(domain.Low, units), Quantities.As(domain.High, units));
        })
        from first in measured
        from remeasured in EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber).Inline(_ => measured, owner)
        from change in Changes(field, edited, field => Quantities.From(field.Value, units), owner)
        select (field, (Func<Length, IO<Unit>>)(value => change.Detached(IO.lift(() => { field.Value = Quantities.As(value, units); }))));

    // --- [REALIZE]
    private static IO<NumericUpDownWithUnitParsing> Field(Func<NumericUpDownWithUnitParsing> create, IPlugInLifetime owner) =>
        from field in use(create)
        from mode in IO.lift(() => {
            field.ValueUpdateMode = NumericUpDownWithUnitParsingUpdateMode.OnValueChange | NumericUpDownWithUnitParsingUpdateMode.WhenDoneChanging | NumericUpDownWithUnitParsingUpdateMode.OnEnterOrLoseFocus;
        })
        from digits in HostTheme.In(field).Inline(_ => IO.lift(() => { field.Font = HostPlatform.Current.Digits(field.Font); }), owner)
        select field;

    private static IO<ControlChange<EventArgs>> Changes<TValue>(NumericUpDownWithUnitParsing field, Func<Edit<TValue>, IO<Unit>> edited, Func<NumericUpDownWithUnitParsing, TValue> read, IPlugInLifetime owner)
        where TValue : notnull =>
        ControlChange<EventArgs>.Attached(
            Subscriptions.Merged(
                Subscriptions.Host<EventArgs>(h => field.ValueChanged += h, h => field.ValueChanged -= h),
                Subscriptions.Host<NumericUpDownWithUnitParsingEventArgs>(h => field.SteppingDone += h, h => field.SteppingDone -= h).Choose(static _ => Some(EventArgs.Empty)),
                Subscriptions.Host<NumericUpDownWithUnitParsingEventArgs>(h => field.TextChangingDone += h, h => field.TextChangingDone -= h).Choose(static _ => Some(EventArgs.Empty))),
            _ => IO.lift(() => (Value: read(field), Previewing: field.IsStepping || field.IsTextChanging))
                .Bind(held => edited(held.Previewing ? Edits.Preview(held.Value) : Edits.Commit(held.Value))),
            owner);
}
