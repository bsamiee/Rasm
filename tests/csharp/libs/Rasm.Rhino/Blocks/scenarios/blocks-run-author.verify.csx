using System;
using Rasm.Rhino.Blocks;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using static LanguageExt.Prelude;

Scenario.Run("blocks-run-author", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    facts.Add("mainThread", RhinoApp.IsOnMainThread);
    DefinitionName blockName = Probe.Expect(
        DefinitionName.Sanitize(value: $"RasmVerifyAuthor{Guid.NewGuid():N}", key: key),
        "block name");
    Brep brep = Brep.CreateFromBox(new Box(
        Plane.WorldXY,
        new Interval(t0: 0.0, t1: 12.0),
        new Interval(t0: 0.0, t1: 6.0),
        new Interval(t0: 0.0, t1: 4.0))) ?? throw new InvalidOperationException(message: "box brep");
    RhinoBlocks blocks = RhinoBlocks.Live(document: scope.Active, mode: RunMode.Scripted);
    AuthorSpec spec = Probe.Expect(
        AuthorSpec.Of(
            name: blockName,
            basePoint: Point3d.Origin,
            key: key),
        "author spec");
    Members source = Probe.Expect(
        Members.Of(geometry: Seq<GeometryBase>(brep), key: key),
        "members");
    BlockOp author = new BlockOp.Author(Spec: spec, Source: source, Conflict: ConflictPolicy.Fail);
    BlockOutcome outcome = Probe.Expect(result: blocks.Run(op: author, key: key), label: "author run", facts: facts);
    BlockOutcome.Receipt receipt = outcome is BlockOutcome.Receipt value
        ? value
        : throw new InvalidOperationException(message: $"unexpected outcome: {outcome.GetType().Name}");
    InstanceDefinition live = scope.Active.InstanceDefinitions.Find(instanceDefinitionName: blockName.Value)
        ?? throw new InvalidOperationException(message: "native find by name failed");
    facts.Add("receipt.created", receipt.Value.Document.Created.Count);
    facts.Add("member.count", live.ObjectCount);
    facts.Add("blockName", blockName.Value);
    Probe.Require(live.ObjectCount >= 1, "author member count");
    Probe.Require(receipt.Value.Document.ResourceChanged.Count >= 1, "author resource receipt");
});
