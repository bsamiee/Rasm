import { matchesGlob } from 'node:path';
import { assert } from '@effect/vitest';
import { createNodes } from '@rasm/nx-buf';
import manifest from '@rasm/nx-buf/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    files: {
        'package.json': '{ "name": "root", "workspaces": ["contracts", "events"] }\n',
        'contracts/package.json': '{ "name": "contracts" }\n',
        'contracts/buf.gen.yaml': 'version: v2\ninputs:\n    - directory: proto\nplugins:\n    - local: protoc-gen-es\n      out: generated/es\n    - local: protoc-gen-connect-query\n      out: generated/query\n',
        'contracts/proto/sheet.proto': 'syntax = "proto3";\npackage sheet;\nmessage Sheet {}\n',
        'events/package.json': '{ "name": "events" }\n',
        'events/buf.gen.yaml': 'version: v2\ninputs:\n    - directory: ../contracts/proto\nplugins:\n    - local: protoc-gen-es\n      out: generated\n',
    },
    broken: ['events/buf.gen.yaml', 'version: v2\nplugins: generated\n'],
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const matching = (pattern: string) => matchesGlob('contracts/proto/sheet.proto', pattern);

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('runs buf generate from the template folder, cached, writing each plugin out folder and reading each input folder', () =>
        inferred(subject).pipe(
            Effect.map(running('buf generate')),
            Effect.map(Array.map(({ project, cwd, cache, outputs, read }) => ({ project, cwd, cache, outputs, proto: read.some(matching) }))),
            Effect.map((generated) =>
                assert.deepStrictEqual(generated, [
                    { project: 'contracts', cwd: 'contracts', cache: true, outputs: ['contracts/generated/es', 'contracts/generated/query'], proto: true },
                    { project: 'events', cwd: 'events', cache: true, outputs: ['events/generated'], proto: true },
                ]),
            ),
        ),
    );
});
