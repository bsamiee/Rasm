import { assert, describe, it } from '@effect/vitest';
import { Capture, Recording } from '@rasm/applications/capture';
import { LaunchEvent } from '@rasm/applications/installations';
import { Event, InstanceState, Session } from '@rasm/applications/instances';
import { KeyStep, Scope } from '@rasm/applications/interface';
import { Diagnostics } from '@rasm/applications/processes';
import { Act } from '@rasm/applications/steps';
import { Effect, Result, Schema, Struct } from 'effect';

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

    it.effect('keeps decoding the events stream past a window change kind this build names no case for, with its spelling kept', () =>
        Effect.gen(function* () {
            const window = {
                number: 9,
                frame: [
                    [0, 0],
                    [400, 300],
                ],
                layer: 0,
                band: 'normal',
                spaces: [3],
                onScreen: true,
                hidden: false,
                shared: true,
                asking: false,
                presented: true,
            };
            const events = [
                { instance: '412@9', sequence: 1, at: '2026-11-26T12:31:00Z', change: { _tag: 'window', change: 'reordered', window } },
                { instance: '412@9', sequence: 2, at: '2026-11-26T12:31:01Z', change: { _tag: 'window', change: 'shown', window } },
            ];
            const decoded = yield* Schema.decodeUnknownEffect(Schema.Array(Event))(events);
            assert.deepStrictEqual<unknown>(decoded.map(Struct.get('change')), events.map(Struct.get('change')));
        }),
    );

    it('names every special key the Swift Key.Special decodes, the keypad clear, JIS, and f13 to f19 keys among them', () => {
        const named = Schema.is(KeyStep.fields.key.members[0]);
        assert.deepStrictEqual(
            ['clear', 'contextualMenu', 'eisu', 'kana', 'f13', 'f19', 'f20', 'Clear'].map((key) => [key, named(key)]),
            [
                ['clear', true],
                ['contextualMenu', true],
                ['eisu', true],
                ['kana', true],
                ['f13', true],
                ['f19', true],
                ['f20', false],
                ['Clear', false],
            ],
        );
    });

    it.effect('decodes the unified log event and act outcome as the Swift server writes them and refuses each missing a key it always writes', () =>
        Effect.gen(function* () {
            const entry = {
                category: 'Watch',
                eventMessage: 'Window server refused the capture',
                messageType: 'Fault',
                processID: 412,
                processImagePath: '/Applications/Preview.app/Contents/MacOS/Preview',
                senderImagePath: '/System/Library/Frameworks/AppKit.framework/Versions/C/AppKit',
                subsystem: 'com.apple.Preview',
                timestamp: '2026-11-26T10:00:00.250Z',
            };
            const delivery = { popUps: [{ closed: true, window: 9 }], proof: 'change', route: { _tag: 'window', number: 7 }, tried: [{ failure: 'value unchanged', rung: 'accessibility' }] };
            const { unified } = Diagnostics.Result.fields;
            const { outcomes } = Act.Result.fields;
            const decoded = yield* Schema.decodeUnknownEffect(unified)(Result.succeed([entry]));
            assert.deepStrictEqual(
                {
                    decoded: Result.getOrThrow(decoded),
                    outcome: yield* Schema.decodeUnknownEffect(outcomes)([Result.succeed(delivery)]),
                    unmessaged: Result.succeed([Struct.omit(entry, ['eventMessage'])]).pipe(Schema.decodeUnknownResult(unified), Result.isFailure),
                    noticed: Result.succeed([{ ...entry, messageType: 'Notice' }]).pipe(Schema.decodeUnknownResult(unified), Result.isFailure),
                    unlisted: Result.isFailure(Schema.decodeUnknownResult(outcomes)([Result.succeed(Struct.omit(delivery, ['popUps']))])),
                },
                { decoded: [entry], outcome: [Result.succeed(delivery)], unmessaged: true, noticed: true, unlisted: true },
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

    it.effect('leads each key frame with its time, the steps begun before it, and its change in pixels of the whole first frame, naming a crop and steps that changed nothing shown', () =>
        Effect.gen(function* () {
            const whole = {
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
            const changed = [
                [350, 230],
                [20, 10],
            ];
            const event = { _tag: 'event', instance: '412@9', sequence: 7, at: '2026-11-26T10:00:00.400Z', change: { _tag: 'focused', control: 3, kind: 'toggle' } };
            const recording = yield* Schema.decodeUnknownEffect(Recording.Result)({
                keyFrames: [
                    { at: '2026-11-26T10:00:00.000Z', screenshot: whole, markers: [{ _tag: 'step', index: 0, at: '2026-11-26T10:00:00.100Z' }, event] },
                    { at: '2026-11-26T10:00:01.250Z', changed, screenshot: { ...whole, width: 40, height: 20, frame: changed }, markers: [] },
                    { at: '2026-11-26T10:00:02.000Z', changed, screenshot: whole, markers: [{ _tag: 'step', index: 1, at: '2026-11-26T10:00:02.100Z' }] },
                ],
            });
            assert.deepStrictEqual(Recording.leads(recording), [['at the start'], ['1.2s in', 'after step 1', 'changed [100,60,40,20]'], ['2.0s in', 'changed [100,60,40,20]', 'then step 2, which changed nothing shown']]);
        }),
    );
});
