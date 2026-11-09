import { useAtomSet } from '@effect/atom-react';
import { Sites } from '@rasm/hosting/sites';
import { Field } from '@rasm/ui';
import { Optic } from 'effect';
import type { Atom } from 'effect/reactivity';
import { PlacementFields } from '#/editor/placement-fields.tsx';
import { draft, hero, portfolio } from '#/editor/state';
import { MediaFigure } from '#/media/media-figure.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Identity = ({ idPrefix, onChooseHero, ...state }: Pick<Atom.Success<typeof draft>, 'assets' | 'portfolio'> & { readonly idPrefix: string; readonly onChooseHero: () => void }) => {
    const setDraft = useAtomSet(draft);
    const field = (name: 'name' | 'introduction' | 'email') => ({ id: `${idPrefix}${name}`, name, value: state.portfolio[name], onChange: (value: string) => setDraft(Optic.replace(portfolio.key(name), value)) });
    return (
        <>
            <Field {...field('name')} label="Display name" placeholder="Name or studio" />
            <Field {...field('introduction')} label="Introduction" multiline={true} placeholder="Optional" />
            <Field {...field('email')} label="Public contact email" placeholder="Optional" type="email" />
            <h3 className="text-heading tracking-display mt-2 outline-none" id={`${idPrefix}hero`} tabIndex={-1}>
                Hero
            </h3>
            {state.portfolio.hero === undefined ? (
                <div className="actions">
                    <p className="hint">Choose library file or upload new one</p>
                    <button className="button button-outline" onClick={onChooseHero} type="button">
                        Choose hero file
                    </button>
                </div>
            ) : (
                <>
                    <div className="media-h-80 flex justify-center">
                        <MediaFigure active={true} placement={state.portfolio.hero} presentation="full" priority={false} renderMedia={true} />
                    </div>
                    <PlacementFields assets={state.assets} onChange={(placement) => setDraft(Optic.replace(hero, placement))} value={state.portfolio.hero}>
                        <button className="button button-outline self-start" onClick={() => setDraft(Optic.replace(hero, undefined))} type="button">
                            Clear hero
                        </button>
                    </PlacementFields>
                </>
            )}
            <a className="mt-8.75 text-sm underline" href={Sites.identity.href('signout', '/')} target="_top">
                Sign out
            </a>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Identity };
