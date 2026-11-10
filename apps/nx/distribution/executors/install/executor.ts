import process from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Config, Effect, Option, Path, Schema, UndefinedOr } from 'effect';
import { built, DistributionError, spawned } from '#/plugins/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROCESSES: Partial<Record<NodeJS.Platform, readonly [string, ...string[]]>> = { darwin: ['ps', '-Ao', 'comm='] };

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const installed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [path, { dryRun }, dry] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Options)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false))]);
    const { project, outputs, configurations, packages } = yield* built(context, Option.none(), 'build');
    const local = packages.filter(({ host, platforms }) => platforms.length === 0 || UndefinedOr.match(host.platforms[process.platform], { onUndefined: () => false, onDefined: (os) => Array.isArrayNonEmpty(Array.intersection([os, `${os}-${process.arch}`], platforms)) }));
    const detail = [
        `build wrote no host package for ${process.platform}-${process.arch} under ${outputs.join(', ') || 'its outputs, which name no path'}`,
        ...(configurations.length === 0 ? [] : [`its targets declare the configurations ${configurations.join(', ')}, which nx install ${project} -c <configuration> passes down`]),
    ].join('; ');
    const loaders = yield* Array.match(local, {
        onEmpty: () => DistributionError.make({ subject: project, detail }),
        onNonEmpty: Effect.forEach((bundle) => (dryRun === true || dry ? Effect.as(Effect.logInfo(`Would run ${bundle.host.install?.command} for ${bundle.file}`), Option.none<string>()) : (bundle.host.install?.run(bundle) ?? Effect.succeedNone))),
    });
    const applications = Array.dedupe(
        Array.getSomes(
            Array.getSomes(loaders)
                .map((executable) => executable.split(path.sep))
                .map((parts) =>
                    Option.map(
                        Array.findFirstIndex(parts, (part) => path.extname(part) === '.app'),
                        (index) => parts.slice(0, index + 1).join(path.sep),
                    ),
                ),
        ),
    );
    const listing = Option.filter(Option.fromUndefinedOr(PROCESSES[process.platform]), () => applications.length > 0);
    const running = yield* Option.match(listing, { onNone: () => Effect.succeed(Array.empty<string>()), onSome: ([command, ...args]) => Effect.map(spawned(command, args), (printed) => printed.split('\n')) });
    const stale = applications.filter((application) => running.some((executable) => executable.startsWith(path.join(application, 'Contents', 'MacOS', path.sep))));
    yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => path.basename(application, '.app')).join(', ')} runs the build it loaded at start, restart it to load this one`);
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) => installed(options, context).pipe(Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
