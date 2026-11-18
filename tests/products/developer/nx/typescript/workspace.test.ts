import { join, matchesGlob } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { setupWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { pathed, refusing } from '@rasm/testing/nx';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const KNIP = { 'node_modules/knip/package.json': JSON.stringify({ name: 'knip', type: 'module', exports: './index.js' }), 'node_modules/knip/index.js': 'export {};' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const duplicated = (config: string, files: string) => ({
    command: 'jscpd .',
    options: { cwd: '.' },
    cache: true,
    inputs: [`{workspaceRoot}/${files}`, `{workspaceRoot}/${config}`, { runtime: 'jscpd --version' }, { externalDependencies: [] }],
    outputs: [],
    metadata: { technologies: ['jscpd'], description: `Reports code duplicated across the workspace files ${config} selects with jscpd`, help: { command: 'jscpd --help', example: { args: ['--changed'] } } },
});

const unused = (command: string) => ({
    command,
    options: { cwd: '.' },
    cache: true,
    inputs: ['{workspaceRoot}/**/*', { externalDependencies: ['knip'] }],
    outputs: [],
    metadata: { technologies: ['knip'], description: `Reports unused files, exports, and dependencies of every workspace package with ${command}`, help: { command: 'npx knip --help', example: { args: ['--production'] } } },
});

const workspaceTargets = async (tempFs: TempFs, context: CreateNodesContext, files: Record<string, string>, options: Parameters<(typeof createNodes)[1]>[1] = {}) => {
    const written = { 'package-lock.json': '', 'package.json': JSON.stringify({ workspaces: ['**'] }), ...files };
    await tempFs.createFiles(written);
    setupWorkspaceContext(tempFs.tempDir);
    const results = await createNodes[1](
        Object.keys(written).filter((file) => matchesGlob(file, createNodes[0])),
        options,
        context,
    );
    return Object.fromEntries(results.flatMap(([, nodes]) => Object.entries({ ...nodes.projects })))['.']?.targets;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('workspace-targets');
        const previous = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } }, workspaceRoot: tempFs.tempDir }));

describe('workspace root checks', () => {
    it('should create no check target without a jscpd configuration or an installed knip', async ({ tempFs, context }) => {
        await expect(workspaceTargets(tempFs, context, { 'index.ts': 'export {};' }).then((targets) => Object.keys({ ...targets }))).resolves.toStrictEqual(['upgrade', 'upgrade--npm']);
    });

    it('should create a check--jscpd target hashing the files the jscpd pattern selects', async ({ tempFs, context }) => {
        const jscpd = duplicated('.config/jscpd.json', '**/*.ts');

        await expect(workspaceTargets(tempFs, context, { '.config/jscpd.json': JSON.stringify({ pattern: '**/*.ts' }) })).resolves.toMatchObject({
            check: { dependsOn: [{ target: 'check--*', params: 'forward' }], inputs: ['...', ...jscpd.inputs] },
            'check--jscpd': jscpd,
        });
    });

    it.for([
        ['.jscpd.json', { '.jscpd.json': '{}', '.config/jscpd.json': '{}' }],
        ['.config/.jscpd.json', { '.config/.jscpd.json': '{}' }],
        ['package.json', { 'package.json': JSON.stringify({ workspaces: ['**'], jscpd: {} }) }],
    ] as const)('should hash every file when %s, the configuration jscpd reads first, states no pattern', async ([config, files], { tempFs, context }) => {
        await expect(workspaceTargets(tempFs, context, files).then((targets) => targets?.['check--jscpd'])).resolves.toStrictEqual(duplicated(config, '**/*'));
    });

    it.for(refusing('jscpd'))('should report the command and $case of jscpd as the error of the workspace manifest', async ({ scripts, refusal }, { tempFs, context }) => {
        pathed(join(tempFs.tempDir, '.bin'), scripts);

        await expect(workspaceTargets(tempFs, context, { '.jscpd.json': '{}' })).rejects.toSatisfy(refused(refusal));
    });

    it('should create a check--knip target where knip resolves from the workspace root', async ({ tempFs, context }) => {
        await expect(workspaceTargets(tempFs, context, KNIP)).resolves.toMatchObject({ check: { dependsOn: [{ target: 'check--*', params: 'forward' }], inputs: ['...', '{workspaceRoot}/**/*', { externalDependencies: ['knip'] }] }, 'check--knip': unused('knip') });
    });

    it('should pass the knipConfig option to knip through --config', async ({ tempFs, context }) => {
        await expect(workspaceTargets(tempFs, context, KNIP, { knipConfig: '.config/knip.json' }).then((targets) => targets?.['check--knip'])).resolves.toStrictEqual(unused('knip --config .config/knip.json'));
    });
});
