import { Dry, dried, executor } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect } from 'effect';
import { located } from '#/host-packages';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Dry.annotate({ title: 'Install', description: "Installs what the project's build wrote in the run's configuration where its host loads it: each package built for this platform through the host's own installer, or the unpacked build folder for a host loading one" }),
    Effect.fnUntraced(function* (options, context, project) {
        const [dry, target] = yield* Effect.all([dried(options), Effect.fromNullishOr(context.targetName)]);
        const { outputs, packages, folders } = yield* located(context, target);
        const loaded = [...packages.filter(({ host, foreign }) => host.install?.unpacked !== true && !foreign), ...folders.filter(({ host }) => host.install?.unpacked === true)].flatMap((found) => Array.fromNullishOr(found.host.install).map((install) => ({ found, install })));
        yield* Array.match(loaded, {
            onEmpty: () => ToolError.make({ subject: project, detail: `build wrote nothing a host on this platform installs under ${outputs.join(', ')}` }),
            onNonEmpty: Effect.forEach(({ found, install }) => (dry ? Effect.logInfo(`Would install ${found.at} through ${install.command}`) : Effect.andThen(install.run(found), Effect.logInfo(`Installed ${found.at} through ${install.command}, a running host loads it at its next start`))), { discard: true }),
        });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
