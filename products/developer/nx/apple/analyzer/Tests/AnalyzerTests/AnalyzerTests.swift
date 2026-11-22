import Foundation
import System
import Testing
import XcodeProjectFormat

@testable import nx_apple_analyzer

// --- [CONSTANTS] -----------------------------------------------------------------------

private let reference =
    #"<BuildableReference BuildableIdentifier="primary" BlueprintIdentifier="A10000000000000000000002" "#
    + #"BuildableName="Viewer.app" BlueprintName="Viewer" ReferencedContainer="container:Viewer.xcodeproj"/>"#
private let plans = #"<TestAction buildConfiguration="Debug"><TestPlans><TestPlanReference reference="container:Viewer.xctestplan" default="YES"/></TestPlans></TestAction>"#
private let configs = #"{ "kind": "group", "name": "Configs", "path": "Configs", "children": [ { "path": "Base.xcconfig" } ] },"#

// --- [MODELS] --------------------------------------------------------------------------

private final class Workspace {
    let root: FilePath = .init(URL.temporaryDirectory.appending(path: "nx-apple-analyzer-\(UUID().uuidString)").path(percentEncoded: false))

    init(_ files: [String: String]) throws {
        for (path, content) in files {
            let file = root.appending(path)
            try FileManager.default.createDirectory(atPath: file.removingLastComponent().string, withIntermediateDirectories: true)
            try Data(content.utf8).write(to: URL(filePath: file.string))
        }
    }

    deinit { try? FileManager.default.removeItem(atPath: root.string) }

    subscript(_ relative: String) -> String { root.appending(relative).string }

    static func project(configurations: String = #""Debug", "Release""#, packages: String = "[]", files: [String] = [], settings: String = "") -> String {
        """
        {
          "default-configuration": "Release",
          "configurations": [ \(configurations) ],
          "localizations": { "development": "en", "supported": [ "Base" ] },
          "packages": \(packages),
          "files": [
            { "kind": "folder", "path": "Sources", "target-membership": [ "Viewer" ] },
            \(files.joined())
            { "kind": "group", "name": "Products", "children": [ { "path": "<PRODUCTS>/Viewer.app", "id": "A10000000000000000000001", "type": "wrapper.application", "index": false } ] },
          ],
          "targets": [
            {
              "name": "Viewer",
              "id": "A10000000000000000000002",
              "product": "Products/Viewer.app",
              "product-type": "application",
              "build-phases": [ "compile-sources", "frameworks", "resources" ],
              "build-settings": { "PRODUCT_BUNDLE_IDENTIFIER": "example.fixture.viewer", \(settings) },
            },
          ],
          "build-settings": { "MARKETING_VERSION": "0.1.0", "PRODUCT_NAME": "$(TARGET_NAME)", "SDKROOT": "macosx" },
        }
        """
    }

