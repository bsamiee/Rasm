import { Array, Effect, Path, Schema, Struct, UndefinedOr } from 'effect';
import { CommandLine } from 'effect/process';
import { executor, Released, released, written } from '#/executor';
import { ran, ToolError } from '#/process';

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.NonEmptyArray(Schema.String);
const Publish = Released.mapFields(
    Struct.assign({
        publish: Command.annotate({ description: "The tool's publish argument list, followed by each file the target's dependency targets declare as outputs" }),
        preview: Schema.optionalKey(Command).annotate({ description: "The tool's own dry-run form, run in place of publish under a dry run, absent prints publish and runs nothing" }),
    }),
).annotate({
    title: 'Publish',
    description: "Publishes the files a project's packaging targets wrote through its tool's own publish command, skipping a project nx release left unversioned; runs through nx release publish",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ publish, preview, ...stated }, context, project) {
        const [path, { dry, unreleased }, target] = yield* Effect.all([Path.Path, released(stated, project), Effect.fromNullishOr(context.targetName)]);
        if (unreleased) {
            return true;
        }
        const { files, outputs } = yield* written(context, target);
        const packaged = yield* Array.match(files, {
            onEmpty: () => ToolError.make({ subject: project, detail: `no file sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first` }),
            onNonEmpty: (found) => Effect.succeed(Array.map(found, (file) => path.relative(context.root, file))),
        });
        return yield* UndefinedOr.match(dry ? preview : publish, {
            onUndefined: () => Effect.as(Effect.logInfo(`Would run ${CommandLine.display([...publish, ...packaged])}`), true),
            onDefined: ([command, ...rest]) => Effect.map(ran(command, [...rest, ...packaged], { cwd: context.root, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code === 0),
        });
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
