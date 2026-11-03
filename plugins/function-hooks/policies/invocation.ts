import { none, type Option, some } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Invocation = readonly [string, ...string[]];

interface Program {
    readonly valued?: readonly string[];
    readonly arities?: Readonly<Record<string, number>>;
    readonly bodies?: readonly string[];
    readonly leading?: number;
    readonly leadingOptions?: readonly string[];
    readonly stdin?: 'default' | 'always';
    readonly stdinless?: readonly string[];
    readonly code?: readonly string[];
    readonly shell?: readonly string[] | 'joined';
    readonly chdir?: readonly string[];
    readonly informational?: readonly string[];
    readonly wholeWords?: true;
    readonly runs?: readonly (readonly string[])[] | '--';
    readonly execs?: readonly string[];
    readonly walks?: { readonly when?: readonly string[]; readonly paths?: readonly string[]; readonly base?: readonly string[]; readonly before?: readonly string[] };
    readonly previews?: readonly [string, ...string[]];
    readonly headless?: readonly string[];
    readonly flags?: readonly string[];
    readonly targets?: readonly string[];
    readonly timer?: true | readonly string[];
    readonly endless?: true | readonly string[];
    readonly verdict?: true;
    readonly repeats?: true;
    readonly standalone?: RegExp;
    readonly closes?: true | readonly string[];
}
interface Operands {
    readonly inputs: readonly string[];
    readonly options: readonly string[];
    readonly values: readonly (readonly [name: string, value: string])[];
    readonly inner: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASSIGNMENT = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const OPTION = /^-./u;
const TILDE = /^~(?<user>[^/]*)/u;
const PYTHON: Program = { valued: ['-W', '-X'], stdin: 'default', stdinless: ['-m'], code: ['-c'], informational: ['-V'] };
const SHELL: Program = { valued: ['-o', '-O'], stdin: 'default', shell: ['-c'] };
const FD: Program = {
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
    informational: ['-h', '-V'],
    execs: ['-x', '-X', '--exec', '--exec-batch'],
    walks: { paths: ['--search-path'], base: ['-C', '--base-directory'] },
};
const FIND: Program = { wholeWords: true, execs: ['-exec', '-execdir', '-ok', '-okdir'], walks: { before: ['-', '(', '!'] } };
const PROCESSES: Program = { valued: ['-d', '-F', '-G', '-g', '-P', '-s', '-t', '-U', '-u'] };
const VERDICT: Program = { verdict: true };
const SQL: Program = {
    valued: ['-cmd', '-init', '-newline', '-nullvalue', '-separator', '-storage-version'],
    leading: 1,
    stdin: 'default',
    stdinless: ['-c', '-s', '-f', '-no-stdin'],
    informational: ['-h', '-help', '-version'],
    wholeWords: true,
};
const PROGRAMS: ReadonlyMap<string, Program> = new Map(
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
        xargs: { valued: ['-E', '-I', '-J', '-L', '-n', '-P', '-R', '-S', '-s'], stdin: 'always', runs: [[]], repeats: true },
        kill: { valued: ['-s', '-n'] },
        pgrep: PROCESSES,
        pkill: PROCESSES,
        sleep: { timer: true },
        wait4path: { endless: true },
        caffeinate: { valued: ['-t', '-w'], runs: [[]], timer: ['-t'], endless: true },
        watch: { valued: ['-n', '--interval'], endless: true },
        arch: { valued: ['-arch', '-d'], runs: [[]] },
        xcrun: { valued: ['--sdk', '--toolchain'], runs: [[]] },
        lockf: { valued: ['-t'], leading: 1, runs: [[]] },
        npx: { runs: [[]] },
        npm: { runs: [['exec'], ['x']], verdict: true },
        pnpm: { runs: [[], ['exec'], ['dlx']], verdict: true },
        dotnet: VERDICT,
        swift: VERDICT,
        xcodebuild: VERDICT,
        pytest: VERDICT,
        vitest: VERDICT,
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
            verdict: true,
        },
        poetry: { runs: [['run']] },
        hatch: { runs: [['run']] },
        mise: { runs: '--' },
        doppler: { runs: '--' },
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
            arities: { '-P': 3, '--parameter-scan': 3, '-L': 2, '--parameter-list': 2 },
            bodies: ['-s', '--setup', '--reference', '-p', '--prepare', '-C', '--conclude', '-c', '--cleanup'],
        },
        fd: FD,
        fdfind: FD,
        find: FIND,
        gfind: FIND,
        tree: { valued: ['-L', '--level', '-I', '--ignore-glob', '-s', '--sort', '-t', '--time', '-w', '--width'], walks: {} },
        du: { valued: ['-B', '-d', '-I', '-t', '-X', '--block-size', '--max-depth', '--threshold', '--exclude', '--exclude-from', '--files0-from', '--time-style'], walks: {} },
        git: { valued: ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'] },
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
            stdin: 'default',
            walks: { when: ['-r', '-R', '--recursive', '--dereference-recursive'] },
            closes: ['-q', '--quiet', '--silent', '-m', '--max-count', '-l', '--files-with-matches'],
        },
        rg: {
            valued: [
                '-e',
                '--regexp',
                '-f',
                '--file',
                '--pre',
                '--pre-glob',
                '--dfa-size-limit',
                '-E',
                '--encoding',
                '--engine',
                '-m',
                '--max-count',
                '--regex-size-limit',
                '-j',
                '--threads',
                '-g',
                '--glob',
                '--iglob',
                '--ignore-file',
                '-d',
                '--max-depth',
                '--max-filesize',
                '-t',
                '--type',
                '-T',
                '--type-not',
                '--type-add',
                '--type-clear',
                '-A',
                '--after-context',
                '-B',
                '--before-context',
                '--color',
                '--colors',
                '-C',
                '--context',
                '--context-separator',
                '--field-context-separator',
                '--field-match-separator',
                '--hostname-bin',
                '--hyperlink-format',
                '-M',
                '--max-columns',
                '--path-separator',
                '-r',
                '--replace',
                '--sort',
                '--sortr',
                '--generate',
            ],
            leading: 1,
            leadingOptions: ['-e', '--regexp', '-f', '--file', '--files', '--type-list', '--generate'],
            informational: ['-h', '-V'],
            walks: {},
            closes: ['-q', '--quiet', '-m', '--max-count', '-l', '--files-with-matches'],
        },
        cat: { stdin: 'default' },
        wc: { stdin: 'default' },
        head: { valued: ['-n', '-c'], stdin: 'default', closes: true },
        tail: { valued: ['-b', '-c', '-n'], stdin: 'default', endless: ['-f', '-F', '--follow'] },
        sort: { valued: ['-k', '-t', '-o', '-S', '-T', '--batch-size', '--parallel', '--random-source', '--compress-program'], stdin: 'default', stdinless: ['--files0-from'] },
        uniq: { valued: ['-f', '-s'], stdin: 'default' },
        cut: { valued: ['-b', '-c', '-f', '-d'], stdin: 'default' },
        shasum: { valued: ['-a'], stdin: 'default' },
        md5: { valued: ['-s'], stdin: 'default', stdinless: ['-s'] },
        tr: { stdin: 'always' },
        tee: { stdin: 'always' },
        pbcopy: { stdin: 'always' },
        read: { valued: ['-a', '-d', '-i', '-n', '-N', '-p', '-t', '-u'], stdin: 'always', stdinless: ['-u'], timer: ['-t'] },
        base64: { valued: ['-b', '-i', '-o', '--break', '--input', '--output'], stdin: 'always', stdinless: ['-i', '--input'] },
        sed: { valued: ['-e', '-f'], leading: 1, leadingOptions: ['-e', '-f', '--expression', '--file'], stdin: 'default' },
        awk: { valued: ['-F', '-v', '-f'], leading: 1, leadingOptions: ['-f'], stdin: 'default', standalone: /^\s*BEGIN\s*\{(?:[^{}]|\{[^{}]*\})*\}\s*$/u },
        jq: { valued: ['-L', '--library-path', '--indent'], arities: { '--arg': 2, '--argjson': 2, '--slurpfile': 2, '--rawfile': 2 }, leading: 1, stdin: 'default', stdinless: ['-n', '--null-input'] },
        yq: { valued: ['-o', '-p', '-I', '--output-format', '--input-format', '--indent', '--from-file'], leading: 1, leadingOptions: ['--from-file'], stdin: 'default', stdinless: ['-n', '--null-input'] },
        sd: {
            valued: ['-f', '-n', '--flags', '--max-replacements'],
            flags: ['-p', '--preview', '-F', '--fixed-strings', '-A', '--across', '-h', '--help', '-V', '--version'],
            leading: 2,
            stdin: 'default',
            informational: ['-h', '-V'],
            previews: ['-p', '--preview'],
        },
        node: { stdin: 'default', stdinless: ['--test', '--run'], code: ['-e', '-p', '--eval', '--print'], informational: ['-v'] },
        perl: { valued: ['-I'], stdin: 'default', code: ['-e', '-E'], informational: ['-v', '-V', '-h'] },
        ruby: { valued: ['-I', '-r', '-C', '-E'], stdin: 'default', code: ['-e'], informational: ['-v', '-h', '--copyright'] },
        eval: { shell: 'joined' },
        set: { valued: ['-o', '+o'] },
        cd: { chdir: ['~'] },
        pushd: { chdir: [] },
        popd: { chdir: [] },
        python: PYTHON,
        python3: PYTHON,
        bash: SHELL,
        sh: SHELL,
        zsh: SHELL,
        dash: SHELL,
        ksh: SHELL,
        sqlite3: SQL,
        duckdb: SQL,
        nx: { verdict: true, valued: ['-c', '--configuration', '-t', '--targets', '--target', '-p', '--projects', '--exclude', '--outputStyle', '--output-style', '--runner', '--base', '--head', '--files', '--parallel', '--tuiAutoExit', '--type', '--withTarget', '--sep'] },
        tsc: {
            valued: ['-p', '--project', '--pretty', '-t', '--target', '-m', '--module', '--moduleResolution', '--lib', '--types', '--typeRoots', '--outDir', '--outFile', '--rootDir', '--declarationDir', '--tsBuildInfoFile', '--jsx', '--locale', '--generateTrace'],
            informational: ['-v', '-h', '-b', '--build', '-w', '--watch', '--init', '--showConfig'],
            targets: ['-p', '--project'],
            verdict: true,
        },
        cargo: {
            valued: ['-p', '--package', '--exclude', '-m', '--manifest-path', '-C', '--target', '--target-dir', '-F', '--features', '--profile', '-j', '--jobs', '--config', '-Z', '--color', '--message-format', '--bin', '--example', '--test', '--bench'],
            targets: ['-p', '--package'],
            verdict: true,
        },
        biome: {
            valued: [
                '--config-path',
                '--max-diagnostics',
                '--reporter',
                '--diagnostic-level',
                '--log-level',
                '--log-kind',
                '--log-path',
                '--vcs-root',
                '--vcs-default-branch',
                '--stdin-file-path',
                '--only',
                '--skip',
                '--colors',
                '--log-file',
                '--reporter-file',
                '--since',
                '--files-max-size',
                '--indent-width',
                '--line-width',
            ],
            targets: [],
        },
        ruff: {
            valued: [
                '--config',
                '--select',
                '--extend-select',
                '--ignore',
                '--per-file-ignores',
                '--extend-per-file-ignores',
                '--fixable',
                '--extend-fixable',
                '--unfixable',
                '--output-format',
                '--extension',
                '--color',
                '--range',
                '-o',
                '--output-file',
                '--target-version',
                '--line-length',
                '--exclude',
                '--extend-exclude',
                '--stdin-filename',
                '--cache-dir',
            ],
            targets: [],
        },
        'ast-grep': {
            valued: [
                '-c',
                '--config',
                '-r',
                '--rule',
                '--inline-rules',
                '-p',
                '--pattern',
                '-l',
                '--lang',
                '-k',
                '--kind',
                '--selector',
                '--strictness',
                '--filter',
                '--globs',
                '--format',
                '--report-style',
                '--color',
                '--no-ignore',
                '--custom-languages',
                '-j',
                '--threads',
                '-A',
                '--after',
                '-B',
                '--before',
                '-C',
                '--context',
                '--max-results',
            ],
            targets: [],
        },
        osascript: { valued: ['-e', '-l', '-s'] },
        open: { valued: ['-a', '-b', '-s', '-u', '-i', '-o', '--env', '--stdin', '--stdout', '--stderr', '--arch', '--url'] },
        analyzeHeadless: { wholeWords: true, informational: ['-help'], valued: ['-log', '-scriptlog', '-scriptPath', '-propertiesPath', '-processor', '-cspec', '-analysisTimeoutPerFile', '-keystore', '-max-cpu', '-librarySearchPaths', '-loader'] },
        Blender: { informational: ['-v', '-h', '--log-list-categories'], headless: ['-b', '--background'] },
    } satisfies Record<string, Program>),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [OPTIONS]

