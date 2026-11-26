import { Description } from '@rasm/applications/source';
import { describe, Dom, operate } from '@rasm/indesign/uxp';
import { Uxp } from '@rasm/uxp/plugin';
import fs from 'adobe:fs';
import uxp from 'adobe:uxp';
import { Cause, Effect, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DECLARED = JSON.stringify({ classes: {}, enumerations: [] });

// --- [MODELS] --------------------------------------------------------------------------

const Path = Schema.toType(Description.Locator);
const Request = Schema.fromJsonString(Schema.Union([Schema.Struct({ describe: Schema.Struct({ within: Schema.optionalKey(Path) }) }), Schema.Struct({ operate: Schema.Struct({ locator: Path, to: Schema.optionalKey(Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])) }) })]));
const Answer = Schema.fromJsonString(Schema.Union([Schema.Struct({ described: Description }), Schema.Struct({ operated: Schema.Boolean }), Schema.Struct({ failed: Schema.String })]));

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = (text: string): string =>
    Effect.runSync(
        Effect.gen(function* () {
            const request = yield* Schema.decodeEffect(Request)(text);
            const answer = 'describe' in request ? { described: yield* describe(request.describe) } : { operated: yield* Effect.as(operate(request.operate), true) };
            return yield* Schema.encodeEffect(Answer)(answer);
        }).pipe(
            Effect.provideServiceEffect(Dom, Dom.make(DECLARED)),
            Effect.catchCause((cause) => Schema.encodeEffect(Answer)({ failed: Cause.pretty(cause) })),
            Effect.orDie,
            Effect.provide(Uxp.layer({ uxp, fs })),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
