// --- [TYPES] ---------------------------------------------------------------------------

type Invocation = readonly [string, ...string[]];
type Given = readonly [name: string, value: string];

interface Program {
    readonly valued?: readonly string[];
    readonly arities?: ReadonlyMap<string, number>;
    readonly bodies?: readonly string[];
    readonly flags?: readonly string[];
    readonly ends?: readonly string[];
    readonly leading?: number;
    readonly leadingOptions?: readonly string[];
    readonly recursive?: readonly string[];
    readonly stdin?: 'default' | 'always';
    readonly stdinless?: readonly string[];
    readonly informational?: readonly string[];
    readonly wholeWords?: true;
    readonly shell?: true;
    readonly runs?: readonly (readonly string[])[] | '--';
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
const _PYTHON: Program = { valued: ['-W', '-X'], stdin: 'default', stdinless: ['-c', '-m'], informational: ['-V'] };
const _SHELL: Program = { valued: ['-o', '-O'], stdin: 'default', stdinless: ['-c'], shell: true };
const _AST_GREP: Program = {
    valued: [
        '-c',
        '--config',
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
            arities: new Map(Object.entries({ '-P': 3, '--parameter-scan': 3, '-L': 2, '--parameter-list': 2 })),
            bodies: ['-s', '--setup', '--reference', '-p', '--prepare', '-C', '--conclude', '-c', '--cleanup'],
        },
        git: { valued: ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'] },
        'ast-grep': _AST_GREP,
        sg: _AST_GREP,
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
            leadingOptions: ['-e', '--regexp', '-f', '--file', '--files', '--type-list'],
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
            leadingOptions: ['-e', '--regexp', '-f', '--file', '-N', '--neg-regexp'],
            recursive: ['-r', '-R', '--recursive', '--dereference-recursive'],
            stdin: 'default',
        },
        du: { valued: ['-B', '-I', '-d', '-t'] },
        tree: {
            valued: ['-L', '--level', '-I', '--ignore-glob', '-s', '--sort', '-t', '--time', '-w', '--width', '-F', '--classify', '--absolute', '--color', '--colour', '--color-scale', '--color-scale-mode', '--icons', '--hyperlink', '--time-style'],
        },
        ls: { valued: ['-D'], recursive: ['-R'] },
        lsof: { valued: ['-c', '-d', '-D', '-f', '-F', '-g', '-i', '-L', '-o', '-p', '-r', '-s', '-S', '-T', '-u', '-x', '+d', '+D'] },
        wait: { valued: ['-p'] },
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
            flags: ['-p', '--preview', '-F', '--fixed-strings', '-s', '-A', '--across', '-h', '--help', '-V', '--version'],
            leading: 2,
            stdin: 'default',
            informational: ['-h', '-V'],
        },
        yamlfmt: { valued: ['-conf', '-debug', '-exclude', '-extensions', '-formatter', '-gitignore_path', '-match_type', '-output_format'], wholeWords: true },
        node: { stdin: 'default', stdinless: ['-e', '-p', '--eval', '--print', '--test', '--run'], informational: ['-v'] },
        dotnet: { informational: ['-h', '-?', '-help', '-version'], wholeWords: true },
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

const _arity = (row: Program, name: string): number => row.arities?.get(name) ?? (row.valued?.includes(name) === true || row.bodies?.includes(name) === true ? 1 : 0);

const _option = (row: Program, word: string): ParsedOption => {
    const cut = word.indexOf('=');
    const given = cut < 0 ? word : word.slice(0, cut);
    const head = row.wholeWords === true && given.startsWith('--') ? given.slice(1) : given;
    const names = head.startsWith('--') || row.wholeWords === true || _arity(row, head) > 0 ? [head] : [...head.slice(1)].map((letter) => `${head.charAt(0)}${letter}`);
    const arities = names.map((name) => _arity(row, name));
    const at = arities.findIndex((arity) => arity > 0);
    const [taken = 0] = at === names.length - 1 && cut < 0 ? arities.slice(at) : [];
    return { names: at < 0 ? names : names.slice(0, at + 1), taken, joined: cut < 0 ? [] : [word.slice(cut + 1)] };
};

const known = (program: string, word: string): boolean => {
    const row = declared(program);
    const { flags } = row;
    return flags === undefined || _option(row, word).names.every((name) => flags.includes(name) || _arity(row, name) > 0);
};

const firstOperand = (invocation: Invocation, index: number): number => {
    const [program] = invocation;
    const word = invocation[index];
    return word === undefined || word === '-' || word === '--' || !word.startsWith('-') || !known(program, word) ? index : firstOperand(invocation, index + 1 + _option(declared(program), word).taken);
};

const _split = (program: string, args: readonly string[]): Operands => {
    const empty: Operands = { inputs: [], options: [], values: [] };
    const row = declared(program);
    const [head, ...rest] = args;
    const flagged = head !== undefined && head !== '-' && head !== '--' && (head.startsWith('-') || (head.startsWith('+') && row.valued?.some((name) => name.startsWith('+')) === true)) && known(program, head);
    const { names, taken, joined } = flagged ? _option(row, head) : { names: [], taken: 0, joined: [] };
    const tail = head === undefined || head === '--' || names.some((name) => row.ends?.includes(name) === true) ? empty : _split(program, rest.slice(taken));
    return head === '--'
        ? { ...empty, inputs: rest }
        : {
              inputs: [...(head === undefined || flagged ? [] : [head]), ...tail.inputs],
              options: [...names, ...tail.options],
              values: [...names.slice(-1).flatMap((name) => [...joined, ...rest.slice(0, taken)].map((value): Given => [name, value])), ...tail.values],
          };
};

const operands = ([program, ...args]: Invocation): Operands => {
    const row = declared(program);
    const split = _split(program, args);
    return { ...split, inputs: split.inputs.slice(split.options.some((name) => row.leadingOptions?.includes(name) === true) ? 0 : (row.leading ?? 0)) };
};

// --- [INVOCATIONS]

const basename = (path: string): string => path.slice(path.lastIndexOf('/') + 1);

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

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Invocation, Operands };
export { basename, declared, firstOperand, invocations, known, operands };
