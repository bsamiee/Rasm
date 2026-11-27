import { constants } from 'node:os';
import { extname, join, resolve } from 'node:path';
import process from 'node:process';
import { getSystemErrorMessage } from 'node:util';
import { Array, Cause, ConfigProvider, Context, DateTime, Duration, Effect, FileSystem, flow, Layer, Match, Option, Predicate, pipe, Record, Schema } from 'effect';
import { Host } from 'effect/net';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CF = { kCFStringEncodingUTF8: 0x08_00_01_00, kCFNumberDoubleType: 13, kCFURLPOSIXPathStyle: 0 } as const;
const IMAGE = {
    launch_activate_socket: { arguments: ['string', 'pointer', 'pointer'], return: 'i32' },
    free: { arguments: ['pointer'] },
    CFRelease: { arguments: ['pointer'] },
    CFGetTypeID: { arguments: ['pointer'], return: 'u64' },
    CFStringGetTypeID: { return: 'u64' },
    CFNumberGetTypeID: { return: 'u64' },
    CFBooleanGetTypeID: { return: 'u64' },
    CFDateGetTypeID: { return: 'u64' },
    CFDataGetTypeID: { return: 'u64' },
    CFArrayGetTypeID: { return: 'u64' },
    CFDictionaryGetTypeID: { return: 'u64' },
    CFStringCreateWithCString: { arguments: ['pointer', 'string', 'u32'], return: 'pointer' },
    CFStringCreateExternalRepresentation: { arguments: ['pointer', 'pointer', 'u32', 'u8'], return: 'pointer' },
    CFNumberGetValue: { arguments: ['pointer', 'i64', 'pointer'], return: 'bool' },
    CFBooleanGetValue: { arguments: ['pointer'], return: 'bool' },
    CFDateGetAbsoluteTime: { arguments: ['pointer'], return: 'f64' },
    CFDataCreate: { arguments: ['pointer', 'buffer', 'i64'], return: 'pointer' },
    CFDataGetBytePtr: { arguments: ['pointer'], return: 'pointer' },
    CFDataGetLength: { arguments: ['pointer'], return: 'i64' },
    CFArrayGetCount: { arguments: ['pointer'], return: 'i64' },
    CFArrayGetValueAtIndex: { arguments: ['pointer', 'i64'], return: 'pointer' },
    CFDictionaryGetCount: { arguments: ['pointer'], return: 'i64' },
    CFDictionaryGetKeysAndValues: { arguments: ['pointer', 'pointer', 'pointer'] },
    CFPropertyListCreateWithData: { arguments: ['pointer', 'pointer', 'u64', 'pointer', 'pointer'], return: 'pointer' },
    CFPreferencesCopyAppValue: { arguments: ['pointer', 'pointer'], return: 'pointer' },
    CFURLCreateFromFileSystemRepresentation: { arguments: ['pointer', 'buffer', 'i64', 'bool'], return: 'pointer' },
    CFURLCopyAbsoluteURL: { arguments: ['pointer'], return: 'pointer' },
    CFURLCopyFileSystemPath: { arguments: ['pointer', 'i64'], return: 'pointer' },
    CFBundleCreate: { arguments: ['pointer', 'pointer'], return: 'pointer' },
    CFBundleGetInfoDictionary: { arguments: ['pointer'], return: 'pointer' },
    CFBundleCopyExecutableURL: { arguments: ['pointer'], return: 'pointer' },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Info = Schema.Struct({ CFBundleIdentifier: Schema.String, CFBundleShortVersionString: Schema.String });
const Agent = Schema.Struct({
    BundleProgram: Schema.optionalKey(Schema.String),
    ExitTimeOut: Schema.optionalKey(Schema.Int),
    Sockets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ SockNodeName: Schema.String, SockServiceName: Schema.Int }))),
});

// --- [SERVICES] ------------------------------------------------------------------------

