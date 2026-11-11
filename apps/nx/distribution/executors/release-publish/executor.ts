import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, UndefinedOr } from 'effect';
import { built } from '#/host-packages';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Released.annotate({ title: 'Release publish', description: "Attaches each host package the project's build wrote to its GitHub release and publishes it to its host's package server, skipping distributions the server holds and a project nx release left unversioned" }),
    Effect.fnUntraced(function* (stated, context, project) {
        const { dry, version, unversioned, attached } = yield* released(stated, context, project);
        const configuration = Option.fromUndefinedOr(context.configurationName);
        const { outputs, packages } = yield* built(context, configuration, 'build');
        if (unversioned) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true);
        }
        const rebuild = Option.match(configuration, { onNone: () => `nx build ${project}`, onSome: (name) => `nx build ${project} -c ${name}` });
        const refused = [
            ...(packages.length === 0 ? [`no host package sits under ${Array.match(outputs, { onEmpty: () => 'its build outputs, which name no path', onNonEmpty: (paths) => paths.join(', ') })}; run ${rebuild} first`] : []),
            ...packages.filter((bundle) => Option.exists(version, (set) => bundle.version !== set)).map((bundle) => `${bundle.file} is at ${bundle.version}, not ${Option.getOrElse(version, () => bundle.version)} that nx release set; run ${rebuild} after versioning`),
        ];
        const settings = { dryRun: dry, registry: Option.fromUndefinedOr(stated.registry) };
        yield* Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => ToolError.make({ subject: project, detail: reasons.join('; ') }) });
        yield* Effect.forEach(packages, (bundle) => Effect.andThen(attached(bundle.version, [bundle.file]), UndefinedOr.match(bundle.host.publish, { onUndefined: () => Effect.void, onDefined: (channel) => channel.run(bundle, settings) })), { discard: true });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
