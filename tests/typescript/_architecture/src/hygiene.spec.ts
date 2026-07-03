import { Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { expect, layer } from '@effect/vitest';
import { Snapshots } from '@rasm/ts-testkit/gauges';
import { Array, Effect } from 'effect';

// --- [CONSTANTS] -------------------------------------------------------------------------

const _ROOT = new URL('../../../..', import.meta.url).pathname;

// The standing estate-wide hygiene sweep; the engine's red-capability is proven by the kit's own
// falsification spec, this suite is its one standing consumer over the live tree.
const _ESTATE = ['tests/typescript', 'libs/typescript'] as const;

// --- [SPECS] -----------------------------------------------------------------------------

layer(NodeContext.layer)('snapshot hygiene', (it) => {
    it.effect('no snapshot outlives its owning spec anywhere in the estate', () =>
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const audits = yield* Effect.forEach(_ESTATE, (home) => Snapshots.audit(path.join(_ROOT, home)));
            expect(Array.flatMap(audits, (audit) => audit.orphans)).toEqual([]);
        }),
    );
});
