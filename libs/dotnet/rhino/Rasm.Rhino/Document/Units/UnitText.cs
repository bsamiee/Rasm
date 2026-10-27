using Rasm.Drafting;
using Rhino;
using Rhino.Input;
using UnitsNet;

namespace Rasm.Rhino.Document.Units;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UnitText {
    // --- [LENGTHS]
    public static IO<Length> ParseLength(string text, StringParserSettings settings, LengthUnit spaceUnit) =>
        Parsed(text, settings, spaceUnit).Map(value => Quantities.From(value, spaceUnit));

    public static IO<double> ParseNumber(string text, StringParserSettings settings) =>
        Parsed(text, settings, LengthUnit.None);

    private static IO<double> Parsed(string text, StringParserSettings settings, LengthUnit spaceUnit) =>
        IO.lift(() => (Value: LengthValue.Create(text, settings, out bool whole), Whole: whole))
            .Bind(parsed => IO.lift(() => parsed.Whole && !parsed.Value.IsUnset() ? Read(parsed.Value, spaceUnit, text) : new RhinoFault(text, RhinoReason.Unreadable))
                .Finally(IO.lift(parsed.Value.Dispose)));

    private static Fin<double> Read(LengthValue parsed, LengthUnit spaceUnit, string text) =>
        LengthUnit.IsNone(parsed.Units) || !LengthUnit.IsNone(spaceUnit) ? parsed.Length(spaceUnit) : new RhinoFault(text, RhinoReason.Unreadable);

    // --- [SCALES]
    public static IO<DrawingScale> ParseScale(string text, StringParserSettings settings, LengthUnit spaceUnit) =>
        (from scale in use(() => ScaleValue.Create(text, settings))
         from paper in use(() => scale.LeftLengthValue())
         from model in use(() => scale.RightLengthValue())
         from parsed in IO.lift(() => scale.IsUnset()
             ? new RhinoFault(text, RhinoReason.Unreadable)
             : (Read(paper, spaceUnit, text), Read(model, spaceUnit, text))
                 .Apply((paperLength, modelLength) => Quantities.From(paperLength, spaceUnit) / Quantities.From(modelLength, spaceUnit)).As()
                 .Bind(DrawingScale.Validated))
         select parsed).Bracket();

    // --- [ANGLES]
    public static IO<double> ParseAngle(string text, StringParserSettings settings) =>
        IO.lift(Fin<double> () => {
            StringParserSettings? results = null;
            AngleUnitSystem parsed = AngleUnitSystem.None;
            int count = StringParser.ParseAngleExpession(text, 0, -1, settings, AngleUnitSystem.Radians, out double radians, ref results, ref parsed);
            return count is not 0 && count == text.Length ? radians : new RhinoFault(text, RhinoReason.Unreadable);
        });
}
