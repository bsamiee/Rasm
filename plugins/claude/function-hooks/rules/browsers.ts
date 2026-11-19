import { basename } from 'node:path';
import { Array, Effect, Option, Record } from 'effect';
import type { Verdict } from '#/rules/decision';
import { commandOf, edited, inserted, quote, type Rule, type Ruleset, refusal, respell, whole } from '#/rules/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WEB_URL = /^https?:/iu;
const BROWSER_NAME = /^[a-z][\w-]*$/u;
const SESSION = new Set(['-s', '--session']);
const EXTENSION_CONFIG = '.playwright/extension.config.json';
const LITERAL_BROWSER = 'pass playwright literally so its session is named for you';
const VERBS = {
    'install-browser': undefined,
    list: undefined,
    'close-all': "closes every agent's sessions, use playwright cli -s=<session> close",
    'kill-all': "kills every agent's browsers, use playwright cli -s=<session> close",
    install: 'writes a .playwright workspace in the working folder, use playwright cli install-browser <browser>',
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const sessionOf = (owner: string) => `-s=${quote(owner)}`;

const openedUrl: Rule = (invocation, { request }) => {
    const documents = [...invocation.operands, ...invocation.supplied('--url')];
    return Option.match(
        Array.findFirst(documents, ({ text }) => WEB_URL.test(text)),
        {
            onNone: () => [],
            onSome: (url) => {
                const opened = ['playwright', 'cli', ...Option.toArray(Option.map(Option.fromNullishOr(request.agent ?? request.session), sessionOf)), 'open', url.raw].join(' ');
                return documents.length === 1 ? whole(invocation, opened, `${invocation.program} ${url.raw}`) : refusal(`${invocation.program} ${url.raw} opens a browser, use ${opened}`);
            },
        },
    );
};

const browserCli: Rule = Effect.fnUntraced(function* (invocation, { request, host }) {
    const [cli, verb] = invocation.operands;
    const named = verb?.text ?? '';
    if (cli?.text !== 'cli') {
        return [];
    }
    const expanded = [invocation.head, cli].flatMap(({ parts }) => parts.flatMap(({ parameter }) => Option.toArray(Option.fromNullishOr(parameter?.name))));
    if (expanded.length > 0) {
        return refusal(`$${expanded[0]} expands later, ${LITERAL_BROWSER}`);
    }
    const stated = Array.findFirst(invocation.args, ({ names }) => names.some((name) => SESSION.has(name)));
    const owned = Option.match(Option.fromNullishOr(request.agent ?? request.session), {
        onNone: (): readonly Verdict[] => [],
        onSome: (owner) =>
            Option.match(stated, {
                onNone: () => inserted(invocation, cli.end, ` ${sessionOf(owner)}`),
                onSome: ({ words, values: [name] }) =>
                    name === undefined || (name.known && (name.text === owner || name.text.startsWith(`${owner}-`))) ? [] : edited(invocation, { start: words[0].start, end: Array.lastNonEmpty(words).end }, `${sessionOf(owner)}-${name.raw}`, `${commandOf(invocation)} ${words[0].raw}`),
            }),
    });
    const extension = named === 'attach' && !invocation.has('--config') && (invocation.has('--extension') || invocation.values('--cdp').some((browser) => BROWSER_NAME.test(browser)));
    const configured = extension && Option.isSome(host.home) ? yield* host.exists(host.path.join(host.home.value, EXTENSION_CONFIG)) : false;
    const attached = verb === undefined || named !== 'attach' ? [] : [...respell({ renames: { '--cdp': '--extension' }, only: BROWSER_NAME, notify: true })(invocation), ...(configured ? inserted(invocation, verb.end, ` --config ~/${EXTENSION_CONFIG}`) : [])];
    if (extension && !configured) {
        return refusal(`playwright cli attach --extension reads ~/${EXTENSION_CONFIG}, run Forge's redeploy switch first, it writes the extension config`);
    }
    return Option.match(Record.get(named)(VERBS), { onNone: () => [...owned, ...attached], onSome: (reason) => (reason === undefined ? [] : refusal(`playwright cli ${named} ${reason}`)) });
});

const hiddenBrowser: Rule = ({ head, command }, { analysis: { declarations, invocations } }) => {
    const names = new Set(head.known ? [] : head.parts.flatMap(({ parameter }) => Option.toArray(Option.fromNullishOr(parameter?.name))));
    const assigning = declarations.filter((declaration) => declaration.source === command.source && declaration.end <= command.start && declaration.assigns.some(({ name }) => names.has(name)));
    const browser = invocations.some((other) => assigning.some(({ start, end }) => other.command.source === command.source && other.command.start >= start && other.command.end <= end) && [...other.words, ...other.rest].some(({ text }) => basename(text) === 'playwright'));
    return browser ? refusal(`$${[...names][0]} expands later, ${LITERAL_BROWSER}`) : [];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const BROWSERS: Ruleset = {
    programs: {
        open: { restAfter: ['--args'], rules: [openedUrl] },
        playwright: { flags: ['-s, --session=', '--config=', '--cdp=', '--extension='], rules: [browserCli] },
    },
    every: [hiddenBrowser],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { BROWSERS };
