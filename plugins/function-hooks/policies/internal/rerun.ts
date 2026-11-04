import { ANALYZE_HEADLESS, serveCommand } from './ghidra.ts';
import { operands } from './invocation.ts';
import { type Call, EXPANSION, invocationsOf, refuse, type Verdict } from './verdict.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const loopReruns = ({ script: { commands, jobs } }: Call): readonly Verdict[] => {
    const runs = commands.filter(({ invocations: chain }) => chain.some(([program]) => program === ANALYZE_HEADLESS));
    const fanning = runs.filter(({ inLoop }) => inLoop || (runs.length > 1 && jobs.length > 0));
    const looping = commands.filter(({ inLoop }) => inLoop);
    return [
        ...(invocationsOf(looping, 'nx').some((invocation) => operands(invocation).inputs[0] !== 'show') ? [refuse('Loop reruns nx tasks, use one nx run-many')] : []),
        ...invocationsOf(fanning, ANALYZE_HEADLESS).map((invocation) => {
            const { options, inputs } = operands(invocation);
            const [, name] = inputs;
            const project = name === undefined || EXPANSION.test(name) ? '<name>' : name;
            return refuse(`${ANALYZE_HEADLESS} fans out per input, ${options.includes('-import') ? 'chain one -import per project with &&' : `send each run with ${serveCommand(project)}`}`);
        }),
    ];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { loopReruns };
