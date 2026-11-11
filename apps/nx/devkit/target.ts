import type { TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import type { Array } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Inputs = NonNullable<TargetConfiguration['inputs']>;
type Command = Array.NonEmptyReadonlyArray<string>;
interface Described {
    readonly description: string;
    readonly help: string;
    readonly example?: NonNullable<NonNullable<TargetConfiguration['metadata']>['help']>['example'];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const NODE = { runtime: 'node --version' } as const;
const SIGNALLED = { continuous: true, readyWhen: { signal: true } } as const satisfies Pick<TargetConfiguration, 'continuous' | 'readyWhen'>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const described = ({ description, help, example = {} }: Described): NonNullable<TargetConfiguration['metadata']> => ({ description, help: { command: help, example } });

const commanded = (commands: Array.NonEmptyReadonlyArray<Command>, cwd: string, metadata: Described, extra: Partial<TargetConfiguration> = {}): TargetConfiguration => {
    const [first, ...rest] = commands;
    return {
        executor: 'nx:run-commands',
        ...extra,
        options: { ...(rest.length === 0 ? { command: line(first) } : { commands: commands.map(line), parallel: false }), cwd, ...extra.options },
        metadata: described(metadata),
    };
};

const cleaned = ({ cleaner }: { readonly cleaner: string }, commands: readonly Command[], cwd: string, metadata: Described, previews?: readonly Command[]): TargetConfiguration => ({
    executor: cleaner,
    options: { commands, cwd, ...(previews === undefined ? {} : { previews }) },
    metadata: described(metadata),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Command, cleaned, commanded, described, type Inputs, line, NODE, SIGNALLED };
