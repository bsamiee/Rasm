import { Pid, State } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { registerResourceTemplate } from '@rasm/mcp/resource';
import { Array, Effect, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema } from 'effect/ai';

// --- [COMPOSITION] ---------------------------------------------------------------------

const status = Effect.gen(function* () {
    const automation = yield* Automation;
    const answering = (state: typeof State.Type) => State.isAnyOf(['Responding'])(state) && state.elements.some(Struct.get('requiresAnswer'));
    const reporting = { report: (state: typeof State.Type) => answering(state) || !State.isAnyOf(['Launching', 'Responding'])(state) };
    const refusing = { ...reporting, refuse: answering };
    const template = yield* registerResourceTemplate`adobe://status/${McpSchema.param('pid', Pid)}`({
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
    return { reported: (pid: number) => template(pid)(reporting), refused: (pid: number) => template(pid)(refusing) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { status };
