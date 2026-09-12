// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, pass } from '../composition/decision.ts';
import { type Command, pastAssignments, strip } from '../text/command.ts';
import { basename } from '../text/path.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SCRIPT = 'write the script file, then run it in one command';
const _TIMER = "hyperfine -N -r <runs> '<command>' times commands";
const _DEVICE = '/dev/';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _authored = (command: Command): boolean => {
    const [head, ...rest] = strip(command.words);
    if (head === undefined) {
        return false;
    }
    const name = basename(head);
    return name === 'echo' || name === 'printf' || (name === 'cat' && rest.length === 0);
};

const _writes = (command: Command): readonly string[] => (_authored(command) ? command.writes.filter((path) => !path.startsWith(_DEVICE)) : []);

const _reads = (command: Command): readonly string[] => [...command.reads, ...strip(command.words)];

const _timed = (command: Command): readonly string[] => {
    const [head] = pastAssignments(command.words);
    return head !== undefined && basename(head) === 'time' ? [`${command.words.join(' ')} times by hand, ${_TIMER}`] : [];
};

const _reasons = (commands: readonly Command[]): readonly string[] =>
    commands.flatMap((command, index) => {
        const written = commands.slice(0, index).flatMap(_writes);
        return [
            ..._reads(command)
                .filter((path) => written.includes(path))
                .map((path) => `${path} written then read in one call, ${_SCRIPT}`),
            ...(command.looped ? _writes(command).map((path) => `${path} appended in a loop, ${_SCRIPT}`) : []),
            ..._timed(command),
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
