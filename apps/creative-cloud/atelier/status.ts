import { Pid, State } from '@rasm/applications/contract';
import { Resource } from '@rasm/mcp/resource';
import { Unmatched } from '@rasm/search/error';
import { Array, Effect, Option, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';
import { Adobe } from '#/adobe';

// --- [COMPOSITION] ---------------------------------------------------------------------

const status = Effect.gen(function* () {
    const { instances } = yield* Adobe;
    const awaitsAnswer = (state: typeof State.Type) => Array.isReadonlyArrayNonEmpty(State.asking(state));
    const report = (state: typeof State.Type) => awaitsAnswer(state) || State.match(state, { running: ({ status: held, active }) => active || held === 'unresponsive', failed: () => true, exited: () => true });
    const template = yield* Resource.template`status/${McpSchema.param('pid', Pid)}`({
        name: 'status',
        schema: State,
        completion: { pid: () => Effect.map(SubscriptionRef.get(instances), Array.map(Struct.get('pid'))) },
        changes: SubscriptionRef.changes(instances),
        content: (_, pid) => SubscriptionRef.get(instances).pipe(Effect.flatMap(Unmatched.single('running instance', Option.some(String(pid)), (row) => [String(row.pid)])), Effect.map(Struct.get('state'))),
    });
    return {
        reported: (pid: number) => template(pid)({ report }),
        answering: (pid: number) => template(pid)({ report, refuse: awaitsAnswer }),
        launching: (pid: number) => template(pid)({ report, refuse: (state) => State.match(state, { running: ({ status: held }) => held === 'launching', failed: () => false, exited: () => false }) }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { status };
