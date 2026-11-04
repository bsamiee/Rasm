import { enabled, operands, programRow } from './invocation.ts';
import { type Call, edit, invocationsOf, type Verdict } from './verdict.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pipefail = ({ script: { commands } }: Call): readonly Verdict[] => {
    const own = commands.filter(({ origin }) => origin === 'call');
    const set = invocationsOf(own, 'set').some((invocation) => operands(invocation).values.some(([, value]) => value === 'pipefail'));
    const masked = own.some(({ pipesOutput, invocations: chain }) => pipesOutput && chain.slice(-1).some(([program]) => programRow(program).exitStatus === true));
    const closing = own.some(({ hasInput, invocations: chain }) => hasInput && chain.some((invocation) => enabled(programRow(invocation[0]).closesInput, operands(invocation).options)));
    return masked && !set && !closing ? [edit(0, 0, 'set -o pipefail; ')] : [];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pipefail };
