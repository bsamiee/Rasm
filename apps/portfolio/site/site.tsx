import { useAtomValue } from '@effect/atom-react';
import { ArrowDown, ArrowUp, Plus } from 'lucide-react';
import { MotionConfig } from 'motion/react';
import { lazy, type ReactNode, Suspense, useEffect, useEffectEvent, useRef, useState } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Modal, ModalOverlay } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import { MediaFigure } from '../media/figure.tsx';
import type { PortfolioData, Session } from '../model/document.ts';
import { Chapter } from './chapter.tsx';
import { ProjectIndex, StickyBar } from './contents.tsx';
import { openDialogs, useNavigation } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Editor = lazy(() => import('../editor/editor.tsx').then((module) => ({ default: module.Editor })));
function Site({ initial, session }: { initial: typeof PortfolioData.Type; session: typeof Session.Type }): ReactNode {
    const [published, setPublished] = useState(initial);
    const [preview, setPreview] = useState<typeof PortfolioData.Type>();
    const [editor, setEditor] = useState<'unopened' | 'open' | 'closed'>('unopened');
    const modalOpen = useAtomValue(openDialogs, (dialogs) => dialogs.size > 0);
    const opener = useRef<HTMLButtonElement>(null);
    const work = useRef<HTMLElement>(null);
    const sticky = useRef<HTMLDivElement>(null);
    const { portfolio } = preview ?? published;
    const navigation = useNavigation(portfolio, work, sticky, editor !== 'open' && !modalOpen);
    const populated = portfolio.entries.length > 0;
    const edit = (): void => {
        opener.current?.focus({ preventScroll: true });
        setEditor('open');
    };
    const resume = useEffectEvent(() => {
        if (session.kind === 'owner' && new URLSearchParams(location.search).has('edit')) {
            edit();
        }
    });
    useEffect(() => resume(), []);
    return (
        <MotionConfig reducedMotion="user">
            <a className="fixed -top-[100px] left-5 z-[100] bg-background p-[15px] focus:top-2.5" href="#work">
                Skip to work
            </a>
            <header className="mx-(--page-gutter) flex min-h-[92px] items-center justify-between gap-7 border-line border-b py-[18px] max-md:min-h-[76px] max-md:gap-5">
                <a className="wrap-anywhere flex min-w-0 max-w-[45%] items-center gap-5 text-[27px] leading-[1.1] tracking-[-0.04em] max-md:max-w-[65%]" href="#top">
                    {portfolio.name || 'Portfolio'}
                    <Plus className="size-9 shrink-0 stroke-2 text-accent" nonScalingStroke={true} />
                </a>
                {session.kind === 'owner' ? (
                    <DialogTrigger isOpen={editor === 'open'} onOpenChange={(open): void => setEditor(open ? 'open' : 'closed')}>
                        <Button className="quiet-link" ref={opener}>
                            Edit portfolio
                        </Button>
                        {editor !== 'unopened' && (
                            <ErrorBoundary
                                fallback={
                                    <ModalOverlay className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/55 p-5" isDismissable={true}>
                                        <Modal className="w-full max-w-lg bg-background p-9">
                                            <Dialog className="flex flex-col gap-5">
                                                <Heading className="text-2xl tracking-[-0.025em]" slot="title">
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
                                    <Editor
                                        onPreview={setPreview}
                                        onPublish={(value): void => {
                                            setPublished(value);
                                            setPreview(undefined);
                                        }}
                                        published={published.portfolio}
                                    />
                                </Suspense>
                            </ErrorBoundary>
                        )}
                    </DialogTrigger>
                ) : (
                    <a className="quiet-link" href={session.signedIn ? '/signout-with-chatgpt?return_to=%2F' : '/signin-with-chatgpt?return_to=%2F%3Fedit%3D1'} target="_top">
                        {session.signedIn ? 'Sign out' : 'Owner sign-in'}
                    </a>
                )}
            </header>
            <ProjectIndex navigation={navigation} portfolio={portfolio} />
            <StickyBar navigation={navigation} portfolio={portfolio} ref={sticky}>
                {preview && (
                    <div className="flex items-center gap-[15px] bg-foreground px-6 py-2 text-[13px] text-background max-sm:flex-wrap max-sm:gap-1 max-sm:px-4 [&>span]:max-sm:w-full">
                        <span>Private preview · snapshot</span>
                        <button className="button button-outline ml-auto max-sm:ml-0" onClick={edit} type="button">
                            Continue editing
                        </button>
                        <button className="button button-ghost" onClick={(): void => setPreview(undefined)} type="button">
                            Return to published
                        </button>
                    </div>
                )}
            </StickyBar>
            <div className="@container mr-(--page-gutter) ml-[calc(var(--page-gutter)+var(--index-width)+var(--index-gap))] max-w-[1800px] max-md:mx-(--page-gutter) xl:mr-[6%]">
                <main>
                    <section className="@container grid @min-[60rem]:grid-cols-2 grid-cols-1 items-end gap-8 pt-12 max-md:pt-[30px]">
                        <div className="min-w-0">
                            <h1 className="wrap-anywhere mb-8 font-medium @min-[60rem]:text-[clamp(4rem,8cqw,7.5rem)] text-[clamp(4rem,calc(2rem+12cqw),11.5rem)] leading-[0.91] tracking-[-0.055em]">
                                Selected
                                <br />
                                <span>
                                    work<span className="text-accent">.</span>
                                </span>
                            </h1>
                            {(portfolio.introduction || populated) && (
                                <div className="flex flex-col items-start gap-4">
                                    {Boolean(portfolio.introduction) && <p className="lead">{portfolio.introduction}</p>}
                                    {populated && (
                                        <a className="plain-link gap-1.5 text-sm" href="#work">
                                            View work
                                            <ArrowDown className="size-3" />
                                        </a>
                                    )}
                                </div>
                            )}
                        </div>
                        {portfolio.hero ? (
                            <div className="flex min-w-0 justify-center [--media-height:75svh]">
                                <MediaFigure active={true} placement={portfolio.hero} presentation="expandable" priority={true} renderMedia={true} />
                            </div>
                        ) : (
                            <div className="relative flex h-[clamp(280px,36vw,580px)] items-center justify-center border border-line bg-surface before:absolute before:-top-px before:-left-px before:size-[22px] before:border-accent before:border-t before:border-l before:content-[''] after:absolute after:-right-px after:-bottom-px after:size-[22px] after:border-accent after:border-r after:border-b after:content-[''] max-md:h-[clamp(240px,52vw,420px)]">
                                <span className="eyebrow absolute top-5 left-[22px] text-muted max-sm:top-4 max-sm:left-4">Hero placement</span>
                                <Plus className="size-11 stroke-1 text-cross" nonScalingStroke={true} />
                            </div>
                        )}
                    </section>
                    {/* biome-ignore lint/correctness/useUniqueElementIds: Public #work fragment links and history entries target this section */}
                    <section className="pt-[82px] pb-[72px] outline-none max-md:py-[60px]" id="work" ref={work} tabIndex={-1}>
                        <div className="border-foreground border-t pt-[19px]">
                            <h2 className={populated ? 'font-medium text-sm' : 'text-[clamp(1.875rem,calc(1rem+3cqw),4rem)] leading-[1.05] tracking-[-0.045em]'}>Projects and studies</h2>
                        </div>
                        {populated ? (
                            portfolio.entries.map((entry, index) => <Chapter entry={entry} index={index} key={entry.id} navigation={navigation} />)
                        ) : (
                            <div className="pt-[62px] pb-3 max-sm:pt-[42px] max-sm:pb-0">
                                <h3 className="text-[23px] tracking-[-0.025em] max-sm:text-lg">No work published yet</h3>
                            </div>
                        )}
                    </section>
                </main>
                <footer className="flex items-center justify-between gap-6 border-line border-t pt-6 pb-8 text-sm max-sm:flex-wrap max-sm:gap-x-6 max-sm:gap-y-2">
                    {Boolean(portfolio.name) && <span className="wrap-anywhere min-w-0 max-sm:w-full">{portfolio.name}</span>}
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
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Site };
