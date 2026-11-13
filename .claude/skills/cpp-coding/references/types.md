# [TYPES]

## [01]-[CONSTRAINED_VALUES]

Values with an invariant keep their constructor private and build through a factory returning `std::expected`:

```cpp
class Extent {
public:
    [[nodiscard]] static constexpr auto make(std::uint32_t width, std::uint32_t height) noexcept -> std::expected<Extent, Refusal> {
        return width == 0 || height == 0 ? std::unexpected{Refusal::Empty} : std::expected<Extent, Refusal>{Extent{width, height}};
    }

    [[nodiscard]] constexpr auto width() const noexcept -> std::uint32_t { return width_; }

    [[nodiscard]] constexpr auto height() const noexcept -> std::uint32_t { return height_; }

    [[nodiscard]] constexpr auto cells() const noexcept -> std::uint64_t { return std::uint64_t{width_} * height_; }

    friend constexpr auto operator==(Extent, Extent) noexcept -> bool = default;

private:
    constexpr Extent(std::uint32_t width, std::uint32_t height) noexcept
        : width_{width}, height_{height} {}

    std::uint32_t width_;
    std::uint32_t height_;
};
```

`Refusal` is the package's refusal enumeration. Every `Extent` a consumer holds has a nonzero width and height, and `cells` widens one operand before it multiplies. `make` is `constexpr`, and `*Extent::make(0, 4)` in a constant expression fails to compile.

## [02]-[TAGGED_VALUES]

Quantities sharing one representation take one type per meaning, declared through one template:

```cpp
template <typename Tag, std::regular Value>
struct Tagged {
    Value value;

    friend constexpr auto operator<=>(const Tagged&, const Tagged&) = default;
};

using Column = Tagged<struct ColumnTag, std::uint32_t>;
using Row = Tagged<struct RowTag, std::uint32_t>;

[[nodiscard]] constexpr auto offset(Extent extent, Column column, Row row) noexcept -> std::uint64_t { return (std::uint64_t{row.value} * extent.width()) + column.value; }
```

`offset(extent, Row{1}, Column{3})` fails to compile, where plain `std::uint32_t` parameters accept either order.

## [03]-[SUM_TYPES]

Closed sets of cases with data are a `std::variant` of aggregates, folded by member `visit` over one arm per alternative:

```cpp
struct Solid {
    Rgb color;
};

struct Stripes {
    Rgb even;
    Rgb odd;
};

using Fill = std::variant<Solid, Stripes>;

template <typename... Arms>
struct Overloaded : Arms... {
    using Arms::operator()...;
};

[[nodiscard]] constexpr auto color_at(const Fill& fill, Row row) noexcept -> Rgb {
    return fill.visit(Overloaded{
        [](const Solid& solid) { return solid.color; },
        [row](const Stripes& stripes) { return row.value % 2 == 0 ? stripes.even : stripes.odd; },
    });
}
```

`Rgb` is an aggregate of `red`, `green`, and `blue`. A third alternative of `Fill` fails to compile until `color_at` gains its arm.

## [04]-[ENUMERATIONS]

Switches over an enumeration return from every enumerator and hold no `default`:

```cpp
enum class Channel : std::uint8_t {
    Red,
    Green,
    Blue,
};

[[nodiscard]] constexpr auto level(Rgb color, Channel channel) noexcept -> std::uint8_t {
    switch (channel) {
    case Channel::Red:
        return color.red;
    case Channel::Green:
        return color.green;
    case Channel::Blue:
        return color.blue;
    }
    std::unreachable();
}
```

`std::unreachable()` is sound where every `Channel` comes from an enumerator or from a decode refusing other values.

## [05]-[RESOURCE_OWNERS]

Owners of a handle no standard owner releases move, refuse copies with a reason, and release once:

```cpp
class Mapping {
public:
    explicit Mapping(std::span<std::byte> region) noexcept
        : region_{region} {}

    Mapping(Mapping&& other) noexcept
        : region_{std::exchange(other.region_, {})} {}

    auto operator=(Mapping&& other) noexcept -> Mapping& {
        unmap(std::exchange(region_, std::exchange(other.region_, {})));
        return *this;
    }

    Mapping(const Mapping&) = delete ("a mapping names one region");
    auto operator=(const Mapping&) -> Mapping& = delete ("a mapping names one region");

    ~Mapping() { unmap(region_); }

    [[nodiscard]] auto bytes() const noexcept [[clang::lifetimebound]] -> std::span<const std::byte> { return region_; }

private:
    static auto unmap(std::span<std::byte> region) noexcept -> void;

    std::span<std::byte> region_;
};
```

`unmap` releases a region and accepts an empty one. Both move operations empty their source through `std::exchange`, a defaulted move copies `region_` and both objects release it. `bytes` binds its result to the object, and a span taken from a temporary `Mapping` is a compiler diagnostic.
