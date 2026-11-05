import { Maybe } from '../../monads.ts';
import { type Command, parse, runJson, type Word } from './command.ts';
import { argumentGroups, basename, type Invocation, informational, isInside, operands, programName, programRow, resolvePath } from './invocation.ts';
import { type Call, EXPANSION, invocationsOf, literalArgument, quote } from './verdict.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Target {
    readonly options?: {
        readonly command?: string;
        readonly commands?: readonly (string | { readonly command: string; readonly forwardAllArgs?: boolean })[];
        readonly args?: string | readonly string[];
        readonly forwardAllArgs?: boolean;
    };
    readonly configurations?: Readonly<Record<string, { readonly command?: string }>>;
    readonly cache?: boolean;
    readonly dependsOn?: readonly (string | { readonly target: string })[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECK_TARGET = /^(?:(?:lint|format|typecheck):(?<tool>[^:]+)|rewrite)(?=:|$)/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GRAPH]

const optionKeys = (invocation: Invocation, known: (name: string) => boolean = () => true): ReadonlySet<string> => {
    const { options, values } = operands(invocation);
    return new Set([...options.filter(known), ...values.flatMap(([name, value]) => (known(name) ? [`${name}=${value}`] : []))]);
};

const groupMembers = (graph: Readonly<Record<string, Target>>, parent: string, project = ''): readonly string[] => {
    const named = (graph[parent]?.dependsOn ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.target)).map((name) => (name.startsWith(`${project}:`) ? name.slice(project.length + 1) : name));
    return Object.keys(graph).filter((candidate) => named.some((name) => name === candidate || (name.endsWith('*') && candidate.startsWith(name.slice(0, -1)))));
};

const cachedTarget = ({ name, targets }: { readonly name: string; readonly targets: Readonly<Record<string, Target>> }, target: string): boolean => targets[target]?.cache === true || groupMembers(targets, target, name).some((member) => cachedTarget({ name, targets }, member));

const groupTarget = (graph: Readonly<Record<string, Target>>, target: string): string => {
    const parent = CHECK_TARGET.exec(target)?.[0] ?? target;
    const members = groupMembers(graph, parent);
    return members.includes(target) && (graph[parent]?.options?.args?.length ?? 0) === 0 && !members.some((name) => (graph[name]?.options?.args?.length ?? 0) > 0) ? parent : target;
};

const projectTargets = ({ system, projects }: Call, named: string, root: string) => projects.getOrInsertComputed(named, () => runJson<{ readonly name: string; readonly targets: Readonly<Record<string, Target>> }>(system, ['mise', 'exec', '--', 'nx', 'show', 'project', named, '--json'], root));

const rootRelative = (word: Word, cwd: string, root: string, home: Maybe<string>): readonly string[] => {
    const [place] = resolvePath(word.text, Maybe.some(cwd), home).values;
    return word.expands || EXPANSION.test(word.raw) || place === undefined || !isInside(place, root) ? [] : [quote(place.slice(root.length + 1) || '.')];
};

const ownerProject = async (call: Call, selectors: readonly string[], invocation: Invocation, cwd: string, root: string) => {
    const { system } = call;
    const [value, ...more] = operands(invocation).values.flatMap(([flag, argument]) => (selectors.includes(flag) ? [argument] : []));
    const [place] = resolvePath(value ?? '.', Maybe.some(cwd), system.home).values.filter((path) => isInside(path, root));
    const located = (path: string): Maybe<string> => {
        const leaf = basename(path);
        const folder = leaf === 'tsconfig.json' ? path.slice(0, path.lastIndexOf('/')) : path;
        return folder === path && leaf.lastIndexOf('.') > 0 ? Maybe.none : Maybe.some(folder.slice(root.length + 1) || '.');
    };
    if (selectors.length === 0) {
        return projectTargets(call, '.', root);
    }
    if (more.length > 0) {
        return Maybe.none;
    }
    const [pattern] = (place !== undefined && (await system.exists(place)) ? located(place) : Maybe.fromNullable(value)).values;
    return pattern === undefined ? Maybe.none : projectTargets(call, pattern, root);
};

// --- [RUNS]

const runScope = (invocation: Invocation, subcommanded: boolean, cwd: Maybe<string>, home: Maybe<string>) => {
    const cut = invocation.indexOf('--');
    const extra = cut < 0 ? [] : invocation.slice(cut + 1);
    const { inputs } = operands(invocation);
    return { places: inputs.slice(Number(subcommanded), inputs.length - extra.length).flatMap((word) => resolvePath(word, cwd, home).values), extra };
};

const launches = ([program, ...args]: Invocation): boolean => {
    const { wraps, module, launcher } = programRow(program);
    return launcher === true || (module !== undefined && args.at(-1) === module) || wraps === '--' || (wraps ?? []).some((path) => path.length > 0 && path.every((step) => args.includes(step)));
};

const coveredBy = (places: readonly string[], folders: readonly string[]): boolean => places.every((place) => folders.some((folder) => isInside(place, folder)));

