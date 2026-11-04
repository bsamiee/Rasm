import { operands, programRow, readsStdin, resolvePath } from './invocation.ts';
import { type Call, invocationSites, refuse, type Verdict } from './verdict.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const inputSources = ({ system: { home }, script: { commands } }: Call): readonly Verdict[] => {
    const written = commands.map((writer) => writer.heredocFiles.flatMap((path) => resolvePath(path, writer.cwd, home).values));
    return invocationSites(commands).flatMap(({ command: { hasInput, cwd }, index, invocation }) => {
        const heredocFiles = new Set(written.slice(0, index).flat());
        const ran = (word: string): boolean => resolvePath(word, cwd, home).values.some((path) => heredocFiles.has(path));
        const [program] = invocation;
        const { code, shell } = programRow(program);
        const { options, inputs, executesSource } = operands(invocation);
        const coded = executesSource && ((code !== undefined && (options.some((name) => code.includes(name)) || (hasInput && readsStdin(invocation)))) || ((code ?? shell) !== undefined && inputs.slice(0, 1).some(ran)));
        return [...(coded ? [refuse(`${program} runs inline code, use Edit or the installed tool`)] : []), ...(!hasInput && readsStdin(invocation) ? [refuse(`${program} waits on stdin with no input, pass a file or pipe`)] : [])];
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { inputSources };
