import Foundation
import System

// --- [OPERATIONS] ----------------------------------------------------------------------

func document(_ path: FilePath) throws(AnalyzerError) -> XMLDocument {
    let data = try read(path)
    do {
        return try XMLDocument(data: data)
    } catch {
        throw AnalyzerError(subject: path.string, reason: .undecodable(error))
    }
}

func values(_ xpath: String, in document: XMLDocument, at path: FilePath) throws(AnalyzerError) -> [String] {
    do {
        return try document.nodes(forXPath: xpath).compactMap(\.stringValue)
    } catch {
        throw AnalyzerError(subject: path.string, reason: .undecodable(error))
    }
}

func schemes(in container: FilePath, identifiers: [String: Set<String>]) throws(AnalyzerError) -> [(scheme: Scheme, references: [Reference])] {
    let shared = container.appending("xcshareddata/xcschemes")
    let entries: [String]
    do {
        entries = exists(shared) ? try FileManager.default.contentsOfDirectory(atPath: shared.string) : []
    } catch {
        throw AnalyzerError(subject: shared.string, reason: .unreadable(error))
    }
    return try entries.filter { entry in FilePath(entry).extension == "xcscheme" }.sorted().map { (entry: String) throws(AnalyzerError) in
        let file = shared.appending(entry)
        let parsed = try document(file)
        let place: (String) -> FilePath = { reference in joined(container.removingLastComponent(), String(reference.trimmingPrefix("\(WorkspaceLocation.container.rawValue):"))) }
        let ids = try values("//BuildAction//BuildableReference/@BlueprintIdentifier", in: parsed, at: file)
        let owners = try values("//BuildAction//BuildableReference/@ReferencedContainer", in: parsed, at: file).map { owner in place(owner).string }
        let plans = try values("//TestAction/TestPlans/TestPlanReference/@reference", in: parsed, at: file).map(place)
        let testables = try values("//TestAction/Testables/TestableReference", in: parsed, at: file)
        let launch = try values("/Scheme/LaunchAction/@buildConfiguration", in: parsed, at: file).first
        let name = file.stem ?? entry
        return (
            Scheme(name: name, containers: owners, plans: plans.compactMap(\.stem), tests: !plans.isEmpty || !testables.isEmpty, launch: launch),
            plans.map { plan in Reference(owner: "scheme \(name)", setting: "TestPlanReference", path: plan) }
                + zip(ids, owners).filter { id, owner in identifiers[owner].map { known in !known.contains(id) } ?? false }
                .map { id, owner in Reference(owner: "scheme \(name)", setting: "BlueprintIdentifier \(id) in \(owner)", path: nil) },
        )
    }
}

func members(of element: XMLElement, group: FilePath, root: FilePath) -> [FilePath] {
    (element.children ?? []).compactMap { node in node as? XMLElement }.flatMap { (child: XMLElement) -> [FilePath] in
        let (location, path) =
            child.attribute(forName: "location")?.stringValue.map { text in text.split(separator: ":", maxSplits: 1, omittingEmptySubsequences: false) }
            .map { parts in (WorkspaceLocation(rawValue: String(parts[0])), parts.dropFirst().joined()) } ?? (.group, "")
        let place: FilePath? =
            switch location {
                case .group: joined(group, path)
                case .container: joined(root, path)
                case .absolute: FilePath(path)
                case nil: nil
            }
        return switch (child.name.flatMap(WorkspaceElement.init(rawValue:)), place) {
            case (.group, let folder?): members(of: child, group: folder, root: root)
            case (.fileReference, let file?): [file]
            default: []
        }
    }
}

func workspace(_ container: FilePath, identifiers: [String: Set<String>]) throws(AnalyzerError) -> Container {
    let root = container.removingLastComponent()
    let listed = try document(container.appending("contents.xcworkspacedata")).rootElement().map { element in members(of: element, group: root, root: root) } ?? []
    let declared = try schemes(in: container, identifiers: identifiers)
    return try checked(
        Container(path: container.string, schemes: declared.map(\.scheme), configurations: [], inputs: [], projects: Set(listed.map(\.string)).sorted(), buildFiles: []),
        declared.flatMap(\.references) + listed.map { member in Reference(owner: "workspace", setting: WorkspaceElement.fileReference.rawValue, path: member) },
    )
}
