import process from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Effect, Option } from 'effect';
import { built, DistributionError, installable } from '#/plugins/host-packages';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (_options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const { project, outputs, configurations, packages } = yield* built(context, Option.none());
            const local = packages.filter(installable);
            const detail = [
                `build wrote no host package for ${process.platform}-${process.arch} under ${outputs.join(', ') || 'its outputs, which name no path'}`,
                ...(configurations.length === 0 ? [] : [`its targets declare the configurations ${configurations.join(', ')}, which nx install ${project} -c <configuration> passes down`]),
            ].join('; ');
            yield* local.length === 0 ? DistributionError.make({ subject: project, detail }) : Effect.forEach(local, (held) => held.host.install.run(held), { discard: true });
            return { success: true };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
