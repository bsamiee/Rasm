using Rasm.Drafting;

namespace Rasm.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Drafting {
    [Fact]
    public void RenumberKeepsSeriesOrderAndSuffixes() {
        Seq<(int Sheet, StandardName Number)> ordered = Seq("A-199-A", "S-199", "A-198-B", "A-199-C")
            .Map(static (text, index) => (Sheet: index, Number: StandardName.From(NamingStandard.Uds, text).ThrowIfFail()));
        Seq<(int Sheet, StandardName Number)> actual = StandardName.Renumber(ordered, []).ThrowIfFail();
        Assert.Equal(new[] { (0, "A-198-A"), (1, "S-199"), (2, "A-199-B"), (3, "A-198-C") }, actual.Select(static entry => (entry.Sheet, entry.Number.Text)));
    }

    [Theory]
    [InlineData("009", "8", "008", "9")]
    [InlineData("999999999", "999999998", "999999998", "999999999")]
    public void RenumberPreservesNumberWidths(string first, string second, string firstExpected, string secondExpected) {
        const string prefix = "P-O-F-S-D-A-";
        Seq<(int Sheet, StandardName Number)> ordered = toSeq(new[] { first, second }
            .Select(static (number, index) => (Sheet: index, Number: StandardName.From(NamingStandard.Iso19650, prefix + number).ThrowIfFail())));
        Assert.Equal([prefix + firstExpected, prefix + secondExpected], StandardName.Renumber(ordered, []).ThrowIfFail().Map(static entry => entry.Number.Text), StringComparer.Ordinal);
    }

    [Theory]
    [InlineData(new[] { "A-102", "A-101" }, new[] { "A-101", "A-102" }, new[] { "A-101", "A-102" })]
    [InlineData(new[] { "A-101", "A-101" }, new string[0], new[] { "A-101", "A-101" })]
    public void RenumberReportsAllCollisions(string[] numbers, string[] others, string[] expected) {
        Seq<(int Sheet, StandardName Number)> ordered = toSeq(numbers.Select(static (text, index) => (Sheet: index, Number: StandardName.From(NamingStandard.Uds, text).ThrowIfFail())));
        Fin<Seq<(int Sheet, StandardName Number)>> result = StandardName.Renumber(ordered, toSeq(others.Select(static text => StandardName.From(NamingStandard.Uds, text).ThrowIfFail())));
        Error failure = Assert.IsType<Fin<Seq<(int Sheet, StandardName Number)>>.Fail>(result).Error;
        Assert.Equal(expected.Select(static (text, index) => (DraftingReason.NumberTaken, Some(index), text)), failure.AsIterable().Map(static error => Assert.IsType<DraftingFault>(error) switch {
            var fault => (fault.Reason, fault.Index, Assert.IsType<string>(fault.Subject)),
        }).ToSeq());
    }

    [Fact]
    public void PositionsUseDisciplineAndNumberOrder() {
        Seq<(int Sheet, StandardName Number)> sheets = Seq("A-102", "S-101", "A-101")
            .Map(static (text, index) => (Sheet: index, Number: StandardName.From(NamingStandard.Uds, text).ThrowIfFail()));
        Assert.Equal(Seq((1, 1, 1), (2, 1, 2), (0, 2, 2)), StandardName.Positions(sheets));
    }

    [Fact]
    public void NamesWithoutSequencesStayUnchanged() {
        Seq<(int Sheet, StandardName Number)> ordered = Seq("A-WALL-E", "S-COLS-N")
            .Map(static (text, index) => (Sheet: index, Number: StandardName.From(NamingStandard.Ncs, text).ThrowIfFail()));
        Assert.Equal(ordered, StandardName.Renumber(ordered, []).ThrowIfFail());
    }

    [Fact]
    public void UnknownNameFieldsAreRejected() {
        StandardName original = StandardName.From(NamingStandard.Ncs, "A-WALL").ThrowIfFail();
        NameField field = original.Standard.Fields[0];
        Fin<StandardName> result = StandardName.Validated(original.Standard, original.Fields.Add(field with { Key = "unknown" }, original.Fields[field]));
        Assert.Equal(DraftingReason.NameBroken, Assert.IsType<DraftingFault>(Assert.IsType<Fin<StandardName>.Fail>(result).Error).Reason);
    }

    [Fact]
    public void OptionalNameFieldsKeepUnambiguousOrder() {
        const string value = "EXST";
        StandardName original = StandardName.From(NamingStandard.Ncs, $"A-WALL-{value}").ThrowIfFail();
        NameField[] optional = [.. original.Standard.Fields.Where(static field => !field.Required && field.Fits(value.Length))];
        Fin<StandardName> result = StandardName.Validated(original.Standard, original.Fields.Remove(optional[0]).Add(optional[1], value));
        Assert.Equal(DraftingReason.NameBroken, Assert.IsType<DraftingFault>(Assert.IsType<Fin<StandardName>.Fail>(result).Error).Reason);
    }

    [Theory]
    [InlineData("1:2:3", 1)]
    [InlineData(":", 2)]
    public void InvalidScaleTerms(string text, int errors) {
        Error failure = Assert.IsType<Fin<DrawingScale>.Fail>(DrawingScale.Parse(text)).Error;
        Assert.Equal(errors, failure.Count);
        Assert.All(failure.AsIterable().ToSeq(), error => {
            DraftingFault fault = Assert.IsType<DraftingFault>(error);
            Assert.Equal(DraftingReason.ScaleRatio, fault.Reason);
            Assert.Equal(text, Assert.IsType<string>(fault.Subject));
        });
    }
}
