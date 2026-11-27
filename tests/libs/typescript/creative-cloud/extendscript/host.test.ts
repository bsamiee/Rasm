import { assert, describe, it } from '@effect/vitest';
import { Completion } from '@rasm/extendscript/completion';
import { assign, preserve, reply, stringify } from '@rasm/extendscript/host';
import { HostError } from '@rasm/object-model/model';
import { Unmatched } from '@rasm/search/unmatched';
import { Arbitrary, Cause, Effect, flow, Option, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SUBJECT = 'layer name';
const PREFERENCES = ['userInteractionLevel', 'measurementUnit', 'enableRedraw', 'locale'] as const;
const READ_ONLY: PropertyDescriptor = {
    enumerable: true,
    set: () => {
        throw new Error('Property is read-only');
    },
};

// --- [MODELS] --------------------------------------------------------------------------

const Scalar = Schema.Union([Schema.Finite, Schema.String, Schema.Boolean, Schema.Null]);
const Refusal = Schema.TaggedStruct('IllegalArgument', { message: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const answered = flow(Schema.decodeEffect(Schema.fromJsonString(Completion)), Effect.flatMap(Completion.decode(Schema.Json, SUBJECT)));
const raising = (error: unknown) => (): never => {
    throw error;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(stringify, () => {
    it.prop('writes every JSON value as printable ASCII that JSON.parse reads back as JSON.stringify writes it', { value: Arbitrary.map(Arbitrary.schema(Schema.Json), structuredClone) }, ({ value }) => {
        const [text, reference] = [stringify(value), JSON.stringify(value)];
        assert.match(text, /^[ -~]*$/u);
        assert.deepStrictEqual(JSON.parse(text), JSON.parse(reference));
    });

    it('writes a record holding a constructor key as that record', () => {
        assert.strictEqual(stringify({ constructor: 'Swatch', tint: 40 }), '{"constructor":"Swatch","tint":40}');
    });

    it('writes a non-finite number and an undefined item as the null JSON.stringify writes', () => {
        const value = [Number.NaN, Number.POSITIVE_INFINITY, undefined, { at: Number.NEGATIVE_INFINITY }];
        assert.strictEqual(stringify(value), JSON.stringify(value));
    });
});

describe(reply, () => {
    it.effect.each([
        {
            named: 'a line it threw on as Thrown at that line, its error number typed',
            error: { message: 'No such element', number: 1302, line: 3 },
            expected: { reason: { _tag: 'Thrown', line: Option.some(3) }, subject: `${SUBJECT}: No such element`, number: Option.some(1302) },
            message: `line 3: ${SUBJECT}: No such element (error 1302)`,
        },
        { named: 'a throw holding no line as Refused', error: { message: 'Target is locked' }, expected: { reason: { _tag: 'Refused' }, subject: SUBJECT, number: Option.none() }, message: `${SUBJECT} refused: Target is locked` },
    ] as const)('answers $named', ({ error, expected, message }) =>
        Effect.map(Effect.flip(answered(reply(raising(error)))), (failed) => {
            assert.instanceOf(failed, HostError);
            assert.deepStrictEqual([Struct.pick(failed, ['reason', 'subject', 'number']), failed.message], [expected, message]);
        }),
    );

    it.effect(
        'answers an argument the host refused as IllegalArgumentError with its message',
        Effect.fnUntraced(function* () {
            const failed = yield* Effect.flip(answered(reply(raising({ _tag: 'IllegalArgument', message: 'Expected Unit, got String' }))));
            assert.instanceOf(failed, Cause.IllegalArgumentError);
            assert.strictEqual(failed.message, 'Expected Unit, got String');
        }),
    );

    it.effect(
        'answers a failed lookup as Unmatched with its subject, query, and candidates',
        Effect.fnUntraced(function* () {
            const failed = yield* Effect.flip(answered(reply(raising({ _tag: 'Unmatched', subject: 'layer', query: '/layer[@name="Ink"]', candidates: ['Art', 'Guides'] }))));
            assert.instanceOf(failed, Unmatched);
            assert.deepStrictEqual(Struct.pick(failed, ['subject', 'query', 'candidates']), { subject: 'layer', query: Option.some('/layer[@name="Ink"]'), candidates: ['Art', 'Guides'] });
        }),
    );
});

describe(assign, () => {
    it.effect(
        'refuses once naming every key the host refused, into nested records too, and leaves every value as it held',
        Effect.fnUntraced(function* () {
            const held: Record<string, unknown> = Object.defineProperties({ kept: 0, nested: { depth: 1, width: 2 } }, { locked: READ_ONLY, frozen: READ_ONLY });
            const { cause } = yield* Effect.flip(Effect.try({ try: () => assign(held, { kept: 4, locked: 1, nested: { width: 3 }, frozen: 2 }), catch: (error) => new Cause.UnknownError(error) }));
            const { message } = yield* Schema.decodeUnknownEffect(Refusal)(cause);
            assert.include(message, 'locked');
            assert.include(message, 'frozen');
            assert.deepStrictEqual([held['kept'], held['nested']], [0, { depth: 1, width: 2 }]);
        }),
    );
});

describe(preserve, () => {
    it.prop(
        'holds any subset of preferences, the empty one included, while the body runs, then restores each to the value it held, after a throwing body too',
        { held: Schema.Struct(Struct.Record(PREFERENCES, Scalar)), values: Schema.Struct(Struct.Record(PREFERENCES, Schema.optionalKey(Scalar))) },
        ({ held, values }) => {
            const target = { ...held };
            assert.deepStrictEqual(
                preserve(target, values, () => ({ ...target })),
                { ...held, ...values },
            );
            assert.throws(() => preserve(target, values, raising(new Error('Body failed'))), 'Body failed');
            assert.deepStrictEqual(target, held);
        },
    );
});
