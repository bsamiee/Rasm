using System;
using Rasm.Rhino.Blocks;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

Scenario.Run("blocks-graph-plan", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    facts.Add("mainThread", RhinoApp.IsOnMainThread);
    DefinitionName blockName = Probe.Expect(
        DefinitionName.Sanitize(value: $"RasmVerifyPlan{Guid.NewGuid():N}", key: key),
        "block name");
    Brep brep = Brep.CreateFromBox(new Box(
        Plane.WorldXY,
        new Interval(t0: 0.0, t1: 12.0),
        new Interval(t0: 0.0, t1: 6.0),
        new Interval(t0: 0.0, t1: 4.0))) ?? throw new InvalidOperationException(message: "box brep");
    int authored = scope.Active.InstanceDefinitions.Add(
        name: blockName.Value,
        description: string.Empty,
        basePoint: Point3d.Origin,
        geometry: brep,
        attributes: new ObjectAttributes());
    Probe.Require(authored >= 0, $"native.idx={authored}");
    facts.Add("blockName", blockName.Value);
    RhinoBlocks blocks = RhinoBlocks.Live(document: scope.Active, mode: RunMode.Scripted);
    BlockOp graph = new BlockOp.Graph(Query: new GraphQuery.Plan(Root: DefinitionRef.Of(name: blockName)));
    BlockOutcome outcome = Probe.Expect(blocks.Run(op: graph, key: key), "graph plan run");
    BlockOutcome.Plan plan = outcome is BlockOutcome.Plan value
        ? value
        : throw new InvalidOperationException(message: $"unexpected outcome: {outcome.GetType().Name}");
    facts.Add("plan.order", plan.Order.Count);
    Probe.Require(plan.Order.Count >= 1, "bake plan order");
});
