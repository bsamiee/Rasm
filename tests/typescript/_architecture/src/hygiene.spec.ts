import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, layer, it as vanilla } from '@effect/vitest';
import { Snapshots } from '@rasm/ts-testkit/gauges';
import { Array, Effect, Option, pipe } from 'effect';

// --- [CONSTANTS] -------------------------------------------------------------------------

const _ROOT = new URL('../../../..', import.meta.url).pathname;

// The standing estate-wide hygiene sweep; the engine's red-capability is proven by the kit's own
// falsification spec, this suite is its one standing consumer over the live tree.
const _ESTATE = ['tests/typescript', 'libs/typescript'] as const;

// The per-spec density cap binds the colocated unit specs of the runtime branch. The kit
// falsification suites and this gauge home live under tests/typescript by the topology law and are
// the declared carve-out: falsification breadth is proof surface, not spec sprawl.
const _SPEC_CAP = 175;
const _SPEC_FILE = /\.(spec|test)\.(ts|tsx|mts|cts)$/;

// Authoring corpora and tool trees never join the density sweep.
const _PRUNE = /(^|\/)(node_modules|dist|coverage|\.git|\.planning|\.api)(\/|$)/;

// --- [OPERATIONS] ------------------------------------------------------------------------

// LOC is the non-blank line count; a spec over cap is a collapse demand, never a split invitation.
const _capped = (entry: string, text: string): Option.Option<string> =>
    pipe(
        Array.filter(text.split('\n'), (line) => line.trim().length > 0),
        (lines) => (lines.length > _SPEC_CAP ? Option.some(`${entry}: ${lines.length} LOC > ${_SPEC_CAP}`) : Option.none()),
    );

// --- [SPECS] -----------------------------------------------------------------------------

layer(NodeContext.layer)('estate hygiene', (it) => {
    it.effect('no snapshot outlives its owning spec anywhere in the estate', () =>
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const audits = yield* Effect.forEach(_ESTATE, (home) => Snapshots.audit(path.join(_ROOT, home)));
            expect(Array.flatMap(audits, (audit) => audit.orphans)).toEqual([]);
        }),
    );

    it.effect('no colocated spec under the runtime branch exceeds the density cap', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const home = path.join(_ROOT, 'libs/typescript');
            const entries = yield* fs.readDirectory(home, { recursive: true });
            const over = yield* Effect.forEach(
                Array.filter(entries, (entry) => _SPEC_FILE.test(entry) && !_PRUNE.test(entry)),
                (entry) => Effect.map(fs.readFileString(path.join(home, entry)), (text) => _capped(entry, text)),
            );
            expect(Array.getSomes(over)).toEqual([]);
        }),
    );
});

describe('cap falsification', () => {
    vanilla('the cap verdict refuses an over-cap spec, passes at cap, and never counts blank padding', () => {
        expect(Option.isSome(_capped('over.spec.ts', Array.replicate('it();', _SPEC_CAP + 1).join('\n')))).toBe(true);
        expect(Option.isNone(_capped('dense.spec.ts', Array.replicate('it();', _SPEC_CAP).join('\n')))).toBe(true);
        expect(
            Option.isNone(_capped('padded.spec.ts', Array.join(Array.appendAll(Array.replicate('it();', 10), Array.replicate('   ', 400)), '\n'))),
        ).toBe(true);
    });
});
