// biome-ignore-all lint/correctness/useUniqueElementIds: Public fragment links target the top and work sections
import { ArrowDown, ArrowUp, Plus } from 'lucide-react';
import { MotionConfig } from 'motion/react';
import { lazy, type ReactNode, Suspense, useEffect, useEffectEvent, useRef, useState } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Modal, ModalOverlay } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import { MediaFigure } from '../media/figure.tsx';
import type { PortfolioData, Session } from '../model/document.ts';
import { Chapter } from './chapter.tsx';
import { ProjectIndex, StickyBar } from './contents.tsx';
import { numeral, useNavigation } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Editor = lazy(() => import('../editor/editor.tsx').then((module) => ({ default: module.Editor })));
function Site({ initial, session }: { initial: typeof PortfolioData.Type; session: typeof Session.Type }): ReactNode {
    const [published, setPublished] = useState(initial);
    const [preview, setPreview] = useState<typeof PortfolioData.Type>();
    const [editor, setEditor] = useState<'unopened' | 'open' | 'closed'>('unopened');
    const opener = useRef<HTMLButtonElement>(null);
    const work = useRef<HTMLElement>(null);
    const { portfolio } = preview ?? published;
    const navigation = useNavigation(portfolio, work);
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
            <main id="top">
                <a className="fixed -top-[100px] left-5 z-[100] bg-background p-[15px] focus:top-2.5" href="#work">
                    Skip to projects
                </a>
                <header className="mx-[38px] flex min-h-[92px] items-center justify-between gap-7 border-line border-b py-[18px] max-md:mx-[22px] max-md:min-h-[76px] max-md:gap-5">
                    <a className="wrap-anywhere flex min-w-0 max-w-[45%] items-center gap-5 text-[27px] leading-[1.1] tracking-[-0.04em] max-md:max-w-[65%]" href="#top">
                        {portfolio.name || 'Portfolio'}
                        <Plus className="size-9 shrink-0 text-accent" nonScalingStroke={true} strokeLinecap="butt" strokeWidth={2} />
                    </a>
                    <span className="eyebrow max-md:hidden">Architecture &amp; spatial practice</span>
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
                <StickyBar navigation={navigation} portfolio={portfolio}>
                    {preview && (
                        <div className="flex items-center gap-[15px] bg-foreground px-6 py-2 text-[13px] text-background max-sm:flex-wrap max-sm:gap-1 max-sm:px-4 [&>span]:max-sm:w-full">
                            <span>Private draft preview</span>
                            <button className="button button-outline ml-auto max-sm:ml-0" onClick={edit} type="button">
                                Continue editing
                            </button>
                            <button className="button button-ghost" onClick={(): void => setPreview(undefined)} type="button">
                                Return to published
                            </button>
                        </div>
                    )}
                </StickyBar>
                <div className="mr-[38px] ml-[270px] max-w-[1800px] max-md:mx-[22px] xl:mr-[6%] xl:ml-[18%]">
                    <section className="@container pt-12 max-md:pt-[30px]">
                        <div className="eyebrow flex justify-between gap-6 text-muted max-sm:gap-5 [&>span:last-child]:text-right max-sm:[&>span:last-child]:max-w-[100px]">
                            <span>Architectural portfolio</span>
                            <span>Projects &amp; studies</span>
                        </div>
                        <h1 className="mt-[54px] mb-[45px] font-medium text-[clamp(64px,15.8cqw,184px)] leading-[0.91] tracking-[-0.055em] max-md:mt-10 max-md:mb-8 max-md:text-[clamp(64px,20cqw,160px)]">
                            Selected
                            <br />
                            <span className="ml-[21%] block max-sm:ml-[18%]">
                                work<span className="text-accent">.</span>
                            </span>
                        </h1>
                        {portfolio.hero ? (
                            <div className="flex justify-center [--media-height:75svh]">
                                <MediaFigure active={true} placement={portfolio.hero} presentation="expandable" priority={true} />
                            </div>
                        ) : (
                            <div className="relative flex h-[clamp(280px,36vw,580px)] items-center justify-center border border-line bg-surface before:absolute before:-top-px before:-left-px before:size-[22px] before:border-accent before:border-t before:border-l before:content-[''] after:absolute after:-right-px after:-bottom-px after:size-[22px] after:border-accent after:border-r after:border-b after:content-[''] max-md:h-[clamp(240px,52vw,420px)]">
                                <span className="eyebrow absolute top-5 left-[22px] text-muted max-sm:top-4 max-sm:left-4">Hero placement</span>
                                <Plus className="size-11 text-cross" nonScalingStroke={true} strokeLinecap="butt" strokeWidth={1} />
                            </div>
                        )}
                        {(portfolio.introduction || populated) && (
                            <div className="flex items-start justify-between gap-10 pt-5 max-sm:flex-col max-sm:gap-3">
                                {Boolean(portfolio.introduction) && <p className="lead">{portfolio.introduction}</p>}
                                {populated && (
                                    <a className="eyebrow plain-link gap-1.5" href="#work">
                                        View projects
                                        <ArrowDown className="size-3" strokeLinecap="butt" strokeLinejoin="miter" />
                                    </a>
                                )}
                            </div>
                        )}
                    </section>
                    <section className="pt-[82px] pb-[72px] max-md:py-[60px]" id="work" ref={work} tabIndex={-1}>
                        <div className="flex items-baseline justify-between gap-5 border-foreground border-t pt-[19px] max-sm:gap-3.5">
                            <h2 className={populated ? 'eyebrow' : 'text-[clamp(30px,4.5vw,64px)] leading-[1.05] tracking-[-0.045em]'}>Projects &amp; studies</h2>
                            <span className="eyebrow shrink-0 text-muted">{numeral(portfolio.entries.length)}</span>
                        </div>
                        {populated ? (
                            portfolio.entries.map((entry, index) => <Chapter entry={entry} index={index} key={entry.id} navigation={navigation} />)
                        ) : (
                            <div className="flex items-center gap-[42px] pt-[62px] pb-3 max-sm:gap-6 max-sm:pt-[42px] max-sm:pb-0">
                                <span aria-hidden="true" className="font-light text-[82px] text-accent leading-none tracking-[-0.06em] max-sm:text-[62px]">
                                    00
                                </span>
                                <div>
                                    <h3 className="text-[23px] tracking-[-0.025em] max-sm:text-lg">No projects published yet.</h3>
                                    <p className="hint">Work will appear here as it is published.</p>
                                </div>
                            </div>
                        )}
                    </section>
                    <footer className="flex items-center justify-between gap-6 border-line border-t pt-6 pb-8 text-sm max-sm:flex-wrap max-sm:gap-x-6 max-sm:gap-y-2">
                        <span className="wrap-anywhere min-w-0 max-sm:w-full">{portfolio.name || 'Architecture portfolio'}</span>
                        {Boolean(portfolio.email) && (
                            <a className="plain-link" href={`mailto:${portfolio.email}`}>
                                Contact
                            </a>
                        )}
                        <a className="plain-link gap-1" href="#top">
                            Back to top
                            <ArrowUp className="size-5" strokeLinecap="butt" strokeLinejoin="miter" />
                        </a>
                    </footer>
                </div>
            </main>
        </MotionConfig>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Site };
