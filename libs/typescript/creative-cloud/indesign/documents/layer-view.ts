import type { Layer } from 'adobe:indesign';
import { flow, Option, Schema, Struct } from 'effect';
import { read } from '#/dom/encoding';
import type { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';

// --- [MODELS] --------------------------------------------------------------------------

class LayerView extends Schema.TaggedClass<LayerView, { readonly brand: unique symbol }>()('Layer', {
    ...Reference.fields,
    index: Schema.Int,
    ...Struct.Record(['visible', 'locked', 'printable', 'ignoreWrap'], Schema.Boolean),
    layerColor: Schema.Json.annotate({ description: 'UIColors member or RGB values' }),
    items: Schema.Int,
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<Layer>('layerColor')), Option.map(LayerView.of));
    static readonly of = (layer: Layer) => read(LayerView, layer.toSpecifier(), () => ({ _tag: 'Layer', ...Reference.of(layer), index: layer.index, visible: layer.visible, locked: layer.locked, printable: layer.printable, ignoreWrap: layer.ignoreWrap, layerColor: layer.layerColor, items: layer.pageItems.length }));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { LayerView };
