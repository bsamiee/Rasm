// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Document } from 'adobe:indesign';
import { opened } from '@rasm/creative-cloud-server/client';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Item } from '@rasm/creative-cloud-server/indesign/jobs';
import { Effect, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Box = Item['bounds'];

// --- [READS] ---------------------------------------------------------------------------

const _bounds: (input: unknown) => readonly [number, number, number, number] = Schema.decodeUnknownSync(Schema.Tuple([Schema.Number, Schema.Number, Schema.Number, Schema.Number]));

const document: Effect.Effect<Document, HostRejection> = Effect.suspend(() => opened(app));

const box = (input: unknown): Box => {
    const [top, left, bottom, right] = _bounds(input);
    return { top, left, bottom, right };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Box };
export { box, document };