const declared = (program: string): Program => PROGRAMS.get(program) ?? {};

const option = (row: Program, word: string) => {
    const arity = (name: string): number => row.arities?.[name] ?? ([row.valued, row.bodies, row.code].some((listed) => listed?.includes(name) === true) ? 1 : 0);
    const cut = word.indexOf('=');
    const given = cut < 0 ? word : word.slice(0, cut);
    const head = row.wholeWords === true && given.startsWith('--') ? given.slice(1) : given;
    if (head.startsWith('--') || row.wholeWords === true || arity(head) > 0) {
        return { names: [head], taken: cut < 0 ? arity(head) : 0, joined: cut < 0 ? [] : [word.slice(cut + 1)] };
    }
    const [, ...letters] = [...word];
    const names = letters.map((letter) => `-${letter}`);
    const arities = names.map(arity);
    const at = arities.findIndex((count) => count > 0);
    const attached = at < 0 ? '' : letters.slice(at + 1).join('');
    const [taken = 0] = attached === '' ? arities.slice(at) : [];
    return { names: at < 0 ? names : names.slice(0, at + 1), taken, joined: attached === '' ? [] : [attached] };
};

const optional = (row: Program, word: string): boolean => OPTION.test(word) && (row.flags === undefined || option(row, word).names.every((name) => [row.flags, row.valued].some((listed) => listed?.includes(name) === true)));

