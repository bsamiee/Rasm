import { Array, Cause, Data, Duration, Effect, Function, Option, Predicate, Record, Stream, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { type CallExpr, type File, LangVariant, type Word } from 'sh-syntax';
import { unquote } from '#/evaluation/expansion';
import { ShellError } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Setting = Predicate.Predicate<ReadonlySet<string>>;
type Pair = readonly [name: string, value: string];

// --- [CONSTANTS] -----------------------------------------------------------------------

const identifier = /^[A-Za-z_]\w*$/u;
const sentinel = { format: String.raw`\0rasm\n\0`, text: '\0rasm\n\0' } as const;
const report = `[ -z "$1" ] || . "$1"
printf "$2"
shift 2
printf 'shell\\0%s\\0\\0' "\${ZSH_VERSION:+zsh}\${BASH_VERSION:+bash}"
while [ -n "$1" ]; do printf 'feature\\0%s\\0%s\\0' "$1" "$( (eval "$1") 2>/dev/null )"; shift; done
shift
while [ -n "$1" ]; do case $(command -v -- "$1" 2>/dev/null) in /*|'') ;; *) printf 'builtin\\0%s\\0\\0' "$1" ;; esac; shift; done
shift
case \${ZSH_VERSION:+z} in
z) eval '
printf "option\\0%s\\0%s\\0" "\${(@kv)options}"
printf "alias\\0%s\\0%s\\0" "\${(@kv)aliases}"
printf "global\\0%s\\0%s\\0" "\${(@kv)galiases}"
printf "suffix\\0%s\\0%s\\0" "\${(@kv)saliases}"
printf "variable\\0%s\\0%s\\0" "\${(@kv)parameters}"
while [ "$#" -gt 0 ]; do printf "value\\0%s\\0%s\\0" "$1" "\${(P)+1}\${\${(P)1-}//\${(#):-0}}"; shift; done
' ;;
*) printf 'option\\0\\0'; { set -o; shopt; } 2>/dev/null; printf '\\0variable\\0\\0'; set; printf '\\0alias\\0\\0'; alias; printf '\\0'
while [ "$#" -gt 0 ]; do eval "printf 'value\\\\0%s\\\\0%s\\\\0' \\"\\$1\\" \\"\\\${$1+1}\\\${$1-}\\""; shift; done ;;
esac`;
const features = {
    casing: ['x=aB; printf %s "${x^}"', 'AB'],
    steps: ['printf %s {1..5..2}', '135'],
    padding: ['printf %s {01..02}', '0102'],
    unicode: [String.raw`printf %s $'\u0041'`, 'A'],
    negative: ['x=abc; printf %s "${x:0:-1}"', 'ab'],
    transforms: ['x=a; printf %s "${x@Q}"', "'a'"],
    nulls: [`setopt shwordsplit 2>/dev/null; x=' '; printf '<%s>' a\${x}""`, '<a><>'],
    leadingnulls: [`setopt shwordsplit 2>/dev/null; x=' '; printf '<%s>' ""\${x}a`, '<><a>'],
    quotedarithmetic: ['printf %s $(( "1" + 1 ))', '2'],
} as const;
const shells = {
    zsh: { variant: LangVariant.LangZsh, names: 'zsh', implied: [] },
    bash: { variant: LangVariant.LangBash, names: 'sh', implied: [] },
    sh: { variant: LangVariant.LangPOSIX, names: 'sh', implied: ['posix'] },
} as const;
const on =
    (name: string): Setting =>
    (set) =>
        set.has(name);
const off = Function.flow(on, Predicate.not);
const options = {
    braceexpand: { zsh: on('braceexpand'), sh: on('braceexpand') },
    braceccl: { zsh: on('braceccl'), sh: Function.constFalse },
    dotglob: { zsh: on('globdots'), sh: on('dotglob') },
    equals: { zsh: on('equals'), sh: Function.constFalse },
    extendedglob: { zsh: on('extendedglob'), sh: Function.constFalse },
    glob: { zsh: on('glob'), sh: off('noglob') },
    globskipdots: { zsh: Function.constTrue, sh: on('globskipdots') },
    globstar: { zsh: Function.constFalse, sh: on('globstar') },
    globsubst: { zsh: on('globsubst'), sh: Function.constTrue },
    ksharrays: { zsh: on('ksharrays'), sh: Function.constTrue },
    multios: { zsh: on('multios'), sh: Function.constFalse },
    nocaseglob: { zsh: off('caseglob'), sh: on('nocaseglob') },
    nomatch: { zsh: on('nomatch'), sh: on('failglob') },
    nounset: { zsh: off('unset'), sh: on('nounset') },
    nullglob: { zsh: Predicate.or(on('nullglob'), on('cshnullglob')), sh: on('nullglob') },
    allexport: { zsh: on('allexport'), sh: on('allexport') },
    lastpipe: { zsh: Function.constTrue, sh: on('lastpipe') },
    aliases: { zsh: on('aliases'), sh: Predicate.or(on('expand_aliases'), on('posix')) },
    octalzeroes: { zsh: on('octalzeroes'), sh: Function.constTrue },
    patsub: { zsh: Function.constFalse, sh: on('patsub_replacement') },
    pipefail: { zsh: on('pipefail'), sh: on('pipefail') },
    posix: { zsh: Function.constFalse, sh: on('posix') },
    wordsplit: { zsh: on('shwordsplit'), sh: Function.constTrue },
    literalbraces: { zsh: off('braceexpand'), sh: Function.constTrue },
    literalregex: { zsh: Function.constFalse, sh: Function.constTrue },
} as const satisfies Record.ReadonlyRecord<string, Record.ReadonlyRecord<(typeof shells)[keyof typeof shells]['names'], Setting>>;

// --- [MODELS] --------------------------------------------------------------------------

class Dialect extends Data.Class<{
    readonly binary: string;
    readonly variant: LangVariant;
    readonly environment: Readonly<Record<string, string>>;
    readonly features: ReadonlySet<keyof typeof features>;
    readonly options: Readonly<Record<keyof typeof options, boolean>>;
    readonly builtins: ReadonlySet<string>;
    readonly aliases: ReadonlyMap<string, { readonly text: string; readonly global: boolean }>;
    readonly suffixes: ReadonlyMap<string, string>;
    readonly variables: ReadonlyMap<string, Option.Option<string>>;
    readonly parameters: ReadonlyMap<string, ReadonlySet<string>>;
}> {
    supports(feature: keyof typeof features): boolean {
        return this.features.has(feature);
    }

    alias(name: string, command: boolean): Option.Option<string> {
        const named = Option.filter(Option.fromUndefinedOr(this.aliases.get(name)), ({ global }) => global || command);
        const suffixed = Option.filter(Option.fromUndefinedOr(this.suffixes.get(name.slice(name.lastIndexOf('.') + 1))), () => command && name.includes('.'));
        const substituted = Option.orElse(Option.map(named, Struct.get('text')), () => Option.map(suffixed, (program) => `${program} ${name}`));
        return Option.filter(substituted, () => this.options.aliases);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const optionsOf = (shell: keyof typeof shells, enabled: Iterable<string>): Dialect['options'] => {
    const reported = new Set([...enabled, ...shells[shell].implied]);
    return Record.map(options, (settings) => settings[shells[shell].names](reported));
};
const posix = (binary: string, environment: Readonly<Record<string, string>>) => new Dialect({ binary, variant: shells.sh.variant, environment, features: new Set(), options: optionsOf('sh', []), builtins: new Set(), aliases: new Map(), suffixes: new Map(), variables: new Map(), parameters: new Map() });
const substitution =
    (global: boolean) =>
    ([name, substitute]: Pair) =>
        [name, { text: substitute, global }] as const;
const listings: Readonly<Record<string, (call: CallExpr, literal: (word: Word | null | undefined) => Option.Option<string>) => readonly Pair[]>> = {
    option: ({ args }, literal) => Option.toArray(Option.filter(Option.all([literal(args[0]), literal(args[1])]), () => args.length === 2)),
    variable: ({ assigns }) => assigns.flatMap(({ name }) => (name === null ? [] : [[name.value, '']])),
    alias: ({ args, assigns }, literal) => {
        const assigned = assigns.map(({ name, value }) => Option.map(literal(value), (written): Pair => [name?.value ?? '', written]));
        const spelled = Option.map(literal(args[1]), (pair): Pair => [pair.slice(0, pair.indexOf('=')), pair.slice(pair.indexOf('=') + 1)]);
        return Option.toArray(Option.filter(Option.firstSomeOf([...assigned, spelled]), ([name]) => identifier.test(name)));
    },
};
const ask = Effect.fnUntraced(function* (at: { readonly shell: string; readonly environment: Readonly<Record<string, string>>; readonly source?: string; readonly timeout?: Duration.Input }, builtins: readonly string[]) {
    const names = Array.dedupe(['IFS', 'OLDPWD', ...Record.keys(at.environment).filter((name) => identifier.test(name))]);
    const snippets = Record.values(features).map(([snippet]) => snippet);
    const handle = yield* ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.spawn(ChildProcess.make(at.shell, ['-c', report, at.shell, at.source ?? '', sentinel.format, ...snippets, '', ...builtins, '', ...names], { env: at.environment, extendEnv: false, stdin: 'ignore' })));
    const [output, refused, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    yield* Effect.when(new Cause.UnknownError(refused, `${at.shell} exited with ${code}`), Effect.succeed(code !== 0));
    const found = Option.liftPredicate(output.lastIndexOf(sentinel.text), (offset) => offset >= 0);
    return yield* Effect.fromOption(Option.map(found, (offset) => output.slice(offset + sentinel.text.length)));
}, Effect.scoped);
const probe = Effect.fnUntraced(function* (at: Parameters<typeof ask>[0], builtins: readonly string[], parse: (source: string, dialect: Dialect) => Effect.Effect<File, ShellError>) {
    const printed = yield* ask(at, builtins).pipe(Effect.timeoutOrElse({ duration: at.timeout ?? Duration.infinity, orElse: () => new Cause.TimeoutError(`${at.shell} printed no probe in time`) }), Effect.mapError(ShellError.of({ _tag: 'Host' }, at.shell)));
    const records = Array.chunksOf(printed.split('\0'), 3).filter(Predicate.isTupleOf(3));
    const stated = Array.groupBy(records, ([kind]) => kind);
    const shell = Option.getOrElse(
        Array.findFirst(Struct.keys(shells), (name) => (stated['shell'] ?? []).some(([, held]) => held === name)),
        () => 'sh' as const,
    );
    const outputs = new Map((stated['feature'] ?? []).map(([, snippet, output]) => [snippet, output]));
    const measured = Struct.keys(features).filter((feature) => outputs.get(features[feature][0]) === features[feature][1]);
    const plain = new Dialect({ ...posix(at.shell, at.environment), variant: shells[shell].variant, features: new Set(measured) });
    const literal = (word: Word | null | undefined) => Option.flatMap(Option.fromNullishOr(word), (held) => unquote(held, plain));
    const list = (kind: string, file: File) => file.stmts.flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? (listings[kind]?.(cmd, literal) ?? []) : []));
    const listed = yield* Effect.forEach(records, ([kind, name, value]) => (name === '' ? Effect.map(parse(value, plain), (file) => list(kind, file)) : Effect.succeed<readonly Pair[]>([[name, value]])));
    const {
        option = [],
        builtin = [],
        alias = [],
        global = [],
        suffix = [],
        variable = [],
        value = [],
    } = Record.map(
        Array.groupBy(Array.zip(records, listed), ([[kind]]) => kind),
        Array.flatMap(([, pairs]) => pairs),
    );
    const values = new Map(value.flatMap(([name, held]) => (held.startsWith('1') ? [[name, held.slice(1)] as const] : [])));
    return new Dialect({
        ...plain,
        options: optionsOf(
            shell,
            option.flatMap(([name, state]) => (state === 'on' ? [name] : [])),
        ),
        builtins: new Set(builtin.map(([name]) => name)),
        aliases: new Map([...alias.map(substitution(false)), ...global.map(substitution(true))]),
        suffixes: new Map(suffix),
        variables: new Map([...variable.map(([name]) => name), ...values.keys()].map((name) => [name, Option.fromUndefinedOr(values.get(name))])),
        parameters: new Map(variable.flatMap(([name, kind]) => (kind === '' ? [] : [[name, new Set(kind.split('-'))] as const]))),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dialect, identifier, posix, probe };
