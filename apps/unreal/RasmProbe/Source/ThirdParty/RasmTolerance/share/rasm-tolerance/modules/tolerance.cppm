module;

#include <rasm/tolerance.hpp>

export module rasm.tolerance;

import std;

// --- [OPERATIONS] ----------------------------------------------------------------------

export namespace rasm {

template <std::floating_point T>
[[nodiscard]] auto within_ulps(T left, T right, unsigned ulps) noexcept -> bool {
    const auto walked = [left](T toward, unsigned steps) { return std::ranges::fold_left(std::views::iota(0U, steps), left, [toward](T at, unsigned) { return std::nextafter(at, toward); }); };
    return walked(-std::numeric_limits<T>::infinity(), ulps) <= right && right <= walked(std::numeric_limits<T>::infinity(), ulps);
}

[[nodiscard]] auto described(double left, double right) -> std::string;

using rasm::labelled;
using rasm::within;

}
