import { execFileSync } from 'node:child_process';
import { existsSync, globSync } from 'node:fs';
import { join, matchesGlob } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-apple';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const FIXTURE = {
    'Package.swift': `// swift-tools-version: 6.4
import PackageDescription

let package = Package(
    name: "Fixture",
    platforms: [.macOS(.v27)],
    targets: [
        .target(name: "Core", path: "libs/Core"),
        .target(name: "Middle", dependencies: ["Core"], path: "libs/Middle"),
        .executableTarget(name: "Tool", dependencies: ["Middle"], path: "apps/Tool"),
        .testTarget(name: "CoreTests", dependencies: ["Core"], path: "libs/tests/CoreTests"),
    ]
)
`,
    'libs/Core/Core.swift': 'public func one() -> Int { 1 }\n',
    'libs/Middle/Middle.swift': 'import Core\n\npublic func two() -> Int { one() + 1 }\n',
    'apps/Tool/main.swift': 'import Middle\n\nprint(two())\n',
    'libs/tests/CoreTests/CoreTests.swift': 'import Core\nimport Testing\n\n@Test func oneIsOne() { #expect(one() == 1) }\n',
};
const REFERENCE = '<BuildableReference BuildableIdentifier="primary" BlueprintIdentifier="A10000000000000000000002" BuildableName="Viewer.app" BlueprintName="Viewer" ReferencedContainer="container:Viewer.xcodeproj"/>';
const VIEWER = {
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
};
const SMALLEST = { 'Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Tool", targets: [.executableTarget(name: "Tool")])\n', 'Sources/Tool/main.swift': 'print(1)\n' };
const NESTED = {
    'nested/nested/proj/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Proj", products: [.library(name: "Lib", targets: ["Lib"])], targets: [.target(name: "Lib")])\n',
    'nested/nested/proj/Sources/Lib/Lib.swift': 'public let lib = 1\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('apple-plugin');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-apple - createNodes', () => {
    describe('config file glob', () => {
        const [glob] = createNodes;
        const matches = (path: string) => matchesGlob(path, glob);

        it.each([
            'Package.swift',
            'libs/core/Package.swift',
            'Package@swift-6.0.swift',
            'apps/Viewer/Info.plist',
            'apps/Viewer/Settings.plist',
            'apps/Viewer/Viewer.entitlements',
            'apps/Viewer/Viewer.xctestplan',
            'Configs/Base.xcconfig',
            'apps/Viewer/Viewer.xcodeproj/project.xcproj',
            'apps/Viewer/Viewer.xcodeproj/project.pbxproj',
            'apps/Viewer/Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme',
            'Apps.xcworkspace/contents.xcworkspacedata',
        ])('should match %s', (path) => {
            expect(matches(path)).toBe(true);
        });

        it.each(['apps/Tool/main.swift', 'package.json', 'Package.resolved', 'apps/Viewer/Viewer.xcodeproj/project.xcworkspace/xcshareddata/swiftpm/Package.resolved'])('should not match %s', (path) => {
            expect(matches(path)).toBe(false);
        });
    });

    describe('createNodes', { timeout: 120_000 }, () => {
        const [, createNodesFunction] = createNodes;

        it('should create nodes', async ({ tempFs, context }) => {
            await tempFs.createFiles(FIXTURE);

            const results = await createNodesFunction(['Package.swift'], {}, context);

            expect(results).toMatchObject([['Package.swift', { projects: { '.': { name: 'Fixture', tags: ['os:darwin'] }, 'libs/Core': { name: 'Core', tags: ['os:darwin'] }, 'libs/Middle': { name: 'Middle' }, 'apps/Tool': { name: 'Tool' } } }]]);
            expect(Object.keys({ ...results[0]?.[1].projects })).toHaveLength(4);
            expect(Object.keys({ ...results[0]?.[1].projects?.['.']?.targets })).toStrictEqual(['build', 'build--swift-build', 'check', 'check--swift-build', 'test', 'test--swift', 'upgrade', 'upgrade--swift-package', 'clean', 'clean--swift-package']);
            expect(results[0]?.[1].projects?.['.']?.targets).toMatchObject({
                build: { dependsOn: ['build--*'] },
                'build--swift-build': {
                    executor: '@rasm/nx-apple:run',
                    cache: true,
                    options: { runs: [['swift', 'build', '--package-path', '.', '--force-resolved-versions', '--no-color-diagnostics']], outputs: ['{workspaceRoot}/.build/out/Products/Debug/Tool'] },
                    outputs: ['{options.outputs.0}'],
                    configurations: { release: { outputs: ['{workspaceRoot}/.build/out/Products/Release/Tool'] } },
                },
                'test--swift': { cache: true, dependsOn: ['build--swift-build'] },
                'clean--swift-package': { executor: '@rasm/nx-distribution:clean', options: { commands: [['swift', 'package', '--package-path', '.', 'clean']], targets: ['build--swift-build', 'test--swift'] } },
            });
        });

        it('should create a project per module whose targets run the targets of its package', async ({ tempFs, context }) => {
            await tempFs.createFiles(FIXTURE);

            const results = await createNodesFunction(['Package.swift'], {}, context);

            expect(Object.keys({ ...results[0]?.[1].projects?.['libs/Core']?.targets })).toStrictEqual(['build', 'build--swift-build', 'check', 'check--swift-build', 'test', 'test--swift', 'clean', 'clean--swift-package']);
            expect(Object.keys({ ...results[0]?.[1].projects?.['libs/Middle']?.targets })).toStrictEqual(['build', 'build--swift-build', 'check', 'check--swift-build', 'clean', 'clean--swift-package']);
            expect(Object.keys({ ...results[0]?.[1].projects?.['apps/Tool']?.targets })).toStrictEqual(['build', 'build--swift-build', 'check', 'check--swift-build', 'install', 'install--swift-package', 'clean', 'clean--swift-package']);
            expect(results[0]?.[1].projects?.['apps/Tool']?.targets).toMatchObject({
                'build--swift-build': { dependsOn: [{ projects: ['Fixture'], target: 'build--swift-build' }] },
                'install--swift-package': { executor: '@rasm/nx-apple:install', options: { package: '.', product: 'Tool' } },
            });
        });

        it('should create nodes for nested project root', async ({ tempFs, context }) => {
            await tempFs.createFiles(NESTED);

            const results = await createNodesFunction(['nested/nested/proj/Package.swift'], {}, context);

            expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['nested/nested/proj', 'nested/nested/proj/Sources/Lib']);
            expect(results[0]?.[1].projects?.['nested/nested/proj']?.targets?.['build--swift-build']?.options).toMatchObject({
                runs: [['swift', 'build', '--package-path', 'nested/nested/proj', '--force-resolved-versions', '--no-color-diagnostics']],
            });
            expect(results[0]?.[1].projects?.['nested/nested/proj']?.targets?.['build--swift-build']?.inputs).toContain('{workspaceRoot}/nested/nested/proj/Sources/Lib/**/*');
            expect(results[0]?.[1].projects?.['nested/nested/proj']?.targets?.['build--swift-build']?.inputs).toContain('{workspaceRoot}/nested/nested/proj/Package.swift');
        });

        it('should merge the inputs a package.json states into the targets that run them', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                ...NESTED,
                'nested/nested/proj/package.json': JSON.stringify({ nx: { targets: { 'build--swift-build': { inputs: ['{projectRoot}/extra.txt'] } } } }),
            });

            const results = await createNodesFunction(['nested/nested/proj/Package.swift'], {}, context);

            expect(results[0]?.[1].projects?.['nested/nested/proj']?.targets?.['build']?.inputs).toContain('{projectRoot}/extra.txt');
            expect(results[0]?.[1].projects?.['nested/nested/proj']?.targets?.['check']?.inputs).not.toContain('{projectRoot}/extra.txt');
        });

        it('should create nodes based on xcode', async ({ tempFs, context }) => {
            await tempFs.createFiles(VIEWER);

            const results = await createNodesFunction(['apps/Viewer/Viewer.xcodeproj/project.xcproj', 'apps/Viewer/Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme'], { outputPath: 'dist' }, context);

            expect(results).toMatchObject([
                ['apps/Viewer/Viewer.xcodeproj/project.xcproj', { projects: { 'apps/Viewer': { name: 'Viewer', tags: ['release', 'os:darwin'], release: { version: { versionActions: '@rasm/nx-apple/release/version-actions', versionActionsOptions: { manifest: 'Viewer.xcodeproj/project.xcproj' } } } } } }],
            ]);
            expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['apps/Viewer']);
            expect(Object.keys({ ...results[0]?.[1].projects?.['apps/Viewer']?.targets })).toStrictEqual(['build', 'build--xcodebuild', 'check', 'check--xcodebuild', 'pack', 'pack--ditto', 'install', 'install--xcodebuild', 'attach', 'attach--gh', 'clean', 'clean--xcodebuild', 'nx-release-publish']);
            expect(results[0]?.[1].projects?.['apps/Viewer']?.targets).toMatchObject({
                'build--xcodebuild': {
                    cache: true,
                    dependsOn: ['^build'],
                    options: { runs: [['xcodebuild', 'build', '-project', 'apps/Viewer/Viewer.xcodeproj', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer']] },
                    configurations: {
                        debug: { runs: [['xcodebuild', 'build', '-project', 'apps/Viewer/Viewer.xcodeproj', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer', '-configuration', 'Debug']] },
                        release: { runs: [['xcodebuild', 'archive', '-project', 'apps/Viewer/Viewer.xcodeproj', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer', '-archivePath', 'dist/Viewer/Viewer.xcarchive', '-configuration', 'Release']] },
                    },
                    defaultConfiguration: 'debug',
                },
                'pack--ditto': { executor: '@rasm/nx-apple:package', outputs: ['{workspaceRoot}/dist/Viewer/*.dmg', '{workspaceRoot}/dist/Viewer/*.zip'] },
                'install--xcodebuild': {
                    options: {
                        runs: [['xcodebuild', 'install', '-project', 'apps/Viewer/Viewer.xcodeproj', '-onlyUsePackageVersionsFromResolvedFile', '-scheme', 'Viewer', '-configuration', 'Release', 'DSTROOT=/Applications', 'INSTALL_PATH=/']],
                        applications: ['/Applications/Viewer.app'],
                    },
                },
                'nx-release-publish': { executor: '@rasm/nx-apple:release-publish', dependsOn: ['pack--ditto', '^nx-release-publish'] },
            });
        });

        it('should give one project the targets of a Swift package and an Xcode project holding one folder', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                ...VIEWER,
                'apps/Viewer/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Viewer", platforms: [.macOS(.v27)], targets: [.executableTarget(name: "Probe", path: "Probe")])\n',
                'apps/Viewer/Probe/main.swift': 'print("probe")\n',
            });

            const results = await createNodesFunction(['apps/Viewer/Package.swift', 'apps/Viewer/Viewer.xcodeproj/project.xcproj', 'apps/Viewer/Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme'], {}, context);

            expect(results).toHaveLength(1);
            expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['apps/Viewer', 'apps/Viewer/Probe']);
            expect(results[0]?.[1].projects?.['apps/Viewer']?.targets).toHaveProperty('build--swift-build');
            expect(results[0]?.[1].projects?.['apps/Viewer']?.targets).toHaveProperty('build--xcodebuild');
        });

        it('should read a package with a remote dependency and no resolved file without resolving or fetching it', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                ...FIXTURE,
                'Package.swift': FIXTURE['Package.swift']
                    .replace('    targets: [', '    dependencies: [.package(url: "https://github.com/apple/swift-argument-parser", from: "1.5.0")],\n    targets: [')
                    .replace('dependencies: ["Middle"], path: "apps/Tool"', 'dependencies: ["Middle", .product(name: "ArgumentParser", package: "swift-argument-parser")], path: "apps/Tool"'),
            });

            await createNodesFunction(['Package.swift'], {}, context);

            await expect(tempFs.readFile('Package.resolved')).rejects.toThrow('ENOENT');
            await expect(tempFs.readFile('.build/workspace-state.json')).rejects.toThrow('ENOENT');
        });

        it('should write every declared output of build--swift-build and nothing outside the build folder swift build keeps', { timeout: 300_000 }, async ({ tempFs, context }) => {
            await tempFs.createFiles(SMALLEST);
            const results = await createNodesFunction(['Package.swift'], {}, context);
            const before = globSync('**', { cwd: tempFs.tempDir, exclude: ['.build/**'] });

            expect(results[0]?.[1].projects?.['.']?.targets?.['build--swift-build']?.options).toStrictEqual({
                runs: [['swift', 'build', '--package-path', '.', '--force-resolved-versions', '--no-color-diagnostics']],
                outputs: ['{workspaceRoot}/.build/out/Products/Debug/Tool'],
            });
            execFileSync('swift', ['build', '--package-path', '.', '--force-resolved-versions', '--no-color-diagnostics'], { cwd: tempFs.tempDir, stdio: 'ignore' });
            expect(existsSync(join(tempFs.tempDir, '.build/out/Products/Debug/Tool'))).toBe(true);
            expect(globSync('**', { cwd: tempFs.tempDir, exclude: ['.build/**'] })).toStrictEqual(before);
        });
    });
});
