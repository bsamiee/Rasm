# Conclusion

## Wrap-up

Functional programming is powerful and elegant. It excels at two essential programming techniques: abstraction and composition.

### Abstraction

- Functions abstract computations, they work for any suitable argument rather than one particular value. A square function, for example, expresses how to square any number instead of only one specific number.
- Functional and imperative languages commonly use functions for this kind of abstraction. Functional programming extends it by treating functions as first-class values: functions can be passed as inputs and returned as outputs.
- Higher-order functions capture highly general computation patterns. `accumulate`, `map`, `filter`, and `fold` are key examples.
- Because these mechanisms express reusable methods of computation directly, functional programming is especially strong at abstraction.

### Composition

Composition builds large programs from smaller programs. The functional paradigm presented in the book supports it through several mutually reinforcing properties:
- Everything is an expression in this paradigm. Conditionals and functions can be combined with smaller expressions to form more complex ones.
- Pure functions return the same output for the same input, making their behavior predictable when combined.
- Functions in this paradigm operate on immutable data. Rather than modifying existing state, they construct new data from their inputs.
- Purity and immutability make functions close to mathematical functions and make new functions easier to construct by composition.
- Functions that agree on a shared structure for inputs or outputs can be connected in a dataflow style. For example, `map`, `filter`, and `fold` compose naturally because lists provide their shared interface.

## The ideas are simpler than they first appear

The functional way of thinking can initially seem intimidating, but its major ideas are inherently simple once understood:
- First-class functions
- Higher-order functions
- Pure functions
- Immutable data
- Currying
- Partial application

## Where to go next

The most effective next step is to apply functional programming in real projects. Mainstream languages such as Kotlin, Java, JavaScript, Swift, Python, and Scala readily support functional programming.

Books, tutorials, blog posts, and documentation can support deeper study; the right resource depends on what you want to explore.

For deeper study of functional thinking:

- Structure and Interpretation of Computer Programs (SICP, also called the Wizard Book), by Harold Abelson and Gerald Jay Sussman with Julie Sussman
- Thinking Functionally with Haskell, by Richard Bird

For deeper study of OCaml and Haskell:

- Real World OCaml
- Real World Haskell

For functional programming in a particular language, use the available learning materials to learn its syntax and idioms. The underlying principles remain valid across languages.

## Mastery

Mastering functional programming requires balancing two activities:

1. Strive to understand the fundamental principles.
2. Apply those principles pragmatically to real-world problems.