const firstOperand = (invocation: Invocation, index: number): number => {
    const [program] = invocation;
    const word = invocation[index];
    return word === undefined || word === '--' || !optional(declared(program), word) ? index : firstOperand(invocation, index + 1 + option(declared(program), word).taken);
};

const separated = (row: Program, [head, ...rest]: readonly string[]): Operands => {
    if (head === undefined || head === '--') {
        return { inputs: rest, options: [], values: [], inner: [] };
    }
    if (!optional(row, head)) {
        const tail = separated(row, rest);
        return { ...tail, inputs: [head, ...tail.inputs] };
    }
    const { names, taken, joined } = option(row, head);
    const launches = names.some((name) => row.execs?.includes(name) === true);
    const end = rest.findIndex((word) => word === ';' || word === '+');
    const launched = launches ? rest.slice(0, end < 0 ? rest.length : end) : [];
    const tail = separated(row, rest.slice(launches ? launched.length + 1 : taken));
    return {
        inputs: tail.inputs,
        options: [...names, ...tail.options],
        values: [...names.slice(-1).flatMap((name) => [...joined, ...rest.slice(0, taken)].map((value) => [name, value] as const)), ...tail.values],
        inner: launches ? launched : tail.inner,
    };
};

const operands = (invocation: Invocation) => {
    const [program, ...args] = invocation;
    const row = declared(program);
    const { inputs: all, ...split } = separated(row, args);
    const count = split.options.some((name) => row.leadingOptions?.includes(name) === true) ? 0 : (row.leading ?? 0);
    const inputs = all.slice(count);
    const { when, paths = [], base = [], before } = row.walks ?? { when: [] };
    const tail = args.slice(firstOperand(invocation, 1) - 1);
    const end = tail.findIndex((word) => before?.some((prefix) => word.startsWith(prefix)) === true);
    const given = [...split.values.flatMap(([name, value]) => (paths.includes(name) ? [value] : [])), ...(before === undefined ? inputs : tail.slice(0, end < 0 ? tail.length : end))];
    const roots = given.length === 0 ? ['.'] : given;
    return {
        ...split,
        leading: all.slice(0, count),
        inputs,
        roots: when?.some((name) => split.options.includes(name)) === false ? [] : roots,
        base: split.values.flatMap(([name, value]) => (base.includes(name) ? [value] : [])),
        bodies: [...(row.shell === 'joined' ? [args.join(' ')] : inputs.slice(0, Number(split.options.some((name) => row.shell?.includes(name) === true)))), ...(row.bodies === undefined ? [] : inputs), ...split.values.filter(([name]) => row.bodies?.includes(name) === true).map(([, value]) => value)],
    };
};

