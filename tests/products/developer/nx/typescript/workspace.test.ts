import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { it, pathed, refusing } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '' };
const KNIP = { 'node_modules/knip/package.json': '{"name":"knip","type":"module","exports":"./index.js"}', 'node_modules/knip/index.js': 'export {};' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('workspace root targets', () => {
    it.for([
        { manager: 'npm', files: ROOT, catalogs: [] },
        { manager: 'pnpm', files: { 'package.json': '{"name":"root"}', 'pnpm-lock.yaml': '', 'pnpm-workspace.yaml': 'packages: []\n' }, catalogs: ['pnpm-workspace.yaml'] },
    ])('upgrades through $manager, moving the catalogs it reads: $catalogs', async ({ manager, files, catalogs }, { workspace }) => {
        const graph = await workspace.write(files).graph({ createNodes });

        expect(graph.nodes['root']?.data.targets?.[`upgrade--${manager}`]?.options).toStrictEqual({ catalogs });
    });

    it.for([
        { case: 'the pattern of .config/jscpd.json', files: { '.config/jscpd.json': '{"pattern":"**/*.ts"}' }, inputs: ['{workspaceRoot}/**/*.ts', '{workspaceRoot}/.config/jscpd.json'] },
        { case: 'every file, as .jscpd.json, which jscpd reads before .config/jscpd.json, states no pattern', files: { '.jscpd.json': '{}', '.config/jscpd.json': '{"pattern":"**/*.ts"}' }, inputs: ['{workspaceRoot}/**/*', '{workspaceRoot}/.jscpd.json'] },
        { case: 'every file, as the jscpd row of package.json states no pattern', files: { 'package.json': '{"name":"root","workspaces":["libs/*"],"jscpd":{}}' }, inputs: ['{workspaceRoot}/**/*', '{workspaceRoot}/package.json'] },
        { case: 'nothing, as no jscpd configuration exists and no check--jscpd runs', files: {}, inputs: undefined },
    ])('hashes $case for check--jscpd', async ({ files, inputs }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, ...files }).graph({ createNodes });

        expect(graph.nodes['root']?.data.targets?.['check--jscpd']?.inputs?.filter((input) => typeof input === 'string')).toStrictEqual(inputs);
    });

    it.for(refusing('jscpd'))('refuses the graph with the command and $case of jscpd', async ({ scripts, refusal }, { workspace }) => {
        pathed(join(workspace.root, '.bin'), scripts);

        await expect(workspace.write({ ...ROOT, '.jscpd.json': '{}' }).graph({ createNodes })).rejects.toSatisfy(refused(refusal));
    });

    it.for([
        { case: 'knip does not resolve', files: {}, options: undefined, run: undefined },
        { case: 'knip resolves', files: KNIP, options: undefined, run: { command: 'knip', cwd: '.' } },
        { case: 'knip resolves and knipConfig names its configuration', files: KNIP, options: { knipConfig: '.config/knip.json' }, run: { command: 'knip --config .config/knip.json', cwd: '.' } },
    ])('runs check--knip as $run.command when $case', async ({ files, options, run }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, ...files }).graph({ createNodes }, options);

        expect(graph.nodes['root']?.data.targets?.['check--knip']?.options).toStrictEqual(run);
    });
});
