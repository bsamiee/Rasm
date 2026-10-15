import { type RefObject, useEffect, useEffectEvent, useRef, useState } from 'react';
import type { Entry, Portfolio } from '../model/document.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Navigation {
    readonly active: string | undefined;
    readonly selected: (entry: typeof Entry.Type) => string | undefined;
    readonly select: (project: string, composition: string | undefined) => void;
    readonly jump: (entry: typeof Entry.Type) => void;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fragment = (project: string, composition?: string): string => `#${new URLSearchParams({ project, ...(composition === undefined ? {} : { composition }) })}`;
const useNavigation = (portfolio: typeof Portfolio.Type, work: RefObject<HTMLElement | null>, sticky: RefObject<HTMLDivElement | null>, foreground: boolean): Navigation => {
    const [active, setActive] = useState<string>();
    const [selections, setSelections] = useState<Readonly<Record<string, string>>>({});
    const intersections = useRef(new Set<string>());
    const selected = (entry: typeof Entry.Type): string | undefined => (entry.compositions.find((item) => item.id === selections[entry.id]) ?? entry.compositions[0])?.id;
    const select = (project: string, composition: string | undefined): void => {
        const target = fragment(project, composition);
        if (location.hash !== target) {
            history.pushState(null, '', target);
        }
        history.scrollRestoration = 'manual';
        if (composition !== undefined) {
            setSelections((value) => ({ ...value, [project]: composition }));
        }
    };
    const jump = (entry: typeof Entry.Type): void => {
        select(entry.id, selected(entry));
        setActive(entry.id);
        const section = document.querySelector(`#${CSS.escape(entry.id)}`);
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
        if (!foreground) {
            return;
        }
        const entry = entries.findLast(({ id }) => intersections.current.has(id));
        setActive(entry?.id);
        const page = work.current !== null && work.current.getBoundingClientRect().top > Number.parseFloat(getComputedStyle(document.documentElement).scrollPaddingTop) ? '#top' : '#work';
        const target = entry === undefined ? page : fragment(entry.id, selected(entry));
        history.scrollRestoration = entry === undefined ? 'auto' : 'manual';
        if (location.hash !== target) {
            history.replaceState(null, '', target);
        }
    });
    const restore = useEffectEvent(() => {
        if (location.hash === '#top' || location.hash === '#work' || location.hash === '') {
            history.scrollRestoration = 'auto';
            setActive(undefined);
            return;
        }
        history.scrollRestoration = 'manual';
        const params = new URLSearchParams(location.hash.slice(1));
        const entry = portfolio.entries.find((item) => item.id === params.get('project'));
        if (entry === undefined) {
            setActive(undefined);
            history.replaceState(null, '', '#work');
            work.current?.scrollIntoView({ behavior: 'instant' });
            return;
        }
        const composition = (entry.compositions.find((item) => item.id === params.get('composition')) ?? entry.compositions[0])?.id;
        if (composition !== undefined) {
            setSelections((value) => ({ ...value, [entry.id]: composition }));
        }
        if (active !== entry.id) {
            document.querySelector(`#${CSS.escape(entry.id)}`)?.scrollIntoView({ behavior: 'instant' });
        }
        setActive(entry.id);
        const target = fragment(entry.id, composition);
        if (location.hash !== target) {
            history.replaceState(null, '', target);
        }
    });
    useEffect(() => {
        let height = sticky.current?.getBoundingClientRect().height ?? 0;
        let observer: IntersectionObserver | undefined;
        const observe = (): void => {
            observer?.disconnect();
            intersections.current.clear();
            const line = Number.parseFloat(getComputedStyle(document.documentElement).scrollPaddingTop);
            observer = new IntersectionObserver((records) => track(records, portfolio.entries), { rootMargin: `${-line}px 0px ${line + 1 - window.innerHeight}px 0px` });
            if (work.current !== null) {
                observer.observe(work.current);
            }
            work.current?.querySelectorAll(':scope > section').forEach(observer.observe, observer);
        };
        document.documentElement.style.setProperty('--sticky-height', `${height}px`);
        observe();
        restore();
        const size = new ResizeObserver((entries) => {
            entries.forEach((entry) => {
                ({ height } = entry.contentRect);
                document.documentElement.style.setProperty('--sticky-height', `${height}px`);
                observe();
            });
        });
        if (sticky.current !== null) {
            size.observe(sticky.current);
        }
        const events = new AbortController();
        globalThis.addEventListener('popstate', restore, { signal: events.signal });
        globalThis.addEventListener('hashchange', restore, { signal: events.signal });
        globalThis.addEventListener('resize', observe, { signal: events.signal });
        return (): void => {
            events.abort();
            observer?.disconnect();
            size.disconnect();
        };
    }, [portfolio.entries, sticky, work]);
    return { active, selected, select, jump };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { fragment, type Navigation, useNavigation };
