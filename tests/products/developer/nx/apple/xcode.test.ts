import { execFileSync } from 'node:child_process';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { Run } from '@rasm/nx-apple/executors/run';
import { it as nx, outputs, unaggregated, unportable } from '@rasm/testing/nx';
import { Array, flow, Option, Record, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const REFERENCE = '<BuildableReference BuildableIdentifier="primary" BlueprintIdentifier="A10000000000000000000002" BuildableName="Viewer.app" BlueprintName="Viewer" ReferencedContainer="container:Viewer.xcodeproj"/>';
const FILES = {
    'apps/Viewer/Viewer.xcodeproj/project.xcproj': `{
  "default-configuration": "Release",
  "configurations": [
    "Debug",
    "Release",
  ],
  "localizations": {
    "development": "en",
    "supported": [
      "Base",
    ],
  },
  "files": [
    { "kind": "folder", "path": "Sources", "target-membership": [ "Viewer" ] },
    {
      "kind": "group",
      "name": "Products",
      "children": [
        { "path": "<PRODUCTS>/Viewer.app", "id": "A10000000000000000000001", "type": "wrapper.application", "index": false },
      ],
    },
  ],
  "targets": [
    {
      "name": "Viewer",
      "id": "A10000000000000000000002",
      "product": "Products/Viewer.app",
      "product-type": "application",
      "build-phases": [
        "compile-sources",
        "frameworks",
        "resources",
      ],
      "build-settings": {
        "PRODUCT_BUNDLE_IDENTIFIER": "example.fixture.viewer",
      },
    },
  ],
  "build-settings": {
    "CODE_SIGN_IDENTITY": "-",
    "CODE_SIGN_STYLE": "Manual",
    "CURRENT_PROJECT_VERSION": "1",
    "GENERATE_INFOPLIST_FILE": "YES",
    "MACOSX_DEPLOYMENT_TARGET": "27.0",
    "MARKETING_VERSION": "0.1.0",
    "PRODUCT_NAME": "$(TARGET_NAME)",
    "SDKROOT": "macosx",
    "SWIFT_VERSION": "6.0",
  },
  "last-upgrade": "27.2",
  "last-swift-update": "27.2",
}
`,
    'apps/Viewer/Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme': `<?xml version="1.0" encoding="UTF-8"?>\n<Scheme LastUpgradeVersion="2720" version="1.7">\n    <BuildAction parallelizeBuildables="YES" buildImplicitDependencies="YES">\n        <BuildActionEntries>\n            <BuildActionEntry buildForTesting="NO" buildForRunning="YES" buildForProfiling="YES" buildForArchiving="YES" buildForAnalyzing="YES">\n                ${REFERENCE}\n            </BuildActionEntry>\n        </BuildActionEntries>\n    </BuildAction>\n    <LaunchAction buildConfiguration="Debug">\n        <BuildableProductRunnable runnableDebuggingMode="0">\n            ${REFERENCE}\n        </BuildableProductRunnable>\n    </LaunchAction>\n</Scheme>\n`,
    'apps/Viewer/Sources/ViewerApp.swift': 'import SwiftUI\n\n@main\nstruct ViewerApp: App {\n    var body: some Scene {\n        WindowGroup { Text("viewer") }\n    }\n}\n',
    'apps/Viewer/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Viewer", platforms: [.macOS(.v27)], targets: [.executableTarget(name: "Probe", path: "Probe")])\n',
    'apps/Viewer/Probe/main.swift': 'print("probe")\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Exported = Schema.Struct({ exports: Schema.Array(Schema.Struct({ archive: Schema.String, folder: Schema.String, run: Schema.Array(Schema.String) })) });

const Settings = Schema.fromJsonString(Schema.Array(Schema.Struct({ buildSettings: Schema.Struct({ TARGET_BUILD_DIR: Schema.String, WRAPPER_NAME: Schema.String }) })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const ran = flow(Schema.decodeUnknownOption(Run), Option.getOrThrow);

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('analyzed', { scope: 'file' }, async ({ fileWorkspace }, { onCleanup }) => {
    vi.stubEnv('CFFIXED_USER_HOME', fileWorkspace.root);
    onCleanup(() => {
        vi.unstubAllEnvs();
    });
    const graph = await fileWorkspace.write(FILES).graph({ createNodes, createDependencies }, { outputPath: 'dist', trustPackages: true });
    return { root: fileWorkspace.root, graph, viewer: Option.getOrThrow(Record.get(graph.nodes, 'Viewer')) };
});

describe('Xcode projects and their shared schemes', { timeout: 240_000 }, () => {
    it('gives the folder of a Swift package and an Xcode project one project holding the targets of both', ({ analyzed: { graph, viewer } }) => {
        expect([Record.keys(graph.nodes).toSorted(), viewer.data.metadata?.technologies?.toSorted(), Array.intersection(['build--swift-build', 'build--xcodebuild'], Object.keys({ ...viewer.data.targets }))]).toStrictEqual([
            ['Probe', 'Viewer'],
            ['swift', 'xcode'],
            ['build--swift-build', 'build--xcodebuild'],
        ]);
    });

    it('builds the scheme with xcodebuild build in each configuration, and selects the configuration its launch action names', ({ analyzed: { viewer } }) => {
        const build = viewer.data.targets?.['build--xcodebuild'];

        expect([ran(build?.configurations?.['debug']).runs, ran(build?.configurations?.['release']).runs, build?.defaultConfiguration]).toStrictEqual([
            [['xcodebuild', 'build', '-project', 'apps/Viewer/Viewer.xcodeproj', '-skipMacroValidation', '-skipPackagePluginValidation', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer', '-configuration', 'Debug']],
            [['xcodebuild', 'build', '-project', 'apps/Viewer/Viewer.xcodeproj', '-skipMacroValidation', '-skipPackagePluginValidation', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer', '-configuration', 'Release']],
            'debug',
        ]);
    });

    it('archives a scheme with installed products in its archive configuration into outputPath before packing it', ({ analyzed: { viewer } }) => {
        expect(Option.getOrThrow(Schema.decodeUnknownOption(Exported)(viewer.data.targets?.['pack--ditto']?.options)).exports).toStrictEqual([
            {
                archive: 'dist/Viewer/Viewer.xcarchive',
                folder: 'dist/Viewer',
                run: ['xcodebuild', 'archive', '-project', 'apps/Viewer/Viewer.xcodeproj', '-skipMacroValidation', '-skipPackagePluginValidation', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer', '-archivePath', 'dist/Viewer/Viewer.xcarchive', '-configuration', 'Release'],
            },
        ]);
    });

    it('installs the application of its archive configuration under DSTROOT=/, naming its bundle for the relaunch notice', ({ analyzed: { viewer } }) => {
        expect(viewer.data.targets?.['install--xcodebuild']?.options).toStrictEqual({
            runs: [['xcodebuild', 'install', '-project', 'apps/Viewer/Viewer.xcodeproj', '-skipMacroValidation', '-skipPackagePluginValidation', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer', '-configuration', 'Release', 'DSTROOT=/']],
            applications: ['Viewer.app'],
        });
    });

    it('lands the application its install command builds at /Applications/Viewer.app, as xcodebuild resolves that command', ({ analyzed: { root, viewer } }) => {
        const [, ...install] = ran(viewer.data.targets?.['install--xcodebuild']?.options).runs.flat();

        const landed = Option.getOrThrow(Schema.decodeOption(Settings)(execFileSync('xcodebuild', ['-showBuildSettings', '-json', ...install], { cwd: root, encoding: 'utf-8' }))).map(({ buildSettings }) => `${buildSettings.TARGET_BUILD_DIR}/${buildSettings.WRAPPER_NAME}`);

        expect(landed).toStrictEqual(['/Applications/Viewer.app']);
    });

    it('packs the archive, its disk image, and its zip into the outputPath folder of the scheme, and releases them', ({ analyzed: { graph, viewer } }) => {
        expect([outputs(graph, 'Viewer', 'pack--ditto').toSorted(), viewer.data.tags, viewer.data.release?.version, viewer.data.targets?.['nx-release-publish']?.dependsOn, Object.keys({ ...viewer.data.targets }).includes('attach--gh')]).toStrictEqual([
            ['dist/Viewer/*.dmg', 'dist/Viewer/*.zip', 'dist/Viewer/Viewer.xcarchive'],
            ['release', 'os:darwin'],
            { versionActions: '@rasm/nx-apple/release/version-actions', versionActionsOptions: { manifest: 'Viewer.xcodeproj/project.xcproj' } },
            ['pack--ditto', '^nx-release-publish'],
            true,
        ]);
    });

    it('cleans the scheme with xcodebuild and removes its archive', ({ analyzed: { viewer } }) => {
        expect(viewer.data.targets?.['clean--xcodebuild']?.options).toStrictEqual({
            commands: [
                ['xcodebuild', 'clean', '-project', 'apps/Viewer/Viewer.xcodeproj', '-skipMacroValidation', '-skipPackagePluginValidation', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer'],
                ['rm', '-rf', 'dist/Viewer/Viewer.xcarchive'],
            ],
            cwd: '.',
            targets: ['pack--ditto'],
        });
    });

    it('runs every tool target through its verb and states no machine path', ({ analyzed: { graph, root } }) => {
        expect([unaggregated(graph), unportable(graph), JSON.stringify(graph).includes(root)]).toStrictEqual([[], [], false]);
    });
});
