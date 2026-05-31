using Rasm.Grasshopper.Components;
using Rasm.TestKit;

namespace Rasm.Grasshopper.Tests.Components;

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class ComponentUiRunLaws {
    [Fact]
    public void PhaseSpecificCallbacksOnlyRunForMatchingPhase() {
        List<ComponentUi.Phase> observed = [];
        ComponentUi ui =
            ComponentUi.When(phase: ComponentUi.Phase.InputPanel, run: _ => { observed.Add(item: ComponentUi.Phase.InputPanel); return Fin.Succ(value: ComponentUi.Decision.Handled); })
            + ComponentUi.When(phase: ComponentUi.Phase.Resize, run: _ => { observed.Add(item: ComponentUi.Phase.Resize); return Fin.Succ(value: ComponentUi.Decision.WithSize(size: new SizeF(width: 80f, height: 40f))); });

        Spec.Succ(
            result: ui.Run(context: new ComponentUi.Callback.Frame(Owner: null!, Current: SizeF.Empty)),
            then: static decision => Assert.Equal(expected: new SizeF(width: 80f, height: 40f), actual: decision.Size.IfNone(SizeF.Empty)));
        Assert.Equal<ComponentUi.Phase>(expected: [ComponentUi.Phase.Resize], actual: observed);
    }

    [Fact]
    public void TerminalDecisionShortCircuitsLaterCallbacks() {
        int hits = 0;
        ComponentUi ui =
            ComponentUi.Of(run: _ => { hits++; return Fin.Succ(value: ComponentUi.Decision.Handled); })
            + ComponentUi.Of(run: _ => { hits++; return Fin.Succ(value: ComponentUi.Decision.WithSize(size: new SizeF(width: 20f, height: 20f))); });

        Spec.Succ(
            result: ui.Run(context: new ComponentUi.Callback.Frame(Owner: null!, Current: SizeF.Empty)),
            then: static decision => {
                Assert.True(condition: decision.IsTerminal);
                Assert.True(condition: decision.Size.IsNone);
            });
        Assert.Equal(expected: 1, actual: hits);
    }

    [Fact]
    public void DecisionAdditionIsRightBiasedAndTerminalAbsorbing() {
        RectangleF left = new(x: 1f, y: 2f, width: 3f, height: 4f);
        RectangleF right = new(x: 5f, y: 6f, width: 7f, height: 8f);
        ComponentUi.Decision merged =
            ComponentUi.Decision.WithBounds(bounds: left)
            + ComponentUi.Decision.WithSize(size: new SizeF(width: 20f, height: 10f))
            + ComponentUi.Decision.WithBounds(bounds: right)
            + ComponentUi.Decision.Release;

        Assert.Equal(expected: right, actual: merged.Bounds.IfNone(RectangleF.Empty));
        Assert.Equal(expected: new SizeF(width: 20f, height: 10f), actual: merged.Size.IfNone(SizeF.Empty));
        Assert.True(condition: merged.IsTerminal);
    }
}
