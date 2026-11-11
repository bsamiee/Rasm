import { ProcessInstance, State } from '@rasm/applications';
import { Resource } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Effect, Function, Option, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';
import { Adobe } from '#/adobe';

// --- [COMPOSITION] ---------------------------------------------------------------------

const status = Effect.gen(function* () {
    const { instances } = yield* Adobe;
    const awaitsAnswer = (state: typeof State.Type) => Array.isReadonlyArrayNonEmpty(State.asking(state));
    const report = (state: typeof State.Type) => awaitsAnswer(state) || State.matchOrElse(state, { running: ({ running }) => running.active || running.status === 'unresponsive' }, Function.constTrue);
    const template = yield* Resource.template`status/${McpSchema.param('instance', ProcessInstance)}`({
        name: 'status',
        schema: State,
        completion: { instance: () => Effect.map(SubscriptionRef.get(instances), (rows) => rows.filter(({ state }) => !State.isAnyOf(['exited'])(state)).map(Struct.get('instance'))) },
        changes: SubscriptionRef.changes(instances),
        content: (_, instance) => SubscriptionRef.get(instances).pipe(Effect.flatMap(Unmatched.single('running instance', Option.some(instance), (row) => [row.instance])), Effect.map(Struct.get('state'))),
    });
    return {
        reported: (instance: typeof ProcessInstance.Type) => template(instance)({ report }),
        answering: (instance: typeof ProcessInstance.Type) => template(instance)({ report, refuse: awaitsAnswer }),
        launching: (instance: typeof ProcessInstance.Type) => template(instance)({ report, refuse: State.matchOrElse({ running: ({ running }) => running.status === 'launching' }, Function.constFalse) }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { status };
