import { useAtomValue } from '@effect/atom-react';
import { sessionPath } from '@rasm/hosting/sites.ts';
import { openDialogs } from '@rasm/ui/dialogs.ts';
import { ArrowDown, ArrowUp, Plus } from 'lucide-react';
import { MotionConfig } from 'motion/react';
import { lazy, type ReactNode, type RefObject, Suspense, useEffect, useRef, useState, useSyncExternalStore } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Modal, ModalOverlay } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import { MediaFigure } from '../media/figure.tsx';
import type { PortfolioData, Session } from '../model/document.ts';
import { Chapter } from './chapter.tsx';
import { ProjectIndex, StickyBar } from './contents.tsx';
import { useNavigation } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Editor = lazy(() => import('../editor/editor.tsx').then((module) => ({ default: module.Editor })));
const Account = ({ session, open, mounted, opener, onOpenChange, children }: { session: typeof Session.Type; open: boolean; mounted: boolean; opener: RefObject<HTMLButtonElement | null>; onOpenChange: (open: boolean) => void; children: ReactNode }) =>
    session.kind === 'owner' ? (
        <DialogTrigger isOpen={open} onOpenChange={onOpenChange}>
            <Button className="quiet-link" ref={opener}>
                Edit portfolio
            </Button>
            {mounted && (
                <ErrorBoundary
                    fallback={
                        <ModalOverlay className="bg-foreground/55 fixed inset-0 z-50 flex items-center justify-center p-5" isDismissable={true}>
                            <Modal className="bg-background w-full max-w-lg p-9">
                                <Dialog className="flex flex-col gap-5">
                                    <Heading className="text-2xl tracking-tight" slot="title">
                                        The editor could not open
                                    </Heading>
                                    <p className="hint">Your published portfolio is still available. Close this message to return to it.</p>
                                    <Button className="button self-start" slot="close">
                                        Close
                                    </Button>
                                </Dialog>
                            </Modal>
                        </ModalOverlay>
                    }
                >
                    <Suspense
                        fallback={
                            <div className="p-9" role="status">
                                Opening editor…
                            </div>
                        }
                    >
                        {children}
                    </Suspense>
                </ErrorBoundary>
            )}
        </DialogTrigger>
    ) : (
        <a className="quiet-link" href={session.signedIn ? sessionPath('signout', '/') : sessionPath('signin', '/?edit=1')} target="_top">
            {session.signedIn ? 'Sign out' : 'Owner sign-in'}
        </a>
    );
const Site = ({ initial, session }: { initial: typeof PortfolioData.Type; session: typeof Session.Type }) => {
    const [published, setPublished] = useState(initial);
    const [preview, setPreview] = useState<typeof PortfolioData.Type>();
    const requested = useSyncExternalStore(
        () => () => undefined,
        () => session.kind === 'owner' && new URLSearchParams(location.search).has('edit'),
        () => false,
    );
    const [editor, setEditor] = useState<'unopened' | 'open' | 'closed'>('unopened');
    const open = editor === 'open' || (editor === 'unopened' && requested);
    const modalOpen = useAtomValue(openDialogs, (dialogs) => dialogs.size > 0);
    const opener = useRef<HTMLButtonElement>(null);
    const work = useRef<HTMLElement>(null);
    const sticky = useRef<HTMLDivElement>(null);
    const { portfolio } = preview ?? published;
    const navigation = useNavigation(portfolio, work, sticky, !(open || modalOpen));
    const populated = portfolio.entries.length > 0;
    useEffect(() => {
        if (requested) {
            opener.current?.focus({ preventScroll: true });
        }
    }, [requested]);
    return (
        <MotionConfig reducedMotion="user">
            <a className="bg-background fixed -top-25 left-5 z-100 p-3.75 focus:top-2.5" href="#work">
                Skip to work
            </a>
            <header className="site-header">
                <a className="site-name" href="#top">
                    {portfolio.name || 'Portfolio'}
                    <Plus className="text-accent size-9 shrink-0 stroke-2" nonScalingStroke={true} />
                </a>
                <Account mounted={editor !== 'unopened' || open} onOpenChange={(next) => setEditor(next ? 'open' : 'closed')} open={open} opener={opener} session={session}>
                    <Editor
                        onPreview={setPreview}
                        onPublish={(value) => {
                            setPublished(value);
                            setPreview(undefined);
                        }}
                        published={published.portfolio}
                    />
                </Account>
            </header>
            <ProjectIndex navigation={navigation} portfolio={portfolio} />
            <StickyBar navigation={navigation} portfolio={portfolio} ref={sticky}>
                {preview && (
                    <div className="bg-foreground text-background text-caption flex items-center gap-3.75 px-6 py-2 max-sm:flex-wrap max-sm:gap-1 max-sm:px-4">
                        <span className="max-sm:w-full">Private preview · snapshot</span>
                        <button
                            className="button button-outline ml-auto max-sm:ml-0"
                            onClick={() => {
                                opener.current?.focus({ preventScroll: true });
                                setEditor('open');
                            }}
                            type="button"
                        >
                            Continue editing
                        </button>
                        <button className="button button-ghost" onClick={() => setPreview(undefined)} type="button">
                            Return to published
                        </button>
                    </div>
                )}
            </StickyBar>
            <div className="site-content">
                <main>
                    <section className="site-introduction">
                        <div className="min-w-0">
                            <h1 className="site-title">
                                Selected
                                <br />
                                <span>
                                    work<span className="text-accent">.</span>
                                </span>
                            </h1>
                            <div className="flex flex-col items-start gap-4">
                                <p className="lead empty:hidden">{portfolio.introduction}</p>
                                {populated && (
                                    <a className="plain-link gap-1.5 text-sm" href="#work">
                                        View work
                                        <ArrowDown className="size-3" />
                                    </a>
                                )}
                            </div>
                        </div>
                        {portfolio.hero === undefined ? (
                            <div className="hero-placeholder">
                                <span className="eyebrow text-muted absolute top-5 left-5.5 max-sm:top-4 max-sm:left-4">Hero placement</span>
                                <Plus className="text-cross size-11 stroke-1" nonScalingStroke={true} />
                            </div>
                        ) : (
                            <div className="media-h-hero flex min-w-0 justify-center">
                                <MediaFigure active={true} placement={portfolio.hero} presentation="expandable" priority={true} renderMedia={true} />
                            </div>
                        )}
                    </section>
                    {/* biome-ignore lint/correctness/useUniqueElementIds: Public #work fragment links and history entries target this section */}
                    <section className="site-work" data-populated={populated} id="work" ref={work} tabIndex={-1}>
                        <div className="border-foreground border-t pt-4.75">
                            <h2 className="work-title">Projects and studies</h2>
                        </div>
                        {portfolio.entries.map((entry, index) => (
                            <Chapter entry={entry} index={index} key={entry.id} navigation={navigation} />
                        ))}
                        {!populated && (
                            <div className="pt-15.5 pb-3 max-sm:pt-10.5 max-sm:pb-0">
                                <h3 className="text-2xl tracking-tight max-sm:text-lg">No work published yet</h3>
                            </div>
                        )}
                    </section>
                </main>
                <footer className="site-footer">
                    <span className="min-w-0 wrap-anywhere empty:hidden max-sm:w-full">{portfolio.name}</span>
                    {Boolean(portfolio.email) && (
                        <a className="plain-link" href={`mailto:${portfolio.email}`}>
                            Contact
                        </a>
                    )}
                    <a className="plain-link ml-auto gap-1" href="#top">
                        Back to top
                        <ArrowUp className="size-5" />
                    </a>
                </footer>
            </div>
        </MotionConfig>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Site };