class Program extends Context.Service<Program>()('@rasm/current-process/program', {
    make: Effect.gen(function* () {
        const [ffi, files] = yield* Effect.all([Effect.promise(() => import('node:ffi')), FileSystem.FileSystem]);
        const { functions: cf, lib } = yield* Effect.sync(() => ffi.dlopen(null, IMAGE));
        const [epoch, decoder] = [ffi.getFloat64(lib.getSymbol('kCFAbsoluteTimeIntervalSince1970')), new TextDecoder()];
        const released =
            <A>(read: (ref: bigint) => A) =>
            (owned: bigint) =>
                Option.liftPredicate(owned, (ref) => ref !== 0n).pipe(
                    Option.map((ref) => {
                        const result = read(ref);
                        cf.CFRelease(ref);
                        return result;
                    }),
                );
        const created = (text: string) => cf.CFStringCreateWithCString(null, text, CF.kCFStringEncodingUTF8);
        const bytes = (data: bigint) => {
            const length = Number(cf.CFDataGetLength(data));
            return length === 0 ? new Uint8Array() : ffi.toBuffer(cf.CFDataGetBytePtr(data), length);
        };
        const text = (string: bigint) => {
            const encoded = cf.CFStringCreateExternalRepresentation(null, string, CF.kCFStringEncodingUTF8, 0);
            const decoded = decoder.decode(bytes(encoded));
            cf.CFRelease(encoded);
            return decoded;
        };
        const value = (ref: bigint): unknown => readers.get(cf.CFGetTypeID(ref))?.(ref);
        const readers: ReadonlyMap<bigint, (ref: bigint) => unknown> = new Map<bigint, (ref: bigint) => unknown>([
            [cf.CFStringGetTypeID(), text],
            [
                cf.CFNumberGetTypeID(),
                (number) => {
                    const held = new Float64Array(1);
                    cf.CFNumberGetValue(number, CF.kCFNumberDoubleType, held);
                    return ffi.getFloat64(ffi.getRawPointer(held));
                },
            ],
            [cf.CFBooleanGetTypeID(), (boolean) => cf.CFBooleanGetValue(boolean) === 1],
            [cf.CFDateGetTypeID(), (date) => Duration.seconds(cf.CFDateGetAbsoluteTime(date) + epoch).pipe(Duration.toMillis, DateTime.makeUnsafe)],
            [cf.CFDataGetTypeID(), bytes],
            [cf.CFArrayGetTypeID(), (array) => globalThis.Array.from({ length: Number(cf.CFArrayGetCount(array)) }, (_, index) => value(cf.CFArrayGetValueAtIndex(array, index)))],
            [
                cf.CFDictionaryGetTypeID(),
                (dictionary) => {
                    const count = Number(cf.CFDictionaryGetCount(dictionary));
                    const [keys, values] = [new BigUint64Array(count), new BigUint64Array(count)];
                    cf.CFDictionaryGetKeysAndValues(dictionary, keys, values);
                    return Object.fromEntries(Array.zipWith(keys, values, (key, entry) => [text(key), value(entry)] as const));
                },
            ],
        ]);
        const location = resolve(process.execPath, '..', '..', '..');
        const representation = new TextEncoder().encode(location);
        const bundle = yield* pipe(
            cf.CFURLCreateFromFileSystemRepresentation(null, representation, representation.byteLength, 1),
            released((url) => cf.CFBundleCreate(null, url)),
            Option.filter((opened) => opened !== 0n),
            Effect.fromOption(() => new Cause.NoSuchElementError(`${location} bundle`)),
        );
        const info = yield* Schema.decodeUnknownEffect(Info)(value(cf.CFBundleGetInfoDictionary(bundle)));
        const executable = yield* pipe(
            cf.CFBundleCopyExecutableURL(bundle),
            released(cf.CFURLCopyAbsoluteURL),
            Option.flatMap(released((url) => cf.CFURLCopyFileSystemPath(url, CF.kCFURLPOSIXPathStyle))),
            Option.flatMap(released(text)),
            Effect.fromOption(() => new Cause.NoSuchElementError(`${location} bundle executable`)),
        );
        const propertyList = (path: string) =>
            files.readFile(path).pipe(
                Effect.map((data) => cf.CFDataCreate(null, data, data.byteLength)),
                Effect.map(released((wrapped) => cf.CFPropertyListCreateWithData(null, wrapped, 0, null, null))),
                Effect.map(Option.flatMap(released(value))),
                Effect.flatMap(Effect.fromOption(() => new Cause.NoSuchElementError(`${path} property list`))),
            );
        const agents = join(location, 'Contents', 'Library', 'LaunchAgents');
        const agent = yield* files.readDirectory(agents).pipe(
            Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())),
            Effect.map(Array.filter((file) => extname(file) === '.plist')),
            Effect.flatMap(Effect.forEach((file) => Effect.flatMap(propertyList(join(agents, file)), Schema.decodeUnknownEffect(Agent)))),
            Effect.map(Array.findFirst(({ BundleProgram }) => BundleProgram !== undefined && join(location, BundleProgram) === process.execPath)),
        );
        const sockets = Option.flatMapNullishOr(agent, ({ Sockets }) => Sockets);
        const activate = (name: string) =>
            Effect.suspend(() => {
                const slots = new BigUint64Array(2);
                const answer = cf.launch_activate_socket(name, slots, slots.subarray(1));
                return Match.value(answer).pipe(
                    Match.when(0, () => {
                        const [listed, count] = [ffi.getUint64(ffi.getRawPointer(slots)), Number(ffi.getUint64(ffi.getRawPointer(slots), BigUint64Array.BYTES_PER_ELEMENT))];
                        const fds = Array.fromIterable(new Int32Array(ffi.toArrayBuffer(listed, count * Int32Array.BYTES_PER_ELEMENT)));
                        cf.free(listed);
                        return Effect.succeedSome(fds);
                    }),
                    Match.when(
                        (code) => code === constants.errno.ESRCH,
                        () => Effect.succeedNone,
                    ),
                    Match.orElse((code) => new Cause.UnknownError(code, `launch_activate_socket ${name}: ${getSystemErrorMessage(-code)}`)),
                );
            });
        const address = yield* sockets.pipe(
            Option.flatMap(flow(Record.values, Array.head)),
            Option.map(({ SockNodeName, SockServiceName }) => Effect.fromResult(Host.hostPortFromInput({ host: SockNodeName, port: SockServiceName }))),
            Effect.transposeOption,
        );
        const descriptors = yield* sockets.pipe(
            Option.map((declaring) => Effect.forEach(Record.keys(declaring), activate)),
            Effect.transposeOption,
            Effect.map(Option.flatMap((answers) => Option.map(Option.all(answers), Array.flatten))),
        );
        const share = agent.pipe(
            Option.flatMapNullishOr(({ ExitTimeOut }) => ExitTimeOut),
            Option.filter((seconds) => seconds > 0),
            Option.map((seconds) => Duration.seconds(seconds / 2)),
        );
        const domain = created(info.CFBundleIdentifier);
        const preferences = ConfigProvider.make((path) =>
            Effect.suspend(() => {
                const [keys, indices] = Array.span(path, Predicate.isString);
                const key = created(keys.join('.'));
                const found = released(value)(cf.CFPreferencesCopyAppValue(key, domain));
                cf.CFRelease(key);
                return found.pipe(Option.getOrUndefined, ConfigProvider.fromUnknown).load(indices);
            }),
        );
        return { identifier: info.CFBundleIdentifier, version: info.CFBundleShortVersionString, executable, address, descriptors, share, preferences };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Program };
