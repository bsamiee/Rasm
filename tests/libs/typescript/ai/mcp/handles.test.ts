import { assert, describe, it } from '@effect/vitest';
import { Handles } from '@rasm/mcp/handles';
import { Cause, Effect, Fiber, Queue, Schema, Stream } from 'effect';
import { McpProtocol } from 'effect/ai';
import { HttpApiClient } from 'effect/http-api';
import { KIND, lines, opened, TEXT } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL = McpProtocol.v2026_07_28;
const EVENTS = ['Preview opened window 3', 'Preview closed window 3'] as const;
const STANDING = { key: 'instance 41', waits: 'Preview window 3 asks to save', settles: 'Preview closed the save sheet' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const urls = HttpApiClient.urlBuilder(Handles);
const decoded =
    <S extends Schema.Top>(schema: S) =>
    (response: Response) =>
        Effect.flatMap(
            Effect.promise(() => response.text()),
            Schema.decodeUnknownEffect(Schema.fromJsonString(schema)),
        );
const followedLines = (response: Response, count: number) =>
    Stream.fromReadableStream({ evaluate: () => response.body ?? new ReadableStream(), onError: (cause) => new Cause.UnknownError(cause, 'follow body failed') }).pipe(
        Stream.decodeText,
        Stream.splitLines,
        Stream.mapEffect((line) => Schema.decodeEffect(Schema.fromJsonString(Handles.Event))(line)),
        Stream.take(count),
        Stream.runCollect,
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('handle routes a harness plugin reaches', () => {
    it.live(
        'streams each event a bound handle owns as the line its results lead with, and leaves an acknowledged event out of the next result while a later one stays',
        Effect.fnUntraced(function* () {
            const { probe, request, called } = yield* opened(PROTOCOL);
            const { handle, server } = yield* Effect.flatMap(request(urls.bind(), { method: 'POST' }), decoded(Handles.Binding));
            const followed = yield* request(urls.follow({ params: { handle } }));
            yield* Queue.offerAll(probe.shared, EVENTS);
            const streamed = yield* followedLines(followed, EVENTS.length);
            const acked = yield* request(urls.ack({ params: { handle }, query: { through: 1 } }), { method: 'DELETE' });
            const shown = lines(yield* called('test_simple_text', { handle }));
            assert.deepStrictEqual(
                { server, streamed, acked: acked.status, shown },
                {
                    server: 'test',
                    streamed: [
                        { _tag: 'Line', sequence: 1, line: '! #1 Preview opened window 3', kind: KIND },
                        { _tag: 'Line', sequence: 2, line: '! #2 Preview closed window 3', kind: KIND },
                    ],
                    acked: 204,
                    shown: ['! #2 Preview closed window 3', '', TEXT],
                },
            );
        }),
    );

    it.live(
        'leaves an event raised while a call of the handle runs to that call result alone, and streams the next event once the call returned',
        Effect.fnUntraced(function* () {
            const { probe, request, called } = yield* opened(PROTOCOL);
            const { handle } = yield* Effect.flatMap(request(urls.bind(), { method: 'POST' }), decoded(Handles.Binding));
            const followed = yield* request(urls.follow({ params: { handle } }));
            const running = yield* Effect.forkScoped(called('gated_write', { handle }));
            yield* Queue.take(probe.entered);
            yield* Queue.offer(probe.shared, EVENTS[0]);
            yield* probe.release.open;
            const returned = lines(yield* Fiber.join(running));
            yield* Queue.offer(probe.shared, EVENTS[1]);
            const streamed = yield* followedLines(followed, 1);
            assert.deepStrictEqual({ returned, streamed }, { returned: ['! #1 Preview opened window 3', '', 'released'], streamed: [{ _tag: 'Line', sequence: 2, line: '! #2 Preview closed window 3', kind: KIND }] });
        }),
    );

    it.live(
        'streams a waiting event with its bare line, then settles its key once a later event of the key settles it',
        Effect.fnUntraced(function* () {
            const { probe, request } = yield* opened(PROTOCOL);
            const { handle } = yield* Effect.flatMap(request(urls.bind(), { method: 'POST' }), decoded(Handles.Binding));
            const followed = yield* request(urls.follow({ params: { handle } }));
            yield* Queue.offerAll(probe.standing, [
                { text: STANDING.waits, standing: Handles.Standing.cases.Waiting.make({ key: STANDING.key }) },
                { text: STANDING.settles, standing: Handles.Standing.cases.Settled.make({ key: STANDING.key }) },
            ]);
            assert.deepStrictEqual(yield* followedLines(followed, 2), [
                { _tag: 'Waiting', sequence: 1, key: STANDING.key, line: STANDING.waits },
                { _tag: 'Settled', key: STANDING.key },
            ]);
        }),
    );
});
