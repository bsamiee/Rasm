import { layer as platform } from '@effect/platform-node/NodeServices';
import type { Policy } from '@rasm/policy';
import { Shell } from '@rasm/shell';
import { Effect, Layer, Option } from 'effect';
import { processor } from 'sh-syntax/sync';

// --- [TYPES] ---------------------------------------------------------------------------

type Shelled = Extract<Parameters<Policy['judge']>[0], { readonly _tag: 'Shell' }>;
type Probe = ReturnType<Shell['Service']['dialect']>;

// --- [COMPOSITION] ---------------------------------------------------------------------

const judging = Layer.provideMerge(Layer.effect(Shell, Shell.make({ processor })), platform);
const where: Effect.Effect<{ readonly dialect: Effect.Success<Probe>; readonly cwd: string }, Effect.Error<Probe>, Shell> = Effect.map(
    Shell.use((shell) => shell.dialect({ shell: '/bin/bash', environment: { PATH: '/usr/bin:/bin' }, timeout: '1 minute' })),
    (dialect) => ({ dialect, cwd: import.meta.dirname }),
);
const shell = (command: string, wait: Shelled['wait'] = 'held', agent: Shelled['agent'] = Option.none()): Shelled => ({ _tag: 'Shell', call: command, agent, command, wait });

// --- [EXPORTS] -------------------------------------------------------------------------

export { judging, shell, where };
