import { ArrowRight } from 'lucide-react';
import type { ReactNode } from 'react';
import { entryTitle } from '../media/display.ts';
import type { Portfolio } from '../model/document.ts';
import { fragment, type Navigation, numeral } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

function ProjectIndex({ portfolio, navigation }: { portfolio: typeof Portfolio.Type; navigation: Navigation }): ReactNode {
    const link = 'flex min-h-11 items-baseline justify-between gap-2.5 border-line border-t py-[13px] text-sm leading-[1.45] hover:text-accent-text aria-[current=location]:font-semibold aria-[current=location]:text-accent-text';
    return (
        <aside aria-label="Project index" className="fixed top-[157px] bottom-[35px] left-[38px] flex w-44 flex-col max-md:hidden xl:w-[12%]">
            <div className="eyebrow flex justify-between pb-[23px] text-muted">
                Index <span>{numeral(portfolio.entries.length)}</span>
            </div>
            <nav className="-mx-1 overflow-auto overscroll-contain px-1 pb-1 [scrollbar-width:thin]">
                {portfolio.entries.length > 0 ? (
                    portfolio.entries.map((entry, index) => {
                        const current = navigation.active === entry.id;
                        return (
                            <a
                                aria-current={current ? 'location' : undefined}
                                className={link}
                                href={fragment(entry.id)}
                                key={entry.id}
                                onClick={(event): void => {
                                    event.preventDefault();
                                    navigation.jump(entry.id);
                                }}
                                ref={current ? (node: HTMLAnchorElement | null): void => node?.scrollIntoView({ block: 'nearest' }) : undefined}
                            >
                                <span className="wrap-anywhere">
                                    {current && <ArrowRight className="-mt-1 mr-1 inline size-5 align-text-bottom" strokeLinecap="butt" strokeLinejoin="miter" />}
                                    {entryTitle(entry)}
                                </span>
                                <span className="shrink-0 tabular-nums">{numeral(index + 1)}</span>
                            </a>
                        );
                    })
                ) : (
                    <a className={link} href="#work">
                        <span>Selected work</span>
                        <span>00</span>
                    </a>
                )}
            </nav>
            <div className="eyebrow mt-auto pt-6 text-muted">
                Architecture
                <br />
                Projects &amp; studies
            </div>
        </aside>
    );
}
function StickyBar({ portfolio, navigation, children }: { portfolio: typeof Portfolio.Type; navigation: Navigation; children: ReactNode }): ReactNode {
    return (
        <div
            className="sticky top-0 z-30"
            ref={(node: HTMLDivElement): (() => void) => {
                const observer = new ResizeObserver(() => document.documentElement.style.setProperty('--sticky-height', `${node.getBoundingClientRect().height}px`));
                observer.observe(node);
                return (): void => observer.disconnect();
            }}
        >
            {children}
            {portfolio.entries.length > 0 && (
                <label className="hidden items-center justify-between gap-4 border-line border-b bg-background px-[22px] py-2 max-md:flex">
                    <span className="eyebrow">Index</span>
                    <select className="control min-w-0 flex-1 truncate border-0 shadow-none" onChange={(event): void => navigation.jump(event.target.value)} value={navigation.active ?? ''}>
                        <option disabled={true} value="">
                            Select a project
                        </option>
                        {portfolio.entries.map((entry, index) => (
                            <option key={entry.id} value={entry.id}>
                                {numeral(index + 1)} — {entryTitle(entry)}
                            </option>
                        ))}
                    </select>
                </label>
            )}
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ProjectIndex, StickyBar };
