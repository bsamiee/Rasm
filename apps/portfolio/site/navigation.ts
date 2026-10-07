import { type RefObject, useEffect, useEffectEvent, useState } from 'react';
import type { Portfolio } from '../model/document.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Navigation {
    readonly active: string | undefined;
    readonly selections: Readonly<Record<string, string>>;
    readonly select: (project: string, composition: string | undefined) => void;
    readonly jump: (project: string) => void;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fragment = (project: string, composition?: string): string => `#${new URLSearchParams({ project, ...(composition ? { composition } : {}) })}`;
const numeral = (value: number): string => String(value).padStart(2, '0');
const useNavigation = (portfolio: typeof Portfolio.Type, work: RefObject<HTMLElement | null>): Navigation => {
    const activationShare = 0.22;
    const introShare = 0.3;
    const [active, setActive] = useState<string>();
    const [selections, setSelections] = useState<Readonly<Record<string, string>>>({});
    const chapter = (project: string): HTMLElement | null => document.querySelector(`#${CSS.escape(project)}`);
    const shown = (project: string): string | undefined => selections[project] ?? portfolio.entries.find((entry) => entry.id === project)?.compositions[0]?.id;
    const select = (project: string, composition: string | undefined): void => {
        const target = fragment(project, composition);
        if (location.hash !== target) {
            history.pushState(null, '', target);
        }
        if (composition) {
            setSelections((value) => ({ ...value, [project]: composition }));
        }
    };
    const jump = (project: string): void => {
        select(project, shown(project));
        setActive(project);
        const section = chapter(project);
        section?.querySelector('h2')?.focus({ preventScroll: true });
        section?.scrollIntoView();
    };
    const track = useEffectEvent(() => {
        const line = Number.parseFloat(document.documentElement.style.getPropertyValue('--sticky-height')) + window.innerHeight * activationShare;
        setActive(
            portfolio.entries.find((entry) => {
                const bounds = chapter(entry.id)?.getBoundingClientRect();
                return bounds !== undefined && bounds.top <= line && bounds.bottom > line;
            })?.id,
        );
    });
    const syncFragment = useEffectEvent(() => {
        const page = work.current && work.current.getBoundingClientRect().top > window.innerHeight * introShare ? '#top' : '#work';
        const target = active ? fragment(active, shown(active)) : page;
        if (location.hash !== target) {
            history.replaceState(null, '', target);
        }
    });
    const restore = useEffectEvent(() => {
        const params = new URLSearchParams(location.hash.slice(1));
        const composition = params.get('composition');
        const entry = portfolio.entries.find((item) => item.id === params.get('project'));
        if (!entry) {
            return;
        }
        if (composition && entry.compositions.some((item) => item.id === composition)) {
            setSelections((value) => ({ ...value, [entry.id]: composition }));
        }
        if (active !== entry.id) {
            chapter(entry.id)?.scrollIntoView({ behavior: 'instant' });
        }
        setActive(entry.id);
    });
    useEffect(() => {
        restore();
        track();
        const events = new AbortController();
        globalThis.addEventListener('popstate', restore, { signal: events.signal });
        globalThis.addEventListener('scroll', track, { passive: true, signal: events.signal });
        globalThis.addEventListener('resize', track, { signal: events.signal });
        globalThis.addEventListener('scrollend', syncFragment, { signal: events.signal });
        return (): void => events.abort();
    }, []);
    return { active, selections, select, jump };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { fragment, type Navigation, numeral, useNavigation };
