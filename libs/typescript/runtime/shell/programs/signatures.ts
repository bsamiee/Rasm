import type { Usage } from '#/programs/usage';

// --- [CONSTANTS] -----------------------------------------------------------------------

const read = { roles: ['read'], count: 'rest' } as const;
const walked = { roles: ['read'], count: 'rest', tree: true } as const;
const plain = { roles: [], count: 'rest' } as const;
const inner = { roles: ['command'], count: 'rest' } as const;
const data = { roles: ['read'] } as const;
const printed = { roles: ['write'] } as const;
const written = { roles: ['create', 'write'] } as const;
const directory = { roles: ['directory'] } as const;
const pattern = (...given: readonly string[]) => ({ roles: [], count: 'one', unless: given }) as const;
const edited = (...given: readonly string[]) =>
    [
        { ...read, unless: given },
        { roles: ['read', 'write'], count: 'rest', when: given },
    ] as const;
const fixed = (...given: readonly string[]) =>
    [
        { ...walked, unless: given },
        { roles: ['read', 'write'], count: 'rest', tree: true, when: given },
    ] as const;
const recursive = (roles: readonly ('read' | 'write' | 'delete')[], ...given: readonly string[]) =>
    [
        { roles, count: 'rest', unless: given },
        { roles, count: 'rest', tree: true, when: given },
    ] as const;
