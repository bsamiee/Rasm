import { Array, Config, Effect, Option, Path, Record, Redacted, Schema, Struct } from 'effect';
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
        secrets: Schema.Record(Schema.String, Schema.String).annotate({ description: 'Flag to the environment variable holding its value, each set variable appends the pair to publish', default: {} }),
        registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry nx release publish passes as --registry, appended to publish and preview after registryFlag' }),
        registryFlag: Schema.optionalKey(Schema.String).annotate({ description: "The tool's flag taking a registry (--source, --index), absent ignores registry" }),
        args: Schema.Array(Schema.String).annotate({ description: 'Arguments appended to publish and to preview', default: [] }),
        cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
    }),
).annotate({ title: 'Publish', description: "Publishes a project through its tool's own publish command, skipping a project nx release left unversioned; runs through nx release publish" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ commands, publish, preview, secrets, registry, registryFlag, args, cwd, ...stated }, context, project) {
        const [path, { dry, unversioned }] = yield* Effect.all([Path.Path, released(stated, project)]);
        const held = yield* Effect.forEach(Record.toEntries(secrets), ([flag, variable]) =>
            Effect.map(
                Config.option(Config.Redacted(variable)),
                Option.map((secret) => [flag, Redacted.value(secret)]),
            ),
        );
        const run = (command: typeof Command.Type) => Effect.map(ran(Array.headNonEmpty(command), Array.tailNonEmpty(command), { cwd: path.join(context.root, cwd), stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code === 0);
        const final = Option.map(
            Option.orElse(
                Option.liftPredicate([...publish, ...Array.getSomes(held).flat()] as const, () => !dry),
                () => Option.fromUndefinedOr(preview),
            ),
            (command) => [...command, ...(registry === undefined || registryFlag === undefined ? [] : [registryFlag, registry]), ...args] as const,
        );
        return unversioned
            ? yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true)
            : yield* Effect.reduce(
                  [...commands, ...Option.toArray(final)],
                  () => true,
                  (succeeded, command) => (succeeded ? run(command) : Effect.succeed(false)),
              ).pipe(Effect.tap(() => (Option.isNone(final) ? Effect.logInfo(`Would run ${CommandLine.display(publish)}`) : Effect.void)));
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