// --- [STDIN]

const informational = (program: string, options: readonly string[]): boolean => options.some((name) => name === '--help' || name === '--version' || declared(program).informational?.includes(name) === true);

const marked = (listed: true | readonly string[] | undefined, options: readonly string[]): boolean => listed === true || options.some((name) => listed?.includes(name) === true);

const waits = (invocation: Invocation): boolean => {
    const [program] = invocation;
    const { stdin, stdinless = [], code = [], standalone } = declared(program);
    const { inputs, options, roots, leading } = operands(invocation);
    return stdin !== undefined && !informational(program, options) && !options.some((name) => [...stdinless, ...code].includes(name)) && !leading.some((text) => standalone?.test(text) === true) && (stdin === 'always' || inputs.includes('-') || (inputs.length === 0 && roots.length === 0));
};

// --- [INVOCATIONS]

const placed = (word: string, cwd: Option<string>, home: Option<string>): Option<string> => {
    const expanded = home.kind === 'none' ? word : word.replace(TILDE, (_match, user: string) => (user === '' ? home.value : `${home.value.slice(0, home.value.lastIndexOf('/'))}/${user}`));
    const absolute = cwd.kind === 'some' && !expanded.startsWith('/') ? `${cwd.value}/${expanded}` : expanded;
    return absolute.startsWith('/')
        ? some(
              `/${absolute
                  .split('/')
                  .filter((part) => part !== '' && part !== '.')
                  .reduce<readonly string[]>((kept, part) => (part === '..' ? kept.slice(0, -1) : kept.concat(part)), [])
                  .join('/')}`,
          )
        : none;
};

