// --- [IMPORTS] -------------------------------------------------------------------------

import { action } from 'adobe:photoshop';
import { type Handler, handler, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { BatchPlay, Descriptors } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Array, Effect, Option, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Failed = (typeof Descriptors)['Type']['failed'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _USER_CANCEL = -128;

// --- [MODELS] --------------------------------------------------------------------------

const _failure = Schema.decodeUnknownOption(Schema.Struct({ _obj: Schema.Literal('error'), message: Schema.String, result: Schema.Number }));

const _results = Schema.decodeUnknownEffect(Schema.Array(Schema.Json));

// --- [FAILURES] ------------------------------------------------------------------------

const _failed = (descriptor: unknown, index: number): Option.Option<Failed> => Option.map(_failure(descriptor), ({ result, message }) => ({ index, result, message }));

const _rejection = (failed: readonly Failed[], continueOnError: boolean): Option.Option<HostRejection> =>
    Array.some(failed, ({ result }) => result === _USER_CANCEL)
        ? Option.some(HostRejection.cases.userCancelled.make({}))
        : Option.map(
              Option.filter(Array.head(failed), () => !continueOnError),
              HostRejection.cases.descriptorFailed.make,
          );

// --- [HANDLER] -------------------------------------------------------------------------

const batchPlay: Handler = handler(BatchPlay, Descriptors, ({ descriptors, continueOnError, immediateRedraw }) =>
    Effect.gen(function* () {
        const results = yield* Effect.tryPromise({ try: () => action.batchPlay([...descriptors], { continueOnError, immediateRedraw }), catch: thrown });
        const failed = Array.getSomes(Array.map(results, _failed));
        yield* Option.match(_rejection(failed, continueOnError), { onNone: () => Effect.void, onSome: Effect.fail });
        const rendered = yield* Effect.mapError(_results(results), (cause) => HostRejection.cases.resultNotJson.make({ cause }));
        return { kind: 'descriptors' as const, results: rendered, failed };
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { batchPlay };
