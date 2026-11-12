#include <catch2/catch_test_macros.hpp>
#include <rasm/tolerance.hpp>

import std;
import rasm.tolerance;

// --- [OPERATIONS] ----------------------------------------------------------------------

TEST_CASE("tolerance operations", "[tolerance]") {
    SECTION("a square root round-trips within four units in the last place") { REQUIRE(rasm::within_ulps(std::sqrt(1000.0) * std::sqrt(1000.0), 1000.0, rasm::default_ulps)); }

    SECTION("values further apart than the walk are refused") { REQUIRE_FALSE(rasm::within_ulps(1.0, 1.0 + 1.0e-9, rasm::default_ulps)); }

    SECTION("a comparison, described [in text]") { REQUIRE(rasm::described(1.0, 1.0) == "1 within tolerance of 1: true"); }
}
