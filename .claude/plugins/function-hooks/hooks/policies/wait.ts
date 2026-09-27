import type { Command } from '../command.ts';
import { type Decision, refuse } from '../composition.ts';
import { type Invocation, invocations, operands, option } from '../invocation.ts';
import { basename } from '../path.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PREFIXED = /^[+-](?!-)/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _reasons = (command: Command): readonly string[] => {
    const waiters: Readonly<Record<string, (invocation: Invocation, wraps: boolean) => boolean>> = {
        sleep: () => true,
        pwait: () => true,
        wait: (invocation) => operands(invocation).inputs.length > 0,
        caffeinate: (invocation, wraps) => !wraps || operands(invocation).options.includes('-w'),
        tail: (invocation) => operands(invocation).options.includes('--pid'),
        lsof: ([program, ...args]) => args.some((word) => _PREFIXED.test(word) && option(program, `-${word.slice(1)}`)[0].includes('-r')),
    };
    const chain = invocations(command.words);
    const line = command.words.join(' ');
    return [
        ...(chain.some((invocation, index) => waiters[basename(invocation[0])]?.(invocation, index < chain.length - 1) === true) ? [`${line} waits`] : []),
        ...(command.polled ? [`${line} repeats until the loop condition changes`] : []),
    ];
};

// --- [POLICY] --------------------------------------------------------------------------

const waitPolicy = (commands: readonly Command[]): (<E>(e: E) => Decision<E>) =>
    refuse(commands.flatMap(_reasons), "act on the command's own exit, or run it with run_in_background and act on its completion notification");

// --- [EXPORTS] -------------------------------------------------------------------------

export { waitPolicy };
