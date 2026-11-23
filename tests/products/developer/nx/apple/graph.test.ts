import { existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { refused } from '@rasm/testing/errors';
import { it as nx, pathed, shimmed } from '@rasm/testing/nx';
import { Effect, Option, Record, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => {
    const { cwd } = await import('node:process');
    return Object.defineProperty({ ...(await actual()) }, 'workspaceDataDirectory', { enumerable: true, get: () => `${cwd()}/tmp/project-graph-cache` });
});
vi.mock(import('node:url'), async (actual) => {
    const real = await actual();
    const { dirname: parent, join: joined, relative } = await import('node:path');
    const plugin = real.fileURLToPath(import.meta.resolve('@rasm/nx-apple/package.json'));
    const installed = parent(createRequire(plugin).resolve('@rasm/nx-apple-darwin-arm64/package.json'));
    const linked = joined(parent(plugin), 'node_modules/@rasm/nx-apple-darwin-arm64');
    return {
        ...real,
        fileURLToPath: (url, options) => {
            const at = real.fileURLToPath(url, options);
            const within = [installed, linked].find((folder) => at.startsWith(`${folder}/`));
            return placed.folder === undefined || within === undefined ? at : joined(placed.folder, relative(within, at));
        },
    };
});

// --- [CONSTANTS] -----------------------------------------------------------------------

const placed = vi.hoisted((): { folder?: string } => ({}));
const TOOLS = '// swift-tools-version: 6.4\nimport PackageDescription\n\n';
const BINARY = '.build/out/Products/Release/nx-apple-analyzer';
const PACKAGE = `${TOOLS}let package = Package(name: "Proj", products: [.library(name: "Lib", targets: ["Lib"])], targets: [.target(name: "Lib")])\n`;
const PROJECT = (name: string) =>
    `{ "default-configuration": "Release", "configurations": [ "Debug", "Release" ], "localizations": { "development": "en", "supported": [ "Base" ] }, "files": [ { "kind": "group", "name": "Products", "children": [ { "path": "<PRODUCTS>/${name}.app", "id": "A10000000000000000000001", "type": "wrapper.application", "index": false } ] } ], "targets": [ { "name": "${name}", "id": "A10000000000000000000002", "product": "Products/${name}.app", "product-type": "application", "build-phases": [ "compile-sources" ] } ], "build-settings": { "PRODUCT_NAME": "$(TARGET_NAME)", "SDKROOT": "macosx" } }\n`;
const SCHEME = (name: string) =>
    `<?xml version="1.0" encoding="UTF-8"?>\n<Scheme LastUpgradeVersion="2720" version="1.7"><BuildAction><BuildActionEntries><BuildActionEntry buildForRunning="YES" buildForArchiving="YES"><BuildableReference BuildableIdentifier="primary" BlueprintIdentifier="A10000000000000000000002" BuildableName="${name}.app" BlueprintName="${name}" ReferencedContainer="container:${name}.xcodeproj"/></BuildActionEntry></BuildActionEntries></BuildAction></Scheme>\n`;
const APPS = {
    'apps/Viewer/Package.swift': `${TOOLS}let package = Package(name: "Viewer", platforms: [.macOS(.v27)], targets: [.executableTarget(name: "Probe", path: "Probe")])\n`,
    'apps/Viewer/Probe/main.swift': 'print("probe")\n',
    'apps/Viewer/Viewer.xcodeproj/project.xcproj': PROJECT('Viewer'),
    'apps/Viewer/Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme': SCHEME('Viewer'),
    'apps/Other/Other.xcodeproj/project.xcproj': PROJECT('Other'),
    'apps/Other/Other.xcodeproj/xcshareddata/xcschemes/Other.xcscheme': SCHEME('Other'),
};
const CONTAINERS = String.raw`apps/Other/Other\.xcodeproj/project\.xcproj, apps/Viewer/Viewer\.xcodeproj/project\.xcproj`;

// --- [MODELS] --------------------------------------------------------------------------

const Stored = Schema.fromJsonString(Schema.Struct({ entries: Schema.Record(Schema.String, Schema.Unknown) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx
    .extend('swift', ({ workspace }, { onCleanup }) => {
        const folder = `${workspace.root}-swift`;
        const shim = (stamp: string) => {
            shimmed(folder, { swift: `# ${stamp}\nprintf '%s\\n' "$*" >> ${folder}/calls.log\nPATH="\${PATH#*:}"\nexec swift "$@"` });
        };
        shim('first');
        onCleanup(() => {
            rmSync(folder, { recursive: true, force: true });
        });
        const calls = () => (existsSync(join(folder, 'calls.log')) ? readFileSync(join(folder, 'calls.log'), 'utf-8').split('\n').filter(Boolean) : []);
        return { shim, calls, described: () => calls().filter((call) => call.startsWith('package ') && call.includes(' describe ')) };
    })
    .extend('warned', ({}, { onCleanup }) => {
        const warned = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        onCleanup(() => {
            warned.mockRestore();
        });
        return () =>
            warned.mock.calls
                .flat()
                .map(String)
                .filter((said) => said.includes(' is inferred from '));
    })
    .extend('core', ({}, { onCleanup }) => {
        onCleanup(() => {
            Option.toArray(Option.fromUndefinedOr(placed.folder)).forEach((folder) => rmSync(folder, { recursive: true, force: true }));
            Reflect.deleteProperty(placed, 'folder');
        });
        return (folder: string, files: Readonly<Record<string, string>>) => {
            Record.toEntries(files).forEach(([at, text]) => {
                mkdirSync(dirname(join(folder, at)), { recursive: true });
                writeFileSync(join(folder, at), text, { mode: 0o755 });
            });
            placed.folder = folder;
        };
    });

describe('graph cache', { timeout: 240_000 }, () => {
    it('describes each package once until its manifest or the swift toolchain changes, holding the last state alone', async ({ workspace, swift }) => {
        workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n' });

        const cold = await workspace.graph({ createNodes, createDependencies });
        const afterCold = swift.described().length;
        const warm = await workspace.graph({ createNodes, createDependencies });
        const afterWarm = swift.described().length;
        await workspace.write({ 'proj/Package.swift': PACKAGE.replace('"Proj"', '"Renamed"') }).graph({ createNodes, createDependencies });
        const afterEdit = swift.described().length;
        swift.shim('upgraded');
        const upgraded = await workspace.graph({ createNodes, createDependencies });
        const afterUpgrade = swift.described().length;
        const { entries } = Effect.runSync(Schema.decodeEffect(Stored)(readFileSync(join(workspace.root, 'tmp/project-graph-cache/apple.hash'), 'utf-8')));

        expect({ afterCold, afterWarm, afterEdit, afterUpgrade, cold: Record.keys(cold.nodes).toSorted(), warm: Record.keys(warm.nodes).toSorted(), upgraded: Record.keys(upgraded.nodes).toSorted(), stored: Record.size(entries) }).toStrictEqual({
            afterCold: 1,
            afterWarm: 1,
            afterEdit: 2,
            afterUpgrade: 3,
            cold: ['Lib', 'Proj'],
            warm: ['Lib', 'Proj'],
            upgraded: ['Lib', 'Renamed'],
            stored: 1,
        });
    });

    it('refuses a manifest swift package describe rejects on every graph, naming the package and the exit', async ({ workspace, swift }) => {
        workspace.write({ 'broken/Package.swift': `${TOOLS}let package = Package(name: \n` });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^swift package --package-path broken .*describe --type json exits 1: /u));
        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^swift package --package-path broken .*describe --type json exits 1: /u));
        expect(swift.described()).toHaveLength(2);
    });
});

