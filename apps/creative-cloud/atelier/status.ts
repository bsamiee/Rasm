import { Pid, State } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Resource } from '@rasm/mcp/resource';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';

// --- [COMPOSITION] ---------------------------------------------------------------------

const status = Effect.gen(function* () {
    const automation = yield* Automation;
    const awaitsAnswer = (state: typeof State.Type) => State.guards.Responding(state) && state.answering;
    const report = (state: typeof State.Type) => awaitsAnswer(state) || !State.isAnyOf(['Launching', 'Responding'])(state);
    const template = yield* Resource.template`status/${McpSchema.param('pid', Pid)}`({
        name: 'status',
        schema: State,
        completion: { pid: () => Effect.map(SubscriptionRef.get(automation.instances), Array.map(Struct.get('pid'))) },
        changes: SubscriptionRef.changes(automation.instances),
        content: (_, pid) =>
            SubscriptionRef.get(automation.instances).pipe(
                Effect.flatMap(Unmatched.single('running instance', Option.some(String(pid)), (row) => [String(row.pid)])),
                Effect.flatMap(({ state }) => (State.guards.Exited(state) ? Effect.succeed(state) : automation.status({ pid }))),
            ),
    });
    return {
        reported: (pid: number) => template(pid)({ report }),
        answering: (pid: number) => template(pid)({ report, refuse: awaitsAnswer }),
        launching: (pid: number) => template(pid)({ report, refuse: State.guards.Launching }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { status };
