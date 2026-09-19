// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, type Brand, Option, pipe, Schema, SchemaGetter, String, Struct } from 'effect';

// --- [TABLE] ---------------------------------------------------------------------------

const HOSTS = {
    illustrator: { id: 'illustrator', bundleId: 'com.adobe.illustrator', transport: 'osascript' },
    photoshop: { id: 'photoshop', bundleId: 'com.adobe.Photoshop', transport: 'socket', port: 39_217, quit: 'quit', uxp: { app: 'PS', data: { loadEvent: 'startup' } } },
    indesign: { id: 'indesign', bundleId: 'com.adobe.InDesign', transport: 'socket', port: 39_218, quit: 'quit saving ask', uxp: { app: 'ID' } },
    acrobat: { id: 'acrobat', bundleId: 'com.adobe.Acrobat.Pro', transport: 'osascript' },
} as const;

const LOOPBACK = { dialed: 'localhost', bound: '127.0.0.1' } as const;
const CHANNELS = { rgb: 255, percent: 100 } as const;

// --- [TYPES] ---------------------------------------------------------------------------

type HostId = (typeof HostId)['Type'];
type Row = (typeof HOSTS)[HostId];
type SocketHost = { [K in HostId]: (typeof HOSTS)[K]['transport'] extends 'socket' ? K : never }[HostId];
type JobId = Brand.Branded<string, 'JobId'>;
type AbsolutePath = Brand.Branded<string, 'AbsolutePath'>;
type TimeoutMs = Brand.Branded<number, 'TimeoutMs'>;
type Autocorrections = (typeof Autocorrections)['Type'];
type Closed<T extends string, L extends readonly T[]> = [T] extends [L[number]] ? L : never;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROBE_MS = 5000;
const TIMEOUT_MS = 30_000;
const TIMEOUT_CEILING_MS = 300_000;
const QUEUE_DEPTH = 8;
const VOLUMES = '/Volumes';
const ARTIFACTS = '.artifacts';
const _UUID_VERSION = 4;
const _rgb: Schema.Finite = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: CHANNELS.rgb }));
const _percent: Schema.Finite = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: CHANNELS.percent }));
const _white: Schema.Finite = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 })).annotate({ description: 'Native normalized Gray white fraction: 0 is black and 1 is white.' });
const SOCKETS: { readonly [K in SocketHost]: (typeof HOSTS)[K] } = Struct.pick(
    HOSTS,
    Array.filter(Struct.keys(HOSTS), (id) => HOSTS[id].transport === 'socket'),
);

// --- [MODELS] --------------------------------------------------------------------------

const Ink: Schema.Union<
    readonly [
        Schema.Struct<{ readonly model: Schema.Literal<'RGB'>; readonly values: Schema.Tuple<readonly [Schema.Number, Schema.Number, Schema.Number]> }>,
        Schema.Struct<{ readonly model: Schema.Literal<'CMYK'>; readonly values: Schema.Tuple<readonly [Schema.Number, Schema.Number, Schema.Number, Schema.Number]> }>,
        Schema.Struct<{ readonly model: Schema.Literal<'GRAY'>; readonly values: Schema.Tuple<readonly [Schema.Number]> }>,
        Schema.Struct<{ readonly model: Schema.Literal<'LAB'>; readonly values: Schema.Tuple<readonly [Schema.Number, Schema.Number, Schema.Number]> }>,
    ]
> = Schema.Union([
    Schema.Struct({ model: Schema.Literal('RGB'), values: Schema.Tuple([_rgb, _rgb, _rgb]) }),
    Schema.Struct({ model: Schema.Literal('CMYK'), values: Schema.Tuple([_percent, _percent, _percent, _percent]) }),
    Schema.Struct({ model: Schema.Literal('GRAY'), values: Schema.Tuple([_white]) }),
    Schema.Struct({ model: Schema.Literal('LAB'), values: Schema.Tuple([_percent, Schema.Finite, Schema.Finite]) }).annotate({
        description: 'CIELAB lightness percentage with finite opponent coordinates; native host limits apply when assigning colors.',
    }),
]);

const HostId: Schema.Literals<Array<keyof typeof HOSTS>> = Schema.Literals(Struct.keys(HOSTS));
const JobId: Schema.Codec<JobId, string> = Schema.String.pipe(Schema.check(Schema.isUUID(_UUID_VERSION)), Schema.brand('JobId'));
const AbsolutePath: Schema.Codec<AbsolutePath, string> = Schema.String.pipe(Schema.check(Schema.isStartsWith('/')), Schema.brand('AbsolutePath'));
const TimeoutMs: Schema.Codec<TimeoutMs, number> = Schema.Int.pipe(Schema.check(Schema.isBetween({ minimum: 1, maximum: TIMEOUT_CEILING_MS })), Schema.brand('TimeoutMs'));
const PageIndex: Schema.Int = Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(0)));
const OptionalString: Schema.OptionFromOptionalKey<Schema.String> = Schema.OptionFromOptionalKey(Schema.String);
const OptionalNumber: Schema.OptionFromOptionalKey<Schema.Number> = Schema.OptionFromOptionalKey(Schema.Number);
const OptionalInt: Schema.OptionFromOptionalKey<Schema.Int> = Schema.OptionFromOptionalKey(Schema.Int);
const OptionalPath: Schema.OptionFromOptionalKey<typeof AbsolutePath> = Schema.OptionFromOptionalKey(AbsolutePath);
const Autocorrections: Schema.OptionFromOptionalKey<Schema.$Array<Schema.String>> = Schema.OptionFromOptionalKey(Schema.Array(Schema.String));
const Undo: Schema.Literals<readonly ['single', 'none']> = Schema.Literals(['single', 'none']);
const DevicePath = (volume: string): Schema.decodeTo<typeof AbsolutePath, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(AbsolutePath, {
            decode: SchemaGetter.transform((independent: string) =>
                pipe(
                    Option.liftPredicate(independent, String.startsWith(`/${volume}/`)),
                    Option.map(String.slice(volume.length + 1)),
                    Option.getOrElse(() => `${VOLUMES}${independent}`),
                ),
            ),
            encode: SchemaGetter.transform((local: string) =>
                pipe(
                    Option.liftPredicate(local, String.startsWith(`${VOLUMES}/`)),
                    Option.map(String.slice(VOLUMES.length)),
                    Option.getOrElse(() => `/${volume}${local}`),
                ),
            ),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Closed, Row, SocketHost };
export {
    AbsolutePath,
    ARTIFACTS,
    Autocorrections,
    CHANNELS,
    DevicePath,
    HOSTS,
    HostId,
    Ink,
    JobId,
    LOOPBACK,
    OptionalInt,
    OptionalNumber,
    OptionalPath,
    OptionalString,
    PageIndex,
    PROBE_MS,
    QUEUE_DEPTH,
    SOCKETS,
    TIMEOUT_CEILING_MS,
    TIMEOUT_MS,
    TimeoutMs,
    Undo,
    VOLUMES,
};
