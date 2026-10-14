import { spawnSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';
import { devNull } from 'node:os';
import { basename, join, resolve } from 'node:path';
import process from 'node:process';
import { Array, flow, Option, Result, Schema, SchemaIssue, String, Struct } from 'effect';
import manifest from '../../package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Invocation = readonly [Word, ...Word[]];
type Route = { readonly kind: 'unnamed'; readonly subject: string; readonly target: string } | { readonly kind: 'target'; readonly subject: string; readonly form: string; readonly splices: readonly Splice[]; readonly context: string };
type Finding = { readonly kind: 'refusal'; readonly reason: string } | { readonly kind: 'rewrite'; readonly splices: readonly Splice[]; readonly notice: string; readonly context: string };
type Fault = { readonly kind: 'unstarted'; readonly cause: Error } | { readonly kind: 'exited'; readonly stderr: string } | { readonly kind: 'undecoded'; readonly cause: Schema.SchemaError };
type Output =
    | { readonly hookSpecificOutput: { readonly hookEventName: 'PreToolUse'; readonly permissionDecision: 'deny'; readonly permissionDecisionReason: string } }
    | { readonly systemMessage: string; readonly hookSpecificOutput: { readonly hookEventName: 'PreToolUse'; readonly permissionDecision: 'allow'; readonly updatedInput: { readonly command: string }; readonly additionalContext: string } };

interface Word {
    readonly text: string;
    readonly raw: string;
    readonly start: number;
    readonly end: number;
}
interface Command {
    readonly words: readonly Word[];
    readonly nested: boolean;
}
interface Script {
    readonly commands: readonly Command[];
    readonly polled: readonly string[];
    readonly patches: readonly string[];
}
interface Splice {
    readonly start: number;
    readonly end: number;
    readonly text: string;
}
interface GitRow {
    readonly reason: string;
    readonly any?: true;
    readonly words?: readonly string[];
    readonly prefixes?: readonly string[];
    readonly safe?: readonly string[];
    readonly refine?: (args: readonly string[], exists: (path: string) => boolean, reason: string) => readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ASSIGNMENT = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const _CLUSTER = /^-[A-Za-z]{2,}$/u;
const _QUOTED = /(?:\$?(?<quote>["'])|\\)(?<body>(?<=')[^']*|(?<=")(?:[^"\\]|\\.)*|(?<=\\)[\s\S])\k<quote>/gu;
const _ESCAPED = /\\(?<char>["\\$`\n])/gu;
const _REDIRECT = /^(?!\d*[<>]|&>)./su;
const _PATCHED = /^\*\*\* (?:Add File|Move to): (?<path>.+)$/gmu;
const _SHELL_BODY = /^-[a-z]*c[a-z]*$/u;
const _REPEAT = /^[-+]r\d*$/u;
const _MINI_CONFIGS: readonly (readonly [RegExp, string])[] = [
    [/^project\.json$/u, 'package.json'],
    [/^\.nxignore$/u, '.gitignore'],
    [/^(?:\.mise(?:\..+)?\.toml|mise\..+\.toml|\.miserc\.toml|\.rtx\.toml|\.tool-versions|\.nvmrc|\.(?:node|python)-version)$/u, 'mise.toml'],
    [/^rust-toolchain$/u, 'rust-toolchain.toml'],
    [/^tsconfig\.(?!base\.json$).+\.json$/u, 'tsconfig.json'],
    [/^(?:\.?ruff\.toml|\.?mypy\.ini|pytest\.ini|tox\.ini|setup\.cfg)$/u, 'pyproject.toml'],
    [/^biome\.jsonc$/u, 'biome.json'],
    [/^\.yamllint(?:\.yml)?$/u, '.yamllint.yaml'],
    [/^\.rustfmt\.toml$/u, 'rustfmt.toml'],
    [/^\.clippy\.toml$/u, 'clippy.toml'],
    [/^rust-project\.json$/u, 'Cargo.toml'],
    [/^rust-analyzer\.toml$/u, '.vscode/settings.json'],
];
const _WRAPPERS: ReadonlyMap<string, { readonly valued: readonly string[]; readonly leading: number }> = new Map(
    Object.entries({
        env: { valued: ['-u', '-C', '-S', '-P'], leading: 0 },
        sudo: { valued: ['-u', '-g', '-C', '-D', '-h', '-p', '-R', '-T', '-U'], leading: 0 },
        timeout: { valued: ['-k', '-s'], leading: 1 },
        nice: { valued: ['-n'], leading: 0 },
        caffeinate: { valued: ['-t', '-w'], leading: 0 },
        xargs: { valued: ['-E', '-I', '-J', '-L', '-n', '-P', '-R', '-S', '-s'], leading: 0 },
        exec: { valued: ['-a'], leading: 0 },
        time: { valued: ['-o'], leading: 0 },
        command: { valued: [], leading: 0 },
        nohup: { valued: [], leading: 0 },
    }),
);
const _WAITERS: ReadonlyMap<string, (args: readonly string[], wraps: boolean) => boolean> = new Map(
    Object.entries({
        sleep: () => true,
        pwait: () => true,
        wait: (args: readonly string[]) => args.some((word) => !word.startsWith('-')),
        caffeinate: (args: readonly string[], wraps: boolean) => !wraps || args.includes('-w'),
        tail: (args: readonly string[]) => args.some(String.startsWith('--pid')),
        lsof: (args: readonly string[]) => args.some((word) => _REPEAT.test(word)),
    }),
);
const _GIT: ReadonlyMap<string, GitRow> = new Map(
    Object.entries({
        branch: { reason: 'deletes or force-moves a branch', words: ['-d', '-D', '-M', '--delete'], prefixes: ['--force'] },
        checkout: {
            reason: 'discards local changes',
            words: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'],
            prefixes: ['--force'],
            refine: (args: readonly string[], exists: (path: string) => boolean): readonly string[] => {
                const [first, ...more] = args.filter((word) => word === '-' || !word.startsWith('-'));
                const separated = args.includes('--');
                if (!separated && args.some((word) => ['-b', '--orphan', '-t', '--track', '--detach'].includes(word))) {
                    return [];
                }
                if (separated || more.length > 0 || first === '.' || first?.startsWith(':') === true) {
                    return ['git checkout with a pathspec overwrites working-tree files. Edit the files'];
                }
                return first !== undefined && first !== '-' && exists(first) ? [`git checkout ${first} names an existing path it overwrites. Edit the file`] : [];
            },
        },
        clean: { reason: 'deletes untracked files. Remove the named files with rm', any: true },
        config: {
            reason: 'defines a git alias that can hide a refused subcommand',
            refine: (args: readonly string[], _exists: (path: string) => boolean, reason: string): readonly string[] => {
                const alias = args.findIndex(String.startsWith('alias.'));
                return alias >= 0 && args.slice(alias + 1).some((word) => !word.startsWith('-')) ? [`git config ${args[alias]} ${reason}`] : [];
            },
        },
        push: { reason: 'rewrites or deletes remote history', words: ['-f', '-d', '--delete', '--mirror', '--prune'], prefixes: ['--force', '+', ':'] },
        rebase: { reason: 'rewrites commits other agents can hold', any: true },
        reflog: { reason: 'erases reflog entries, the last recovery path', words: ['delete', 'drop', 'expire'] },
        reset: {
            reason: 'wipes working-tree or index state',
            words: ['--hard', '--merge', '--keep'],
            refine: (args: readonly string[], exists: (path: string) => boolean): readonly string[] => {
                const targets = args.filter((word) => !word.startsWith('-'));
                const [target] = targets;
                return target === undefined || args.includes('--') || targets.some(exists) ? [] : [`git reset ${target} moves HEAD and drops commits from the branch`];
            },
        },
        restore: {
            reason: 'overwrites working-tree files. Edit the files',
            refine: (args: readonly string[], _exists: (path: string) => boolean, reason: string): readonly string[] => (args.some((word) => word === '-S' || word === '--staged') && !args.some((word) => word === '-W' || word === '--worktree') ? [] : [`git restore ${reason}`]),
        },
        revert: { reason: 'reverses committed history', any: true },
        stash: { reason: 'hides uncommitted work other agents depend on. Commit to a branch', any: true, safe: ['list', 'show'] },
        switch: { reason: 'discards local changes', words: ['-f', '-C', '--discard-changes'], prefixes: ['--force'] },
        worktree: { reason: 'creates a second checkout with its own metadata and sync cost. Work in the main checkout', any: true, safe: ['list'] },
    } satisfies Record<string, GitRow>),
);

// --- [MODELS] --------------------------------------------------------------------------

const Capture = Schema.Struct({ text: Schema.String, range: Schema.Struct({ byteOffset: Schema.Struct({ start: Schema.Number, end: Schema.Number }) }) });
const Hits = Schema.fromJsonString(
    Schema.Array(
        Schema.Union([Schema.Struct({ ruleId: Schema.Literal('command'), metaVariables: Schema.Struct({ single: Schema.Struct({ CMD: Capture }), multi: Schema.Struct({ ARGS: Schema.optionalKey(Schema.Array(Capture)) }) }) }), Schema.Struct({ ruleId: Schema.Literals(['polled', 'patch']), text: Schema.String })]),
    ),
);
const Call = Schema.fromJsonString(Schema.Struct({ cwd: Schema.String, tool_name: Schema.Literals(['Bash', 'apply_patch']), tool_input: Schema.Struct({ command: Schema.String }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PARSE]

const _word = ({ text, range: { byteOffset } }: typeof Capture.Type): Word => ({ text: text.replace(_QUOTED, (_match, quote: string | undefined, body: string) => (quote === '"' ? body.replace(_ESCAPED, '$<char>') : body)), raw: text, start: byteOffset.start, end: byteOffset.end });

const _described = (fault: Fault): string => {
    switch (fault.kind) {
        case 'unstarted':
            return `ast-grep did not run, ${fault.cause.message}`;
        case 'exited':
            return `ast-grep failed, ${fault.stderr.trim()}`;
        default:
            return `ast-grep output does not decode, ${SchemaIssue.makeFormatterDefault()(fault.cause.issue)}`;
    }
};

const _body = ([head, ...rest]: Invocation): Option.Option<string> => {
    const program = basename(head.text);
    const flag = rest.findIndex(({ text }) => _SHELL_BODY.test(text));
    if (program === 'eval') {
        return Option.some(rest.map(Struct.get('text')).join(' '));
    }
    return ['bash', 'sh', 'zsh', 'dash', 'ksh'].includes(program) && flag >= 0 ? Option.map(Option.fromUndefinedOr(rest.slice(flag + 1).find(({ text }) => !text.startsWith('-'))), Struct.get('text')) : Option.none();
};

const _chain = (words: readonly Word[]): readonly Invocation[] => {
    const [head, ...rest] = words;
    if (head === undefined) {
        return [];
    }
    if (_ASSIGNMENT.test(head.text)) {
        return _chain(rest);
    }
    const row = _WRAPPERS.get(basename(head.text));
    const skipped = (left: readonly Word[]): readonly Word[] => {
        const [next, ...after] = left;
        if (next === undefined || row === undefined) {
            return [];
        }
        if (next.text === '--') {
            return after.slice(row.leading);
        }
        return next.text.startsWith('-') || _ASSIGNMENT.test(next.text) ? skipped(after.slice(row.valued.includes(next.text) ? 1 : 0)) : left.slice(row.leading);
    };
    const separator = rest.findIndex(({ text }) => text === '--');
    const launched = separator < 0 ? [] : rest.slice(separator + 1);
    const inner = ['mise', 'doppler', 'op'].includes(basename(head.text)) ? launched : skipped(rest);
    return [[head, ...rest.slice(0, rest.length - inner.length)], ..._chain(inner)];
};

const _parse = (text: string, nested: boolean): Result.Result<Script, readonly [Fault, ...Fault[]]> => {
    const rules = [
        'id: command\nlanguage: bash\nrule: {kind: command, any: [{pattern: $CMD $$$ARGS}, {pattern: $CMD}]}',
        `id: polled\nlanguage: bash\nrule: {any: [{kind: while_statement, not: {has: {field: condition, any: [{matches: reader}, {has: {stopBy: end, matches: reader}}]}}}, {kind: c_style_for_statement, not: {has: {field: condition, regex: '.'}}}]}\nutils: {reader: {kind: command, has: {field: name, regex: '^read$'}}}`,
        `id: patch\nlanguage: bash\nrule: {kind: heredoc_body, inside: {kind: heredoc_redirect, inside: {kind: redirected_statement, has: {field: body, any: [{matches: patcher}, {has: {matches: patcher, nthChild: {position: 1, reverse: true}}}]}}}}\nutils: {patcher: {kind: command, has: {field: name, regex: '^(apply_patch|applypatch)$'}}}`,
    ];
    const ran = spawnSync('ast-grep', ['scan', '--stdin', '--config', devNull, '--json=compact', '--inline-rules', rules.join('\n---\n')], { input: text, encoding: 'utf8' });
    if (ran.error !== undefined) {
        return Result.fail([{ kind: 'unstarted', cause: ran.error }]);
    }
    return ran.status === 0
        ? Result.match(Schema.decodeUnknownResult(Hits)(ran.stdout), {
              onFailure: (cause) => Result.fail([{ kind: 'undecoded', cause }]),
              onSuccess: (hits) => {
                  const commands = hits.flatMap((hit): readonly Command[] => (hit.ruleId === 'command' ? [{ words: [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? [])].filter(({ text: word }) => _REDIRECT.test(word)).map(_word), nested }] : []));
                  const inner = commands.flatMap(({ words }) => _chain(words).slice(-1).flatMap(flow(_body, Option.toArray))).map((body) => _parse(body, true));
                  const scripts: readonly Script[] = [{ commands, polled: hits.flatMap((hit) => (hit.ruleId === 'polled' ? [hit.text] : [])), patches: hits.flatMap((hit) => (hit.ruleId === 'patch' ? [hit.text] : [])) }, ...Array.getSuccesses(inner)];
                  const [fault, ...more] = Array.getFailures(inner).flat();
                  return fault === undefined ? Result.succeed({ commands: scripts.flatMap(Struct.get('commands')), polled: scripts.flatMap(Struct.get('polled')), patches: scripts.flatMap(Struct.get('patches')) }) : Result.fail([fault, ...more]);
              },
          })
        : Result.fail([{ kind: 'exited', stderr: ran.stderr }]);
};

// --- [GIT]

const _git = ([, ...words]: Invocation, exists: (path: string) => boolean): readonly string[] => {
    const texts = words.map(Struct.get('text'));
    const valued = ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'];
    const operand = (index: number): number => {
        const word = texts[index];
        return word === undefined || !word.startsWith('-') ? index : operand(index + (valued.includes(word) ? 2 : 1));
    };
    const at = operand(0);
    const [key, ...rest] = texts.slice(at);
    const row = key === undefined ? undefined : _GIT.get(key);
    const args = rest.flatMap((word) => (_CLUSTER.test(word) ? [...word.slice(1)].map((letter) => `-${letter}`) : [word]));
    const [first] = args;
    const hit = args.find((word) => row?.words?.includes(word) === true || row?.prefixes?.some((prefix) => word.startsWith(prefix)) === true);
    if (texts.slice(0, at).some(String.startsWith('alias.'))) {
        return ['inline git alias can hide a refused subcommand'];
    }
    if (row === undefined || (first !== undefined && row.safe?.includes(first) === true)) {
        return [];
    }
    if (row.any === true) {
        return [`git ${key} ${row.reason}`];
    }
    return hit === undefined ? (row.refine?.(args, exists, row.reason) ?? []) : [`git ${key} ${hit} ${row.reason}`];
};

// --- [WAIT]

const _waits = ([head, ...words]: Invocation, wraps: boolean): boolean => _WAITERS.get(basename(head.text))?.(words.map(Struct.get('text')), wraps) === true;

const _wait = (script: Script): readonly Finding[] => {
    const reasons = [
        ...script.commands.flatMap(({ words }) => {
            const chain = _chain(words);
            return chain.some((invocation, index) => _waits(invocation, index < chain.length - 1)) ? [`${words.map(Struct.get('text')).join(' ')} waits`] : [];
        }),
        ...script.polled.map((loop) => `${loop.split('\n', 1).join('')} repeats until the loop condition changes`),
    ];
    return reasons.length === 0 ? [] : [{ kind: 'refusal', reason: [...reasons, "Act on the awaited command's own exit, polling its exec session with write_stdin"].join('. ') }];
};

// --- [HIDDEN]

const _fd = ([head, ...rest]: Invocation): readonly Finding[] => {
    const words = rest.map(Struct.get('text'));
    const flags = words.slice(0, words.includes('--') ? words.indexOf('--') : words.length).flatMap((word) => (word.startsWith('--') || !word.startsWith('-') ? [word] : [...word.slice(1)].map((letter) => `-${letter}`)));
    const run = flags.findIndex((flag) => ['-x', '-X', '--exec', '--exec-batch'].includes(flag));
    const context = 'fd ran with --hidden added, it skips dot-folders without it';
    return basename(head.text) !== 'fd' || flags.slice(0, run < 0 ? flags.length : run).some((flag) => ['-H', '--hidden', '-u', '--unrestricted', '--help', '-h', '--version'].includes(flag)) ? [] : [{ kind: 'rewrite', splices: [{ start: head.end, end: head.end, text: ' --hidden' }], notice: context, context }];
};

// --- [TARGETS]

const _dotnet = ([head, subcommand, ...rest]: Invocation, exists: (path: string) => boolean, root: string): Option.Option<Route> => {
    const last = rest.at(-1) ?? subcommand;
    if (subcommand === undefined || last === undefined || rest.some(({ text }) => ['-h', '-?', '--help', '--version'].includes(text))) {
        return Option.none();
    }
    const subject = `dotnet ${subcommand.text}`;
    const named = rest.find(({ text }) => text.endsWith('.csproj') || exists(join(text, `${basename(text)}.csproj`)));
    const forwarded = rest.flatMap((word) => (word === named ? [] : [word.raw]));
    const target = (form: string, context: string): Option.Option<Route> => Option.some({ kind: 'target', subject, form, splices: [{ start: head.start, end: last.end, text: form }], context });
    switch (subcommand.text) {
        case 'build':
        case 'test':
            return named === undefined
                ? Option.some({ kind: 'unnamed', subject, target: subcommand.text })
                : target([`nx run ${basename(named.text, '.csproj')}:${subcommand.text}`, ...(forwarded.length === 0 ? [] : ['--', ...forwarded])].join(' '), subcommand.text === 'build' ? 'Read a failed build from its .artifacts/dotnet/binlog/ log through the binlog MCP' : 'Nx caches the result');
        case 'format':
            return target(`nx run ${root}:${rest.some(({ text }) => text === '--verify-no-changes') ? 'lint:dotnet-format' : 'format:dotnet'}`, 'The target covers every project of Workspace.slnx');
        default:
            return Option.none();
    }
};

const _scan = (invocation: Invocation, _exists: (path: string) => boolean, root: string): Option.Option<Route> => {
    const [head, subcommand] = invocation;
    const modes = invocation.flatMap((word, index) => (index > 1 && (word.text === '-U' || word.text === '--update-all') ? [{ start: invocation[index - 1]?.end ?? word.start, end: word.end, text: '' }] : []));
    return subcommand?.text !== 'scan' || modes.length === 0 || invocation.some(({ text }) => text === '--stdin')
        ? Option.none()
        : Option.some({ kind: 'target', subject: 'ast-grep scan -U', form: `nx run ${root}:rewrite`, splices: [{ start: head.start, end: subcommand.end, text: `nx run ${root}:rewrite --` }, ...modes], context: 'Nx owns rewrite execution and source exclusion' });
};

const _ROUTES: ReadonlyMap<string, typeof _dotnet> = new Map(Object.entries({ dotnet: _dotnet, 'ast-grep': _scan, sg: _scan }));

const _targets = (script: Script, exists: (path: string) => boolean, root: string): readonly Finding[] =>
    script.commands.flatMap(({ words, nested }) =>
        _chain(words).flatMap((invocation): readonly Finding[] => {
            const found = Option.flatMap(Option.fromUndefinedOr(_ROUTES.get(basename(invocation[0].text))), (route) => route(invocation, exists, root));
            return Option.toArray(
                Option.map(found, (route): Finding => {
                    if (route.kind === 'unnamed') {
                        return { kind: 'refusal', reason: `${route.subject} names no project file. Run nx run <project>:${route.target}` };
                    }
                    if (nested) {
                        return { kind: 'refusal', reason: `${route.subject} runs outside the Nx task graph. Run ${route.form}` };
                    }
                    return { kind: 'rewrite', splices: route.splices, notice: `${route.subject} ran as ${route.form}`, context: `${route.subject} ran as ${route.form}. ${route.context}` };
                }),
            );
        }),
    );

// --- [FILES]

const _files = (patches: readonly string[]): readonly Finding[] =>
    patches
        .flatMap((patch) => [...patch.matchAll(_PATCHED)].flatMap(({ groups }) => Option.toArray(Option.map(Option.fromUndefinedOr(groups?.path), basename))))
        .flatMap((name) => _MINI_CONFIGS.flatMap(([pattern, owner]): readonly Finding[] => (pattern.test(name) ? [{ kind: 'refusal', reason: `${name} is a mini config. Edit ${owner}` }] : [])));

// --- [DECISION]

const _spliced = (text: string, splices: readonly Splice[]): string => {
    const bytes = Buffer.from(text);
    const ordered = splices.toSorted((left, right) => left.start - right.start);
    return Buffer.concat([...ordered.flatMap((splice, index) => [bytes.subarray(ordered[index - 1]?.end ?? 0, splice.start), Buffer.from(splice.text)]), bytes.subarray(ordered.at(-1)?.end ?? 0)]).toString();
};

const _answer = (tool: 'Bash' | 'apply_patch', command: string, exists: (path: string) => boolean, root: string): Option.Option<Output> => {
    const script = tool === 'apply_patch' ? Result.succeed<Script>({ commands: [], polled: [], patches: [command] }) : _parse(command, false);
    const findings = Result.map(script, (parsed) => [
        ...parsed.commands.flatMap(({ words }) => _chain(words).flatMap((invocation): readonly Finding[] => (basename(invocation[0].text) === 'git' ? _git(invocation, exists).map((reason) => ({ kind: 'refusal', reason })) : []))),
        ..._wait(parsed),
        ...parsed.commands.flatMap(({ words, nested }) => (nested ? [] : _chain(words).flatMap(_fd))),
        ..._targets(parsed, exists, root),
        ..._files(parsed.patches),
    ]);
    const deny = (reason: string): Option.Option<Output> => Option.some({ hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: reason } });
    return Result.match(findings, {
        onFailure: (faults) => deny(`command not parsed, ${Array.dedupe(faults.map(_described)).join('. ')}`),
        onSuccess: (found) => {
            const refusals = Array.dedupe(found.flatMap((finding) => (finding.kind === 'refusal' ? [finding.reason] : [])));
            const rewrites = found.flatMap((finding) => (finding.kind === 'rewrite' ? [finding] : []));
            if (refusals.length > 0) {
                return deny(refusals.join('. '));
            }
            return rewrites.length === 0
                ? Option.none()
                : Option.some({
                      systemMessage: rewrites.map(Struct.get('notice')).join(' · '),
                      hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow', updatedInput: { command: _spliced(command, rewrites.flatMap(Struct.get('splices'))) }, additionalContext: rewrites.map(Struct.get('context')).join('. ') },
                  });
        },
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

Result.match(Schema.decodeUnknownResult(Call)(readFileSync(0, 'utf8')), {
    onFailure: (cause) => {
        process.stderr.write(`hook input does not hold the Codex PreToolUse form, ${SchemaIssue.makeFormatterDefault()(cause.issue)}\n`);
        process.exitCode = 2;
    },
    onSuccess: ({ cwd, tool_name, tool_input }) =>
        Option.map(
            _answer(tool_name, tool_input.command, (path) => existsSync(resolve(cwd, path)), manifest.name),
            (output) => process.stdout.write(JSON.stringify(output)),
        ),
});
