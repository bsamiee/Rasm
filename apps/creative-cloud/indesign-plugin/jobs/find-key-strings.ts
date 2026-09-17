// --- [IMPORTS] -------------------------------------------------------------------------

import { app } from 'adobe:indesign';
import { type Handler, handler, thrown } from '@rasm/creative-cloud-server/client';
import { FindKeyStrings, Keys } from '@rasm/creative-cloud-server/indesign/jobs';
import { Effect } from 'effect';

// --- [HANDLER] -------------------------------------------------------------------------

const findKeyStrings: Handler = handler(FindKeyStrings, Keys, ({ text }) =>
    Effect.try({ try: () => ({ kind: 'keys' as const, keys: app.findKeyStrings(text), translated: app.translateKeyString(text) }), catch: thrown }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { findKeyStrings };
