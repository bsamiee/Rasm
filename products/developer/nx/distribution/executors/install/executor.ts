import { Array, Cause, Effect, Schema } from 'effect';
import { located } from '#/host-packages';
import { executor, Words } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Install = Schema.Struct({ __unparsed__: Words }).annotate({
    title: 'Install',
    description: "Installs what the project's build wrote in the run's configuration where its host loads it: each package built for this platform through the host's own installer, or the unpacked build folder for a host loading one",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Install, function* ({ __unparsed__: words = [] }, context) {
    const { project, outputs, packages, folders } = yield* located(context);
    const loaded = [...packages.filter(({ host, foreign }) => host.install?.unpacked !== true && !foreign), ...folders.filter(({ host }) => host.install?.unpacked === true)].flatMap((found) => Array.fromNullishOr(found.host.install).map((install) => ({ found, install })));
    yield* Array.match(loaded, {
        onEmpty: () => Effect.fail(new Cause.UnknownError(outputs, `${project}: build wrote nothing a host on this platform installs under ${outputs.join(', ')}`)),
        onNonEmpty: Effect.forEach(({ found, install }) => Effect.andThen(install.run(found, words), Effect.logInfo(`Installed ${found.at} through ${install.command}, a running host loads it at its next start`)), { discard: true }),
    });
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Install };
