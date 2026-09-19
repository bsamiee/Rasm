// --- [IMPORTS] -------------------------------------------------------------------------

import { app } from 'adobe:indesign';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Body, Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { Effect } from 'effect';
import { translate } from '../host.ts';

// --- [HANDLER] -------------------------------------------------------------------------

const findKeyStrings = ({ text }: Body<'findKeyStrings'>): Effect.Effect<Reply<'findKeyStrings'>, HostRejection> =>
    Effect.map(translate(text), (translated) => ({ kind: 'keys', keys: app.findKeyStrings(text), translated }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { findKeyStrings };
