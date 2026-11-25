import { Array, Cause, Data, type Duration, Effect, Option, Record, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { type CallExpr, type File, LangVariant, type Word } from 'sh-syntax';
import { text } from '#/evaluation/expansion';
import { ShellError } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Options = ReturnType<typeof bash>;
type Kind = 'variant' | 'feature' | 'value' | 'builtin' | 'option' | 'alias' | 'global' | 'suffix' | 'variable';

// --- [CONSTANTS] -----------------------------------------------------------------------

const sentinel = '\0rasm\n\0';
const identifier = /^[A-Za-z_]\w*$/u;
const features = {
    casing: ['x=aB; printf %s "${x^}"', 'AB'],
    steps: ['printf %s {1..5..2}', '135'],
    padding: ['printf %s {01..02}', '0102'],
    unicode: [String.raw`printf %s $'\u0041'`, 'A'],
    negative: ['x=abc; printf %s "${x:0:-1}"', 'ab'],
    transforms: ['x=a; printf %s "${x@Q}"', "'a'"],
    nulls: [`setopt shwordsplit 2>/dev/null; x=' '; printf '<%s>' a\${x}""`, '<a><>'],
} as const;
const variants: Readonly<Record<string, LangVariant>> = { zsh: LangVariant.LangZsh, bash: LangVariant.LangBash };
const tables = [
    ['option', 'options'],
    ['alias', 'aliases'],
    ['global', 'galiases'],
    ['suffix', 'saliases'],
] as const;
const assigned = (line: string) => Option.liftPredicate([line.slice(0, line.indexOf('=')), line.slice(line.indexOf('=') + 1)] as const, ([name]) => identifier.test(name));
const record = (kind: Kind, name: string, value: string) => `printf '${kind}\\0%s\\0%s\\0' ${name} ${value}`;
const probe = (names: readonly string[], builtins: readonly string[]) =>
    [
        '[ -n "$1" ] && . "$1"; shift',
        `printf '${sentinel.replaceAll('\0', String.raw`\0`).replace('\n', String.raw`\n`)}'`,
        record('variant', 'shell', '"${ZSH_VERSION:+zsh}${BASH_VERSION:+bash}"'),
        `for f; do ${record('feature', '"$f"', '"$( (eval "$f") 2>/dev/null )"')}; done`,
        `for n in ${builtins.map((name) => `'${name}'`).join(' ')}; do case $(command -v -- "$n" 2>/dev/null) in /*|'') ;; *) ${record('builtin', '"$n"', "''")} ;; esac; done`,
        `case \${ZSH_VERSION:+z} in z) eval '${[
            ...names.map((name) => record('value', name, `"\${${name}+1}\${\${${name}-}//\${(#):-0}}"`)),
            ...tables.map(([kind, table]) => `for k v in "\${(@kv)${table}}"; do ${record(kind, '"$k"', '"$v"')}; done`),
            `for k v in "\${(@kv)parameters}"; do ${record('variable', '"$k"', '"$v"')}; done`,
        ]
            .join('; ')
            .replaceAll("'", '"')}' ;;`,
        `*) ${names.map((name) => record('value', name, `"\${${name}+1}\${${name}-}"`)).join('; ')}; printf 'option\\0\\0'; { set -o; shopt; } 2>/dev/null; printf '\\0variable\\0\\0'; set; printf '\\0alias\\0\\0'; alias; printf '\\0' ;; esac`,
    ].join('\n');
const zsh = (on: (name: string) => boolean): Options => ({
    braceexpand: on('braceexpand'),
    braceccl: on('braceccl'),
    dotglob: on('globdots'),
    equals: on('equals'),
    extendedglob: on('extendedglob'),
    glob: on('glob'),
    globskipdots: true,
    globstar: false,
    globsubst: on('globsubst'),
    ksharrays: on('ksharrays'),
    multios: on('multios'),
    nocaseglob: !on('caseglob'),
    nomatch: on('nomatch'),
    nounset: !on('unset'),
    nullglob: on('nullglob') || on('cshnullglob'),
    allexport: on('allexport'),
    lastpipe: true,
    aliases: on('aliases'),
    octalzeroes: on('octalzeroes'),
    patsub: false,
    pipefail: on('pipefail'),
    posix: false,
    wordsplit: on('shwordsplit'),
});
const bash = (on: (name: string) => boolean, posix: boolean) => ({
    braceexpand: on('braceexpand'),
    braceccl: false,
    dotglob: on('dotglob'),
    equals: false,
    extendedglob: false,
    glob: !on('noglob'),
    globskipdots: on('globskipdots'),
    globstar: on('globstar'),
    globsubst: true,
    ksharrays: true,
    multios: false,
    nocaseglob: on('nocaseglob'),
    nomatch: on('failglob'),
    nounset: on('nounset'),
    nullglob: on('nullglob'),
    allexport: on('allexport'),
    lastpipe: on('lastpipe'),
    aliases: on('expand_aliases') || posix,
    octalzeroes: true,
    patsub: on('patsub_replacement'),
    pipefail: on('pipefail'),
    posix,
    wordsplit: true,
});

// --- [MODELS] --------------------------------------------------------------------------

class Dialect extends Data.Class<{
    readonly binary: string;
    readonly variant: LangVariant;
    readonly environment: Readonly<Record<string, string>>;
    readonly features: ReadonlySet<keyof typeof features>;
    readonly options: Options;
    readonly builtins: ReadonlySet<string>;
    readonly aliases: ReadonlyMap<string, { readonly text: string; readonly global: boolean }>;
    readonly suffixes: ReadonlyMap<string, string>;
    readonly variables: ReadonlyMap<string, Option.Option<string>>;
    readonly parameters: ReadonlyMap<string, ReadonlySet<string>>;
}> {
    supports(feature: keyof typeof features): boolean {
        return this.features.has(feature);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const posix = (binary: string, environment: Readonly<Record<string, string>>) => new Dialect({ binary, variant: LangVariant.LangPOSIX, environment, features: new Set(), options: bash(() => false, true), builtins: new Set(), aliases: new Map(), suffixes: new Map(), variables: new Map(), parameters: new Map() });

const probed = Effect.fnUntraced(function* (at: { readonly shell: string; readonly environment: Readonly<Record<string, string>>; readonly source?: string; readonly timeout?: Duration.Input }, builtins: readonly string[], parse: (source: string, dialect: Dialect) => Effect.Effect<File, ShellError>) {
    const names = Array.dedupe(['IFS', 'OLDPWD', ...Record.keys(at.environment).filter((name) => identifier.test(name))]);
    const snippets = Record.toEntries(features);
    const command = ChildProcess.make(at.shell, ['-c', probe(names, builtins), at.shell, at.source ?? '', ...snippets.map(([, [snippet]]) => snippet)], { env: at.environment, extendEnv: false, stdin: 'ignore' });
    const spawned = Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* (yield* ChildProcessSpawner.ChildProcessSpawner).spawn(command);
            const [output, refused, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return yield* code === 0 ? Effect.succeed(output) : Effect.fail(new Cause.UnknownError(refused, `${at.shell} exited with ${code}`));
        }),
    );
    const printed = yield* (at.timeout === undefined ? spawned : Effect.timeoutOrElse(spawned, { duration: at.timeout, orElse: () => Effect.fail(new Cause.TimeoutError(`${at.shell} printed no probe in time`)) })).pipe(
        Effect.flatMap((output) =>
            Effect.fromOption(
                Option.map(
                    Option.liftPredicate(output.lastIndexOf(sentinel), (found) => found >= 0),
                    (found) => output.slice(found + sentinel.length),
                ),
            ),
        ),
        Effect.mapError(ShellError.of({ _tag: 'Host' }, at.shell)),
    );
    const records: readonly { readonly kind: string; readonly name: string; readonly value: string }[] = Array.chunksOf(printed.split('\0'), 3).flatMap(([kind, name = '', value]) => (value === undefined ? [] : [{ kind, name, value }]));
    const listed = (kind: Kind) => records.filter((held) => held.kind === kind);
    const variant = Option.getOrElse(
        Option.flatMap(Array.head(listed('variant')), ({ value }) => Record.get(variants, value)),
        () => LangVariant.LangPOSIX,
    );
    const measured = new Set(snippets.flatMap(([feature, [snippet, expected]]) => (listed('feature').some(({ name, value }) => name === snippet && value === expected) ? [feature] : [])));
    const plain = new Dialect({ ...posix(at.shell, at.environment), variant, features: measured });
    const said = (word: Word | undefined) => Option.flatMap(Option.fromUndefinedOr(word), (held) => text(held, plain));
    const decoders: Readonly<Record<string, (call: CallExpr) => readonly (readonly [string, string])[]>> = {
        option: ({ args }) => Option.toArray(Option.filter(Option.all([said(args[0]), said(args[1])]), ([, state]) => args.length === 2 && (state === 'on' || state === 'off'))),
        variable: ({ assigns }) => assigns.flatMap(({ name }) => (name === null ? [] : [[name.value, ''] as const])),
        alias: ({ args, assigns }) => Option.toArray(Option.flatMap(Option.firstSomeOf([...assigns.map(({ name, value }) => Option.map(said(value ?? undefined), (written) => `${name?.value ?? ''}=${written}`)), said(args[1])]), assigned)),
    };
    const blobs = yield* Effect.forEach(
        records.filter(({ name }) => name === ''),
        ({ kind, value }) => Effect.map(parse(value, plain), (file) => ({ kind, file })),
    );
    const decoded = blobs.flatMap(({ kind, file }) => file.stmts.flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? (decoders[kind] ?? Array.empty)(cmd) : [])).map(([name, value]) => ({ kind, name, value })));
    const all = [...records.filter(({ name }) => name !== ''), ...decoded];
    const pairs = (kind: Kind) => all.filter((held) => held.kind === kind).map(({ name, value }) => [name, value] as const);
    const enabled = new Set(pairs('option').flatMap(([name, state]) => (state === 'on' ? [name] : [])));
    const on = (name: string) => enabled.has(name);
    const values = new Map(pairs('value').flatMap(([name, value]) => (value.startsWith('1') ? [[name, value.slice(1)] as const] : [])));
    return new Dialect({
        ...plain,
        options: variant === LangVariant.LangZsh ? zsh(on) : bash(on, variant === LangVariant.LangPOSIX || on('posix')),
        builtins: new Set(pairs('builtin').map(([name]) => name)),
        aliases: new Map((['alias', 'global'] as const).flatMap((kind) => pairs(kind).map(([name, value]) => [name, { text: value, global: kind === 'global' }] as const))),
        suffixes: new Map(pairs('suffix')),
        variables: new Map([...pairs('variable').map(([name]) => name), ...values.keys()].map((name) => [name, Option.fromUndefinedOr(values.get(name))])),
        parameters: new Map(variant === LangVariant.LangZsh ? pairs('variable').map(([name, kind]) => [name, new Set(kind.split('-'))] as const) : []),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dialect, posix, probed };
