import { symlinkSync } from 'node:fs';
import { dirname, join, matchesGlob } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { CreateNodesContext } from '@nx/devkit';
import { setupWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-typescript';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const INSTALLED = { typescript: import.meta.resolve('@typescript/native/package.json'), '@nx/js': import.meta.resolve('@nx/js/package.json') } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const typecheckTarget = async (tempFs: TempFs, context: CreateNodesContext, files: Record<string, string>, options: Parameters<(typeof createNodes)[1]>[1] = {}) => {
    const written = { 'package-lock.json': '', 'package.json': JSON.stringify({ workspaces: ['**'] }), 'libs/my-lib/package.json': '{}', 'libs/my-lib/src/index.ts': 'export {};', ...files };
    await tempFs.createFiles(written);
    setupWorkspaceContext(tempFs.tempDir);
    const results = await createNodes[1](
        Object.keys(written).filter((file) => matchesGlob(file, createNodes[0])),
        options,
        context,
    );
    return Object.fromEntries(results.flatMap(([, nodes]) => Object.entries({ ...nodes.projects })))['libs/my-lib']?.targets;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('typecheck-target');
        const previous = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        Object.entries(INSTALLED).forEach(([name, from]) => {
            tempFs.createDirSync(dirname(join('node_modules', name)));
            symlinkSync(dirname(fileURLToPath(from)), join(tempFs.tempDir, 'node_modules', name), 'dir');
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }));

describe('createTypecheckTarget', () => {
    describe('build mode', () => {
        it('should leave the default tsconfig.json out of the command', async ({ tempFs, context }) => {
            const targets = await typecheckTarget(tempFs, context, {
                'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], references: [{ path: './tsconfig.lib.json' }], compilerOptions: { noEmit: true } }),
                'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { composite: true, outDir: 'dist' } }),
            });

            expect(targets?.['build--tsc']?.options).toStrictEqual({ commands: ['tsc --build'], cwd: 'libs/my-lib' });
        });

        it('should pass a non-default tsconfig to --build', async ({ tempFs, context }) => {
            const targets = await typecheckTarget(tempFs, context, {
                'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.check.json' }] }),
                'libs/my-lib/tsconfig.check.json': JSON.stringify({ include: ['src/**/*.ts'], references: [{ path: './tsconfig.lib.json' }], compilerOptions: { noEmit: true } }),
                'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { composite: true, outDir: 'dist' } }),
            });

            expect(targets?.['check--tsc']?.options).toStrictEqual({ commands: ['tsc --build tsconfig.check.json'], cwd: 'libs/my-lib' });
        });

        it('should depend on the build target and on referenced projects', async ({ tempFs, context }) => {
            const targets = await typecheckTarget(tempFs, context, {
                'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], references: [{ path: './tsconfig.lib.json' }], compilerOptions: { composite: true, outDir: 'dist' } }),
                'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { composite: true, outDir: 'out' } }),
            });

            expect(targets?.['check--tsc']).toMatchObject({ executor: 'nx:noop', dependsOn: ['build--tsc'] });
            expect(targets?.['build--tsc']).toMatchObject({ options: { commands: ['tsc --build'] }, dependsOn: ['codegen--*', '^codegen', '^build--tsc'], syncGenerators: ['@nx/js:typescript-sync'] });
        });
    });

    describe('noEmit mode', () => {
        it('should leave the default tsconfig.json out of the command', async ({ tempFs, context }) => {
            const targets = await typecheckTarget(tempFs, context, { 'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { noEmit: true } }) });

            expect(targets?.['check--tsc']).toMatchObject({ options: { commands: ['tsc'], cwd: 'libs/my-lib' }, metadata: { help: { command: 'npx tsc --help' } } });
        });

        it('should pass a non-default tsconfig with -p, and depend on upstream builds so a referenced library builds once through its own cached target rather than inside each dependent tsc --build', async ({ tempFs, context }) => {
            const targets = await typecheckTarget(tempFs, context, {
                'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.app.json' }] }),
                'libs/my-lib/tsconfig.app.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { noEmit: true } }),
            });

            expect(targets?.['check--tsc']).toMatchObject({ options: { commands: ['tsc -p tsconfig.app.json'], cwd: 'libs/my-lib' }, outputs: [] });
            expect(targets?.['check--tsc']?.dependsOn).toStrictEqual(['codegen--*', '^codegen', '^build--tsc']);
            expect(targets?.['build--tsc']).toBeUndefined();
        });

        it('should derive inputs from the named inputs and the compiler', async ({ tempFs, context }) => {
            const files = { 'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { noEmit: true } }) };
            const named = await typecheckTarget(tempFs, { ...context, nxJsonConfiguration: { namedInputs: { production: [] } } }, files, { compilerPackages: ['typescript', '@effect/tsgo'] });
            const unnamed = await typecheckTarget(tempFs, context, files);

            expect(named?.['check--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/src/index.ts', '^production', { dependentTasksOutputFiles: '**/*.{d.ts,d.cts,d.mts,tsbuildinfo}', transitive: true }, { externalDependencies: ['typescript', '@effect/tsgo'] }]);
            expect(unnamed?.['check--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/src/index.ts', '^default', { dependentTasksOutputFiles: '**/*.{d.ts,d.cts,d.mts,tsbuildinfo}', transitive: true }, { externalDependencies: ['typescript'] }]);
        });
    });
});

describe('selectTypecheckTsConfig', () => {
    it('should not run a tsconfig name that is unsafe in a shell', async ({ tempFs, context }) => {
        const targets = await typecheckTarget(tempFs, context, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig $(id).json' }, { path: './tsconfig;x.json' }] }),
            'libs/my-lib/tsconfig $(id).json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { noEmit: true } }),
            'libs/my-lib/tsconfig;x.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { noEmit: true } }),
        });

        expect(targets?.['check--tsc']?.options).toStrictEqual({ commands: ["tsc -p 'tsconfig $(id).json'", "tsc -p 'tsconfig;x.json'"], cwd: 'libs/my-lib' });
    });
});
