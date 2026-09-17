// --- [IMPORTS] -------------------------------------------------------------------------

import { evaluate, type Handler, handler } from '@rasm/creative-cloud-server/client';
import { Execute } from '@rasm/creative-cloud-server/frames';
import { Schema } from 'effect';

// --- [HANDLER] -------------------------------------------------------------------------

const execute: Handler = handler(Execute, Schema.Json, ({ code }) => evaluate(code));

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