const sourced = { roles: ['read', 'execute'], count: 'optional' } as const;
const signatures: readonly (typeof Usage.Encoded)[] = [
    { name: 'cat', operands: [read], input: data, output: printed, abbreviates: true },
    {
        name: 'cp',
        flags: [
            ['none', '-R', '-r', '--recursive'],
            ['one', '-t', '--target-directory'],
        ],
        values: { '-t': written },
        operands: [...recursive(['read'], '-R'), { ...written, count: 'one', unless: ['-t'] }],
        abbreviates: true,
    },
    {
        name: 'mv',
        flags: [['one', '-t', '--target-directory']],
        values: { '-t': written },
        operands: [
            { roles: ['read', 'delete'], count: 'rest', tree: true },
            { ...written, count: 'one', unless: ['-t'] },
        ],
        abbreviates: true,
    },
    {
        name: 'rm',
        flags: [['none', '-R', '-r', '--recursive']],
        operands: recursive(['delete'], '-R'),
        abbreviates: true,
    },
    { name: 'rmdir', operands: [{ roles: ['delete'], count: 'rest' }], abbreviates: true },
    {
        name: 'mkdir',
        operands: [{ roles: ['create'], count: 'rest' }],
        abbreviates: true,
    },
    {
        name: 'ln',
        flags: [
            ['none', '-s', '--symbolic'],
            ['one', '-t', '--target-directory'],
        ],
        values: { '-t': written },
        operands: [{ roles: ['read'], count: 'one' }, read, { ...written, count: 'optional', unless: ['-t'] }],
        abbreviates: true,
    },
    { name: 'touch', values: { '-r': data }, operands: [{ ...written, count: 'rest' }], abbreviates: true },
    { name: 'truncate', operands: [{ ...written, count: 'rest' }], abbreviates: true },
    { name: 'chmod', aliases: ['chown', 'chgrp'], flags: [['none', '-R', '--recursive']], operands: [{ roles: [], count: 'one' }, ...recursive(['write'], '-R')], abbreviates: true },
    { name: 'xattr', operands: [{ roles: ['read', 'write'], count: 'rest' }] },
    { name: 'ls', flags: [['none', '-R', '--recursive']], operands: recursive(['read'], '-R'), output: printed, abbreviates: true },
    { name: 'du', operands: [walked], output: printed, abbreviates: true },
    { name: 'stat', aliases: ['file', 'realpath', 'readlink', 'wait4path'], operands: [read], output: printed, abbreviates: true },
    { name: 'head', aliases: ['tail', 'wc', 'cut', 'nl', 'shasum', 'sha256sum', 'md5sum', 'md5', 'cksum', 'sum', 'strings', 'od', 'xxd', 'hexdump', 'rev', 'fold', 'paste', 'expand', 'unexpand', 'tac', 'more', 'less'], operands: [read], input: data, output: printed, abbreviates: true },
    { name: 'sort', flags: [['one', '-o', '--output']], values: { '-o': written }, operands: [read], input: data, output: { ...printed, unless: ['-o'] }, abbreviates: true },
    {
        name: 'uniq',
        operands: [
            { roles: ['read'], count: 'optional' },
            { ...written, count: 'optional' },
        ],
        input: data,
        output: printed,
        abbreviates: true,
    },
    {
        name: 'tr',
        operands: [
            { roles: [], count: 'one' },
            { roles: [], count: 'optional' },
        ],
        input: data,
        output: printed,
        abbreviates: true,
    },
    { name: 'tee', flags: [['none', '-a', '--append']], operands: [{ ...written, count: 'rest' }], input: data, output: printed, abbreviates: true },
    { name: 'shred', operands: [{ roles: ['write', 'delete'], count: 'rest' }], abbreviates: true },
    { name: 'trash', operands: [{ roles: ['delete'], count: 'rest', tree: true }] },
    { name: 'rimraf', operands: [{ roles: ['delete'], count: 'rest', tree: true }] },
    { name: 'mktemp', operands: [{ roles: [], count: 'optional' }], output: printed, abbreviates: true },
    { name: 'screencapture', operands: [{ ...written, count: 'rest' }] },
    {
        name: 'base64',
        flags: [
            ['one', '-i', '--input'],
            ['one', '-o', '--output'],
        ],
        values: { '-i': data, '-o': written },
        operands: [{ roles: ['read'], count: 'optional' }],
        input: { ...data, unless: ['-i'] },
        output: { ...printed, unless: ['-o'] },
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
        values: { '-o': written },
        operands: [
            { roles: ['read'], count: 'optional', unless: ['-e', '-i'] },
            { ...plain, when: ['-e'] },
        ],
        input: { ...data, unless: ['-e', '-i'] },
        output: { ...printed, unless: ['-o'] },
        abbreviates: true,
    },
    {
        name: 'sed',
        flags: [
            ['one', '-e', '--expression'],
            ['one', '-f', '--file'],
        ],
        values: { '-f': data },
        operands: [pattern('-e', '-f'), ...edited('-i', '--in-place')],
        input: data,
        output: { ...printed, unless: ['-i', '--in-place'] },
        abbreviates: true,
    },
    {
        name: 'sd',
        operands: [
            { roles: [], count: 'one' },
            { roles: [], count: 'one' },
            { roles: ['read', 'write'], count: 'rest', unless: ['-p'] },
            { ...read, when: ['-p'] },
        ],
        input: data,
        output: printed,
    },
    { name: 'awk', values: { '-f': { roles: ['read', 'execute'] } }, operands: [{ roles: ['code'], count: 'one', unless: ['-f'] }, read], input: data, output: printed },
    {
        name: 'grep',
        flags: [['none', '-R', '-r', '--recursive']],
        values: { '-f': data },
        operands: [pattern('-e', '-f'), ...recursive(['read'], '-R')],
        input: data,
        output: printed,
        abbreviates: true,
    },
    {
        name: 'rg',
        values: { '-f': data },
        operands: [pattern('-e', '-f', '--files'), walked],
        input: data,
        output: printed,
    },
    {
        name: 'fd',
        flags: [
            ['rest', '-x', '--exec'],
            ['rest', '-X', '--exec-batch'],
        ],
        values: { '-x': { roles: ['command'], ends: [';'] }, '-X': { roles: ['command'], ends: [';'] } },
        operands: [{ roles: [], count: 'optional' }, walked],
        output: printed,
    },
    {
        name: 'find',
        flags: [
            ['none', '-delete'],
            ['rest', '-exec'],
            ['rest', '-execdir'],
            ['rest', '-ok'],
            ['rest', '-okdir'],
            ['one', '-name'],
            ['one', '-iname'],
            ['one', '-path'],
            ['one', '-ipath'],
            ['one', '-regex'],
            ['one', '-iregex'],
            ['one', '-type'],
            ['one', '-maxdepth'],
            ['one', '-mindepth'],
            ['one', '-newer'],
            ['one', '-mtime'],
            ['one', '-mmin'],
            ['one', '-size'],
            ['one', '-perm'],
            ['one', '-user'],
            ['one', '-group'],
            ['none', '-print'],
            ['none', '-print0'],
            ['none', '-prune'],
            ['none', '-depth'],
            ['none', '-not'],
            ['none', '-a', '-and'],
            ['none', '-o', '-or'],
        ],
        values: { '-exec': { roles: ['command'], ends: [';', '+'] }, '-execdir': { roles: ['command'], ends: [';', '+'] }, '-ok': { roles: ['command'], ends: [';'] }, '-okdir': { roles: ['command'], ends: [';'] }, '-newer': data, '-f': { roles: ['read'], tree: true } },
        operands: [
            { ...walked, unless: ['-delete'] },
            { roles: ['read', 'delete'], count: 'rest', tree: true, when: ['-delete'] },
        ],
        output: printed,
    },
    {
        name: 'tar',
        flags: [
            ['none', '-c', '--create'],
            ['one', '-C', '--directory'],
        ],
        values: { '-C': directory },
        operands: [{ ...walked, when: ['-c'] }],
        abbreviates: true,
    },
    {
        name: 'patch',
        flags: [
            ['one', '-i', '--input'],
            ['one', '-d', '--directory'],
            ['one', '-o', '--output'],
        ],
        values: { '-i': data, '-d': directory, '-o': written },
        operands: [
            { roles: ['read', 'write'], count: 'optional' },
            { roles: ['read'], count: 'optional' },
        ],
        input: { ...data, unless: ['-i'] },
        abbreviates: true,
    },
    {
        name: 'curl',
        values: { '-o': written, '-T': data, '-K': data },
        operands: [{ roles: ['host'], count: 'rest' }],
        output: { ...printed, unless: ['-o', '-O'] },
    },
    { name: 'ping', operands: [{ roles: ['host'], count: 'one' }], output: printed },
    {
        name: 'nc',
        operands: [
            { roles: ['host'], count: 'one' },
            { roles: [], count: 'optional' },
        ],
        input: data,
        output: printed,
    },
    {
        name: 'ffmpeg',
        flags: [
            ['one', '-i'],
            ['one', '-t'],
        ],
        values: { '-i': data },
        operands: [{ ...written, count: 'rest' }],
    },
    { name: 'jq', flags: [['none', '-n', '--null-input']], values: { '-f': data }, operands: [pattern('-f'), read], input: { ...data, unless: ['-n'] }, output: printed },
    {
        name: 'yq',
        flags: [
            ['none', '-i', '--inplace'],
            ['none', '-n', '--null-input'],
            ['one', '--from-file'],
        ],
        values: { '--from-file': data },
        operands: [pattern('--from-file'), ...edited('-i')],
        input: { ...data, unless: ['-n'] },
        output: { ...printed, unless: ['-i'] },
    },
    {
        name: 'duckdb',
        aliases: ['sqlite3'],
        flags: [
            ['one', '-c'],
            ['one', '-s'],
            ['one', '-f'],
            ['one', '-cmd'],
            ['one', '-init'],
            ['none', '-no-stdin'],
        ],
        values: { '-c': { roles: ['code'] }, '-s': { roles: ['code'] }, '-cmd': { roles: ['code'] }, '-f': { roles: ['read', 'execute'] }, '-init': { roles: ['read', 'execute'] } },
        operands: [
            { roles: ['read', 'write'], count: 'optional' },
            { roles: ['code'], count: 'rest' },
        ],
        input: { roles: ['code'], unless: ['-c', '-s', '-f', '-no-stdin'] },
        output: printed,
    },
    {
        name: 'hyperfine',
        values: { '-p': { roles: ['script'] }, '-s': { roles: ['script'] }, '-c': { roles: ['script'] }, '--export-json': written },
        output: printed,
    },
    { name: 'printf', flags: [['one', '-v']], values: { '-v': { roles: ['assignment'] } }, operands: [plain], output: { ...printed, unless: ['-v'] } },
    { name: 'echo', operands: [plain], output: printed },
    {
        name: 'read',
        flags: [
            ['one', '-t'],
            ['one', '-p'],
            ['one', '-d'],
            ['one', '-n'],
            ['one', '-u'],
            ['one', '-a'],
            ['none', '-r'],
        ],
        values: { '-a': { roles: ['assignment'] } },
        operands: [{ roles: ['assignment'], count: 'rest' }],
        input: data,
    },
    { name: 'sleep', operands: [plain] },
    { name: 'eval', operands: [{ roles: ['script'], count: 'rest' }] },
    { name: 'source', aliases: ['.'], operands: [{ roles: ['read', 'execute'], count: 'one' }, plain], interspersed: false },
    {
        name: 'sh',
        aliases: ['bash', 'zsh', 'dash', 'ksh', 'mksh'],
        flags: [
            ['none', '-c'],
            ['none', '-s'],
            ['none', '-i'],
            ['none', '-l', '--login'],
            ['one', '-o'],
            ['none', '-e'],
            ['none', '-u'],
            ['none', '-x'],
            ['none', '-n'],
            ['none', '-v'],
        ],
        operands: [{ roles: ['shell'], count: 'one', when: ['-c'] }, { roles: [], count: 'optional', when: ['-c'] }, { ...sourced, unless: ['-c', '-s'] }, plain],
        input: { roles: ['shell'], unless: ['-c'] },
        interspersed: false,
    },
    {
        name: 'python',
        aliases: ['python3'],
        flags: [
            ['rest', '-c'],
            ['rest', '-m'],
        ],
        values: { '-c': { roles: ['code'] } },
        operands: [{ ...sourced, unless: ['-c', '-m'] }, plain],
        input: { roles: ['code'], unless: ['-c', '-m'] },
        interspersed: false,
    },
    {
        name: 'node',
        aliases: ['tsx'],
        flags: [
            ['one', '-e', '--eval'],
            ['one', '-p', '--print'],
        ],
        values: { '-e': { roles: ['code'] }, '-p': { roles: ['code'] } },
        operands: [{ ...sourced, unless: ['-e', '-p'] }, plain],
        input: { roles: ['code'], unless: ['-e', '-p'] },
        interspersed: false,
    },
    {
        name: 'perl',
        aliases: ['ruby'],
        flags: [
            ['one', '-e'],
            ['one', '-E'],
            ['optional', '-i'],
        ],
        values: { '-e': { roles: ['code'] }, '-E': { roles: ['code'] } },
        operands: [{ ...sourced, unless: ['-e', '-E'] }, ...edited('-i')],
        input: { roles: ['code'], unless: ['-e', '-E'] },
        interspersed: false,
    },
    {
        name: 'osascript',
        values: { '-e': { roles: ['code'] } },
        operands: [{ ...sourced, unless: ['-e'] }, plain],
        input: { roles: ['code'], unless: ['-e'] },
        interspersed: false,
    },
    {
        name: 'Blender',
        aliases: ['blender'],
        flags: [
            ['one', '-P', '--python'],
            ['one', '--python-expr'],
            ['none', '-b', '--background'],
        ],
        values: { '-P': { roles: ['read', 'execute'] }, '--python-expr': { roles: ['code'] } },
        operands: [{ roles: ['read', 'write'], count: 'optional' }],
    },
    {
        name: 'ssh',
        values: { '-l': { roles: ['user'] }, '-J': { roles: ['host'] }, '-F': data, '-i': data },
        operands: [
            { roles: ['host'], count: 'one' },
            { roles: ['script'], count: 'rest' },
        ],
        input: data,
        output: printed,
        interspersed: false,
    },
    {
        name: 'docker',
        commands: [
            { name: 'exec', values: { '-w': directory, '-u': { roles: ['user'] }, '-e': { roles: ['assignment'] } }, operands: [{ roles: ['container'], count: 'one' }, inner], interspersed: false },
            {
                name: 'run',
                flags: [
                    ['one', '-w', '--workdir'],
                    ['one', '-u', '--user'],
                    ['one', '-e', '--env'],
                    ['one', '-v', '--volume'],
                    ['none', '--rm'],
                ],
                values: { '-w': directory, '-u': { roles: ['user'] }, '-e': { roles: ['assignment'] } },
                operands: [{ roles: ['container'], count: 'one' }, inner],
                interspersed: false,
            },
        ],
    },
    { name: 'kubectl', commands: [{ name: 'exec', values: { '-c': { roles: ['container'] } }, operands: [{ roles: ['container'], count: 'one' }, inner] }] },
    {
        name: 'sudo',
        values: { '-u': { roles: ['user'] }, '-D': directory },
        operands: [
            { ...inner, unless: ['-s', '-i', '-e', '-l'] },
            { roles: ['script'], count: 'rest', when: ['-s', '-i'] },
            { roles: ['read', 'write'], count: 'rest', when: ['-e'] },
        ],
        interspersed: false,
    },
    {
        name: 'doas',
        values: { '-u': { roles: ['user'] }, '-C': data },
        operands: [{ ...inner, unless: ['-s'] }],
        interspersed: false,
    },
    {
        name: 'env',
        flags: [
            ['none', '-i', '--ignore-environment'],
            ['one', '-u', '--unset'],
            ['one', '-C', '--chdir'],
            ['one', '-S', '--split-string'],
        ],
        values: { '-i': { roles: ['unset'] }, '-u': { roles: ['unset'] }, '-C': directory },
        operands: [{ roles: ['assignment'], count: 'rest' }, inner],
        interspersed: false,
    },
    { name: 'xargs', flags: [['one', '-a', '--arg-file']], values: { '-I': { roles: ['placeholder'] }, '-J': { roles: ['placeholder'] }, '-a': data }, operands: [inner], input: { roles: ['command'], unless: ['-a'] }, interspersed: false },
    { name: 'parallel', operands: [inner], input: { roles: ['command'] }, interspersed: false },
    {
        name: 'watch',
        operands: [
            { roles: ['script'], count: 'rest', unless: ['-x'] },
            { ...inner, when: ['-x'] },
        ],
        interspersed: false,
    },
    { name: 'timeout', operands: [{ roles: [], count: 'one' }, inner], interspersed: false },
    { name: 'nohup', aliases: ['setsid', 'builtin', 'arch', 'caffeinate'], operands: [inner], interspersed: false },
    { name: 'nice', operands: [inner], interspersed: false },
    {
        name: 'stdbuf',
        flags: [
            ['one', '-i', '--input'],
            ['one', '-o', '--output'],
            ['one', '-e', '--error'],
        ],
        operands: [inner],
        interspersed: false,
    },
    {
        name: 'time',
        flags: [
            ['one', '-o', '--output'],
            ['none', '-a', '--append'],
            ['none', '-p'],
            ['none', '-l'],
            ['none', '-h'],
        ],
        values: { '-o': written },
        operands: [inner],
        interspersed: false,
    },
    {
        name: 'exec',
        flags: [
            ['none', '-c'],
            ['none', '-l'],
            ['one', '-a'],
        ],
        operands: [inner],
        interspersed: false,
    },
    {
        name: 'command',
        flags: [
            ['none', '-p'],
            ['none', '-v'],
            ['none', '-V'],
        ],
        operands: [
            { ...inner, unless: ['-v', '-V'] },
            { ...plain, when: ['-v', '-V'] },
        ],
        interspersed: false,
    },
    {
        name: 'lockf',
        flags: [
            ['one', '-t'],
            ['none', '-k'],
            ['none', '-n'],
            ['none', '-s'],
        ],
        operands: [{ ...written, count: 'one' }, inner],
        interspersed: false,
    },
    {
        name: 'xcrun',
        operands: [
            { ...inner, unless: ['-f'] },
            { ...plain, when: ['-f'] },
        ],
        interspersed: false,
    },
    {
        name: 'npx',
        flags: [
            ['one', '-p', '--package'],
            ['none', '-y', '--yes'],
            ['one', '-c', '--call'],
        ],
        values: { '-c': { roles: ['script'] } },
        operands: [{ ...inner, unless: ['-c'] }],
        interspersed: false,
    },
    {
        name: 'uvx',
        flags: [
            ['one', '--from'],
            ['one', '-w', '--with'],
            ['one', '-p', '--python'],
        ],
        operands: [inner],
        interspersed: false,
    },
    {
        name: 'uv',
        commands: [
            { name: 'run', operands: [inner], interspersed: false },
            { name: 'tool', commands: [{ name: 'run', operands: [inner], interspersed: false }] },
        ],
    },
    {
        name: 'pnpm',
        commands: [
            { name: 'exec', operands: [inner], interspersed: false },
            { name: 'dlx', operands: [inner], interspersed: false },
        ],
    },
    { name: 'npm', commands: [{ name: 'exec', aliases: ['x'], operands: [inner], interspersed: false }] },
    { name: 'poetry', aliases: ['hatch'], commands: [{ name: 'run', operands: [inner], interspersed: false }] },
    {
        name: 'pixi',
        commands: [
            { name: 'run', operands: [inner], interspersed: false },
            { name: 'exec', operands: [inner], interspersed: false },
        ],
    },
    { name: 'mise', commands: [{ name: 'exec', aliases: ['x'], operands: [inner], interspersed: false }] },
    { name: 'doppler', aliases: ['op'], commands: [{ name: 'run', operands: [inner], interspersed: false }] },
    {
        name: 'git',
        values: { '-C': directory, '--git-dir': directory, '--work-tree': directory },
        commands: [
            { name: 'add', operands: [walked] },
            {
                name: 'rm',
                operands: [
                    { roles: ['delete'], count: 'rest', unless: ['-r', '--cached'] },
                    { roles: ['delete'], count: 'rest', tree: true, when: ['-r'], unless: ['--cached'] },
                    { ...read, when: ['--cached'] },
                ],
            },
            {
                name: 'mv',
                operands: [
                    { roles: ['read', 'delete'], count: 'rest', tree: true },
                    { ...written, count: 'one' },
                ],
            },
            { name: 'clean', operands: [{ roles: ['delete'], count: 'rest', tree: true }] },
            { name: 'restore', operands: [{ roles: ['write'], count: 'rest', tree: true }] },
            {
                name: 'checkout',
                operands: [
                    { roles: [], count: 'optional' },
                    { roles: ['write'], count: 'rest', tree: true },
                ],
            },
            { name: 'apply', operands: [read], input: data },
            { name: 'commit', values: { '-F': data } },
            {
                name: 'clone',
                operands: [
                    { roles: ['host'], count: 'one' },
                    { roles: ['create'], count: 'optional' },
                ],
            },
        ],
        abbreviates: true,
    },
    {
        name: 'make',
        values: { '-C': directory, '-f': data },
        operands: [plain],
        abbreviates: true,
    },
    {
        name: 'ninja',
        flags: [
            ['one', '-C'],
            ['one', '-f'],
        ],
        values: { '-C': directory, '-f': data },
        operands: [plain],
    },
    {
        name: 'cmake',
        flags: [
            ['one', '-S'],
            ['one', '-B'],
            ['one', '--build'],
            ['one', '-P'],
            ['one', '-D'],
        ],
        values: { '-S': data, '-B': { roles: ['create', 'write'], tree: true }, '--build': { roles: ['read', 'write'], tree: true }, '-P': { roles: ['read', 'execute'] } },
    },
    { name: 'ctest', flags: [['one', '--test-dir']], values: { '--test-dir': directory } },
    {
        name: 'cpack',
        flags: [
            ['one', '--config'],
            ['one', '-B'],
        ],
        values: { '--config': data, '-B': { roles: ['create', 'write'], tree: true } },
    },
    {
        name: 'clang-format',
        flags: [
            ['none', '-i'],
            ['one', '--style'],
        ],
        operands: edited('-i'),
        input: data,
        output: { ...printed, unless: ['-i'] },
    },
    {
        name: 'clang-tidy',
        flags: [
            ['none', '--fix'],
            ['none', '--fix-errors'],
            ['one', '-p'],
        ],
        values: { '-p': data },
        operands: edited('--fix', '--fix-errors'),
    },
    {
        name: 'gersemi',
        flags: [
            ['none', '-i', '--in-place'],
            ['none', '-c', '--check'],
            ['none', '--diff'],
        ],
        operands: fixed('-i'),
        input: data,
    },
    {
        name: 'rustfmt',
        flags: [
            ['none', '--check'],
            ['one', '--edition'],
            ['one', '--config-path'],
        ],
        operands: [
            { roles: ['read', 'write'], count: 'rest', unless: ['--check'] },
            { ...read, when: ['--check'] },
        ],
        input: data,
        output: printed,
    },
    {
        name: 'cargo-fmt',
        values: { '--manifest-path': data },
    },
    { name: 'swiftlint', flags: [['none', '--fix', '--autocorrect']], operands: fixed('--fix'), commands: [{ name: 'lint', flags: [['none', '--fix', '--autocorrect']], operands: fixed('--fix') }] },
    {
        name: 'ruff',
        commands: [
            {
                name: 'check',
                flags: [
                    ['none', '--fix'],
                    ['none', '--fix-only'],
                    ['none', '--diff'],
                    ['one', '-o', '--output-file'],
                ],
                values: { '-o': written },
                operands: [
                    { ...walked, unless: ['--fix', '--fix-only'] },
                    { roles: ['read', 'write'], count: 'rest', tree: true, when: ['--fix', '--fix-only'], unless: ['--diff'] },
                    { ...walked, when: ['--diff'] },
                ],
            },
            {
                name: 'format',
                flags: [
                    ['none', '--check'],
                    ['none', '--diff'],
                ],
                operands: [
                    { roles: ['read', 'write'], count: 'rest', tree: true, unless: ['--check', '--diff'] },
                    { ...walked, when: ['--check', '--diff'] },
                ],
            },
        ],
    },
    {
        name: 'ryl',
        flags: [
            ['none', '--fix'],
            ['none', '--diff'],
            ['one', '-o', '--output-file'],
        ],
        values: { '-o': written },
        operands: [
            { ...walked, unless: ['--fix'] },
            { roles: ['read', 'write'], count: 'rest', tree: true, when: ['--fix'], unless: ['--diff'] },
            { ...walked, when: ['--diff'] },
        ],
    },
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
                operands: fixed('-U', '-i'),
            },
            {
                name: 'scan',
                flags: [
                    ['none', '-U', '--update-all'],
                    ['none', '-i', '--interactive'],
                ],
                operands: fixed('-U', '-i'),
            },
        ],
    },
    { name: 'mypy', aliases: ['refurb', 'pytest', 'ty'], operands: [walked] },
    { name: 'stubgen', flags: [['one', '-o', '--output']], values: { '-o': { roles: ['create', 'write'], tree: true } }, operands: [read] },
    {
        name: 'tsc',
        values: { '-p': data, '--outDir': { roles: ['create', 'write'], tree: true } },
        operands: [read],
    },
    { name: 'pulumi', values: { '-C': directory, '--cwd': directory } },
    {
        name: 'nx',
        flags: [
            ['one', '-c', '--configuration'],
            ['one', '-p', '--projects'],
            ['one', '-t', '--targets', '--target'],
        ],
    },
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { signatures };
