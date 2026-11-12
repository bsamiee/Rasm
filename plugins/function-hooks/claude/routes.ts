import { ProcessInstance, State } from '@rasm/applications';
import { Schema, Struct } from 'effect';
import { Decision, Request } from '#/rules/decision';

// --- [MODELS] --------------------------------------------------------------------------

const Instance = Schema.Struct({ pid: Schema.Int, instance: ProcessInstance, bundle: Schema.String.check(Schema.isPattern(/^[\w.-]+$/u)) });
const Exited = Schema.Array(Instance.fields.instance);
const Watched = Instance.mapFields(Struct.assign({ ended: Schema.Boolean, status: Schema.optionalKey(State.Running.schema.fields.status), asks: Schema.Boolean, conditions: Schema.Array(Schema.String), notes: Schema.Array(Schema.String) }));

const Routes = { decide: [Request, Decision], observe: [Schema.String, Schema.Struct({ created: Schema.Array(Instance), reused: Schema.Array(Instance), exited: Exited })], quit: [Schema.Array(Instance), Exited], watched: [Schema.String, Schema.Array(Watched)] } as const;
type Routes = { readonly [Route in keyof typeof Routes]: { readonly [At in 0 | 1]: (typeof Routes)[Route][At]['Encoded'] } };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Instance, Routes };
