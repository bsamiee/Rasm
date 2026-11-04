import { Option } from '../../monads.ts';
import type { System } from '../command-decision.ts';
import type { Command } from './command.ts';
import { lockDeletes } from './ghidra.ts';
import { firstOperand, type Invocation, invocations, isInside, operands, programRow, resolvePath } from './invocation.ts';
import { type Call, EXPANSION, edit, editOrRefuse, expanded, refuse, UNRESOLVED_FOLDER, type Verdict } from './verdict.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const GLOB = /[*?[{]/u;
const NAME_CHARACTER = /[\p{L}\p{N}]/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const deletedPath =
    (system: System, command: Command, name: string, start: Option<string>, patterns: readonly string[], covered: (word: string) => string) =>
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
        if (!patterns.some((pattern) => NAME_CHARACTER.test(pattern)) && root.values.some((top) => isInside(top, place))) {
            return [refuse(covered(path))];
        }
        const edits = path === word ? [] : spoken.map(({ raw, span, literal }) => editOrRefuse(command, edit(span.start, span.end, literal, Option.some(`${name} ${raw} became ${literal}`)), `${name} ${raw} expands at run time, pass ${literal}`));
        const [target = place] = resolvePath(path, start, home).values;
        const targets = patterns.length === 0 ? [target] : patterns.map((pattern) => `${target}/${pattern}`);
        return [...edits, ...(await Promise.all(targets.map((candidate) => lockDeletes(system, name, candidate)))).flat()];
    };

const deletes = async ({ system, script: { commands }, project }: Call): Promise<readonly Verdict[]> => {
    const removing = (invocation: Invocation): readonly Invocation[] => {
        if (invocation[0] !== 'git') {
            return ['rm', 'rimraf', 'trash', 'unlink', 'shred'].includes(invocation[0]) ? [invocation] : [];
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
        const [program, ...args] = last;
        const { options, inner, roots, base, leading, values } = operands(last);
        const [remover] = removing(last);
        const [feeder] = chain.slice(0, -1).filter(([wrapper]) => programRow(wrapper).stdin === 'always');
        const name = program === 'git' ? 'git rm' : program;
        if (remover !== undefined) {
            return feeder === undefined ? [...new Set(operands(remover).inputs)].map(deletedPath(system, command, name, cwd, [], (word) => `${name} ${word} covers the repository root, run nx run ${project}:clean`)) : [[refuse(`${name} reads paths from ${feeder[0]}, use fd -X ${name}`)]];
        }
        if (!(options.includes('-delete') || invocations(inner).some((launched) => removing(launched).length > 0))) {
            return [];
        }
        const tests = ['-name', '-iname', '-path', '-ipath', '-wholename', '-iwholename', '-regex', '-iregex'];
        const patterns = program === 'find' || program === 'gfind' ? args.flatMap((word, at) => (tests.includes(word) ? args.slice(at + 1, at + 2) : [])) : [...leading, ...values.flatMap(([flag, value]) => (flag === '-e' || flag === '--extension' ? [value] : []))];
        const folder = base.reduce((place, word) => resolvePath(word, place, system.home), cwd);
        return [...new Set(roots)].map(deletedPath(system, command, program, folder, patterns, (word) => `${program} deletes everything under ${word}, add a -name pattern`));
    });
    return (await Promise.all(judged)).flat();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { deletes };
