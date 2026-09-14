// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, pass } from '../composition/decision.ts';
import { fromNullable, none, type Option, some } from '../composition/option.ts';
import { type Command, LAUNCHERS, strip } from '../text/command.ts';
import { basename } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Place {
    readonly home: Option<string>;
    readonly cwd: string;
}

type Relation = 'inside' | 'above' | 'apart';

// Positional words and the options the option words open, `-ab` opens `-a` and `-b`, `--name=value` opens `--name`
interface Scan {
    readonly found: readonly string[];
    readonly given: readonly string[];
}

// Start paths a walker reads from its arguments, none when the arguments do not walk
type Walker = (args: readonly string[]) => Option<readonly string[]>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NAME = 'CloudStorage';
const _CLOUD = `~/Library/${_NAME}`;
const _WHY = `descends into ${_CLOUD}, where dataless cloud placeholders hang the walker on the file provider`;
const _HOME = /^(?:~|\$HOME|\$\{HOME\})(?=\/|$)/u;
// Expression primaries of find, three characters or more, `(`, or `!`, the positional words before the first are the start paths
const _PRIMARY = /^(?:-.{2,}|\(|!)$/u;
const _RECURSIVE = /^-[A-Za-z]*[rR]/u;
const _LISTS = /^-[A-Za-z]*R/u;
// Options whose value is the next word, read from the installed fd, ripgrep, ugrep, BSD du, eza, and BSD ls
const _FD_STARTS: readonly string[] = ['-C', '--base-directory', '--search-path'];
const _FD: readonly string[] = [
    ..._FD_STARTS,
    '-d',
    '--max-depth',
    '--min-depth',
    '--exact-depth',
    '-E',
    '--exclude',
    '-t',
    '--type',
    '-e',
    '--extension',
    '-S',
    '--size',
    '--changed-within',
    '--changed-before',
    '-o',
    '--owner',
    '--format',
    '--batch-size',
    '--ignore-file',
    '-c',
    '--color',
    '--ignore-contain',
    '-j',
    '--threads',
    '--max-results',
    '--path-separator',
    '--and',
];
// Words from the first of these on are the command fd runs per result
const _FD_COMMAND: readonly string[] = ['-x', '--exec', '-X', '--exec-batch'];
const _RG: readonly string[] = [
    '-A',
    '--after-context',
    '-B',
    '--before-context',
    '-C',
    '--context',
    '-d',
    '--max-depth',
    '-E',
    '--encoding',
    '-e',
    '--regexp',
    '-f',
    '--file',
    '-g',
    '--glob',
    '--iglob',
    '-j',
    '--threads',
    '-M',
    '--max-columns',
    '-m',
    '--max-count',
    '-r',
    '--replace',
    '-t',
    '--type',
    '-T',
    '--type-not',
    '--type-add',
    '--type-clear',
    '--max-filesize',
    '--color',
    '--colors',
    '--sort',
    '--sortr',
    '--path-separator',
    '--pre',
    '--pre-glob',
    '--ignore-file',
    '--dfa-size-limit',
    '--regex-size-limit',
    '--engine',
    '--field-context-separator',
    '--field-match-separator',
    '--context-separator',
    '--hostname-bin',
    '--hyperlink-format',
    '--generate',
];
// Options after which no positional is the pattern
const _RG_PATTERNS: readonly string[] = ['-e', '--regexp', '-f', '--file', '--files', '--type-list'];
const _GREP: readonly string[] = [
    '-A',
    '--after-context',
    '-B',
    '--before-context',
    '-C',
    '--context',
    '-d',
    '--directories',
    '-D',
    '--devices',
    '-e',
    '--regexp',
    '-f',
    '--file',
    '-g',
    '--glob',
    '--iglob',
    '-J',
    '--jobs',
    '-M',
    '--file-magic',
    '-m',
    '--max-count',
    '-N',
    '--neg-regexp',
    '-O',
    '--file-extension',
    '-t',
    '--file-type',
    '--include',
    '--exclude',
    '--include-dir',
    '--exclude-dir',
    '--include-from',
    '--exclude-from',
    '--label',
    '--binary-files',
    '--color',
    '--colour',
    '--colors',
    '--colours',
    '--encoding',
    '--format',
    '--replace',
    '--from',
    '--config',
];
const _GREP_PATTERNS: readonly string[] = ['-e', '--regexp', '-f', '--file', '-N', '--neg-regexp'];
const _DU: readonly string[] = ['-B', '-I', '-d', '-t'];
const _TREE: readonly string[] = [
    '-L',
    '--level',
    '-I',
    '--ignore-glob',
    '-s',
    '--sort',
    '-t',
    '--time',
    '-w',
    '--width',
    '-F',
    '--classify',
    '--absolute',
    '--color',
    '--colour',
    '--color-scale',
    '--color-scale-mode',
    '--icons',
    '--hyperlink',
    '--time-style',
];
const _LS: readonly string[] = ['-D'];

// --- [WORDS] ---------------------------------------------------------------------------

// Options an option word opens and whether the next word is its value, a cluster closes at its first valued letter
const _option = (valued: readonly string[], word: string): readonly [readonly string[], boolean] => {
    const [head = word] = word.split('=');
    const names = head.startsWith('--') ? [head] : [...head.slice(1)].map((letter) => `-${letter}`);
    const at = names.findIndex((name) => valued.includes(name));
    return [at < 0 ? names : names.slice(0, at + 1), at === names.length - 1 && !word.includes('=')];
};

const _scan = (valued: readonly string[], args: readonly string[]): Scan => {
    const [head, ...rest] = args;
    if (head === undefined) {
        return { found: [], given: [] };
    }
    if (head === '--') {
        return { found: rest, given: [] };
    }
    if (head === '-' || !head.startsWith('-')) {
        const tail = _scan(valued, rest);
        return { ...tail, found: [head, ...tail.found] };
    }
    const [opened, takes] = _option(valued, head);
    const tail = _scan(valued, rest.slice(takes ? 1 : 0));
    return { ...tail, given: [...opened, ...tail.given] };
};

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

// Positionals after the pattern, every positional once an option supplied the pattern
const _afterPattern = (valued: readonly string[], patterns: readonly string[], args: readonly string[]): readonly string[] => {
    const scan = _scan(valued, args);
    return scan.given.some((name) => patterns.includes(name)) ? scan.found : scan.found.slice(1);
};

const _command = (word: string): boolean => word.startsWith('-') && _option(_FD, word)[0].some((name) => _FD_COMMAND.includes(name));

const _primary = (word: string): boolean => _PRIMARY.test(word);

const _recurses = (args: readonly string[]): boolean =>
    args.some(
        (word, index) =>
            word === '--recursive' ||
            word === '--dereference-recursive' ||
            _RECURSIVE.test(word) ||
            (word.endsWith('recurse') && (word.startsWith('--directories=') || args[index - 1] === '-d' || args[index - 1] === '--directories')),
    );

const _WALKERS: Readonly<Record<string, Walker>> = {
    fd: (args) => some([..._afterPattern(_FD, [], _until(_command, args)), ..._values(_FD_STARTS, args)]),
    find: (args) => some(_scan([], _until(_primary, args)).found),
    rg: (args) => some(_afterPattern(_RG, _RG_PATTERNS, args)),
    grep: (args) => (_recurses(args) ? some(_afterPattern(_GREP, _GREP_PATTERNS, args)) : none),
    du: (args) => some(_scan(_DU, args).found),
    tree: (args) => some(_scan(_TREE, args).found),
    ls: (args) => (args.some((word) => _LISTS.test(word)) ? some(_scan(_LS, args).found) : none),
    lsof: (args) => (args.includes('+D') ? some(_values(['+D'], args)) : none),
};

// --- [PATHS] ---------------------------------------------------------------------------

const _segments = (home: string, cwd: string, word: string): readonly string[] => {
    const expanded = word.replace(_HOME, () => home);
    const kept: string[] = [];
    for (const segment of (expanded.startsWith('/') ? expanded : `${cwd}/${expanded}`).split('/')) {
        if (segment === '..') {
            kept.pop();
        } else if (segment !== '' && segment !== '.') {
            kept.push(segment);
        }
    }
    return kept;
};

const _prefixes = (parent: readonly string[], child: readonly string[]): boolean =>
    parent.length <= child.length && parent.every((segment, index) => segment === child[index]);

const _relation = (cloud: readonly string[], path: readonly string[]): Relation => {
    if (_prefixes(cloud, path)) {
        return 'inside';
    }
    return _prefixes(path, cloud) ? 'above' : 'apart';
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const _invoked = (words: readonly string[]): readonly string[] => {
    const stripped = strip(words);
    const [head] = stripped;
    const at = stripped.indexOf('--');
    return head !== undefined && LAUNCHERS.includes(basename(head)) && at > 0 ? stripped.slice(at + 1) : stripped;
};

const _refused = (home: string, cwd: string, starts: readonly string[], args: readonly string[]): boolean => {
    const cloud = _segments(home, cwd, _CLOUD);
    const relate = (word: string): Relation => _relation(cloud, _segments(home, cwd, word));
    const relations = (starts.length === 0 ? [cwd] : starts).map(relate);
    const excluded = args.some((word) => word.includes(_NAME) && relate(word) !== 'above');
    return relations.includes('inside') || (relations.includes('above') && !excluded);
};

const _reason = (home: string, cwd: string, command: Command): readonly string[] => {
    const [head, ...args] = _invoked(command.words);
    const walker = head === undefined ? none : fromNullable(_WALKERS[basename(head)]);
    const starts = walker.kind === 'some' ? walker.value(args) : none;
    return starts.kind === 'some' && _refused(home, cwd, starts.value, args) ? [`${command.words.join(' ')} ${_WHY}`] : [];
};

// --- [POLICY] --------------------------------------------------------------------------

const walkPolicy =
    (commands: readonly Command[], place: Place): (<E>(e: E) => Decision<E>) =>
    <E>(e: E): Decision<E> => {
        if (place.home.kind === 'none') {
            return pass(e);
        }
        const home = place.home.value;
        const reasons = commands.flatMap((command) => _reason(home, place.cwd, command));
        return reasons.length === 0 ? pass(e) : deny(reasons.join(', '));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Place };
export { walkPolicy };
