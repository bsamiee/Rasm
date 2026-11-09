import Foundation
import System
import XcodeProjectFormat

// --- [CONSTANTS] -----------------------------------------------------------------------

let pathSettings: Set<String> = ["CODE_SIGN_ENTITLEMENTS", "INFOPLIST_FILE"]

// --- [MODELS] --------------------------------------------------------------------------

struct Entry: Encodable {
    let id: String
    let container: String
}

struct Scheme: Encodable {
    let name: String
    let entries: [Entry]
    let plans: [String]
    let tests: Bool
}

struct Member: Encodable, Hashable {
    let product: String
    let packages: [String]
}

struct Setting: Encodable {
    let target: String
    let setting: String
    let value: String
}

struct Container: Encodable {
    let path: String
    let schemes: [Scheme]
    let inputs: [String]
    let members: [Member]
    let projects: [String]
    let unresolved: [Setting]
}

struct Failure: Encodable {
    let path: String
    let reason: String
}

struct Report: Encodable {
    let containers: [Container]
    let failures: [Failure]
}

struct Reference {
    let owner: String
    let setting: String
    let path: FilePath?

    var missing: String? { path.map { file in exists(file) ? nil : "\(owner) \(setting): \(file.string)" } ?? "\(owner) \(setting)" }
}

struct Node {
    let names: [String]
    let id: XCSchema.ObjectID?
    let path: FilePath?
}

struct Configuration {
    let files: [FilePath]
    let values: [String: [String]]
    let missing: [FilePath]

    static let empty = Self(files: [], values: [:], missing: [])

    func merging(_ other: Self) -> Self {
        Self(files: files + other.files, values: values.merging(other.values, uniquingKeysWith: +), missing: missing + other.missing)
    }
}

struct Analysis {
    let container: Container
    let references: [Reference]
}

// --- [ERRORS] --------------------------------------------------------------------------

struct AnalyzerError: LocalizedError {
    enum Reason: Sendable {
        case unreadable(any Error)
        case undecodable(any Error)
        case missing([String])
    }

    let subject: String
    let reason: Reason

