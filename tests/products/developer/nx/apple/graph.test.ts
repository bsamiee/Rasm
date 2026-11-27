import { existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { globWithWorkspaceContext, hashFile } from '@nx/devkit/internal';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { ExitError } from '@rasm/process';
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
    const { below } = await import('@rasm/nx/inference');
    const plugin = real.fileURLToPath(import.meta.resolve('@rasm/nx-apple/package.json'));
    const installed = parent(createRequire(plugin).resolve('@rasm/nx-apple-darwin-arm64/package.json'));
    const linked = joined(parent(plugin), 'node_modules/@rasm/nx-apple-darwin-arm64');
    return {
        ...real,
        fileURLToPath: (url, options) => {
            const at = real.fileURLToPath(url, options);
            const within = [installed, linked].find((folder) => below(folder)(at));
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

// --- [OPERATIONS] ----------------------------------------------------------------------

const stamped = (root: string) => JSON.stringify({ sources: ['core/{Package.swift,Package.resolved,Sources/**/*}'], files: Record.fromIterableWith(['core/Package.swift', 'core/Package.resolved', 'core/Sources/analyzer/main.swift'], (at) => [at, hashFile(join(root, at))]) });

const built = (root: string, place: (folder: string, files: Readonly<Record<string, string>>) => void) => {
    place(join(root, 'core'), {
        'Package.swift': `${TOOLS}let package = Package(name: "Analyzer", targets: [.executableTarget(name: "analyzer"), .testTarget(name: "AnalyzerTests")])\n`,
        'Package.resolved': '{ "pins": [], "version": 3 }\n',
        'Sources/analyzer/main.swift': 'print("analyzer")\n',
        'Tests/AnalyzerTests/AnalyzerTests.swift': 'import Testing\n',
        [BINARY]: `#!/bin/sh\ntouch analyzed\nprintf '%s' '{"containers":[],"failures":{}}'\n`,
    });
    place(join(root, 'core'), { [`${BINARY}.hash`]: stamped(root) });
};

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

describe('files Nx hands the plugin', () => {
    it('are the Xcode containers alone, so a package manifest at the workspace root charges no project configuration change to the root project, nor does a setting, scheme, plist, entitlements, or test plan edit', async ({ workspace }) => {
        const markers = ['A/A.xcodeproj/project.pbxproj', 'A/A.xcodeproj/project.xcproj', 'W.xcworkspace/contents.xcworkspacedata'];
        workspace.write(
            Record.fromIterableWith(
                [
                    ...markers,
                    'Package.swift',
                    'Package@swift-6.4.swift',
                    'proj/Package.swift',
                    'proj/Package@swift-6.4.swift',
                    '.config/xcode.xcconfig',
                    'Xcode.xcconfig',
                    'A/A.xcodeproj/xcshareddata/xcschemes/A.xcscheme',
                    'W.xcworkspace/xcshareddata/xcschemes/W.xcscheme',
                    'A/Info.plist',
                    'A/Settings.plist',
                    'A/A.entitlements',
                    'A/A.xctestplan',
                ],
                (at) => [at, ''],
            ),
        );

        const matched = await globWithWorkspaceContext(workspace.root, [createNodes[0]]);

        expect(matched.toSorted()).toStrictEqual(markers);
    });
});

describe('graph cache', () => {
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

    it('drops the projects of a package whose manifest is deleted after a first graph', async ({ workspace }) => {
        workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n', 'other/Package.swift': `${TOOLS}let package = Package(name: "Other", targets: [.target(name: "Kept")])\n`, 'other/Sources/Kept/Kept.swift': 'public let kept = 1\n' });

        const whole = await workspace.graph({ createNodes, createDependencies });
        const deleted = await workspace.remove('other/Package.swift').graph({ createNodes, createDependencies });

        expect({ whole: Record.keys(whole.nodes).toSorted(), deleted: Record.keys(deleted.nodes).toSorted() }).toStrictEqual({ whole: ['Kept', 'Lib', 'Other', 'Proj'], deleted: ['Lib', 'Proj'] });
    });

    it('describes again after an edit to a setting, scheme, plist, or entitlements file the analysis reads, though none marks a project', async ({ workspace, swift }) => {
        workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n', 'proj/Configs/Base.xcconfig': 'SWIFT_VERSION = 6.0\n', 'Fixture.xcworkspace/xcshareddata/xcschemes/Lib.xcscheme': SCHEME('Lib') });

        await workspace.graph({ createNodes, createDependencies });
        await workspace.graph({ createNodes, createDependencies });
        await workspace.write({ 'proj/Configs/Base.xcconfig': 'SWIFT_VERSION = 6.4\n' }).graph({ createNodes, createDependencies });
        await workspace.write({ 'Fixture.xcworkspace/xcshareddata/xcschemes/Lib.xcscheme': SCHEME('Other') }).graph({ createNodes, createDependencies });
        await workspace.write({ 'proj/Sources/Lib/Info.plist': '<plist version="1.0"><dict/></plist>\n' }).graph({ createNodes, createDependencies });
        await workspace.write({ 'proj/Sources/Lib/Lib.entitlements': '<plist version="1.0"><dict/></plist>\n' }).graph({ createNodes, createDependencies });

        expect(swift.described()).toHaveLength(5);
    });

    it('describes again after XCODE_XCCONFIG_FILE names another file, as every Xcode target hashes the file it names', async ({ workspace, swift, onTestFinished }) => {
        workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n', 'One.xcconfig': '', 'Two.xcconfig': '' });
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });

        vi.stubEnv('XCODE_XCCONFIG_FILE', join(workspace.root, 'One.xcconfig'));
        await workspace.graph({ createNodes, createDependencies });
        await workspace.graph({ createNodes, createDependencies });
        vi.stubEnv('XCODE_XCCONFIG_FILE', join(workspace.root, 'Two.xcconfig'));
        await workspace.graph({ createNodes, createDependencies });

        expect(swift.described()).toHaveLength(2);
    });

    it('refuses a manifest swift package describe rejects on every graph, naming the package and the exit', async ({ workspace, swift }) => {
        workspace.write({ 'broken/Package.swift': `${TOOLS}let package = Package(name: \n` });
        const refusal = Schema.Struct({ ...ExitError.fields, command: Schema.String.check(Schema.isPattern(/^swift package --package-path broken .*describe --type json$/u)), code: Schema.Literal(1) });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(refusal));
        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(refusal));
        expect(swift.described()).toHaveLength(2);
    });
});

