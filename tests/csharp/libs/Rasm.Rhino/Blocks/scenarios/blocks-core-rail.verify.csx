using System;
using Rasm.Rhino.Blocks;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

Scenario.Run("blocks-core-rail", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    facts.Add("mainThread", RhinoApp.IsOnMainThread);
    DefinitionName blockName = Probe.Expect(DefinitionName.Sanitize(value: $"RasmVerifyCore{Guid.NewGuid():N}", key: key), "block name");
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
    InstanceDefinition live = scope.Active.InstanceDefinitions.Find(instanceDefinitionName: blockName.Value)
        ?? throw new InvalidOperationException(message: "native find by name failed");
    Probe.Require(authored >= 0, $"native.idx={authored}");
    Probe.Require(live.ObjectCount >= 1, $"member.count={live.ObjectCount}");
    facts.Add("native.idx", authored);
    facts.Add("member.count", live.ObjectCount);
    facts.Add("blockName", blockName.Value);
    RhinoBlocks blocks = RhinoBlocks.Live(document: scope.Active, mode: RunMode.Scripted);
    BlockOp graph = new BlockOp.Graph(Query: new GraphQuery.Members(Ref: DefinitionRef.Of(name: blockName)));
    BlockOutcome outcome = Probe.Expect(blocks.Run(op: graph, key: key), "graph run");
    BlockOutcome.MembersResult members = outcome is BlockOutcome.MembersResult value
        ? value
        : throw new InvalidOperationException(message: $"unexpected outcome: {outcome.GetType().Name}");
    facts.Add("run.members", members.Values.Count);
    Probe.Require(members.Values.Count >= 1, "graph members");
});
