// --- [IMPORTS] -------------------------------------------------------------------------

import { json, type Settled } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { Execute, type Job } from '@rasm/creative-cloud-server/frames';
import { Effect, Option, Result, Schema } from 'effect';
import type { Live } from '../enums.ts';
import { execute as prepare } from '../execute.ts';

// --- [HANDLER] -------------------------------------------------------------------------

const execute = (live: Live, job: Job): Effect.Effect<Settled> =>
    Schema.decodeUnknownEffect(Execute)(job.body).pipe(
        Effect.mapError((cause) => HostRejection.cases.malformedParams.make({ cause })),
        Effect.map((body) => prepare(live, body)),
        Effect.flatMap(({ autocorrections, run }) => Effect.map(Effect.result(Effect.flatMap(run, json)), (result) => ({ autocorrections: Option.some(autocorrections), result }))),
        Effect.catch((rejection) => Effect.succeed({ autocorrections: Option.none(), result: Result.fail(rejection) })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
