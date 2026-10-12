import type { ReactNode, Ref } from 'react';
import { entryTitle, numeral } from '../media/display.ts';
import type { Portfolio } from '../model/document.ts';
import { fragment, type Navigation } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

function ProjectIndex({ portfolio, navigation }: { portfolio: typeof Portfolio.Type; navigation: Navigation }): ReactNode {
    return (
        <nav aria-label="Work index" className="project-index">
            <a className="eyebrow min-h-11 self-start pb-[23px] text-muted hover:text-accent-text" href="#work">
                Index
            </a>
            <div className="-mx-1 grid grid-cols-[max-content_minmax(0,1fr)] overflow-auto overscroll-contain px-1 pb-1 [scrollbar-width:thin]">
                {portfolio.entries.length > 0 ? (
                    portfolio.entries.map((entry, index) => {
                        const current = navigation.active === entry.id;
                        return (
                            <a
                                aria-current={current ? 'location' : undefined}
                                className="group index-link"
                                href={fragment(entry.id)}
                                key={entry.id}
                                onClick={(event): void => {
                                    if (!(event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) && event.button === 0) {
                                        event.preventDefault();
                                        navigation.jump(entry);
                                    }
                                }}
                                ref={
                                    current
                                        ? (node: HTMLAnchorElement | null): void => {
                                              const scrollport = node?.parentElement;
                                              if (!(node && scrollport)) {
                                                  return;
                                              }
                                              const item = node.getBoundingClientRect();
                                              const bounds = scrollport.getBoundingClientRect();
                                              if (item.top < bounds.top || item.bottom > bounds.bottom) {
                                                  scrollport.scrollBy({ top: item.top < bounds.top ? item.top - bounds.top : item.bottom - bounds.bottom, behavior: 'instant' });
                                              }
                                          }
                                        : undefined
                                }
                            >
                                <span className="font-mono text-accent-text tabular-nums">[{numeral(index + 1)}]</span>
                                <span className="wrap-anywhere underline-offset-4 group-hover:underline group-aria-[current=location]:underline">{entryTitle(entry)}</span>
                            </a>
                        );
                    })
                ) : (
                    <a className="group index-link" href="#work">
                        <span className="col-span-full">Selected work</span>
                    </a>
                )}
            </div>
        </nav>
    );
}
function StickyBar({ portfolio, navigation, children, ref }: { portfolio: typeof Portfolio.Type; navigation: Navigation; children: ReactNode; ref: Ref<HTMLDivElement> }): ReactNode {
    return (
        <div className="sticky top-0 z-30" ref={ref}>
            {children}
            {portfolio.entries.length > 0 && (
                <label className="hidden items-center justify-between gap-4 border-line border-b bg-background px-(--page-gutter) py-2 max-md:flex">
                    <span className="eyebrow">Index</span>
                    <select
                        className="control min-w-0 flex-1 truncate border-0 shadow-none"
                        onChange={(event): void => {
                            const entry = portfolio.entries.find((item) => item.id === event.target.value);
                            if (entry) {
                                navigation.jump(entry);
                            }
                        }}
                        value={navigation.active ?? ''}
                    >
                        <option disabled={true} value="">
                            Select work
                        </option>
                        {portfolio.entries.map((entry, index) => (
                            <option key={entry.id} value={entry.id}>
                                [{numeral(index + 1)}] {entryTitle(entry)}
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
