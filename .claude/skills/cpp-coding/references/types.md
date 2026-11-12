# [TYPES]

## [01]-[CONSTRAINED_VALUES]

Values with an invariant keep their constructor private and build through a factory returning `std::expected`:

```cpp
class Cents {
public:
    [[nodiscard]] static constexpr auto make(std::int64_t value) noexcept -> std::expected<Cents, Refusal> {
        return value < 0 ? std::unexpected{Refusal::Negative} : std::expected<Cents, Refusal>{Cents{value}};
    }

    [[nodiscard]] constexpr auto value() const noexcept -> std::int64_t { return value_; }

    friend constexpr auto operator<=>(Cents, Cents) noexcept = default;

private:
    constexpr explicit Cents(std::int64_t value) noexcept : value_{value} {}

    std::int64_t value_;
};
```

`Refusal` is the package's refusal enumeration. Every `Cents` a consumer holds is non-negative, so no function taking one tests its sign. `make` is `constexpr`, and `static_assert(Cents::make(-1).error() == Refusal::Negative);` checks the refusal at compile time.

## [02]-[SUM_TYPES]

Closed sets of cases with data are a `std::variant` of aggregates, folded by member `visit` over one arm per alternative:

```cpp
using Posting = std::variant<Deposit, Withdrawal, Transfer>;

template <typename... Arms>
struct Overloaded : Arms... {
    using Arms::operator()...;
};

[[nodiscard]] constexpr auto signed_value(const Posting& posting) -> std::int64_t {
    return posting.visit(Overloaded{
        [](const Deposit& deposit) { return deposit.amount.value(); },
        [](const Withdrawal& withdrawal) { return -withdrawal.amount.value(); },
        [](const Transfer& transfer) { return -transfer.amount.value(); },
    });
}
```

`Deposit`, `Withdrawal`, and `Transfer` are aggregates holding a `Cents amount`. A fourth alternative fails to compile until `signed_value` gains its arm.

## [03]-[RESOURCE_OWNERS]

Owners of a handle move, refuse copies with a reason, and release once:

```cpp
class Lease {
public:
    explicit Lease(std::FILE* file) noexcept : file_{file} {}
    Lease(Lease&& other) noexcept : file_{std::exchange(other.file_, nullptr)} {}
    auto operator=(Lease&& other) noexcept -> Lease& {
        std::ranges::swap(file_, other.file_);
        return *this;
    }
    Lease(const Lease&) = delete("a lease names one open file");
    auto operator=(const Lease&) -> Lease& = delete("a lease names one open file");
    ~Lease() {
        if (file_ != nullptr) {
            std::fclose(file_);
        }
    }

private:
    std::FILE* file_;
};
```

Handles a standard owner already releases (`std::unique_ptr` with a deleter, `std::fstream`, `std::jthread`) use that owner.

## [04]-[EXPLICIT_OBJECT]

Members returning a part of the object take an explicit object parameter and forward its value category:

```cpp
struct Journal {
    std::vector<Posting> postings;

    template <typename Self>
    [[nodiscard]] auto entries(this Self&& self) -> decltype(auto) {
        return std::forward_like<Self>(self.postings);
    }
};
```

`entries` returns a `const` reference from a `const` journal and an rvalue from a temporary.

## [05]-[COMPILE_TIME_CHECKS]

Types a template or factory must refuse are stated as a concept and asserted once:

```cpp
template <typename Value>
concept FactoryBuilt = requires {
    requires !std::constructible_from<Value, std::int64_t>;
    requires !std::default_initializable<Value>;
    { Value::make(std::int64_t{}) } -> std::same_as<std::expected<Value, ledger::Refusal>>;
};

static_assert(FactoryBuilt<ledger::Cents>);
```

Requirements test a dependent type, a `requires` expression over a concrete type with a private constructor is a hard error in place of `false`.
