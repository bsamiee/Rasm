import process from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, Option, UndefinedOr } from 'effect';
import { built, DistributionError } from '#/plugins/host-packages';

// --- [OPERATIONS] ----------------------------------------------------------------------

const installed = Effect.fnUntraced(function* (context: ExecutorContext) {
    const { project, outputs, configurations, packages } = yield* built(context, Option.none(), 'build');
    const local = packages.filter(({ host, platforms }) => platforms.length === 0 || UndefinedOr.match(host.platforms[process.platform], { onUndefined: () => false, onDefined: (os) => Array.isArrayNonEmpty(Array.intersection([os, `${os}-${process.arch}`], platforms)) }));
    const detail = [
        `build wrote no host package for ${process.platform}-${process.arch} under ${outputs.join(', ') || 'its outputs, which name no path'}`,
        ...(configurations.length === 0 ? [] : [`its targets declare the configurations ${configurations.join(', ')}, which nx install ${project} -c <configuration> passes down`]),
    ].join('; ');
    yield* local.length === 0 ? DistributionError.make({ subject: project, detail }) : Effect.forEach(local, (bundle) => bundle.host.install?.run(bundle) ?? Effect.void, { discard: true });
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (_options: unknown, context: ExecutorContext) => installed(context).pipe(Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
