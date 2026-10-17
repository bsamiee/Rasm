import { none, type Option, some } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Invocation = readonly [string, ...string[]];
type Given = readonly [name: string, value: string];

interface Program {
    readonly valued?: readonly string[];
    readonly arities?: ReadonlyMap<string, number>;
    readonly bodies?: readonly string[];
    readonly leading?: number;
    readonly leadingOptions?: readonly string[];
    readonly recursive?: readonly string[];
    readonly stdin?: 'default' | 'always';
    readonly stdinless?: readonly string[];
    readonly code?: readonly string[];
    readonly writes?: readonly string[];
    readonly defaultOperand?: string;
    readonly informational?: readonly string[];
    readonly wholeWords?: true;
    readonly shell?: true;
    readonly runs?: readonly (readonly string[])[] | '--';
    readonly execs?: readonly string[];
    readonly spans?: readonly string[];
    readonly manifests?: readonly string[];
    readonly solutions?: readonly string[];
    readonly commands?: number;
    readonly excludes?: readonly string[];
    readonly patterns?: readonly string[];
    readonly aliases?: ReadonlyMap<string, string>;
    readonly relocations?: readonly string[];
}
interface Operands {
    readonly inputs: readonly string[];
    readonly options: readonly string[];
    readonly values: readonly Given[];
    readonly positions: readonly { readonly at: number; readonly names: readonly string[]; readonly taken: number }[];
}
interface ParsedOption {
    readonly names: readonly string[];
    readonly taken: number;
    readonly joined: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ENV_ASSIGN = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const _QUOTED = /(?:\$?(?<quote>["'])|\\)(?<body>(?<=')[^']*|(?<=")(?:[^"\\]|\\.)*|(?<=\\)[\s\S])\k<quote>/gu;
const _ESCAPED = /\\(?<char>["\\$`\n])/gu;
const _BARE = /^[\w%+,./:=@-]+$/u;
const _TILDE = /^~(?<user>[^/]*)/u;
const _PYTHON: Program = { valued: ['-W', '-X'], stdin: 'default', stdinless: ['-m'], code: ['-c'], informational: ['-V'] };
const _SHELL: Program = { valued: ['-o', '-O'], stdin: 'default', code: ['-c'], shell: true };
const _DIRECTORY: Program = { writes: ['PWD'] };
const _FD: Program = {
    valued: [
        '-e',
        '--extension',
        '-t',
        '--type',
        '-E',
        '--exclude',
        '-d',
        '--max-depth',
        '--min-depth',
        '--exact-depth',
        '-S',
        '--size',
        '--changed-within',
        '--changed-before',
        '-o',
        '--owner',
        '-C',
        '--base-directory',
        '--search-path',
        '-j',
        '--threads',
        '--max-results',
        '-c',
        '--color',
        '--format',
        '--path-separator',
        '--ignore-file',
        '--ignore-contain',
        '--and',
        '--batch-size',
    ],
    leading: 1,
    execs: ['-x', '-X', '--exec', '--exec-batch'],
};
const _FIND: Program = { wholeWords: true, execs: ['-exec', '-execdir', '-ok', '-okdir'] };
const _AST_GREP: Program = {
    valued: [
        '-c',
        '--config',
        '--custom-languages',
        '-r',
        '--rule',
        '--rewrite',
        '--inline-rules',
        '-p',
        '--pattern',
        '--selector',
        '--strictness',
        '-k',
        '--kind',
        '-l',
        '--lang',
        '--format',
        '--report-style',
        '--filter',
        '--min-severity',
        '--no-ignore',
        '--globs',
        '-j',
        '--threads',
        '--color',
        '--heading',
        '--inspect',
        '-A',
        '--after',
        '-B',
        '--before',
        '-C',
        '--context',
        '--max-results',
        '--items',
        '--type',
        '--match',
        '--view',
        '--outline-rules',
        '-t',
        '--test-dir',
        '--snapshot-dir',
        '-f',
    ],
    commands: 1,
};
const _SQL: Program = {
    valued: ['-cmd', '-init', '-newline', '-nullvalue', '-separator', '-storage-version'],
    leading: 1,
    stdin: 'default',
    stdinless: ['-c', '-s', '-f', '-no-stdin'],
    informational: ['-h', '-help', '-version'],
    wholeWords: true,
};
const _PROGRAMS: ReadonlyMap<string, Program> = new Map(
    Object.entries({
        sudo: {
            valued: ['-C', '-D', '-g', '-h', '-p', '-R', '-T', '-U', '-u', '--close-from', '--chdir', '--group', '--host', '--prompt', '--chroot', '--command-timeout', '--other-user', '--user'],
            runs: [[]],
        },
        doas: { valued: ['-C', '-u'], runs: [[]] },
        env: { valued: ['-C', '-P', '-S', '-u'], runs: [[]] },
        command: { runs: [[]] },
        exec: { valued: ['-a'], runs: [[]] },
        nice: { valued: ['-n'], runs: [[]] },
        nohup: { runs: [[]] },
        setsid: { runs: [[]] },
        stdbuf: { valued: ['-e', '-i', '-o'], runs: [[]] },
        timeout: { valued: ['-k', '-s', '--kill-after', '--signal'], leading: 1, runs: [[]] },
        time: { valued: ['-o'], runs: [[]] },
        xargs: { valued: ['-E', '-I', '-J', '-L', '-n', '-P', '-R', '-S', '-s'], stdin: 'always', runs: [[]] },
        caffeinate: { valued: ['-t', '-w'], runs: [[]] },
        arch: { valued: ['-arch', '-d'], runs: [[]] },
        xcrun: { valued: ['--sdk', '--toolchain'], runs: [[]] },
        lockf: { valued: ['-t'], leading: 1, runs: [[]] },
        npx: { runs: [[]] },
        npm: { runs: [['exec'], ['x']] },
        pnpm: { runs: [[], ['exec'], ['dlx']] },
        uv: {
            valued: [
                '--extra',
                '--no-extra',
                '--group',
                '--no-group',
                '--only-group',
                '--no-editable-package',
                '--env-file',
                '-w',
                '--with',
                '--with-editable',
                '--with-requirements',
                '--package',
                '--python-platform',
                '--from',
                '-c',
                '--constraints',
                '-b',
                '--build-constraints',
                '--overrides',
                '--torch-backend',
                '--bump',
                '--output-format',
                '--index',
                '--default-index',
                '-i',
                '--index-url',
                '--extra-index-url',
                '-f',
                '--find-links',
                '--index-strategy',
                '--keyring-provider',
                '-P',
                '--upgrade-package',
                '--upgrade-group',
                '--resolution',
                '--prerelease',
                '--prerelease-package',
                '--fork-strategy',
                '--exclude-newer',
                '--exclude-newer-package',
                '--no-sources-package',
                '--reinstall-package',
                '--link-mode',
                '-C',
                '--config-setting',
                '--config-settings-package',
                '--no-build-isolation-package',
                '--no-build-package',
                '--no-binary-package',
                '--cache-dir',
                '--refresh-package',
                '-p',
                '--python',
                '--color',
                '--allow-insecure-host',
                '--directory',
                '--project',
                '--config-file',
            ],
            runs: [['run'], ['tool', 'run']],
        },
        poetry: { runs: [['run']] },
        hatch: { runs: [['run']] },
        mise: { runs: '--' },
        doppler: {
            valued: [
                '--command',
                '-c',
                '--config',
                '--dynamic-ttl',
                '--fallback',
                '--format',
                '--mount',
                '--mount-max-reads',
                '--mount-template',
                '--name-transformer',
                '--only-secrets',
                '--passphrase',
                '-p',
                '--project',
                '--api-host',
                '--attempts',
                '--config-dir',
                '--dashboard-host',
                '--dns-resolver-address',
                '--dns-resolver-proto',
                '--dns-resolver-timeout',
                '--scope',
                '--timeout',
                '-t',
                '--token',
            ],
            aliases: new Map([
                ['-c', '--config'],
                ['-p', '--project'],
                ['-t', '--token'],
            ]),
            runs: '--',
        },
        op: { runs: '--' },
        hyperfine: {
            valued: [
                '-w',
                '--warmup',
                '-m',
                '--min-runs',
                '-M',
                '--max-runs',
                '-r',
                '--runs',
                '--reference-name',
                '-D',
                '--parameter-step-size',
                '-S',
                '--shell',
                '--style',
                '--sort',
                '-u',
                '--time-unit',
                '--export-asciidoc',
                '--export-csv',
                '--export-json',
                '--export-markdown',
                '--export-orgmode',
                '--output',
                '--input',
                '-n',
                '--command-name',
            ],
            arities: new Map(Object.entries({ '-P': 3, '--parameter-scan': 3, '-L': 2, '--parameter-list': 2 })),
            bodies: ['-s', '--setup', '--reference', '-p', '--prepare', '-C', '--conclude', '-c', '--cleanup'],
        },
        fd: _FD,
        fdfind: _FD,
        find: _FIND,
        gfind: _FIND,
        git: { valued: ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'] },
        'ast-grep': _AST_GREP,
        sg: _AST_GREP,
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
            leadingOptions: ['-e', '--regexp', '-f', '--file', '-N', '--neg-regexp'],
            recursive: ['-r', '-R', '--recursive', '--dereference-recursive'],
            stdin: 'default',
        },
        cat: { stdin: 'default' },
        wc: { stdin: 'default' },
        head: { valued: ['-n', '-c'], stdin: 'default' },
        tail: { valued: ['-b', '-c', '-n'], stdin: 'default' },
        sort: { valued: ['-k', '-t', '-o', '-S', '-T', '--batch-size', '--parallel', '--random-source', '--compress-program'], stdin: 'default', stdinless: ['--files0-from'] },
        uniq: { valued: ['-f', '-s'], stdin: 'default' },
        cut: { valued: ['-b', '-c', '-f', '-d'], stdin: 'default' },
        shasum: { valued: ['-a'], stdin: 'default' },
        md5: { valued: ['-s'], stdin: 'default', stdinless: ['-s'] },
        tr: { stdin: 'always' },
        tee: { stdin: 'always' },
        pbcopy: { stdin: 'always' },
        read: { stdin: 'always', stdinless: ['-u'] },
        base64: { valued: ['-b', '-i', '-o', '--break', '--input', '--output'], stdin: 'always', stdinless: ['-i', '--input'] },
        sed: { valued: ['-e', '-f'], leading: 1, leadingOptions: ['-e', '-f', '--expression', '--file'], stdin: 'default' },
        awk: { valued: ['-F', '-v', '-f'], leading: 1, leadingOptions: ['-f'], stdin: 'default' },
        jq: { valued: ['-L', '--library-path', '--indent'], arities: new Map(Object.entries({ '--arg': 2, '--argjson': 2, '--slurpfile': 2, '--rawfile': 2 })), leading: 1, stdin: 'default', stdinless: ['-n', '--null-input'] },
        yq: { valued: ['-o', '-p', '-I', '--output-format', '--input-format', '--indent', '--from-file'], leading: 1, leadingOptions: ['--from-file'], stdin: 'default', stdinless: ['-n', '--null-input'] },
        sd: {
            valued: ['-f', '-n', '--flags', '--max-replacements'],
            leading: 2,
            stdin: 'default',
            informational: ['-h', '-V'],
        },
        node: { stdin: 'default', stdinless: ['--test', '--run'], code: ['-e', '-p', '--eval', '--print'], informational: ['-v'] },
        perl: { valued: ['-I'], stdin: 'default', code: ['-e', '-E'], informational: ['-v', '-V', '-h'] },
        ruby: { valued: ['-I', '-r', '-C', '-E'], stdin: 'default', code: ['-e'], informational: ['-v', '-h', '--copyright'] },
        cd: { ..._DIRECTORY, defaultOperand: '~' },
        pushd: _DIRECTORY,
        popd: _DIRECTORY,
        dotnet: {
            valued: ['-c', '-configuration', '-f', '-framework', '-r', '-runtime', '-o', '-output', '-v', '-verbosity', '-a', '-arch', '-os', '-binarylog', '-results-directory', '-project'],
            aliases: new Map([
                ['-c', '-configuration'],
                ['-f', '-framework'],
                ['-r', '-runtime'],
                ['-o', '-output'],
                ['-v', '-verbosity'],
                ['-a', '-arch'],
            ]),
            informational: ['-h', '-?', '-help', '-version', '-info', '-list-sdks', '-list-runtimes'],
            wholeWords: true,
            relocations: ['-project'],
            manifests: ['.csproj', '.fsproj', '.vbproj'],
            solutions: ['.slnx'],
        },
        ruff: {
            valued: [
                '--config',
                '--target-version',
                '--select',
                '--ignore',
                '--extend-select',
                '--extend-ignore',
                '--per-file-ignores',
                '--extend-per-file-ignores',
                '--fixable',
                '--unfixable',
                '--exclude',
                '--extend-exclude',
                '--line-length',
                '--output-format',
                '-o',
                '--output-file',
                '--stdin-filename',
                '--cache-dir',
                '--range',
            ],
            commands: 1,
        },
        ty: {
            valued: ['--python-version', '--python-platform', '--python', '--project', '--config', '--config-file', '--exclude', '--output-format', '--extra-search-path', '--typeshed', '--error', '--warn', '--ignore', '--color'],
            commands: 1,
            excludes: ['--exclude'],
        },
        mypy: {
            valued: ['--python-version', '--platform', '--config-file', '--exclude', '--cache-dir', '-p', '--package', '-m', '--module', '-c', '--command', '--follow-imports', '--python-executable', '--custom-typeshed-dir'],
            commands: 0,
            patterns: ['--exclude'],
        },
        refurb: { valued: ['--ignore', '--load', '--enable', '--disable', '--config-file', '--explain', '--python-version', '--format', '--sort', '--timing-stats'] },
        tsc: {
            valued: ['-p', '--project', '--pretty', '-t', '--target', '-m', '--module', '--moduleResolution', '--outDir', '--rootDir', '--lib', '--jsx', '--locale'],
            aliases: new Map([['-p', '--project']]),
            relocations: ['--project'],
            manifests: ['.json'],
        },
        swift: {
            valued: ['--package-path', '--scratch-path', '--cache-path', '--config-path', '--security-path', '-c', '--configuration', '--product', '--target', '--triple', '--sdk', '--toolchain', '--swift-sdk', '-j', '--jobs', '-Xswiftc', '-Xcc', '-Xcxx', '-Xlinker', '--build-system', '--filter', '--skip'],
        },
        cargo: {
            valued: ['-p', '--package', '--exclude', '--manifest-path', '--lockfile-path', '--profile', '--target', '--target-dir', '-F', '--features', '-j', '--jobs', '--color', '--config', '-Z', '--bin', '--example', '--test', '--bench', '--message-format'],
            spans: ['--workspace', '--all'],
            aliases: new Map([['-p', '--package']]),
        },
        nx: { valued: ['-t', '--targets', '--target', '-p', '--projects', '-c', '--configuration', '--base', '--head', '--exclude', '--output-style'] },
        python: _PYTHON,
        python3: _PYTHON,
        bash: _SHELL,
        sh: _SHELL,
        zsh: _SHELL,
        dash: _SHELL,
        ksh: _SHELL,
        sqlite3: _SQL,
        duckdb: _SQL,
    } satisfies Record<string, Program>),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [OPTIONS]

const declared = (program: string): Program => _PROGRAMS.get(program) ?? {};

const _arity = (row: Program, name: string): number => row.arities?.get(name) ?? ([row.valued, row.bodies, row.code].some((names) => names?.includes(name) === true) ? 1 : 0);

const _option = (row: Program, word: string): ParsedOption => {
    const cut = word.indexOf('=');
    const given = cut < 0 ? word : word.slice(0, cut);
    const head = row.wholeWords === true && given.startsWith('--') ? given.slice(1) : given;
    const names = head.startsWith('--') || row.wholeWords === true || _arity(row, head) > 0 ? [head] : [...head.slice(1)].map((letter) => `${head.charAt(0)}${letter}`);
    const arities = names.map((name) => _arity(row, name));
    const at = arities.findIndex((arity) => arity > 0);
    const [taken = 0] = at === names.length - 1 && cut < 0 ? arities.slice(at) : [];
    return { names: (at < 0 ? names : names.slice(0, at + 1)).map((name) => row.aliases?.get(name) ?? name), taken, joined: cut < 0 ? [] : [word.slice(cut + 1)] };
};

const firstOperand = (invocation: Invocation, index: number): number => {
    const [program] = invocation;
    const word = invocation[index];
    return word === undefined || word === '-' || word === '--' || !word.startsWith('-') ? index : firstOperand(invocation, index + 1 + _option(declared(program), word).taken);
};

const _split = (program: string, args: readonly string[], at: number): Operands => {
    const empty: Operands = { inputs: [], options: [], values: [], positions: [] };
    const row = declared(program);
    const [head, ...rest] = args;
    const flagged = head !== undefined && head !== '-' && head !== '--' && head.startsWith('-');
    const { names, taken, joined } = flagged ? _option(row, head) : { names: [], taken: 0, joined: [] };
    const tail = head === undefined || head === '--' ? empty : _split(program, rest.slice(taken), at + 1 + taken);
    return head === '--'
        ? { ...empty, inputs: rest }
        : {
              inputs: [...(head === undefined || flagged ? [] : [head]), ...tail.inputs],
              options: [...names, ...tail.options],
              values: [...names.slice(-1).flatMap((name) => [...joined, ...rest.slice(0, taken)].map((value): Given => [name, value])), ...tail.values],
              positions: [...(names.length === 0 ? [] : [{ at, names, taken }]), ...tail.positions],
          };
};

const operands = ([program, ...args]: Invocation): Operands => {
    const row = declared(program);
    const split = _split(program, args, 1);
    return { ...split, inputs: split.inputs.slice(split.options.some((name) => row.leadingOptions?.includes(name) === true) ? 0 : (row.leading ?? 0)) };
};

// --- [STDIN]

const informational = (program: string, options: readonly string[]): boolean => options.some((name) => name === '--help' || name === '--version' || declared(program).informational?.includes(name) === true);

const waits = (invocation: Invocation): boolean => {
    const [program] = invocation;
    const { stdin, stdinless = [], code = [], recursive = [] } = declared(program);
    const { inputs, options } = operands(invocation);
    return stdin !== undefined && !informational(program, options) && !options.some((name) => [...stdinless, ...code, ...recursive].includes(name)) && (stdin === 'always' || inputs.length === 0 || inputs.includes('-'));
};

// --- [INVOCATIONS]

const basename = (path: string): string => path.slice(path.lastIndexOf('/') + 1);

const segments = (path: string): readonly string[] =>
    path
        .split('/')
        .filter((part) => part !== '' && part !== '.')
        .reduce<readonly string[]>((kept, part) => (part === '..' ? kept.slice(0, -1) : kept.concat(part)), []);

const placed = (word: string, cwd: Option<string>, home: Option<string>): Option<string> => {
    const user = _TILDE.exec(word)?.groups?.user;
    const expanded = user === undefined || home.kind === 'none' ? word : `${user === '' ? home.value : `${home.value.slice(0, home.value.lastIndexOf('/'))}/${user}`}${word.slice(user.length + 1)}`;
    if (!expanded.startsWith('/') && cwd.kind === 'none') {
        return none;
    }
    return some(`/${segments(expanded.startsWith('/') || cwd.kind === 'none' ? expanded : `${cwd.value}/${expanded}`).join('/')}`);
};

const within = (path: string, folder: string): boolean => path === folder || path.startsWith(folder === '/' ? '/' : `${folder}/`);

const unquoted = (word: string): string => word.replace(_QUOTED, (_match, quote: string | undefined, body: string) => (quote === '"' ? body.replace(_ESCAPED, '$<char>') : body));

const quoted = (word: string): string => (_BARE.test(word) ? word : `'${word.replaceAll("'", String.raw`'\''`)}'`);

const _wrapped = (program: string, words: readonly string[], pending: readonly (readonly string[])[]): readonly string[] => {
    const row = declared(program);
    const [head, ...rest] = words;
    if (head?.startsWith('-') === true) {
        return _wrapped(program, rest.slice(_option(row, head).taken), pending);
    }
    const deeper = pending.flatMap(([first, ...more]) => (first !== undefined && first === head ? [more] : []));
    if (deeper.length > 0) {
        return _wrapped(program, rest, deeper);
    }
    return pending.some((path) => path.length === 0) ? words.slice(row.leading ?? 0) : [];
};

const _chain = (program: string, args: readonly string[]): readonly Invocation[] => {
    const { runs = [] } = declared(program);
    const launched = args.slice(args.includes('--') ? args.indexOf('--') + 1 : args.length);
    const inner = runs === '--' ? launched : _wrapped(program, args, runs);
    return [[program, ...args.slice(0, args.length - inner.length)], ...invocations(inner)];
};

const invocations = ([head, ...rest]: readonly string[]): readonly Invocation[] => {
    if (head === undefined) {
        return [];
    }
    return _ENV_ASSIGN.test(head) ? invocations(rest) : _chain(basename(head), rest);
};

const _exec = (invocation: Invocation): number => {
    const { execs = [] } = declared(invocation[0]);
    return operands(invocation).positions.find(({ names }) => names.some((name) => execs.includes(name)))?.at ?? invocation.length;
};

const inner = (invocation: Invocation): readonly string[] => {
    const rest = invocation.slice(_exec(invocation) + 1);
    const end = rest.findIndex((word) => word === ';' || word === '+');
    return end < 0 ? rest : rest.slice(0, end);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Given, Invocation };
export { basename, declared, firstOperand, informational, inner, invocations, operands, placed, quoted, segments, unquoted, waits, within };
