#include <rasm/tolerance.hpp>

#include <algorithm>
#include <bit>
#include <cmath>
#include <cstdint>
#include <limits>

// --- [OPERATIONS] ----------------------------------------------------------------------

namespace rasm {

auto within(double left, double right, unsigned ulps) noexcept -> bool {
    const auto ordered = [](double value) {
        const auto bits = std::bit_cast<std::int64_t>(value);
        return bits < 0 ? std::numeric_limits<std::int64_t>::min() - bits : bits;
    };
    const auto low = std::min(ordered(left), ordered(right));
    const auto high = std::max(ordered(left), ordered(right));
    return !std::isnan(left) && !std::isnan(right) && static_cast<std::uint64_t>(high) - static_cast<std::uint64_t>(low) <= ulps;
}

}
