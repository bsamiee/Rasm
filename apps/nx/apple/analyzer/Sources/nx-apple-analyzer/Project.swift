import Foundation
import System
import XcodeProjectFormat

// --- [MODELS] --------------------------------------------------------------------------

struct Node {
    let names: [String]
    let id: XCSchema.ObjectID?
    let path: FilePath?
    let built: Bool
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func placed(_ path: XCSchema.FilePath, group: FilePath?, root: FilePath) -> FilePath? {
    switch path.base {
        case .group: group.map { folder in joined(folder, path.path) }
        case .project, .absolute: joined(root, path.path)
        case .developer, .buildProducts, .sdk, .sourceRoot: nil
    }
}

func nodes(_ references: [XCSchema.Reference], group: FilePath?, names: [String], root: FilePath) -> [Node] {
    references.flatMap { reference in
        let (name, path, id, built, children): (String?, XCSchema.FilePath, XCSchema.ObjectID?, Bool, [XCSchema.Reference]) =
            switch reference {
                case .fileReference(let file): (nil, file.path, file.objectID, !file.buildFiles.isEmpty, [])
                case .group(let group): (group.name, group.path, group.objectID, false, group.children)
                case .folder(let folder): (nil, folder.path, folder.objectID, !folder.targets.isEmpty, [])
                case .variantGroup(let group): (group.name, group.path, group.objectID, !group.buildFiles.isEmpty, group.children.map(XCSchema.Reference.fileReference))
                case .versionGroup(let group): (group.name, group.path, group.objectID, !group.buildFiles.isEmpty, group.children.map(XCSchema.Reference.fileReference))
            }
        let location: FilePath? = placed(path, group: group, root: root)
        let named: [String] = names + [name ?? FilePath(path.path).lastComponent?.string ?? path.path]
        return [Node(names: named, id: id, path: location, built: built)] + nodes(children, group: location, names: named, root: root)
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

func project(_ container: FilePath, _ decoded: XCSchema.Project, identifiers: [String: Set<String>]) throws(AnalyzerError) -> Container {
    let root: FilePath = container.removingLastComponent()
    let tree: [Node] = nodes(decoded.topLevelReferences, group: root, names: [], root: root)
    let locals: [String] = decoded.packages.compactMap { package in
        if case .local(let local) = package.location { joined(root, local.path).string } else { nil }
    }
    let targets: [XCSchema.CommonTargetProperties] = decoded.targets.map(\.commonProperties)
    let bases: [(owner: String, file: FilePath?)] =
        decoded.configurations.compactMap(\.file).map { file in ("project", located(file, in: tree)) }
        + targets.flatMap { target in target.specializedConfigurations.compactMap(\.file).map { file in ("target \(target.name)", located(file, in: tree)) } }
    let remotes: [FilePath] =
        targets.flatMap { target in target.dependencies.compactMap { dependency in if case .remoteTarget(let remote, _) = dependency { located(remote.project, in: tree) } else { nil } } }
        + decoded.importedProducts.compactMap { product in located(product.project, in: tree) }
    let declared: [(scheme: Scheme, references: [Reference])] = try schemes(in: container, identifiers: identifiers)
    return try checked(
        Container(
            path: container.string,
            schemes: declared.map(\.scheme),
            configurations: decoded.configurations.map(\.name.rawValue),
            inputs: Set((bases.compactMap(\.file) + declared.flatMap(\.references).compactMap(\.path)).map(\.string)).sorted(),
            projects: Set(remotes.map(\.string) + locals).sorted(),
            buildFiles: Set(tree.filter(\.built).compactMap(\.path).map(\.string)).sorted(),
        ),
        bases.map { base in Reference(owner: base.owner, setting: "baseConfigurationReference", path: base.file) } + declared.flatMap(\.references),
    )
}

func decoded(_ container: FilePath) throws(AnalyzerError) -> XCSchema.Project {
    let data: Data = try read(container.appending("project.xcproj"))
    do {
        return try XCSchema.Project(jsonRepresentation: data)
    } catch {
        throw AnalyzerError(subject: container.string, reason: .undecodable(error))
    }
}

func versioned(_ held: XCSchema.Project, to version: String) throws(AnalyzerError) -> XCSchema.Project {
    let build: Int =
        try held.buildSettings[VersionSetting.project.rawValue].map { (setting: XCSchema.BuildSetting) throws(AnalyzerError) -> Int in
            guard let number: Int = setting.text.flatMap(Int.init) else { throw AnalyzerError(subject: VersionSetting.project.rawValue, reason: .nonInteger(String(describing: setting))) }
            return number
        } ?? 0
    var next: XCSchema.Project = held
    next.buildSettings[VersionSetting.marketing.rawValue] = .string(version)
    next.buildSettings[VersionSetting.project.rawValue] = .string(String(build + 1))
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
