using System;
using System.IO;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;

Scenario.Run("blocks-native-add", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    string tempDir = Path.Combine(path1: Path.GetTempPath(), path2: $"RasmLinkNative{Guid.NewGuid():N}");
    Directory.CreateDirectory(path: tempDir);
    string childPath = Path.GetFullPath(path: Path.Combine(path1: tempDir, path2: "child.3dm"));
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

    string linkName = scope.Active.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: "child");
    int linkIdx = scope.Active.InstanceDefinitions.Add(
        name: linkName,
        description: string.Empty,
        basePoint: Point3d.Origin,
        geometry: System.Array.Empty<GeometryBase>(),
        attributes: System.Array.Empty<ObjectAttributes>());
    facts.Add("link.idx", linkIdx);
    facts.Add("child.path", childPath);
    facts.Add("child.exists", File.Exists(path: childPath));
    InstanceDefinition placeholder = linkIdx >= 0
        ? scope.Active.InstanceDefinitions[linkIdx]
        : null;
    facts.Add("link.objectCount.before", placeholder?.ObjectCount ?? -1);
    facts.Add("link.updateType.before", placeholder?.UpdateType.ToString() ?? "missing");

    using FileReference reference = FileReference.CreateFromFullPath(fullPath: childPath);
    facts.Add("reference.isSet", reference.IsSet);
    bool attached = linkIdx >= 0 && scope.Active.InstanceDefinitions.ModifySourceArchive(
        idefIndex: linkIdx,
        sourceArchive: reference,
        updateType: InstanceDefinitionUpdateType.Linked,
        quiet: true);
    facts.Add("link.attached", attached);
    InstanceDefinition afterAttach = linkIdx >= 0
        ? scope.Active.InstanceDefinitions[linkIdx]
        : null;
    facts.Add("link.updateType.afterAttach", afterAttach?.UpdateType.ToString() ?? "missing");
    facts.Add("link.sourceArchive.afterAttach", afterAttach?.SourceArchive ?? string.Empty);

    bool loaded = linkIdx >= 0 && scope.Active.InstanceDefinitions.UpdateLinkedInstanceDefinition(
        idefIndex: linkIdx,
        filename: childPath,
        updateNestedLinks: true,
        quiet: true);
    facts.Add("link.loaded", loaded);
    bool refreshed = linkIdx >= 0
        && afterAttach is not null
        && scope.Active.InstanceDefinitions.RefreshLinkedBlock(definition: afterAttach);
    facts.Add("link.refreshed", refreshed);

    InstanceDefinition linked = linkIdx >= 0
        ? scope.Active.InstanceDefinitions[linkIdx]
        : null;
    facts.Add("link.objectCount.after", linked?.ObjectCount ?? -1);
    facts.Add("link.updateType.after", linked?.UpdateType.ToString() ?? "missing");
    facts.Add("link.sourceArchive", linked?.SourceArchive ?? string.Empty);
    Probe.Require(linkIdx >= 0, "link add");
    Probe.Require(
        afterAttach?.UpdateType is InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded,
        "linked attach state");
    Probe.Require(!string.IsNullOrWhiteSpace(value: afterAttach?.SourceArchive), "source archive set");
    Probe.Require(linked is { ObjectCount: >= 1 }, $"linked geometry empty: objectCount={linked?.ObjectCount ?? -1}");
});
