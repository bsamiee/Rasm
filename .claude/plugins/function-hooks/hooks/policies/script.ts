import type { Command } from '../command.ts';
import { type Decision, refuse } from '../composition.ts';
import { invocations } from '../invocation.ts';
import { basename } from '../path.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _writes = (command: Command): readonly string[] =>
    invocations(command.words)
        .slice(-1)
        .flatMap(([head, ...rest]) => {
            const name = basename(head);
            return name === 'echo' || name === 'printf' || (name === 'cat' && rest.length === 0) ? command.writes.filter((path) => !path.startsWith('/dev/')) : [];
        });

const _reasons = (commands: readonly Command[]): readonly string[] => {
    const timer = "hyperfine -N -r <runs> '<command>' times commands";
    return commands.flatMap((command, index) => {
        const written = commands.slice(0, index).flatMap(_writes);
        return [
            ...[...command.reads, ...command.words].filter((path) => written.includes(path)).map((path) => `${path} written then read in one call`),
            ...(command.looped ? _writes(command).map((path) => `${path} appended in a loop`) : []),
            ...(invocations(command.words).some(([head]) => basename(head) === 'time') ? [`${command.words.join(' ')} times by hand, ${timer}`] : []),
            ...command.clocks.map((clock) => `${clock} times by hand, ${timer}`),
        ];
    });
};

// --- [POLICY] --------------------------------------------------------------------------

const scriptPolicy = (commands: readonly Command[]): (<E>(e: E) => Decision<E>) => refuse(_reasons(commands));

// --- [EXPORTS] -------------------------------------------------------------------------

export { scriptPolicy };
