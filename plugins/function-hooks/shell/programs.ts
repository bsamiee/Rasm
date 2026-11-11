import type { Fix, Policy } from '#/policies/fix';

// --- [TYPES] ---------------------------------------------------------------------------

type When = true | readonly string[];

interface Program {
    readonly aliases?: readonly string[];
    readonly flags?: readonly `-${string}`[];
    readonly closed?: true;
    readonly camelCase?: true;
    readonly stops?: true | readonly string[];
    readonly dump?: true;
    readonly completer?: string;
    readonly leading?: number;
    readonly leadingUnless?: readonly string[];
    readonly informational?: readonly string[];
    readonly wraps?: readonly (readonly string[])[] | '--';
    readonly launcher?: true;
    readonly module?: string;
    readonly execs?: readonly string[];
    readonly shell?: readonly string[];
    readonly joins?: true;
    readonly code?: readonly string[];
    readonly reads?: 'default' | 'always';
    readonly readsUnless?: readonly string[];
    readonly readsNothing?: RegExp;
    readonly walks?: { readonly when?: readonly string[]; readonly paths?: readonly string[]; readonly base?: readonly string[]; readonly before?: readonly string[]; readonly names?: readonly string[]; readonly stats?: true };
    readonly ends?: readonly string[];
    readonly timer?: When;
    readonly endless?: When;
    readonly closes?: When;
    readonly reports?: true;
    readonly routes?: { readonly selects?: readonly string[]; readonly modes?: Readonly<Record<string, string>>; readonly ignores?: readonly string[] };
    readonly headless?: readonly string[];
    readonly launching?: readonly string[];
    readonly fixes?: readonly Fix[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROGRAMS: Policy['programs'] = {
    sudo: { aliases: ['doas'], wraps: [[]] },
    env: { wraps: [[]] },
    command: { informational: ['-v'], wraps: [[]] },
    builtin: { wraps: [[]] },
    nohup: { aliases: ['setsid', 'nice', 'stdbuf', 'exec', 'arch', 'xcrun'], wraps: [[]] },
    npx: { wraps: [[]], launcher: true },
    time: { flags: ['-o='], wraps: [[]] },
    xargs: { flags: ['-a='], reads: 'always', readsUnless: ['-a'], wraps: [[]] },
    lockf: { flags: ['-t='], leading: 1, wraps: [[]] },
    flock: { flags: ['-w, --wait=', '--timeout=', '-E, --conflict-exit-code='], leading: 1, wraps: [[]] },
    npm: { wraps: [['exec'], ['x']], reports: true },
    pnpm: { wraps: [[], ['exec'], ['dlx']], launcher: true, reports: true },
    'effect-tsgo': { flags: ['--completions=', '--log-level='], wraps: [[]], launcher: true },
    dotnet: { aliases: ['xcodebuild'], reports: true },
    swift: { completer: 'swift@common', flags: ['--configuration='], reports: true },
    uv: { wraps: [['run'], ['tool', 'run']], reports: true },
    poetry: { aliases: ['hatch'], wraps: [['run']] },
    pixi: { wraps: [['run'], ['exec']] },
    mise: { wraps: '--', launcher: true },
    doppler: { aliases: ['op'], wraps: '--' },
    head: { reads: 'default', closes: true },
    sort: { reads: 'default', readsUnless: ['--files0-from'] },
    uniq: { aliases: ['wc', 'cut', 'shasum'], reads: 'default' },
    md5: { reads: 'default', readsUnless: ['-s'] },
    tr: { aliases: ['tee', 'pbcopy'], reads: 'always' },
    sed: { leading: 1, leadingUnless: ['-e', '-f'], reads: 'default' },
    awk: { leading: 1, leadingUnless: ['-f'], reads: 'default', readsNothing: /^\s*BEGIN\s*\{(?:[^{}]|\{[^{}]*\})*\}\s*$/u },
    yq: { flags: ['-o, --output-format=', '-p, --input-format=', '-I, --indent=', '--from-file=', '-n, --null-input'], leading: 1, leadingUnless: ['--from-file'], reads: 'default', readsUnless: ['--null-input'] },
    node: { flags: ['--inspect', '--inspect-brk', '--inspect-wait'], stops: true, reads: 'default', readsUnless: ['--test', '--run'], code: ['--eval', '--print'], informational: ['-v', '--v8-options', '--check'] },
    perl: { flags: ['-I=', '-M=', '-m='], stops: true, reads: 'default', code: ['-e', '-E'], informational: ['-v'] },
    ruby: { flags: ['-I=', '-r=', '-C=', '-E='], stops: true, reads: 'default', code: ['-e'], informational: ['-v', '--copyright'] },
    eval: { joins: true },
    set: {},
    python: { aliases: ['python3'], stops: true, module: '-m', reads: 'default', readsUnless: ['-m'], code: ['-c'] },
    bash: { aliases: ['sh', 'zsh', 'dash', 'ksh'], stops: true, reads: 'default', shell: ['-c'], informational: ['-n', '--noexec'] },
    sqlite3: { leading: 1, reads: 'default', readsUnless: ['-c', '-s', '-f', '-no-stdin'] },
    duckdb: { flags: ['-cmd=', '-init=', '-storage-version='], leading: 1, reads: 'default', readsUnless: ['-c', '-s', '-f', '-no-stdin'] },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PROGRAMS, type Program };