const within = (path: string, folder: string): boolean => path === folder || path.startsWith(folder === '/' ? '/' : `${folder}/`);

const unquoted = (word: string): string => word.replace(/(?:\$?(?<quote>["'])|\\)(?<body>(?<=')[^']*|(?<=")(?:[^"\\]|\\.)*|(?<=\\)[\s\S])\k<quote>/gu, (_match, quote: string | undefined, body: string) => (quote === '"' ? body.replace(/\\(?<char>["\\$`\n])/gu, '$<char>') : body));

const invocations = ([head, ...rest]: readonly string[]): readonly Invocation[] => {
    if (head === undefined) {
        return [];
    }
    if (ASSIGNMENT.test(head)) {
        return invocations(rest);
    }
    const program = head.slice(head.lastIndexOf('/') + 1);
    const row = declared(program);
    const wrapped = (words: readonly string[], pending: readonly (readonly string[])[]): readonly string[] => {
        const [first, ...more] = words;
        const deeper = pending.flatMap(([step, ...steps]) => (step !== undefined && step === first ? [steps] : []));
        if (first !== undefined && optional(row, first)) {
            return wrapped(more.slice(option(row, first).taken), pending);
        }
        if (deeper.length > 0) {
            return wrapped(more, deeper);
        }
        return pending.some((path) => path.length === 0) ? words.slice(row.leading ?? 0) : [];
    };
    const inner = row.runs === '--' ? rest.slice(rest.includes('--') ? rest.indexOf('--') + 1 : rest.length) : wrapped(rest, row.runs ?? []);
    return [[program, ...rest.slice(0, rest.length - inner.length)], ...invocations(inner)];
};

const staged = <W extends { readonly text: string }>(words: readonly W[], [invocation, ...rest]: readonly Invocation[]): readonly { readonly assigned: readonly W[]; readonly words: readonly W[] }[] => {
    const at = words.findIndex(({ text }) => !ASSIGNMENT.test(text));
    return invocation === undefined || at < 0 ? [] : [{ assigned: words.slice(0, at), words: words.slice(at, at + invocation.length) }, ...staged(words.slice(at + invocation.length), rest)];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { declared, firstOperand, type Invocation, informational, invocations, marked, operands, option, optional, placed, staged, unquoted, waits, within };
