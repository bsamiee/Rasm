import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, layer, it as vanilla } from '@effect/vitest';
import { Snapshots } from '@rasm/ts-testkit/gauges';
import { Array, Effect, Option, pipe } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = new URL('../../..', import.meta.url).pathname;

const _ESTATE = ['tests/typescript', 'libs/typescript', 'apps'] as const;

const _SPEC_CAP = 175;
const _SPEC_FILE = /\.(spec|test)\.(ts|tsx|mts|cts)$/;

const _PRUNE = /(^|\/)(node_modules|dist|coverage|\.git|\.planning|\.api)(\/|$)/;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _capped = (entry: string, text: string): Option.Option<string> =>
    pipe(
        Array.filter(text.split('\n'), (line) => line.trim().length > 0),
        (lines) => (lines.length > _SPEC_CAP ? Option.some(`${entry}: ${lines.length} LOC > ${_SPEC_CAP}`) : Option.none()),
    );

// --- [SPECS] ---------------------------------------------------------------------------

layer(NodeContext.layer)('estate hygiene', (it) => {
    it.effect('no snapshot outlives its owning spec anywhere in the estate', () =>
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const audits = yield* Effect.forEach(_ESTATE, (home) =>
                Effect.orElseSucceed(Snapshots.audit(path.join(_ROOT, home)), () => ({ scanned: 0, orphans: [] as ReadonlyArray<string> })),
            );
            expect(Array.flatMap(audits, (audit) => audit.orphans)).toEqual([]);
        }),
    );

    it.effect('no colocated spec under the runtime branch or an app tree exceeds the density cap', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const over = yield* Effect.forEach(['libs/typescript', 'apps'] as const, (tree) =>
                Effect.gen(function* () {
                    const home = path.join(_ROOT, tree);
                    const entries = yield* Effect.orElseSucceed(fs.readDirectory(home, { recursive: true }), () => [] as ReadonlyArray<string>);
                    return yield* Effect.forEach(
                        Array.filter(entries, (entry) => _SPEC_FILE.test(entry) && !_PRUNE.test(entry)),
                        (entry) => Effect.map(fs.readFileString(path.join(home, entry)), (text) => _capped(entry, text)),
                    );
                }),
            );
            expect(Array.getSomes(Array.flatten(over))).toEqual([]);
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
