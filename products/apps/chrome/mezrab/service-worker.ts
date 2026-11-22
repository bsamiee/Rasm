import iosShare from '@material-symbols/svg-400/outlined/ios_share.svg';
import share from '@material-symbols/svg-400/outlined/share.svg';
import shareWindows from '@material-symbols/svg-400/outlined/share_windows.svg';
import { Duration, Effect, Option, Schema, Struct } from 'effect';
import type messages from '#/_locales/en/messages.json';
import { toast } from '#/toast.ts';
import css from '#/toast.css?inline';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHOWN = { plain: Duration.seconds(4), action: Duration.seconds(8) } as const;
const BADGES = { unreadable: '✕', uncopied: '✕', unconfirmed: '✓' } as const satisfies Partial<Record<keyof typeof messages, string>>;
const GLYPHS: Partial<Record<chrome.runtime.PlatformInfo['os'], string>> = { [chrome.runtime.PlatformOs.MAC]: iosShare, [chrome.runtime.PlatformOs.WIN]: shareWindows };

// --- [ERRORS] --------------------------------------------------------------------------

class CopyError extends Schema.TaggedError<CopyError, { readonly brand: unique symbol }>()('CopyError', { reason: Schema.Literals(Struct.keys(BADGES)), tabId: Schema.Int, cause: Schema.Defect() }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const marked = ({ reason, tabId }: CopyError) =>
    Effect.promise(() => Promise.all([chrome.action.setBadgeText({ tabId, text: BADGES[reason] }), chrome.action.setTitle({ tabId, title: chrome.i18n.getMessage(reason) })])).pipe(
        Effect.andThen(Effect.sleep(SHOWN.plain)),
        Effect.andThen(Effect.promise(() => chrome.action.getTitle({}))),
        Effect.flatMap((title) => Effect.promise(() => Promise.all([chrome.action.setBadgeText({ tabId, text: '' }), chrome.action.setTitle({ tabId, title })]))),
    );

const confirmed = (injected: readonly { readonly result?: unknown }[]) => injected.every(({ result }) => result === true);

// --- [COMPOSITION] ---------------------------------------------------------------------

chrome.action.onClicked.addListener(({ id: tabId = chrome.tabs.TAB_ID_NONE, url }) => {
    Effect.gen(function* () {
        const link = yield* Effect.mapError(Effect.fromNullishOr(url), (cause) => CopyError.make({ reason: 'unreadable', tabId, cause }));
        yield* Effect.tryPromise({ try: () => navigator.clipboard.writeText(link), catch: (cause) => CopyError.make({ reason: 'uncopied', tabId, cause }) });
        const [zoom, { os }] = yield* Effect.all([Effect.promise(() => chrome.tabs.getZoom(tabId)), Effect.promise(() => chrome.runtime.getPlatformInfo())], { concurrency: 'unbounded' });
        const offered = Option.as(
            Option.filter(Option.fromNullishOr(URL.parse(link)), ({ protocol }) => protocol === 'http:' || protocol === 'https:'),
            { share: { url: link, label: chrome.i18n.getMessage('share'), glyph: GLYPHS[os] ?? share, shown: Duration.toMillis(SHOWN.action) } },
        );
        return yield* Effect.tryPromise({
            try: () =>
                chrome.scripting.executeScript({
                    target: { tabId },
                    func: toast,
                    args: [{ css, text: chrome.i18n.getMessage('copied'), lang: chrome.i18n.getUILanguage(), dir: chrome.i18n.getMessage('@@bidi_dir'), zoom, shown: Duration.toMillis(SHOWN.plain), ...Option.getOrUndefined(offered) }],
                    injectImmediately: true,
                }),
            catch: (cause) => CopyError.make({ reason: 'unconfirmed', tabId, cause }),
        }).pipe(Effect.filterOrFail(confirmed, (injected) => CopyError.make({ reason: 'unconfirmed', tabId, cause: injected })));
    }).pipe(Effect.tapError(Effect.logWarning), Effect.catch(marked), Effect.runFork);
});
