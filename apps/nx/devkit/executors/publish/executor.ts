import { Array, Effect, Option, Path, Record, Schema, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { executor, Released, released } from '#/executor';
import { ran } from '#/process';

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.NonEmptyArray(Schema.String);
const Publish = Released.mapFields(
    Struct.assign({
        commands: Schema.Array(Command).annotate({ description: 'Argument lists run in order before the publish, under a dry run too (a pack, a build)', default: [] }),
        publish: Command.annotate({ description: "The tool's publish argument list" }),
        preview: Schema.optionalKey(Command).annotate({ description: "The tool's own dry-run form, run in place of publish under a dry run, absent prints publish and runs nothing" }),
        flags: Schema.Struct(Record.map(Struct.omit(Released.fields, ['dryRun', 'nxReleaseVersionData']), () => Schema.optionalKey(Schema.String))).annotate({
            description: "Option nx release publish passes to the tool's flag for it (registry to --source), a set option appends the flag and its value to publish and to preview, a true one the flag alone",
            default: {},
        }),
        args: Schema.Array(Schema.String).annotate({ description: 'Arguments appended to publish and to preview', default: [] }),
        cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
    }),
).annotate({ title: 'Publish', description: "Publishes a project through its tool's own publish command, skipping a project nx release left unversioned; runs through nx release publish" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ commands, publish, preview, flags, args, cwd, ...stated }, context, project) {
        const [path, { dry, unversioned }] = yield* Effect.all([Path.Path, released(stated, project)]);
        const passed = Struct.keys(flags).flatMap((option) =>
            Option.match(Option.all([Option.fromUndefinedOr(flags[option]), Option.filter(Option.fromUndefinedOr(stated[option]), (value) => value !== false)]), {
                onNone: () => [],
                onSome: ([flag, value]) => [flag, ...(value === true ? [] : [`${value}`])],
            }),
        );
        const run = (command: typeof Command.Type) => Effect.map(ran(Array.headNonEmpty(command), Array.tailNonEmpty(command), { cwd: path.join(context.root, cwd), stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code === 0);
        const whole = (command: typeof Command.Type) => [...command, ...passed, ...args] as const;
        const final = Option.orElse(
            Option.liftPredicate(whole(publish), () => !dry),
            () => Option.map(Option.fromUndefinedOr(preview), whole),
        );
        return unversioned
            ? yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true)
            : yield* Effect.reduce(
                  [...commands, ...Option.toArray(final)],
                  () => true,
                  (succeeded, command) => (succeeded ? run(command) : Effect.succeed(false)),
              ).pipe(Effect.tap(() => (Option.isNone(final) ? Effect.logInfo(`Would run ${CommandLine.display(whole(publish))}`) : Effect.void)));
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
