import { Pid, State } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { registerResourceTemplate } from '@rasm/mcp/resource';
import { Array, Effect, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';

// --- [COMPOSITION] ---------------------------------------------------------------------

const status = Effect.gen(function* () {
    const automation = yield* Automation;
    const answering = (state: typeof State.Type) => State.isAnyOf(['Responding'])(state) && state.elements.some(Struct.get('requiresAnswer'));
    const report = (state: typeof State.Type) => answering(state) || !State.isAnyOf(['Launching', 'Responding'])(state);
    const template = yield* registerResourceTemplate`status/${McpSchema.param('pid', Pid)}`({
        name: 'status',
        description: SchemaAST.resolveDescription(State.ast),
        mimeType: 'application/json',
        schema: State,
        completion: { pid: () => Effect.map(SubscriptionRef.get(automation.instances), Array.map(Struct.get('pid'))) },
        changes: SubscriptionRef.changes(automation.instances),
        content: (_, pid) =>
            SubscriptionRef.get(automation.instances).pipe(
                Effect.flatMap((rows) => Effect.fromOption(Array.findFirst(rows, (row) => row.pid === pid))),
                Effect.flatMap(({ state }) => (State.isAnyOf(['Exited'])(state) ? Effect.succeed(state) : automation.status({ pid }))),
            ),
    });
    return {
        reported: (pid: number) => template(pid)({ report }),
        dialog: (pid: number) => template(pid)({ report, refuse: answering }),
        launch: (pid: number) => template(pid)({ report, refuse: State.isAnyOf(['Launching']) }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { status };
