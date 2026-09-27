"""Blender's catalog and the package rows the host resolves against it, the records the host and Blender's Python exchange as JSON."""

from attrs import frozen

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class Catalog:
    """Blender's version, configuration folder, synced remote repository folders by module, bundled add-on ids, Essentials asset folder, and ID type by ID code."""

    version: str
    settings: str
    repositories: dict[str, str]
    core: frozenset[str]
    essentials: str
    types: dict[str, str]


@frozen
class Local:
    """Package row the session installs from its staged archive, or from Blender's bundled add-ons without one, and enables, with the workspaces that place it."""

    identity: str
    archive: str | None
    workspaces: tuple[str, ...] = ()


@frozen
class Listed:
    """Package row a remote repository lists, installed there from its staged archive while its version differs and enabled, with the workspaces that place it."""

    identity: str
    archive: str
    repository: str
    version: str
    workspaces: tuple[str, ...] = ()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Catalog", "Listed", "Local"]
