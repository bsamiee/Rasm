// --- [TYPES] ---------------------------------------------------------------------------

type Invocation = readonly [string, ...string[]];
type Given = readonly [name: string, value: string];

interface Program {
    readonly valued?: readonly string[];
    readonly pairs?: readonly string[];
    readonly ends?: readonly string[];
    readonly leading?: number;
    readonly supplies?: readonly string[];
    readonly recursive?: readonly string[];
    readonly stdin?: 'default' | 'always';
    readonly sources?: readonly string[];
    readonly wholeWords?: true;
    readonly shell?: true;
    readonly runs?: readonly string[] | '--';
}
interface Operands {
    readonly inputs: readonly string[];
    readonly options: readonly string[];
    readonly values: readonly Given[];
}
interface ParsedOption {
    readonly names: readonly string[];
    readonly taken: number;
    readonly joined: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ENV_ASSIGN = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const _PYTHON: Program = { valued: ['-W', '-X'], stdin: 'default', sources: ['-c', '-m', '-V'] };
const _SHELL: Program = { valued: ['-o', '-O'], stdin: 'default', sources: ['-c'], shell: true };
const _SQL: Program = {
    valued: ['-cmd', '-init', '-newline', '-nullvalue', '-separator', '-storage-version'],
    leading: 1,
    stdin: 'default',
    sources: ['-c', '-s', '-f', '-h', '-help', '-version', '-no-stdin'],
    wholeWords: true,
};
const PROGRAMS: Readonly<Record<string, Program>> = {
    sudo: {
        valued: ['-C', '-D', '-g', '-h', '-p', '-R', '-T', '-U', '-u', '--close-from', '--chdir', '--group', '--host', '--prompt', '--chroot', '--command-timeout', '--other-user', '--user'],
        runs: [],
    },
    doas: { valued: ['-C', '-u'], runs: [] },
    env: { valued: ['-C', '-P', '-S', '-u'], runs: [] },
    command: { runs: [] },
    exec: { valued: ['-a'], runs: [] },
    nice: { valued: ['-n'], runs: [] },
    nohup: { runs: [] },
    setsid: { runs: [] },
    stdbuf: { valued: ['-e', '-i', '-o'], runs: [] },
    timeout: { valued: ['-k', '-s', '--kill-after', '--signal'], leading: 1, runs: [] },
    time: { valued: ['-o'], runs: [] },
    xargs: { valued: ['-E', '-I', '-J', '-L', '-n', '-P', '-R', '-S', '-s'], stdin: 'always', runs: [] },
    caffeinate: { valued: ['-t', '-w'], runs: [] },
    arch: { valued: ['-arch', '-d'], runs: [] },
    npx: { runs: [] },
    npm: { runs: ['exec', 'x'] },
    pnpm: { runs: ['exec', 'dlx'] },
    uv: { runs: ['run', 'tool'] },
    poetry: { runs: ['run'] },
    hatch: { runs: ['run'] },
    mise: { runs: '--' },
    doppler: { runs: '--' },
    op: { runs: '--' },
    git: { valued: ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'] },
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
        ends: ['-x', '--exec', '-X', '--exec-batch'],
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
        stdin: 'default',
        sources: ['--files', '--type-list'],
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
        recursive: ['-r', '-R', '--recursive', '--dereference-recursive'],
        stdin: 'default',
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
    ls: { valued: ['-D'], recursive: ['-R'] },
    lsof: { valued: ['-c', '-d', '-D', '-f', '-F', '-g', '-i', '-L', '-o', '-p', '-r', '-s', '-S', '-T', '-u', '-x', '+d', '+D'] },
    wait: { valued: ['-p'] },
    cat: { stdin: 'default' },
    wc: { stdin: 'default' },
    head: { valued: ['-n', '-c'], stdin: 'default' },
    tail: { valued: ['-b', '-c', '-n'], stdin: 'default' },
    sort: { valued: ['-k', '-t', '-o', '-S', '-T', '--batch-size', '--parallel', '--random-source', '--compress-program'], stdin: 'default', sources: ['--files0-from'] },
    uniq: { valued: ['-f', '-s'], stdin: 'default' },
    cut: { valued: ['-b', '-c', '-f', '-d'], stdin: 'default' },
    shasum: { valued: ['-a'], stdin: 'default' },
    md5: { valued: ['-s'], stdin: 'default', sources: ['-s'] },
    tr: { stdin: 'always' },
    tee: { stdin: 'always' },
    pbcopy: { stdin: 'always' },
    read: { stdin: 'always', sources: ['-u'] },
    base64: { valued: ['-b', '-i', '-o', '--break', '--input', '--output'], stdin: 'always', sources: ['-i', '--input'] },
    sed: { valued: ['-e', '-f'], leading: 1, supplies: ['-e', '-f', '--expression', '--file'], stdin: 'default' },
    awk: { valued: ['-F', '-v', '-f'], leading: 1, supplies: ['-f'], stdin: 'default' },
    jq: { valued: ['-L', '--library-path', '--indent'], pairs: ['--arg', '--argjson', '--slurpfile', '--rawfile'], leading: 1, stdin: 'default', sources: ['-n', '--null-input'] },
    yq: { valued: ['-o', '-p', '-I', '--output-format', '--input-format', '--indent', '--from-file'], leading: 1, supplies: ['--from-file'], stdin: 'default', sources: ['-n', '--null-input'] },
    sd: { valued: ['-f', '-n', '--flags', '--max-replacements'], leading: 2, stdin: 'default' },
    node: { stdin: 'default', sources: ['-e', '-p', '-v', '--eval', '--print', '--test', '--run'] },
    python: _PYTHON,
    python3: _PYTHON,
    bash: _SHELL,
    sh: _SHELL,
    zsh: _SHELL,
    dash: _SHELL,
    ksh: _SHELL,
    sqlite3: _SQL,
    duckdb: _SQL,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const basename = (path: string): string => path.slice(path.lastIndexOf('/') + 1);

const _arity = (row: Program | undefined, name: string): number => {
    if (row?.pairs?.includes(name) === true) {
        return 2;
    }
    return row?.valued?.includes(name) === true ? 1 : 0;
};

const option = (program: string, word: string): ParsedOption => {
    const row = PROGRAMS[program];
    const [head = word] = word.split('=');
    const names = head.startsWith('--') || row?.wholeWords === true || _arity(row, head) > 0 ? [head] : [...head.slice(1)].map((letter) => `${head.charAt(0)}${letter}`);
    const arities = names.map((name) => _arity(row, name));
    const at = arities.findIndex((arity) => arity > 0);
    const [taken = 0] = at === names.length - 1 && !word.includes('=') ? arities.slice(at) : [];
    return { names: at < 0 ? names : names.slice(0, at + 1), taken, joined: word.includes('=') ? [word.slice(head.length + 1)] : [] };
};

const _split = (program: string, args: readonly string[]): Operands => {
    const row = PROGRAMS[program];
    const empty: Operands = { inputs: [], options: [], values: [] };
    const [head, ...rest] = args;
    if (head === undefined) {
        return empty;
    }
    if (head === '--') {
        return { ...empty, inputs: rest };
    }
    if (head === '-' || !(head.startsWith('-') || (head.startsWith('+') && row?.valued?.some((name) => name.startsWith('+')) === true))) {
        const tail = _split(program, rest);
        return { ...tail, inputs: [head, ...tail.inputs] };
    }
    const { names, taken, joined } = option(program, head);
    const tail = names.some((name) => row?.ends?.includes(name) === true) ? empty : _split(program, rest.slice(taken));
    const values = names.slice(-1).flatMap((name) => [...joined, ...rest.slice(0, taken)].map((value): Given => [name, value]));
    return { inputs: tail.inputs, options: [...names, ...tail.options], values: [...values, ...tail.values] };
};

const operands = ([program, ...args]: Invocation): Operands => {
    const row = PROGRAMS[program];
    const split = _split(program, args);
    const leading = split.options.some((name) => row?.supplies?.includes(name) === true) ? 0 : (row?.leading ?? 0);
    return { ...split, inputs: split.inputs.slice(leading) };
};

const _wrapped = (program: string, words: readonly string[]): readonly string[] => {
    const { runs, leading = 0 } = PROGRAMS[program] ?? {};
    const [head] = words;
    if (runs === undefined || runs === '--') {
        return runs === '--' && words.includes('--') ? words.slice(words.indexOf('--') + 1) : [];
    }
    if (head === undefined || !(head.startsWith('-') || runs.includes(head))) {
        return words.slice(leading);
    }
    return _wrapped(program, words.slice(1 + (head.startsWith('-') ? option(program, head).taken : 0)));
};

const invocations = (words: readonly string[]): readonly Invocation[] => {
    const [head, ...rest] = words;
    if (head === undefined) {
        return [];
    }
    if (_ENV_ASSIGN.test(head)) {
        return invocations(rest);
    }
    const program = basename(head);
    const inner = _wrapped(program, rest);
    return [[program, ...rest.slice(0, rest.length - inner.length)], ...invocations(inner)];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Invocation, Operands };
export { basename, invocations, operands, option, PROGRAMS };
