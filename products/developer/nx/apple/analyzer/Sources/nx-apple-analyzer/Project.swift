import Foundation
import System
import XcodeProjectFormat

// --- [MODELS] --------------------------------------------------------------------------

struct Tree {
    let built: [FilePath]
    private let ids: [XCSchema.ObjectID: FilePath?]
    private let names: [[String]: FilePath?]

    init(_ references: [XCSchema.Reference], root: FilePath) {
        func nodes(_ references: [XCSchema.Reference], group: FilePath?, names: [String]) -> [(names: [String], id: XCSchema.ObjectID?, path: FilePath?, built: Bool)] {
            references.flatMap { reference in
                let (name, path, id, built, children): (String?, XCSchema.FilePath, XCSchema.ObjectID?, Bool, [XCSchema.Reference]) =
                    switch reference {
                        case .fileReference(let file): (nil, file.path, file.objectID, !file.buildFiles.isEmpty, [])
                        case .group(let group): (group.name, group.path, group.objectID, false, group.children)
                        case .folder(let folder): (nil, folder.path, folder.objectID, !folder.targets.isEmpty, [])
                        case .variantGroup(let group): (group.name, group.path, group.objectID, !group.buildFiles.isEmpty, group.children.map(XCSchema.Reference.fileReference))
                        case .versionGroup(let group): (group.name, group.path, group.objectID, !group.buildFiles.isEmpty, group.children.map(XCSchema.Reference.fileReference))
                    }
                let location: FilePath? =
                    switch path.base {
                        case .group: group.map { folder in joined(folder, path.path) }
                        case .project, .absolute: joined(root, path.path)
                        case .developer, .buildProducts, .sdk, .sourceRoot: nil
                    }
                let named = names + [name ?? FilePath(path.path).lastComponent?.string ?? path.path]
                return [(named, id, location, built)] + nodes(children, group: location, names: named)
            }
        }
        let tree = nodes(references, group: root, names: [])
        built = tree.filter(\.built).compactMap(\.path)
        ids = Dictionary(tree.compactMap { node in node.id.map { id in (id, node.path) } }) { first, _ in first }
        names = Dictionary(tree.map { node in (node.names, node.path) }) { first, _ in first }
    }

    func path(of reference: XCSchema.GroupTreeReference) -> FilePath? {
        switch reference {
            case .objectID(let id): ids[id, default: nil]
            case .namePath(let path): names[path.components.map(\.description), default: nil]
        }
    }

    func path(of reference: XCSchema.GroupTreeAnchoredReference) -> FilePath? {
        path(of: reference.anchor).map { anchor in reference.relativePath.map { relative in joined(anchor, relative.description) } ?? anchor }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func project(_ container: FilePath, _ decoded: XCSchema.Project, identifiers: [String: Set<String>]) throws(AnalyzerError) -> Container {
    let root = container.removingLastComponent()
    let tree = Tree(decoded.topLevelReferences, root: root)
    let locals = decoded.packages.compactMap { package in
        if case .local(let local) = package.location { joined(root, local.path).string } else { nil }
    }
    let targets = decoded.targets.map(\.commonProperties)
    let bases: [(owner: String, file: FilePath?)] =
        decoded.configurations.compactMap(\.file).map { file in ("project", tree.path(of: file)) }
        + targets.flatMap { target in target.specializedConfigurations.compactMap(\.file).map { file in ("target \(target.name)", tree.path(of: file)) } }
    let remotes =
        targets.flatMap { target in target.dependencies.compactMap { dependency in if case .remoteTarget(let remote, _) = dependency { tree.path(of: remote.project) } else { nil } } }
        + decoded.importedProducts.compactMap { product in tree.path(of: product.project) }
    let declared = try schemes(in: container, identifiers: identifiers)
    return try checked(
        Container(
            path: container.string,
            schemes: declared.map(\.scheme),
            configurations: decoded.configurations.map(\.name.rawValue),
            inputs: Set((bases.compactMap(\.file) + declared.flatMap(\.references).compactMap(\.path)).map(\.string)).sorted(),
            projects: Set(remotes.map(\.string) + locals).sorted(),
            buildFiles: Set(tree.built.map(\.string)).sorted(),
        ),
        bases.map { base in Reference(owner: base.owner, setting: "baseConfigurationReference", path: base.file) } + declared.flatMap(\.references),
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

func versioned(_ data: Data, to version: String?) throws(AnalyzerError) -> XCSchema.Project {
    let (held, written): (XCSchema.Project, Data)
    do {
        held = try XCSchema.Project(jsonRepresentation: data)
        written = try held.jsonRepresentation()
    } catch {
        throw AnalyzerError(subject: "standard input", reason: .undecodable(error))
    }
    guard version == nil || written == data else { throw AnalyzerError(subject: "standard input", reason: .rewritten) }
    var next = held
    next.buildSettings[marketingSetting] = version.map(XCSchema.BuildSetting.string) ?? held.buildSettings[marketingSetting]
    return next
}

extension XCSchema.BuildSetting {
    var text: String? {
        switch self {
            case .string(let value): value
            case .array: nil
        }
    }
}
