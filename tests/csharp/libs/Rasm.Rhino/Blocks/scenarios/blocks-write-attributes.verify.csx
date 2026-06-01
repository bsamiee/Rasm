using System;
using Rasm.Rhino.Blocks;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

Scenario.Run("blocks-write-attributes", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    RhinoBlocks blocks = RhinoBlocks.Live(document: scope.Active, mode: RunMode.Scripted);
    DefinitionName blockName = Probe.Expect(
        DefinitionName.Sanitize(value: $"RasmVerifyAttr{Guid.NewGuid():N}", key: key),
        "block name");
    Brep brep = Brep.CreateFromBox(new Box(
        Plane.WorldXY,
        new Interval(t0: 0.0, t1: 10.0),
        new Interval(t0: 0.0, t1: 5.0),
        new Interval(t0: 0.0, t1: 3.0))) ?? throw new InvalidOperationException(message: "box brep");
    Probe.Require(
        scope.Active.InstanceDefinitions.Add(
            name: blockName.Value,
            description: string.Empty,
            basePoint: Point3d.Origin,
            geometry: brep,
            attributes: new ObjectAttributes()) >= 0,
        "block add");
    DefinitionRef refer = DefinitionRef.Of(name: blockName);
    BlockOutcome placed = Probe.Expect(
        blocks.Run(
            op: new BlockOp.Instance(new BlockInstanceTask.Place(
                Ref: refer,
                At: Seq(Placement.Of(xform: Transform.Identity)),
                Policy: BatchPolicy.Default)),
            key: key),
        "place");
    BlockOutcome.Receipt receipt = placed is BlockOutcome.Receipt value
        ? value
        : throw new InvalidOperationException(message: $"unexpected place outcome: {placed.GetType().Name}");
    Guid instanceId = receipt.Value.Document.Created[0];
    facts.Add("instanceId", instanceId);
    HashMap<string, string> values = HashMap<string, string>().AddOrUpdate(key: "Mark", value: "Written");
    BlockOutcome written = Probe.Expect(
        blocks.Run(
            op: new BlockOp.Attributes(new BlockAttributeTask.Write(
                Ref: refer,
                Values: values,
                Policy: ConstraintPolicy.Extend,
                InstanceId: Some(instanceId))),
            key: key),
        "write attributes");
    BlockOutcome.Receipt writeReceipt = written is BlockOutcome.Receipt writeValue
        ? writeValue
        : throw new InvalidOperationException(message: $"unexpected write outcome: {written.GetType().Name}");
    facts.Add("attributeChanged", writeReceipt.Value.Document.AttributeChanged.Count);
    Probe.Require(writeReceipt.Value.Document.AttributeChanged.Contains(instanceId), "attribute changed id");
    InstanceObject instance = scope.Active.Objects.FindId(id: instanceId) as InstanceObject
        ?? throw new InvalidOperationException(message: "instance missing");
    string read = instance.Attributes.GetUserString(key: "Mark") ?? string.Empty;
    facts.Add("readMark", read);
    Probe.Require(string.Equals(a: read, b: "Written", comparisonType: StringComparison.Ordinal), $"readMark={read}");
});
