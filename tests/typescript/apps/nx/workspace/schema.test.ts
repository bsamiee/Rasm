import { inspect } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Ajv, type ErrorObject } from 'ajv';
import { Array, Effect, Layer, Record } from 'effect';
import schema from 'nx/schemas/project-schema.json' with { type: 'json' };
import { Composed, SLOW } from '#/workspace/graph';

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = ({ instancePath, data }: ErrorObject) => `${instancePath} ${inspect(data, { breakLength: Number.POSITIVE_INFINITY })}`;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.merge(layer, Composed.layer), { excludeTestServices: true, timeout: SLOW.timeout })('project configuration schema', (methods) => {
    methods.effect(
        'holds only projects whose resolved configuration project-schema.json accepts',
        Effect.fnUntraced(function* () {
            const { projectGraph } = yield* Composed;
            const validate = new Ajv({ allErrors: true, strict: false, verbose: true }).compile(schema);
            const refused = Record.filter(
                Record.map(projectGraph.nodes, (node) => (validate(node.data) ? [] : Array.dedupe((validate.errors ?? []).map(located)))),
                Array.isReadonlyArrayNonEmpty,
            );
            assert.deepStrictEqual(refused, {}, 'nrwl/nx@2a424d3 packages/nx/schemas/project-schema.json, packages/nx/src/command-line/show/project.spec.ts');
        }),
        SLOW,
    );
});
