// --- [IMPORTS] -------------------------------------------------------------------------

import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { Execute } from '@rasm/creative-cloud-server/frames';
import { Effect, Option, Schema } from 'effect';
import { execute as run } from '../execute.ts';

// --- [HANDLER] -------------------------------------------------------------------------

const execute = (body: Schema.Json): Effect.Effect<Schema.Json, HostRejection> =>
    Schema.decodeUnknownEffect(Execute)(body).pipe(
        Effect.mapError((cause) => HostRejection.cases.malformedParams.make({ cause })),
        Effect.flatMap(run),
        Effect.flatMap((value) =>
            Effect.mapError(Schema.decodeUnknownEffect(Schema.Json)(Option.getOrNull(Option.fromNullishOr(value))), (cause) => HostRejection.cases.resultNotJson.make({ cause })),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
