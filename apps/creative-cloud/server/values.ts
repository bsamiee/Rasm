// --- [IMPORTS] -------------------------------------------------------------------------

import { type Brand, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type HostId = 'illustrator' | 'photoshop' | 'indesign' | 'acrobat';
type JobId = Brand.Branded<string, 'JobId'>;
type TimeoutMs = Brand.Branded<number, 'TimeoutMs'>;
type AbsolutePath = Brand.Branded<string, 'AbsolutePath'>;
type PixelBudget = 'preview' | 'detail';
type Undo = 'single' | 'none';

interface NormalizedRegion {
    readonly left: number;
    readonly top: number;
    readonly width: number;
    readonly height: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TIMEOUT_DEFAULT = 30_000;
const PROBE_MS = 5000;
const DPI = { minimum: 48, maximum: 600 } as const;
const _UUID_VERSION = 4;

// --- [MODELS] --------------------------------------------------------------------------

const HostId: Schema.Codec<HostId> = Schema.Literals(['illustrator', 'photoshop', 'indesign', 'acrobat']);
const JobId: Schema.Codec<JobId, string> = Schema.String.pipe(Schema.check(Schema.isUUID(_UUID_VERSION)), Schema.brand('JobId'));
const TimeoutMs: Schema.Codec<TimeoutMs, number> = Schema.Int.pipe(Schema.check(Schema.isBetween({ minimum: 1, maximum: 300_000 })), Schema.brand('TimeoutMs'));
const PageIndex: Schema.Codec<number> = Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(0)));
const ArtboardIndex: Schema.Codec<number> = Schema.Int.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 99 })));
const Dpi: Schema.Codec<number> = Schema.Int.pipe(Schema.check(Schema.isBetween(DPI)));
const _Fraction = Schema.Number.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 1 })));
const NormalizedRegion: Schema.Codec<NormalizedRegion> = Schema.Struct({ left: _Fraction, top: _Fraction, width: _Fraction, height: _Fraction }).pipe(
    Schema.check(Schema.makeFilter((region: NormalizedRegion) => (region.left + region.width <= 1 && region.top + region.height <= 1 ? undefined : 'left + width and top + height stay within 1'))),
);
const PixelBudget: Schema.Codec<PixelBudget> = Schema.Literals(['preview', 'detail']);
const AbsolutePath: Schema.Codec<AbsolutePath, string> = Schema.String.pipe(Schema.check(Schema.isStartsWith('/')), Schema.brand('AbsolutePath'));
const Undo: Schema.Codec<Undo> = Schema.Literals(['single', 'none']);

// --- [EXPORTS] -------------------------------------------------------------------------

export { AbsolutePath, ArtboardIndex, DPI, Dpi, HostId, JobId, NormalizedRegion, PageIndex, PixelBudget, PROBE_MS, TIMEOUT_DEFAULT, TimeoutMs, Undo };
