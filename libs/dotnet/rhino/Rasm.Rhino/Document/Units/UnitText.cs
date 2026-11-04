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
        Whole(text, settings).Bind(parsed => IO.lift(() => Read(parsed, spaceUnit, text))).Bracket();

    private static IO<LengthValue> Whole(string text, StringParserSettings settings) =>
        from parsed in IO.lift(() => (Value: LengthValue.Create(text, settings, out bool whole), Whole: whole))
        from owned in use(() => parsed.Value)
        from complete in IO.lift(() => RhinoFault.Unless(parsed.Whole && !owned.IsUnset(), RhinoReason.Unreadable, text))
        select owned;

    private static Fin<double> Read(LengthValue parsed, LengthUnit spaceUnit, string text) =>
        LengthUnit.IsNone(parsed.Units) || !LengthUnit.IsNone(spaceUnit) ? parsed.Length(spaceUnit) : new RhinoFault(text, RhinoReason.Unreadable);

    // --- [SCALES]
    public static IO<DrawingScale> ParseScale(string text, StringParserSettings settings, LengthUnit spaceUnit) =>
        (from sides in IO.lift(() => text.Split([':', '='], StringSplitOptions.TrimEntries) is [var first, var second]
             ? Fin.Succ((Paper: first, Model: second))
             : new RhinoFault(text, RhinoReason.Unreadable))
         from paper in Whole(sides.Paper, settings)
         from model in Whole(sides.Model, settings)
         from lengths in IO.lift(() => (Read(paper, spaceUnit, text), Read(model, spaceUnit, text)).Apply(static (left, right) => (Paper: left, Model: right)).As())
         from positive in IO.lift(() => RhinoFault.Unless(lengths.Paper > 0d && lengths.Model > 0d, RhinoReason.Unreadable, text))
         from parsed in IO.lift(() => DrawingScale.Validated(Quantities.From(lengths.Paper, spaceUnit) / Quantities.From(lengths.Model, spaceUnit)))
         select parsed).Bracket();

    // --- [ANGLES]
    public static IO<Angle> ParseAngle(string text, StringParserSettings settings) =>
        IO.lift(Fin<Angle> () => {
            StringParserSettings? results = null;
            AngleUnitSystem parsed = AngleUnitSystem.None;
            int count = StringParser.ParseAngleExpession(text, 0, -1, settings, AngleUnitSystem.Radians, out double radians, ref results, ref parsed);
            return count is not 0 && count == text.Length ? Angle.FromRadians(Quantities.Exact(radians)) : new RhinoFault(text, RhinoReason.Unreadable);
        });
}
