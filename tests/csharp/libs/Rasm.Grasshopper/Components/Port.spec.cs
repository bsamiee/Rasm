using System.Reflection;
using Rasm.Grasshopper.Components;
using Rasm.TestKit;

namespace Rasm.Grasshopper.Tests.Components;

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class ComponentUiCapabilityLaws {
    private static bool Supports(ComponentUi ui, ComponentUi.Phase phase) =>
        (bool)typeof(ComponentUi)
            .GetMethod(name: "Supports", bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(obj: ui, parameters: [phase])!;

    [Fact]
    public void PhaseAgnosticCallbacksDoNotAdvertiseResize() {
        ComponentUi ui = ComponentUi.Of(run: static _ => Fin.Succ(value: ComponentUi.Decision.Pass));
        Spec.Holds(condition: !Supports(ui: ui, phase: ComponentUi.Phase.Resize), label: "phase-agnostic UI advertised native resize");
    }

    [Fact]
    public void ExplicitResizeCallbacksAdvertiseResize() {
        ComponentUi ui = ComponentUi.When(phase: ComponentUi.Phase.Resize, run: static _ => Fin.Succ(value: ComponentUi.Decision.Pass));
        Spec.Holds(condition: Supports(ui: ui, phase: ComponentUi.Phase.Resize), label: "resize UI did not advertise native resize");
    }
}

public sealed class PortKindCollisionLaws {
    [Fact]
    public void PrimitiveTypeCollisionsResolveToIntentionalDefaults() {
        Spec.Some(PortKind.From(type: typeof(int)), static kind => Assert.Equal(expected: PortKind.Integer, actual: kind));
        Spec.Some(PortKind.From(type: typeof(string)), static kind => Assert.Equal(expected: PortKind.Text, actual: kind));
        Spec.Holds(condition: !PortKind.TypeCollisions.Exists(static item => item.Type == typeof(int) || item.Type == typeof(string)), label: "intentional primitive defaults leaked into collision set");
    }
}
