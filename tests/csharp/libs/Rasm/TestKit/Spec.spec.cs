using Rasm.TestKit;
using Xunit.Sdk;

namespace Rasm.Tests.TestKit;

// --- [OPERATIONS] ---------------------------------------------------------------------------------
public sealed class SpecHarnessLaws {
    [Fact]
    public void SupportMatrixReportsFalsePositiveAndFalseNegativeRows() {
        Spec.SupportMatrix(("true row", static () => true, true), ("false row", static () => false, false));
        _ = Assert.Throws<XunitException>(static () => Spec.SupportMatrix(("false positive", static () => false, true)));
        _ = Assert.Throws<XunitException>(static () => Spec.SupportMatrix(("false negative", static () => true, false)));
    }

    [Fact]
    public void ConservationLawRejectsNegativeAndUnbalancedCounts() {
        Spec.CountsConserve(attempted: 5, emitted: 3, rejected: 2, label: "balanced");
        _ = Assert.ThrowsAny<XunitException>(static () => Spec.CountsConserve(attempted: 5, emitted: 4, rejected: 2, label: "unbalanced"));
        _ = Assert.ThrowsAny<XunitException>(static () => Spec.CountsConserve(attempted: 5, emitted: -1, rejected: 6, label: "negative"));
    }
}