const checkerRuns = async ({ system, parses }: Call, graph: Readonly<Record<string, Target>>, program: string, subcommand: string | undefined) => {
    const { subcommandless } = programRow(program);
    const listed = Object.entries(graph).flatMap(([target, { options, configurations = {} }]) => {
        const kind = CHECK_TARGET.exec(target);
        const tool = kind?.groups?.tool;
        const entries = [...(options?.command === undefined ? (options?.commands ?? []) : [options.command]), ...Object.values(configurations).flatMap(({ command }) => (command === undefined ? [] : [command]))];
        const args = [options?.args ?? []].flat();
        return kind !== null && (tool === undefined || tool === programName(program) || tool === subcommand || entries.some((entry) => (typeof entry === 'string' ? entry : entry.command).includes(program)))
            ? entries
                  .map((entry) => {
                      const configured = typeof entry === 'string' ? { command: entry } : entry;
                      const forwardAllArgs = configured.forwardAllArgs ?? options?.forwardAllArgs ?? true;
                      return { target, text: [configured.command, ...(forwardAllArgs ? args : [])].join(' '), args: forwardAllArgs ? args : [], forwardAllArgs };
                  })
                  .filter(({ text }) => !text.includes('{args'))
            : [];
    });
    const parsed = (await Promise.all(listed.map(async (configured) => (await parses.getOrInsertComputed(configured.text, (source) => parse(system, source))).values.map(({ commands }) => ({ ...configured, commands }))))).flat();
    return parsed
        .flatMap(({ commands, text, ...configured }) =>
            invocationsOf(configured.args.length === 0 ? commands : commands.filter(({ extent, words: configuredWords }) => commands.length === 1 && configuredWords.every(literalArgument) && extent.end - extent.start === new TextEncoder().encode(text.trim()).length), program).map((ran) => ({ ...configured, ran })),
        )
        .flatMap(({ ran, target, forwardAllArgs }) => (subcommandless === true || operands(ran).inputs[0] === subcommand ? [{ target, forwardAllArgs, keys: optionKeys(ran), options: operands(ran).options, ...runScope(ran, subcommandless !== true, system.root, system.home) }] : []));
};

const forwardedArguments = (program: string, words: readonly Word[], dropped: ReadonlySet<string>, subcommand: boolean) => {
    const listed = argumentGroups(program, words.values()).toArray();
    const first = listed.findIndex(({ names }) => names.length === 0);
    return listed.filter(({ names, words: group }, at) => (names.length === 0 ? !subcommand || at !== first : !(new Set(names).isSubsetOf(dropped) || dropped.has(group.map(({ text }) => text).join('=')))));
};

const checkerTarget = async (call: Call, { cwd, hasInput }: Command, invocation: Invocation, words: readonly Word[]): Promise<Maybe<{ readonly run: string; readonly args: readonly string[] }>> => {
    const [program] = invocation;
    const { selectors, subcommandless, inert = [], modes = [] } = programRow(program);
    const { home } = call.system;
    const [root] = call.system.root.values;
    const [folder] = cwd.values;
    const { options, inputs } = operands(invocation);
    if (selectors === undefined || root === undefined || folder === undefined || !isInside(folder, root) || hasInput || informational(program, options)) {
        return Maybe.none;
    }
    const [project] = (await ownerProject(call, selectors, invocation, folder, root)).values;
    if (project === undefined) {
        return Maybe.none;
    }
    const { name, targets } = project;
    const [subcommand] = subcommandless === true ? [] : inputs;
    const { places, extra } = runScope(invocation, subcommandless !== true, cwd, home);
    const found = (await checkerRuns(call, targets, program, subcommand)).filter((ran) => modes.every(([flag]) => options.includes(flag) === ran.options.includes(flag)));
    const known = new Set(found.flatMap((ran) => ran.options));
    const wanted = optionKeys(invocation, (flag) => known.has(flag));
    const fitting = found.filter((ran) => wanted.isSubsetOf(ran.keys) && extra.every((word) => ran.extra.includes(word)) && (ran.places.length === 0 || (places.length > 0 && coveredBy(places, ran.places))));
    const fewest = Math.min(...fitting.map(({ keys }) => keys.size));
    const targetOf = (ran: (typeof found)[number]): string => (ran.places.length > 0 ? ran.target : groupTarget(targets, ran.target));
    const [target, ...others] = [...new Set(fitting.filter(({ keys }) => keys.size === fewest).map(targetOf))];
    const scoped = fitting.some((ran) => ran.target === target && ran.places.length > 0);
    const aggregate = !found.some((ran) => ran.target === target);
    const kept = forwardedArguments(program, words.slice(1), new Set([...known, ...selectors, ...inert]), subcommand !== undefined);
    const selected = fitting.filter((ran) => targetOf(ran) === target);
    const evaluating = kept.some(({ words: group }) => !group.every(literalArgument));
    const switched = kept.some(({ names }) => names.length > 0);
    const moved = selectors.length === 0 && folder !== root;
    const passed = scoped ? [] : kept.flatMap((item) => item.words).map((word) => (moved ? rootRelative(word, folder, root, home) : [word.raw]));
    const forwarded = [...passed.flat(), ...(moved && !scoped && passed.length === 0 ? [quote(folder.slice(root.length + 1))] : [])];
    const ignored = forwarded.length > 0 && selected.some(({ forwardAllArgs }) => !forwardAllArgs);
    const uniform = Maybe.fromNullable(target)
        .values.flatMap((parent) => groupMembers(targets, parent))
        .every((member) => found.some((ran) => ran.target === member && wanted.isSubsetOf(ran.keys)));
    const widened = forwarded.length > 0 && modes.some(([flag]) => options.includes(flag));
    if (
        target === undefined ||
        others.length > 0 ||
        ignored ||
        evaluating ||
        (aggregate && (switched || widened || (wanted.size > 0 && !uniform))) ||
        ((moved || scoped) && switched) ||
        passed.some((argument) => argument.length === 0) ||
        !coveredBy(places, [root]) ||
        (!scoped && words.some(({ text }) => text === '--'))
    ) {
        return Maybe.none;
    }
    return Maybe.some({ run: `${name}:${target}`, args: forwarded.length === 0 ? [] : [`--args=${quote([targets[target]?.options?.args ?? [], forwarded].flat().join(' '))}`] });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cachedTarget, checkerTarget, launches, optionKeys, projectTargets, type Target };
