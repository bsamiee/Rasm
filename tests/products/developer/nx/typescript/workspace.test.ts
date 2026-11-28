import { spawnSync } from 'node:child_process';
import { join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { it, pathed, refusing, typescript } from '@rasm/testing/nx';
import { HashPlanInspector } from 'nx/src/hasher/hash-plan-inspector';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","type":"module","workspaces":["libs/*"]}', 'package-lock.json': '' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = async (graph: ProjectGraph, root: string, target: string, tool: string) => {
    const inspector = new HashPlanInspector({ ...graph, externalNodes: { [`npm:${tool}`]: { type: 'npm', name: `npm:${tool}`, data: { packageName: tool, version: '6.0.0' } } } }, root);
    await inspector.init();
    return inspector.inspectTaskInputs({ project: 'root', target })[`root:${target}`]?.files.toSorted();
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('workspace root checks', () => {
    it.for([
        { case: '.config/jscpd.json alone', files: { '.config/jscpd.json': '{"pattern":"**/*.ts"}' } },
        { case: '.jscpd.json beside .config/jscpd.json', files: { '.jscpd.json': '{"pattern":"**/*.ts"}', '.config/jscpd.json': '{"pattern":"**/*.ts"}' } },
        { case: 'the jscpd row of package.json', files: { 'package.json': '{"name":"root","type":"module","workspaces":["libs/*"],"jscpd":{"pattern":"**/*.ts"}}' } },
    ])('hashes for check--jscpd the configuration jscpd itself reads from $case', async ({ files }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, ...files, 'libs/lib/index.ts': 'export {};' }).graph({ createNodes });
        const read = /^Using config from (?<config>.+)$/mu.exec(spawnSync(...workspace.spawned('jscpd', ['--debug', '.'], { encoding: 'utf-8' })).stderr)?.groups?.['config'] ?? '';

        await expect(hashed(graph, workspace.root, 'check--jscpd', 'jscpd')).resolves.toContain(read);
    });

    it('leaves out of the check--jscpd hash the files the jscpd ignore row skips', async ({ workspace }) => {
        const graph = await workspace.write({ ...ROOT, '.jscpd.json': '{"pattern":"**/*.ts","ignore":["libs/generated/**"]}', 'libs/lib/index.ts': 'export {};', 'libs/generated/index.ts': 'export {};' }).graph({ createNodes });

        await expect(hashed(graph, workspace.root, 'check--jscpd', 'jscpd')).resolves.toStrictEqual(['.jscpd.json', 'libs/lib/index.ts']);
    });

    it.for(refusing('jscpd'))('refuses the graph with the command and $case of jscpd', async ({ scripts, refusal }, { workspace }) => {
        pathed(join(workspace.root, '.bin'), scripts);

        await expect(workspace.write({ ...ROOT, '.jscpd.json': '{}' }).graph({ createNodes })).rejects.toSatisfy(refused(refusal));
    });

    it.for([{ manager: 'yarn', lock: 'yarn.lock', files: ['.config/knip.json', '.config/mise.toml', '.github/workflows/ci.yml', 'libs/lib/index.ts', 'libs/lib/package.json', 'libs/lib/tsconfig.json', 'package.json', 'yarn.lock'] }])(
        'hashes the sources, manifests, configurations, and $manager lockfile knip reads for check--knip, leaving out documents, stylesheets, and manifests no knip plugin reads',
        async ({ lock, files }, { workspace }) => {
            const graph = await workspace
                .install(typescript, new URL('../package.json', import.meta.resolve('knip')).href)
                .write({
                    'package.json': '{"name":"root","type":"module","workspaces":["libs/*"]}',
                    [lock]: '',
                    '.config/knip.json': '{"ignoreBinaries":["uv"]}',
                    '.github/workflows/ci.yml': 'on: push\n',
                    '.config/mise.toml': '[tools]\n',
                    'libs/lib/package.json': '{"name":"lib"}',
                    'libs/lib/tsconfig.json': '{"include":["*.ts"]}',
                    'libs/lib/index.ts': 'export {};',
                    'libs/lib/README.md': '# lib',
                    'site/page.css': 'a { color: black; }',
                    'crates/core/Cargo.toml': '[package]\nname = "core"\n',
                    'libs/lib/pyproject.toml': '[project]\nname = "lib"\n',
                })
                .graph({ createNodes }, { knipConfig: '.config/knip.json' });

            await expect(hashed(graph, workspace.root, 'check--knip', 'knip')).resolves.toStrictEqual(files);
        },
    );
});
