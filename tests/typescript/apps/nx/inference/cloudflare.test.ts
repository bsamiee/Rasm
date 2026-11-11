import runtime from '@cloudflare/workers-types/package.json' with { type: 'json' };
import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-cloudflare';
import manifest from '@rasm/nx-cloudflare/package.json' with { type: 'json' };
import tool from 'cf/package.json' with { type: 'json' };
import { Array, Effect, Record } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOL = /--.*$/u;
const VERBS = ['codegen', 'build', 'check', 'fix', 'test', 'serve', 'preview', 'install', 'deploy', 'upgrade', 'clean'];
const SOURCE = {
    cached: 'naxodev/oss@8b43d4a packages/nx-cloudflare/src/plugin.spec.ts "typegen is deterministic -> cacheable", cloudflare/workers-sdk@877b249 fixtures/experimental-new-config/turbo.json outputs ".cloudflare/types/**"',
    packaged: 'cloudflare/workers-sdk@877b249 fixtures/vitest-plugin-examples/package.json "typegen:new-config": "cd new-config && cf workers types --include-runtime=false" beside @cloudflare/workers-types',
    verbs: 'README [04] verbs, [03] "a project answers a verb only where one of its tools performs that operation": the Vite config serves a Worker and a Pulumi stack deploys it, cloudflare/workers-sdk@877b249 fixtures/experimental-new-config/package.json runs cf for "check:type" alone',
} as const;

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: [tool.name],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["worker", "queue"] }\n',
        'worker/package.json': '{ "name": "worker", "type": "module" }\n',
        'worker/cloudflare.config.ts': 'export default {};\n',
        'queue/package.json': '{ "name": "queue", "type": "module" }\n',
        'queue/cloudflare.config.ts': 'export default {};\n',
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const answered = (graph: ProjectGraph) => {
    const targets = Object.values(graph.nodes).flatMap((node) => Record.toEntries(node.data.targets ?? {}));
    return {
        verbs: Array.intersection(
            VERBS,
            targets.map(([name]) => name.replace(TOOL, '')),
        ).toSorted(),
        continuous: targets.filter(([, target]) => target.continuous === true).map(([name]) => name),
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('generates the types of each Worker with cf workers types, cached, writing .cloudflare/types', () =>
        inferred(subject).pipe(
            Effect.map(running('cf workers types')),
            Effect.map(Array.map(({ project, command, cache, outputs }) => ({ project, command, cache, outputs }))),
            Effect.map((generated) =>
                assert.deepStrictEqual(
                    generated,
                    [
                        { project: 'queue', command: 'cf workers types', cache: true, outputs: ['queue/.cloudflare/types'] },
                        { project: 'worker', command: 'cf workers types', cache: true, outputs: ['worker/.cloudflare/types'] },
                    ],
                    SOURCE.cached,
                ),
            ),
        ),
    );

    it.effect('leaves the runtime types out of the generated file where a Worker resolves @cloudflare/workers-types', () =>
        inferred({ ...subject, packages: [tool.name, runtime.name] }).pipe(
            Effect.map(running('cf workers types')),
            Effect.map(Array.map(({ project, command }) => ({ project, command }))),
            Effect.map((generated) =>
                assert.deepStrictEqual(
                    generated,
                    [
                        { project: 'queue', command: 'cf workers types --include-runtime=false' },
                        { project: 'worker', command: 'cf workers types --include-runtime=false' },
                    ],
                    SOURCE.packaged,
                ),
            ),
        ),
    );

    it.effect('answers the codegen and clean verbs alone, and starts no continuous task', () => Effect.map(inferred(subject), (graph) => assert.deepStrictEqual(answered(graph), { verbs: ['clean', 'codegen'], continuous: [] }, SOURCE.verbs)));
});
