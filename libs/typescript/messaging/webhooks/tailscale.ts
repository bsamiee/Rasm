import { type Array, type Duration, type Redacted, Schema } from 'effect';
import { Scheme } from '#/scheme';
import { Webhook } from '#/webhook';

// --- [MODELS] --------------------------------------------------------------------------

const envelope = { timestamp: Schema.DateTimeUtcFromString, version: Schema.Int, tailnet: Schema.String, message: Schema.String };
const node = { nodeID: Schema.String, deviceName: Schema.String, managedBy: Schema.String, actor: Schema.String, url: Schema.String };

const documented = [
    Schema.Struct({ _tag: Schema.tagDefaultOmit('Node'), ...envelope, type: Schema.Literals(['nodeCreated', 'nodeNeedsApproval', 'nodeApproved', 'nodeDeleted']), data: Schema.Struct(node) }),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('KeyExpiry'), ...envelope, type: Schema.Literals(['nodeKeyExpiringInOneDay', 'nodeKeyExpired']), data: Schema.Struct({ ...node, expiration: Schema.DateTimeUtcFromString }) }),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('Policy'), ...envelope, type: Schema.Literals(['policyUpdate']), data: Schema.Struct({ newPolicy: Schema.String, oldPolicy: Schema.String, url: Schema.String, actor: Schema.String }) }),
    Schema.Struct({
        _tag: Schema.tagDefaultOmit('Role'),
        ...envelope,
        type: Schema.Literals(['userRoleUpdated']),
        data: Schema.Struct({ user: Schema.String, url: Schema.String, actor: Schema.String, oldRoles: Schema.Array(Schema.String), newRoles: Schema.Array(Schema.String) }),
    }),
] as const;
const Event = Schema.Union([...documented, Schema.Struct({ _tag: Schema.tagDefaultOmit('Other'), ...envelope, type: Schema.String, data: Schema.OptionFromOptionalNullOr(Schema.Record(Schema.String, Schema.Json)) })]).pipe(Schema.toTaggedUnion('_tag'));
const Batch = Schema.Array(Event);
const Kind = Schema.Literals(documented.flatMap((member) => member.fields.type.literals));

// --- [COMPOSITION] ---------------------------------------------------------------------

const TailscaleEvent = Object.assign(Event, {
    Batch,
    Kind,
    webhook: (secrets: Array.NonEmptyReadonlyArray<Redacted.Redacted>, tolerance: Duration.Input = '5 minutes') => Webhook({ scheme: Scheme.tailscale, payload: Batch, secrets, tolerance }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { TailscaleEvent };
