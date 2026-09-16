// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, pass } from '../composition.ts';
import { basename, type Command, pastAssignments, strip } from '../text/command.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _TIMER = "hyperfine -N -r <runs> '<command>' times commands";

// --- [OPERATIONS] ----------------------------------------------------------------------

const _writes = (command: Command): readonly string[] => {
    const [head, ...rest] = strip(command.words);
    if (head === undefined) {
        return [];
    }
    const name = basename(head);
    return name === 'echo' || name === 'printf' || (name === 'cat' && rest.length === 0) ? command.writes.filter((path) => !path.startsWith('/dev/')) : [];
};

const _reasons = (commands: readonly Command[]): readonly string[] =>
    commands.flatMap((command, index) => {
        const written = commands.slice(0, index).flatMap(_writes);
        const [head] = pastAssignments(command.words);
        return [
            ...[...command.reads, ...strip(command.words)].filter((path) => written.includes(path)).map((path) => `${path} written then read in one call`),
            ...(command.looped ? _writes(command).map((path) => `${path} appended in a loop`) : []),
            ...(head !== undefined && basename(head) === 'time' ? [`${command.words.join(' ')} times by hand, ${_TIMER}`] : []),
            ...command.clocks.map((clock) => `${clock} times by hand, ${_TIMER}`),
        ];
    });

// --- [POLICY] --------------------------------------------------------------------------

const scriptPolicy =
    (commands: readonly Command[]): (<E>(e: E) => Decision<E>) =>
    <E>(e: E): Decision<E> => {
        const distinct: ReadonlySet<string> = new Set(_reasons(commands));
        return distinct.size === 0 ? pass(e) : deny([...distinct].join(', '));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { scriptPolicy };
