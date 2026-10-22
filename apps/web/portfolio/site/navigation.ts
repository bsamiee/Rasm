import { type RefObject, useEffect, useEffectEvent, useRef, useState, useSyncExternalStore } from 'react';
import type { Entry, Portfolio } from '../model/portfolio.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const fragment = (project: string, composition?: string) => `#${new URLSearchParams({ project, ...(composition === undefined ? {} : { composition }) })}`;
const useNavigation = (portfolio: typeof Portfolio.Type, work: RefObject<HTMLElement | null>, sticky: RefObject<HTMLDivElement | null>, foreground: boolean) => {
    const hash = useSyncExternalStore(
        (notify) => {
            navigation.addEventListener('currententrychange', notify);
            return () => navigation.removeEventListener('currententrychange', notify);
        },
        () => location.hash,
        () => '',
    );
    const [selections, setSelections] = useState<Readonly<Record<string, string>>>({});
    const intersections = useRef(new Set<string>());
    const params = new URLSearchParams(hash.slice(1));
    const current = portfolio.entries.find((item) => item.id === params.get('project'));
    const selected = (entry: typeof Entry.Type) => (entry.compositions.find((item) => item.id === ((entry === current && params.get('composition')) || selections[entry.id])) ?? entry.compositions[0])?.id;
    const write = (method: 'pushState' | 'replaceState', target: string) => {
        history.scrollRestoration = target.startsWith('#project') ? 'manual' : 'auto';
        if (location.hash !== target) {
            history[method](null, '', target);
        }
    };
    const select = (project: string, composition: string | undefined) => {
        write('pushState', fragment(project, composition));
        if (composition !== undefined) {
            setSelections((value) => ({ ...value, [project]: composition }));
        }
    };
    const jump = (entry: typeof Entry.Type) => {
        select(entry.id, selected(entry));
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
        const entry = entries.findLast(({ id }) => intersections.current.has(id));
        const page = work.current !== null && work.current.getBoundingClientRect().top > Number.parseFloat(getComputedStyle(document.documentElement).scrollPaddingTop) ? '#top' : '#work';
        const shown = current === undefined ? null : document.querySelector(`#${CSS.escape(current.id)}`);
        const held = shown !== null && shown.getBoundingClientRect().top < globalThis.innerHeight && Math.ceil(globalThis.scrollY + globalThis.innerHeight) >= document.documentElement.scrollHeight;
        if (foreground && !held) {
            write('replaceState', entry === undefined ? page : fragment(entry.id, selected(entry)));
        }
    });
    const restore = useEffectEvent((event?: Event) => {
        const target = new URLSearchParams(location.hash.slice(1));
        const entry = portfolio.entries.find((item) => item.id === target.get('project'));
        if (['', '#top', '#work'].includes(location.hash)) {
            history.scrollRestoration = 'auto';
            return;
        }
        if (entry === undefined) {
            write('replaceState', '#work');
            work.current?.scrollIntoView({ behavior: 'instant' });
            return;
        }
        if (event === undefined || entry.id !== current?.id) {
            document.querySelector(`#${CSS.escape(entry.id)}`)?.scrollIntoView({ behavior: 'instant' });
        }
        write('replaceState', fragment(entry.id, (entry.compositions.find((item) => item.id === target.get('composition')) ?? entry.compositions[0])?.id));
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
    return { active: current?.id, selected, select, jump };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { fragment, useNavigation };
