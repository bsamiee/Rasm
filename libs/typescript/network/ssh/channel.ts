import { type Array, Cause, Data, Deferred, Duration, Effect, Fiber, Function, Match, Option, Predicate, Queue, Record, Redacted, Ref, Result, Schema, Stream, TxRef } from 'effect';
import { Host as Address, type NetAddress } from 'effect/net';
import { type ChildProcess, CommandLine } from 'effect/process';
import { Socket } from 'effect/socket';
import { type Host, SshError } from '#/host';
import { Mux } from '#/mux';

// --- [TYPES] ---------------------------------------------------------------------------

type Field = Parameters<typeof Mux.pack>[1][number];
type Message = Data.TaggedEnum<{
    OpenConfirmation: { readonly remote: number; readonly window: number; readonly packet: number };
    OpenFailure: { readonly answer: string };
    WindowAdjust: { readonly bytes: number };
    Data: { readonly bytes: Uint8Array };
    ExtendedData: { readonly bytes: Uint8Array };
    Eof: object;
    Close: object;
    ExitStatus: { readonly code: number };
    ExitSignal: { readonly signal: string };
    Request: { readonly reply: boolean };
    Reply: { readonly granted: boolean };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const ssh = {
    message: { open: 90, openConfirmation: 91, openFailure: 92, windowAdjust: 93, data: 94, extendedData: 95, eof: 96, close: 97, request: 98, success: 99, failure: 100 },
    window: 64 * 32 * 1024,
    packet: 32 * 1024,
    origin: ['127.0.0.1', 65_535],
    signal: 'TERM',
} as const;
const encoder = new TextEncoder();
const decoder = new TextDecoder();

// --- [MODELS] --------------------------------------------------------------------------

const Message = Data.taggedEnum<Message>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const getRequest = (frame: Uint8Array) =>
    Result.gen(function* () {
        const name = yield* Mux.getString(frame, 6);
        const at = 10 + name.length;
        const reply = yield* Result.fromOption(Option.fromUndefinedOr(frame[at]), Function.constVoid);
        return yield* Match.value(decoder.decode(name)).pipe(
            Match.when('exit-status', () => Result.map(Mux.getU32(frame, at + 1), (code) => Message.ExitStatus({ code }))),
            Match.when('exit-signal', () => Result.map(Mux.getString(frame, at + 1), (signal) => Message.ExitSignal({ signal: decoder.decode(signal) }))),
            Match.orElse(() => Result.succeed(Message.Request({ reply: reply !== 0 }))),
        );
    });
const messages = new Map<number, (frame: Uint8Array) => Result.Result<Message, void>>([
    [ssh.message.openConfirmation, (frame) => Result.map(Result.all([Mux.getU32(frame, 6), Mux.getU32(frame, 10), Mux.getU32(frame, 14)]), ([remote, window, packet]) => Message.OpenConfirmation({ remote, window, packet }))],
    [ssh.message.openFailure, (frame) => Result.map(Result.all([Mux.getU32(frame, 6), Mux.getString(frame, 10)]), ([code, description]) => Message.OpenFailure({ answer: `${code} ${decoder.decode(description)}`.trim() }))],
    [ssh.message.windowAdjust, (frame) => Result.map(Mux.getU32(frame, 6), (bytes) => Message.WindowAdjust({ bytes }))],
    [ssh.message.data, (frame) => Result.map(Mux.getString(frame, 6), (bytes) => Message.Data({ bytes }))],
    [ssh.message.extendedData, (frame) => Result.map(Mux.getString(frame, 10), (bytes) => Message.ExtendedData({ bytes }))],
    [ssh.message.eof, () => Result.succeed(Message.Eof())],
    [ssh.message.close, () => Result.succeed(Message.Close())],
    [ssh.message.request, getRequest],
    [ssh.message.success, () => Result.succeed(Message.Reply({ granted: true }))],
    [ssh.message.failure, () => Result.succeed(Message.Reply({ granted: false }))],
]);
const decode = (frame: Uint8Array) =>
    Option.fromUndefinedOr(frame[1]).pipe(
        Option.flatMapNullishOr((type) => messages.get(type)),
        Result.fromOption(Function.constVoid),
        Result.flatMap((read) => read(frame)),
        Result.mapError(() => `message ${frame.subarray(0, 2).join(' ')} length ${frame.length}`),
    );
const open = Effect.fnUntraced(
    function* (control: string, host: Pick<Host, 'name' | 'timeout'>, kind: string, fields: readonly Field[], batches: readonly (readonly { readonly answer: string; readonly name: string; readonly fields: readonly Field[] }[])[]) {
        const subject = host.name;
        const lost = (cause: unknown) => SshError.of({ _tag: 'Unreachable', output: Option.none() }, subject)(cause);
        yield* Effect.annotateCurrentSpan('server.address', subject);
        const socket = yield* Mux.proxy(control, subject);
        const [pull, writer, opened, replies, data, extended, status, window, consumed] = yield* Effect.all([
            Socket.readerBytes(socket),
            socket.writer,
            Deferred.make<{ readonly remote: number; readonly packet: number }, SshError>(),
            Queue.unbounded<boolean, SshError>(),
            Queue.unbounded<Uint8Array, SshError | Cause.Done>(),
            Queue.unbounded<Uint8Array, SshError | Cause.Done>(),
            Deferred.make<number, SshError>(),
            TxRef.make<Result.Result<number, SshError>>(Result.succeed(0)),
            Ref.make(0),
        ]).pipe(Effect.mapError(lost));
        const send = (type: number, ...body: readonly Field[]) => Effect.mapError(writer.writeAll(Mux.pack([Uint8Array.of(0, type)], body)), lost);
        const settle = (error: SshError) => Effect.all([Deferred.fail(opened, error), Queue.fail(replies, error), Queue.fail(data, error), Queue.fail(extended, error), Deferred.fail(status, error), TxRef.set(window, Result.fail(error))], { discard: true });
        const ended = Effect.all([Queue.end(data), Queue.end(extended)], { discard: true });
        const dispatch = Message.$match({
            OpenConfirmation: ({ remote, window: granted, packet }) => Effect.andThen(TxRef.set(window, Result.succeed(granted)), Deferred.succeed(opened, { remote, packet })),
            OpenFailure: ({ answer }) => Deferred.fail(opened, SshError.of({ _tag: 'Refused', answer }, subject)()),
            WindowAdjust: ({ bytes }) => TxRef.update(window, (held) => Result.map(held, (left) => left + bytes)),
            Data: ({ bytes }) => Queue.offer(data, bytes),
            ExtendedData: ({ bytes }) => Queue.offer(extended, bytes),
            Eof: () => ended,
            Close: () => Effect.andThen(ended, settle(SshError.of({ _tag: 'Protocol', received: 'close' }, subject)())),
            ExitStatus: ({ code }) => Deferred.succeed(status, code),
            ExitSignal: ({ signal }) => Deferred.fail(status, SshError.of({ _tag: 'Signal', signal }, subject)()),
            Request: ({ reply }) =>
                Effect.when(
                    Effect.flatMap(Deferred.await(opened), ({ remote }) => send(ssh.message.failure, remote)),
                    Effect.succeed(reply),
                ),
            Reply: ({ granted }) => Queue.offer(replies, granted),
        });
        yield* Stream.unfold(new Uint8Array(), Mux.readFrame(pull, subject)).pipe(
            Stream.mapEffect((bytes) => Effect.fromResult(decode(bytes))),
            Stream.takeUntil(Message.$is('Close')),
            Stream.runForEach(dispatch),
            Effect.catch((error) =>
                Match.value(error).pipe(
                    Match.when(Predicate.isString, (received) => SshError.of({ _tag: 'Protocol', received }, subject)()),
                    Match.when(Socket.isSocketError, lost),
                    Match.orElse(Function.identity<SshError>),
                    settle,
                ),
            ),
            Effect.forkScoped,
        );
        yield* send(ssh.message.open, kind, 0, ssh.window, ssh.packet, ...fields);
        const { remote, packet } = yield* Deferred.await(opened);
        yield* Effect.forEach(
            batches,
            (batch) =>
                Effect.andThen(
                    Effect.forEach(batch, ({ name, fields: values }) => send(ssh.message.request, remote, name, true, ...values), { discard: true }),
                    Effect.forEach(batch, ({ answer }) => Effect.filterOrFail(Queue.take(replies), Function.identity, Function.constant(SshError.of({ _tag: 'Refused', answer }, subject)())), { discard: true }),
                ),
            { discard: true },
        );
        const credit = Effect.fnUntraced(function* (bytes: number) {
            const due = yield* Ref.modify(consumed, (held) => (held + bytes >= ssh.window / 2 ? [Option.some(held + bytes), 0] : [Option.none(), held + bytes]));
            yield* Option.match(due, { onNone: () => Effect.void, onSome: (granted) => Effect.ignore(send(ssh.message.windowAdjust, remote, granted)) });
        });
        const grant = (wanted: number) =>
            Effect.tx(
                Effect.gen(function* () {
                    const held = yield* Effect.fromResult(yield* TxRef.get(window));
                    const size = Math.min(held, wanted, packet);
                    yield* size === 0 ? Effect.txRetry : TxRef.set(window, Result.succeed(held - size));
                    return size;
                }),
            );
        const transmit = (bytes: Uint8Array): Effect.Effect<void, SshError> => (bytes.length === 0 ? Effect.void : Effect.flatMap(grant(bytes.length), (size) => Effect.andThen(send(ssh.message.data, remote, bytes.subarray(0, size)), transmit(bytes.subarray(size)))));
        return {
            transmit,
            status,
            eof: send(ssh.message.eof, remote),
            signal: send(ssh.message.request, remote, 'signal', false, ssh.signal),
            stdout: Stream.tap(Stream.fromQueue(data), ({ length }) => credit(length)),
            stderr: Stream.tap(Stream.fromQueue(extended), ({ length }) => credit(length)),
        };
    },
    (effect, _control, host) =>
        effect.pipe(
            Effect.timeout(Option.getOrElse(host.timeout, () => Duration.infinity)),
            Effect.catchTag('TimeoutError', (cause) => SshError.of({ _tag: 'Unreachable', output: Option.none() }, host.name)(cause)),
        ),
);
const Channel = {
    exec: Effect.fn('Channel.exec')(function* <E = never, R = never>(control: string, host: Pick<Host, 'name' | 'shell' | 'timeout'>, words: Array.NonEmptyReadonlyArray<CommandLine.Word>, options?: { readonly env?: ChildProcess.CommandOptions['env']; readonly stdin?: Stream.Stream<Uint8Array, E, R> }) {
        const line = yield* Effect.fromResult(CommandLine.join(words, { format: host.shell }));
        const env = Record.toEntries(options?.env ?? {}).flatMap(([name, value]) => (value === undefined ? [] : [{ answer: `env ${name}`, name: 'env', fields: [name, Predicate.isString(value) ? value : Redacted.value(value)] }]));
        const channel = yield* open(control, host, 'session', [], [env, [{ answer: 'exec', name: 'exec', fields: [line] }]]);
        yield* Effect.addFinalizer(() =>
            Effect.ignore(
                Effect.when(
                    channel.signal,
                    Effect.map(Deferred.isDone(channel.status), (done) => !done),
                ),
            ),
        );
        const feeding = yield* Effect.catchIf(Effect.andThen(Stream.runForEach(options?.stdin ?? Stream.empty, channel.transmit), channel.eof), Schema.is(SshError), () => Effect.void).pipe(Effect.forkScoped);
        return { stdout: channel.stdout, stderr: channel.stderr, exit: Effect.raceFirst(Deferred.await(channel.status), Effect.andThen(Fiber.join(feeding), Effect.never)) };
    }),
    dial: Effect.fn('Channel.dial')(function* (control: string, host: Pick<Host, 'name' | 'timeout'>, end: Address.HostPort | NetAddress.UnixPathAddress) {
        const [kind, ...fields] = Match.valueTags(end, {
            HostPort: ({ host: at, port }) => ['direct-tcpip', Address.formatHost(at), port, ...ssh.origin] as const,
            UnixPathAddress: ({ path }) => ['direct-streamlocal@openssh.com', path, ...ssh.origin] as const,
        });
        const channel = yield* open(control, host, kind, fields, []);
        const pull = yield* Stream.toPull(channel.stdout);
        const write = (chunk: Uint8Array | string | Socket.CloseEvent) => Effect.mapError(Socket.isCloseEvent(chunk) ? channel.eof : channel.transmit(Predicate.isString(chunk) ? encoder.encode(chunk) : chunk), (cause) => Socket.SocketError.make({ reason: Socket.SocketWriteError.make({ cause }) }));
        return Socket.make({
            reader: Effect.succeed({
                pull: Effect.mapError(pull, (error) => Socket.SocketError.make({ reason: Cause.isDone(error) ? Socket.SocketCloseError.make({ code: 1000 }) : Socket.SocketReadError.make({ cause: error }) })),
                upgrade: Socket.SocketUpgradeError.unsupported,
            }),
            writer: Effect.acquireRelease(Effect.succeed({ write, writeAll: (chunks: Array.NonEmptyReadonlyArray<Uint8Array | string>) => Effect.forEach(chunks, write, { discard: true }) }), () => Effect.ignore(channel.eof)),
        });
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Channel };
