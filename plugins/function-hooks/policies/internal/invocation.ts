import { Maybe } from '../../monads.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Invocation = readonly [string, ...string[]];

interface ArgumentGroup<A> {
    readonly words: readonly [A, ...A[]];
    readonly names: readonly string[];
    readonly joined: readonly string[];
    readonly inner: readonly A[] | undefined;
}
interface Program {
    readonly aliases?: readonly string[];
    readonly valueOptions?: readonly string[];
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
    readonly singleDashLong?: true;
    readonly wraps?: readonly (readonly string[])[] | '--';
    readonly launcher?: true;
    readonly versioned?: true;
    readonly module?: string;
    readonly execs?: readonly string[];
    readonly walks?: { readonly when?: readonly string[]; readonly paths?: readonly string[]; readonly base?: readonly string[]; readonly before?: readonly string[]; readonly names?: readonly string[] };
    readonly removes?: true | readonly string[];
    readonly previews?: readonly [string, ...string[]];
    readonly headless?: readonly string[];
    readonly flags?: readonly string[];
    readonly selectors?: readonly string[];
    readonly modes?: readonly (readonly [string, ...string[]])[];
    readonly subcommandless?: true;
    readonly inert?: readonly string[];
    readonly timer?: true | readonly string[];
    readonly endless?: true | readonly string[];
    readonly exitStatus?: true;
    readonly repeats?: true;
    readonly stdinlessScript?: RegExp;
    readonly closesInput?: true | readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASSIGNMENT = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const OPTION = /^-./u;
const TILDE = /^~(?<user>[^/]*)/u;
const DIRNAME = /^.*\//su;
const PACKAGES = /^.*\./su;
const HELP: readonly string[] = ['--help', '--version'];
const PROGRAMS: ReadonlyMap<string, Program> = new Map(
    Object.entries<Program>({
        sudo: {
            valueOptions: ['-C', '-D', '-g', '-h', '-p', '-R', '-T', '-U', '-u', '--close-from', '--chdir', '--group', '--host', '--prompt', '--chroot', '--command-timeout', '--other-user', '--user'],
            wraps: [[]],
        },
        doas: { valueOptions: ['-C', '-u'], wraps: [[]] },
        env: { valueOptions: ['-C', '-P', '-S', '-u'], wraps: [[]] },
        command: { informational: ['-v', '-V'], wraps: [[]] },
        builtin: { wraps: [[]] },
        nohup: { aliases: ['setsid', 'npx'], wraps: [[]] },
        exec: { valueOptions: ['-a'], wraps: [[]] },
        nice: { valueOptions: ['-n'], wraps: [[]] },
        stdbuf: { valueOptions: ['-e', '-i', '-o'], wraps: [[]] },
        timeout: { valueOptions: ['-k', '-s', '--kill-after', '--signal'], leading: 1, wraps: [[]] },
        time: { valueOptions: ['-o'], wraps: [[]] },
        xargs: { valueOptions: ['-E', '-I', '-J', '-L', '-n', '-P', '-R', '-S', '-s'], stdin: 'always', wraps: [[]], repeats: true },
        kill: { valueOptions: ['-s', '-n'] },
        ps: { valueOptions: ['-p', '-o', '-O', '-U', '-u', '-G', '-g', '-t', '-M', '-N'] },
        pgrep: { aliases: ['pkill'], valueOptions: ['-d', '-F', '-G', '-g', '-P', '-s', '-t', '-U', '-u'] },
        sleep: { timer: true },
        wait4path: { endless: true },
        caffeinate: { valueOptions: ['-t', '-w'], wraps: [[]], timer: ['-t'], endless: true },
        watch: { valueOptions: ['-n', '--interval'], endless: true },
        arch: { valueOptions: ['-arch', '-d'], wraps: [[]] },
        xcrun: { valueOptions: ['--sdk', '--toolchain'], wraps: [[]] },
        lockf: { valueOptions: ['-t'], leading: 1, wraps: [[]] },
        flock: { valueOptions: ['-w', '--wait', '--timeout', '-E', '--conflict-exit-code'], leading: 1, wraps: [[]] },
        npm: { wraps: [['exec'], ['x']], exitStatus: true },
        pnpm: { wraps: [[], ['exec'], ['dlx']], exitStatus: true },
        'effect-tsgo': { valueOptions: ['--completions', '--log-level'], wraps: [[]], launcher: true },
        dotnet: { aliases: ['swift', 'xcodebuild', 'pytest', 'vitest'], exitStatus: true },
        uv: {
            valueOptions: [
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
            wraps: [['run'], ['tool', 'run']],
            exitStatus: true,
        },
        poetry: { aliases: ['hatch'], wraps: [['run']] },
        pixi: { valueOptions: ['-e', '--environment', '--manifest-path', '--auth-file', '--pypi-keyring-provider', '--concurrent-solves', '--concurrent-downloads', '--color'], wraps: [['run']] },
        mise: { aliases: ['doppler', 'op'], wraps: '--' },
        hyperfine: {
            valueOptions: [
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
        fd: {
            aliases: ['fdfind'],
            valueOptions: [
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
            walks: { paths: ['--search-path'], base: ['-C', '--base-directory'], names: ['-e', '--extension'] },
        },
        find: {
            aliases: ['gfind'],
            singleDashLong: true,
            execs: ['-exec', '-execdir', '-ok', '-okdir'],
            walks: { before: ['-', '(', '!'], names: ['-name', '-iname', '-path', '-ipath', '-wholename', '-iwholename', '-regex', '-iregex'] },
            removes: ['-delete'],
        },
        rm: { aliases: ['rimraf', 'trash', 'unlink', 'shred'], removes: true },
        tree: { valueOptions: ['-L', '--level', '-I', '--ignore-glob', '-s', '--sort', '-t', '--time', '-w', '--width'], walks: {} },
        du: { valueOptions: ['-B', '-d', '-I', '-t', '-X', '--block-size', '--max-depth', '--threshold', '--exclude', '--exclude-from', '--files0-from', '--time-style'], walks: {} },
        git: { valueOptions: ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'] },
        rsync: {
            valueOptions: [
                '-B',
                '-e',
                '-f',
                '-M',
                '-T',
                '-@',
                '--info',
                '--debug',
                '--backup-dir',
                '--suffix',
                '--confine-root',
                '--chmod',
                '--checksum-choice',
                '--block-size',
                '--rsh',
                '--rsync-path',
                '--max-delete',
                '--max-size',
                '--min-size',
                '--max-alloc',
                '--partial-dir',
                '--usermap',
                '--groupmap',
                '--chown',
                '--timeout',
                '--contimeout',
                '--modify-window',
                '--temp-dir',
                '--compare-dest',
                '--copy-dest',
                '--link-dest',
                '--compress-choice',
                '--compress-level',
                '--compress-threads',
                '--skip-compress',
                '--filter',
                '--exclude',
                '--exclude-from',
                '--include',
                '--include-from',
                '--files-from',
                '--copy-as',
                '--address',
                '--port',
                '--sockopts',
                '--outbuf',
                '--remote-option',
                '--out-format',
                '--log-file',
                '--log-file-format',
                '--password-file',
                '--early-input',
                '--bwlimit',
                '--stop-after',
                '--write-batch',
                '--only-write-batch',
                '--read-batch',
                '--protocol',
                '--iconv',
                '--checksum-seed',
            ],
        },
        grep: {
            valueOptions: [
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
            closesInput: ['-q', '--quiet', '--silent', '-m', '--max-count', '-l', '--files-with-matches'],
        },
        rg: {
            valueOptions: [
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
            closesInput: ['-q', '--quiet', '-m', '--max-count', '-l', '--files-with-matches'],
        },
        cat: { aliases: ['wc'], stdin: 'default' },
        head: { valueOptions: ['-n', '-c'], stdin: 'default', closesInput: true },
        tail: { valueOptions: ['-b', '-c', '-n'], stdin: 'default', endless: ['-f', '-F', '--follow'] },
        sort: { valueOptions: ['-k', '-t', '-o', '-S', '-T', '--batch-size', '--parallel', '--random-source', '--compress-program'], stdin: 'default', stdinless: ['--files0-from'] },
        uniq: { valueOptions: ['-f', '-s'], stdin: 'default' },
        cut: { valueOptions: ['-b', '-c', '-f', '-d'], stdin: 'default' },
        shasum: { valueOptions: ['-a'], stdin: 'default' },
        md5: { valueOptions: ['-s'], stdin: 'default', stdinless: ['-s'] },
        tr: { aliases: ['tee', 'pbcopy'], stdin: 'always' },
        read: { valueOptions: ['-a', '-d', '-i', '-n', '-N', '-p', '-t', '-u'], stdin: 'always', stdinless: ['-u'], timer: ['-t'] },
        base64: { valueOptions: ['-b', '-i', '-o', '--break', '--input', '--output'], stdin: 'always', stdinless: ['-i', '--input'] },
        sed: { valueOptions: ['-e', '-f'], leading: 1, leadingOptions: ['-e', '-f', '--expression', '--file'], stdin: 'default' },
        awk: { valueOptions: ['-F', '-v', '-f'], leading: 1, leadingOptions: ['-f'], stdin: 'default', stdinlessScript: /^\s*BEGIN\s*\{(?:[^{}]|\{[^{}]*\})*\}\s*$/u },
        jq: { valueOptions: ['-L', '--library-path', '--indent'], arities: { '--arg': 2, '--argjson': 2, '--slurpfile': 2, '--rawfile': 2 }, leading: 1, stdin: 'default', stdinless: ['-n', '--null-input'] },
        yq: { valueOptions: ['-o', '-p', '-I', '--output-format', '--input-format', '--indent', '--from-file'], leading: 1, leadingOptions: ['--from-file'], stdin: 'default', stdinless: ['-n', '--null-input'] },
        sd: {
            valueOptions: ['-f', '-n', '--flags', '--max-replacements'],
            flags: ['-F', '--fixed-strings', '-A', '--across'],
            leading: 2,
            stdin: 'default',
            informational: ['-h', '-V'],
            previews: ['-p', '--preview'],
        },
        node: { stdin: 'default', stdinless: ['--test', '--run'], code: ['-e', '-p', '--eval', '--print'], informational: ['-v'] },
        perl: { valueOptions: ['-I'], stdin: 'default', code: ['-e', '-E'], informational: ['-v', '-V', '-h'] },
        ruby: { valueOptions: ['-I', '-r', '-C', '-E'], stdin: 'default', code: ['-e'], informational: ['-v', '-h', '--copyright'] },
        eval: { shell: 'joined' },
        set: { valueOptions: ['-o', '+o'] },
        cd: { chdir: ['~'] },
        pushd: { aliases: ['popd'], chdir: [] },
        python: { aliases: ['python3'], valueOptions: ['-W', '-X'], module: '-m', stdin: 'default', stdinless: ['-m'], code: ['-c'], informational: ['-V'] },
        bash: { aliases: ['sh', 'zsh', 'dash', 'ksh'], valueOptions: ['-o', '-O'], stdin: 'default', shell: ['-c'] },
        sqlite3: { aliases: ['duckdb'], valueOptions: ['-cmd', '-init', '-newline', '-nullvalue', '-separator', '-storage-version'], leading: 1, stdin: 'default', stdinless: ['-c', '-s', '-f', '-no-stdin'], informational: ['-h', '-help', '-version'], singleDashLong: true },
        nx: { exitStatus: true, valueOptions: ['-c', '--configuration', '-t', '--targets', '--target', '-p', '--projects', '--exclude', '--outputStyle', '--output-style', '--runner', '--base', '--head', '--files', '--parallel', '--tuiAutoExit', '--type', '--withTarget', '--sep'] },
        tsc: {
            versioned: true,
            valueOptions: ['--pretty', '-t', '--target', '-m', '--module', '--moduleResolution', '--lib', '--types', '--typeRoots', '--outDir', '--outFile', '--rootDir', '--declarationDir', '--tsBuildInfoFile', '--jsx', '--locale', '--generateTrace'],
            informational: ['-v', '-h', '-b', '--build', '-w', '--watch', '--init', '--showConfig'],
            selectors: ['-p', '--project'],
            exitStatus: true,
        },
        cargo: {
            modes: [['--fix']],
            valueOptions: ['--exclude', '-m', '--manifest-path', '-C', '--target', '--target-dir', '-F', '--features', '--profile', '-j', '--jobs', '--config', '-Z', '--color', '--message-format', '--bin', '--example', '--test', '--bench'],
            selectors: ['-p', '--package'],
            exitStatus: true,
        },
        biome: {
            modes: [['--write', '--fix'], ['--unsafe']],
            valueOptions: [
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
            selectors: [],
        },
        ruff: {
            modes: [['--fix'], ['--no-fix'], ['--fix-only'], ['--no-fix-only'], ['--unsafe-fixes'], ['--no-unsafe-fixes'], ['--check'], ['--diff']],
            valueOptions: [
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
            selectors: [],
        },
        'ast-grep': {
            modes: [
                ['--update-all', '-U'],
                ['--interactive', '-i'],
            ],
            valueOptions: [
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
                '--inspect',
                '--heading',
                '--outline-rules',
                '--items',
                '--type',
                '--match',
                '--view',
            ],
            informational: ['-h', '-V'],
            selectors: [],
        },
        mypy: {
            valueOptions: [
                '-O',
                '--output',
                '--config-file',
                '--python-version',
                '--python-executable',
                '--platform',
                '--always-true',
                '--always-false',
                '--untyped-calls-exclude',
                '--deprecated-calls-exclude',
                '--disable-error-code',
                '--enable-error-code',
                '--cache-dir',
                '--custom-typing-module',
                '--custom-typeshed-dir',
                '-n',
                '--num-workers',
                '--any-exprs-report',
                '--cobertura-xml-report',
                '--html-report',
                '--linecount-report',
                '--linecoverage-report',
                '--lineprecision-report',
                '--txt-report',
                '--xml-report',
                '--xslt-html-report',
                '--xslt-txt-report',
                '--junit-xml',
                '--find-occurrences',
                '--exclude',
                '-m',
                '--module',
                '-p',
                '--package',
                '-c',
                '--command',
            ],
            arities: { '--shadow-file': 2 },
            selectors: [],
            subcommandless: true,
            inert: ['--cache-dir', '--no-pretty', '--config-file=pyproject.toml'],
        },
        refurb: { valueOptions: ['--ignore', '--load', '--enable', '--disable', '--config-file', '--python-version', '--format', '--sort', '--timing-stats', '--explain'], informational: ['-h', '--explain'], selectors: [], subcommandless: true },
        osascript: { valueOptions: ['-e', '-l', '-s'] },
        open: { valueOptions: ['-a', '-b', '-s', '-u', '-i', '-o', '--env', '--stdin', '--stdout', '--stderr', '--arch', '--url'] },
        xed: { valueOptions: ['-l', '--line'], informational: ['-h', '-v'] },
        analyzeHeadless: { singleDashLong: true, informational: ['-help'], valueOptions: ['-log', '-scriptlog', '-scriptPath', '-propertiesPath', '-processor', '-cspec', '-analysisTimeoutPerFile', '-keystore', '-max-cpu', '-librarySearchPaths', '-loader'] },
        Blender: { informational: ['-v', '-h', '--log-list-categories'], headless: ['-b', '--background', '-c', '--command'] },
        'watch-apps': { informational: ['-h'], valueOptions: ['--document', '--arguments', '--environment', '--pid', '--window', '--longest-side'] },
    }).flatMap(([name, row]) => [name, ...(row.aliases ?? [])].map((alias) => [alias, row] as const)),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [OPTIONS]

const programName = (program: string): string => {
    const separator = program.indexOf('@');
    const name = separator < 0 ? program : program.slice(0, separator);
    return PROGRAMS.get(name)?.versioned === true ? name : program;
};

const programRow = (program: string): Program => PROGRAMS.get(programName(program)) ?? {};

const option = (row: Program, word: string) => {
    const arity = (name: string): number => {
        const key = row.shell !== undefined && row.shell !== 'joined' && name.startsWith('+') ? `-${name.slice(1)}` : name;
        return row.arities?.[key] ?? ([row.valueOptions, row.bodies, row.code, row.selectors, row.walks?.paths, row.walks?.base, row.walks?.names].some((listed) => listed?.includes(key) === true) ? 1 : 0);
    };
    const canonical = (name: string): string => row.modes?.find((aliases) => aliases.includes(name))?.[0] ?? name;
    const cut = word.indexOf('=');
    const given = canonical(cut < 0 ? word : word.slice(0, cut));
    const head = row.singleDashLong === true && given.startsWith('--') ? given.slice(1) : given;
    if (head.startsWith('--') || row.singleDashLong === true || arity(head) > 0) {
        return { names: [head], taken: cut < 0 ? arity(head) : 0, joined: cut < 0 ? [] : [word.slice(cut + 1)] };
    }
    const [, ...letters] = [...word];
    const names = letters.map((letter) => canonical(`${word.slice(0, 1)}${letter}`));
    const arities = names.map(arity);
    if (row.shell !== undefined && row.shell !== 'joined') {
        return { names, taken: arities.reduce((sum, count) => sum + count, 0), joined: [] };
    }
    const at = arities.findIndex((count) => count > 0);
    const attached = at < 0 ? '' : letters.slice(at + 1).join('');
    const [taken = 0] = attached === '' ? arities.slice(at) : [];
    return { names: at < 0 ? names : names.slice(0, at + 1), taken, joined: attached === '' ? [] : [attached] };
};

const isOption = (row: Program, word: string): boolean =>
    (OPTION.test(word) || (row.shell !== undefined && row.shell !== 'joined' && word.startsWith('+') && word.length > 1)) && (row.flags === undefined || option(row, word).names.every((name) => [row.flags, row.valueOptions, row.informational, row.previews, HELP].some((listed) => listed?.includes(name) === true)));

const firstOperand = ([program, ...args]: Invocation): number => {
    const row = programRow(program);
    const skipped = ([word, ...rest]: readonly string[]): number => (word === undefined || word === '--' || !isOption(row, word) ? 0 : 1 + option(row, word).taken + skipped(rest.slice(option(row, word).taken)));
    return 1 + skipped(args);
};

const argumentGroups = function* <A extends string | { readonly text: string }>(program: string, input: ArrayIterator<A>): Generator<ArgumentGroup<A>, void> {
    const row = programRow(program);
    const read = (value: A): string => (typeof value === 'string' ? value : value.text);
    const launched = (): readonly A[] => {
        const next = input.next();
        return next.done === true || read(next.value) === ';' || read(next.value) === '+' ? [] : [next.value, ...launched()];
    };
    const next = input.next();
    if (next.done === true || read(next.value) === '--') {
        return;
    }
    const head = next.value;
    const { names, taken, joined } = isOption(row, read(head)) ? option(row, read(head)) : { names: [], taken: 0, joined: [] };
    const supplied = taken === 0 ? [] : input.take(taken).toArray();
    yield { words: [head, ...supplied], names, joined, inner: names.some((name) => row.execs?.includes(name) === true) ? [...supplied, ...launched()] : undefined };
    if (names.length > 0 || row.shell === undefined || row.shell === 'joined') {
        yield* argumentGroups(program, input);
    }
};

const operands = (invocation: Invocation) => {
    const [program, ...args] = invocation;
    const row = programRow(program);
    const shell = row.shell !== undefined && row.shell !== 'joined';
    const input = args.values();
    const groups = argumentGroups(program, input).toArray();
    const all = [...groups.flatMap(({ names, words }) => (names.length === 0 ? words : [])), ...input];
    const flags = groups.flatMap(({ names, words: [, ...given], joined }) => {
        const supplied = [...joined, ...given].values();
        const last = names.at(-1);
        return names.map((name) => [name, supplied.take(name === last && joined.length > 0 ? joined.length : option(row, name).taken).toArray()] as const);
    });
    const options = flags.map(([name]) => name);
    const values = flags.flatMap(([name, given]) => given.map((value) => [name, value] as const));
    const executesSource = flags.reduce((executes, [name, given]) => (shell && (name === '-n' || name === '+n' || ((name === '-o' || name === '+o') && given[0] === 'noexec')) ? name.startsWith('+') : executes), true);
    const count = options.some((name) => row.leadingOptions?.includes(name) === true) ? 0 : (row.leading ?? 0);
    const inputs = all.slice(count);
    const { when, paths, base, before } = row.walks ?? { when: [] };
    const listed = (names: readonly string[] | undefined): readonly string[] => values.flatMap(([name, value]) => (names?.includes(name) === true ? [value] : []));
    const tail = args.slice(firstOperand(invocation) - 1);
    const end = tail.findIndex((word) => before?.some((prefix) => word.startsWith(prefix)) === true);
    const places = [...listed(paths), ...(before === undefined ? inputs : tail.slice(0, end < 0 ? tail.length : end))];
    const roots = places.length === 0 ? ['.'] : places;
    return {
        options,
        values,
        inner: groups.find((group) => group.inner !== undefined)?.inner ?? [],
        executesSource,
        leading: all.slice(0, count),
        inputs,
        roots: when?.some((name) => options.includes(name)) === false ? [] : roots,
        base: listed(base),
        patterns: listed(row.walks?.names),
        bodies: [...(row.shell === 'joined' ? [args.join(' ')] : inputs.slice(0, Number(options.some((name) => row.shell?.includes(name) === true)))), ...(row.bodies === undefined ? [] : inputs), ...listed(row.bodies)],
    };
};

const informational = (program: string, options: readonly string[]): boolean => options.some((name) => [HELP, programRow(program).informational].some((listed) => listed?.includes(name) === true));

const enabled = (listed: true | readonly string[] | undefined, options: readonly string[]): boolean => listed === true || options.some((name) => listed?.includes(name) === true);

// --- [STDIN]

const readsStdin = (invocation: Invocation): boolean => {
    const [program] = invocation;
    const { stdin, stdinless = [], code = [], shell, stdinlessScript } = programRow(program);
    const { inputs, options, roots, leading } = operands(invocation);
    return (
        stdin !== undefined &&
        !informational(program, options) &&
        !options.some((name) => [...stdinless, ...code, ...(shell === 'joined' ? [] : (shell ?? []))].includes(name)) &&
        !leading.some((text) => stdinlessScript?.test(text) === true) &&
        (stdin === 'always' || inputs.includes('-') || (inputs.length === 0 && roots.length === 0))
    );
};

// --- [PATHS]

const resolvePath = (word: string, cwd: Maybe<string>, home: Maybe<string>): Maybe<string> => {
    const [expanded = word] = home.map((folder) => word.replace(TILDE, (_match, user: string) => (user === '' ? folder : `${folder.slice(0, folder.lastIndexOf('/'))}/${user}`))).values;
    const [absolute = expanded] = cwd.map((folder) => (expanded.startsWith('/') ? expanded : `${folder}/${expanded}`)).values;
    return absolute.startsWith('/')
        ? Maybe.some(
              `/${absolute
                  .split('/')
                  .filter((part) => part !== '' && part !== '.')
                  .reduce<readonly string[]>((kept, part) => (part === '..' ? kept.slice(0, -1) : kept.concat(part)), [])
                  .join('/')}`,
          )
        : Maybe.none;
};

const basename = (path: string): string => path.replace(DIRNAME, '');

const isInside = (path: string, folder: string): boolean => path === folder || path.startsWith(folder === '/' ? '/' : `${folder}/`);

// --- [INVOCATIONS]

const unquote = (word: string): string => word.replace(/(?:\$?(?<quote>["'])|\\)(?<body>(?<=')[^']*|(?<=")(?:[^"\\]|\\.)*|(?<=\\)[\s\S])\k<quote>/gu, (_match, quote: string | undefined, body: string) => (quote === '"' ? body.replace(/\\(?<char>["\\$`\n])/gu, '$<char>') : body));

const invocations = ([head, ...rest]: readonly string[]): readonly Invocation[] => {
    if (head === undefined) {
        return [];
    }
    if (ASSIGNMENT.test(head)) {
        return invocations(rest);
    }
    const program = basename(head);
    const row = programRow(program);
    const operand = firstOperand([program, ...rest]) - 1;
    if (informational(program, operands([program, ...rest.slice(0, operand)]).options)) {
        return [[program, ...rest]];
    }
    const innerWords = (words: readonly string[], pending: readonly (readonly string[])[]): readonly string[] => {
        const [first, ...more] = words;
        const deeper = pending.flatMap(([step, ...steps]) => (step !== undefined && step === first ? [steps] : []));
        if (first !== undefined && isOption(row, first)) {
            return innerWords(more.slice(option(row, first).taken), pending);
        }
        if (deeper.length > 0) {
            return innerWords(more, deeper);
        }
        return pending.some((path) => path.length === 0) ? words.slice(row.leading ?? 0) : [];
    };
    const [module, ...args] = row.module !== undefined && rest.slice(0, operand).includes(row.module) ? rest.slice(operand) : [];
    const wrapped = row.wraps === '--' ? rest.slice(rest.includes('--') ? rest.indexOf('--') + 1 : rest.length) : innerWords(rest, row.wraps ?? []);
    const inner = module === undefined ? wrapped : [module.replace(PACKAGES, ''), ...args];
    return [[program, ...rest.slice(0, rest.length - inner.length)], ...invocations(inner)];
};

const invocationParts = <W extends { readonly text: string }>(words: readonly W[], [invocation, ...rest]: readonly Invocation[]): readonly { readonly assigned: readonly W[]; readonly words: readonly W[] }[] => {
    const at = words.findIndex(({ text }) => !ASSIGNMENT.test(text));
    return invocation === undefined || at < 0 ? [] : [{ assigned: words.slice(0, at), words: words.slice(at, at + invocation.length) }, ...invocationParts(words.slice(at + invocation.length), rest)];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { argumentGroups, basename, enabled, firstOperand, type Invocation, informational, invocationParts, invocations, isInside, operands, programName, programRow, readsStdin, resolvePath, unquote };
