import { faultText, Maybe } from '../../monads.ts';
import type { System } from '../command-decision.ts';
import type { Command } from './command.ts';
import { lockDeletes } from './ghidra.ts';
import { enabled, firstOperand, type Invocation, invocations, isInside, operands, programRow, resolvePath } from './invocation.ts';
import { projectTargets } from './nx-target.ts';
import { type Call, EXPANSION, edit, editOrRefuse, expanded, refuse, UNRESOLVED_FOLDER, type Verdict } from './verdict.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const GLOB = /[*?[{]/u;
const NAME_CHARACTER = /[\p{L}\p{N}]/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const deletedPath =
    (system: System, command: Command, name: string, start: Maybe<string>, patterns: readonly string[], covered: (word: string, top: string) => Promise<string>) =>
    async (word: string): Promise<readonly Verdict[]> => {
        const { root, home } = system;
        const tail = command.parts.at(-1)?.words ?? [];
        const spoken = tail.filter(({ text, expands }) => expands && text === word);
        const path = expanded(tail, word);
        if (spoken.length > 0 && EXPANSION.test(path)) {
            return [refuse(`${name} ${word} expands at run time, pass a literal path`)];
        }
        const cut = spoken.length > 0 ? path.search(GLOB) : -1;
        const [place] = resolvePath(cut < 0 ? path : path.slice(0, path.lastIndexOf('/', cut) + 1), start, home).values;
        if (place === undefined) {
            return [refuse(`${name} ${UNRESOLVED_FOLDER}`)];
        }
        const [top] = patterns.some((pattern) => NAME_CHARACTER.test(pattern)) ? [] : root.values.filter((folder) => isInside(folder, place));
        if (top !== undefined) {
            return [refuse(await covered(path, top))];
        }
        const edits = path === word ? [] : spoken.map(({ raw, span, literal }) => editOrRefuse(command, edit(span.start, span.end, literal, Maybe.some(`${name} ${raw} became ${literal}`)), `${name} ${raw} expands at run time, pass ${literal}`));
        const [target = place] = resolvePath(path, start, home).values;
        const targets = patterns.length === 0 ? [target] : patterns.map((pattern) => `${target}/${pattern}`);
        return [...edits, ...(await Promise.all(targets.map((candidate) => lockDeletes(system, name, candidate)))).flat()];
    };

const deletes = async (call: Call): Promise<readonly Verdict[]> => {
    const {
        system,
        script: { commands },
    } = call;
    const removing = (invocation: Invocation): readonly Invocation[] => {
        if (invocation[0] !== 'git') {
            return programRow(invocation[0]).removes === true ? [invocation] : [];
        }
        const [key, ...args] = invocation.slice(firstOperand(invocation));
        return key === 'rm' && !args.includes('--cached') ? [['rm', ...args]] : [];
    };
    const judged = commands.flatMap((command): readonly (readonly Verdict[] | Promise<readonly Verdict[]>)[] => {
        const { invocations: chain, cwd } = command;
        const last = chain.at(-1);
        if (last === undefined) {
            return [];
        }
        const [program] = last;
        const { options, inner, roots, base, leading, patterns } = operands(last);
        const [remover] = removing(last);
        const [feeder] = chain.slice(0, -1).filter(([wrapper]) => programRow(wrapper).stdin === 'always');
        const name = program === 'git' ? 'git rm' : program;
        if (remover !== undefined) {
            const covered = async (word: string, top: string): Promise<string> =>
                `${name} ${word} covers the repository root, ${(await projectTargets(call, '.', top)).match(
                    ({ name: project, targets }) => (targets.clean === undefined ? 'name each path inside it' : `run nx run ${project}:clean`),
                    (faults) => `name each path inside it. ${faultText(faults)}`,
                )}`;
            return feeder === undefined ? [...new Set(operands(remover).inputs)].map(deletedPath(system, command, name, cwd, [], covered)) : [[refuse(`${name} reads paths from ${feeder[0]}, use fd -X ${name}`)]];
        }
        if (!(enabled(programRow(program).removes, options) || invocations(inner).some((launched) => removing(launched).length > 0))) {
            return [];
        }
        const folder = base.reduce((place, word) => resolvePath(word, place, system.home), cwd);
        return [...new Set(roots)].map(deletedPath(system, command, program, folder, [...leading, ...patterns], (word) => Promise.resolve(`${program} deletes everything under ${word}, add a -name pattern`)));
    });
    return (await Promise.all(judged)).flat();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { deletes };
