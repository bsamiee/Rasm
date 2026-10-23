import type { Link } from 'adobe:indesign';
import { flow, Option, Schema } from 'effect';
import { read } from '#/dom/encoding';
import type { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';

// --- [MODELS] --------------------------------------------------------------------------

class LinkView extends Schema.TaggedClass<LinkView, { readonly brand: unique symbol }>()('Link', {
    ...Reference.fields,
    status: Schema.String,
    filePath: Schema.String,
    linkResourceURI: Schema.String,
    linkType: Schema.String,
    needed: Schema.Boolean,
    edited: Schema.Boolean,
    size: Schema.Finite.annotate({ description: 'Source file size in bytes' }),
    date: Schema.String.annotate({ description: 'Source file modification date' }),
    parent: Reference.annotate({ description: 'Placed graphic or story' }),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<Link>('linkResourceURI')), Option.map(LinkView.of));
    static readonly of = (link: Link) =>
        read(LinkView, link.toSpecifier(), () => ({ _tag: 'Link', ...Reference.of(link), status: link.status, filePath: link.filePath, linkResourceURI: link.linkResourceURI, linkType: link.linkType, needed: link.needed, edited: link.edited, size: link.size, date: link.date, parent: link.parent }));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { LinkView };
