import { execFileSync } from 'node:child_process';
import { existsSync, readdirSync, realpathSync } from 'node:fs';
import { join, matchesGlob, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { Install } from '@rasm/nx-apple/executors/install';
import { execute, Run } from '@rasm/nx-apple/executors/run';
import { it as nx, outputs, targetOptions, unaggregated, unportable } from '@rasm/testing/nx';
import { Array, Effect, Option, Predicate, Record, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => {
    const { cwd } = await import('node:process');
    return Object.defineProperty({ ...(await actual()) }, 'workspaceDataDirectory', { enumerable: true, get: () => `${cwd()}/tmp/project-graph-cache` });
});

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOLS = '// swift-tools-version: 6.4\nimport PackageDescription\n\n';
const SHELL = 'x; touch pwned';
const PLIST = '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict/></plist>\n';
const VERSIONED = '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict><key>CFBundleShortVersionString</key><string>1.0.0</string></dict></plist>\n';
const REFERENCE = '<BuildableReference BuildableIdentifier="primary" BlueprintIdentifier="A10000000000000000000002" BuildableName="Viewer.app" BlueprintName="Viewer" ReferencedContainer="container:Viewer.xcodeproj"/>';
const PACKAGES = {
    'Package.swift': `${TOOLS}let package = Package(\n    name: "Fixture",\n    platforms: [.macOS(.v27)],\n    targets: [\n        .target(name: "Core", path: "libs/Core"),\n        .target(name: "Middle", dependencies: ["Core"], path: "libs/Middle"),\n        .executableTarget(name: "Tool", dependencies: ["Middle"], path: "apps/Tool"),\n        .testTarget(name: "CoreTests", dependencies: ["Core"], path: "libs/tests/CoreTests"),\n    ]\n)\n`,
    'libs/Core/Core.swift': 'public func one() -> Int { 1 }\n',
    'libs/Middle/Middle.swift': 'import Core\n\npublic func two() -> Int { one() + 1 }\n',
    'apps/Tool/main.swift': 'import Middle\n\nprint(two())\n',
    'libs/tests/CoreTests/CoreTests.swift': 'import Core\nimport Testing\n\n@Test func oneIsOne() { #expect(one() == 1) }\n',
    'packages/kit/Package.swift': `${TOOLS}let package = Package(name: "KitPackage", products: [.executable(name: "Kit", targets: ["Kit"])], targets: [.executableTarget(name: "Kit")])\n`,
    'packages/kit/Sources/Kit/main.swift': 'print("kit")\n',
    'packages/consumer/Package.swift': `${TOOLS}let package = Package(name: "Consumer", dependencies: [.package(path: "../kit")], targets: [.executableTarget(name: "Consumer")])\n`,
    'packages/consumer/Sources/Consumer/main.swift': 'print("consumer")\n',
    'packages/remote/Package.swift': `${TOOLS}let package = Package(name: "Remote", dependencies: [.package(url: "https://github.com/apple/swift-argument-parser", from: "1.5.0")], targets: [.executableTarget(name: "Remote", dependencies: [.product(name: "ArgumentParser", package: "swift-argument-parser")])])\n`,
    'packages/remote/Sources/Remote/main.swift': 'print("remote")\n',
    'packages/signer/Package.swift': `${TOOLS}let package = Package(name: "Signer", products: [.executable(name: "signer-cli", targets: ["Signer"])], targets: [.executableTarget(name: "Signer", exclude: ["Info.plist", "Signer.entitlements"])])\n`,
    'packages/signer/Sources/Signer/main.swift': 'print("signer")\n',
    'packages/signer/Sources/Signer/Info.plist': VERSIONED,
    'packages/signer/Sources/Signer/Signer.entitlements': PLIST,
    [`${SHELL}/Package.swift`]: `${TOOLS}let package = Package(name: "Shell", products: [.library(name: "Quoted", targets: ["Quoted"])], targets: [.target(name: "Quoted")])\n`,
    [`${SHELL}/Sources/Quoted/Quoted.swift`]: 'public let quoted = 1\n',
};
const XCODE = {
    'apps/Viewer/Package.swift': `${TOOLS}let package = Package(name: "Viewer", platforms: [.macOS(.v27)], targets: [.executableTarget(name: "Probe", path: "Probe")])\n`,
    'apps/Viewer/Probe/main.swift': 'print("probe")\n',
    'apps/Viewer/Viewer.xcodeproj/project.xcproj': `{
  "default-configuration": "Release",
  "configurations": [ { "name": "Debug", "file": "Configs/Base.xcconfig" }, { "name": "Release", "file": "Configs/Base.xcconfig" } ],
  "localizations": { "development": "en", "supported": [ "Base" ] },
  "files": [
    { "kind": "folder", "path": "Sources", "target-membership": [ "Viewer" ] },
    { "kind": "group", "name": "Configs", "path": "Configs", "children": [ { "path": "Base.xcconfig" } ] },
    { "kind": "group", "name": "Products", "children": [ { "path": "<PRODUCTS>/Viewer.app", "id": "A10000000000000000000001", "type": "wrapper.application", "index": false } ] },
  ],
  "targets": [
    {
      "name": "Viewer",
      "id": "A10000000000000000000002",
      "product": "Products/Viewer.app",
      "product-type": "application",
      "build-phases": [ "compile-sources", "frameworks", "resources" ],
      "build-settings": { "PRODUCT_BUNDLE_IDENTIFIER": "example.fixture.viewer", "INFOPLIST_FILE": "$(SRCROOT)/Support/Info.plist" },
    },
  ],
  "build-settings": { "CODE_SIGN_IDENTITY": "-", "CODE_SIGN_STYLE": "Manual", "GENERATE_INFOPLIST_FILE": "YES", "MACOSX_DEPLOYMENT_TARGET": "27.0", "MARKETING_VERSION": "0.1.0", "PRODUCT_NAME": "$(TARGET_NAME)", "SDKROOT": "macosx", "SWIFT_VERSION": "6.0" },
}
`,
    'apps/Viewer/Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme': `<?xml version="1.0" encoding="UTF-8"?>\n<Scheme LastUpgradeVersion="2720" version="1.7">\n    <BuildAction parallelizeBuildables="YES" buildImplicitDependencies="YES">\n        <BuildActionEntries>\n            <BuildActionEntry buildForTesting="NO" buildForRunning="YES" buildForProfiling="YES" buildForArchiving="YES" buildForAnalyzing="YES">\n                ${REFERENCE}\n            </BuildActionEntry>\n        </BuildActionEntries>\n    </BuildAction>\n    <LaunchAction buildConfiguration="Debug">\n        <BuildableProductRunnable runnableDebuggingMode="0">\n            ${REFERENCE}\n        </BuildableProductRunnable>\n    </LaunchAction>\n</Scheme>\n`,
    'apps/Viewer/Configs/Base.xcconfig': '#include "Signing.xcconfig"\nCODE_SIGN_ENTITLEMENTS[config=Release] = Support/$(TARGET_NAME)-$(CONFIGURATION).entitlements\n',
    'apps/Viewer/Configs/Signing.xcconfig': 'CODE_SIGN_ENTITLEMENTS = Support/$(TARGET_NAME).entitlements // development\n',
    'apps/Viewer/Support/Info.plist': PLIST,
    'apps/Viewer/Support/Viewer.entitlements': PLIST,
    'apps/Viewer/Support/Viewer-Release.entitlements': PLIST,
    'apps/Viewer/Sources/ViewerApp.swift': 'import SwiftUI\n\n@main\nstruct ViewerApp: App {\n    var body: some Scene {\n        WindowGroup { Text("viewer") }\n    }\n}\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Described = Schema.fromJsonString(Schema.Struct({ targets: Schema.Array(Schema.Struct({ path: Schema.String, sources: Schema.Array(Schema.String) })) }));
const Settings = Schema.fromJsonString(Schema.Array(Schema.Struct({ buildSettings: Schema.Struct({ SRCROOT: Schema.String, INFOPLIST_FILE: Schema.String, CODE_SIGN_ENTITLEMENTS: Schema.String, TARGET_BUILD_DIR: Schema.String, WRAPPER_NAME: Schema.String }) })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const settings = (root: string, argv: readonly string[]) => Effect.runSync(Schema.decodeEffect(Settings)(execFileSync('xcodebuild', ['-showBuildSettings', '-json', ...argv], { cwd: root, encoding: 'utf-8', env: { ...env, CFFIXED_USER_HOME: root } })));

const compiledBy =
    (folder: string) =>
    ({ targets }: typeof Described.Type) =>
        targets.flatMap(({ path, sources }) => sources.map((source) => join(folder, path, source)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('fixture', { scope: 'file' }, async ({ fileWorkspace }) => {
    const graph = await fileWorkspace.write({ ...PACKAGES, ...XCODE }).graph({ createNodes, createDependencies }, { outputPath: 'dist' });
    const fetched = ['packages/remote/Package.resolved', 'packages/remote/.build/workspace-state.json', 'packages/remote/.build/checkouts'].filter((at) => existsSync(join(fileWorkspace.root, at)));
    const described = await Effect.runPromise(Effect.forEach(['packages/consumer', 'packages/kit'], (folder) => Effect.map(Schema.decodeEffect(Described)(execFileSync('swift', ['package', '--package-path', folder, 'describe', '--type', 'json'], { cwd: fileWorkspace.root, encoding: 'utf-8' })), compiledBy(folder))));
    return { root: fileWorkspace.root, graph, workspace: fileWorkspace, fetched, compiled: described.flat() };
});

describe('SwiftPM packages as swift package describe reports them', { timeout: 240_000 }, () => {
    it('reads a package with a remote dependency and no resolved file without resolving or fetching it', ({ fixture: { graph, fetched } }) => {
        expect({ inferred: 'Remote' in graph.nodes, fetched }).toStrictEqual({ inferred: true, fetched: [] });
    });

    it('infers one project per package and per module a product holds, at the module folder', ({ fixture: { graph } }) => {
        expect(Record.collect(graph.nodes, (name, { data: { root } }) => `${name}@${root}`).toSorted()).toStrictEqual([
            'Consumer@packages/consumer',
            'Core@libs/Core',
            'Fixture@.',
            'Kit@packages/kit/Sources/Kit',
            'KitPackage@packages/kit',
            'Middle@libs/Middle',
            'Probe@apps/Viewer/Probe',
            'Quoted@x; touch pwned/Sources/Quoted',
            'Remote@packages/remote',
            'Shell@x; touch pwned',
            'Signer@packages/signer',
            'Tool@apps/Tool',
            'Viewer@apps/Viewer',
        ]);
    });

    it('passes each package path to swift as a literal argument', ({ fixture: { root } }) => {
        expect(existsSync(join(root, 'pwned'))).toBe(false);
    });

    it('records a local package dependency as static on its manifest and each module dependency SwiftPM reports as implicit', ({ fixture: { graph } }) => {
        expect({ Consumer: graph.dependencies['Consumer'], Middle: graph.dependencies['Middle'], Tool: graph.dependencies['Tool'] }).toStrictEqual({
            Consumer: [{ source: 'Consumer', target: 'KitPackage', type: 'static' }],
            Middle: [{ source: 'Middle', target: 'Core', type: 'implicit' }],
            Tool: [{ source: 'Tool', target: 'Middle', type: 'implicit' }],
        });
    });

    it('hashes every source file SwiftPM compiles for a package and each local package it depends on, and no source of another package', ({ fixture: { graph, compiled } }) => {
        const inputs = (graph.nodes['Consumer']?.data.targets?.['build--swift-build']?.inputs ?? []).filter(Predicate.isString).map((input) => input.replace(/^\{workspaceRoot\}\//u, ''));
        const covered = (file: string) => inputs.some((input) => matchesGlob(file, input));

        expect({ compiled: compiled.length, uncovered: compiled.filter((file) => !covered(file)), foreign: ['apps/Tool/main.swift', 'packages/signer/Sources/Signer/main.swift'].filter(covered) }).toStrictEqual({ compiled: 2, uncovered: [], foreign: [] });
    });

    it('writes each declared output of build--swift-build where swift build puts the product, and nothing outside the package build folder', async ({ fixture: { graph, workspace } }) => {
        const before = workspace.files('tmp', 'packages/signer/.build');

        const { success } = await execute(await targetOptions(Run, graph, 'Signer', 'build--swift-build'), workspace.context(graph, 'Signer', 'build--swift-build'));

        const declared = outputs(graph, 'Signer', 'build--swift-build');
        expect({ success, declared: declared.length, missing: declared.filter((output) => !existsSync(join(workspace.root, output))), files: workspace.files('tmp', 'packages/signer/.build') }).toStrictEqual({ success: true, declared: 1, missing: [], files: before });
    });

    it('signs and versions an executable product named apart from its module by the entitlements and Info.plist that module holds', async ({ fixture: { graph } }) => {
        const signer = graph.nodes['Signer']?.data;
        const install = await targetOptions(Install, graph, 'Signer', 'install--swift-package');

        expect({ install, packHashesPlist: signer?.targets?.['pack--ditto']?.inputs?.includes('{workspaceRoot}/packages/signer/Sources/Signer/Info.plist'), version: signer?.release?.version?.versionActionsOptions }).toStrictEqual({
            install: { package: 'packages/signer', product: 'signer-cli', entitlements: 'packages/signer/Sources/Signer/Signer.entitlements' },
            packHashesPlist: true,
            version: { manifest: 'Sources/Signer/Info.plist' },
        });
    });
});

describe('Xcode projects and their shared schemes', { timeout: 240_000 }, () => {
    it('gives the folder of a Swift package and an Xcode project one project holding the targets of both', ({ fixture: { graph } }) => {
        const viewer = Option.getOrThrow(Record.get(graph.nodes, 'Viewer'));

        expect({ technologies: viewer.data.metadata?.technologies?.toSorted(), targets: Array.intersection(['build--swift-build', 'build--xcodebuild'], Record.keys(viewer.data.targets ?? {})) }).toStrictEqual({ technologies: ['swift', 'xcode'], targets: ['build--swift-build', 'build--xcodebuild'] });
    });

    it('hashes each Info.plist and entitlements file xcodebuild resolves for the target in every configuration', ({ fixture: { root, graph } }) => {
        const located = ['Debug', 'Release']
            .flatMap((configuration) => settings(root, ['-project', 'apps/Viewer/Viewer.xcodeproj', '-configuration', configuration]))
            .flatMap(({ buildSettings: { SRCROOT, INFOPLIST_FILE, CODE_SIGN_ENTITLEMENTS } }) => [resolve(SRCROOT, INFOPLIST_FILE), resolve(SRCROOT, CODE_SIGN_ENTITLEMENTS)])
            .map((at) => join('{workspaceRoot}', relative(root, realpathSync(at))));
        const hashed = graph.nodes['Viewer']?.data.targets?.['build--xcodebuild']?.inputs?.filter(Predicate.isString) ?? [];

        expect({ resolved: Array.dedupe(located).length, unhashed: Array.difference(located, hashed) }).toStrictEqual({ resolved: 3, unhashed: [] });
    });

    it('lands the application its install command builds at /Applications/Viewer.app, as xcodebuild resolves that command', async ({ fixture: { root, graph } }) => {
        const { runs } = await targetOptions(Run, graph, 'Viewer', 'install--xcodebuild');
        const [tool, ...install] = runs.flat();

        expect({ invocations: runs.length, tool, landed: settings(root, install).map(({ buildSettings }) => `${buildSettings.TARGET_BUILD_DIR}/${buildSettings.WRAPPER_NAME}`) }).toStrictEqual({ invocations: 1, tool: 'xcodebuild', landed: ['/Applications/Viewer.app'] });
    });

    it('installs the application through the run executor into the applications folder xcodebuild names', async ({ fixture: { root, graph, workspace }, onTestFinished }) => {
        const applications = join(root, 'installed');
        vi.stubEnv('CFFIXED_USER_HOME', root);
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });

        const inferred = await targetOptions(Run, graph, 'Viewer', 'install--xcodebuild');

        const { success } = await execute({ ...inferred, __unparsed__: [`LOCAL_APPS_DIR=${applications}`] }, workspace.context(graph, 'Viewer', 'install--xcodebuild'));

        expect({ success, installed: readdirSync(applications) }).toStrictEqual({ success: true, installed: ['Viewer.app'] });
    });
});

describe('the graph as a whole', { timeout: 240_000 }, () => {
    it('runs every tool target through its verb and states no machine path', ({ fixture: { graph, root } }) => {
        expect({ unaggregated: unaggregated(graph), unportable: unportable(graph), machine: JSON.stringify(graph).includes(root) }).toStrictEqual({ unaggregated: [], unportable: [], machine: false });
    });
});
