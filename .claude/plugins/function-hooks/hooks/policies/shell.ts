// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, pass } from '../composition/decision.ts';
import { type Command, pastAssignments, strip } from '../text/command.ts';
import { basename } from '../text/path.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ALTERNATIVE = 'read the exit code of a foreground command or the completion notification of a background command or agent';
const _WAIT = `waits, ${_ALTERNATIVE}`;
const _POLL = `polls until its exit status changes, ${_ALTERNATIVE}`;
const _LAUNCHERS: readonly string[] = ['mise', 'doppler', 'op'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const _launched = (words: readonly string[]): readonly string[] =>
    _LAUNCHERS.includes(basename(strip(words)[0] ?? '')) ? words.filter((_word, index) => index > 0 && words[index - 1] === '--') : [];

const _heads = (words: readonly string[]): readonly string[] =>
    [pastAssignments(words)[0], strip(words)[0], ..._launched(words)].map((word) => basename(word ?? ''));

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
