import { assert, it } from '@effect/vitest';
import { HostError } from '@rasm/object-model';
import { Uxp } from '@rasm/uxp/plugin';
import { Effect, Option } from 'effect';
import { vi } from 'vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

vi.mock(import('adobe:uxp'), () => ({ default: { host: {} }, entrypoints: {} }));
vi.mock(import('adobe:fs'), () => ({}));

it.effect('names the body line an error was built on when the body read its stack before throwing', () =>
    Effect.map(Effect.flip(Uxp.execute({ root: {}, type: () => 'Object', kinds: {} }, "(function body() {\n    const first = 1;\n    const error = new Error('early ' + first);\n    void error.stack;\n    throw error;\n})")), (failed) => {
        assert.instanceOf(failed, HostError);
        assert.deepStrictEqual([failed.reason, failed.subject], [{ _tag: 'Thrown', line: Option.some(3) }, 'early 1']);
    }),
);
