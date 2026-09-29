"""Applies the interface independently to each installed product among Illustrator, Photoshop, InDesign, and Acrobat."""

import anyio

from interface.adobe import acrobat, illustrator, indesign, photoshop
from interface.adobe.session import converged
from interface.host import Applied, Failed, Host

# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Applied | Failed, ...]:
    """Outcome of each installed product."""
    return tuple(filter(None, await anyio.gather(*(converged(host, product) for product in (illustrator.PRODUCT, photoshop.PRODUCT, indesign.PRODUCT, acrobat.PRODUCT)))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
