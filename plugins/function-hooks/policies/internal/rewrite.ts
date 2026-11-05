import { Maybe } from '../../monads.ts';
import { type Command, parse, type Word } from './command.ts';
import { argumentGroups, basename, type Invocation, informational, operands } from './invocation.ts';
import { cachedTarget, checkerTarget, launches, optionKeys, projectTargets } from './nx-target.ts';
import { type Call, edit, editOrRefuse, MAX_TIMEOUT, refuse, type Verdict } from './verdict.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ESCAPED_NEWLINE = /(?<!\\)(?:\\\\)*\\n/u;
const DURATION = /^(?<value>\d+(?:\.\d+)?)(?<unit>[smh]?)$/u;
const CACHE_FLAGS: readonly string[] = ['--skip-nx-cache', '--skipNxCache', '--disable-nx-cache', '--disableNxCache'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TIMEOUT]

const timeoutCommands = async ({ request, system, script: { commands } }: Call): Promise<readonly Verdict[]> => {
    const second = 1000;
    const applied = request.kind === 'foreground' && request.timeout.values.length === 0;
    const limited = commands.flatMap((command) =>
        command.invocations.flatMap((invocation, at) => {
            const [from] = command.parts[at]?.words ?? [];
            const [to] = [...(command.parts[at + 1]?.assigned ?? []), ...(command.parts[at + 1]?.words ?? [])];
            const [amount] = operands(invocation).leading;
            const groups = invocation[0] === 'timeout' ? DURATION.exec(amount ?? '')?.groups : undefined;
            const limit = Number(groups?.value) * second * 60 ** 'smh'.indexOf(String(groups?.unit));
            return from === undefined || to === undefined || amount === undefined || !(limit > 0) ? [] : [{ command, from, to, amount, limit }];
        }),
    );
    const installed =
        limited.length > 0 &&
        (await system.run(['timeout', '--version'], { cwd: system.plugin, stdin: '' }).then(
            ({ exitCode }) => exitCode === 0,
            () => false,
        ));
    return installed
        ? []
        : limited.map(({ command, from, to, amount, limit }) => {
              const capped = limit > MAX_TIMEOUT ? `capped at ${MAX_TIMEOUT / second}s` : `set to ${amount}`;
              return editOrRefuse(command, edit(from.span.start, to.span.start, '', Maybe.some(`timeout is not installed, call timeout ${applied ? capped : 'stays'}`), applied ? Maybe.some(Math.min(limit, MAX_TIMEOUT)) : Maybe.none), 'timeout is not installed, remove it');
          });
};

// --- [PROGRAMS]

const optionRespellings = (program: string, words: readonly Word[], table: Readonly<Record<string, string>>, reason = ''): readonly Verdict[] =>
    argumentGroups(program, words.values())
        .flatMap(({ words: [head, ...taken], names, joined }) => {
            const kept = names.map((name) => table[name] ?? name).filter((name) => name !== '');
            const [first] = kept;
            if (kept.join(' ') === names.join(' ')) {
                return [];
            }
            if (first === undefined) {
                return [edit(head.span.start - 1, (taken.at(-1) ?? head).span.end, '', Maybe.some(`${program} ${head.text} removed${reason}`))];
            }
            const text = first.startsWith('--') ? [first, ...joined].join('=') : `-${kept.map((name) => name.slice(1)).join('')}${joined.join('')}`;
            return [edit(head.span.start, head.span.end, text, Maybe.some(`${program} ${head.text} became ${text}`))];
        })
        .toArray();

const sdFixes = (invocation: Invocation, [head, ...rest]: readonly Word[]): readonly Verdict[] => {
    const { options, leading } = operands(invocation);
    const [find] = leading;
    const [word] = argumentGroups('sd', rest.values()).flatMap(({ names, words: [first] }) => (names.length === 0 && first.text.startsWith('-') ? [first] : []));
    const breaks = find !== undefined && (find.includes('\n') || (!options.some((name) => name === '-F' || name === '--fixed-strings') && ESCAPED_NEWLINE.test(find)));
    return [
        ...(head === undefined || !breaks || options.some((name) => name === '-A' || name === '--across') ? [] : [edit(head.span.end, head.span.end, ' -A')]),
        ...(word === undefined || rest.some(({ text }) => text === '--') ? [] : [edit(word.span.start, word.span.start, '-- ', Maybe.some(`sd -- added before ${word.raw}`))]),
    ];
};

const nxCacheFlags = async (call: Call, invocation: Invocation, words: readonly Word[]): Promise<readonly Verdict[]> => {
    const { inputs, options, values } = operands(invocation);
    const [root] = call.system.root.values;
    if (root === undefined || !CACHE_FLAGS.some((flag) => options.includes(flag))) {
        return [];
    }
    const listed = values.flatMap(([name, value]) => (['-t', '--targets', '--target'].includes(name) ? value.split(',') : []));
    const addresses = inputs[0] === 'run' ? inputs.slice(1, 2).flatMap((word) => (word.includes(':') ? [{ project: word.slice(0, word.indexOf(':')), target: word.slice(word.indexOf(':') + 1) }] : [])) : [];
    const named = values.flatMap(([name, value]) => (['-p', '--projects'].includes(name) ? value.split(',') : []));
    const projects = named.length > 0 ? named : ['.'];
    const asked = inputs[0] === 'run' ? addresses : [...listed, ...inputs.slice(0, listed.length === 0 ? 1 : 0)].flatMap((target) => projects.map((project) => ({ project, target })));
    const cached = await Promise.all(asked.map(async ({ project, target }) => (await projectTargets(call, project, root)).values.some((held) => cachedTarget(held, target))));
    return asked.length > 0 && cached.every(Boolean) ? optionRespellings('nx', words, Object.fromEntries(CACHE_FLAGS.map((name) => [name, ''])), `, ${[...new Set(asked.map(({ target }) => target))].join(' ')} inputs hash every source`) : [];
};

