import type { Command } from '../command.ts';
import { type Decision, none, type Option, refuse, some } from '../composition.ts';
import { type Invocation, invocations, operands, option } from '../invocation.ts';
import { basename } from '../path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Place {
    readonly home: Option<string>;
    readonly cwd: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NAME = 'CloudStorage';
const _CLOUD = `~/Library/${_NAME}`;
const _HOME = /^(?:~|\$HOME|\$\{HOME\})(?=\/|$)/u;
const _PRIMARY = /^(?:-.{2,}|\(|!)$/u;
const _RECURSIVE = /^-[A-Za-z]*[rR]/u;
const _RECURSIVE_LIST = /^-[A-Za-z]*R/u;

// --- [WORDS] ---------------------------------------------------------------------------

const _until = (stop: (word: string) => boolean, args: readonly string[]): readonly string[] => {
    const at = args.findIndex(stop);
    return at < 0 ? args : args.slice(0, at);
};

const _values = (flags: readonly string[], args: readonly string[]): readonly string[] =>
    args.flatMap((word, index) => {
        const [name = word, value] = word.split('=');
        if (!flags.includes(name)) {
            return [];
        }
        return value === undefined ? args.slice(index + 1, index + 2) : [value];
    });

// --- [WALKERS] -------------------------------------------------------------------------

const _inputs = (invocation: Invocation): Option<readonly string[]> => some(operands(invocation).inputs);

const _recurses = (args: readonly string[]): boolean =>
    args.some(
        (word, index) =>
            word === '--recursive' ||
            word === '--dereference-recursive' ||
            _RECURSIVE.test(word) ||
            (word.endsWith('recurse') && (word.startsWith('--directories=') || args[index - 1] === '-d' || args[index - 1] === '--directories')),
    );

// --- [PATHS] ---------------------------------------------------------------------------

const _segments = (home: string, cwd: string, word: string): readonly string[] => {
    const expanded = word.replace(_HOME, () => home);
    return (expanded.startsWith('/') ? expanded : `${cwd}/${expanded}`).split('/').reduce<string[]>((kept, segment) => {
        if (segment === '..') {
            kept.pop();
        } else if (segment !== '' && segment !== '.') {
            kept.push(segment);
        }
        return kept;
    }, []);
};

const _prefixes = (parent: readonly string[], child: readonly string[]): boolean => parent.length <= child.length && parent.every((segment, index) => segment === child[index]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const _refused = (home: string, cwd: string, starts: readonly string[], args: readonly string[]): boolean => {
    const cloud = _segments(home, cwd, _CLOUD);
    const relate = (word: string): 'inside' | 'above' | 'apart' => {
        const path = _segments(home, cwd, word);
        if (_prefixes(cloud, path)) {
            return 'inside';
        }
        return _prefixes(path, cloud) ? 'above' : 'apart';
    };
    const relations = (starts.length === 0 ? [cwd] : starts).map(relate);
    const excluded = args.some((word) => word.includes(_NAME) && relate(word) !== 'above');
    return relations.includes('inside') || (relations.includes('above') && !excluded);
};

const _walks = (home: string, cwd: string, invocation: Invocation): boolean => {
    const [program, ...args] = invocation;
    const inputs = (): Option<readonly string[]> => _inputs(invocation);
    const walkers: Readonly<Record<string, () => Option<readonly string[]>>> = {
        fd: () =>
            some([
                ...operands([program, ..._until((word) => word.startsWith('-') && option(program, word)[0].some((name) => ['-x', '--exec', '-X', '--exec-batch'].includes(name)), args)]).inputs,
                ..._values(['-C', '--base-directory', '--search-path'], args),
            ]),
        find: () => _inputs([program, ..._until((word) => _PRIMARY.test(word), args)]),
        rg: inputs,
        grep: () => (_recurses(invocation) ? inputs() : none),
        du: inputs,
        tree: inputs,
        ls: () => (invocation.some((word) => _RECURSIVE_LIST.test(word)) ? inputs() : none),
        lsof: () => (invocation.includes('+D') ? some(_values(['+D'], invocation)) : none),
    };
    const starts = walkers[basename(program)]?.() ?? none;
    return starts.kind === 'some' && _refused(home, cwd, starts.value, invocation);
};

// --- [POLICY] --------------------------------------------------------------------------

const walkPolicy = (commands: readonly Command[], { home, cwd }: Place): (<E>(e: E) => Decision<E>) =>
    refuse(
        home.kind === 'none'
            ? []
            : commands.flatMap((command) =>
                  invocations(command.words).some((invocation) => _walks(home.value, cwd, invocation))
                      ? [`${command.words.join(' ')} descends into ${_CLOUD}, where dataless cloud placeholders hang the walker on the file provider`]
                      : [],
              ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Place };
export { walkPolicy };
