import { assert, describe, it } from '@effect/vitest';
import { Capture, Event, InstanceState, LaunchEvent, Recording, Scope, Session } from '@rasm/applications/contract';
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

    it.effect('decodes the session with the Space of every display and the asking change with its default and cancel refs as the Swift server writes them', () =>
        Effect.gen(function* () {
            const session = yield* Schema.decodeEffect(Session)({ onConsole: true, locked: false, asleep: false, secureInput: true, spaces: [3, 9] });
            const asking = yield* Schema.decodeEffect(Event.fields.change)({
                _tag: 'asking',
                container: 4,
                kind: 'dialog',
                title: 'Save',
                text: ['Save changes?'],
                answers: [
                    { ref: 7, label: 'Save' },
                    { ref: 8, label: 'Cancel' },
                ],
                default: 7,
                cancel: 8,
            });
            assert.deepStrictEqual(
                { spaces: session.spaces, single: Result.isFailure(Schema.decodeUnknownResult(Session)({ onConsole: true, locked: false, asleep: false, secureInput: false, space: 3 })), asking },
                {
                    spaces: [3, 9],
                    single: true,
                    asking: {
                        _tag: 'asking',
                        container: 4,
                        kind: 'dialog',
                        title: 'Save',
                        text: ['Save changes?'],
                        answers: [
                            { ref: 7, label: 'Save' },
                            { ref: 8, label: 'Cancel' },
                        ],
                        default: 7,
                        cancel: 8,
                    },
                },
            );
        }),
    );

    it.effect('writes the scope and capture source the Swift server reads with their tag beside their fields', () =>
        Effect.gen(function* () {
            const scope = yield* Schema.encodeEffect(Scope)({ _tag: 'rows', ref: 8, from: 20, count: 10 });
            const source = yield* Schema.encodeEffect(Capture.fields.source)({ _tag: 'element', ref: 5 });
            assert.deepStrictEqual({ scope, source }, { scope: { _tag: 'rows', ref: 8, from: 20, count: 10 }, source: { _tag: 'element', ref: 5 } });
        }),
    );

    it.effect('leads each key frame with its time after the first and the pixel rectangle of its image that changed', () =>
        Effect.gen(function* () {
            const screenshot = {
                png: new Uint8Array(),
                width: 400,
                height: 240,
                frame: [
                    [300, 200],
                    [200, 120],
                ],
                windows: [7],
                backingStore: false,
            };
            const recording = yield* Schema.decodeUnknownEffect(Recording.Result)({
                keyFrames: [
                    { at: '2026-11-26T10:00:00.000Z', screenshot },
                    {
                        at: '2026-11-26T10:00:01.250Z',
                        changed: [
                            [350, 230],
                            [20, 10],
                        ],
                        screenshot,
                    },
                    { at: '2026-11-26T10:00:02.000Z', screenshot },
                ],
            });
            assert.deepStrictEqual(Recording.leads(recording), [['+0'], ['+1s 250ms', 'changed [100, 60, 40, 20]'], ['+2s']]);
        }),
    );
});
