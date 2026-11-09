import Foundation
import System
import XcodeProjectFormat

// --- [CONSTANTS] -----------------------------------------------------------------------

let pathSettings: Set<String> = ["CODE_SIGN_ENTITLEMENTS", "INFOPLIST_FILE"]

// --- [MODELS] --------------------------------------------------------------------------

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

func project(_ container: FilePath, _ decoded: XCSchema.Project, identifiers: [String: Set<String>]) throws(AnalyzerError) -> Container {
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
            members: products.flatMap { product in locals.filter { local in local.written == product.package?.packageName }.map { local in (product.productName, [local.path]) } },
            remotes: target.dependencies.compactMap { dependency in if case .remoteTarget(let remote, _) = dependency { located(remote.project, in: tree) } else { nil } },
            unresolved: settings.filter(\.macro).map { setting in Setting(target: target.name, setting: setting.key, value: setting.value) },
            files: targetFiles.files + settings.filter { setting in !setting.macro }.map { setting in joined(root, setting.value) },
            references: settings.filter { setting in !setting.macro }.map { setting in Reference(owner: "target \(target.name)", setting: setting.key, path: joined(root, setting.value)) }
                + targetLevel.map { file in Reference(owner: "target \(target.name)", setting: "baseConfigurationReference", path: file) }
                + targetFiles.missing.map { file in Reference(owner: "target \(target.name)", setting: "#include", path: file) },
        )
    }
    let declared = try schemes(in: container, identifiers: identifiers)
    return try checked(
        Container(
            path: container.string,
            schemes: declared.map(\.scheme),
            inputs: Set((projectFiles.files + targets.flatMap(\.files) + declared.flatMap(\.references).compactMap(\.path)).map(\.string)).sorted(),
            members: Dictionary(targets.flatMap(\.members), uniquingKeysWith: +).mapValues { packages in Set(packages).sorted() },
            projects: Set((targets.flatMap(\.remotes) + decoded.importedProducts.compactMap { product in located(product.project, in: tree) }).map(\.string) + locals.map(\.path)).sorted(),
            unresolved: targets.flatMap(\.unresolved),
        ),
        projectLevel.map { file in Reference(owner: "project", setting: "baseConfigurationReference", path: file) }
            + projectFiles.missing.map { file in Reference(owner: "project", setting: "#include", path: file) } + targets.flatMap(\.references) + declared.flatMap(\.references),
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
