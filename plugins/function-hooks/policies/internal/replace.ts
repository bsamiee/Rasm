import { operands, programRow } from './invocation.ts';
import { type Call, invocationSites, quote, refuse, type Verdict } from './verdict.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const noOpReplacements = ({ script: { commands } }: Call): readonly Verdict[] =>
    invocationSites(commands).flatMap(({ invocation }) => {
        const [program] = invocation;
        const { previews } = programRow(program);
        const { leading, inputs, options } = operands(invocation);
        const [find, replacement] = leading;
        if (previews === undefined || find === undefined) {
            return [];
        }
        const preview = `preview with ${program} ${previews[0]}`;
        if (find === '') {
            return [refuse(`${program} empty pattern matches everywhere, ${preview}`)];
        }
        const [line] = find.split('\n');
        return find === replacement && inputs.length > 0 && !options.some((name) => previews.includes(name)) ? [refuse(`${program} replaces ${quote(line ?? find)} with itself, ${preview}`)] : [];
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { noOpReplacements };
