import { Effect, Schema, Struct, UndefinedOr } from 'effect';
import { CommandLine } from 'effect/process';
import { Dry, dried, executor, packaged } from '#/executor';
import { ran } from '#/process';

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.NonEmptyArray(Schema.String);
const Publish = Dry.mapFields(
    Struct.assign({
        publish: Command.annotate({ description: "The tool's publish argument list, followed by each file the target's dependency targets declare as outputs" }),
        preview: Schema.optionalKey(Command).annotate({ description: "The tool's own dry-run form, run in place of publish under a dry run, absent prints publish and runs nothing" }),
    }),
).annotate({ title: 'Publish', description: "Delivers the files a project's packaging targets wrote to its registry or store through its tool's own publish command; runs through nx release publish" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ publish, preview, ...stated }, context) {
        const [dry, files] = yield* Effect.all([dried(stated), packaged(context)]);
        return yield* UndefinedOr.match(dry ? preview : publish, {
            onUndefined: () => Effect.as(Effect.logInfo(`Would run ${CommandLine.display([...publish, ...files])}`), true),
            onDefined: ([command, ...rest]) => Effect.map(ran(command, [...rest, ...files], { cwd: context.root, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code === 0),
        });
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
