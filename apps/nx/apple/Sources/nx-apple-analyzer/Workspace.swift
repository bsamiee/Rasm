import Foundation
import System

// --- [OPERATIONS] ----------------------------------------------------------------------

func attributes(_ path: FilePath, _ queries: [String: String]) throws(AnalyzerError) -> [String: [String]] {
    let data = try read(path)
    do {
        let document = try XMLDocument(data: data)
        return try queries.mapValues { name in try document.nodes(forXPath: name).compactMap { node in node.stringValue } }
    } catch {
        throw AnalyzerError(subject: path.string, reason: .undecodable(error))
    }
}

func schemes(in container: FilePath) throws(AnalyzerError) -> [(scheme: Scheme, references: [Reference])] {
    let shared = container.appending("xcshareddata/xcschemes")
    let entries: [String]
    do {
        entries = exists(shared) ? try FileManager.default.contentsOfDirectory(atPath: shared.string) : []
    } catch {
        throw AnalyzerError(subject: shared.string, reason: .unreadable(error))
    }
    return try entries.filter { entry in FilePath(entry).extension == "xcscheme" }.sorted().map { (entry: String) throws(AnalyzerError) in
        let file = shared.appending(entry)
        let found = try attributes(
            file,
            [
                "ids": "//BuildAction//BuildableReference/@BlueprintIdentifier", "containers": "//BuildAction//BuildableReference/@ReferencedContainer",
                "plans": "//TestAction/TestPlans/TestPlanReference/@reference", "testables": "//TestAction/Testables/TestableReference",
            ],
        )
        let place = { (reference: String) in joined(container.removingLastComponent(), String(reference.trimmingPrefix("container:"))) }
        let plans = (found["plans"] ?? []).map(place)
        let name = file.stem ?? entry
        return (
            Scheme(
                name: name,
                entries: zip(found["ids"] ?? [], found["containers"] ?? []).map { id, owner in Entry(id: id, container: place(owner).string) },
                plans: plans.compactMap(\.stem),
                tests: !plans.isEmpty || !(found["testables"] ?? []).isEmpty,
            ),
            plans.map { plan in Reference(owner: "scheme \(name)", setting: "TestPlanReference", path: plan) },
        )
    }
}

func members(of element: XMLElement, group: FilePath, root: FilePath) -> [FilePath] {
    (element.children ?? []).compactMap { node in node as? XMLElement }.flatMap { (child: XMLElement) -> [FilePath] in
        let location = child.attribute(forName: "location")?.stringValue ?? "group:"
        let path = String(location.drop { character in character != ":" }.dropFirst())
        let place: FilePath? =
            switch location.prefix(while: { character in character != ":" }) {
                case "group": joined(group, path)
                case "container": joined(root, path)
                case "absolute": FilePath(path)
                default: nil
            }
        return switch (child.name, place) {
            case ("Group"?, let folder?): members(of: child, group: folder, root: root)
            case ("FileRef"?, let file?): [file]
            default: []
        }
    }
}

func workspace(_ container: FilePath) throws(AnalyzerError) -> Analysis {
    let root = container.removingLastComponent()
    let file = container.appending("contents.xcworkspacedata")
    let listed: [FilePath]
    do {
        listed = try XMLDocument(data: try read(file)).rootElement().map { element in members(of: element, group: root, root: root) } ?? []
    } catch {
        throw AnalyzerError(subject: file.string, reason: .undecodable(error))
    }
    let declared = try schemes(in: container)
    return Analysis(
        container: Container(path: container.string, schemes: declared.map(\.scheme), inputs: [], members: [], projects: Set(listed.map(\.string)).sorted(), unresolved: []),
        references: declared.flatMap(\.references) + listed.map { member in Reference(owner: "workspace", setting: "FileRef", path: member) },
    )
}
