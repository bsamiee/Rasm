import type { ReactNode, Ref } from 'react';
import { entryTitle, numeral } from '../media/display.ts';
import type { Portfolio } from '../model/document.ts';
import { fragment, type useNavigation } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ProjectIndex = ({ portfolio, navigation }: { portfolio: typeof Portfolio.Type; navigation: ReturnType<typeof useNavigation> }) => (
    <nav aria-label="Work index" className="project-index">
        <a className="eyebrow min-h-11 self-start pb-5.75 text-muted hover:text-accent-text" href="#work">
            Index
        </a>
        <div className="index-list">
            {portfolio.entries.length > 0 ? (
                portfolio.entries.map((entry, index) => (
                    <a
                        aria-current={navigation.active === entry.id ? 'location' : undefined}
                        className="group index-link"
                        href={fragment(entry.id)}
                        key={entry.id}
                        onClick={(event) => {
                            if (!(event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) && event.button === 0) {
                                event.preventDefault();
                                navigation.jump(entry);
                            }
                        }}
                        ref={navigation.active === entry.id ? (node: HTMLAnchorElement | null) => node?.scrollIntoView({ block: 'nearest', behavior: 'instant' }) : undefined}
                    >
                        <span className="font-mono text-accent-text tabular-nums">[{numeral(index + 1)}]</span>
                        <span className="wrap-anywhere underline-offset-4 group-hover:underline group-aria-current:underline">{entryTitle(entry)}</span>
                    </a>
                ))
            ) : (
                <a className="group index-link" href="#work">
                    <span className="col-span-full">Selected work</span>
                </a>
            )}
        </div>
    </nav>
);
const StickyBar = ({ portfolio, navigation, children, ref }: { portfolio: typeof Portfolio.Type; navigation: ReturnType<typeof useNavigation>; children: ReactNode; ref: Ref<HTMLDivElement> }) => (
    <div className="sticky top-0 z-30" ref={ref}>
        {children}
        {portfolio.entries.length > 0 && (
            <nav aria-label="Work index" className="hidden max-md:block">
                <label className="flex items-center justify-between gap-4 border-line border-b bg-background px-(--page-gutter) py-2">
                    <span className="eyebrow">Index</span>
                    <select
                        className="control min-w-0 flex-1 truncate border-0 shadow-none"
                        onChange={(event) => {
                            const entry = portfolio.entries.find((item) => item.id === event.target.value);
                            if (entry !== undefined) {
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
            </nav>
        )}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ProjectIndex, StickyBar };
