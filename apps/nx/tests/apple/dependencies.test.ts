import { type CreateDependenciesContext, type CreateNodesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const consumer = (name: string, path: string) => `// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "${name}", dependencies: [.package(path: "${path}")], targets: [.executableTarget(name: "${name}", dependencies: [.product(name: "Clients", package: "clients")])])\n`;

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('apple-dependencies');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        await tempFs.createFiles({
            'clients/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Clients", products: [.library(name: "Clients", targets: ["Clients"])], targets: [.target(name: "Clients")])\n',
            'clients/Sources/Clients/Clients.swift': 'public let clients = 1\n',
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }))
    .extend('contextWith', ({ tempFs }) => (projects: Readonly<Record<string, string>>): CreateDependenciesContext => ({
        workspaceRoot: tempFs.tempDir,
        projects: Record.map(projects, (root, name) => ({ name, root })),
        externalNodes: {},
        fileMap: { projectFileMap: {}, nonProjectFiles: [] },
        filesToProcess: { projectFileMap: {}, nonProjectFiles: [] },
        nxJsonConfiguration: {},
    }));

describe('@rasm/nx-apple/plugin/dependencies', { timeout: 120_000 }, () => {
    const [, createNodesFunction] = createNodes;

    it('records an edge as static when the source project owns the build file', async ({ tempFs, context, contextWith }) => {
        await tempFs.createFiles({ 'Package.swift': consumer('App', 'clients'), 'Sources/App/main.swift': 'import Clients\n' });
        await createNodesFunction(['Package.swift', 'clients/Package.swift'], {}, context);

        const result = await createDependencies({}, contextWith({ App: '.', Clients: 'clients' }));

        expect(result).toStrictEqual([{ source: 'App', target: 'Clients', type: DependencyType.static, sourceFile: 'Package.swift' }]);
    });

    it('records a nested project that owns its build file as static', async ({ tempFs, context, contextWith }) => {
        await tempFs.createFiles({ 'api-checker/core/Package.swift': consumer('Core', '../../clients'), 'api-checker/core/Sources/Core/main.swift': 'import Clients\n' });
        await createNodesFunction(['api-checker/core/Package.swift', 'clients/Package.swift'], {}, context);

        const result = await createDependencies({}, contextWith({ Core: 'api-checker/core', Clients: 'clients' }));

        expect(result[0]).toMatchObject({ type: DependencyType.static, sourceFile: 'api-checker/core/Package.swift' });
    });

    it('records an ancestor-configured project as implicit, since Nx rejects a sourceFile outside the source project', async ({ tempFs, context, contextWith }) => {
        await tempFs.createFiles({
            'Package.swift':
                '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Root", products: [.library(name: "Core", targets: ["Core"]), .library(name: "Clients", targets: ["Clients"])], targets: [.target(name: "Core", dependencies: ["Clients"], path: "core"), .target(name: "Clients", path: "lib")])\n',
            'core/Core.swift': 'import Clients\n',
            'lib/Clients.swift': 'public let clients = 1\n',
        });
        await createNodesFunction(['Package.swift'], {}, context);

        const result = await createDependencies({}, contextWith({ Root: '.', Core: 'core', Clients: 'lib' }));

        expect(result).toStrictEqual([
            {
                source: 'Core',
                target: 'Clients',
                type: DependencyType.implicit,
            },
        ]);
        expect(result[0]).not.toHaveProperty('sourceFile');
    });
});
