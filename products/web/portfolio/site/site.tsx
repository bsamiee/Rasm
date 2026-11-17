import { useAtomValue } from '@effect/atom-react';
import { Identity } from '@rasm/hosting/identity';
import { ModalDialog } from '@rasm/ui';
import { flow, Option, Schema } from 'effect';
import { UrlParams } from 'effect/http';
import { Atom } from 'effect/reactivity';
import { ArrowDown, ArrowUp, Plus } from 'lucide-react';
import { MotionConfig } from 'motion/react';
import { lazy, Suspense, useEffect, useRef, useState } from 'react';
import { Button, DialogTrigger, Heading } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import { MediaFigure } from '#/media/media-figure.tsx';
import type { Bootstrap } from '#/model/api';
import type { PortfolioData } from '#/model/portfolio';
import { Chapter } from '#/site/chapter.tsx';
import { UrlFragment, useNavigation } from '#/site/navigation';
import { StickyBar, WorkIndex } from '#/site/work-index.tsx';

// --- [CONSTANTS] -----------------------------------------------------------------------

const edit = { name: 'edit', value: 'true' } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const editing = Atom.searchParam(edit.name, { schema: Schema.Literal(edit.value) }).pipe(Atom.withServerValue(() => Option.none()));
const Editor = lazy(() => import('#/editor/editor.tsx').then((module) => ({ default: module.Editor })));
const Site = ({ published: served, session }: typeof Bootstrap.Type) => {
    const [published, setPublished] = useState(served);
    const [preview, setPreview] = useState(Option.none<typeof PortfolioData.Type>());
    const requested = Option.isSome(useAtomValue(editing)) && session === 'allowed';
    const [toggled, setToggled] = useState(Option.none<boolean>());
    const open = Option.getOrElse(toggled, () => requested);
    const modalOpen = useAtomValue(ModalDialog.open, (count) => count > 0);
    const opener = useRef<HTMLButtonElement>(null);
    const work = useRef<HTMLElement>(null);
    const sticky = useRef<HTMLDivElement>(null);
    const { portfolio } = Option.getOrElse(preview, () => published);
    const navigation = useNavigation(portfolio, work, sticky, !(open || modalOpen));
    const populated = portfolio.entries.length > 0;
    useEffect(() => {
        if (requested) {
            opener.current?.focus({ preventScroll: true });
        }
    }, [requested]);
    return (
        <MotionConfig reducedMotion="user">
            <a className="fixed -top-25 left-5 z-100 bg-background p-3.75 focus:top-2.5" href={UrlFragment.work}>
                Skip to work
            </a>
            <header className="site-header">
                <a className="site-name" href={UrlFragment.top}>
                    {portfolio.name || 'Portfolio'}
                    <Plus className="size-9 shrink-0 stroke-2 text-accent" nonScalingStroke />
                </a>
                {session === 'allowed' ? (
                    <DialogTrigger isOpen={open} onOpenChange={flow(Option.some, setToggled)}>
                        <Button className="quiet-link" ref={opener}>
                            Edit portfolio
                        </Button>
                        {(Option.isSome(toggled) || requested) && (
                            <ErrorBoundary
                                fallback={
                                    <ModalDialog className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/55 p-5" dialog={{ className: 'flex flex-col gap-5' }} isDismissable modal={{ className: 'w-full max-w-lg bg-background p-9' }}>
                                        <Heading className="text-2xl tracking-tight" slot="title">
                                            Editor failed to open
                                        </Heading>
                                        <p className="hint">Published portfolio is unaffected</p>
                                        <Button className="button self-start" slot="close">
                                            Close
                                        </Button>
                                    </ModalDialog>
                                }
                            >
                                <Suspense fallback={<output className="block p-9">Opening editor…</output>}>
                                    <Editor
                                        onPreview={flow(Option.some, setPreview)}
                                        onPublish={(data) => {
                                            setPublished(data);
                                            setPreview(Option.none());
                                        }}
                                        published={published.portfolio}
                                    />
                                </Suspense>
                            </ErrorBoundary>
                        )}
                    </DialogTrigger>
                ) : (
                    <a className="quiet-link" href={session === 'signedIn' ? Identity.href('signout', '/') : Identity.href('signin', `/?${UrlParams.toString({ [edit.name]: edit.value })}`)} target="_top">
                        {session === 'signedIn' ? 'Sign out' : 'Owner sign-in'}
                    </a>
                )}
            </header>
            <WorkIndex navigation={navigation} portfolio={portfolio} />
            <StickyBar navigation={navigation} portfolio={portfolio} ref={sticky}>
                {Option.isSome(preview) && (
                    <div className="flex items-center gap-3.75 bg-foreground px-6 py-2 text-caption text-background max-sm:flex-wrap max-sm:gap-1 max-sm:px-4">
                        <span className="max-sm:w-full">Private preview · snapshot</span>
                        <Button
                            className="ml-auto button button-outline max-sm:ml-0"
                            onPress={() => {
                                opener.current?.focus({ preventScroll: true });
                                setToggled(Option.some(true));
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
                                    <a className="plain-link gap-1.5 text-sm" href={UrlFragment.work}>
                                        View work
                                        <ArrowDown className="size-3" />
                                    </a>
                                )}
                            </div>
                        </div>
                        {portfolio.hero === undefined ? (
                            <div className="hero-placeholder">
                                <span className="absolute top-5 left-5.5 eyebrow text-muted max-sm:top-4 max-sm:left-4">Hero placement</span>
                                <Plus className="size-11 stroke-1 text-cross" nonScalingStroke />
                            </div>
                        ) : (
                            <div className="flex min-w-0 justify-center media-h-hero">
                                <MediaFigure active placement={portfolio.hero} presentation="expandable" priority renderMedia />
                            </div>
                        )}
                    </section>
                    <section className="site-work" data-populated={populated} id="work" ref={work} tabIndex={-1}>
                        <div className="border-t border-foreground pt-4.75">
                            <h2 className="work-title">Projects and studies</h2>
                        </div>
                        {portfolio.entries.map((entry) => (
                            <Chapter entry={entry} key={entry.id} navigation={navigation} />
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
                    <a className="ml-auto plain-link gap-1" href={UrlFragment.top}>
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
