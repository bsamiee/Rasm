import { existsSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { it as nx, outputs, unaggregated, unportable } from '@rasm/testing/nx';
import { Array, Predicate, Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOLS = '// swift-tools-version: 6.4\nimport PackageDescription\n\n';
const SHELL = 'x; touch pwned';
const FILES = {
    'Package.swift': `${TOOLS}let package = Package(\n    name: "Fixture",\n    platforms: [.macOS(.v27)],\n    targets: [\n        .target(name: "Core", path: "libs/Core"),\n        .target(name: "Middle", dependencies: ["Core"], path: "libs/Middle"),\n        .executableTarget(name: "Tool", dependencies: ["Middle"], path: "apps/Tool"),\n        .testTarget(name: "CoreTests", dependencies: ["Core"], path: "libs/tests/CoreTests"),\n    ]\n)\n`,
    'libs/Core/Core.swift': 'public func one() -> Int { 1 }\n',
    'libs/Middle/Middle.swift': 'import Core\n\npublic func two() -> Int { one() + 1 }\n',
    'apps/Tool/main.swift': 'import Middle\n\nprint(two())\n',
    'libs/tests/CoreTests/CoreTests.swift': 'import Core\nimport Testing\n\n@Test func oneIsOne() { #expect(one() == 1) }\n',
    'packages/kit/Package.swift': `${TOOLS}let package = Package(name: "KitPackage", products: [.executable(name: "Kit", targets: ["Kit"])], targets: [.executableTarget(name: "Kit", exclude: ["Info.plist", "Kit.entitlements"], resources: [.copy("Assets")])])\n`,
    'packages/kit/Sources/Kit/main.swift': 'print("kit")\n',
    'packages/kit/Sources/Kit/Info.plist': '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict><key>CFBundleShortVersionString</key><string>1.0.0</string></dict></plist>\n',
    'packages/kit/Sources/Kit/Kit.entitlements': '<?xml version="1.0" encoding="UTF-8"?>\n<plist version="1.0"><dict/></plist>\n',
    'packages/kit/Sources/Kit/Assets/logo.txt': 'kit\n',
    'packages/consumer/Package.swift': `${TOOLS}let package = Package(name: "Consumer", dependencies: [.package(path: "../kit")], targets: [.executableTarget(name: "Consumer")])\n`,
    'packages/consumer/Sources/Consumer/main.swift': 'print("consumer")\n',
    'packages/remote/Package.swift': `${TOOLS}let package = Package(name: "Remote", dependencies: [.package(url: "https://github.com/apple/swift-argument-parser", from: "1.5.0")], targets: [.executableTarget(name: "Remote", dependencies: [.product(name: "ArgumentParser", package: "swift-argument-parser")])])\n`,
    'packages/remote/Sources/Remote/main.swift': 'print("remote")\n',
    'nested/nested/proj/Package.swift': `${TOOLS}let package = Package(name: "Proj", products: [.library(name: "Lib", targets: ["Lib"])], targets: [.target(name: "Lib")])\n`,
    'nested/nested/proj/Sources/Lib/Lib.swift': 'public let lib = 1\n',
    [`${SHELL}/Package.swift`]: `${TOOLS}let package = Package(name: "Shell", products: [.library(name: "Quoted", targets: ["Quoted"])], targets: [.target(name: "Quoted")])\n`,
    [`${SHELL}/Sources/Quoted/Quoted.swift`]: 'public let quoted = 1\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const targets = (graph: ProjectGraph, project: string) => Record.keys(graph.nodes[project]?.data.targets ?? {}).filter((target) => target.includes('--'));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('described', { scope: 'file' }, async ({ fileWorkspace }) => ({ root: fileWorkspace.root, graph: await fileWorkspace.write(FILES).graph({ createNodes, createDependencies }, { coveragePath: 'coverage/{projectName}' }) }));

describe('SwiftPM packages as swift package describe reports them', { timeout: 120_000 }, () => {
    it('infers a project per package and per module a product holds, named by the package and the module', ({ described: { graph } }) => {
        expect(Record.collect(graph.nodes, (name, { data: { root } }) => `${name}@${root}`).toSorted()).toStrictEqual([
            'Consumer@packages/consumer',
            'Core@libs/Core',
            'Fixture@.',
            'Kit@packages/kit/Sources/Kit',
            'KitPackage@packages/kit',
            'Lib@nested/nested/proj/Sources/Lib',
            'Middle@libs/Middle',
            'Proj@nested/nested/proj',
            'Quoted@x; touch pwned/Sources/Quoted',
            'Remote@packages/remote',
            'Shell@x; touch pwned',
            'Tool@apps/Tool',
        ]);
    });

    it('passes each package path to swift as a literal argument', ({ described: { root } }) => {
        expect(existsSync(join(root, 'pwned'))).toBe(false);
    });

    it('reads a package with a remote dependency and no resolved file without resolving or fetching it', ({ described: { root } }) => {
        expect([existsSync(join(root, 'packages/remote/Package.resolved')), existsSync(join(root, 'packages/remote/.build/workspace-state.json')), existsSync(join(root, 'packages/remote/.build/checkouts'))]).toStrictEqual([false, false, false]);
    });

    it('runs the package targets of a module through its package, testing only modules a test target depends on', ({ described: { graph } }) => {
        expect([targets(graph, 'Core'), targets(graph, 'Middle'), graph.nodes['Middle']?.data.targets?.['build--swift-build']?.dependsOn]).toStrictEqual([
            ['build--swift-build', 'check--swift-build', 'test--swift', 'clean--swift-package'],
            ['build--swift-build', 'check--swift-build', 'clean--swift-package'],
            [{ projects: ['Fixture'], target: 'build--swift-build' }],
        ]);
    });

    it('resolves the build of a package of executables alone to its products in the bin folder of each configuration, and of a package with resources to none', ({ described: { graph } }) => {
        expect([outputs(graph, 'Fixture', 'build--swift-build'), outputs(graph, 'Fixture', 'build--swift-build', 'release'), outputs(graph, 'KitPackage', 'build--swift-build')]).toStrictEqual([['.build/out/Products/Debug/Tool'], ['.build/out/Products/Release/Tool'], []]);
    });

    it('caches the build of every package, its replay restoring the products it resolves to', ({ described: { graph } }) => {
        expect([graph.nodes['Fixture']?.data.targets?.['build--swift-build']?.cache, graph.nodes['KitPackage']?.data.targets?.['build--swift-build']?.cache]).toStrictEqual([true, true]);
    });

    it('writes the codecov report and the lcov report under coveragePath in the coverage configuration of a package with tests', ({ described: { graph } }) => {
        expect([outputs(graph, 'Fixture', 'test--swift'), outputs(graph, 'Fixture', 'test--swift', 'coverage')]).toStrictEqual([[], ['.build/out/Products/Debug/codecov/Fixture.json', 'coverage/Fixture/lcov.info']]);
    });

    it('installs an executable product, signed with the entitlements beside its sources', ({ described: { graph } }) => {
        expect([graph.nodes['Tool']?.data.targets?.['install--swift-package']?.options, graph.nodes['Kit']?.data.targets?.['install--swift-package']?.options]).toStrictEqual([
            { package: '.', product: 'Tool' },
            { package: 'packages/kit', product: 'Kit', entitlements: 'packages/kit/Sources/Kit/Kit.entitlements' },
        ]);
    });

    it('releases an executable product whose sources hold an Info.plist, versioned by that file and zipped from the release bin folder', ({ described: { graph } }) => {
        expect([Array.intersection(targets(graph, 'Kit'), ['pack--ditto', 'attach--gh']), graph.nodes['Kit']?.data.tags, graph.nodes['Kit']?.data.release, outputs(graph, 'Kit', 'pack--ditto'), graph.nodes['Tool']?.data.tags]).toStrictEqual([
            ['pack--ditto', 'attach--gh'],
            ['release', 'os:darwin'],
            { version: { versionActions: '@rasm/nx-apple/release/version-actions', versionActionsOptions: { manifest: 'Info.plist' } } },
            ['packages/kit/.build/out/Products/Release/Kit.zip'],
            ['os:darwin'],
        ]);
    });

    it('hashes the sources, manifest, and lock of the package and the sources of each local package it depends on', ({ described: { graph } }) => {
        expect(graph.nodes['Consumer']?.data.targets?.['build--swift-build']?.inputs?.filter(Predicate.isString).toSorted()).toStrictEqual([
            '{workspaceRoot}/.swift-version',
            '{workspaceRoot}/packages/consumer/.swiftpm/configuration/*.json',
            '{workspaceRoot}/packages/consumer/Package.resolved',
            '{workspaceRoot}/packages/consumer/Package.swift',
            '{workspaceRoot}/packages/consumer/Package@swift-*.swift',
            '{workspaceRoot}/packages/consumer/Sources/Consumer/**/*',
            '{workspaceRoot}/packages/kit/.swiftpm/configuration/*.json',
            '{workspaceRoot}/packages/kit/Package.swift',
            '{workspaceRoot}/packages/kit/Package@swift-*.swift',
            '{workspaceRoot}/packages/kit/Sources/Kit/**/*',
        ]);
    });

    it('records a local package dependency as static on the manifest and a module dependency as implicit', ({ described: { graph } }) => {
        expect([graph.dependencies['Consumer'], graph.dependencies['Middle'], graph.dependencies['Tool']]).toStrictEqual([[{ source: 'Consumer', target: 'KitPackage', type: 'static' }], [{ source: 'Middle', target: 'Core', type: 'implicit' }], [{ source: 'Tool', target: 'Middle', type: 'implicit' }]]);
    });

    it('runs every tool target through its verb and states no machine path', ({ described: { graph, root } }) => {
        expect([unaggregated(graph), unportable(graph), JSON.stringify(graph).includes(root)]).toStrictEqual([[], [], false]);
    });
});
