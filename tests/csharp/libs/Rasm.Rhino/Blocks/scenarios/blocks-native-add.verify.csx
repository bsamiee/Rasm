using System;
using System.IO;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;

Scenario.Run("blocks-native-add", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    Brep brep = Brep.CreateFromBox(new Box(
        Plane.WorldXY,
        new Interval(0.0, 12.0),
        new Interval(0.0, 6.0),
        new Interval(0.0, 4.0))) ?? throw new InvalidOperationException(message: "box brep");
    string name = $"RasmNative{Guid.NewGuid():N}";
    int idx = scope.Active.InstanceDefinitions.Add(
        name: name,
        description: string.Empty,
        basePoint: Point3d.Origin,
        geometry: brep,
        attributes: new ObjectAttributes());
    InstanceDefinition live = scope.Active.InstanceDefinitions.Find(instanceDefinitionName: name)
        ?? throw new InvalidOperationException(message: "native definition missing");
    facts.Add("idx", idx);
    facts.Add("name", name);
    facts.Add("objectCount", live.ObjectCount);
    Probe.Require(idx >= 0, $"idx={idx}");
    Probe.Require(live.ObjectCount >= 1, $"objectCount={live.ObjectCount}");

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
        Probe.Require(child.Write(path: childPath, version: 8), "child write");
    }

    string linkName = scope.Active.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: "child");
    int linkIdx = scope.Active.InstanceDefinitions.Add(
        name: linkName,
        description: string.Empty,
        basePoint: Point3d.Origin,
        geometry: [],
        attributes: []);
    Probe.Require(linkIdx >= 0, $"linkIdx={linkIdx}");
    facts.Add("link.idx", linkIdx);

    using FileReference reference = FileReference.CreateFromFullPath(fullPath: childPath);
    bool attached = scope.Active.InstanceDefinitions.ModifySourceArchive(
        idefIndex: linkIdx,
        sourceArchive: reference,
        updateType: InstanceDefinitionUpdateType.Linked,
        quiet: true);
    facts.Add("link.attached", attached);
    Probe.Require(attached, "ModifySourceArchive");

    InstanceDefinition linkedAfterAttach = scope.Active.InstanceDefinitions[linkIdx]
        ?? throw new InvalidOperationException(message: "linked definition missing after attach");
    facts.Add("link.objectCount.afterAttach", linkedAfterAttach.ObjectCount);

    bool loaded = scope.Active.InstanceDefinitions.UpdateLinkedInstanceDefinition(
        idefIndex: linkIdx,
        filename: childPath,
        updateNestedLinks: true,
        quiet: true);
    facts.Add("link.loaded", loaded);
    Probe.Require(loaded, "UpdateLinkedInstanceDefinition");

    InstanceDefinition linked = scope.Active.InstanceDefinitions[linkIdx]
        ?? throw new InvalidOperationException(message: "linked definition missing after load");
    facts.Add("link.objectCount", linked.ObjectCount);
    Probe.Require(linked.ObjectCount >= 1, $"link.objectCount={linked.ObjectCount}");
});
