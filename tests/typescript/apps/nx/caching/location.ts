import { assert } from '@effect/vitest';
import { hashed } from '@rasm/testing/nx/cli';
import { Effect, Record } from 'effect';
import type { Subject } from '#/inference/cases';
import { type Methods, placed, present, SLOW } from '#/inference/workspaces';

// --- [OPERATIONS] ----------------------------------------------------------------------

const location = (subject: Subject, it: Methods) =>
    it.effect.skipIf(!present(subject))(
        'gives every cacheable task the hash it has in a second checkout at another path',
        Effect.fnUntraced(function* () {
            const read = Effect.map(Effect.flatMap(placed(subject), hashed), (hashes) =>
                Record.map(
                    Record.filter(hashes, ({ cache }) => cache),
                    ({ hash }) => hash,
                ),
            );
            assert.deepStrictEqual(yield* read, yield* read, 'gradle/develocity-build-validation-scripts@f5a59c5 03-validate-local-build-caching-different-locations.sh, vercel/turborepo@af729ab crates/turborepo/tests/final_hash_contract.rs');
        }),
        SLOW,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { location };
