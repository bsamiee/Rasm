import { assert, describe, it } from '@effect/vitest';
import { Capture, Event, InstanceState, LaunchEvent, Scope } from '@rasm/applications/contract';
import { Effect, Result, Schema } from 'effect';

describe('contract', () => {
    it.effect('decodes the tagged change, state, and launch event the Swift server writes', () =>
        Effect.gen(function* () {
            const event = yield* Schema.decodeEffect(Event)({ instance: '412@9', sequence: 7, at: '2026-11-26T10:00:00Z', change: { _tag: 'value', control: 3, kind: 'toggle', fact: { _tag: 'list', values: [{ _tag: 'integer', value: 1 }] } } });
            const state = yield* Schema.decodeEffect(InstanceState)({ _tag: 'exited', termination: { _tag: 'signal', signal: 9, reporting: true } });
            const launch = yield* Schema.decodeEffect(LaunchEvent)({ _tag: 'opened', reply: 'done' });
            assert.deepStrictEqual(
                { change: event.change, state, launch },
                { change: { _tag: 'value', control: 3, kind: 'toggle', fact: { _tag: 'list', values: [{ _tag: 'integer', value: 1 }] } }, state: { _tag: 'exited', termination: { _tag: 'signal', signal: 9, reporting: true } }, launch: { _tag: 'opened', reply: 'done' } },
            );
        }),
    );

    it('refuses a change, fact, or state keyed by its case with no tag', () => {
        assert.deepStrictEqual(
            {
                change: Result.isFailure(Schema.decodeUnknownResult(Event.fields.change)({ frontmost: { active: true } })),
                fact: Result.isFailure(Schema.decodeUnknownResult(Event.fields.change)({ _tag: 'value', control: 3, kind: 'toggle', fact: { integer: { value: 1 } } })),
                state: Result.isFailure(Schema.decodeUnknownResult(InstanceState)({ exited: { status: 0 } })),
            },
            { change: true, fact: true, state: true },
        );
    });

    it.effect('writes the scope and capture source the Swift server reads with their tag beside their fields', () =>
        Effect.gen(function* () {
            const scope = yield* Schema.encodeEffect(Scope)({ _tag: 'rows', ref: 8, from: 20, count: 10 });
            const source = yield* Schema.encodeEffect(Capture.fields.source)({ _tag: 'element', ref: 5 });
            assert.deepStrictEqual({ scope, source }, { scope: { _tag: 'rows', ref: 8, from: 20, count: 10 }, source: { _tag: 'element', ref: 5 } });
        }),
    );
});
