import type { ExecutorContext } from '@nx/devkit';
import { Effect, Schema } from 'effect';
import { applied, Operation, run, StackRequest } from '#/executors/stacks';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({
    programs: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Program packages, each resolved from the project and exporting stacks(request)' }),
    operation: Operation,
}).annotate({ title: 'Install', description: "Applies the stacks a program package states for a project's install on this machine through the Pulumi Automation API, refreshing live state first" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ programs, ...selection }: typeof Install.Type, context: ExecutorContext) {
    return yield* applied(programs, StackRequest.Install({ context }), selection);
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Install };
