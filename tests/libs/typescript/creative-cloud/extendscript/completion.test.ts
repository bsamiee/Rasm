import { assert, describe, it } from '@effect/vitest';
import { Completion } from '@rasm/extendscript';
import { reply, written } from '@rasm/extendscript/host';
import { Unmatched } from '@rasm/search';
import { Effect, flow, Option, Schema, Struct } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const answered = flow(Schema.decodeEffect(Schema.fromJsonString(Completion)), Effect.flatMap(Completion.answered(Schema.String, 'layer name')));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Completion.answered', () => {
    it.effect(
        'succeeds with the value a host body returned',
        Effect.fnUntraced(function* () {
            assert.strictEqual(yield* answered(reply(() => 'Ink')), 'Ink');
        }),
    );

    it.effect(
        'fails Unmatched with the facts of a failed lookup a host wrote and a message naming them',
        Effect.fnUntraced(function* () {
            const failed = yield* Effect.flip(answered(written({ _tag: 'Unmatched', subject: 'layer', query: '/layer[@name="Ink"]', candidates: ['Art', 'Guides'] })));
            assert.instanceOf(failed, Unmatched);
            assert.deepStrictEqual([Struct.pick(failed, ['subject', 'query', 'candidates']), failed.message], [{ subject: 'layer', query: Option.some('/layer[@name="Ink"]'), candidates: ['Art', 'Guides'] }, 'No layer matching /layer[@name="Ink"], candidates: Art and Guides']);
        }),
    );
});
