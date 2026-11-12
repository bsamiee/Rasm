#include <catch2/catch_test_macros.hpp>

import std;
import rasm.extent;

// --- [OPERATIONS] ----------------------------------------------------------------------

TEST_CASE("extent, closed at both ends", "[extent]") {
    constexpr rasm::Extent unit{.lower = 0.0, .upper = 1.0};

    SECTION("length is the distance between its ends") { STATIC_REQUIRE(unit.length() == 1.0); }

    SECTION("an end one rounding step outside still reaches") {
        REQUIRE(unit.reaches(std::nextafter(1.0, 2.0)));
        REQUIRE_FALSE(unit.reaches(1.5));
    }
}
