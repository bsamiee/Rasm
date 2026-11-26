import type { Usage } from '#/programs/usage';

// --- [TYPES] ---------------------------------------------------------------------------

type Row = typeof Usage.Encoded;
type Slot = NonNullable<Row['input']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const word = { roles: [], count: 'one' } as const;
const words = { roles: [], count: 'rest' } as const;
const file = { roles: ['read'] } as const;
const files = { roles: ['read'], count: 'rest' } as const;
const trees = { roles: ['read'], count: 'rest', tree: true } as const;
const edits = { roles: ['read', 'write'], count: 'rest' } as const;
const removals = { roles: ['delete'], count: 'rest', tree: true } as const;
const moves = { roles: ['read', 'delete'], count: 'rest', tree: true } as const;
const target = { roles: ['create', 'write'] } as const;
const folder = { roles: ['create', 'write'], tree: true } as const;
const stdout = { roles: ['write'] } as const;
const directory = { roles: ['directory'] } as const;
const variable = { roles: ['variable'] } as const;
const user = { roles: ['user'] } as const;
const host = { roles: ['host'], count: 'one' } as const;
const container = { roles: ['container'], count: 'one' } as const;
const code = { roles: ['code'] } as const;
const program = { roles: ['read', 'execute'] } as const;
const script = { roles: ['script'] } as const;
const command = { roles: ['command'], count: 'rest' } as const;
const wrapper = { operands: [command], interspersed: false } as const;
const action = (...ends: readonly string[]) => ({ roles: ['command'], ends }) as const;
const toggle = (otherwise: Slot, flagged: Slot, ...flags: readonly string[]) => [
    { ...otherwise, unless: flags },
    { ...flagged, when: flags },
];
const recurse = (slot: Slot, ...flags: readonly string[]) => toggle(slot, { ...slot, tree: true }, ...flags);
const rewrite = (slot: Slot, ...flags: readonly string[]) => toggle(slot, { ...slot, roles: edits.roles }, ...flags);
const repair = (...flags: readonly string[]) => [
    { ...trees, unless: flags },
    { ...edits, tree: true, when: flags, unless: ['--diff'] },
    { ...trees, when: ['--diff'] },
];
const launch = (...names: readonly string[]) => names.map((name) => ({ name, ...wrapper }));
const flag = <A extends 'none' | 'one' | 'rest'>(arity: A, ...spellings: readonly string[]) => spellings.map((spelling) => [arity, spelling] as const);
const interpret = (codes: readonly string[], ...rest: readonly Slot[]) => ({ operands: [{ ...program, count: 'optional', unless: codes }, ...rest], input: { ...code, unless: codes }, interspersed: false }) as const;
const format = { flags: [...flag('none', '--check', '--all'), ['one', '-p', '--package'], ...flag('one', '--manifest-path', '--message-format')], values: { '--manifest-path': file } } as const satisfies Partial<Row>;
const lint = { flags: [['none', '--fix', '--autocorrect']], operands: rewrite(trees, '--fix') } as const satisfies Partial<Row>;
const signatures: readonly Row[] = [
    { name: 'cat', operands: [files], input: { roles: ['read', 'copy'] }, output: stdout, abbreviates: true },
    {
        name: 'cp',
        flags: [
            ['none', '-R', '-r', '--recursive'],
            ['one', '-t', '--target-directory'],
        ],
        values: { '-t': target },
        operands: [...recurse(files, '-R'), { ...target, count: 'one', unless: ['-t'] }],
        abbreviates: true,
    },
    { name: 'mv', flags: [['one', '-t', '--target-directory']], values: { '-t': target }, operands: [moves, { ...target, count: 'one', unless: ['-t'] }], abbreviates: true },
    { name: 'rm', flags: [['none', '-R', '-r', '--recursive']], operands: recurse({ roles: ['delete'], count: 'rest' }, '-R'), abbreviates: true },
    { name: 'rmdir', operands: [{ roles: ['delete'], count: 'rest' }], abbreviates: true },
    { name: 'rsync', aliases: ['ditto'], operands: [trees, word] },
    { name: 'mkdir', operands: [{ roles: ['create'], count: 'rest' }], abbreviates: true },
    {
        name: 'ln',
        flags: [
            ['none', '-s', '--symbolic'],
            ['one', '-t', '--target-directory'],
        ],
        values: { '-t': target },
        operands: [{ ...file, count: 'one' }, files, { ...target, count: 'optional', unless: ['-t'] }],
        abbreviates: true,
    },
    { name: 'touch', values: { '-r': file }, operands: [{ ...target, count: 'rest' }], abbreviates: true },
    { name: 'truncate', operands: [{ ...target, count: 'rest' }], abbreviates: true },
    { name: 'chmod', aliases: ['chown', 'chgrp'], flags: [['none', '-R', '--recursive']], operands: [word, ...recurse({ roles: ['write'], count: 'rest' }, '-R')], abbreviates: true },
    { name: 'xattr', operands: [edits] },
    { name: 'ls', flags: [['none', '-R', '--recursive']], operands: recurse(files, '-R'), output: stdout, abbreviates: true },
    { name: 'du', operands: [trees], output: stdout, abbreviates: true },
    { name: 'stat', aliases: ['file', 'realpath', 'readlink', 'wait4path'], operands: [files], output: stdout, abbreviates: true },
    { name: 'head', aliases: ['tail', 'wc', 'cut', 'nl', 'shasum', 'sha256sum', 'md5sum', 'md5', 'cksum', 'sum', 'strings', 'od', 'xxd', 'hexdump', 'rev', 'fold', 'paste', 'expand', 'unexpand', 'tac', 'more', 'less'], operands: [files], input: file, output: stdout, abbreviates: true },
    { name: 'sort', flags: [['one', '-o', '--output']], values: { '-o': target }, operands: [files], input: file, output: { ...stdout, unless: ['-o'] }, abbreviates: true },
    {
        name: 'uniq',
        operands: [
            { ...file, count: 'optional' },
            { ...target, count: 'optional' },
        ],
        input: file,
        output: stdout,
        abbreviates: true,
    },
    { name: 'tr', operands: [word, { ...word, count: 'optional' }], input: file, output: stdout, abbreviates: true },
    { name: 'tee', flags: [['none', '-a', '--append']], operands: [{ ...target, count: 'rest' }], input: { roles: ['read', 'copy'] }, output: stdout, abbreviates: true },
    { name: 'shred', operands: [{ roles: ['write', 'delete'], count: 'rest' }], abbreviates: true },
    { name: 'trash', operands: [removals] },
    { name: 'rimraf', operands: [removals] },
    { name: 'mktemp', operands: [{ ...word, count: 'optional' }], output: stdout, abbreviates: true },
    { name: 'screencapture', operands: [{ ...target, count: 'rest' }] },
    {
        name: 'base64',
        flags: [
            ['one', '-i', '--input'],
            ['one', '-o', '--output'],
        ],
        values: { '-i': file, '-o': target },
        operands: [{ ...file, count: 'optional' }],
        input: { ...file, unless: ['-i'] },
        output: { ...stdout, unless: ['-o'] },
        abbreviates: true,
    },
    {
        name: 'shuf',
        flags: [
            ['none', '-e', '--echo'],
            ['one', '-i', '--input-range'],
            ['one', '-n', '--head-count'],
            ['one', '-o', '--output'],
        ],
        values: { '-o': target },
        operands: [
            { ...file, count: 'optional', unless: ['-e', '-i'] },
            { ...words, when: ['-e'] },
        ],
        input: { ...file, unless: ['-e', '-i'] },
        output: { ...stdout, unless: ['-o'] },
        abbreviates: true,
    },
    {
        name: 'sed',
        flags: [
            ['one', '-e', '--expression'],
            ['one', '-f', '--file'],
        ],
        values: { '-f': file },
        operands: [{ ...word, unless: ['-e', '-f'] }, ...rewrite(files, '-i', '--in-place')],
        input: file,
        output: { ...stdout, unless: ['-i', '--in-place'] },
        abbreviates: true,
    },
    { name: 'sd', operands: [word, word, ...toggle(edits, files, '-p')], input: file, output: stdout },
    { name: 'awk', values: { '-f': program }, operands: [{ ...code, count: 'one', unless: ['-f'] }, files], input: file, output: stdout },
    {
        name: 'grep',
        flags: [['none', '-R', '-r', '--recursive', '--dereference-recursive']],
        values: { '-f': file },
        operands: [{ ...word, unless: ['-e', '-f'] }, ...recurse(files, '-R')],
        input: file,
        output: stdout,
        abbreviates: true,
    },
    { name: 'rg', values: { '-f': file }, operands: [{ ...word, unless: ['-e', '-f', '--files'] }, trees], input: file, output: stdout },
    {
        name: 'fd',
        flags: [
            ['rest', '-x', '--exec'],
            ['rest', '-X', '--exec-batch'],
        ],
        values: { '-x': action(';'), '-X': action(';') },
        operands: [{ ...word, count: 'optional' }, trees],
        output: stdout,
    },
    {
        name: 'find',
        flags: [
            ...flag('rest', '-exec', '-execdir', '-ok', '-okdir'),
            ...flag('one', '-name', '-iname', '-path', '-ipath', '-regex', '-iregex', '-type', '-maxdepth', '-mindepth', '-newer', '-mtime', '-mmin', '-size', '-perm', '-user', '-group'),
            ...flag('none', '-delete', '-print', '-print0', '-prune', '-depth', '-not'),
            ['none', '-a', '-and'],
            ['none', '-o', '-or'],
        ],
        values: { '-exec': action(';', '+'), '-execdir': action(';', '+'), '-ok': action(';'), '-okdir': action(';'), '-newer': file, '-f': { ...file, tree: true } },
        operands: toggle(trees, moves, '-delete'),
        output: stdout,
    },
    {
        name: 'tar',
        flags: [
            ['none', '-c', '--create'],
            ['one', '-C', '--directory'],
        ],
        values: { '-C': directory },
        operands: [{ ...trees, when: ['-c'] }],
        abbreviates: true,
    },
    {
        name: 'patch',
        flags: [
            ['one', '-i', '--input'],
            ['one', '-d', '--directory'],
            ['one', '-o', '--output'],
        ],
        values: { '-i': file, '-d': directory, '-o': target },
        operands: [
            { ...edits, count: 'optional' },
            { ...file, count: 'optional' },
        ],
        input: { ...file, unless: ['-i'] },
        abbreviates: true,
    },
    { name: 'curl', values: { '-o': target, '-T': file, '-K': file }, operands: [{ ...host, count: 'rest' }], output: { ...stdout, unless: ['-o', '-O'] } },
    { name: 'ping', operands: [host], output: stdout },
    { name: 'nc', operands: [host, { ...word, count: 'optional' }], input: file, output: stdout },
    { name: 'ffmpeg', flags: flag('one', '-i', '-t'), values: { '-i': file }, operands: [{ ...target, count: 'rest' }] },
    { name: 'jq', flags: [['none', '-n', '--null-input']], values: { '-f': file }, operands: [{ ...word, unless: ['-f'] }, files], input: { ...file, unless: ['-n'] }, output: stdout },
    {
        name: 'yq',
        flags: [
            ['none', '-i', '--inplace'],
            ['none', '-n', '--null-input'],
            ['one', '--from-file'],
        ],
        values: { '--from-file': file },
        operands: [{ ...word, unless: ['--from-file'] }, ...rewrite(files, '-i')],
        input: { ...file, unless: ['-n'] },
        output: { ...stdout, unless: ['-i'] },
    },
    {
        name: 'duckdb',
        aliases: ['sqlite3'],
        flags: [...flag('one', '-c', '-s', '-f', '-cmd', '-init'), ['none', '-no-stdin']],
        values: { '-c': code, '-s': code, '-cmd': code, '-f': program, '-init': program },
        operands: [
            { ...edits, count: 'optional' },
            { ...code, count: 'rest' },
        ],
        input: { ...code, unless: ['-c', '-s', '-f', '-no-stdin'] },
        output: stdout,
    },
    { name: 'hyperfine', flags: [...flag('none', '-S', '-N'), ['one', '--shell']], values: { '-p': script, '-s': script, '-c': script, '-C': script, '--export-json': target }, output: stdout },
    { name: 'printf', flags: [['one', '-v']], values: { '-v': variable }, operands: [words], output: { ...stdout, unless: ['-v'] } },
    {
        name: 'print',
        flags: [...flag('none', '-a', '-b', '-c', '-D', '-e', '-i', '-l', '-m', '-n', '-N', '-o', '-O', '-p', '-P', '-r', '-R', '-s', '-S', '-z'), ...flag('one', '-C', '-f', '-u', '-v', '-x', '-X')],
        values: { '-v': variable },
        operands: [words],
        output: { ...stdout, unless: ['-v'] },
    },
    { name: 'test', aliases: ['['], flags: [['one', '-v']], values: { '-v': variable } },
    {
        name: 'declare',
        aliases: ['typeset', 'local', 'export', 'readonly'],
        flags: flag('none', '-a', '-A', '-E', '-f', '-F', '-g', '-h', '-H', '-i', '-I', '-k', '-l', '-L', '-m', '-n', '-p', '-r', '-R', '-t', '-T', '-u', '-U', '-W', '-x', '-z', '-Z'),
        operands: [
            { ...variable, count: 'rest', when: ['-a', '-A', '-i', '-n'] },
            { ...words, unless: ['-a', '-A', '-i', '-n'] },
        ],
    },
    { name: 'integer', aliases: ['float'], operands: [{ ...variable, count: 'rest' }] },
    { name: 'echo', operands: [words], output: stdout },
    { name: 'read', flags: [...flag('one', '-t', '-p', '-d', '-n', '-u', '-a'), ['none', '-r']], values: { '-a': variable }, operands: [{ ...variable, count: 'rest' }], input: file },
    { name: 'sleep', operands: [words] },
    { name: 'eval', operands: [{ ...script, count: 'rest' }] },
    { name: 'source', aliases: ['.'], operands: [{ ...program, count: 'one' }, words], interspersed: false },
    {
        name: 'sh',
        aliases: ['bash', 'zsh', 'dash', 'ksh', 'mksh'],
        flags: [...flag('none', '-c', '-s', '-i', '-e', '-u', '-x', '-n'), ['none', '-l', '--login'], ['one', '-o'], ['none', '-v', '--verbose']],
        operands: [{ roles: ['shell'], count: 'one', when: ['-c'] }, { ...word, count: 'optional', when: ['-c'] }, { ...program, count: 'optional', unless: ['-c', '-s'] }, words],
        input: { roles: ['shell'], unless: ['-c'] },
        interspersed: false,
    },
    { name: 'python', aliases: ['python3'], flags: flag('rest', '-c', '-m'), values: { '-c': code }, ...interpret(['-c', '-m'], words) },
    {
        name: 'node',
        aliases: ['tsx'],
        flags: [
            ['one', '-e', '--eval'],
            ['one', '-p', '--print'],
        ],
        values: { '-e': code, '-p': code },
        ...interpret(['-e', '-p'], words),
    },
    { name: 'perl', aliases: ['ruby'], flags: [...flag('one', '-e', '-E'), ['optional', '-i']], values: { '-e': code, '-E': code }, ...interpret(['-e', '-E'], ...rewrite(files, '-i')) },
    { name: 'osascript', values: { '-e': code }, ...interpret(['-e'], words) },
    {
        name: 'Blender',
        aliases: ['blender'],
        flags: [
            ['one', '-P', '--python'],
            ['one', '--python-expr'],
            ['none', '-b', '--background'],
        ],
        values: { '-P': program, '--python-expr': code },
        operands: [{ ...edits, count: 'optional' }],
    },
    { name: 'ssh', values: { '-l': user, '-J': { roles: ['host'] }, '-F': file, '-i': file }, operands: [host, { ...script, count: 'rest' }], input: file, output: stdout, interspersed: false },
    {
        name: 'docker',
        commands: [
            { name: 'exec', values: { '-w': directory, '-u': user, '-e': { roles: ['assignment'] } }, operands: [container, command], interspersed: false },
            {
                name: 'run',
                flags: [
                    ['one', '-w', '--workdir'],
                    ['one', '-u', '--user'],
                    ['one', '-e', '--env'],
                    ['one', '-v', '--volume'],
                    ['none', '--rm'],
                ],
                values: { '-w': directory, '-u': user, '-e': { roles: ['assignment'] } },
                operands: [container, command],
                interspersed: false,
            },
        ],
    },
    { name: 'kubectl', commands: [{ name: 'exec', values: { '-c': { roles: ['container'] } }, operands: [container, command] }] },
    {
        name: 'sudo',
        values: { '-u': user, '-D': directory },
        operands: [
            { ...command, unless: ['-s', '-i', '-e', '-l'] },
            { ...script, count: 'rest', when: ['-s', '-i'] },
            { ...edits, when: ['-e'] },
        ],
        interspersed: false,
    },
    { name: 'doas', values: { '-u': user, '-C': file }, operands: [{ ...command, unless: ['-s'] }], interspersed: false },
    {
        name: 'env',
        flags: [
            ['none', '-i', '--ignore-environment'],
            ['one', '-u', '--unset'],
            ['one', '-C', '--chdir'],
            ['one', '-S', '--split-string'],
        ],
        values: { '-i': { roles: ['unset'] }, '-u': { roles: ['unset'] }, '-C': directory },
        operands: [{ roles: ['assignment'], count: 'rest' }, command],
        interspersed: false,
    },
    { name: 'xargs', flags: [['one', '-a', '--arg-file']], values: { '-I': { roles: ['placeholder'] }, '-J': { roles: ['placeholder'] }, '-a': file }, ...wrapper, input: { roles: ['command'], unless: ['-a'] } },
    { name: 'parallel', flags: [['one', '-j', '--jobs', '-P', '--max-procs']], ...wrapper, input: { roles: ['command'] } },
    { name: 'watch', operands: toggle({ ...script, count: 'rest' }, command, '-x'), interspersed: false },
    { name: 'timeout', operands: [word, command], interspersed: false },
    { name: 'nohup', aliases: ['setsid', 'builtin', 'noglob', 'nocorrect', 'arch', 'caffeinate'], ...wrapper },
    { name: 'nice', ...wrapper },
    {
        name: 'stdbuf',
        flags: [
            ['one', '-i', '--input'],
            ['one', '-o', '--output'],
            ['one', '-e', '--error'],
        ],
        ...wrapper,
    },
    { name: 'time', flags: [['one', '-o', '--output'], ['none', '-a', '--append'], ...flag('none', '-p', '-l', '-h')], values: { '-o': target }, ...wrapper },
    { name: 'exec', flags: [...flag('none', '-c', '-l'), ['one', '-a']], ...wrapper },
    { name: 'command', flags: flag('none', '-p', '-v', '-V'), operands: toggle(command, words, '-v', '-V'), interspersed: false },
    { name: 'lockf', flags: [['one', '-t'], ...flag('none', '-k', '-n', '-s')], operands: [{ ...target, count: 'one' }, command], interspersed: false },
    { name: 'xcrun', operands: toggle(command, words, '-f'), interspersed: false },
    {
        name: 'npx',
        flags: [
            ['one', '-p', '--package'],
            ['none', '-y', '--yes'],
            ['one', '-c', '--call'],
        ],
        values: { '-c': script },
        operands: [{ ...command, unless: ['-c'] }],
        interspersed: false,
    },
    {
        name: 'uvx',
        flags: [
            ['one', '--from'],
            ['one', '-w', '--with'],
            ['one', '-p', '--python'],
        ],
        ...wrapper,
    },
    { name: 'uv', commands: [...launch('run'), { name: 'tool', commands: launch('run') }] },
    { name: 'pnpm', commands: launch('exec', 'dlx') },
    { name: 'npm', commands: [{ name: 'exec', aliases: ['x'], ...wrapper }] },
    { name: 'poetry', aliases: ['hatch'], commands: launch('run') },
    { name: 'pixi', commands: launch('run', 'exec') },
    { name: 'mise', commands: [{ name: 'exec', aliases: ['x'], ...wrapper }] },
    { name: 'doppler', aliases: ['op'], commands: launch('run') },
    {
        name: 'git',
        values: { '-C': directory, '--git-dir': directory, '--work-tree': directory },
        commands: [
            { name: 'add', operands: [trees] },
            {
                name: 'rm',
                operands: [
                    { roles: ['delete'], count: 'rest', unless: ['-r', '--cached'] },
                    { ...removals, when: ['-r'], unless: ['--cached'] },
                    { ...files, when: ['--cached'] },
                ],
            },
            { name: 'mv', operands: [moves, { ...target, count: 'one' }] },
            { name: 'clean', operands: [removals] },
            { name: 'restore', operands: [{ roles: ['write'], count: 'rest', tree: true }] },
            {
                name: 'checkout',
                operands: [
                    { ...word, count: 'optional' },
                    { roles: ['write'], count: 'rest', tree: true },
                ],
            },
            { name: 'apply', operands: [files], input: file },
            { name: 'commit', values: { '-F': file } },
            { name: 'clone', operands: [host, { roles: ['create'], count: 'optional' }] },
        ],
        abbreviates: true,
    },
    { name: 'make', flags: [['optional', '-j', '--jobs']], values: { '-C': directory, '-f': file }, operands: [words], abbreviates: true },
    { name: 'ninja', flags: flag('one', '-C', '-f', '-j'), values: { '-C': directory, '-f': file }, operands: [words] },
    { name: 'cmake', flags: flag('one', '-S', '-B', '--build', '-P', '-D'), values: { '-S': file, '-B': folder, '--build': { ...edits, count: 'one', tree: true }, '-P': program } },
    { name: 'ctest', flags: [['one', '--test-dir']], values: { '--test-dir': directory } },
    { name: 'cpack', flags: flag('one', '--config', '-B'), values: { '--config': file, '-B': folder } },
    {
        name: 'clang-format',
        flags: [
            ['none', '-i'],
            ['one', '--style'],
        ],
        operands: rewrite(files, '-i'),
        input: file,
        output: { ...stdout, unless: ['-i'] },
    },
    { name: 'clang-tidy', flags: [...flag('none', '--fix', '--fix-errors'), ['one', '-p']], values: { '-p': file }, operands: rewrite(files, '--fix', '--fix-errors') },
    {
        name: 'gersemi',
        flags: [
            ['none', '-i', '--in-place'],
            ['none', '-c', '--check'],
            ['none', '--diff'],
        ],
        operands: rewrite(trees, '-i'),
        input: file,
    },
    { name: 'rustfmt', flags: [['none', '--check'], ...flag('one', '--edition', '--config-path')], operands: toggle(edits, files, '--check'), input: file, output: stdout },
    { name: 'cargo-fmt', ...format },
    { name: 'cargo', commands: [{ name: 'fmt', ...format }] },
    { name: 'swiftlint', ...lint, commands: [{ name: 'lint', ...lint }] },
    {
        name: 'ruff',
        inherited: flag('one', '--cache-dir', '--config'),
        commands: [
            { name: 'check', flags: [...flag('none', '--fix', '--fix-only', '--diff'), ['one', '-o', '--output-file']], values: { '-o': target }, operands: repair('--fix', '--fix-only') },
            { name: 'format', flags: flag('none', '--check', '--diff'), operands: toggle({ ...edits, tree: true }, trees, '--check', '--diff') },
        ],
    },
    { name: 'ryl', flags: [...flag('none', '--fix', '--diff'), ['one', '-o', '--output-file']], values: { '-o': target }, operands: repair('--fix') },
    {
        name: 'ast-grep',
        aliases: ['sg'],
        commands: [
            {
                name: 'run',
                flags: [
                    ['one', '-p', '--pattern'],
                    ['one', '-r', '--rewrite'],
                    ['none', '-U', '--update-all'],
                    ['none', '-i', '--interactive'],
                ],
                operands: rewrite(trees, '-U', '-i'),
            },
            {
                name: 'scan',
                flags: [
                    ['none', '-U', '--update-all'],
                    ['none', '-i', '--interactive'],
                ],
                operands: rewrite(trees, '-U', '-i'),
            },
        ],
    },
    { name: 'mypy', aliases: ['refurb', 'pytest', 'ty'], operands: [trees] },
    { name: 'mypy', aliases: ['refurb'], flags: flag('one', '--cache-dir', '--config-file', '--python-version') },
    { name: 'vitest', flags: [['one', '-c', '--config'], ['one', '-r', '--root'], ['one', '-t', '--testNamePattern'], ...flag('one', '--dir', '--exclude', '--mode', '--project', '--repeats', '--reporter', '--shard')] },
    {
        name: 'playwright',
        flags: [
            ['one', '-c', '--config'],
            ['one', '-g', '--grep'],
            ['one', '-G', '--grep-invert'],
            ['one', '-j', '--workers'],
            ...flag('one', '--add-reporter', '--browser', '--global-timeout', '--last-failed-file', '--max-failures', '--output', '--project', '--repeat-each', '--reporter', '--retries', '--shard', '--test-list', '--test-list-invert', '--timeout', '--trace', '--tsconfig'),
        ],
    },
    { name: 'analyzeHeadless', flags: flag('one', '-import', '-process', '-log', '-scriptlog', '-scriptPath', '-propertiesPath', '-preScript', '-postScript', '-processor', '-cspec', '-loader', '-analysisTimeoutPerFile', '-keystore', '-max-cpu', '-librarySearchPaths') },
    { name: 'stubgen', flags: [['one', '-o', '--output']], values: { '-o': folder }, operands: [files] },
    { name: 'tsc', values: { '-p': file, '--outDir': folder }, operands: [files] },
    { name: 'pulumi', values: { '-C': directory, '--cwd': directory } },
    {
        name: 'nx',
        flags: [
            ['one', '-c', '--configuration'],
            ['one', '-p', '--projects'],
            ['one', '-t', '--targets', '--target'],
            ['none', '--skip-sync', '--skipSync'],
            ['none', '--only-cache', '--onlyCache'],
            ['none', '--only-daemon', '--onlyDaemon'],
            ['none', '--only-workspace-data', '--onlyWorkspaceData'],
        ],
    },
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { signatures };
