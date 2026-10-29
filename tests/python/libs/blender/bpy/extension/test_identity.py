"""Extension identity: host names and identifiers derived from the manifest, manifest and folder reads, and add-on preferences lookup."""

from uuid import NAMESPACE_URL, uuid5

from expression import Error, Nothing, Ok, Some
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "base,name",
    [
        ("Panel", "FIXTURE_PT_main"),
        ("Menu", "FIXTURE_MT_main"),
        ("Header", "FIXTURE_HT_main"),
        ("UIList", "FIXTURE_UL_main"),
        ("AssetShelf", "FIXTURE_AST_main"),
        ("FileHandler", "FIXTURE_FH_main"),
        ("GizmoGroup", "FIXTURE_GGT_main"),
        ("Gizmo", "FIXTURE_GT_main"),
        ("RenderEngine", "FIXTURE_MAIN"),
        ("Operator", "fixture.main"),
        ("PropertyGroup", f"GROUP_{uuid5(NAMESPACE_URL, 'fixture/main').hex}"),
    ],
)
def test_names_follow_the_host_form_of_each_base(observed: Observations, base: str, name: str) -> None:
    """One key names a class of each host base in that base's form, a property group by a digest of the extension and key."""
    assert observed["extension.names", base] == Ok(name)


def test_names_and_keys_outside_host_limits_are_refused(observed: Observations) -> None:
    """A name past the host's 63 bytes and a key outside lowercase snake case fault, and a valid key prefixes the extension ID."""
    assert observed["extension.refusals"] == {"long": Error(("UNREGISTERED",)), "key": Error(("UNREGISTERED",)), "identifier": Ok("fixture_amount")}


def test_read_holds_manifest_and_user_folder(observed: Observations) -> None:
    """Identity holds the package, manifest fields, and the user folder Blender keeps for the extension."""
    assert observed["extension.read", "package"] == "bl_ext.user_default.fixture"
    assert observed["extension.read", "manifest"] == ("fixture", "Fixture", "1.0.0", "https://docs.example.org/fixture/")
    assert observed["extension.read", "folder"] == ("user_default", "fixture")


def test_read_refuses_a_package_blender_holds_as_no_extension(observed: Observations) -> None:
    """A package with no manifest that Blender holds as no extension faults for both."""
    assert observed["extension.read", "foreign"] == Error(("UNREADABLE", "UNREGISTERED"))


def test_preferences_read_as_their_registered_class(observed: Observations) -> None:
    """Add-on preferences read as the class the extension registered, named for its package, and as no other class."""
    assert observed["extension.preferences"] == {"own": Some("bl_ext.user_default.fixture"), "foreign": Nothing}
