// --- [IMPORTS] -------------------------------------------------------------------------

import { type Settled, handler as settling } from '@rasm/creative-cloud-server/client';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Job } from '@rasm/creative-cloud-server/frames';
import { Effect, Option, type Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Handler = (job: Job) => Effect.Effect<Settled>;

// --- [HANDLER] -------------------------------------------------------------------------

const handler = <Body, Value>(
    body: Schema.Codec<Body, unknown, never, never>,
    value: Schema.Codec<Value, unknown, never, never>,
    work: (input: Body) => Effect.Effect<Value, HostRejection>,
): Handler => {
    const settle = settling(body, value, work);
    return (job): Effect.Effect<Settled> => Effect.map(Effect.result(settle(job.body)), (result) => ({ autocorrections: Option.none(), result }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Handler };
export { handler };
