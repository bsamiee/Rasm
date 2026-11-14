import type { ExecutorContext } from '@nx/devkit';
import { Completed } from '@rasm/process';
import { Effect, Schema, Struct, UndefinedOr } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';
import { Dry, dried, packaged, run } from '#/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.NonEmptyArray(Schema.String);
const Publish = Dry.mapFields(
    Struct.assign({
        publish: Command.annotate({ description: "The tool's publish argument list, followed by each file the target's dependency targets declare as outputs" }),
        preview: Schema.optionalKey(Command).annotate({ description: "The tool's own dry-run form, run in place of publish under a dry run, absent prints publish and runs nothing" }),
    }),
).annotate({ title: 'Publish', description: "Delivers the files a project's packaging targets wrote to its registry or store through its tool's own publish command; runs through nx release publish" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ publish, preview, ...stated }: typeof Publish.Type, context: ExecutorContext) {
    const [dry, files] = yield* Effect.all([dried(stated), packaged(context)]);
    yield* UndefinedOr.match(dry ? preview : publish, {
        onUndefined: () => Effect.logInfo(`Would run ${CommandLine.display([...publish, ...files])}`),
        onDefined: ([command, ...rest]) => Effect.asVoid(Completed.output(ChildProcess.make(command, [...rest, ...files], { cwd: context.root, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }))),
    });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