    var errorDescription: String? {
        switch reason {
            case .unreadable(let cause): "\(subject) is unreadable: \(cause.localizedDescription)"
            case .undecodable(let cause): "\(subject) is undecodable: \(cause.localizedDescription)"
            case .missing(let references): (["\(subject) references what does not exist:"] + references).joined(separator: "\n  ")
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func exists(_ path: FilePath) -> Bool { FileManager.default.fileExists(atPath: path.string) }

func read(_ path: FilePath) throws(AnalyzerError) -> Data {
    do {
        return try Data(contentsOf: URL(filePath: path.string))
    } catch {
        throw AnalyzerError(subject: path.string, reason: .unreadable(error))
    }
}

func attributes(_ path: FilePath, _ queries: [String: String]) throws(AnalyzerError) -> [String: [String]] {
    let data = try read(path)
    do {
        let document = try XMLDocument(data: data)
        return try queries.mapValues { name in try document.nodes(forXPath: name).compactMap { node in node.stringValue } }
    } catch {
        throw AnalyzerError(subject: path.string, reason: .undecodable(error))
    }
}

func joined(_ base: FilePath, _ relative: String) -> FilePath {
    relative.hasPrefix("/") || relative.hasPrefix("~") ? FilePath(NSString(string: relative).expandingTildeInPath) : base.appending(relative).lexicallyNormalized()
}

func placed(_ path: XCSchema.FilePath, group: FilePath?, root: FilePath) -> FilePath? {
    switch path.base {
        case .group: group.map { folder in joined(folder, path.path) }
        case .project, .absolute: joined(root, path.path)
        case .developer, .buildProducts, .sdk, .sourceRoot: nil
    }
}

func nodes(_ references: [XCSchema.Reference], group: FilePath?, names: [String], root: FilePath) -> [Node] {
    references.flatMap { reference in
        let (name, path, id, children): (String?, XCSchema.FilePath, XCSchema.ObjectID?, [XCSchema.Reference]) =
            switch reference {
                case .fileReference(let file): (nil, file.path, file.objectID, [])
                case .group(let group): (group.name, group.path, group.objectID, group.children)
                case .folder(let folder): (nil, folder.path, folder.objectID, [])
                case .variantGroup(let group): (group.name, group.path, group.objectID, group.children.map(XCSchema.Reference.fileReference))
                case .versionGroup(let group): (group.name, group.path, group.objectID, group.children.map(XCSchema.Reference.fileReference))
            }
        let location = placed(path, group: group, root: root)
        let named = names + [name ?? FilePath(path.path).lastComponent?.string ?? path.path]
        return [Node(names: named, id: id, path: location)] + nodes(children, group: location, names: named, root: root)
    }
}

func located(_ reference: XCSchema.GroupTreeReference, in tree: [Node]) -> FilePath? {
    switch reference {
        case .objectID(let id): tree.first { node in node.id == id }?.path
        case .namePath(let names): tree.first { node in node.names == names.components.map(\.description) }?.path
    }
}

func located(_ reference: XCSchema.GroupTreeAnchoredReference, in tree: [Node]) -> FilePath? {
    located(reference.anchor, in: tree).map { anchor in reference.relativePath.map { relative in joined(anchor, relative.description) } ?? anchor }
}

func values(_ settings: [String: XCSchema.BuildSetting]) -> [String: [String]] {
    Dictionary(
        settings.compactMap { key, setting in
            let name = String(key.prefix { character in character != "[" })
            return switch (pathSettings.contains(name), setting) {
                case (true, .string(let value)): (name, [value])
                case (true, .array(let items)): (name, items)
                case (false, _): nil
            }
        },
        uniquingKeysWith: +,
    )
}

func configuration(_ file: FilePath, optional: Bool) throws(AnalyzerError) -> Configuration {
    guard exists(file) else { return optional ? .empty : Configuration(files: [], values: [:], missing: [file]) }
    let lines = String(decoding: try read(file), as: UTF8.self).split(whereSeparator: \.isNewline)
    let includes = lines.compactMap { line in line.wholeMatch(of: /\s*#include(\?)?\s*"([^"<][^"]*)"\s*/).map { found in (joined(file.removingLastComponent(), String(found.2)), found.1 != nil) } }
    let assigned = lines.compactMap { line in line.wholeMatch(of: /\s*([A-Za-z0-9_]+)(?:\[[^\]]*\])*\s*=\s*(.*?)\s*/).map { found in (String(found.1), [String(found.2)]) } }
    let own = Configuration(files: [file], values: Dictionary(assigned.filter { key, _ in pathSettings.contains(key) }, uniquingKeysWith: +), missing: [])
    return try includes.map { (path, optional) throws(AnalyzerError) in try configuration(path, optional: optional) }.reduce(own) { held, next in held.merging(next) }
}

func configurations(_ files: [FilePath?]) throws(AnalyzerError) -> Configuration {
    try files.compactMap(\.self).filter(exists).map { (file: FilePath) throws(AnalyzerError) in try configuration(file, optional: false) }.reduce(.empty) { held, next in held.merging(next) }
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

func project(_ container: FilePath, _ decoded: XCSchema.Project, identifiers: [String: Set<String>]) throws(AnalyzerError) -> Analysis {
    let root = container.removingLastComponent()
    let tree = nodes(decoded.topLevelReferences, group: root, names: [], root: root)
    let locals = decoded.packages.compactMap { package in
        if case .local(let local) = package.location { (written: FilePath(local.path).lastComponent?.string, path: joined(root, local.path).string) } else { nil }
    }
    let projectLevel = decoded.configurations.compactMap(\.file).map { file in located(file, in: tree) }
    let projectFiles = try configurations(projectLevel)
    let targets = try decoded.targets.map(\.commonProperties).map { (target: XCSchema.CommonTargetProperties) throws(AnalyzerError) in
        let targetLevel = target.specializedConfigurations.compactMap(\.file).map { file in located(file, in: tree) }
        let targetFiles = try configurations(targetLevel)
        let levels = [values(target.buildSettings), targetFiles.values, values(decoded.buildSettings), projectFiles.values]
        let settings = pathSettings.sorted().flatMap { key in
            (levels.first { level in level[key] != nil }?[key] ?? []).map { value in (key: key, value: value, macro: value.contains("$(") || value.contains("${")) }
        }
        let products =
            target.packageProductTargetMembers.map(\.packageProduct) + target.dependencies.compactMap { dependency in if case .package(let product, _) = dependency { product } else { nil } }
        return (
            members: products.compactMap { product in
                let packages = locals.filter { local in local.written == product.package?.packageName }.map(\.path).sorted()
                return packages.isEmpty ? nil : Member(product: product.productName, packages: packages)
            },
            remotes: target.dependencies.compactMap { dependency in if case .remoteTarget(let remote, _) = dependency { located(remote.project, in: tree) } else { nil } },
            unresolved: settings.filter(\.macro).map { setting in Setting(target: target.name, setting: setting.key, value: setting.value) },
            files: targetFiles.files + settings.filter { setting in !setting.macro }.map { setting in joined(root, setting.value) },
            references: settings.filter { setting in !setting.macro }.map { setting in Reference(owner: "target \(target.name)", setting: setting.key, path: joined(root, setting.value)) }
                + targetLevel.map { file in Reference(owner: "target \(target.name)", setting: file == nil ? "baseConfigurationReference names no file of the group tree" : "baseConfigurationReference", path: file) }
                + targetFiles.missing.map { file in Reference(owner: "target \(target.name)", setting: "#include", path: file) },
        )
    }
    let declared = try schemes(in: container)
    let stale = declared.flatMap { scheme, _ in
        scheme.entries.filter { entry in identifiers[entry.container].map { ids in !ids.contains(entry.id) } ?? false }
            .map { entry in Reference(owner: "scheme \(scheme.name)", setting: "BlueprintIdentifier \(entry.id) names no target of \(entry.container)", path: nil) }
    }
    return Analysis(
        container: Container(
            path: container.string,
            schemes: declared.map(\.scheme),
            inputs: Set((projectFiles.files + targets.flatMap(\.files) + declared.flatMap(\.references).compactMap(\.path)).map(\.string)).sorted(),
            members: Set(targets.flatMap(\.members)).sorted { left, right in (left.product, left.packages.joined()) < (right.product, right.packages.joined()) },
            projects: Set((targets.flatMap(\.remotes) + decoded.importedProducts.compactMap { product in located(product.project, in: tree) }).map(\.string)).sorted(),
            unresolved: targets.flatMap(\.unresolved),
        ),
        references: projectLevel.map { file in Reference(owner: "project", setting: file == nil ? "baseConfigurationReference names no file of the group tree" : "baseConfigurationReference", path: file) }
            + projectFiles.missing.map { file in Reference(owner: "project", setting: "#include", path: file) } + targets.flatMap(\.references) + declared.flatMap(\.references) + stale,
    )
}

func decoded(_ container: FilePath) throws(AnalyzerError) -> XCSchema.Project {
    let data = try read(container.appending("project.xcproj"))
    do {
        return try XCSchema.Project(jsonRepresentation: data)
    } catch {
        throw AnalyzerError(subject: container.string, reason: .undecodable(error))
    }
}

func checked(_ analysis: Analysis) throws(AnalyzerError) -> Container {
    let missing = Set(analysis.references.compactMap(\.missing)).sorted()
    guard missing.isEmpty else { throw AnalyzerError(subject: analysis.container.path, reason: .missing(missing)) }
    return analysis.container
}

func report(_ containers: [FilePath]) -> Report {
    let projects = containers.filter { container in container.extension == "xcodeproj" }.map { container in (container, Result { () throws(AnalyzerError) in try decoded(container) }) }
    let identifiers = Dictionary(
        uniqueKeysWithValues: projects.compactMap { container, result in (try? result.get()).map { project in (container.string, Set(project.targets.map(\.commonProperties.objectID.rawValue))) } }
    )
    let outcomes =
        projects.map { container, result in (container, Result { () throws(AnalyzerError) in try checked(project(container, result.get(), identifiers: identifiers)) }) }
        + containers.filter { container in container.extension == "xcworkspace" }.map { container in (container, Result { () throws(AnalyzerError) in try checked(workspace(container)) }) }
    return Report(
        containers: outcomes.compactMap { _, outcome in try? outcome.get() },
        failures: outcomes.compactMap { container, outcome in if case .failure(let error) = outcome { Failure(path: container.string, reason: error.localizedDescription) } else { nil } },
    )
}

// --- [COMPOSITION] ---------------------------------------------------------------------

do {
    let containers = try JSONDecoder().decode([String].self, from: FileHandle.standardInput.readToEnd() ?? Data()).map { file in FilePath(file).removingLastComponent() }
    FileHandle.standardOutput.write(try JSONEncoder().encode(report(containers)))
} catch {
    FileHandle.standardError.write(Data(error.localizedDescription.utf8))
    exit(1)
}
