## Mini Preprocessor

Mini's preprocessor, or compiler, is used to create definitions to make code more readable. Variables, for example, use the preprocessor the assign a label to an address.

## \#include :: Preprocessor.INCLUDE

```assembly
#include library
```

`library.` will be prepended to all of it's definitions. Thus if `library` defines a label `mylabel`, it can be accessed as `mylabel` instead of `library.mylabel`.

## #define :: Preprocessor.DEFINE

```assembly
#define x R8
```

All instances of `x` will be replaced with `R8`.

## #undefine :: Preprocessor.UNDEF

```assembly
#undefine x
```

`x` will no longer be replaced.

## #section :: Preprocessor.SECTION_DEF

```assembly
#section mysection
```

Begins a new section, all definitions made inside the section will be part of `mysection`.

Prefer using the `section` keyword instead of using the preprocessor directly:

```assembly
section mysection:
ends
```

## #ends :: Preprocessor.SECTION_END

```assembly
#ends
```

Ends a a section.