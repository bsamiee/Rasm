// --- [IMPORTS] -------------------------------------------------------------------------

import { type Brand, Schema, SchemaGetter, Struct } from 'effect';

// --- [TABLE] ---------------------------------------------------------------------------

const HOSTS = {
    illustrator: { id: 'illustrator', bundleId: 'com.adobe.illustratorBeta', channel: 'osascript' },
    photoshop: { id: 'photoshop', bundleId: 'com.adobe.Photoshop', channel: 'socket', port: 39_217, uxp: { app: 'PS', minVersion: '27.11', data: { loadEvent: 'startup' } } },
    indesign: { id: 'indesign', bundleId: 'com.adobe.InDesign', channel: 'socket', port: 39_218, uxp: { app: 'ID', minVersion: '21.6' } },
    acrobat: { id: 'acrobat', bundleId: 'com.adobe.Acrobat.Pro', channel: 'osascript' },
} as const;

// --- [TYPES] ---------------------------------------------------------------------------

type HostId = (typeof HostId)['Type'];
type Row = (typeof HOSTS)[HostId];
type JobId = Brand.Branded<string, 'JobId'>;
type AbsolutePath = Brand.Branded<string, 'AbsolutePath'>;
type TimeoutMs = Brand.Branded<number, 'TimeoutMs'>;
type Autocorrections = (typeof Autocorrections)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROBE_MS = 5000;
const TIMEOUT_MS = 30_000;
const QUEUE_DEPTH = 8;
const LOAD_CEILING = 99;
const _UUID_VERSION = 4;
const _TIMEOUT_CEILING_MS = 300_000;

// --- [MODELS] --------------------------------------------------------------------------

const HostId: Schema.Literals<Array<keyof typeof HOSTS>> = Schema.Literals(Struct.keys(HOSTS));
const JobId: Schema.Codec<JobId, string> = Schema.String.pipe(Schema.check(Schema.isUUID(_UUID_VERSION)), Schema.brand('JobId'));
const AbsolutePath: Schema.Codec<AbsolutePath, string> = Schema.String.pipe(Schema.check(Schema.isStartsWith('/')), Schema.brand('AbsolutePath'));
const TimeoutMs: Schema.Codec<TimeoutMs, number> = Schema.Int.pipe(Schema.check(Schema.isBetween({ minimum: 1, maximum: _TIMEOUT_CEILING_MS })), Schema.brand('TimeoutMs'));
const PageIndex: Schema.Int = Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(0)));
const Autocorrections: Schema.OptionFromOptionalKey<Schema.$Array<Schema.String>> = Schema.OptionFromOptionalKey(Schema.Array(Schema.String));
const Undo: Schema.Literals<readonly ['single', 'none']> = Schema.Literals(['single', 'none']);
const DevicePath = (volume: string): Schema.decodeTo<typeof AbsolutePath, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(AbsolutePath, {
            decode: SchemaGetter.transform((independent: string) => (independent.startsWith(`/${volume}/`) ? independent.slice(volume.length + 1) : `/Volumes${independent}`)),
            encode: SchemaGetter.transform((local: string) => (local.startsWith('/Volumes/') ? local.slice('/Volumes'.length) : `/${volume}${local}`)),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Row };
export { AbsolutePath, Autocorrections, DevicePath, HOSTS, HostId, JobId, LOAD_CEILING, PageIndex, PROBE_MS, QUEUE_DEPTH, TIMEOUT_MS, TimeoutMs, Undo };
