import iosShare from '@material-symbols/svg-400/outlined/ios_share.svg';
import shareElsewhere from '@material-symbols/svg-400/outlined/share.svg';
import shareWindows from '@material-symbols/svg-400/outlined/share_windows.svg';
import { Cause, Duration, Effect, Option, Schema, Struct } from 'effect';
import type messages from '#/_locales/en/messages.json';
import { toast } from '#/toast';
import stylesheet from '#/toast.css?inline';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BADGES = { unreadable: '✕', uncopied: '✕', unconfirmed: '✓' } as const satisfies Partial<Record<keyof typeof messages, string>>;
const GLYPHS: Partial<Record<chrome.runtime.PlatformInfo['os'], string>> = { [browser.runtime.PlatformOs.MAC]: iosShare, [browser.runtime.PlatformOs.WIN]: shareWindows };

// --- [ERRORS] --------------------------------------------------------------------------

class CopyError extends Schema.TaggedError<CopyError, { readonly brand: unique symbol }>()('CopyError', { reason: Schema.Literals(Struct.keys(BADGES)), tabId: Schema.Int, cause: Schema.Defect() }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const marked = ({ reason, tabId }: CopyError) => Effect.promise(() => Promise.all([browser.action.setBadgeText({ tabId, text: BADGES[reason] }), browser.action.setTitle({ tabId, title: browser.i18n.getMessage(reason) }), browser.alarms.create(`${tabId}`, { delayInMinutes: Duration.toMinutes(Duration.seconds(4)) })]));

const confirmed = (injected: readonly { readonly result?: unknown }[]) => injected.every(({ result }) => result === true);

// --- [COMPOSITION] ---------------------------------------------------------------------

browser.alarms.onAlarm.addListener(({ name }) => {
    Effect.gen(function* () {
        const tabId = yield* Schema.decodeEffect(Schema.FiniteFromString)(name);
        const title = yield* Effect.promise(() => browser.action.getTitle({}));
        yield* Effect.tryPromise({ try: () => Promise.all([browser.action.setBadgeText({ tabId, text: '' }), browser.action.setTitle({ tabId, title })]), catch: (cause) => new Cause.UnknownError(cause, `Tab ${tabId} closed before its badge cleared`) });
    }).pipe(Effect.ignore, Effect.runFork);
});

browser.tabs.onZoomChange.addListener(({ tabId, newZoomFactor }) => {
    Effect.tryPromise({ try: () => browser.scripting.executeScript({ target: { tabId }, func: toast.zoomed, args: [newZoomFactor], injectImmediately: true }), catch: (cause) => new Cause.UnknownError(cause, `Tab ${tabId} grants no script access, so it shows no toast to rescale`) }).pipe(Effect.ignore, Effect.runFork);
});

browser.action.onClicked.addListener(({ id: tabId = browser.tabs.TAB_ID_NONE, url }) => {
    Effect.gen(function* () {
        const link = yield* Effect.mapError(Effect.fromNullishOr(url), (cause) => CopyError.make({ reason: 'unreadable', tabId, cause }));
        yield* Effect.tryPromise({ try: () => navigator.clipboard.writeText(link), catch: (cause) => CopyError.make({ reason: 'uncopied', tabId, cause }) });
        const [zoom, { os }] = yield* Effect.promise(() => Promise.all([browser.tabs.getZoom(tabId), browser.runtime.getPlatformInfo()]));
        const web = Option.filter(Option.fromNullishOr(URL.parse(link)), ({ protocol }) => protocol === 'http:' || protocol === 'https:');
        const offered = Option.as(web, { share: { url: link, label: browser.i18n.getMessage('share'), glyph: GLYPHS[os] ?? shareElsewhere } });
        return yield* Effect.tryPromise({
            try: () => browser.scripting.executeScript({ target: { tabId }, func: toast, args: [{ css: stylesheet, text: browser.i18n.getMessage('copied'), lang: browser.i18n.getUILanguage(), dir: browser.i18n.getMessage('@@bidi_dir'), zoom, ...Option.getOrUndefined(offered) }], injectImmediately: true }),
            catch: (cause) => CopyError.make({ reason: 'unconfirmed', tabId, cause }),
        }).pipe(Effect.filterOrFail(confirmed, (injected) => CopyError.make({ reason: 'unconfirmed', tabId, cause: injected })));
    }).pipe(Effect.tapError(Effect.logWarning), Effect.catch(marked), Effect.runFork);
});