const forwardedDuplicates = async (call: Call, invocation: Invocation, words: readonly Word[]): Promise<readonly Verdict[]> => {
    const [root] = call.system.root.values;
    const [subcommand, address] = operands(invocation).inputs;
    const cut = address?.indexOf(':') ?? -1;
    const separator = words.find(({ text }) => text === '--');
    if (root === undefined || subcommand !== 'run' || address === undefined || cut < 0 || separator === undefined) {
        return [];
    }
    const [project] = (await projectTargets(call, address.slice(0, cut), root)).values;
    const configured = project?.targets[address.slice(cut + 1)]?.options?.command;
    const [ran] = configured === undefined ? [] : (await call.parses.getOrInsertComputed(configured, (source) => parse(call.system, source))).values.flatMap(({ commands }) => commands.at(-1)?.invocations.slice(-1) ?? []);
    if (ran === undefined) {
        return [];
    }
    const [program] = ran;
    const keys = optionKeys(ran);
    const groups = argumentGroups(program, words.slice(words.indexOf(separator) + 1).values()).toArray();
    const dropped = groups.filter(({ names, words: group }) => names.length > 0 && optionKeys([program, ...group.map(({ text }) => text)]).isSubsetOf(keys));
    const spelled = dropped.flatMap(({ words: group }) => group.map(({ raw }) => raw)).join(' ');
    const notice = Maybe.some(`nx ${spelled} removed, ${address} sets it`);
    return dropped.length === groups.length && dropped.length > 0 ? [edit(separator.span.start - 1, (dropped.at(-1)?.words.at(-1) ?? separator).span.end, '', notice)] : dropped.map(({ words: [first, ...taken] }) => edit(first.span.start - 1, (taken.at(-1) ?? first).span.end, '', notice));
};

const programRewrites = async (call: Call, invocation: Invocation, words: readonly Word[]): Promise<readonly Verdict[]> => {
    const [program] = invocation;
    const [head, ...rest] = words;
    const { options } = operands(invocation);
    const stated = (names: readonly string[]): boolean => informational(program, options) || names.some((name) => options.includes(name));
    if (head === undefined) {
        return [];
    }
    switch (program) {
        case 'fd':
        case 'rg':
            return stated(['--hidden', '--no-hidden', '-u', '--unrestricted', ...(program === 'fd' ? ['-H'] : ['-.', '--no-config'])]) ? [] : [edit(head.span.end, head.span.end, ' --hidden')];
        case 'ast-grep': {
            const [first] = rest;
            const anchor = first === undefined || first.text.startsWith('-') ? head : first;
            return stated(['--custom-languages']) ? [] : [edit(anchor.span.end, anchor.span.end, ' --custom-languages allow')];
        }
        case 'sd':
            return informational(program, options) ? [] : sdFixes(invocation, words);
        case 'tree':
            return optionRespellings(program, rest, { '-a': '', '--all': '', '--noreport': '', '--dirsfirst': '--group-directories-first' });
        case 'refurb': {
            const text = 'python -m tools.refurb';
            return basename(head.text) === program ? [edit(head.span.start, head.span.end, text, Maybe.some(`refurb became ${text} to read the mypy cache`))] : [];
        }
        case 'stubtest':
            return stated(['--mypy-config-file']) ? [] : call.system.root.values.map((root) => edit(head.span.end, head.span.end, ` --mypy-config-file ${root}/.config/stubtest.ini`));
        case 'nx':
            return [...(await nxCacheFlags(call, invocation, rest)), ...(await forwardedDuplicates(call, invocation, words))];
        default:
            return [];
    }
};

// --- [COMMANDS]

const commandRewrites = async (call: Call, command: Command): Promise<readonly Verdict[]> => {
    const last = command.invocations.at(-1);
    const words = command.parts.at(-1)?.words ?? [];
    const [head] = words;
    const tail = words.at(-1);
    const route = call.routes.get(command);
    if (last === undefined || head === undefined || tail === undefined) {
        return [];
    }
    if (route?.kind === 'refuse') {
        return [route];
    }
    if (command.origin !== 'call') {
        return route === undefined ? [] : [refuse(`Nested ${route.program} cannot be rewritten, run ${route.text}`)];
    }
    if (route !== undefined) {
        return [edit(route.start.span.start, tail.span.end, route.text, Maybe.some(`${route.program} became ${route.text}`))];
    }
    const [found] = (await checkerTarget(call, command, last, words)).values;
    if (found === undefined) {
        return programRewrites(call, last, words);
    }
    const { run, args } = found;
    const [start = head] = command.parts[command.invocations.findLastIndex((invocation, at) => at < command.invocations.length - 1 && !launches(invocation)) + 1]?.words ?? [];
    return [edit(start.span.start, tail.span.end, [`nx run ${run}`, ...args].join(' '), Maybe.some(`${last[0]} became nx run ${run}`))];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { commandRewrites, timeoutCommands };