describe('Xcode containers the core cannot read', () => {
    it.for([
        { case: 'an undecodable project.xcproj', files: { 'Viewer.xcodeproj/project.xcproj': 'not json at all' }, refusal: /^Viewer\.xcodeproj is undecodable: /u },
        { case: 'a project.pbxproj with no project.xcproj beside it', files: { 'Legacy.xcodeproj/project.pbxproj': '// !$*UTF8*$!\n{}\n' }, refusal: /^Legacy\.xcodeproj\/project\.pbxproj has no reader, and no project\.xcproj sits beside it$/u },
    ])('refuses $case, naming the container', async ({ files, refusal }, { workspace }) => {
        workspace.write(files);

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(refusal));
    });
});

describe('a core built inside the workspace', () => {
    it('reads the containers through it after an edit to a file outside its stamped sources, a test edit included', async ({ workspace, core }) => {
        workspace.write(APPS);
        built(workspace.root, core);
        core(join(workspace.root, 'core'), { 'Tests/AnalyzerTests/AnalyzerTests.swift': 'import Testing\n\n@Test func added() {}\n' });

        await workspace.graph({ createNodes, createDependencies });

        expect(existsSync(join(workspace.root, 'analyzed'))).toBe(true);
    });

    it.for([
        { case: 'a source it was built from changes', files: { 'Sources/analyzer/main.swift': 'print("edited")\n' }, removed: [], named: 'core/Sources/analyzer/main.swift' },
        { case: 'a source joins its sources', files: { 'Sources/analyzer/Added.swift': 'let added = 1\n' }, removed: [], named: 'core/Sources/analyzer/Added.swift' },
        { case: 'its stamp is absent', files: {}, removed: [`${BINARY}.hash`], named: `core/${BINARY}.hash` },
    ])('refuses the graph, naming the file and the command that rebuilds the core, where $case', async ({ files, removed, named }, { workspace, core }) => {
        workspace.write(APPS);
        built(workspace.root, core);
        core(join(workspace.root, 'core'), files);
        removed.forEach((at) => rmSync(join(workspace.root, 'core', at)));

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(new RegExp(`^the @rasm/nx-apple-darwin-arm64 core predates ${RegExp.escape(named)}, mise deps install nx-apple rebuilds it$`, 'u')));
        expect(existsSync(join(workspace.root, 'analyzed'))).toBe(false);
    });
});

describe('hosts lacking a tool the plugin reads', () => {
    it('presents no project and warns once where swift is absent', async ({ workspace, warned }) => {
        workspace.write({ 'proj/Package.swift': PACKAGE, 'other/Package.swift': PACKAGE.replace('"Proj"', '"Other"') });
        pathed(join(workspace.root, 'empty'), {});

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect({ projects: Record.keys(graph.nodes), warnings: warned() }).toStrictEqual({ projects: [], warnings: ['No project is inferred from other/Package.swift, proj/Package.swift: swift is absent on this host'] });
    });

    it.for([
        { case: 'is absent', files: {}, ran: false, reason: 'the @rasm/nx-apple-darwin-arm64 core is absent, mise deps install nx-apple builds it' },
        {
            case: 'reports in a form the plugin does not read',
            files: { [BINARY]: `#!/bin/sh\ntouch analyzed\nprintf '%s' '{"containers":[{"configurations":[],"inputs":[],"path":"apps/Other/Other.xcodeproj","projects":[],"schemes":[]}],"failures":{}}'\n` },
            ran: true,
            reason: 'the @rasm/nx-apple-darwin-arm64 core reports in a form this plugin does not read, mise deps install nx-apple rebuilds it: Missing key',
        },
    ])('keeps the Swift targets of each project, compiles nothing, and warns once naming the command where the core $case', async ({ files, ran, reason }, { workspace, swift, warned, core }) => {
        workspace.write(APPS);
        core(`${workspace.root}-core`, files);

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
