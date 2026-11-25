import { layer as platform } from '@effect/platform-node/NodeServices';
import { Shell } from '@rasm/shell';
import { type Effect, Layer } from 'effect';
import { processor } from 'sh-syntax/sync';

// --- [TYPES] ---------------------------------------------------------------------------

type Probe = ReturnType<Shell['Service']['dialect']>;

// --- [COMPOSITION] ---------------------------------------------------------------------

const judging = Layer.provideMerge(Layer.effect(Shell, Shell.make({ processor })), platform);
const dialect: Effect.Effect<Effect.Success<Probe>, Effect.Error<Probe>, Shell> = Shell.use((shell) => shell.dialect({ shell: '/bin/bash', environment: { PATH: '/usr/bin:/bin' }, timeout: '1 minute' }));
const call = (command: string, index: number) => ({ tool: 'Bash', tool_use_id: `call-${index}`, command });

// --- [EXPORTS] -------------------------------------------------------------------------

export { call, dialect, judging };
