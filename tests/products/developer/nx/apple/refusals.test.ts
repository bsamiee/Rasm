import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { refused } from '@rasm/testing/errors';
import { it, pathed, shimmed } from '@rasm/testing/nx';
import { describe, expect, onTestFinished, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.mock(import('node:url'), async (actual) => {
    const real = await actual();
    return { ...real, fileURLToPath: (url, options) => (core.at !== undefined && String(url).endsWith('/nx-apple-analyzer') ? core.at : real.fileURLToPath(url, options)) };
});

// --- [CONSTANTS] -----------------------------------------------------------------------

const core = vi.hoisted((): { at?: string } => ({}));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('packages', { timeout: 120_000 }, () => {
    const PACKAGE = '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Proj", products: [.library(name: "Lib", targets: ["Lib"])], targets: [.target(name: "Lib")])\n';

    it('refuses a manifest swift package describe rejects, naming the package and the exit', async ({ workspace }) => {
        workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n', 'broken/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: \n' });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^swift package --package-path broken .*describe --type json exits 1: /u));
    });

    it('presents no project and warns where swift is absent', async ({ workspace }) => {
        const folder = mkdtempSync(join(tmpdir(), 'apple-absent-'));
        onTestFinished(() => rmSync(folder, { recursive: true }));
        const warned = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        onTestFinished(() => warned.mockRestore());
        workspace.write({ 'proj/Package.swift': PACKAGE, 'proj/Sources/Lib/Lib.swift': 'public let lib = 1\n' });
        pathed(folder, {});

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect([Object.keys(graph.nodes), warned.mock.calls]).toStrictEqual([[], [['proj/Package.swift presents no project: swift is absent on this host']]]);
    });
});

describe('Xcode projects', { timeout: 120_000 }, () => {
    it('refuses a project.xcproj the analyzer cannot decode, naming the project', async ({ workspace }) => {
        workspace.write({ 'Viewer.xcodeproj/project.xcproj': 'not json at all' });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^Viewer\.xcodeproj is undecodable: /u));
    });

    it('presents no project and warns where the core reports without a key the plugin reads, naming the command that rebuilds it', async ({ workspace }) => {
        const folder = mkdtempSync(join(tmpdir(), 'apple-core-'));
        onTestFinished(() => rmSync(folder, { recursive: true }));
        const warned = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        onTestFinished(() => warned.mockRestore());
        shimmed(folder, { 'nx-apple-analyzer': `cat > /dev/null\nprintf '%s' '{"containers":[{"buildFiles":[],"configurations":["Debug","Release"],"inputs":[],"path":"Viewer.xcodeproj","projects":[],"schemes":[]}],"failures":{}}'` });
        Object.assign(core, { at: join(folder, 'nx-apple-analyzer') });
        onTestFinished(() => {
            Reflect.deleteProperty(core, 'at');
        });
        workspace.write({ 'Viewer.xcodeproj/project.xcproj': '{ "default-configuration": "Release", "configurations": [ "Debug", "Release" ], "localizations": { "development": "en", "supported": [ "Base" ] }, "files": [], "targets": [] }\n' });

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect([Object.keys(graph.nodes), warned.mock.calls]).toStrictEqual([[], [[expect.stringMatching(/^Viewer\.xcodeproj\/project\.xcproj presents no project: the @rasm\/nx-apple-darwin-arm64 core reports in a form this plugin does not read, nx build @rasm\/nx-apple-darwin-arm64 rebuilds it: Missing key/u)]]]);
    });

    it('refuses a project.pbxproj with no project.xcproj beside it', async ({ workspace }) => {
        workspace.write({ 'Legacy.xcodeproj/project.pbxproj': '// !$*UTF8*$!\n{}\n' });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^Legacy\.xcodeproj\/project\.pbxproj has no reader, and no project\.xcproj sits beside it$/u));
    });
});