    static func scheme(_ testActions: [String] = []) -> String {
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <Scheme LastUpgradeVersion="2720" version="1.7">
            <BuildAction parallelizeBuildables="YES" buildImplicitDependencies="YES">
                <BuildActionEntries>
                    <BuildActionEntry buildForTesting="YES" buildForRunning="YES" buildForProfiling="YES" buildForArchiving="YES" buildForAnalyzing="YES">\(reference)</BuildActionEntry>
                </BuildActionEntries>
            </BuildAction>
            \(testActions.joined())
            <LaunchAction buildConfiguration="Debug"><BuildableProductRunnable runnableDebuggingMode="0">\(reference)</BuildableProductRunnable></LaunchAction>
        </Scheme>
        """
    }

    func viewer(schemes: [Scheme] = [], inputs: [String] = [], projects: [String] = []) -> Container {
        Container(path: self["Viewer.xcodeproj"], schemes: schemes, configurations: ["Debug", "Release"], inputs: inputs, projects: projects, buildFiles: [self["Sources"]])
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct AnalyzerSmokeTests {
    @Test
    func defaultProject_declaresSchemesConfigurationsAndBuiltFolders() throws {
        let workspace = try Workspace(["Viewer.xcodeproj/project.xcproj": Workspace.project(), "Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme": Workspace.scheme()])
        let scheme = Scheme(
            name: "Viewer",
            containers: [workspace["Viewer.xcodeproj"]],
            plans: [],
            tests: false,
            launch: "Debug",
            archive: Archive(configuration: "Release", bundles: ["Viewer.app"]),
        )

        #expect(report([workspace.root.appending("Viewer.xcodeproj")]) == Report(containers: [workspace.viewer(schemes: [scheme])], failures: [:]))
    }

    @Test
    func baseConfigurations_areInputs() throws {
        let configurations = #"{ "name": "Debug", "file": "Configs/Base.xcconfig" }, { "name": "Release", "file": "Configs/Base.xcconfig" }"#
        let workspace = try Workspace(["Viewer.xcodeproj/project.xcproj": Workspace.project(configurations: configurations, files: [configs]), "Configs/Base.xcconfig": "SWIFT_VERSION = 6.0\n"])

        #expect(report([workspace.root.appending("Viewer.xcodeproj")]) == Report(containers: [workspace.viewer(inputs: [workspace["Configs/Base.xcconfig"]])], failures: [:]))
    }

    @Test
    func localPackages_areProjects() throws {
        let workspace = try Workspace(["Viewer.xcodeproj/project.xcproj": Workspace.project(packages: #"[ { "kind": "local", "path": "Core" } ]"#), "Core/Package.swift": ""])

        #expect(report([workspace.root.appending("Viewer.xcodeproj")]) == Report(containers: [workspace.viewer(projects: [workspace["Core"]])], failures: [:]))
    }

    @Test
    func testPlans_makeTheSchemeTest() throws {
        let workspace = try Workspace([
            "Viewer.xcodeproj/project.xcproj": Workspace.project(), "Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme": Workspace.scheme([plans]), "Viewer.xctestplan": "{}",
        ])
        let scheme = Scheme(
            name: "Viewer",
            containers: [workspace["Viewer.xcodeproj"]],
            plans: ["Viewer"],
            tests: true,
            launch: "Debug",
            archive: Archive(configuration: "Release", bundles: ["Viewer.app"]),
        )

        #expect(report([workspace.root.appending("Viewer.xcodeproj")]) == Report(containers: [workspace.viewer(schemes: [scheme], inputs: [workspace["Viewer.xctestplan"]])], failures: [:]))
    }

    @Test
    func locatedSettings_areInputsFromProjectAndIncludedConfigurations() throws {
        let configurations = #"{ "name": "Debug", "file": "Configs/Base.xcconfig" }, { "name": "Release", "file": "Configs/Base.xcconfig" }"#
        let workspace = try Workspace([
            "Viewer.xcodeproj/project.xcproj": Workspace.project(configurations: configurations, files: [configs], settings: #""INFOPLIST_FILE": "$(SRCROOT)/Info.plist""#),
            "Configs/Base.xcconfig": "#include \"Shared.xcconfig\"\n#include? \"Local.xcconfig\"\nCODE_SIGN_ENTITLEMENTS = $(TARGET_NAME).entitlements // signed\n",
            "Configs/Shared.xcconfig": "SWIFT_VERSION = 6.0\n", "Info.plist": "", "Viewer.entitlements": "",
        ])
        let inputs = ["Configs/Base.xcconfig", "Configs/Shared.xcconfig", "Info.plist", "Viewer.entitlements"].map { file in workspace[file] }

        #expect(report([workspace.root.appending("Viewer.xcodeproj")]) == Report(containers: [workspace.viewer(inputs: inputs)], failures: [:]))
    }

    @Test
    func conditionalSettingsOfConfigurationFiles_applyInTheirConfiguration() throws {
        let configurations = #"{ "name": "Debug", "file": "Configs/Base.xcconfig" }, { "name": "Release", "file": "Configs/Base.xcconfig" }"#
        let workspace = try Workspace([
            "Viewer.xcodeproj/project.xcproj": Workspace.project(configurations: configurations, files: [configs]),
            "Configs/Base.xcconfig": "CODE_SIGN_ENTITLEMENTS = Debug.entitlements\nCODE_SIGN_ENTITLEMENTS[config=Release] = Release.entitlements\n",
            "Debug.entitlements": "", "Release.entitlements": "",
        ])
        let inputs = ["Configs/Base.xcconfig", "Debug.entitlements", "Release.entitlements"].map { file in workspace[file] }

        #expect(report([workspace.root.appending("Viewer.xcodeproj")]) == Report(containers: [workspace.viewer(inputs: inputs)], failures: [:]))
    }

    @Test
    func unresolvedLocatedSettings_areFailuresOfTheirContainer() throws {
        let workspace = try Workspace(["Viewer.xcodeproj/project.xcproj": Workspace.project(settings: #""INFOPLIST_FILE": "$(PLATFORM_DIR)/Info.plist""#)])
        let failure = "\(workspace["Viewer.xcodeproj"]) has missing references:\n  target Viewer INFOPLIST_FILE resolves to nothing"

        #expect(report([workspace.root.appending("Viewer.xcodeproj")]) == Report(containers: [], failures: [workspace["Viewer.xcodeproj"]: failure]))
    }

    @Test
    func archiveConfiguration_decidesWhetherTheSchemeInstalls() throws {
        let settings = #""SKIP_INSTALL[config=Release]": "YES""#
        let archiving = #"<ArchiveAction buildConfiguration="Debug" revealArchiveInOrganizer="YES"/>"#
        let skipped = try Workspace(["Viewer.xcodeproj/project.xcproj": Workspace.project(settings: settings), "Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme": Workspace.scheme()])
        let debug = try Workspace(["Viewer.xcodeproj/project.xcproj": Workspace.project(settings: settings), "Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme": Workspace.scheme([archiving])])

        #expect(report([skipped.root.appending("Viewer.xcodeproj")]).containers.flatMap(\.schemes).map(\.archive) == [nil])
        #expect(report([debug.root.appending("Viewer.xcodeproj")]).containers.flatMap(\.schemes).map(\.archive) == [Archive(configuration: "Debug", bundles: ["Viewer.app"])])
    }

    @Test
    func workspaces_listTheirProjects() throws {
        let workspace = try Workspace([
            "Viewer.xcodeproj/project.xcproj": Workspace.project(),
            "Apps.xcworkspace/contents.xcworkspacedata": #"<?xml version="1.0" encoding="UTF-8"?><Workspace version="1.0"><FileRef location="group:Viewer.xcodeproj"/></Workspace>"#,
        ])
        let listed = Container(path: workspace["Apps.xcworkspace"], schemes: [], configurations: [], inputs: [], projects: [workspace["Viewer.xcodeproj"]], buildFiles: [])

        #expect(report([workspace.root.appending("Viewer.xcodeproj"), workspace.root.appending("Apps.xcworkspace")]) == Report(containers: [workspace.viewer(), listed], failures: [:]))
    }

    @Test
    func missingReferences_areFailuresOfTheirContainer() throws {
        let workspace = try Workspace(["Viewer.xcodeproj/project.xcproj": Workspace.project(configurations: #"{ "name": "Debug", "file": "Configs/Base.xcconfig" }, "Release""#, files: [configs])])
        let failure = "\(workspace["Viewer.xcodeproj"]) has missing references:\n  project baseConfigurationReference: \(workspace["Configs/Base.xcconfig"])"

        #expect(report([workspace.root.appending("Viewer.xcodeproj")]) == Report(containers: [], failures: [workspace["Viewer.xcodeproj"]: failure]))
    }

    @Test
    func undecodableProjects_areFailuresOfTheirContainer() throws {
        let workspace = try Workspace(["Viewer.xcodeproj/project.xcproj": "not json at all"])
        let failure = try #require(report([workspace.root.appending("Viewer.xcodeproj")]).failures[workspace["Viewer.xcodeproj"]])

        #expect(failure.hasPrefix("\(workspace["Viewer.xcodeproj"]) is undecodable: "))
    }

    @Test
    func version_writesMarketingVersionAndKeepsTheRest() throws {
        let written = try XCSchema.Project(jsonRepresentation: Data(Workspace.project().utf8)).jsonRepresentation()
        let next = try versioned(written, to: "2.0.0")

        #expect(try versioned(written, to: nil).buildSettings[marketingSetting]?.text == "0.1.0")
        #expect(next.buildSettings[marketingSetting]?.text == "2.0.0")
        #expect(try next.jsonRepresentation() == Data(try #require(String(bytes: written, encoding: .utf8)).replacing(#""MARKETING_VERSION": "0.1.0""#, with: #""MARKETING_VERSION": "2.0.0""#).utf8))
    }

    @Test
    func version_refusesAProjectXcodeWouldRewrite() {
        #expect(throws: AnalyzerError.self) { try versioned(Data(Workspace.project().utf8), to: "2.0.0") }
    }

    @Test
    func version_readsAProjectXcodeWouldRewrite() throws {
        #expect(try versioned(Data(Workspace.project().utf8), to: nil).buildSettings[marketingSetting]?.text == "0.1.0")
    }
}
