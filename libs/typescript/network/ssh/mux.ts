import { makeNet } from '@effect/platform-node/NodeSocket';
import { Array, Bytes, Duration, Effect, Match, Number, Option, Predicate, Result, Schema } from 'effect';
import { Socket } from 'effect/socket';
import { Forward } from '#/forward';
import { SshError } from '#/host';

// --- [TYPES] ---------------------------------------------------------------------------

type Field = number | boolean | string | Uint8Array;

// --- [CONSTANTS] -----------------------------------------------------------------------

const mux = {
    version: 4,
    id: 0,
    limit: 256 * 1024,
    request: { hello: 0x00_00_00_01, alive: 0x10_00_00_04, terminate: 0x10_00_00_05, open: 0x10_00_00_06, close: 0x10_00_00_07, stop: 0x10_00_00_09, proxy: 0x10_00_00_0f, info: 0x20_00_00_01 },
} as const;
const encoder = new TextEncoder();
const decoder = new TextDecoder();

// --- [MODELS] --------------------------------------------------------------------------

const Reply = Schema.TaggedUnion({
    Ok: {},
    Refused: { answer: Schema.String },
    Alive: { pid: Schema.Int },
    RemotePort: { port: Schema.Int },
    ExtInfo: { message: Schema.String },
    Proxy: {},
    Closed: {},
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const putU32 = (value: number) => Uint8Array.of(value >>> 24, value >>> 16, value >>> 8, value);
const pack = (head: readonly Uint8Array[], fields: readonly Field[]) => {
    const body = [
        ...head,
        ...fields.flatMap((field) =>
            Match.value(Predicate.isString(field) ? encoder.encode(field) : field).pipe(
                Match.when(Predicate.isNumber, (value) => [putU32(value)]),
                Match.when(Predicate.isBoolean, (value) => [Uint8Array.of(value ? 1 : 0)]),
                Match.orElse((bytes) => [putU32(bytes.length), bytes]),
            ),
        ),
    ];
    return Array.prepend(body, putU32(Number.sumAll(body.map(({ length }) => length))));
};
const getU32 = (frame: Uint8Array, at: number): Result.Result<number, void> => (at + 4 <= frame.length ? Result.succeed(new DataView(frame.buffer, frame.byteOffset + at, 4).getUint32(0)) : Result.failVoid);
const getString = (frame: Uint8Array, at: number) => Result.flatMap(getU32(frame, at), (length) => (at + 4 + length <= frame.length ? Result.succeed(frame.subarray(at + 4, at + 4 + length)) : Result.failVoid));
const gather =
    (pull: Effect.Effect<Array.NonEmptyReadonlyArray<Uint8Array>, Socket.SocketError>, need: number) =>
    (held: readonly Uint8Array[]): Effect.Effect<readonly Uint8Array[], Socket.SocketError> =>
        Number.sumAll(held.map(({ length }) => length)) >= need ? Effect.succeed(held) : pull.pipe(Effect.flatMap((chunks) => gather(pull, need)([...held, ...chunks])));
const readFrame = (pull: Effect.Effect<Array.NonEmptyReadonlyArray<Uint8Array>, Socket.SocketError>, subject: string) =>
    Effect.fnUntraced(function* (held: Uint8Array) {
        const headed = Bytes.concat(yield* gather(pull, 4)([held]));
        const length = new DataView(headed.buffer, headed.byteOffset, 4).getUint32(0);
        yield* length > mux.limit ? SshError.of({ _tag: 'Protocol', received: `frame length ${length}` }, subject)() : Effect.void;
        const whole = Bytes.concat(yield* gather(pull, 4 + length)([headed]));
        return [whole.subarray(4, 4 + length), whole.subarray(4 + length)] as const;
    });
const refused = (frame: Uint8Array) => Result.map(getString(frame, 8), (answer) => Reply.cases.Refused.make({ answer: decoder.decode(answer) }));
const replies = new Map<number, (frame: Uint8Array) => Result.Result<typeof Reply.Type, void>>([
    [0x80_00_00_01, () => Result.succeed(Reply.cases.Ok.make({}))],
    [0x80_00_00_02, refused],
    [0x80_00_00_03, refused],
    [0x80_00_00_05, (frame) => Result.map(getU32(frame, 8), (pid) => Reply.cases.Alive.make({ pid }))],
    [0x80_00_00_07, (frame) => Result.map(getU32(frame, 8), (port) => Reply.cases.RemotePort.make({ port }))],
    [0x80_00_00_0f, () => Result.succeed(Reply.cases.Proxy.make({}))],
    [0x90_00_00_01, (frame) => Result.map(getString(frame, 12), (message) => Reply.cases.ExtInfo.make({ message: decoder.decode(message) }))],
]);
const greet = (frame: Uint8Array) =>
    Result.gen(function* () {
        const [type, version] = yield* Result.all([getU32(frame, 0), getU32(frame, 4)]).pipe(Result.mapError(() => `hello length ${frame.length}`));
        return yield* type === mux.request.hello && version === mux.version ? Result.void : Result.fail(`hello 0x${type.toString(16).padStart(8, '0')} version ${version}`);
    });
const decode = ([frame, rest]: readonly [Uint8Array, Uint8Array]) =>
    Result.gen(function* () {
        const [type, id] = yield* Result.all([getU32(frame, 0), getU32(frame, 4)]).pipe(Result.mapError(() => `reply length ${frame.length}`));
        const code = `reply 0x${type.toString(16).padStart(8, '0')}`;
        const read = yield* Option.fromUndefinedOr(replies.get(type)).pipe(Result.fromOption(() => code));
        yield* id === mux.id ? Result.void : Result.fail(`${code} id ${id}`);
        yield* rest.length === 0 ? Result.void : Result.fail(`${code} trailing ${rest.length}`);
        return yield* read(frame).pipe(Result.mapError(() => `${code} length ${frame.length}`));
    });
const connect = Effect.fnUntraced(
    function* (control: string, subject: string, _within: Duration.Input, type: number, ...body: readonly (number | string)[]) {
        const protocol = (received: string) => SshError.of({ _tag: 'Protocol', received }, subject)();
        yield* Effect.annotateCurrentSpan('server.address', subject);
        const socket = yield* makeNet({ path: control });
        const reader = yield* socket.reader;
        const pull = yield* Socket.readerBytes({ ...socket, reader: Effect.succeed(reader) });
        const writer = yield* socket.writer;
        const frame = readFrame(pull, subject);
        yield* writer.writeAll(pack([], [mux.request.hello, mux.version]));
        const [hello, held] = yield* frame(new Uint8Array());
        yield* greet(hello).pipe(Result.mapError(protocol), Effect.fromResult);
        yield* writer.writeAll(pack([], [type, mux.id, ...body]));
        const reply = yield* frame(held).pipe(
            Effect.flatMap((framed) => decode(framed).pipe(Result.mapError(protocol), Effect.fromResult)),
            Effect.catchReason('SocketError', 'SocketCloseError', () => Effect.succeed(Reply.cases.Closed.make({}))),
        );
        return { reply, reader, socket };
    },
    (effect, _control, subject, within) =>
        effect.pipe(
            Effect.timeout(within),
            Effect.catchTag(['SocketError', 'TimeoutError'], (cause) => SshError.of({ _tag: 'Unreachable', output: Option.none() }, subject)(cause)),
        ),
);
const reject = (subject: string) =>
    Reply.matchOrElse(
        {
            Refused: ({ answer }) => SshError.of({ _tag: 'Refused', answer }, subject)(),
            Closed: () => SshError.of({ _tag: 'Unreachable', output: Option.none() }, subject)(),
        },
        ({ _tag }) => SshError.of({ _tag: 'Protocol', received: `reply ${_tag}` }, subject)(),
    );
const Mux = {
    pack,
    getU32,
    getString,
    readFrame,
    alive: Effect.fn('Mux.alive')(
        function* (control: string, subject: string, within: Duration.Input = Duration.infinity) {
            const { reply } = yield* connect(control, subject, within, mux.request.alive);
            return yield* Reply.matchOrElse(reply, { Alive: ({ pid }) => Effect.succeedSome(pid) }, reject(subject));
        },
        Effect.scoped,
        Effect.catchReason('SshError', 'Unreachable', () => Effect.succeedNone),
    ),
    terminate: Effect.fn('Mux.terminate')(function* (control: string, subject: string, within: Duration.Input = Duration.infinity) {
        const { reply } = yield* connect(control, subject, within, mux.request.terminate);
        return yield* Reply.matchOrElse(reply, { Ok: () => Effect.void, Closed: () => Effect.void }, reject(subject));
    }, Effect.scoped),
    stop: Effect.fn('Mux.stop')(function* (control: string, subject: string, within: Duration.Input = Duration.infinity) {
        const { reply } = yield* connect(control, subject, within, mux.request.stop);
        return yield* Reply.matchOrElse(reply, { Ok: () => Effect.void }, reject(subject));
    }, Effect.scoped),
    forward: Effect.fn('Mux.forward')(function* (control: string, subject: string, forward: typeof Forward.Type, within: Duration.Input = Duration.infinity) {
        const { reply } = yield* connect(control, subject, within, mux.request.open, ...Forward.fields(forward));
        return yield* Reply.matchOrElse(reply, { Ok: () => Effect.succeed(forward), RemotePort: ({ port }) => Effect.succeed(Forward.bound(forward, port)) }, reject(subject));
    }, Effect.scoped),
    cancel: Effect.fn('Mux.cancel')(function* (control: string, subject: string, forward: typeof Forward.Type, within: Duration.Input = Duration.infinity) {
        const { reply } = yield* connect(control, subject, within, mux.request.close, ...Forward.fields(forward));
        return yield* Reply.matchOrElse(reply, { Ok: () => Effect.void }, reject(subject));
    }, Effect.scoped),
    info: Effect.fn('Mux.info')(function* (control: string, subject: string, name: 'connection' | 'channels', within: Duration.Input = Duration.infinity) {
        const { reply } = yield* connect(control, subject, within, mux.request.info, name);
        return yield* Reply.matchOrElse(reply, { ExtInfo: ({ message }) => Effect.succeed(message) }, reject(subject));
    }, Effect.scoped),
    proxy: Effect.fn('Mux.proxy')(function* (control: string, subject: string, within: Duration.Input = Duration.infinity) {
        const { reply, reader, socket } = yield* connect(control, subject, within, mux.request.proxy);
        yield* Reply.matchOrElse(reply, { Proxy: () => Effect.void }, reject(subject));
        return Socket.make({ reader: Effect.succeed(reader), writer: socket.writer });
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Mux };
