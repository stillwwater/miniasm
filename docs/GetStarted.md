## Variables

A variable is declared like so:

```assembly
var x: 5
```

Which is just a shortcut for this:

```assembly
allocate
define $x &ax
mov x 5
```

A variable may also have its type defined.

```assembly
int a: 7
```

By default, variables (like instructions and registers) are passed by reference. To retrieve the value stored in the variable's address, use the `&` operator.

```assembly
num x: 8
add x 3		; adds 3 to x
print &x	; <num> 11

num y: 8
add &y 3	; adds 3 and the value in y, storing the answer in the accumulator
print &y	; <num> 4 (y is unchanged)
print &ax	; <num> 11
```

To assign a variable to nil use an empty `&` operator:

```assembly
var x: &

print &x	; <nil>
print &		; <nil>
```

Variable types:

| Type   | C# Type                    | Symbol            |
| ------ | -------------------------- | ----------------- |
| `int`  | `Symbol<int>`              | `MetaSymbol.Ptr`  |
| `num`  | `Symbol<float>`            | `MetaSymbol.Num`  |
| `str`  | `Symbol<string>`           | `MetaSymbol.Str`  |
| `vec`  | `Symbol<Vector3>`          | `MetaSymbol.Vec3` |
| `list` | `Symbol<List<MetaSymbol>>` | `MetaSymbol.List` |
| `any`  | `MetaSymbol`               | `MetaSymbol`      |
| `nil`  | `Null`                     | `MetaSymbol.nil`  |

 ## Labels

Labels can be used to group instructions in a procedure. Labels can have comma separated typed parameters.

```assembly
mylabel (str name):
	mov out "hello "
	print &name
end

mylabel "mini"	; hello mini
```

Labels can define other labels. These sub-labels can only be called from inside its parent, and have access to variables defined inside its parent.

```assembly
mylabel (str name):
	mov out "hello "
	jmp mylabel.b
mylabel.b:
	print &name	; variable from parent is accessible
end

mylabel "mini"		; hello mini
mylabel.b		; UndefinedError
```

Labels support overloading, by giving them a different signature.

```assembly
mylabel (str s):
	print &s
end

mylabel (num x):
	mul x 2
	print &x
end

mylabel (int x):
	mul x 4
	print &x
end

mylabel "7"	; <str> 7
mylabel 7	; <num> 14
mylabel int 7	; <int> 28
```

For labels with multiple parameters, separate each parameter with a comma. Or don't, it's up to you.

```assembly
myadd (num a, num b):
	add a &b
end

;; this is perfectly valid
;; because arguments are just a list
myadd (num a num b):
	add a &b
end
```

Documentation can be defined before the label's header using `;;`.

```assembly
;; adds b to a and prints a
myadd (num a, num b):
	add a &b
	print &a
end

help myadd	; prints documentation to the console
```

## Sections

Sections are a way to organize procedure groups and variable definitions.

```assembly
section mysection:
	str name: "gelii"

	mylabel:
		jmp mysublabel
	mysublabel:
		jmp mysection.mylabel2
	end

	mylabel2:
		mov out "hello "
		print &mysection.name
	end

	mysection.mylabel	; hello gelii
ends
```

The preprocessor instruction `#include` can be used to avoid writing the section's name

```assembly
#include mysection
mylabel	; hello gelii
```

Registers
---

Registers can be used as a quick way to store data temporarily, as well as retrieving and setting crucial information about the running program. For general purpose data storage, use variables instead.

| Register Name | Address     | Description                                   |
| ------------- | ----------- | --------------------------------------------- |
| `R0`          | `R0`        | `-1`: error, `0`: halt, `1`: ready, 2 `debug` |
| `ax`          | `R1`        | accumulator                                   |
| `ip`          | `R2`        | current instruction                           |
| `in`          | `R6`        | input buffer                                  |
| `out`         | `R7`        | output buffer                                 |
| `cx`          | `R9`        | counter register                              |
| `R10`-`R19`   | `R10`-`R19` | current instruction parameters                |

For the first 10 registers (`R0` to `R10`), the ones with odd number addresses are the ones that get used more often.

Registers are passed by reference to an instruction, to retrieve the value stored in the register's address, use the `&` operator.

```assembly
mov ax out	; sets ax to the pointer for out
mov ax &out	; sets ax to the value stored in out
```

The output buffer register is flushed with the `io` instruction, and its contents are printed to the screen.

```assembly
mov out "hello "
add out "world!"
io out	; hello world!
```

The input buffer register can be used to receive input from the console.

```assembly
io in
```

The program status register can be used to halt the program

```assembly
mov R0 0	; halt
```

Some APIs might define special registers, again variables are often preferred since there are limited registers available. The `GameObject` API uses `R20`-`R25 `to give information about the current object's transform and the status of the scene.

## Expressions

A line that can be executed (any line that is not a definition) is an expression. The syntax consists of an optional return address, followed by an instruction, followed by zero or more arguments. By default the return address for an expression is the accumulator (`R1` or `ax`).

```assembly
print "hello world!"	; hello world!

mov R3 8		; set R3 to 8
add R3 3		; add 3 to the value in R3
print &R3		; <num> 11

add 8 3			; returns answer to ax
print &ax		; <num> 11

R3 add 8 3		; returns answer to R3
print &R3		; <num> 11

num x: 8
add x 3
print &x		; <num> 11
```
