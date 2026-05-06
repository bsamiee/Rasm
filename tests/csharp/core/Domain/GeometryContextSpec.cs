using Core.Domain;
using LanguageExt;
using Rhino;
using Xunit;

namespace Core.Tests.Domain;

// --- [EXAMPLES] --------------------------------------------------------------------------------

public sealed class GeometryContextSpec {
    [Theory]
    [InlineData(UnitSystem.Unset)]
    [InlineData(UnitSystem.CustomUnits)]
    public void RejectsUnsupported(UnitSystem units) =>
        Assert.True(
            condition: GeometryContext.CreateDefault(units: units)
                .ToFin()
                .IsFail);

    [Fact]
    public void RejectsMissingDocument() =>
        Assert.True(
            condition: GeometryContext.FromDocument(doc: null)
                .ToFin()
                .IsFail);
}
