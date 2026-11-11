import process from 'node:process';
import { Dry, dried, executor } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Path, UndefinedOr } from 'effect';
import { built } from '#/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROCESSES: Partial<Record<NodeJS.Platform, readonly [string, ...string[]]>> = { darwin: ['ps', '-Ao', 'comm='] };

// --- [MODELS] --------------------------------------------------------------------------

const Options = Dry.annotate({ title: 'Install', description: "Installs each host package the project's build wrote for this platform, in the configuration each build task ran in, through the host's own installer" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const execute = executor(
    Options,
    Effect.fnUntraced(function* (options, context) {
        const [path, dry] = yield* Effect.all([Path.Path, dried(options)]);
        const { project, outputs, configurations, packages } = yield* built(context, Option.none(), 'build');
        const local = packages.filter(({ host, platforms }) => platforms.length === 0 || UndefinedOr.match(host.platforms[process.platform], { onUndefined: () => false, onDefined: (os) => Array.isArrayNonEmpty(Array.intersection([os, `${os}-${process.arch}`], platforms)) }));
        const detail = [
            `build wrote no host package for ${process.platform}-${process.arch} under ${outputs.join(', ') || 'its outputs, which name no path'}`,
            ...(configurations.length === 0 ? [] : [`its targets declare the configurations ${configurations.join(', ')}, which nx install ${project} -c <configuration> passes down`]),
        ].join('; ');
        const loaders = yield* Array.match(local, {
            onEmpty: () => ToolError.make({ subject: project, detail }),
            onNonEmpty: Effect.forEach((bundle) => (dry ? Effect.as(Effect.logInfo(`Would run ${bundle.host.install?.command} for ${bundle.file}`), Option.none<string>()) : (bundle.host.install?.run(bundle) ?? Effect.succeedNone))),
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
        const running = yield* Option.match(listing, { onNone: () => Effect.succeed(Array.empty<string>()), onSome: ([command, ...args]) => Effect.map(printed(command, args), (listed) => listed.split('\n')) });
        const stale = applications.filter((application) => running.some((executable) => executable.startsWith(path.join(application, 'Contents', 'MacOS', path.sep))));
        yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => path.basename(application, '.app')).join(', ')} runs the build it loaded at start, restart it to load this one`);
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
