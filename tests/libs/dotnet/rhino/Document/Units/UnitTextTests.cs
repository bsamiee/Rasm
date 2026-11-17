using System.Globalization;
using System.Numerics;
using CsCheck;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.Input;
using Xunit;

namespace Rasm.Rhino.Tests.Document.Units;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class UnitTextTests {

    // --- [LENGTHS]

    [CulturedFact(["en-US", "de-DE"])]
    public Task LengthTextReadsInTheSpaceUnitByRhinoUnitScale() =>
        Suffixed.Select(Spaces, Gen.Int[1, 1_000_000]).SampleAsync(static async (suffix, space, steps) =>
            Assert.Equal(
                steps * ModelGrid.Model.Absolute * RhinoMath.UnitScale(suffix.System, space.System),
                Quantities.As(await RhinoHost.RunAsync(UnitText.ParseLength(Text(steps, suffix.Text), StringParserSettings.DefaultParseSettings, space.Unit)), space.Unit),
                RhinoMath.SqrtEpsilon * Math.Max(1d, steps * ModelGrid.Model.Absolute * RhinoMath.UnitScale(suffix.System, space.System))));

    [Fact]
    public Task UnitBearingTextInAUnitlessSpaceFailsAsUnreadable() =>
        Suffixed.Select(Gen.Int[1, 1_000_000]).SampleAsync(static async (suffix, steps) =>
            Assert.Equal(RhinoReason.Unreadable, (await Expect<RhinoFault>.FailAsync(UnitText.ParseLength(Text(steps, suffix.Text), StringParserSettings.DefaultParseSettings, LengthUnit.None))).Reason));

    // --- [SCALES]

    [CulturedFact(["en-US", "de-DE"])]
    public Task ScaleTextReducesToItsLowestTerms() =>
        Gen.Int[1, 1000].Select(Gen.Int[1, 1000]).SampleAsync(static async (paper, model) => {
            int common = (int)BigInteger.GreatestCommonDivisor(paper, model);
            Assert.Equal(
                (new BigInteger(paper / common), new BigInteger(model / common)),
                (await RhinoHost.RunAsync(UnitText.ParseScale(string.Create(CultureInfo.InvariantCulture, $"{paper}:{model}"), StringParserSettings.DefaultParseSettings, LengthUnit.Millimeters))).Terms);
        });

    [Theory]
    [InlineData("1:0")]
    [InlineData("0:1")]
    [InlineData("1:2:3")]
    [InlineData("1:")]
    [InlineData(":1")]
    [InlineData("1")]
    public async Task MalformedScaleTextFailsAsUnreadable(string text) =>
        Assert.Equal(RhinoReason.Unreadable, (await Expect<RhinoFault>.FailAsync(UnitText.ParseScale(text, StringParserSettings.DefaultParseSettings, LengthUnit.Millimeters))).Reason);

    // --- [GENERATORS]

    private static readonly Gen<(string Text, UnitSystem System)> Suffixed =
        Gen.OneOf(("mm", UnitSystem.Millimeters), ("cm", UnitSystem.Centimeters), ("m", UnitSystem.Meters), ("in", UnitSystem.Inches), ("ft", UnitSystem.Feet));

    private static readonly Gen<(UnitSystem System, LengthUnit Unit)> Spaces =
        Gen.OneOf(UnitSystem.Millimeters, UnitSystem.Centimeters, UnitSystem.Meters, UnitSystem.Inches, UnitSystem.Feet).Select(static system => (system, LengthUnit.FromKnownUnitSystem(system)));

    // --- [INPUTS]

    private static string Text(int steps, string suffix) => string.Create(CultureInfo.InvariantCulture, $"{steps * (decimal)ModelGrid.Model.Absolute}{suffix}");
}
