import { useAtom, useAtomMount, useAtomSet, useAtomValue } from '@effect/atom-react';
import { Identity } from '@rasm/hosting/identity';
import { ModalDialog } from '@rasm/ui';
import { Match, Option } from 'effect';
import { UrlParams } from 'effect/http';
import { ArrowDown, ArrowUp, LucideProvider, Plus } from 'lucide-react';
import { MotionConfig } from 'motion/react';
import { lazy, Suspense, useEffect, useRef } from 'react';
import { Button, DialogTrigger, Heading } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import { joinDetails } from '#/media/labels';
import { MediaFigure } from '#/media/media-figure.tsx';
import { Chapter } from '#/site/chapter.tsx';
import { scrollTracking, topHref, workAnchor, workSection } from '#/site/navigation';
import { bootstrap, edit, editor, preview, requested, shown } from '#/site/state';
import { StickyBar, WorkIndex } from '#/site/work-index.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Editor = lazy(() => import('#/editor/editor.tsx').then((module) => ({ default: module.Editor })));
const Site = () => {
    const { session } = useAtomValue(bootstrap);
    const portfolio = useAtomValue(shown);
    const asked = useAtomValue(requested);
    const [view, setView] = useAtom(editor);
    const [previewed, setPreview] = useAtom(preview);
    const work = useAtomSet(workSection);
    const opener = useRef<HTMLButtonElement>(null);
    const populated = portfolio.entries.length > 0;
    useAtomMount(scrollTracking);
    useEffect(() => {
        if (asked) {
            opener.current?.focus({ preventScroll: true });
        }
    }, [asked]);
    return (
        <MotionConfig reducedMotion="user">
            <LucideProvider strokeWidth={1.5}>
                <title>{joinDetails([portfolio.name ?? 'Portfolio', 'Architecture'])}</title>
                <meta content={portfolio.introduction ?? 'Selected architectural projects and studies'} name="description" />
                <a className="skip-link" href={workAnchor.href}>
                    Skip to work
                </a>
                <header className="site-header">
                    <a className="site-name" href={topHref}>
                        {portfolio.name ?? 'Portfolio'}
                        <Plus nonScalingStroke size={36} strokeWidth={2} />
                    </a>
                    {Match.value(session).pipe(
                        Match.when('allowed', () => (
                            <DialogTrigger isOpen={view === 'open'} onOpenChange={(open) => setView(open ? 'open' : 'closed')}>
                                <Button className="quiet-link" ref={opener}>
                                    Edit portfolio
                                </Button>
                                {view !== 'idle' && (
                                    <ErrorBoundary
                                        fallback={
                                            <ModalDialog isDismissable modal={{ className: 'notice' }}>
                                                <Heading slot="title">Editor failed to open</Heading>
                                                <p className="hint">Published portfolio is unaffected</p>
                                                <Button className="button" slot="close">
                                                    Close
                                                </Button>
                                            </ModalDialog>
                                        }
                                    >
                                        <Suspense fallback={<output className="notice">Opening editor…</output>}>
                                            <Editor />
                                        </Suspense>
                                    </ErrorBoundary>
                                )}
                            </DialogTrigger>
                        )),
                        Match.when('signedIn', () => (
                            <a className="quiet-link" href={Identity.href('signout', '/')} target="_top">
                                Sign out
                            </a>
                        )),
                        Match.when('signedOut', () => (
                            <a className="quiet-link" href={Identity.href('signin', `/?${UrlParams.toString({ [edit.name]: edit.value })}`)} target="_top">
                                Owner sign-in
                            </a>
                        )),
                        Match.exhaustive,
                    )}
                </header>
                <WorkIndex />
                <StickyBar>
                    {Option.isSome(previewed) && (
                        <div className="preview-bar">
                            <span>Private preview · snapshot</span>
                            <Button
                                className="button button-outline"
                                onPress={() => {
                                    opener.current?.focus({ preventScroll: true });
                                    setView('open');
                                }}
                            >
                                Continue editing
                            </Button>
                            <Button className="button button-ghost" onPress={() => setPreview(Option.none())}>
                                Return to published
                            </Button>
                        </div>
                    )}
                </StickyBar>
                <div className="site-content">
                    <main>
                        <section className="site-introduction">
                            <div className="introduction-text">
                                <h1 className="site-title">
                                    Selected
                                    <br />
                                    <span>
                                        work<span className="accent">.</span>
                                    </span>
                                </h1>
                                <div>
                                    <p className="lead">{portfolio.introduction}</p>
                                    {populated && (
                                        <a className="plain-link" href={workAnchor.href}>
                                            View work
                                            <ArrowDown size={12} />
                                        </a>
                                    )}
                                </div>
                            </div>
                            {portfolio.hero === undefined ? (
                                <div className="hero-placeholder">
                                    <span className="eyebrow">Hero placement</span>
                                    <Plus nonScalingStroke size={44} strokeWidth={1} />
                                </div>
                            ) : (
                                <div className="hero-media">
                                    <MediaFigure active placement={portfolio.hero} presentation="expandable" priority renderMedia />
                                </div>
                            )}
                        </section>
                        <section className="site-work" data-populated={populated} id={workAnchor.id} ref={work} tabIndex={-1}>
                            <div>
                                <h2 className="work-title">Projects and studies</h2>
                            </div>
                            {portfolio.entries.map((entry) => (
                                <Chapter entry={entry} key={entry.id} />
                            ))}
                            {!populated && (
                                <div className="work-empty">
                                    <h3>No work published yet</h3>
                                </div>
                            )}
                        </section>
                    </main>
                    <footer className="site-footer">
                        <span>{portfolio.name}</span>
                        {portfolio.email !== undefined && (
                            <a className="plain-link" href={`mailto:${portfolio.email}`}>
                                Contact
                            </a>
                        )}
                        <a className="plain-link" href={topHref}>
                            Back to top
                            <ArrowUp size={20} />
                        </a>
                    </footer>
                </div>
            </LucideProvider>
        </MotionConfig>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Site };
