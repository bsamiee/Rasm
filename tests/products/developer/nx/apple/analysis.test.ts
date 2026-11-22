import { execFileSync } from 'node:child_process';
import { existsSync, mkdtempSync, readFileSync, realpathSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { refused } from '@rasm/testing/errors';
import { shimmed, it as workspaced } from '@rasm/testing/nx';
import { Array, Effect, Predicate, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLIST = '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict/></plist>\n';
const XCODE = {
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
  "build-settings": { "MACOSX_DEPLOYMENT_TARGET": "27.0", "PRODUCT_NAME": "$(TARGET_NAME)", "SDKROOT": "macosx", "SWIFT_VERSION": "6.0" },
}
`,
    'apps/Viewer/Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme': `<?xml version="1.0" encoding="UTF-8"?>\n<Scheme LastUpgradeVersion="2720" version="1.7">\n    <BuildAction parallelizeBuildables="YES" buildImplicitDependencies="YES">\n        <BuildActionEntries>\n            <BuildActionEntry buildForTesting="NO" buildForRunning="YES" buildForProfiling="YES" buildForArchiving="YES" buildForAnalyzing="YES">\n                <BuildableReference BuildableIdentifier="primary" BlueprintIdentifier="A10000000000000000000002" BuildableName="Viewer.app" BlueprintName="Viewer" ReferencedContainer="container:Viewer.xcodeproj"/>\n            </BuildActionEntry>\n        </BuildActionEntries>\n    </BuildAction>\n</Scheme>\n`,
    'apps/Viewer/Configs/Base.xcconfig': '#include "Signing.xcconfig"\nCODE_SIGN_ENTITLEMENTS[config=Release] = Support/$(TARGET_NAME)-$(CONFIGURATION).entitlements\n',
    'apps/Viewer/Configs/Signing.xcconfig': 'CODE_SIGN_ENTITLEMENTS = Support/$(TARGET_NAME).entitlements // development\n',
    'apps/Viewer/Support/Info.plist': PLIST,
    'apps/Viewer/Support/Viewer.entitlements': PLIST,
    'apps/Viewer/Support/Viewer-Release.entitlements': PLIST,
    'apps/Viewer/Sources/ViewerApp.swift': 'import SwiftUI\n\n@main\nstruct ViewerApp: App {\n    var body: some Scene {\n        WindowGroup { Text("viewer") }\n    }\n}\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Located = Schema.fromJsonString(Schema.Array(Schema.Struct({ buildSettings: Schema.Struct({ SRCROOT: Schema.String, INFOPLIST_FILE: Schema.String, CODE_SIGN_ENTITLEMENTS: Schema.String }) })));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced
    .extend('swift', ({}, { onCleanup }) => {
        const folder = mkdtempSync(join(tmpdir(), 'apple-swift-'));
        const shim = (stamp: string) => {
            shimmed(folder, { swift: `# ${stamp}\nprintf '%s\\n' "$*" >> ${folder}/calls.log\nPATH="\${PATH#*:}"\nexec swift "$@"` });
        };
        shim('first');
        onCleanup(() => {
            rmSync(folder, { recursive: true });
        });
        return {
            shim,
            described: () =>
                readFileSync(join(folder, 'calls.log'), 'utf-8')
                    .split('\n')
                    .filter((call) => call.endsWith(' describe --type json')),
        };
    })
    .extend('xcodebuild', ({}, { onCleanup }) => {
        const folder = mkdtempSync(join(tmpdir(), 'apple-xcodebuild-'));
        shimmed(folder, { xcodebuild: `printf '%s\\n' "$*" >> ${folder}/calls.log\nPATH="\${PATH#*:}"\nexec xcodebuild "$@"` });
        onCleanup(() => {
            rmSync(folder, { recursive: true });
        });
        return { called: () => (existsSync(join(folder, 'calls.log')) ? readFileSync(join(folder, 'calls.log'), 'utf-8').split('\n').filter(Boolean) : []) };
    });

describe('Xcode projects as their files state them', { timeout: 240_000 }, () => {
    it('reads a project, its configuration files, and its shared schemes without running xcodebuild', async ({ workspace, xcodebuild }) => {
        const graph = await workspace.write(XCODE).graph({ createNodes, createDependencies });

        expect([Object.keys(graph.nodes['Viewer']?.data.targets ?? {}).includes('build--xcodebuild'), xcodebuild.called()]).toStrictEqual([true, []]);
    });

    it('hashes each Info.plist and entitlements file xcodebuild resolves for the target in every configuration', async ({ workspace }) => {
        const graph = await workspace.write(XCODE).graph({ createNodes, createDependencies });
        const root = realpathSync(workspace.root);
        const settings = await Effect.runPromise(
            Effect.forEach(['Debug', 'Release'], (configuration) =>
                Schema.decodeEffect(Located)(execFileSync('xcodebuild', ['-showBuildSettings', '-json', '-project', 'apps/Viewer/Viewer.xcodeproj', '-configuration', configuration], { cwd: workspace.root, encoding: 'utf-8', env: { ...env, CFFIXED_USER_HOME: workspace.root } })),
            ),
        );
        const located = settings
            .flat()
            .flatMap(({ buildSettings: { SRCROOT, INFOPLIST_FILE, CODE_SIGN_ENTITLEMENTS } }) => [resolve(SRCROOT, INFOPLIST_FILE), resolve(SRCROOT, CODE_SIGN_ENTITLEMENTS)])
            .map((at) => join('{workspaceRoot}', relative(root, realpathSync(at))));
        const hashed = graph.nodes['Viewer']?.data.targets?.['build--xcodebuild']?.inputs?.filter(Predicate.isString) ?? [];

        expect([Array.dedupe(located).toSorted(), Array.difference(located, hashed)]).toStrictEqual([['{workspaceRoot}/apps/Viewer/Support/Info.plist', '{workspaceRoot}/apps/Viewer/Support/Viewer-Release.entitlements', '{workspaceRoot}/apps/Viewer/Support/Viewer.entitlements'], []]);
    });
});

describe('cache', { timeout: 120_000 }, () => {
    const PACKAGE = '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Proj", products: [.library(name: "Lib", targets: ["Lib"])], targets: [.target(name: "Lib")])\n';
    const BROKEN = '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: \n';

    it('reuses the graph while no manifest, toolchain, or option changes', async ({ workspace, swift }) => {
        await workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n' }).graph({ createNodes, createDependencies });

        await workspace.graph({ createNodes, createDependencies });

        expect(swift.described()).toHaveLength(1);
    });

    it('describes a package again once its manifest changes', async ({ workspace, swift }) => {
        await workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n' }).graph({ createNodes, createDependencies });

        await workspace.write({ 'proj/Package.swift': PACKAGE.replace('"Proj"', '"Renamed"') }).graph({ createNodes, createDependencies });

        expect(swift.described()).toHaveLength(2);
    });

    it('describes a package again once the swift toolchain changes', async ({ workspace, swift }) => {
        await workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n' }).graph({ createNodes, createDependencies });
        swift.shim('upgraded');

        await workspace.graph({ createNodes, createDependencies });

        expect(swift.described()).toHaveLength(2);
    });

    it('keeps no graph of a run that failed to describe a package', async ({ workspace, swift }) => {
        workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n', 'broken/Package.swift': BROKEN });
        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/exits 1/u));

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/exits 1/u));

        expect(swift.described()).toHaveLength(4);
    });
});
