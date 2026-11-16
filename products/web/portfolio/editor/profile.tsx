import { useAtomSet } from '@effect/atom-react';
import { Identity } from '@rasm/hosting/identity';
import { Field } from '@rasm/ui';
import { Optic } from 'effect';
import type { Atom } from 'effect/reactivity';
import { PlacementFields } from '#/editor/placement-fields.tsx';
import { draft, hero, portfolio } from '#/editor/state';
import { MediaFigure } from '#/media/media-figure.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Profile = ({ idPrefix, onChooseHero, ...state }: Pick<Atom.Success<typeof draft>, 'assets' | 'portfolio'> & { readonly idPrefix: string; readonly onChooseHero: () => void }) => {
    const setDraft = useAtomSet(draft);
    const field = (name: 'name' | 'introduction' | 'email') => ({ id: `${idPrefix}${name}`, name, value: state.portfolio[name], onChange: (value: string) => setDraft(Optic.replace(portfolio.key(name), value)) });
    return (
        <>
            <Field {...field('name')} label="Display name" placeholder="Name or studio" />
            <Field {...field('introduction')} label="Introduction" multiline placeholder="Optional" />
            <Field {...field('email')} label="Public contact email" placeholder="Optional" type="email" />
            <h3 className="mt-2 text-heading tracking-display outline-none" id={`${idPrefix}hero`} tabIndex={-1}>
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
                    <div className="flex justify-center media-h-80">
                        <MediaFigure active placement={state.portfolio.hero} presentation="full" priority={false} renderMedia />
                    </div>
                    <PlacementFields assets={state.assets} onChange={(placement) => setDraft(Optic.replace(hero, placement))} value={state.portfolio.hero}>
                        <button className="button self-start button-outline" onClick={() => setDraft(Optic.replace(hero, undefined))} type="button">
                            Clear hero
                        </button>
                    </PlacementFields>
                </>
            )}
            <a className="mt-8.75 text-sm underline" href={Identity.sites.href('signout', '/')} target="_top">
                Sign out
            </a>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Profile };
