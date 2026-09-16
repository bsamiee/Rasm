// --- [IMPORTS] -------------------------------------------------------------------------

import { type Brand, Schema, Struct } from 'effect';

// --- [TABLE] ---------------------------------------------------------------------------

const HOSTS = {
    illustrator: { id: 'illustrator', bundleId: 'com.adobe.illustratorBeta', processName: 'Adobe Illustrator', channel: 'osascript' },
    photoshop: { id: 'photoshop', bundleId: 'com.adobe.Photoshop', processName: 'Adobe Photoshop 2026', channel: 'socket', port: 39_217 },
    indesign: { id: 'indesign', bundleId: 'com.adobe.InDesign', processName: 'Adobe InDesign 2026 (Beta)', channel: 'socket', port: 39_218 },
    acrobat: { id: 'acrobat', bundleId: 'com.adobe.Acrobat.Pro', processName: 'AdobeAcrobat', channel: 'osascript' },
} as const;

// --- [TYPES] ---------------------------------------------------------------------------

type HostId = (typeof HostId)['Type'];
type Row = (typeof HOSTS)[HostId];
type JobId = Brand.Branded<string, 'JobId'>;
type AbsolutePath = Brand.Branded<string, 'AbsolutePath'>;
type Autocorrections = (typeof Autocorrections)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROBE_MS = 5000;
const QUEUE_DEPTH = 8;
const LOAD_CEILING = 99;
const _UUID_VERSION = 4;

// --- [MODELS] --------------------------------------------------------------------------

const HostId: Schema.Literals<Array<keyof typeof HOSTS>> = Schema.Literals(Struct.keys(HOSTS));
const JobId: Schema.Codec<JobId, string> = Schema.String.pipe(Schema.check(Schema.isUUID(_UUID_VERSION)), Schema.brand('JobId'));
const AbsolutePath: Schema.Codec<AbsolutePath, string> = Schema.String.pipe(Schema.check(Schema.isStartsWith('/')), Schema.brand('AbsolutePath'));
const Autocorrections: Schema.OptionFromOptionalKey<Schema.$Array<Schema.String>> = Schema.OptionFromOptionalKey(Schema.Array(Schema.String));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Row };
export { AbsolutePath, Autocorrections, HOSTS, HostId, JobId, LOAD_CEILING, PROBE_MS, QUEUE_DEPTH };
