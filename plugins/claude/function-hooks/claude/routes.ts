import { Schema } from 'effect';
import { Decision, Request } from '#/rules/decision';

// --- [MODELS] --------------------------------------------------------------------------

const Routes = {
    decide: [Request, Decision],
    tasks: [
        Schema.Struct({ files: Schema.Array(Schema.String) }),
        Schema.Array(Schema.Struct({ file: Schema.String, live: Schema.Boolean, stuck: Schema.optionalKey(Schema.Struct({ key: Schema.String, text: Schema.String })) })).annotate({
            description: 'Each background task output file in request order: whether a process still writes it, and the evidence it hangs or loops, keyed so one notice goes per kind',
        }),
    ],
} as const;
type Routes = { readonly [Route in keyof typeof Routes]: { readonly [At in 0 | 1]: (typeof Routes)[Route][At]['Encoded'] } };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Routes };
