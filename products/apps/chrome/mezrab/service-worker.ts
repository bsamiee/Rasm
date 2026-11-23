import iosShare from '@material-symbols/svg-400/outlined/ios_share.svg';
import shareElsewhere from '@material-symbols/svg-400/outlined/share.svg';
import shareWindows from '@material-symbols/svg-400/outlined/share_windows.svg';
import { Cause, Duration, Effect, Option, Schema, Struct } from 'effect';
import type messages from '#/_locales/en/messages.json';
import stylesheet from '#/toast.css?inline';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARKED = Duration.seconds(4);
const BADGES = { unreadable: '✕', uncopied: '✕', unconfirmed: '✓' } as const satisfies Partial<Record<keyof typeof messages, string>>;
const GLYPHS: Partial<Record<chrome.runtime.PlatformInfo['os'], string>> = { [browser.runtime.PlatformOs.MAC]: iosShare, [browser.runtime.PlatformOs.WIN]: shareWindows };

// --- [ERRORS] --------------------------------------------------------------------------

class CopyError extends Schema.TaggedError<CopyError, { readonly brand: unique symbol }>()('CopyError', { reason: Schema.Literals(Struct.keys(BADGES)), tabId: Schema.Int, cause: Schema.Defect() }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const toast = ({ css, text, lang, dir, zoom, share }: { readonly css: string; readonly text: string; readonly lang: string; readonly dir: string; readonly zoom: number; readonly share?: { readonly url: string; readonly label: string; readonly glyph: string } }) => {
    const host = document.querySelector('mezrab-toast') ?? document.createElement('mezrab-toast');
    const root = host.shadowRoot ?? host.attachShadow({ mode: 'open' });
    const output = root.querySelector('output') ?? document.createElement('output');
    const label = root.querySelector('span') ?? document.createElement('span');
    const button = root.querySelector('button') ?? document.createElement('button');
    const data = { url: share?.url ?? location.href, ...(document.title === '' ? {} : { title: document.title }) };
    const offer = share !== undefined && 'share' in navigator && 'canShare' in navigator && navigator.canShare(data) ? share : undefined;
    const sheet = new CSSStyleSheet();
    const removed = ({ newState }: ToggleEvent) => {
        const exiting = output.getAnimations();
        const left = () => {
            if (newState === 'closed' && !output.matches(':popover-open') && exiting.every(({ playState }) => playState === 'finished' || playState === 'idle')) {
                host.remove();
            }
        };
        exiting.forEach((animation) => {
            Object.assign(animation, { onfinish: left, oncancel: left });
        });
        left();
    };
    const closed = (previous: EventTarget | null) => {
        if (root.activeElement !== null && previous instanceof HTMLElement) {
            previous.focus();
        }
        output.hidePopover();
    };
    const dismissing = (previous: EventTarget | null) =>
        Object.assign(output, {
            onanimationend: () => closed(previous),
            onkeydown: ({ key }: KeyboardEvent) => {
                if (key === 'Escape') {
                    closed(previous);
                }
            },
            onclick: () => {
                if (offer !== undefined) {
                    output.getAnimations().forEach((animation) => animation.pause());
                    navigator
                        .share(data)
                        .then(() => closed(previous))
                        .catch(() => closed(previous));
                }
            },
        });
    sheet.replaceSync(css);
    root.adoptedStyleSheets = [sheet];
    if (!host.isConnected) {
        root.append(output);
        document.documentElement.append(host);
    }
    label.textContent = text;
    if (offer !== undefined) {
        button.style.setProperty('--glyph', `url("${offer.glyph}")`);
        Object.assign(button, { ariaLabel: offer.label, onfocus: ({ relatedTarget }: FocusEvent) => dismissing(relatedTarget) });
    }
    output.replaceChildren(label, ...(offer === undefined ? [] : [button]));
    Object.assign(output, { popover: 'manual', ariaLive: 'off', lang, dir, ontoggle: removed });
    output.style.zoom = `${1 / zoom}`;
    dismissing(null);
    output
        .getAnimations()
        .filter((animation) => animation instanceof CSSAnimation)
        .forEach((animation) => {
            Object.assign(animation, { currentTime: 0 });
        });
    output.togglePopover(true);
    output.ariaNotify(text);
    return output.matches(':popover-open');
};

const marked = ({ reason, tabId }: CopyError) => Effect.promise(() => Promise.all([browser.action.setBadgeText({ tabId, text: BADGES[reason] }), browser.action.setTitle({ tabId, title: browser.i18n.getMessage(reason) }), browser.alarms.create(`${tabId}`, { delayInMinutes: Duration.toMinutes(MARKED) })]));

const confirmed = (injected: readonly { readonly result?: unknown }[]) => injected.every(({ result }) => result === true);

// --- [COMPOSITION] ---------------------------------------------------------------------

browser.alarms.onAlarm.addListener(({ name }) => {
    Effect.gen(function* () {
        const tabId = yield* Schema.decodeEffect(Schema.FiniteFromString)(name);
        const title = yield* Effect.promise(() => browser.action.getTitle({}));
        yield* Effect.tryPromise({ try: () => Promise.all([browser.action.setBadgeText({ tabId, text: '' }), browser.action.setTitle({ tabId, title })]), catch: (cause) => new Cause.UnknownError(cause, `Tab ${tabId} closed before its badge cleared`) });
    }).pipe(Effect.ignore, Effect.runFork);
});

browser.action.onClicked.addListener(({ id: tabId = browser.tabs.TAB_ID_NONE, url }) => {
    Effect.gen(function* () {
        const link = yield* Effect.mapError(Effect.fromNullishOr(url), (cause) => CopyError.make({ reason: 'unreadable', tabId, cause }));
        yield* Effect.tryPromise({ try: () => navigator.clipboard.writeText(link), catch: (cause) => CopyError.make({ reason: 'uncopied', tabId, cause }) });
        const [zoom, { os }] = yield* Effect.all([Effect.promise(() => browser.tabs.getZoom(tabId)), Effect.promise(() => browser.runtime.getPlatformInfo())], { concurrency: 'unbounded' });
        const offered = Option.as(
            Option.filter(Option.fromNullishOr(URL.parse(link)), ({ protocol }) => protocol === 'http:' || protocol === 'https:'),
            { share: { url: link, label: browser.i18n.getMessage('share'), glyph: GLYPHS[os] ?? shareElsewhere } },
        );
        return yield* Effect.tryPromise({
            try: () => browser.scripting.executeScript({ target: { tabId }, func: toast, args: [{ css: stylesheet, text: browser.i18n.getMessage('copied'), lang: browser.i18n.getUILanguage(), dir: browser.i18n.getMessage('@@bidi_dir'), zoom, ...Option.getOrUndefined(offered) }], injectImmediately: true }),
            catch: (cause) => CopyError.make({ reason: 'unconfirmed', tabId, cause }),
        }).pipe(Effect.filterOrFail(confirmed, (injected) => CopyError.make({ reason: 'unconfirmed', tabId, cause: injected })));
    }).pipe(Effect.tapError(Effect.logWarning), Effect.catch(marked), Effect.runFork);
});
