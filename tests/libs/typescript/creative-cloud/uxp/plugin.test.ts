import { assert, describe, it } from '@effect/vitest';
import { Uxp } from '@rasm/uxp/plugin';
import { Effect, Option, Struct } from 'effect';
import { vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const COVER = { name: 'cover.psd' };
const HELD = { root: { documents: { length: 2, 0: COVER, 1: { name: 'spine.psd' } } }, type: () => 'Document', kinds: { document: { collection: 'documents', is: () => true } } };

// --- [OPERATIONS] ----------------------------------------------------------------------

const named = (value: string) => ({ steps: [{ separator: '/', kind: 'document', selector: { _tag: 'Key', key: 'name', value } }] }) as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

vi.mock(import('adobe:uxp'), () => ({ default: { host: {} }, entrypoints: {} }));
vi.mock(import('adobe:fs'), () => ({}));

describe('Uxp.located', () => {
    it.effect(
        'returns the object a document name selects',
        Effect.fnUntraced(function* () {
            assert.strictEqual(yield* Uxp.located(HELD, named('cover.psd')), COVER);
        }),
    );

    it.effect(
        'fails Unmatched with the kind, the specifier, and the held names for a document name no document holds',
        Effect.fnUntraced(function* () {
            const failed = yield* Effect.flip(Effect.asVoid(Uxp.located(HELD, named('back.psd'))));
            assert.deepStrictEqual(Struct.pick(failed, ['_tag', 'subject', 'query', 'candidates']), { _tag: 'Unmatched', subject: 'document', query: Option.some('/document[@name="back.psd"]'), candidates: ['cover.psd', 'spine.psd'] });
        }),
    );
});
