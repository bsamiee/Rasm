using Rasm.Drafting;
using Rhino;
using Rhino.Input;
using UnitsNet;

namespace Rasm.Rhino.Document.Notation;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UnitText {
    // --- [LENGTHS]
    public static IO<Length> ParseLength(string text, StringParserSettings settings, LengthUnit spaceUnit) =>
        Parsed(text, settings, spaceUnit).Map(value => Quantities.From(value, spaceUnit));

    public static IO<double> ParseNumber(string text, StringParserSettings settings) =>
        Parsed(text, settings, LengthUnit.None);

    private static IO<double> Parsed(string text, StringParserSettings settings, LengthUnit spaceUnit) =>
        (from parsed in use(() => (Value: LengthValue.Create(text, settings, out bool whole), Whole: whole), static parsed => parsed.Value.Dispose())
         let refused = new UnparsedText(typeof(LengthValue), nameof(LengthValue.Create), text)
         from value in IO.lift(() => parsed.Whole && !parsed.Value.IsUnset() ? Read(parsed.Value, spaceUnit, refused) : refused)
         select value)
            .Bracket();

    private static Fin<double> Read(LengthValue parsed, LengthUnit spaceUnit, UnparsedText refused) =>
        LengthUnit.IsNone(parsed.Units) || !LengthUnit.IsNone(spaceUnit) ? parsed.Length(spaceUnit) : refused;

    // --- [SCALES]
    public static IO<DrawingScale> ParseScale(string text, StringParserSettings settings, LengthUnit spaceUnit) =>
        (from scale in use(() => ScaleValue.Create(text, settings))
         from paper in use(scale.LeftLengthValue)
         from model in use(scale.RightLengthValue)
         from drawing in IO.lift(() => scale.IsUnset()
             ? new UnparsedText(typeof(ScaleValue), nameof(ScaleValue.Create), text)
             : (Read(paper, spaceUnit, new UnparsedText(typeof(ScaleValue), nameof(ScaleValue.LeftLengthValue), text)),
                Read(model, spaceUnit, new UnparsedText(typeof(ScaleValue), nameof(ScaleValue.RightLengthValue), text)))
                 .Apply((paperLength, modelLength) => Quantities.From(paperLength, spaceUnit) / Quantities.From(modelLength, spaceUnit)).As()
                 .Bind(Conversions.Validated<DrawingScale, QuantityValue, InvalidDrafting>))
         select drawing)
            .Bracket();

    // --- [ANGLES]
    public static IO<double> ParseAngle(string text, StringParserSettings settings) =>
        IO.lift(Fin<double> () => {
            StringParserSettings? results = null;
            AngleUnitSystem parsed = AngleUnitSystem.None;
            int count = StringParser.ParseAngleExpession(text, 0, -1, settings, AngleUnitSystem.Radians, out double radians, ref results, ref parsed);
            return count != 0 && count == text.Length ? radians : new UnparsedText(typeof(StringParser), nameof(StringParser.ParseAngleExpession), text);
        });
}
