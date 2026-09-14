// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, pass } from '../composition/decision.ts';
import { type Command, LAUNCHERS, pastAssignments, strip } from '../text/command.ts';
import { basename } from '../text/path.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WAIT = 'waits';
const _POLL = 'polls until its exit status changes';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _launched = (words: readonly string[]): readonly string[] => {
    const [head] = strip(words);
    return head !== undefined && LAUNCHERS.includes(basename(head)) ? words.filter((_word, index) => index > 0 && words[index - 1] === '--') : [];
};

const _heads = (words: readonly string[]): readonly string[] => [...pastAssignments(words).slice(0, 1), ...strip(words).slice(0, 1), ..._launched(words)].map(basename);

const _reason = (command: Command): readonly string[] => {
    const heads = _heads(command.words);
    if (heads.includes('sleep')) {
        return [`${command.words.join(' ')} ${_WAIT}`];
    }
    return command.condition && !heads.includes('read') ? [`${command.words.join(' ')} ${_POLL}`] : [];
};

// --- [POLICY] --------------------------------------------------------------------------

const waitPolicy =
    (commands: readonly Command[]): (<E>(e: E) => Decision<E>) =>
    <E>(e: E): Decision<E> => {
        const reasons = commands.flatMap(_reason);
        return reasons.length === 0 ? pass(e) : deny(reasons.join(', '));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { waitPolicy };
