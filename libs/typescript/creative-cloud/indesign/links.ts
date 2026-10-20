import type { Link } from 'adobe:indesign';
import { flow, Option, Schema } from 'effect';
import type { Model } from './model.ts';
import { Reference, read } from './references.ts';

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
    parent: Reference.annotate({ description: 'Graphic or story the link places' }),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<Link>('linkResourceURI')), Option.map(LinkView.of));
    static readonly of = (link: Link) =>
        read(LinkView, link.toSpecifier(), () => ({ _tag: 'Link', ...Reference.of(link), status: link.status, filePath: link.filePath, linkResourceURI: link.linkResourceURI, linkType: link.linkType, needed: link.needed, edited: link.edited, size: link.size, date: link.date, parent: link.parent }));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { LinkView };
