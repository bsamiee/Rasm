import Foundation
import System
import XcodeProjectFormat

// --- [CONSTANTS] -----------------------------------------------------------------------

let application = XCSchema.ProductTypeID(abbreviatedRepresentation: "application")

// --- [MODELS] --------------------------------------------------------------------------

struct Tree {
    let built: [FilePath]
    private let ids: [XCSchema.ObjectID: (path: FilePath?, file: String)]
    private let names: [[String]: (path: FilePath?, file: String)]

    init(_ references: [XCSchema.Reference], root: FilePath) {
        func nodes(_ references: [XCSchema.Reference], group: FilePath?, names: [String]) -> [(names: [String], id: XCSchema.ObjectID?, path: FilePath?, file: String, built: Bool)] {
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
                let file = FilePath(path.path).lastComponent?.string ?? path.path
                let named = names + [name ?? file]
                return [(named, id, location, file, built)] + nodes(children, group: location, names: named)
            }
        }
        let tree = nodes(references, group: root, names: [])
        built = tree.filter(\.built).compactMap(\.path)
        ids = Dictionary(tree.compactMap { node in node.id.map { id in (id, (node.path, node.file)) } }) { first, _ in first }
        names = Dictionary(tree.map { node in (node.names, (node.path, node.file)) }) { first, _ in first }
    }

    func node(of reference: XCSchema.GroupTreeReference) -> (path: FilePath?, file: String)? {
        switch reference {
            case .objectID(let id): ids[id]
            case .namePath(let path): names[path.components.map(\.description)]
        }
    }

    func path(of reference: XCSchema.GroupTreeAnchoredReference) -> FilePath? {
        node(of: reference.anchor).flatMap(\.path).map { anchor in reference.relativePath.map { relative in joined(anchor, relative.description) } ?? anchor }
    }
}

struct Analysis {
    let container: FilePath
    let configurations: [String]
    let projects: [String]
    let buildFiles: [String]
    let buildSettings: [String: [String: String]]
    let built: Built
    let references: [Reference]
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func analysis(_ container: FilePath, _ decoded: XCSchema.Project) throws(AnalyzerError) -> Analysis {
    let root = container.removingLastComponent()
    let tree = Tree(decoded.topLevelReferences, root: root)
    let locals = decoded.packages.compactMap { package in
        if case .local(let local) = package.location { joined(root, local.path).string } else { nil }
    }
    let targets = decoded.targets.map(\.commonProperties)
    let configurations = decoded.configurations.map(\.name.rawValue)
    let based = { (configurations: [XCSchema.Configuration], owner: String) throws(AnalyzerError) -> [String: [Source]] in
        Dictionary(
            try configurations.map { (configuration: XCSchema.Configuration) throws(AnalyzerError) in
                (
                    configuration.name.rawValue,
                    try configuration.file.map { (file: XCSchema.GroupTreeAnchoredReference) throws(AnalyzerError) in
                        try included(tree.path(of: file), owner: owner, setting: "baseConfigurationReference")
                    } ?? [],
                )
            }
        ) { first, _ in first }
    }
    let project = try based(decoded.configurations, "project")
    let evaluated = try targets.map { (target: XCSchema.CommonTargetProperties) throws(AnalyzerError) in
        let specialized = try based(target.specializedConfigurations, "target \(target.name)")
        let settings = configurations.map { configuration in
            Settings(
                layers: [
                    project[configuration, default: []].flatMap(\.assignments), assignments(decoded.buildSettings), specialized[configuration, default: []].flatMap(\.assignments),
                    assignments(target.buildSettings),
                ],
                container: container,
                target: target.name,
                configuration: configuration,
            )
        }
        let located = settings.flatMap { held in
            Resolved.allCases.filter(\.locates).compactMap { (located: Resolved) -> Reference? in
                switch held.resolved(located.rawValue) {
                    case nil, .some(""): nil
                    case .some(let value): Reference(owner: "target \(target.name)", setting: located.rawValue, path: value.map { at in joined(root, at) })
                }
            }
        }
        let stated = settings.first { held in held.configuration == decoded.defaultConfigurationName.rawValue }.map { held in
            Resolved.allCases.compactMap { (setting: Resolved) -> (String, String)? in
                switch held.resolved(setting.rawValue) {
                    case nil, .some(nil), .some(""): nil
                    case .some(let value?): (setting.rawValue, setting.locates ? joined(root, value).string : value)
                }
            }
        }
        let bundle = target.productTypeID == application ? target.product.flatMap(tree.node(of:)).map(\.file) : nil
        let installed = bundle.map { file in settings.filter { held in held.resolved(skipInstall) != enabled }.map { held in (held.configuration, file) } } ?? []
        return (
            id: target.objectID.rawValue,
            name: target.name,
            installed: Dictionary(installed) { first, _ in first },
            settings: Dictionary(stated ?? []) { first, _ in first },
            references: specialized.values.flatMap(\.self).map(\.reference) + located,
        )
    }
    let remotes =
        targets.flatMap { target in target.dependencies.compactMap { dependency in if case .remoteTarget(let remote, _) = dependency { tree.node(of: remote.project).flatMap(\.path) } else { nil } } }
        + decoded.importedProducts.compactMap { product in tree.node(of: product.project).flatMap(\.path) }
    return Analysis(
        container: container,
        configurations: configurations,
        projects: Set(remotes.map(\.string) + locals).sorted(),
        buildFiles: Set(tree.built.map(\.string)).sorted(),
        buildSettings: Dictionary(evaluated.map { held in (held.name, held.settings) }) { first, _ in first },
        built: Built(configuration: decoded.defaultConfigurationName.rawValue, products: Dictionary(evaluated.map { held in (held.id, held.installed) }) { first, _ in first }),
        references: project.values.flatMap(\.self).map(\.reference) + evaluated.flatMap(\.references),
    )
}

func project(_ analysis: Analysis, built: [String: Built]) throws(AnalyzerError) -> Container {
    let declared = try schemes(in: analysis.container, built: built)
    let references = analysis.references + declared.flatMap(\.references)
    return try checked(
        Container(
            path: analysis.container.string,
            schemes: declared.map(\.scheme),
            configurations: analysis.configurations,
            inputs: Set(references.compactMap(\.path).map(\.string)).sorted(),
            projects: analysis.projects,
            buildFiles: analysis.buildFiles,
            buildSettings: analysis.buildSettings,
        ),
        references,
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
