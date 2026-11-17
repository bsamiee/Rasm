import { Schema } from 'effect';
import { Decision, Request } from '#/rules/decision';

// --- [MODELS] --------------------------------------------------------------------------

const Routes = {
    decide: [Request, Decision],
    paused: [Schema.Struct({ since: Schema.Finite, first: Schema.Array(Schema.String) }), Schema.Struct({ holds: Schema.Array(Schema.String), notes: Schema.Array(Schema.String) })],
    ready: [Schema.Array(Schema.String), Schema.Array(Schema.Struct({ id: Schema.String, readiness: Schema.optionalKey(Schema.Number), url: Schema.optionalKey(Schema.String) }))],
} as const;
type Routes = { readonly [Route in keyof typeof Routes]: { readonly [At in 0 | 1]: (typeof Routes)[Route][At]['Encoded'] } };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Routes };