describe('Xcode containers the core cannot read', { timeout: 240_000 }, () => {
    it.for([
        { case: 'an undecodable project.xcproj', files: { 'Viewer.xcodeproj/project.xcproj': 'not json at all' }, refusal: /^Viewer\.xcodeproj is undecodable: /u },
        { case: 'a project.pbxproj with no project.xcproj beside it', files: { 'Legacy.xcodeproj/project.pbxproj': '// !$*UTF8*$!\n{}\n' }, refusal: /^Legacy\.xcodeproj\/project\.pbxproj has no reader, and no project\.xcproj sits beside it$/u },
    ])('refuses $case, naming the container', async ({ files, refusal }, { workspace }) => {
        workspace.write(files);

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(refusal));
    });
});

describe('hosts lacking a tool the plugin reads', { timeout: 240_000 }, () => {
    it('presents no project and warns once where swift is absent', async ({ workspace, warned }) => {
        workspace.write({ 'proj/Package.swift': PACKAGE, 'other/Package.swift': PACKAGE.replace('"Proj"', '"Other"') });
        pathed(join(workspace.root, 'empty'), {});

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect({ projects: Record.keys(graph.nodes), warnings: warned() }).toStrictEqual({ projects: [], warnings: ['No project is inferred from other/Package.swift, proj/Package.swift: swift is absent on this host'] });
    });

    it.for([
        { case: 'is absent', inside: false, files: {}, ran: false, reason: 'the @rasm/nx-apple-darwin-arm64 core is absent, nx build @rasm/nx-apple-darwin-arm64 builds it' },
        {
            case: 'is older than its sources',
            inside: true,
            files: { 'Package.swift': `${TOOLS}let package = Package(name: "Analyzer", targets: [.executableTarget(name: "analyzer")])\n`, 'Sources/analyzer/main.swift': 'print("analyzer")\n', [BINARY]: '#!/bin/sh\ntouch analyzed\n', [`${BINARY}.hash`]: 'a hash of other sources' },
            ran: false,
            reason: 'the @rasm/nx-apple-darwin-arm64 core is older than its sources, nx build @rasm/nx-apple-darwin-arm64 rebuilds it',
        },
        {
            case: 'reports in a form the plugin does not read',
            inside: false,
            files: { [BINARY]: `#!/bin/sh\ntouch analyzed\nprintf '%s' '{"containers":[{"configurations":[],"inputs":[],"path":"apps/Other/Other.xcodeproj","projects":[],"schemes":[]}],"failures":{}}'\n` },
            ran: true,
            reason: 'the @rasm/nx-apple-darwin-arm64 core reports in a form this plugin does not read, nx build @rasm/nx-apple-darwin-arm64 rebuilds it: Missing key',
        },
    ])('keeps the Swift targets of each project, compiles nothing, and warns once naming the command where the core $case', async ({ inside, files, ran, reason }, { workspace, swift, warned, core }) => {
        workspace.write(APPS);
        core(inside ? join(workspace.root, 'core') : `${workspace.root}-core`, files);

        const graph = await workspace.graph({ createNodes, createDependencies });

        const viewer = Record.keys(graph.nodes['Viewer']?.data.targets ?? {});
        expect({
            swiftTargets: viewer.includes('build--swift-build'),
            xcodeTargets: viewer.filter((target) => target.endsWith('--xcodebuild')),
            xcodeOnly: 'Other' in graph.nodes,
            compiles: swift.calls().filter((call) => call.startsWith('build') && !call.includes('--show-bin-path')),
            ran: existsSync(join(workspace.root, 'analyzed')),
            warnings: warned(),
        }).toStrictEqual({ swiftTargets: true, xcodeTargets: [], xcodeOnly: false, compiles: [], ran, warnings: [expect.stringMatching(new RegExp(`^No Xcode target is inferred from ${CONTAINERS}: ${RegExp.escape(reason)}`, 'u'))] });
    });
});
