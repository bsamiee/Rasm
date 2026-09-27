import { basename } from './path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Invocation = readonly [string, ...string[]];

interface Program {
    readonly valued: readonly string[];
    readonly pairs?: readonly string[];
    readonly leading?: number;
    readonly supplies?: readonly string[];
}
interface Operands {
    readonly inputs: readonly string[];
    readonly options: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ENV_ASSIGN = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const _DURATION = /^\d+(?:\.\d+)?[smhd]?$/u;
const _PYTHON: Program = { valued: ['-W', '-X'] };
const _SHELL: Program = { valued: ['-o', '-O'] };
const _SQL: Program = { valued: ['-cmd', '-init', '-newline', '-nullvalue', '-separator', '-storage-version'], leading: 1 };
const _PROGRAMS: Readonly<Record<string, Program>> = {
    sudo: { valued: ['-C', '-D', '-g', '-h', '-p', '-R', '-T', '-U', '-u', '--close-from', '--chdir', '--group', '--host', '--prompt', '--chroot', '--command-timeout', '--other-user', '--user'] },
    doas: { valued: ['-C', '-u'] },
    env: { valued: ['-C', '-P', '-S', '-u'] },
    exec: { valued: ['-a'] },
    nice: { valued: ['-n'] },
    stdbuf: { valued: ['-e', '-i', '-o'] },
    timeout: { valued: ['-k', '-s', '--kill-after', '--signal'] },
    time: { valued: ['-o'] },
    xargs: { valued: ['-E', '-I', '-J', '-L', '-n', '-P', '-R', '-S', '-s'] },
    caffeinate: { valued: ['-t', '-w'] },
    arch: { valued: ['-arch', '-d'] },
    fd: {
        valued: [
            '-C',
            '--base-directory',
            '--search-path',
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
        ],
        leading: 1,
    },
    rg: {
        valued: [
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
        ],
        leading: 1,
        supplies: ['-e', '--regexp', '-f', '--file', '--files', '--type-list'],
    },
    grep: {
        valued: [
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
        ],
        leading: 1,
        supplies: ['-e', '--regexp', '-f', '--file', '-N', '--neg-regexp'],
    },
    du: { valued: ['-B', '-I', '-d', '-t'] },
    tree: {
        valued: [
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
        ],
    },
    ls: { valued: ['-D'] },
    lsof: { valued: ['-c', '-d', '-D', '-f', '-F', '-g', '-i', '-L', '-o', '-p', '-r', '-s', '-S', '-T', '-u', '-x'] },
    wait: { valued: ['-p'] },
    head: { valued: ['-n', '-c'] },
    tail: { valued: ['-b', '-c', '-n'] },
    sort: { valued: ['-k', '-t', '-o', '-S', '-T', '--batch-size', '--parallel', '--random-source', '--compress-program'] },
    uniq: { valued: ['-f', '-s'] },
    cut: { valued: ['-b', '-c', '-f', '-d'] },
    shasum: { valued: ['-a'] },
    md5: { valued: ['-s'] },
    base64: { valued: ['-b', '-i', '-o', '--break', '--input', '--output'] },
    sed: { valued: ['-e', '-f'], leading: 1, supplies: ['-e', '-f', '--expression', '--file'] },
    awk: { valued: ['-F', '-v', '-f'], leading: 1, supplies: ['-f'] },
    jq: { valued: ['-L', '--library-path', '--indent'], pairs: ['--arg', '--argjson', '--slurpfile', '--rawfile'], leading: 1 },
    yq: { valued: ['-o', '-p', '-I', '--output-format', '--input-format', '--indent', '--from-file'], leading: 1, supplies: ['--from-file'] },
    sd: { valued: ['-f', '-n', '--flags', '--max-replacements'], leading: 2 },
    python: _PYTHON,
    python3: _PYTHON,
    bash: _SHELL,
    sh: _SHELL,
    zsh: _SHELL,
    sqlite3: _SQL,
    duckdb: _SQL,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const _arity = (row: Program | undefined, name: string): number => {
    if (row?.pairs?.includes(name) === true) {
        return 2;
    }
    return row?.valued.includes(name) === true ? 1 : 0;
};

const option = (program: string, word: string): readonly [readonly string[], number] => {
    const row = _PROGRAMS[basename(program)];
    const [head = word] = word.split('=');
    const names = head.startsWith('--') || _arity(row, head) > 0 ? [head] : [...head.slice(1)].map((letter) => `-${letter}`);
    const arities = names.map((name) => _arity(row, name));
    const at = arities.findIndex((arity) => arity > 0);
    const [taken = 0] = at === names.length - 1 && !word.includes('=') ? arities.slice(at) : [];
    return [at < 0 ? names : names.slice(0, at + 1), taken];
};

const _split = (program: string, args: readonly string[]): Operands => {
    const [head, ...rest] = args;
    if (head === undefined) {
        return { inputs: [], options: [] };
    }
    if (head === '--') {
        return { inputs: rest, options: [] };
    }
    if (head === '-' || !head.startsWith('-')) {
        const tail = _split(program, rest);
        return { ...tail, inputs: [head, ...tail.inputs] };
    }
    const [named, taken] = option(program, head);
    const tail = _split(program, rest.slice(taken));
    return { ...tail, options: [...named, ...tail.options] };
};

const operands = ([program, ...args]: Invocation): Operands => {
    const row = _PROGRAMS[basename(program)];
    const split = _split(program, args);
    const leading = split.options.some((name) => row?.supplies?.includes(name) === true) ? 0 : (row?.leading ?? 0);
    return { ...split, inputs: split.inputs.slice(leading) };
};

const _wrapped = (program: string, words: readonly string[]): readonly string[] => {
    const wrappers = ['sudo', 'doas', 'env', 'command', 'exec', 'nice', 'nohup', 'stdbuf', 'timeout', 'time', 'xargs', 'caffeinate', 'arch', 'setsid', 'uv', 'npm', 'npx', 'pnpm', 'poetry', 'hatch'];
    const [head] = words;
    if (!wrappers.includes(program)) {
        return ['mise', 'doppler', 'op'].includes(program) && words.includes('--') ? words.slice(words.indexOf('--') + 1) : [];
    }
    if (head === undefined || !(head.includes('=') || _DURATION.test(head) || ['run', 'exec', 'tool', 'x'].includes(head) || head.startsWith('-'))) {
        return words;
    }
    const taken = head.startsWith('-') ? option(program, head)[1] : 0;
    return _wrapped(program, words.slice(1 + taken));
};

const invocations = (words: readonly string[]): readonly Invocation[] => {
    const [head, ...rest] = words;
    if (head === undefined) {
        return [];
    }
    if (_ENV_ASSIGN.test(head)) {
        return invocations(rest);
    }
    const inner = _wrapped(basename(head), rest);
    return [[head, ...rest.slice(0, rest.length - inner.length)], ...invocations(inner)];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Invocation };
export { invocations, operands, option };
