using System;
using System.IO;
using LanguageExt;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using static LanguageExt.Prelude;

Scenario.Run("blocks-placement-reference", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    RhinoBlocks blocks = RhinoBlocks.Live(document: scope.Active, mode: RunMode.Scripted);
    DefinitionName staticName = Probe.Expect(DefinitionName.Sanitize(value: $"RasmStatic{Guid.NewGuid():N}", key: key), "static name");
    Brep brep = Brep.CreateFromBox(new Box(
        Plane.WorldXY,
        new Interval(t0: 0.0, t1: 8.0),
        new Interval(t0: 0.0, t1: 4.0),
        new Interval(t0: 0.0, t1: 2.0))) ?? throw new InvalidOperationException(message: "box brep");
    Probe.Require(
        scope.Active.InstanceDefinitions.Add(
            name: staticName.Value,
            description: string.Empty,
            basePoint: Point3d.Origin,
            geometry: brep,
            attributes: new ObjectAttributes()) >= 0,
        "static block add");
    BlockOp staticPlace = new BlockOp.Place(
        Ref: DefinitionRef.Of(name: staticName),
        At: Seq(Placement.Of(xform: Transform.Identity, reference: true)));
    Probe.ExpectRejected(result: blocks.Run(op: staticPlace, key: key), label: "static reference place");
    facts.Add("staticReferenceRejected", true);
    string tempDir = Path.Combine(path1: Path.GetTempPath(), path2: $"RasmLink{Guid.NewGuid():N}");
    Directory.CreateDirectory(path: tempDir);
    string childPath = Path.Combine(path1: tempDir, path2: "child.3dm");
    string childFullPath = Path.GetFullPath(path: childPath);
    using (File3dm child = new()) {
        Brep childBrep = Brep.CreateFromBox(new Box(
            Plane.WorldXY,
            new Interval(t0: 0.0, t1: 5.0),
            new Interval(t0: 0.0, t1: 3.0),
            new Interval(t0: 0.0, t1: 1.0))) ?? throw new InvalidOperationException(message: "child brep");
        Probe.Require(
            child.AllInstanceDefinitions.Add(
                name: "ChildBlock",
                description: string.Empty,
                basePoint: Point3d.Origin,
                geometry: childBrep,
                attributes: new ObjectAttributes()) >= 0,
            "child block add");
        Probe.Require(child.Objects.Add(childBrep, new ObjectAttributes()) != Guid.Empty, "child model object");
        Probe.Require(child.Write(path: childPath, version: 8), "child write");
    }
    FileEndpoint source = Probe.Expect(FileEndpoint.From(path: childFullPath), "child endpoint");
    facts.Add("child.path", childFullPath);
    BlockOp linkOp = new BlockOp.CreateArchiveLinks(
        Sources: Seq(source),
        Policy: new LinkCreatePolicy(Update: UpdatePolicy.Linked, Layer: LayerStyle.Reference));
    BlockOutcome linkOutcome = Probe.Expect(blocks.Run(op: linkOp, key: key), "create archive links");
    BlockOutcome.Receipt linkReceipt = linkOutcome is BlockOutcome.Receipt created
        ? created
        : throw new InvalidOperationException(message: $"unexpected link outcome: {linkOutcome.GetType().Name}");
    facts.Add("link.receipt.changes", linkReceipt.Value.Document.ResourceChanged.Count);
    string linkedBlockName = linkReceipt.Value.Document.ResourceChanged
        .Find(change => change.Kind == DocumentResourceKind.Block)
        .Map(change => change.Name)
        .IfNone(string.Empty);
    facts.Add("link.receipt.name", linkedBlockName);
    Probe.Require(!string.IsNullOrWhiteSpace(value: linkedBlockName), "link receipt block name");
    InstanceDefinition linked = scope.Active.InstanceDefinitions.Find(instanceDefinitionName: linkedBlockName)
        ?? throw new InvalidOperationException(message: $"linked definition missing for receipt name {linkedBlockName}");
    facts.Add("linked.name", linked.Name ?? string.Empty);
    facts.Add("linked.sourceArchive", linked.SourceArchive ?? string.Empty);
    facts.Add("linked.layerStyle", linked.LayerStyle.ToString());
    facts.Add("linked.updateType", linked.UpdateType.ToString());
    facts.Add("linked.objectCount", linked.ObjectCount);
    facts.Add("linked.memberCount", linked.GetObjectIds()?.Length ?? 0);
    Probe.Require(linked.ObjectCount >= 1, $"linked geometry empty: objectCount={linked.ObjectCount}");
    Probe.Require(linked.UpdateType == InstanceDefinitionUpdateType.Linked, "linked update");
    Probe.Require(linked.LayerStyle == InstanceDefinitionLayerStyle.Reference, "linked layer style");
    DefinitionName linkedDefName = Probe.Expect(DefinitionName.From(value: linked.Name ?? string.Empty, key: key), "linked name");
    BlockOp linkedPlace = new BlockOp.Place(
        Ref: DefinitionRef.Of(name: linkedDefName),
        At: Seq(Placement.Of(xform: Transform.Identity, reference: true)));
    BlockOutcome linkedOutcome = Probe.Expect(blocks.Run(op: linkedPlace, key: key), "linked reference place");
    BlockOutcome.Receipt linkedReceipt = linkedOutcome is BlockOutcome.Receipt receipt
        ? receipt
        : throw new InvalidOperationException(message: $"unexpected place outcome: {linkedOutcome.GetType().Name}");
    Probe.Require(linkedReceipt.Value.Document.Created.Count == 1, "placed instance count");
    Guid instanceId = linkedReceipt.Value.Document.Created[0];
    InstanceObject placed = scope.Active.Objects.FindId(id: instanceId) as InstanceObject
        ?? throw new InvalidOperationException(message: "placed instance missing");
    facts.Add("instance.id", instanceId);
    facts.Add("instance.definition", placed.InstanceDefinition?.Name ?? string.Empty);
});
