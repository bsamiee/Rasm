# [MODULES]

## [01]-[PRIMARY_INTERFACE]

Primary interface units re-export their interface partitions and declare nothing of their own:

```cpp
export module raster;

export import :budget;
export import :face;
```

Consumers write `import raster;` and name no partition.

## [02]-[INTERFACE_PARTITIONS]

Interface partitions export the declarations of one concept:

```cpp
export module raster:budget;

import std;

export namespace raster {

[[nodiscard]] auto bytes_for(std::uint64_t cells, std::uint32_t channels) noexcept -> std::optional<std::size_t>;

}
```

## [03]-[GLOBAL_MODULE_FRAGMENT]

Headers supplying macros or C declarations enter through the global module fragment of the one partition calling them:

```cpp
module;

#include <stdckdint.h>

module raster:checked;

import std;

namespace raster {

[[nodiscard]] constexpr auto product(std::size_t left, std::size_t right) noexcept -> std::optional<std::size_t> {
    std::size_t result{};
    return ckd_mul(&result, left, right) ? std::nullopt : std::optional{result};
}

}
```

`raster:checked` is an internal partition: `module raster:checked;` holds no `export`, and no consumer sees `product`. `ckd_mul` is a macro of `<stdckdint.h>`, and no module exports a macro.

## [04]-[IMPLEMENTATION_UNITS]

Implementation units define what an interface declares and import the internal partitions they read:

```cpp
module raster;

import std;
import :checked;

namespace raster {

auto bytes_for(std::uint64_t cells, std::uint32_t channels) noexcept -> std::optional<std::size_t> { return product(cells, channels); }

}
```

`module raster;` imports the primary interface implicitly.

## [05]-[HEADER_FACE]

Functions a consumer without named modules calls (a host compiling at its own standard) are declared in a header and defined in an ordinary source:

```cpp
#pragma once

#include <cstdint>

namespace raster {

[[nodiscard]] auto area(std::uint32_t width, std::uint32_t height) noexcept -> std::uint64_t;

}
```

One partition includes that header in its global module fragment and re-exports each function by `using`:

```cpp
module;

#include <raster/area.hpp>

export module raster:face;

export namespace raster {

using raster::area;

}
```

Functions a header declares attach to the global module, so one definition serves `import raster;` and `#include <raster/area.hpp>`. Face functions pass arithmetic types, pointers with a length, and aggregates of them, a standard library type has one layout per standard library build.
