import { Buffer } from 'node:buffer';
import { constants } from 'node:os';
import process from 'node:process';
import { getSystemErrorMessage } from 'node:util';
import { Array, Context, DateTime, Duration, Effect, Layer, Match, Option, pipe, Record, Result, Schema, String, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CF = { kCFStringEncodingUTF8: 0x08_00_01_00, kCFNumberDoubleType: 13, kCFURLPOSIXPathStyle: 0 } as const;
const OS_LOG = { types: { default: 0x00, info: 0x01, debug: 0x02, error: 0x10, fault: 0x11 }, header: Uint8Array.of(0x02, 0x01, 0x22, 0x08), format: '\0%{public}s\0' } as const;
const IFMIB = {
    count: [4, 18, 0, 1, 1],
    data: [4, 18, 0, 2, 0, 1],
    index: 4,
    size: 180,
    flags: 20,
    running: 0x01 | 0x40,
    sendDrops: 32,
    counters: { receive: { bytes: 116, packets: 76, errors: 84, dropped: 148 }, transmit: { bytes: 124, packets: 92, errors: 100 } },
} as const;
const VM = { flavor: 4, size: 416, pages: { internal: 140, purgeable: 88, wired: 12, compressor: 128 }, events: { faults: 48, pageins: 32, pageouts: 40, swapins: 112, swapouts: 120 } } as const;
const PROC = { listFds: 1, fdInfo: 8, taskInfo: 4, taskSize: 96, task: { virtual: 0, switches: 80, threads: 84 } } as const;
const IMAGE = {
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
    launch_activate_socket: { arguments: ['string', 'pointer', 'pointer'], return: 'i32' },
    free: { arguments: ['pointer'] },
    os_log_create: { arguments: ['string', 'string'], return: 'pointer' },
    os_log_type_enabled: { arguments: ['pointer', 'u8'], return: 'bool' },
    _os_log_impl: { arguments: ['pointer', 'pointer', 'u8', 'pointer', 'buffer', 'u32'] },
    _dyld_image_count: { return: 'u32' },
    _dyld_get_image_header: { arguments: ['u32'], return: 'pointer' },
    getsectiondata: { arguments: ['pointer', 'string', 'string', 'buffer'], return: 'pointer' },
    sysctl: { arguments: ['pointer', 'u32', 'pointer', 'pointer', 'pointer', 'u64'], return: 'i32' },
    __error: { return: 'pointer' },
    mach_host_self: { return: 'u32' },
    host_statistics64: { arguments: ['u32', 'i32', 'buffer', 'buffer'], return: 'i32' },
    mach_error_string: { arguments: ['i32'], return: 'pointer' },
    proc_pidinfo: { arguments: ['i32', 'i32', 'u64', 'buffer', 'i32'], return: 'i32' },
} as const;

// --- [ERRORS] --------------------------------------------------------------------------

class DarwinError extends Schema.TaggedError<DarwinError, { readonly brand: unique symbol }>()('DarwinError', {
    reason: Schema.TaggedUnion({ Unavailable: {}, Refused: { code: Schema.Int, detail: Schema.String } }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: DarwinError['reason'], subject: string) => (cause?: unknown) => DarwinError.make({ reason, subject, ...(cause === undefined ? {} : { cause }), message: [subject, String.toLowerCase(reason._tag), ...Record.values(Struct.omit(reason, ['_tag']))].join(' ') });
}

// --- [SERVICES] ------------------------------------------------------------------------

class Darwin extends Context.Service<Darwin>()('@rasm/darwin/image/Darwin', {
    make: Effect.gen(function* () {
        const ffi = yield* Effect.tryPromise({ try: () => import('node:ffi'), catch: DarwinError.of({ _tag: 'Unavailable' }, 'node:ffi') });
        const { functions: native, lib } = yield* Effect.acquireDisposable(Effect.try({ try: () => ffi.dlopen(null, IMAGE), catch: DarwinError.of({ _tag: 'Unavailable' }, 'process image symbols') }));
        const { _dyld_image_count: imageCount, _dyld_get_image_header: imageHeader, _os_log_impl: logWrite } = native;
        const [epoch, page, host, decoder, encoder] = [ffi.getFloat64(lib.getSymbol('kCFAbsoluteTimeIntervalSince1970')), Number(ffi.getUint64(lib.getSymbol('vm_kernel_page_size'))), native.mach_host_self(), new TextDecoder(), new TextEncoder()];
        const refused = (subject: string, code = ffi.getInt32(native.__error())) => DarwinError.of({ _tag: 'Refused', code, detail: getSystemErrorMessage(-code) }, subject)();
        const released =
            <A>(read: (ref: bigint) => A) =>
            (owned: bigint) =>
                Option.liftPredicate(owned, (ref) => ref !== 0n).pipe(
                    Option.map((ref) => {
                        const result = read(ref);
                        native.CFRelease(ref);
                        return result;
                    }),
                );
        const created = (text: string) => native.CFStringCreateWithCString(null, text, CF.kCFStringEncodingUTF8);
        const bytes = (data: bigint) => {
            const length = Number(native.CFDataGetLength(data));
            return length === 0 ? new Uint8Array() : ffi.toBuffer(native.CFDataGetBytePtr(data), length);
        };
        const text = (string: bigint) => {
            const encoded = native.CFStringCreateExternalRepresentation(null, string, CF.kCFStringEncodingUTF8, 0);
            const decoded = decoder.decode(bytes(encoded));
            native.CFRelease(encoded);
            return decoded;
        };
        const value = (ref: bigint): unknown => readers.get(native.CFGetTypeID(ref))?.(ref);
        const readers: ReadonlyMap<bigint, (ref: bigint) => unknown> = new Map<bigint, (ref: bigint) => unknown>([
            [native.CFStringGetTypeID(), text],
            [
                native.CFNumberGetTypeID(),
                (number) => {
                    const held = new Float64Array(1);
                    native.CFNumberGetValue(number, CF.kCFNumberDoubleType, held);
                    return ffi.getFloat64(ffi.getRawPointer(held));
                },
            ],
            [native.CFBooleanGetTypeID(), (boolean) => native.CFBooleanGetValue(boolean) === 1],
            [native.CFDateGetTypeID(), (date) => Duration.seconds(native.CFDateGetAbsoluteTime(date) + epoch).pipe(Duration.toMillis, DateTime.makeUnsafe)],
            [native.CFDataGetTypeID(), bytes],
            [native.CFArrayGetTypeID(), (array) => globalThis.Array.from({ length: Number(native.CFArrayGetCount(array)) }, (_, index) => value(native.CFArrayGetValueAtIndex(array, index)))],
            [
                native.CFDictionaryGetTypeID(),
                (dictionary) => {
                    const count = Number(native.CFDictionaryGetCount(dictionary));
                    const [keys, values] = [new BigUint64Array(count), new BigUint64Array(count)];
                    native.CFDictionaryGetKeysAndValues(dictionary, keys, values);
                    return Object.fromEntries(Array.zipWith(keys, values, (key, entry) => [text(key), value(entry)] as const));
                },
            ],
        ]);
        const bundle = (path: string) =>
            Effect.sync(() => {
                const representation = encoder.encode(path);
                const opened = pipe(
                    native.CFURLCreateFromFileSystemRepresentation(null, representation, representation.byteLength, 1),
                    released((url) => native.CFBundleCreate(null, url)),
                    Option.flatMap(released((held) => ({ info: value(native.CFBundleGetInfoDictionary(held)), url: native.CFBundleCopyExecutableURL(held) }))),
                );
                const executable = opened.pipe(
                    Option.flatMap(({ url }) => released(native.CFURLCopyAbsoluteURL)(url)),
                    Option.flatMap(released((url) => native.CFURLCopyFileSystemPath(url, CF.kCFURLPOSIXPathStyle))),
                    Option.flatMap(released(text)),
                );
                return Option.all({ info: Option.map(opened, ({ info }) => info), executable });
            });
        const propertyList = (data: Uint8Array) =>
            Effect.sync(() =>
                pipe(
                    native.CFDataCreate(null, data, data.byteLength),
                    released((wrapped) => native.CFPropertyListCreateWithData(null, wrapped, 0, null, null)),
                    Option.flatMap(released(value)),
                ),
            );
        const preference = (domain: string, key: string) =>
            Effect.sync(() => {
                const [named, owner] = [created(key), created(domain)];
                const found = released(value)(native.CFPreferencesCopyAppValue(named, owner));
                native.CFRelease(named);
                native.CFRelease(owner);
                return found;
            });
        const activateSocket = (name: string) =>
            Effect.suspend(() => {
                const slots = new BigUint64Array(2);
                return Match.value(native.launch_activate_socket(name, slots, slots.subarray(1))).pipe(
                    Match.when(0, () => {
                        const [listed, count] = [ffi.getUint64(ffi.getRawPointer(slots)), Number(ffi.getUint64(ffi.getRawPointer(slots), BigUint64Array.BYTES_PER_ELEMENT))];
                        const fds = Array.fromIterable(new Int32Array(ffi.toArrayBuffer(listed, count * Int32Array.BYTES_PER_ELEMENT)));
                        native.free(listed);
                        return Effect.succeedSome(fds);
                    }),
                    Match.when(constants.errno.ESRCH, () => Effect.succeedNone),
                    Match.orElse((code) => refused(`launch_activate_socket ${name}`, code)),
                );
            });
        const unifiedLog = (subsystem: string) =>
            Effect.suspend(() => {
                const [length, literal] = [new BigUint64Array(1), Buffer.from(OS_LOG.format)];
                return Array.findFirst(Array.makeBy(imageCount(), imageHeader), (header) => {
                    const section = native.getsectiondata(header, '__TEXT', '__oslogstring', length);
                    const at = section === 0n ? -1 : ffi.toBuffer(section, Number(length[0]), false).indexOf(literal);
                    return at < 0 ? Option.none() : Option.some({ dso: header, format: section + BigInt(at + 1) });
                }).pipe(Effect.fromOption(DarwinError.of({ _tag: 'Unavailable' }, 'a %{public}s format in the __oslogstring section of a loaded image')));
            }).pipe(
                Effect.map(({ dso, format }) => {
                    const categories = new Map<string, bigint>();
                    return (category: string, type: keyof typeof OS_LOG.types, message: () => string) => {
                        const log = categories.getOrInsertComputed(category, (named) => native.os_log_create(subsystem, named));
                        if (native.os_log_type_enabled(log, OS_LOG.types[type]) === 0) {
                            return;
                        }
                        const argument = Buffer.alloc(OS_LOG.header.length + BigUint64Array.BYTES_PER_ELEMENT);
                        argument.set(OS_LOG.header);
                        argument.writeBigUInt64LE(ffi.getRawPointer(encoder.encode(`${message()}\0`)), OS_LOG.header.length);
                        logWrite(dso, log, OS_LOG.types[type], format, argument, argument.length);
                    };
                }),
            );
        const interfaces = Effect.suspend(() => {
            const [count, length, data, record] = [new Int32Array(1), new BigUint64Array(1), Int32Array.from(IFMIB.data), Buffer.alloc(IFMIB.size)];
            const sysctl = (name: Int32Array, into: Int32Array | Buffer) => {
                length[0] = BigInt(into.byteLength);
                return native.sysctl(name, name.length, into, length, null, 0n) === 0 ? 0 : ffi.getInt32(native.__error());
            };
            const listed = sysctl(Int32Array.from(IFMIB.count), count);
            return listed === 0
                ? Effect.filterMapEffect(Array.range(1, count[0] ?? 0), (index) => {
                      data[IFMIB.index] = index;
                      const code = sysctl(data, record);
                      const running = code === 0 && (record.readUInt32LE(IFMIB.flags) & IFMIB.running) === IFMIB.running;
                      return code === 0 || code === constants.errno.ENOENT
                          ? Effect.succeed(running ? Result.succeed({ name: record.toString('utf-8', 0, record.indexOf(0)), traffic: { receive: counts(record, IFMIB.counters.receive), transmit: { ...counts(record, IFMIB.counters.transmit), dropped: record.readUInt32LE(IFMIB.sendDrops) } } }) : Result.failVoid)
                          : refused(`sysctl IFMIB_IFDATA ${index}`, code);
                  })
                : refused('sysctl IFMIB_IFCOUNT', listed);
        });
        const memory = Effect.suspend(() => {
            const [statistics, count] = [Buffer.alloc(VM.size), Uint32Array.of(VM.size / Uint32Array.BYTES_PER_ELEMENT)];
            const code = native.host_statistics64(host, VM.flavor, statistics, count);
            const { internal, purgeable, wired, compressor } = Record.map(VM.pages, (offset) => statistics.readUInt32LE(offset) * page);
            return code === 0 ? Effect.succeed({ used: internal - purgeable + wired + compressor, ...counts(statistics, VM.events) }) : DarwinError.of({ _tag: 'Refused', code, detail: `${ffi.toString(native.mach_error_string(code))}` }, 'host_statistics64 HOST_VM_INFO64')();
        });
        const task = Effect.suspend(() => {
            const [info, descriptors] = [Buffer.alloc(PROC.taskSize), Buffer.alloc(Math.max(native.proc_pidinfo(process.pid, PROC.listFds, 0n, null, 0), 0))];
            const [read, held] = [native.proc_pidinfo(process.pid, PROC.taskInfo, 0n, info, info.length), native.proc_pidinfo(process.pid, PROC.listFds, 0n, descriptors, descriptors.length)];
            return read === info.length && held > 0 ? Effect.succeed({ virtual: Number(info.readBigUInt64LE(PROC.task.virtual)), switches: info.readInt32LE(PROC.task.switches), threads: info.readInt32LE(PROC.task.threads), descriptors: held / PROC.fdInfo }) : refused(`proc_pidinfo ${process.pid}`);
        });
        return { bundle, propertyList, preference, activateSocket, unifiedLog, interfaces, memory, task };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const counts = <K extends string>(buffer: Buffer, offsets: Readonly<Record<K, number>>) => Record.map(offsets, (offset) => Number(buffer.readBigUInt64LE(offset)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Darwin, DarwinError };
