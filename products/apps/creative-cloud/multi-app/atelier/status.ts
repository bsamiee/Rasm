import { ProcessInstance, State } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Resource } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Effect, flow, Option, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';

// --- [COMPOSITION] ---------------------------------------------------------------------

const status = Effect.gen(function* () {
    const { instances } = yield* ApplicationServer;
    const template = yield* Resource.template`status/${McpSchema.param('instance', ProcessInstance)}`({
        name: 'status',
        schema: State,
        completion: { instance: () => Effect.map(SubscriptionRef.get(instances), (rows) => rows.filter(({ state }) => !State.isAnyOf(['exited'])(state)).map(Struct.get('instance'))) },
        changes: SubscriptionRef.changes(instances),
        content: (_, instance) => SubscriptionRef.get(instances).pipe(Effect.flatMap(Unmatched.single('running instance', Option.some(instance), (row) => [row.instance])), Effect.map(Struct.get('state'))),
    });
    return {
        reported: (instance: typeof ProcessInstance.Type) => template(instance)({ report: State.reports }),
        answering: (instance: typeof ProcessInstance.Type) => template(instance)({ report: State.reports, refuse: flow(State.asking, Array.isReadonlyArrayNonEmpty) }),
        launching: (instance: typeof ProcessInstance.Type) => template(instance)({ report: State.reports, refuse: State.launching }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { status };
