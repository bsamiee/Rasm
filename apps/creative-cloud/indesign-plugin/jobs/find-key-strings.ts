// --- [IMPORTS] -------------------------------------------------------------------------

import { app } from 'adobe:indesign';
import type { Body, Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { Effect } from 'effect';

// --- [HANDLER] -------------------------------------------------------------------------

const findKeyStrings = ({ text }: Body<'findKeyStrings'>): Effect.Effect<Reply<'findKeyStrings'>> =>
    Effect.sync(() => ({ kind: 'keys', keys: app.findKeyStrings(text), translated: app.translateKeyString(text) }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { findKeyStrings };
