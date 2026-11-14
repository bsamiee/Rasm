// --- [TYPES] ---------------------------------------------------------------------------

interface Program {
    readonly aliases?: readonly string[];
    readonly flags?: readonly `-${string}`[];
    readonly completer?: string;
    readonly dumpsHelp?: true;
    readonly strict?: true;
    readonly camelCase?: true;
    readonly stopsAtOperand?: true | readonly string[];
    readonly restAfter?: readonly string[];
    readonly informational?: readonly string[];
    readonly leading?: number;
    readonly leadingUnless?: readonly string[];
    readonly wraps?: readonly (readonly string[])[] | '--';
    readonly module?: string;
    readonly execs?: readonly string[];
    readonly shell?: readonly string[];
    readonly concatenates?: true;
    readonly code?: readonly string[];
    readonly reads?: true;
    readonly readsUnless?: readonly string[];
    readonly readsNothing?: RegExp;
    readonly recursive?: { readonly when?: readonly string[]; readonly roots?: readonly string[]; readonly base?: readonly string[]; readonly until?: readonly string[]; readonly names?: readonly string[]; readonly stats?: true };
    readonly closesInput?: true | readonly string[];
    readonly pipefail?: true;
    readonly launcher?: true;
    readonly headless?: readonly string[];
    readonly switches?: readonly string[];
    readonly nx?: { readonly selects?: readonly string[]; readonly targets?: Readonly<Record<string, string>>; readonly ignores?: readonly string[] };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCH_APPS = 'watch-apps';
const RTK = 'rtk';
const PROGRAMS: Readonly<Record<string, Program>> = {
    sudo: { aliases: ['doas'], wraps: [[]] },
    env: { wraps: [[]] },
    command: { informational: ['-v'], wraps: [[]] },
    builtin: { wraps: [[]] },
    nohup: { aliases: ['setsid', 'nice', 'stdbuf', 'exec', 'arch', 'xcrun'], wraps: [[]] },
    [RTK]: { wraps: [[], ['bypass']] },
    npx: { wraps: [[]] },
    time: { flags: ['-o='], wraps: [[]] },
    xargs: { flags: ['-a='], reads: true, readsUnless: ['-a'], wraps: [[]] },
    lockf: { flags: ['-t='], leading: 1, wraps: [[]] },
    flock: { flags: ['-w, --wait=', '--timeout=', '-E, --conflict-exit-code='], leading: 1, wraps: [[]] },
    npm: { wraps: [['exec'], ['x']] },
    pnpm: { wraps: [[], ['exec'], ['dlx']] },
    'effect-tsgo': { flags: ['--completions=', '--log-level='], wraps: [[]] },
    swift: { completer: 'swift@common', flags: ['--configuration='] },
    uv: { wraps: [['run'], ['tool', 'run']] },
    poetry: { aliases: ['hatch'], wraps: [['run']] },
    pixi: { flags: ['-e, --environment=', '--manifest-path=', '--locked', '--frozen'], wraps: [['run'], ['exec']] },
    mise: { wraps: '--' },
    doppler: { aliases: ['op'], wraps: '--' },
    head: { reads: true },
    sort: { reads: true, readsUnless: ['--files0-from'] },
    uniq: { aliases: ['wc', 'cut', 'shasum'], reads: true },
    md5: { reads: true, readsUnless: ['-s'] },
    tr: { leading: 2, reads: true },
    tee: { aliases: ['pbcopy'], leading: Number.POSITIVE_INFINITY, reads: true },
    sed: { leading: 1, leadingUnless: ['-e', '-f'], reads: true },
    awk: { leading: 1, leadingUnless: ['-f'], reads: true, readsNothing: /^\s*BEGIN\s*\{(?:[^{}]|\{[^{}]*\})*\}\s*$/u },
    yq: { flags: ['-o, --output-format=', '-p, --input-format=', '-I, --indent=', '--from-file=', '-n, --null-input'], leading: 1, leadingUnless: ['--from-file'], reads: true, readsUnless: ['--null-input'] },
    node: { flags: ['--inspect', '--inspect-brk', '--inspect-wait'], stopsAtOperand: true, reads: true, readsUnless: ['--test', '--run'], code: ['--eval', '--print'], informational: ['-v', '--v8-options', '--check'] },
    perl: { flags: ['-I=', '-M=', '-m='], stopsAtOperand: true, reads: true, code: ['-e', '-E'], informational: ['-v'] },
    ruby: { flags: ['-I=', '-r=', '-C=', '-E='], stopsAtOperand: true, reads: true, code: ['-e'], informational: ['-v', '--copyright'] },
    eval: { concatenates: true },
    set: {},
    python: { aliases: ['python3'], stopsAtOperand: true, module: '-m', reads: true, readsUnless: ['-m'], code: ['-c'] },
    bash: { aliases: ['sh', 'zsh', 'dash', 'ksh'], stopsAtOperand: true, reads: true, shell: ['-c'], informational: ['-n', '--noexec'] },
    sqlite3: { leading: 1, reads: true, readsUnless: ['-c', '-s', '-f', '-no-stdin'] },
    duckdb: { flags: ['-cmd=', '-init=', '-storage-version='], leading: 1, reads: true, readsUnless: ['-c', '-s', '-f', '-no-stdin'] },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PROGRAMS, type Program, RTK, WATCH_APPS };
