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
        DisposalOps.Within(() => (Value: LengthValue.Create(text, settings, out bool whole), Whole: whole),
            parsed => IO.pure(Errors.ParseError($"{nameof(LengthValue)}.{nameof(LengthValue.Create)} parsed no whole value from {text}"))
                .Bind(refused => IO.lift(() => parsed.Whole && !parsed.Value.IsUnset() ? Read(parsed.Value, spaceUnit, refused) : refused)),
            static parsed => IO.lift(parsed.Value.Dispose));

    private static Fin<double> Read(LengthValue parsed, LengthUnit spaceUnit, Error refused) =>
        LengthUnit.IsNone(parsed.Units) || !LengthUnit.IsNone(spaceUnit) ? parsed.Length(spaceUnit) : refused;

    // --- [SCALES]
    public static IO<DrawingScale> ParseScale(string text, StringParserSettings settings, LengthUnit spaceUnit) =>
        DisposalOps.Within(() => ScaleValue.Create(text, settings), scale =>
            DisposalOps.Within(scale.LeftLengthValue, paper =>
                DisposalOps.Within(scale.RightLengthValue, model => IO.lift(() => scale.IsUnset()
                    ? Errors.ParseError($"{nameof(ScaleValue)}.{nameof(ScaleValue.Create)} parsed no whole value from {text}")
                    : (Read(paper, spaceUnit, Errors.ParseError($"{nameof(ScaleValue)}.{nameof(ScaleValue.LeftLengthValue)} parsed no whole value from {text}")),
                       Read(model, spaceUnit, Errors.ParseError($"{nameof(ScaleValue)}.{nameof(ScaleValue.RightLengthValue)} parsed no whole value from {text}")))
                        .Apply((paperLength, modelLength) => Quantities.From(paperLength, spaceUnit) / Quantities.From(modelLength, spaceUnit)).As()
                        .Bind(static ratio => DrawingScale.TryCreate(ratio, out DrawingScale scale, out DraftingFault? fault) ? Fin.Succ(scale) : fault)))));

    // --- [ANGLES]
    public static IO<double> ParseAngle(string text, StringParserSettings settings) =>
        IO.lift(Fin<double> () => {
            StringParserSettings? results = null;
            AngleUnitSystem parsed = AngleUnitSystem.None;
            int count = StringParser.ParseAngleExpession(text, 0, -1, settings, AngleUnitSystem.Radians, out double radians, ref results, ref parsed);
            return count is not 0 && count == text.Length ? radians : Errors.ParseError($"{nameof(StringParser)}.{nameof(StringParser.ParseAngleExpession)} parsed no whole value from {text}");
        });
}
