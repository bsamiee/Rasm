using Eto.Forms;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
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
    internal const double Divisions = 100d;

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

internal sealed class DocumentUnits : ViewModel;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NumberFields {
    // --- [SCALARS]
    public static IO<(NumericUpDownWithUnitParsing Control, Func<double, IO<Unit>> Show)> Scalar(NumberRange range, Func<Edit<double>, IO<Unit>> edited) =>
        from field in use(() => Values.Fielded(new ScaledField(range)))
        from ranged in IO.lift(() => range.Applied(field))
        from digits in Values.Digits(field)
        from show in Values.Bound(field, Values.Numeric(field), Values.Stepping, None, None, edited)
        select (Control: (NumericUpDownWithUnitParsing)field, Show: (Func<double, IO<Unit>>)(value => show(Some(value))));

    // --- [DISTANCES]
    public static IO<Unit> Measured(NumericUpDownWithUnitParsing field, RhinoDoc doc, bool modelUnits) =>
        from units in IO.lift(static () => new DocumentUnits())
        from bound in IO.lift(() => {
            field.DataContext = units;
            if (modelUnits) field.UseViewModelUnits(units);
            else field.UseViewPageUnits(units);
            units.UnitsDistanceDisplayAndPrecisionFromDoc(doc);
        })
        from followed in Subscriptions.On<DocumentEventArgs>(static h => RhinoDoc.DocumentPropertiesChanged += h, static h => RhinoDoc.DocumentPropertiesChanged -= h)
            .Documents().In(doc.RuntimeSerialNumber)
            .Attach(_ => IO.lift(() => units.UnitsDistanceDisplayAndPrecisionFromDoc(doc)).OnUiThread())
        select unit;
}
