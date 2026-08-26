using System.Globalization;
using System.Text.Json.Nodes;
using Eto.Drawing;
using Grasshopper2.Components;
using Rasm.Bridge.Contract;
using Rasm.ScenarioKit;

namespace Rasm.Scenarios;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Gh2Scenarios {
    private static readonly double[] Left = [1.5, -2.0, 10.0];
    private static readonly double[] Right = [4.0, 0.5, -3.0];
    private const double Factor = 3.0;

    [RhinoScenario("gh2", Requires = ["gh2.editor", "gh2.solve"], BudgetMs = 30_000)]
    public static Fin<Unit> Arithmetic(ScenarioContext ctx) {
        ArgumentNullException.ThrowIfNull(ctx);
        return ctx.WithGrasshopperDocument(scope =>
            from a in scope.Doc.Number(new PointF(0, 0), Left)
            from b in scope.Doc.Number(new PointF(0, 80), Right)
            from factor in scope.Doc.Number(new PointF(0, 160), Factor)
            from add in scope.Doc.Node(Gh2Object.Addition, new PointF(200, 40))
            from multiply in scope.Doc.Node(Gh2Object.Multiplication, new PointF(400, 100))
            from wireA in Gh2Document.Wire(a, add.Parameters.Input(0))
            from wireB in Gh2Document.Wire(b, add.Parameters.Input(1))
            from wireSum in Gh2Document.Wire(add.Parameters.Output(0), multiply.Parameters.Input(0))
            from wireFactor in Gh2Document.Wire(factor, multiply.Parameters.Input(1))
            from topology in ctx.Require(EvidenceName.Create("graph.wired"), scope.Doc.ObjectCount == 5 && scope.Doc.WireCount == 4)
            from solve in ctx.Expect(EvidenceName.Create("solve"), scope.Doc.Solve())
            from sums in ctx.Case(EvidenceName.Create("sum"), () => Judged(ctx, add, "sum", static (left, right) => left + right))
            from products in ctx.Case(EvidenceName.Create("product"), () => Judged(ctx, multiply, "product", static (left, right) => (left + right) * Factor))
            from manifest in Manifest(ctx, scope, add, multiply)
            from capture in ctx.CaptureGrasshopper(EvidenceName.Create("arithmetic"))
            select unit);
    }

    private static Fin<Unit> Judged(ScenarioContext ctx, Component component, string label, Func<double, double, double> oracle) =>
        ctx.Expect(EvidenceName.Create(label + ".items"), Gh2Document.Output<double>(component.Parameters.Output(0))).Bind(Fin<Unit> (items) => {
            double[] expected = [.. Left.Select((left, index) => oracle(left, Right[index]))];
            ctx.Fact(EvidenceName.Create(label + ".expected"), expected);
            return items.Length == expected.Length && expected.Select((value, index) => Math.Abs(items[index] - value) <= 1.0e-9).All(static close => close)
                ? unit
                : Error.New($"{label}: [{Render(items)}] differs from oracle [{Render(expected)}]");
        });

    private static Fin<Unit> Manifest(ScenarioContext ctx, GrasshopperDocumentScope scope, Component add, Component multiply) {
        JsonArray nodes = [];
        foreach (Component component in (Component[])[add, multiply]) {
            nodes.Add(new JsonObject {
                ["id"] = component.InstanceId.ToString("D"),
                ["name"] = component.Nomen.Name,
                ["inputs"] = component.Parameters.InputCount,
                ["outputs"] = component.Parameters.OutputCount,
                ["phase"] = component.State.Phase.ToString(),
            });
        }
        ctx.Manifest(EvidenceRole.Gh2CanvasManifest, EvidenceName.Create("arithmetic"), new JsonObject {
            ["objects"] = scope.Doc.ObjectCount,
            ["wires"] = scope.Doc.WireCount,
            ["components"] = nodes,
        });
        return unit;
    }

    private static string Render(double[] values) =>
        string.Join(", ", values.Select(static value => value.ToString("R", CultureInfo.InvariantCulture)));
}
