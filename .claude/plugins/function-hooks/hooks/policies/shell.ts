// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { type Command, pastAssignments, strip } from '../text/argv.ts';
import { basename } from '../text/path.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WAIT = 'waits, read the exit code of a foreground command or the completion notification of a background command or agent';
const _POLLS: readonly string[] = ['lsof', 'pgrep', 'ps', 'timeout', 'wait'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const _waits = (command: Command): boolean => {
    const heads = [pastAssignments(command.words), strip(command.words)].map((words) => basename(words[0] ?? ''));
    return heads.includes('sleep') || (command.looped && heads.some((head) => _POLLS.includes(head)));
};

// --- [RULES] ---------------------------------------------------------------------------

const waitGuard =
    (commands: readonly Command[]): (<E>(e: E) => Decision<E>) =>
    <E>(e: E): Decision<E> => {
        const hits = commands.filter(_waits).map((command) => command.words.join(' '));
        return hits.length === 0 ? rewrite(e) : deny(`${hits.join(', ')} ${_WAIT}`);
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { waitGuard };
