using System.Diagnostics.CodeAnalysis;
using Rasm.Bridge.Scenarios;
using Rasm.Domain;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;

namespace Rasm.Rhino.Scenarios;

// --- [OPERATIONS] ---------------------------------------------------------------------------

// Ownership: the Blocks theme — the pilot scenario corpus on the bridge scenario SDK. Each
// entrypoint is one Fin rail over ScenarioContext: Require fuses assert+fact, Expect facts every
// projection, DocumentScope restores the document on the green path and stays live on failure so
// the runner's auto-capture can shoot the evidence frame before draining the scope.
internal static class BlocksScenarios {
    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> CoreRail(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let op = Op.Of()
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        from name in ctx.Expect(label: "blockName", projection: DefinitionName.Sanitize(value: Stamp(stem: "RasmVerifyCore"), key: op))
        from brep in BoxBrep(x: 12.0, y: 6.0, z: 4.0)
        let authored = Note(ctx: ctx, key: "native.idx", value: AddDefinition(doc: scope.Doc, name: name, brep: brep))
        from added in ctx.Require(label: "native add", observed: authored >= 0)
        from live in Found(doc: scope.Doc, name: name.Value)
        let memberCount = Note(ctx: ctx, key: "member.count", value: live.ObjectCount)
        from nativeMembers in ctx.Require(label: "native member count", observed: memberCount >= 1)
        from outcome in ctx.Expect(
            label: "graph run",
            projection: RhinoBlocks.Live(document: scope.Doc, mode: RunMode.Scripted)
                .Run(op: new BlockOp.Graph(Query: new GraphQuery.Members(Ref: DefinitionRef.Of(name: name))), key: op))
        from result in As<BlockOutcome.MembersResult>(outcome: outcome)
        let runMembers = Note(ctx: ctx, key: "run.members", value: result.Values.Count)
        from graphMembers in ctx.Require(label: "graph members", observed: runMembers >= 1)
        select Done(scope: scope);

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> Stats(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let op = Op.Of()
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        from name in ctx.Expect(label: "blockName", projection: DefinitionName.Sanitize(value: Stamp(stem: "RasmVerifyStats"), key: op))
        from brep in BoxBrep(x: 12.0, y: 6.0, z: 4.0)
        from added in ctx.Require(label: "native add", observed: AddDefinition(doc: scope.Doc, name: name, brep: brep) >= 0)
        from outcome in ctx.Expect(
            label: "stats run",
            projection: RhinoBlocks.Live(document: scope.Doc, mode: RunMode.Scripted)
                .Run(op: new BlockOp.Graph(Query: new GraphQuery.Stats()), key: op))
        from stats in As<BlockOutcome.TableStats>(outcome: outcome)
        let total = Note(ctx: ctx, key: "table.count", value: stats.Count)
        let active = Note(ctx: ctx, key: "table.activeCount", value: stats.ActiveCount)
        from tableCount in ctx.Require(label: "table count", observed: total >= 1)
        from activeCount in ctx.Require(label: "active count", observed: active >= 1)
        select Done(scope: scope);

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> GraphPlan(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let op = Op.Of()
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        from name in ctx.Expect(label: "blockName", projection: DefinitionName.Sanitize(value: Stamp(stem: "RasmVerifyPlan"), key: op))
        from brep in BoxBrep(x: 12.0, y: 6.0, z: 4.0)
        from added in ctx.Require(label: "native add", observed: AddDefinition(doc: scope.Doc, name: name, brep: brep) >= 0)
        from outcome in ctx.Expect(
            label: "graph plan run",
            projection: RhinoBlocks.Live(document: scope.Doc, mode: RunMode.Scripted)
                .Run(op: new BlockOp.Graph(Query: new GraphQuery.Plan(Root: DefinitionRef.Of(name: name))), key: op))
        from plan in As<BlockOutcome.Plan>(outcome: outcome)
        let order = Note(ctx: ctx, key: "plan.order", value: plan.Order.Count)
        from planOrder in ctx.Require(label: "bake plan order", observed: order >= 1)
        select Done(scope: scope);

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> Author(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let op = Op.Of()
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        let blocks = RhinoBlocks.Live(document: scope.Doc, mode: RunMode.Scripted)
        from name in ctx.Expect(label: "blockName", projection: DefinitionName.Sanitize(value: Stamp(stem: "RasmVerifyAuthor"), key: op))
        from brep in BoxBrep(x: 12.0, y: 6.0, z: 4.0)
        from spec in ctx.Expect(label: "author spec", projection: AuthorSpec.Of(name: name, basePoint: Point3d.Origin, key: op))
        from source in ctx.Expect(label: "members", projection: Members.Of(geometry: Seq<GeometryBase>(brep), key: op))
        from outcome in ctx.Expect(
            label: "author run",
            projection: blocks.Run(op: new BlockOp.Author(Spec: spec, Source: source, Conflict: ConflictPolicy.Fail), key: op))
        from receipt in As<BlockOutcome.Receipt>(outcome: outcome)
        from live in Found(doc: scope.Doc, name: name.Value)
        let created = Note(ctx: ctx, key: "receipt.created", value: receipt.Value.Document.Created.Count)
        let memberCount = Note(ctx: ctx, key: "member.count", value: live.ObjectCount)
        from authorMembers in ctx.Require(label: "author member count", observed: memberCount >= 1)
        from resourceReceipt in ctx.Require(label: "author resource receipt", observed: receipt.Value.Document.ResourceChanged.Count >= 1)
        select Done(scope: scope);

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> Bounds(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let op = Op.Of()
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        from name in ctx.Expect(label: "blockName", projection: DefinitionName.Sanitize(value: Stamp(stem: "RasmVerifyBounds"), key: op))
        from brep in BoxBrep(x: 12.0, y: 6.0, z: 4.0)
        let authored = Note(ctx: ctx, key: "native.idx", value: AddDefinition(doc: scope.Doc, name: name, brep: brep))
        from added in ctx.Require(label: "native add", observed: authored >= 0)
        from live in Found(doc: scope.Doc, name: name.Value)
        let memberCount = Note(ctx: ctx, key: "member.count", value: live.ObjectCount)
        from nativeMembers in ctx.Require(label: "native member count", observed: memberCount >= 1)
        let blocks = RhinoBlocks.Live(document: scope.Doc, mode: RunMode.Scripted)
        from used in ctx.Expect(
            label: "use.name",
            projection: blocks.Use(refer: DefinitionRef.Of(name: name), project: static def => Fin.Succ(value: def.Name ?? string.Empty), key: op))
        from outcome in ctx.Expect(
            label: "bounds run",
            projection: blocks.Run(op: new BlockOp.Instance(Task: new BlockInstanceTask.Bounds(Ref: DefinitionRef.Of(name: name), Policy: BoundsPolicy.Default)), key: op))
        from boxed in As<BlockOutcome.Bounds>(outcome: outcome)
        let valid = Note(ctx: ctx, key: "bounds.valid", value: boxed.Value.IsValid)
        let diagonal = Note(ctx: ctx, key: "bounds.diagonal", value: boxed.Value.Diagonal.Length)
        from boundsValid in ctx.Require(label: "bounds valid", observed: valid)
        select Done(scope: scope);

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> WriteAttributes(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let op = Op.Of()
        let blocks = RhinoBlocks.Live(document: scope.Doc, mode: RunMode.Scripted)
        from name in ctx.Expect(label: "blockName", projection: DefinitionName.Sanitize(value: Stamp(stem: "RasmVerifyAttr"), key: op))
        from brep in BoxBrep(x: 10.0, y: 5.0, z: 3.0)
        from added in ctx.Require(label: "block add", observed: AddDefinition(doc: scope.Doc, name: name, brep: brep) >= 0)
        let refer = DefinitionRef.Of(name: name)
        from placedOutcome in ctx.Expect(
            label: "place",
            projection: blocks.Run(
                op: new BlockOp.Instance(Task: new BlockInstanceTask.Place(Ref: refer, At: Seq(Placement.Of(xform: Transform.Identity)), Policy: BatchPolicy.Default)),
                key: op))
        from placed in As<BlockOutcome.Receipt>(outcome: placedOutcome)
        from instanceId in placed.Value.Document.Created.Head.ToFin(Fail: Error.New(message: "place produced no created instance id"))
        let idFact = Note(ctx: ctx, key: "instanceId", value: instanceId)
        from writtenOutcome in ctx.Expect(
            label: "write attributes",
            projection: blocks.Run(
                op: new BlockOp.Attributes(Task: new BlockAttributeTask.Write(
                    Ref: refer,
                    Values: HashMap(("Mark", "Written")),
                    Policy: ConstraintPolicy.Extend,
                    InstanceId: Some(instanceId))),
                key: op))
        from written in As<BlockOutcome.Receipt>(outcome: writtenOutcome)
        let changed = Note(ctx: ctx, key: "attributeChanged", value: written.Value.Document.AttributeChanged.Count)
        from changedId in ctx.Require(label: "attribute changed id", observed: written.Value.Document.AttributeChanged.Exists(id => id == instanceId))
        from instance in Optional(scope.Doc.Objects.FindId(id: instanceId) as InstanceObject)
            .ToFin(Fail: Error.New(message: "instance missing"))
        let mark = Note(ctx: ctx, key: "readMark", value: instance.Attributes.GetUserString(key: "Mark") ?? string.Empty)
        from readBack in ctx.Require(label: "readMark written", observed: string.Equals(a: mark, b: "Written", comparisonType: StringComparison.Ordinal))
        select Done(scope: scope);

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> ArchiveClosure(ScenarioContext ctx) =>
        from op in Fin.Succ(value: Op.Of())
        let root = Path.Combine(path1: Path.GetTempPath(), path2: Stamp(stem: "RasmClosure"))
        let nestedPath = Path.Combine(path1: root, path2: "nested.3dm")
        let childPath = Path.Combine(path1: root, path2: "sub", path3: "child.3dm")
        let parentPath = Path.Combine(path1: root, path2: "parent.3dm")
        from nested in WriteModel(path: nestedPath, compose: static _ => true)
        from child in WriteModel(
            path: childPath,
            compose: static model => model.AllInstanceDefinitions.AddLinked(
                filename: Path.Combine(path1: "..", path2: "nested.3dm"), name: "NestedLink", description: string.Empty) >= 0)
        from parent in WriteModel(
            path: parentPath,
            compose: static model => model.AllInstanceDefinitions.AddLinked(
                filename: Path.Combine(path1: "sub", path2: "child.3dm"), name: "ChildLink", description: string.Empty) >= 0)
        from walked in WithModel(path: parentPath, project: model =>
            from edges in Archive.LinkedArchiveClosure(root: model, rootPath: parentPath, key: op)
            from report in Archive.ValidateArchiveClosure(root: model, rootPath: parentPath, key: op)
            select (Edges: edges, Report: report))
        let countFact = Note(ctx: ctx, key: "closure.count", value: walked.Edges.Count)
        let validFact = Note(ctx: ctx, key: "validate.valid", value: walked.Report.Valid)
        let brokenFact = Note(ctx: ctx, key: "validate.broken", value: walked.Report.Broken.Count)
        let cyclesFact = Note(ctx: ctx, key: "validate.cycles", value: walked.Report.Cycles.Count)
        from closureCount in ctx.Require(label: "closure count", observed: walked.Edges.Count == 2)
        from reportValid in ctx.Require(label: "validate valid", observed: walked.Report.Valid)
        from brokenEmpty in ctx.Require(label: "validate broken empty", observed: walked.Report.Broken.IsEmpty)
        from cyclesEmpty in ctx.Require(label: "validate cycles empty", observed: walked.Report.Cycles.IsEmpty)
        from edgesMatch in ctx.Require(label: "validate edges", observed: walked.Report.Edges.Count == walked.Edges.Count)
        let childFact = Note(ctx: ctx, key: "closure.child", value: walked.Edges[0].Link.Full.Value)
        let nestedFact = Note(ctx: ctx, key: "closure.nested", value: walked.Edges[1].Link.Full.Value)
        from depthZero in ctx.Require(label: "depth0", observed: walked.Edges[0].Depth == 0)
        from childMatch in ctx.Require(label: "child path", observed: string.Equals(
            a: Path.GetFullPath(path: childPath), b: walked.Edges[0].Link.Full.Value, comparisonType: StringComparison.OrdinalIgnoreCase))
        from depthOne in ctx.Require(label: "depth1", observed: walked.Edges[1].Depth == 1)
        from nestedMatch in ctx.Require(label: "nested path", observed: string.Equals(
            a: Path.GetFullPath(path: nestedPath), b: walked.Edges[1].Link.Full.Value, comparisonType: StringComparison.OrdinalIgnoreCase))
        select unit;

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> ArchiveValidateBroken(ScenarioContext ctx) =>
        from op in Fin.Succ(value: Op.Of())
        let root = Path.Combine(path1: Path.GetTempPath(), path2: Stamp(stem: "RasmBroken"))
        let parentPath = Path.Combine(path1: root, path2: "parent.3dm")
        from parent in WriteModel(
            path: parentPath,
            compose: static model => model.AllInstanceDefinitions.AddLinked(
                filename: "missing-child.3dm", name: "MissingLink", description: string.Empty) >= 0)
        from direct in WithModel(path: parentPath, project: model => Archive.ValidateArchiveClosure(root: model, rootPath: parentPath, key: op))
        let directValid = Note(ctx: ctx, key: "direct.valid", value: direct.Valid)
        let directBroken = Note(ctx: ctx, key: "direct.broken", value: direct.Broken.Count)
        from directInvalid in ctx.Require(label: "direct invalid", observed: !direct.Valid)
        from directBrokenSome in ctx.Require(label: "direct broken", observed: direct.Broken.Count >= 1)
        from scope in DocumentScope.Open(ctx: ctx)
        from endpoint in ctx.Expect(label: "endpoint", projection: FileEndpoint.From(path: parentPath))
        from outcome in ctx.Expect(
            label: "validate op",
            projection: RhinoBlocks.Live(document: scope.Doc, mode: RunMode.Scripted)
                .Run(op: new BlockOp.ValidateArchiveClosure(Source: endpoint), key: op))
        from routed in As<BlockOutcome.ClosureReport>(outcome: outcome)
        let routedValid = Note(ctx: ctx, key: "op.valid", value: routed.Value.Valid)
        let routedBroken = Note(ctx: ctx, key: "op.broken", value: routed.Value.Broken.Count)
        from opInvalid in ctx.Require(label: "op invalid", observed: !routed.Value.Valid)
        from opBrokenSome in ctx.Require(label: "op broken", observed: routed.Value.Broken.Count >= 1)
        select Done(scope: scope);

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> NativeAdd(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let root = Path.Combine(path1: Path.GetTempPath(), path2: Stamp(stem: "RasmLinkNative"))
        let childPath = Path.GetFullPath(path: Path.Combine(path1: root, path2: "child.3dm"))
        from childBrep in BoxBrep(x: 5.0, y: 3.0, z: 1.0)
        from child in WriteModel(path: childPath, compose: model =>
            model.AllInstanceDefinitions.Add(
                name: "ChildBlock", description: string.Empty, basePoint: Point3d.Origin, geometry: childBrep, attributes: new ObjectAttributes()) >= 0
            && model.Objects.Add(childBrep, new ObjectAttributes()) != Guid.Empty)
        let pathFact = Note(ctx: ctx, key: "child.path", value: childPath)
        let existsFact = Note(ctx: ctx, key: "child.exists", value: File.Exists(path: childPath))
        let table = scope.Doc.InstanceDefinitions
        let linkIdx = Note(ctx: ctx, key: "link.idx", value: table.Add(
            name: table.GetUnusedInstanceDefinitionName(root: "child"),
            description: string.Empty,
            basePoint: Point3d.Origin,
            geometry: System.Array.Empty<GeometryBase>(),
            attributes: System.Array.Empty<ObjectAttributes>()))
        from linkAdded in ctx.Require(label: "link add", observed: linkIdx >= 0)
        let beforeCount = Note(ctx: ctx, key: "link.objectCount.before", value: table[linkIdx].ObjectCount)
        let beforeType = Note(ctx: ctx, key: "link.updateType.before", value: table[linkIdx].UpdateType.ToString())
        let attached = Note(ctx: ctx, key: "link.attached", value: AttachLink(doc: scope.Doc, index: linkIdx, path: childPath))
        let afterAttach = table[linkIdx]
        let afterType = Note(ctx: ctx, key: "link.updateType.afterAttach", value: afterAttach.UpdateType.ToString())
        let afterSource = Note(ctx: ctx, key: "link.sourceArchive.afterAttach", value: afterAttach.SourceArchive ?? string.Empty)
        from attachState in ctx.Require(
            label: "linked attach state",
            observed: afterAttach.UpdateType is InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded)
        from sourceSet in ctx.Require(label: "source archive set", observed: !string.IsNullOrWhiteSpace(value: afterAttach.SourceArchive))
        let loaded = Note(ctx: ctx, key: "link.loaded", value: table.UpdateLinkedInstanceDefinition(
            idefIndex: linkIdx, filename: childPath, updateNestedLinks: true, quiet: true))
        let refreshed = Note(ctx: ctx, key: "link.refreshed", value: table.RefreshLinkedBlock(definition: afterAttach))
        let linked = table[linkIdx]
        let afterCount = Note(ctx: ctx, key: "link.objectCount.after", value: linked.ObjectCount)
        let finalType = Note(ctx: ctx, key: "link.updateType.after", value: linked.UpdateType.ToString())
        let finalSource = Note(ctx: ctx, key: "link.sourceArchive", value: linked.SourceArchive ?? string.Empty)
        from geometryLoaded in ctx.Require(label: "linked geometry", observed: linked.ObjectCount >= 1)
        select Done(scope: scope);

    [RhinoScenario(theme: "blocks")]
    internal static Fin<Unit> PlacementReference(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let op = Op.Of()
        let blocks = RhinoBlocks.Live(document: scope.Doc, mode: RunMode.Scripted)
        from staticName in ctx.Expect(label: "static name", projection: DefinitionName.Sanitize(value: Stamp(stem: "RasmStatic"), key: op))
        from brep in BoxBrep(x: 8.0, y: 4.0, z: 2.0)
        from staticAdded in ctx.Require(label: "static block add", observed: AddDefinition(doc: scope.Doc, name: staticName, brep: brep) >= 0)
        from rejected in Rejected(
            outcome: blocks.Run(
                op: new BlockOp.Instance(Task: new BlockInstanceTask.Place(
                    Ref: DefinitionRef.Of(name: staticName), At: Seq(Placement.Of(xform: Transform.Identity, reference: true)))),
                key: op),
            detail: "static reference place: expected rejection")
        let rejectedFact = Note(ctx: ctx, key: "staticReferenceRejected", value: true)
        let root = Path.Combine(path1: Path.GetTempPath(), path2: Stamp(stem: "RasmLink"))
        let childFullPath = Path.GetFullPath(path: Path.Combine(path1: root, path2: "child.3dm"))
        from childBrep in BoxBrep(x: 5.0, y: 3.0, z: 1.0)
        from child in WriteModel(path: childFullPath, compose: model =>
            model.AllInstanceDefinitions.Add(
                name: "ChildBlock", description: string.Empty, basePoint: Point3d.Origin, geometry: childBrep, attributes: new ObjectAttributes()) >= 0
            && model.Objects.Add(childBrep, new ObjectAttributes()) != Guid.Empty)
        from endpoint in ctx.Expect(label: "child endpoint", projection: FileEndpoint.From(path: childFullPath))
        let pathFact = Note(ctx: ctx, key: "child.path", value: childFullPath)
        from linkOutcome in ctx.Expect(
            label: "create archive links",
            projection: blocks.Run(
                op: new BlockOp.Linked(Change: new LinkLifecycle.Create(
                    Sources: Seq(endpoint),
                    Policy: new LinkCreatePolicy(Update: UpdatePolicy.Linked, Layer: LayerStyle.Reference))),
                key: op))
        from linkReceipt in As<BlockOutcome.Receipt>(outcome: linkOutcome)
        let changesFact = Note(ctx: ctx, key: "link.receipt.changes", value: linkReceipt.Value.Document.ResourceChanged.Count)
        let linkedBlockName = linkReceipt.Value.Document.ResourceChanged
            .Find(static change => change.Kind == DocumentResourceKind.Block)
            .Map(f: static change => change.Name)
            .IfNone(string.Empty)
        let nameFact = Note(ctx: ctx, key: "link.receipt.name", value: linkedBlockName)
        from receiptName in ctx.Require(label: "link receipt block name", observed: !string.IsNullOrWhiteSpace(value: linkedBlockName))
        from linked in Found(doc: scope.Doc, name: linkedBlockName)
        let linkedName = Note(ctx: ctx, key: "linked.name", value: linked.Name ?? string.Empty)
        let linkedSource = Note(ctx: ctx, key: "linked.sourceArchive", value: linked.SourceArchive ?? string.Empty)
        let linkedLayer = Note(ctx: ctx, key: "linked.layerStyle", value: linked.LayerStyle.ToString())
        let linkedUpdate = Note(ctx: ctx, key: "linked.updateType", value: linked.UpdateType.ToString())
        let linkedCount = Note(ctx: ctx, key: "linked.objectCount", value: linked.ObjectCount)
        let linkedMembers = Note(ctx: ctx, key: "linked.memberCount", value: linked.GetObjectIds()?.Length ?? 0)
        from linkedGeometry in ctx.Require(label: "linked geometry", observed: linked.ObjectCount >= 1)
        from linkedUpdateType in ctx.Require(label: "linked update", observed: linked.UpdateType == InstanceDefinitionUpdateType.Linked)
        from linkedLayerStyle in ctx.Require(label: "linked layer style", observed: linked.LayerStyle == InstanceDefinitionLayerStyle.Reference)
        from linkedDefName in ctx.Expect(label: "linked name", projection: DefinitionName.From(value: linked.Name ?? string.Empty, key: op))
        from placeOutcome in ctx.Expect(
            label: "linked reference place",
            projection: blocks.Run(
                op: new BlockOp.Instance(Task: new BlockInstanceTask.Place(
                    Ref: DefinitionRef.Of(name: linkedDefName), At: Seq(Placement.Of(xform: Transform.Identity, reference: true)))),
                key: op))
        from placedReceipt in As<BlockOutcome.Receipt>(outcome: placeOutcome)
        from placedCount in ctx.Require(label: "placed instance count", observed: placedReceipt.Value.Document.Created.Count == 1)
        from instanceId in placedReceipt.Value.Document.Created.Head.ToFin(Fail: Error.New(message: "placed instance id missing"))
        from placedObject in Optional(scope.Doc.Objects.FindId(id: instanceId) as InstanceObject)
            .ToFin(Fail: Error.New(message: "placed instance missing"))
        let idFact = Note(ctx: ctx, key: "instance.id", value: instanceId)
        let defFact = Note(ctx: ctx, key: "instance.definition", value: placedObject.InstanceDefinition?.Name ?? string.Empty)
        select Done(scope: scope);

    private static int AddDefinition(RhinoDoc doc, DefinitionName name, Brep brep) =>
        doc.InstanceDefinitions.Add(
            name: name.Value, description: string.Empty, basePoint: Point3d.Origin, geometry: brep, attributes: new ObjectAttributes());

    private static Fin<TCase> As<TCase>(BlockOutcome outcome) where TCase : BlockOutcome =>
        outcome is TCase admitted
            ? Fin.Succ(value: admitted)
            : Fin.Fail<TCase>(error: Error.New(message: $"unexpected outcome: {outcome.GetType().Name}"));

    // BOUNDARY ADAPTER — FileReference is a native handle; the bracket owns its disposal so the
    // attach projection stays a pure bool inside the scenario rail.
    private static bool AttachLink(RhinoDoc doc, int index, string path) {
        using FileReference reference = FileReference.CreateFromFullPath(fullPath: path);
        return reference.IsSet && doc.InstanceDefinitions.ModifySourceArchive(
            idefIndex: index, sourceArchive: reference, updateType: InstanceDefinitionUpdateType.Linked, quiet: true);
    }

    [SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "Ownership transfers into the Fin rail; the document/table copies the geometry and the transient brep is finalizer-released.")]
    private static Fin<Brep> BoxBrep(double x, double y, double z) =>
        Optional(Brep.CreateFromBox(new Box(
            Plane.WorldXY,
            new Interval(t0: 0.0, t1: x),
            new Interval(t0: 0.0, t1: y),
            new Interval(t0: 0.0, t1: z))))
        .ToFin(Fail: Error.New(message: "box brep construction failed"));

    private static Unit Done(DocumentScope scope) {
        scope.Dispose();
        return unit;
    }

    private static Fin<InstanceDefinition> Found(RhinoDoc doc, string name) =>
        Optional(doc.InstanceDefinitions.Find(instanceDefinitionName: name))
            .ToFin(Fail: Error.New(message: $"native find by name failed: {name}"));

    private static T Note<T>(ScenarioContext ctx, string key, T value) {
        ctx.Fact(key: key, value: value);
        return value;
    }

    private static Fin<Unit> Rejected<T>(Fin<T> outcome, string detail) =>
        outcome switch {
            Fin<T>.Fail => Fin.Succ(value: unit),
            _ => Fin.Fail<Unit>(error: Error.New(message: detail)),
        };

    private static string Stamp(string stem) =>
        $"{stem}{Guid.NewGuid():N}";

    // BOUNDARY ADAPTER — File3dm fixtures are write-once native models; the bracket owns model
    // disposal and directory creation so fixture composition stays a pure bool predicate.
    private static Fin<string> WriteModel(string path, Func<File3dm, bool> compose) {
        using File3dm model = new();
        _ = Directory.CreateDirectory(path: Path.GetDirectoryName(path: path) ?? ".");
        return compose(model) && model.Write(path: path, version: 8)
            ? Fin.Succ(value: path)
            : Fin.Fail<string>(error: Error.New(message: $"model fixture failed: {path}"));
    }

    // BOUNDARY ADAPTER — File3dm.Read returns a null-on-failure native handle; the bracket owns
    // disposal across the projection.
    private static Fin<T> WithModel<T>(string path, Func<File3dm, Fin<T>> project) {
        using File3dm? model = File3dm.Read(path: path);
        return model is null
            ? Fin.Fail<T>(error: Error.New(message: $"archive read failed: {path}"))
            : project(model);
    }
}
