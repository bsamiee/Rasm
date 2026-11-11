import { assert } from '@effect/vitest';
import { hashed } from '@rasm/testing/nx/cli';
import { Array, Effect, Record } from 'effect';
import type { Subject } from '#/inference/cases';
import { type Methods, placed, present, SLOW } from '#/inference/workspaces';

// --- [CONSTANTS] -----------------------------------------------------------------------

const KINDS = ['files', 'runtime', 'environment'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const continuous = (subject: Subject, it: Methods) =>
    it.effect.skipIf(!present(subject))(
        'hashes into each cacheable task the files, runtime commands, and variables of every continuous task it waits on over an edge that keeps inputs',
        Effect.fnUntraced(function* () {
            const hashes = yield* Effect.flatMap(placed(subject), hashed);
            const waiting = Record.toEntries(Record.filter(hashes, ({ cache }) => cache)).flatMap(([task, held]) => Array.cartesian(held.waits, KINDS).map(([served, kind]) => ({ task, served, kind, held: held[kind] })));
            const lacking = waiting.flatMap(({ task, served, kind, held }) => Array.difference(hashes[served]?.[kind] ?? [], held).map((input) => `${task} lacks ${kind} ${input} of ${served}`));
            assert.deepStrictEqual(lacking, [], 'nrwl/nx@2a424d3 astro-docs reference/inputs.mdoc "Inputs of continuous dependencies", packages/nx/src/native/tasks/hash_planner.rs a_continuous_dependency_contributes_its_inputs_and_its_servers');
        }),
        SLOW,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { continuous };
