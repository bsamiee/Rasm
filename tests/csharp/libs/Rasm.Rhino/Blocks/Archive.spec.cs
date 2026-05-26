using System.Collections.Immutable;
using Rasm.Rhino.Blocks;
using Rhino.Geometry;

namespace Rasm.Rhino.Tests.Blocks;

// --- [ALGEBRAIC] ------------------------------------------------------------------------
public sealed class BlockArchivePlanLaws {
    [Fact]
    public void BakePlanOrdersNestedDefinitionsBeforeContainersAndSurvivesCycles() {
        Definition child = DefinitionOf(name: "child", id: Guid.Parse(input: "41035b57-1928-432d-831d-04a56c442048"), members: []);
        Definition parent = DefinitionOf(name: "parent", id: Guid.Parse(input: "a69c611a-fef8-4b16-a571-bc7bd8f841c5"), members: [Guid.Parse(input: "8983d56c-7e2f-41bf-b365-4c2863f4c82c")]);
        Definition cyclic = DefinitionOf(name: "cycle", id: Guid.Parse(input: "f1d35ee7-c399-4c97-8069-7a216c9f75f1"), members: [Guid.Parse(input: "0808496b-6d4e-4f82-b4bd-2879ed3cd9ec")]);
        Archive.Graph graph = new(
            Definitions: [parent, child, cyclic],
            Instances: [
                new Archive.Instance(ObjectId: parent.MemberIds[0], ParentDefId: child.Id, Xform: Transform.Identity),
                new Archive.Instance(ObjectId: cyclic.MemberIds[0], ParentDefId: cyclic.Id, Xform: Transform.Identity),
            ],
            LinkedArchives: []);

        Definition[] plan = [.. Archive.BakePlan(graph: graph)];

        Assert.True(condition: System.Array.FindIndex(array: plan, match: def => def.Id.Equals(child.Id)) < System.Array.FindIndex(array: plan, match: def => def.Id.Equals(parent.Id)));
        Assert.Contains(collection: plan, filter: def => def.Id.Equals(cyclic.Id));
    }

    private static Definition DefinitionOf(string name, Guid id, ImmutableArray<Guid> members) =>
        new(
            Id: DefinitionId.From(value: id).IfFail(error => throw new InvalidOperationException(message: error.Message)),
            Index: Option<DefinitionIndex>.None,
            Name: DefinitionName.Create(value: name),
            Description: Option<string>.None,
            Url: Option<ArchivePath>.None,
            UrlDescription: Option<string>.None,
            Source: Option<ArchivePath>.None,
            MemberIds: members,
            Live: Option<LiveStats>.None);
}
