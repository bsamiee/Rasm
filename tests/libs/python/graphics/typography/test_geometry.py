"""Outline geometry the rebuilds share: offsets of outlines the native boolean refused raw, and unions of near-coincident parts."""

import pytest

from rasm.typography import area, offset, Point, union

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_offset_grows_an_outline_whose_raw_rim_refuses_the_boolean() -> None:
    """Mohreh Italic's sheared hamza grown by a half stem reaches past every side of the original by the distance and at most the miter limit's four times it, the stroke rim the native union refused before it was simplified on the trace grid."""
    outline = tuple(
        tuple(Point(x=x, y=y, on=on) for x, y, on in contour)
        for contour in (
            (
                (-90.0413818359375, 1936.89013671875, True),
                (-157.8560028076172, 1936.89013671875, False),
                (-207.75071716308594, 1869.0755615234375, False),
                (-181.08322143554688, 1794.8248291015625, True),
                (-162.07447814941406, 1743.8013916015625, True),
                (-149.36181640625, 1709.3712158203125, False),
                (-171.96389770507812, 1678.771484375, False),
                (-202.09288024902344, 1678.771484375, True),
                (-228.10482788085938, 1678.771484375, True),
                (-181.08322143554688, 1549.712158203125, True),
                (245.11264038085938, 1549.712158203125, True),
                (198.09103393554688, 1678.771484375, True),
                (56.0257453918457, 1678.771484375, True),
                (-9.004137992858887, 1678.771484375, False),
                (-44.748207092285156, 1776.8775634765625, False),
                (-22.189058303833008, 1807.830810546875, False),
                (9.004137992858887, 1807.830810546875, True),
                (151.06942749023438, 1807.830810546875, True),
                (104.04781341552734, 1936.89013671875, True),
            ),
            (
                (227.4772186279297, 1136.5223388671875, True),
                (124.05701446533203, 1420.65283203125, True),
                (-134.06161499023438, 1420.65283203125, True),
                (-30.764169692993164, 1136.5223388671875, True),
            ),
            ((743.7144165039062, 1136.5223388671875, True), (640.2942504882812, 1420.65283203125, True), (382.1756286621094, 1420.65283203125, True), (485.5958251953125, 1136.5223388671875, True)),
        )
    )
    distance = 387.17793103448275
    grown = offset(outline, distance)
    (xmin, ymin, xmax, ymax), (left, bottom, right, top) = (
        (min(xs), min(ys), max(xs), max(ys)) for held in (outline, grown) for xs, ys in (([point.x for contour in held for point in contour], [point.y for contour in held for point in contour]),)
    )
    reaches = (xmin - left, ymin - bottom, right - xmax, top - ymax)
    assert all(distance - 1 <= reach <= 4 * distance for reach in reaches), reaches
    assert area(grown) > area(outline)


def test_union_of_near_coincident_seam_parts_keeps_both() -> None:
    """Two Mohreh Italic seam strips sharing an edge within a hundredth of a unit, whose unsnapped union kept 1628 of their 5797 square units."""
    first = (
        (
            Point(x=9066.6669921875, y=258.0),
            Point(x=9066.6669921875, y=-0.0036072274670004845),
            Point(x=9067.3310546875, y=0.0, on=False),
            Point(x=9068.0, y=0.0),
            Point(x=9088.0, y=0.0),
            Point(x=9088.0, y=258.0),
        ),
    )
    second = (
        (
            Point(x=9066.6669921875, y=313.0),
            Point(x=9066.6669921875, y=-1.1368683772161603e-13),
            Point(x=9072.0, y=0.0),
            Point(x=9072.0, y=197.0),
            Point(x=9088.0, y=152.6056365966797),
            Point(x=9088.0, y=258.0),
            Point(x=9072.0, y=258.0),
            Point(x=9072.0, y=313.0),
        ),
    )
    assert area(union(first, second)) == pytest.approx(area(first) + (9072.0 - 9066.6669921875) * (313.0 - 258.0), abs=2.0)
