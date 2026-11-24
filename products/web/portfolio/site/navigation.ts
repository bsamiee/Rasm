import { Array, Data, Effect, Equal, Function, Option, Queue, Schema, Stream, Struct } from 'effect';
import { Atom, type AtomRegistry } from 'effect/reactivity';
import { Composition } from '#/model/placement';
import { Entry } from '#/model/portfolio';
import { editor, shown } from '#/site/state';

// --- [TYPES] ---------------------------------------------------------------------------

type Place = Data.TaggedEnum<{ Anchor: { readonly id: 'top' | 'work' }; Project: Position }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const silent = { scroll: 'manual', focusReset: 'manual' } as const satisfies NavigationInterceptOptions;
const instant = { behavior: 'instant' } as const satisfies ScrollIntoViewOptions;

// --- [MODELS] --------------------------------------------------------------------------

class Position extends Schema.Opaque<Position, unknown>()(Schema.fromURLSearchParams(Schema.Struct({ project: Entry.fields.id, composition: Schema.optional(Composition.fields.id) }))) {}
const Place = Data.taggedEnum<Place>();
const top = Place.Anchor({ id: 'top' });
const work = Place.Anchor({ id: 'work' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const href = Place.$match({ Anchor: ({ id }) => `#${id}`, Project: ({ project, composition }) => Option.match(Schema.encodeOption(Position)({ project, composition }), { onNone: () => `#${work.id}`, onSome: (params) => `#${params}` }) });
const read = (hash: string): Option.Option<Place> => {
    const text = hash.slice(1);
    return text === ''
        ? Option.some(top)
        : Option.orElse(
              Array.findFirst([top, work], ({ id }) => id === text),
              () => Option.map(Schema.decodeOption(Position)(new URLSearchParams(text)), Place.Project),
          );
};
const project = (place: Option.Option<Place>) => Option.map(Option.filter(place, Place.$is('Project')), Struct.get('project'));
const visit = (place: Place, mode: NavigationHistoryBehavior) => {
    const target = href(place);
    history.scrollRestoration = Place.$is('Project')(place) ? 'manual' : 'auto';
    if (location.hash !== target) {
        navigation.navigate(target, { history: mode, info: silent });
    }
};
const line = () => Number.parseFloat(getComputedStyle(document.documentElement).scrollPaddingTop);
const observed = <A>(observe: (emit: (value: A) => void) => { readonly disconnect: () => void }) =>
    Stream.callback<A>((queue) =>
        Effect.acquireRelease(
            Effect.sync(() => observe(Function.flow((value: A) => Queue.offerUnsafe(queue, value), Function.constVoid))),
            (observer) => Effect.sync(() => observer.disconnect()),
        ),
    );
const resized = (bar: HTMLElement) =>
    Stream.merge(
        observed<unknown>((emit) => {
            const sized = new ResizeObserver(emit);
            sized.observe(bar);
            return sized;
        }),
        Stream.fromEventListener(globalThis, 'resize'),
    );
const crossed = (bar: HTMLElement, targets: readonly HTMLElement[]) =>
    observed<readonly IntersectionObserverEntry[]>((emit) => {
        document.documentElement.style.setProperty('--sticky-height', `${bar.getBoundingClientRect().height}px`);
        const margin = line();
        const crossing = new IntersectionObserver(emit, { rootMargin: `${-margin}px 0px ${margin + 1 - globalThis.innerHeight}px 0px` });
        targets.forEach((target) => crossing.observe(target));
        return crossing;
    }).pipe(
        Stream.scan(
            () => new Set<Element>(),
            (held, records) => held.union(new Set(records.filter(({ isIntersecting }) => isIntersecting).map(({ target }) => target))).difference(new Set(records.filter(({ isIntersecting }) => !isIntersecting).map(({ target }) => target))),
        ),
        Stream.drop(1),
    );

// --- [SERVICES] ------------------------------------------------------------------------

const element = () =>
    Atom.writable(
        () => Option.none<HTMLElement>(),
        (context, mounted: HTMLElement | null) => context.setSelf(Option.map(Option.fromNullOr(mounted), Equal.byReferenceUnsafe)),
    ).pipe(Atom.keepAlive);
const stickyBar = element();
const workSection = element();
const chapterSection = Atom.family((_: Entry['id']) => element());
const selections = Atom.make<Readonly<Partial<Record<Entry['id'], Composition['id']>>>>({});
const settle = (registry: AtomRegistry.AtomRegistry, from: Option.Option<Place>) => {
    const destination = read(location.hash);
    const arrived = project(destination);
    const mounted = Option.flatMap(arrived, (id) => registry.get(chapterSection(id)));
    Option.match(mounted, {
        onSome: (chapter) => {
            history.scrollRestoration = 'manual';
            if (!Equal.equals(arrived, project(from))) {
                chapter.scrollIntoView(instant);
            }
        },
        onNone: () => {
            if (!Option.exists(destination, Place.$is('Anchor'))) {
                visit(work, 'replace');
                Option.match(registry.get(workSection), { onNone: Function.constVoid, onSome: (section) => section.scrollIntoView(instant) });
            }
        },
    });
};
const fragment = Atom.make((get) => {
    const listeners = new AbortController();
    const settled = ({ from }: NavigationCurrentEntryChangeEvent) => settle(get.registry, read(new URL(from.url ?? location.href).hash));
    navigation.addEventListener('currententrychange', () => get.setSelf(location.hash), { signal: listeners.signal });
    navigation.addEventListener(
        'navigate',
        (event) => {
            if (event.info === silent && event.canIntercept) {
                event.intercept(silent);
            }
            if (event.info !== silent && event.hashChange) {
                navigation.addEventListener('currententrychange', settled, { once: true, signal: listeners.signal });
            }
        },
        { signal: listeners.signal },
    );
    get.addFinalizer(() => listeners.abort());
    return location.hash;
}).pipe(Atom.withServerValue(() => ''));
const entries = Atom.make((get) => new Map(get(shown).entries.map((entry) => [entry.id, entry] as const)));
const position = Atom.make((get) => Option.filter(Option.filter(read(get(fragment)), Place.$is('Project')), ({ project: id }) => get(entries).has(id)));
const activeEntry = Atom.make((get) => Option.map(get(position), Struct.get('project'))).pipe(Atom.withServerValue(() => Option.none()));
const shownComposition = (entry: Entry | undefined, stated: Composition['id'] | undefined) =>
    Array.findFirst(entry?.compositions ?? [], ({ id }) => id === stated).pipe(
        Option.orElse(() => Array.head(entry?.compositions ?? [])),
        Option.map(Struct.get('id')),
    );
const selectedComposition = Atom.family((id: Entry['id']) =>
    Atom.make((get) => shownComposition(get(entries).get(id), Option.getOrUndefined(Option.filter(get(position), ({ project: current }) => current === id))?.composition ?? get(selections)[id])).pipe(Atom.withServerValue((get) => shownComposition(get(entries).get(id), undefined))),
);
const selectComposition = Atom.writable(Function.constVoid, (context, { project: id, composition }: Position) => {
    visit(Place.Project({ project: id, composition }), 'push');
    context.set(selections, { ...context.get(selections), ...(composition !== undefined && { [id]: composition }) });
});
const jumpToEntry = Atom.writable(Function.constVoid, (context, { id }: Entry) => {
    context.set(selectComposition, { project: id, composition: Option.getOrUndefined(context.get(selectedComposition(id))) });
    Option.match(context.get(chapterSection(id)), {
        onNone: Function.constVoid,
        onSome: (chapter) => {
            (chapter.ariaLabelledByElements ?? []).filter((heading) => heading instanceof HTMLElement).forEach((heading) => heading.focus({ preventScroll: true }));
            chapter.scrollIntoView();
        },
    });
});
const track = (registry: AtomRegistry.AtomRegistry, section: HTMLElement, held: ReadonlySet<Element>) => {
    const current = Option.flatMap(registry.get(position), ({ project: id }) => registry.get(chapterSection(id)));
    const pinned = Option.exists(current, (chapter) => chapter.getBoundingClientRect().top < globalThis.innerHeight && Math.ceil(globalThis.scrollY + globalThis.innerHeight) >= document.documentElement.scrollHeight);
    if (registry.get(editor) !== 'open' && !pinned) {
        const reached = Array.findLast(registry.get(shown).entries, (entry) => Option.exists(registry.get(chapterSection(entry.id)), (chapter) => held.has(chapter)));
        visit(Option.match(reached, { onNone: () => (section.getBoundingClientRect().top > line() ? top : work), onSome: (entry) => Place.Project({ project: entry.id, composition: Option.getOrUndefined(registry.get(selectedComposition(entry.id))) }) }), 'replace');
    }
};
const follow =
    (registry: AtomRegistry.AtomRegistry, section: HTMLElement) =>
    ([held, index]: readonly [ReadonlySet<Element>, number]) =>
        Effect.sync(() => (index === 0 ? settle(registry, Option.none()) : track(registry, section, held)));
const scrollTracking = Atom.make((get) =>
    Option.match(Option.all({ bar: get(stickyBar), section: get(workSection) }), {
        onNone: () => Effect.void,
        onSome: ({ bar, section }) => {
            const targets = [section, ...Array.getSomes(get(shown).entries.map((entry) => get(chapterSection(entry.id))))];
            return resized(bar).pipe(
                Stream.switchMap(() => crossed(bar, targets)),
                Stream.zipWithIndex,
                Stream.runForEach(follow(get.registry, section)),
            );
        },
    }),
);
const topHref = href(top);
const workAnchor = { id: work.id, href: href(work) } as const;
const entryHref = ({ id }: Entry) => href(Place.Project({ project: id }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { activeEntry, chapterSection, entryHref, jumpToEntry, scrollTracking, selectComposition, selectedComposition, stickyBar, topHref, workAnchor, workSection };
