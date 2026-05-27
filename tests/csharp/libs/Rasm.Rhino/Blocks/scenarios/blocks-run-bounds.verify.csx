using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Rhino.Blocks;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using static LanguageExt.Prelude;

Scenario.Run("blocks-run-bounds", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    facts.Add("mainThread", RhinoApp.IsOnMainThread);
    DefinitionName blockName = Probe.Expect(
        DefinitionName.Sanitize(value: $"RasmVerifyBounds{Guid.NewGuid():N}", key: key),
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
    InstanceDefinition live = scope.Active.InstanceDefinitions.Find(instanceDefinitionName: blockName.Value)
        ?? throw new InvalidOperationException(message: "native find by name failed");
    Probe.Require(authored >= 0, $"native.idx={authored}");
    Probe.Require(live.ObjectCount >= 1, $"member.count={live.ObjectCount}");
    facts.Add("native.idx", authored);
    facts.Add("member.count", live.ObjectCount);
    facts.Add("blockName", blockName.Value);
    RhinoBlocks blocks = RhinoBlocks.Live(document: scope.Active, mode: RunMode.Scripted);
    string used = Probe.Expect(
        blocks.Use(
            refer: DefinitionRef.Of(name: blockName),
            project: def => Fin.Succ(value: def.Name ?? string.Empty),
            key: key),
        "use");
    facts.Add("use.name", used);
    BlockOp bounds = new BlockOp.Bounds(
        Ref: DefinitionRef.Of(name: blockName),
        Policy: BoundsPolicy.Default);
    BlockOutcome outcome = Probe.Expect(blocks.Run(op: bounds, key: key), "bounds run");
    BlockOutcome.Bounds boxed = outcome is BlockOutcome.Bounds value
        ? value
        : throw new InvalidOperationException(message: $"unexpected outcome: {outcome.GetType().Name}");
    facts.Add("bounds.valid", boxed.Value.IsValid);
    facts.Add("bounds.diagonal", boxed.Value.Diagonal.Length);
    Probe.Require(boxed.Value.IsValid, "bounds valid");
});
