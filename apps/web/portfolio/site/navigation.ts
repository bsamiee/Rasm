import { Array, Option, Schema } from 'effect';
import { type RefObject, useEffect, useEffectEvent, useRef, useState, useSyncExternalStore } from 'react';
import { CompositionId } from '#/model/placement.ts';
import { type Entry, EntryId, type Portfolio } from '#/model/portfolio.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Params = Schema.fromURLSearchParams(Schema.Struct({ project: EntryId, composition: Schema.optional(CompositionId) }));
const Fragment = {
    top: '#top',
    work: '#work',
    of: (project: typeof EntryId.Type, composition?: typeof CompositionId.Type) => Option.match(Schema.encodeOption(Params)({ project, composition }), { onNone: () => Fragment.work, onSome: (params) => `#${params}` }),
    read: (hash: string) => Schema.decodeOption(Params)(new URLSearchParams(hash.slice(1))),
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const useNavigation = (portfolio: typeof Portfolio.Type, work: RefObject<HTMLElement | null>, sticky: RefObject<HTMLDivElement | null>, foreground: boolean) => {
    const hash = useSyncExternalStore(
        (notify) => {
            navigation.addEventListener('currententrychange', notify);
            return () => navigation.removeEventListener('currententrychange', notify);
        },
        () => location.hash,
        () => '',
    );
    const [selections, setSelections] = useState<Readonly<Partial<Record<typeof EntryId.Type, typeof CompositionId.Type>>>>({});
    const intersections = useRef(new Set<string>());
    const located = Option.flatMap(({ project, composition }: typeof Params.Type) =>
        Option.map(
            Array.findFirst(portfolio.entries, ({ id }) => id === project),
            (entry) => ({ entry, composition }),
        ),
    );
    const current = located(Fragment.read(hash));
    const first = (entry: typeof Entry.Type, composition: typeof CompositionId.Type | undefined) => (entry.compositions.find(({ id }) => id === composition) ?? entry.compositions[0])?.id;
    const selected = (entry: typeof Entry.Type) =>
        first(
            entry,
            Option.getOrElse(
                Option.flatMap(
                    Option.filter(current, (shown) => shown.entry === entry),
                    ({ composition }) => Option.fromUndefinedOr(composition),
                ),
                () => selections[entry.id],
            ),
        );
    const write = (method: 'pushState' | 'replaceState', target: string) => {
        history.scrollRestoration = Option.isSome(Fragment.read(target)) ? 'manual' : 'auto';
        if (location.hash !== target) {
            history[method](null, '', target);
        }
    };
    const select = (project: typeof EntryId.Type, composition: typeof CompositionId.Type | undefined) => {
        write('pushState', Fragment.of(project, composition));
        if (composition !== undefined) {
            setSelections((value) => ({ ...value, [project]: composition }));
        }
    };
    const jump = (entry: typeof Entry.Type) => {
        select(entry.id, selected(entry));
        const section = document.getElementById(entry.id);
        section?.querySelector('h3')?.focus({ preventScroll: true });
        section?.scrollIntoView();
    };
    const track = useEffectEvent((records: readonly IntersectionObserverEntry[], entries: readonly (typeof Entry.Type)[]) => {
        records.forEach(({ isIntersecting, target: { id } }) => {
            if (isIntersecting) {
                intersections.current.add(id);
            } else {
                intersections.current.delete(id);
            }
        });
        const entry = entries.findLast(({ id }) => intersections.current.has(id));
        const page = work.current !== null && work.current.getBoundingClientRect().top > Number.parseFloat(getComputedStyle(document.documentElement).scrollPaddingTop) ? Fragment.top : Fragment.work;
        const held = Option.exists(current, ({ entry: shown }) => {
            const section = document.getElementById(shown.id);
            return section !== null && section.getBoundingClientRect().top < globalThis.innerHeight && Math.ceil(globalThis.scrollY + globalThis.innerHeight) >= document.documentElement.scrollHeight;
        });
        if (foreground && !held) {
            write('replaceState', entry === undefined ? page : Fragment.of(entry.id, selected(entry)));
        }
    });
    const restore = useEffectEvent((event?: Event) => {
        if (['', Fragment.top, Fragment.work].includes(location.hash)) {
            history.scrollRestoration = 'auto';
            return;
        }
        Option.match(located(Fragment.read(location.hash)), {
            onNone: () => {
                write('replaceState', Fragment.work);
                work.current?.scrollIntoView({ behavior: 'instant' });
            },
            onSome: ({ entry, composition }) => {
                if (event === undefined || !Option.exists(current, (shown) => shown.entry.id === entry.id)) {
                    document.getElementById(entry.id)?.scrollIntoView({ behavior: 'instant' });
                }
                write('replaceState', Fragment.of(entry.id, first(entry, composition)));
            },
        });
    });
    useEffect(() => {
        const { current: bar } = sticky;
        const { current: section } = work;
        if (bar === null || section === null) {
            return;
        }
        const events = new AbortController();
        let observer: IntersectionObserver | undefined;
        const observe = () => {
            const line = Number.parseFloat(getComputedStyle(document.documentElement).scrollPaddingTop);
            const next = new IntersectionObserver((records) => track(records, portfolio.entries), { rootMargin: `${-line}px 0px ${line + 1 - globalThis.innerHeight}px 0px` });
            observer?.disconnect();
            intersections.current.clear();
            [section, ...section.querySelectorAll(':scope > section')].forEach(next.observe, next);
            observer = next;
        };
        const measure = () => {
            document.documentElement.style.setProperty('--sticky-height', `${bar.getBoundingClientRect().height}px`);
            observe();
        };
        const size = new ResizeObserver(measure);
        measure();
        restore();
        size.observe(bar);
        ['popstate', 'hashchange'].forEach((name) => {
            globalThis.addEventListener(name, restore, { signal: events.signal });
        });
        globalThis.addEventListener('resize', observe, { signal: events.signal });
        return () => {
            events.abort();
            observer?.disconnect();
            size.disconnect();
        };
    }, [portfolio.entries, sticky, work]);
    return { active: Option.getOrUndefined(Option.map(current, ({ entry }) => entry.id)), selected, select, jump };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Fragment, useNavigation };
