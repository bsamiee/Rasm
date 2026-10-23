import { useAtomSet } from '@effect/atom-react';
import { Field, MenuButton, SelectField, SliderField } from '@rasm/ui';
import { Match, Optic, Record, Schema, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { draft, hero, updateEntry } from '#/editor/state';
import { descriptionLabel, sheetLabel } from '#/media/labels';
import { type AssetCollection, ImageAsset, sheets } from '#/model/asset';
import { type Placement, percentBounds } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const PlacementFields = ({ value, assets, onChange, entryId }: { value: typeof Placement.Type; assets: typeof AssetCollection.Type; onChange: (value: typeof Placement.Type) => void; entryId?: string }) => {
    const setDraft = useAtomSet(draft);
    return (
        <div className="border-line flex flex-col gap-3.25 border-t pt-4.5">
            <span className="text-caption wrap-anywhere">{value.asset.name}</span>
            <Field label="Caption" onChange={(caption) => onChange({ ...value, caption })} value={value.caption} />
            {Match.value(value).pipe(
                Match.discriminatorsExhaustive('kind')({
                    pdf: (sheet) => (
                        <MenuButton label="Sheet" onAction={({ page }) => onChange({ ...sheet, page })} options={sheets(sheet.asset).map((page) => ({ id: String(page.number), label: sheetLabel(page, sheet.asset.pages.length), page }))}>
                            {sheetLabel(sheet.page, sheet.asset.pages.length)}
                        </MenuButton>
                    ),
                    image: (image) => {
                        const cover = image.role === 'photograph' && image.framing.mode === 'cover' ? image.framing : undefined;
                        return (
                            <>
                                <SelectField
                                    label="Image use"
                                    onChange={({ id }) => onChange({ ...image, role: id, framing: { mode: 'contain' } })}
                                    options={Record.toEntries({ drawing: 'Drawing (full sheet)', photograph: 'Photograph' } satisfies Record.ReadonlyRecord<typeof image.role, string>).map(([id, label]) => ({ id, label }))}
                                    value={image.role}
                                />
                                {image.role === 'photograph' && (
                                    <>
                                        <SelectField
                                            label="Framing"
                                            onChange={({ framing }) => onChange({ ...image, framing })}
                                            options={
                                                [
                                                    { id: 'contain', label: 'Fit whole image', framing: { mode: 'contain' } },
                                                    { id: 'cover', label: 'Crop to frame', framing: { mode: 'cover', x: 50, y: 50 } },
                                                ] as const
                                            }
                                            value={image.framing.mode}
                                        />
                                        {cover && (
                                            <div className="field-pair">
                                                <SliderField label="Horizontal focal point" maxValue={percentBounds.maximum} minValue={percentBounds.minimum} onChange={(x) => onChange({ ...image, framing: { ...cover, x } })} value={cover.x} />
                                                <SliderField label="Vertical focal point" maxValue={percentBounds.maximum} minValue={percentBounds.minimum} onChange={(y) => onChange({ ...image, framing: { ...cover, y } })} value={cover.y} />
                                            </div>
                                        )}
                                    </>
                                )}
                            </>
                        );
                    },
                    video: (film) => (
                        <div className="flex flex-col gap-3">
                            <MenuButton
                                label="Video poster"
                                onAction={({ asset }) => onChange({ ...film, poster: asset })}
                                options={Object.values(assets)
                                    .filter(Schema.is(ImageAsset))
                                    .map((asset) => ({ id: asset.id, label: asset.name, asset }))}
                            >
                                {film.poster === undefined ? 'Choose poster image' : `Poster · ${film.poster.name}`}
                            </MenuButton>
                            {film.poster && (
                                <button className="button button-ghost self-start" onClick={() => onChange(Struct.omit(film, ['poster']))} type="button">
                                    Clear poster
                                </button>
                            )}
                            <p className="hint">Poster shows before playback. Videos with speech or meaningful audio need burned-in captions. Transcripts do not replace captions</p>
                        </div>
                    ),
                }),
            )}
            <Field label="Accessible description" onChange={(alt) => onChange({ ...value, alt })} placeholder="What image or sheet conveys" value={value.alt ?? ''} />
            <Field label={descriptionLabel(value)} multiline={true} onChange={(description) => onChange({ ...value, description })} value={value.description ?? ''} />
            {entryId !== undefined && (
                <div className="actions">
                    <button className="button button-outline" onClick={() => setDraft(AsyncResult.map(Optic.replace(hero, value)))} type="button">
                        Use as hero
                    </button>
                    <button className="button button-outline" onClick={() => setDraft(AsyncResult.map(updateEntry(entryId, (current) => ({ ...current, cover: value }))))} type="button">
                        Use as cover
                    </button>
                </div>
            )}
        </div>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PlacementFields };
