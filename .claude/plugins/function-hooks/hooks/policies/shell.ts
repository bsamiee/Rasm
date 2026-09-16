// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, pass } from '../composition.ts';
import { basename, type Command, LAUNCHERS, pastAssignments, strip } from '../text/command.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _reason = (command: Command): readonly string[] => {
    const { words } = command;
    const stripped = strip(words);
    const [head] = stripped;
    const launched = head !== undefined && LAUNCHERS.includes(basename(head)) ? words.filter((_word, index) => index > 0 && words[index - 1] === '--') : [];
    const heads = [...pastAssignments(words).slice(0, 1), ...stripped.slice(0, 1), ...launched].map(basename);
    if (heads.includes('sleep')) {
        return [`${words.join(' ')} waits`];
    }
    return command.condition && !heads.includes('read') ? [`${words.join(' ')} polls until its exit status changes`] : [];
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
