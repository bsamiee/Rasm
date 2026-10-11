# mypy: disable-error-code="attr-defined"
# ty: ignore[unresolved-attribute]
"""Stable ACES gamut compression through native compositor math."""

from itertools import starmap

import bpy

from rasm.blender.extension.identity import Key
from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, INPUTS, item_key, masked, math, node
from rasm.blender.render.effects.stack import CompositorEffect, Shell
from rasm.imaging.effects.grade.gamut import Channel, GamutCompression

# --- [OPERATIONS] -----------------------------------------------------------------------


def compressed(shell: Shell[GamutCompression]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns compression with bounded powers and the defined zero-achromatic case."""
    limit, threshold = Channel.limit.__name__, Channel.threshold.__name__
    exponent = End(node=INPUTS, socket=item_key(GamutCompression.power.__name__))
    channels = ((GamutCompression.cyan.__name__, "Red"), (GamutCompression.magenta.__name__, "Green"), (GamutCompression.yellow.__name__, "Blue"))
    peak, achromatic, sign, magnitude, nonzero, inverse = (End(node=key) for key in ("peak", "achromatic", "sign", "magnitude", "nonzero", "inverse"))

    def channel(key: Key, socket: str) -> BlenderResult[Fragment]:
        source = End(node="channels", socket=socket)
        bound, floor = (End(node=INPUTS, socket=item_key(key, member)) for member in (limit, threshold))
        span, headroom, ratio, raised, remaining, root, scale = (End(node=item_key(key, term)) for term in ("span", "headroom", "ratio", "raised", "remaining", "root", "scale"))
        normalized, distance, over, excess, lower, upper, bounded = (End(node=item_key(key, term)) for term in ("normalized", "distance", "over", "excess", "lower", "upper", "bounded"))
        powered, grown, denominator, shrunk, base, pulled = (End(node=item_key(key, term)) for term in ("powered", "grown", "denominator", "shrunk", "base", "pulled"))
        signed, unit, candidate, active, identity = (End(node=item_key(key, term)) for term in ("signed", "unit", "candidate", "active", "identity"))
        return Fragment.joined(
            math(span.node, "SUBTRACT", bound, floor),
            math(headroom.node, "SUBTRACT", 1.0, floor),
            math(ratio.node, "DIVIDE", headroom, span),
            math(raised.node, "POWER", ratio, exponent),
            math(remaining.node, "SUBTRACT", 1.0, raised),
            math(root.node, "POWER", remaining, inverse),
            math(scale.node, "DIVIDE", headroom, root),
            math(normalized.node, "DIVIDE", source, magnitude),
            math(distance.node, "SUBTRACT", sign, normalized),
            math(over.node, "SUBTRACT", distance, floor),
            math(excess.node, "MAXIMUM", over, 0.0),
            math(lower.node, "MINIMUM", excess, scale),
            math(upper.node, "MAXIMUM", excess, scale),
            math(bounded.node, "DIVIDE", lower, upper),
            math(powered.node, "POWER", bounded, exponent),
            math(grown.node, "ADD", 1.0, powered),
            math(denominator.node, "POWER", grown, inverse),
            math(shrunk.node, "DIVIDE", lower, denominator),
            math(base.node, "MINIMUM", distance, floor),
            math(pulled.node, "ADD", base, shrunk),
            math(signed.node, "MULTIPLY", sign, pulled),
            math(unit.node, "SUBTRACT", 1.0, signed),
            math(candidate.node, "MULTIPLY", achromatic, unit),
            math(active.node, "LESS_THAN", floor, distance),
            masked(identity.node, "FLOAT", nonzero, 0.0, source),
            masked(item_key(key, "result"), "FLOAT", active, identity, candidate),
        )

    return Fragment.joined(
        node("channels", bpy.types.CompositorNodeSeparateColor, {"Image": shell.source}, {"mode": "RGB"}),
        math(peak.node, "MAXIMUM", End(node="channels", socket="Red"), End(node="channels", socket="Green")),
        math(achromatic.node, "MAXIMUM", peak, End(node="channels", socket="Blue")),
        math(sign.node, "SIGN", achromatic),
        math(magnitude.node, "ABSOLUTE", achromatic),
        math(nonzero.node, "ABSOLUTE", sign),
        math(inverse.node, "DIVIDE", 1.0, exponent),
        *starmap(channel, channels),
        node("compressed", bpy.types.CompositorNodeCombineColor, {socket: End(node=item_key(key, "result")) for key, socket in channels}, {"mode": "RGB"}),
    ).map(ended("compressed", "Image"))


# --- [COMPOSITION] ----------------------------------------------------------------------

gamut_compression = CompositorEffect(record=GamutCompression, graph=compressed, straight=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["compressed", "gamut_compression"]
