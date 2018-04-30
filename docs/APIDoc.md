bootloader :: APIs.Bootloader
---

Loads APIs into memory.

`require (str apiname) -> nil`: Loads an API defined in C#, if a reference to it exists in the memory

## stdio :: APIs.STD.InputOutput

`peek -> nil`: Prints memory contents

`help (str instruction) -> nil`: Returns the documentation for an instruction

`dump -> nil`: Dumps memory contents to `mini.dmp`

`dump (str file) -> nil`: Dumps memory contents to file

---

`io (int addr)`: Prints output or receives input depending on the address passed.

---

`import (str library)`: Executes a file located in the `Lib` directory

`load (str source)`: Executes a source file

## kw :: APIs.STD.Keywords

`mov (int addr, any value) -> nil`: moves value to address addr

`mov (any value) -> any`: moves value to accumulator

---

`for (int counter_addr, num max, int instruction_addr) -> nil`: Loops from `0` to `max`, storing iterator in `counter_addr` and calling `instruction_addr`

```assembly
for cx 5 pcx	; 0 1 2 3 4
```

## comp :: APIs.STD.Comparisons

`cmp (any a, any b) -> num`: compares two values, returns answer to `ax`

`ctype (any a, any b) -> num`: compares types of two values

`ctype (int addr, str type) -> num`: compares type of value in `addr` with the string representation of the type

---

`jg (int addr) -> nil`: Jumps to `addr` if `ax`is greater than `0`

`jl (int addr) -> nil`: Jumps to `addr` if `ax`is less than `0`

`je (int addr) -> nil`: Jumps to `addr` if `ax`is `0`

`jne (int addr) -> nil`: Jumps to `addr` if `ax`is not`0`

```assembly
mylabel:
	print $true
end

cmp 2 5
print &ax	; <num> -1
jl mylabel	; true

str a: "hello"
cmp &a $hello
print &ax	; <num> 0
je mylabel	; "hello" == $hello: true

mov R5 &a	; copy value in a into R5
mov a "hello world"
cmp &a &R5
print &ax	; <num> 1
jg mylabel	; true

```

---

`jmp (int addr) -> nil`:

`jmp (int addr, any param) -> nil`: Jumpts to addr

## string :: APIs.STD.String

`add (int addr, str b) -> nil`: Concatenates string b to address addr

`add (str a, str b) -> str`: Concatenates strings a and b, returns answer to accumulator

---

`str (num a) -> str`: Converts a to a string, returns answer to accumulator

```assembly
str s: $hello

mov s "hello world"
mov s 'hello '
add s $world!

print &s	; hello world!
```

## vector :: APIs.STD.Vector

`mov (int addr, vec value, str axis) -> nil`: Sets value in `addr` to the value of `axis` of vector `value`

```assembly
var a: (1.0 1.1 1.2)
mov ax &a $y
print &ax	; <num> 1.1
```

`mov (int addr, str axis, num value) -> nil`: Sets the `axis` of a vector stored in `addr` to `value`

```assembly
var a: (1.0 1.1 1.2)
mov a $x 3.4
primt &a	; <vec> (3.4 1.1 1.2)
```

---

`for (int counter_addr, vec max, int instruction_addr) -> nil`: Loops from `(0 0 0)` to vector `max`, storing the iterator in `counter_addr` and calling `instruction_addr`.

```assembly
for cx (1 2 2) pcx
; <vec> (0 0 0)
; <vec> (0 0 1)
; <vec> (0 1 0)
; <vec> (0 1 1)
```

## list :: APIs.STD.List

`size (list) -> num`: Returns the size of a list

---

`push (int addr, any value) -> nil`: Pushes `value` to a list stored in `addr`

`pop (int addr) -> any`: Pops value from a list stored in `addr` and returns it to the acumulator

`pop (int addr, num index) -> any`: Pops value from a list stored in `addr` at `index` and returns it.

```assembly
var mylist: num (1 2 3)	; define a list of number

push mylist 4		; push 4 to mylist
push mylist 5		; push 5 to mylist
print &mylist		; (1 2 3 4 5)

pop mylist		; pop value from mylist
print ax		; <num> 5
print &mylist		; (1 2 3 4)

pop mylist 2		; pop value from mylist at index 2
print ax		; <num> 3
print &mylist		; (1 2 4)
```

---

`mov (int addr, num index, any value) -> nil`: Set the value at the `index` of a list stored in `addr` to `value`

```assembly
var mylist: num (1 2 3 4)	; define a list of numbers
mov mylist 1 3			; move value 3 into index 1 of mylist
print &mylist			; (1 3 3 4)
```

`mov (int addr, list value, num index) -> nil`: Set the value in `addr` to the value in the `index` of the list `value`

```assembly
var mylist: str ($a $b $c)	; define a list of strings
mov ax &mylist 1		; move value stored in index 1 of mylist to ax
print &ax			; <str> b
```

---

`for (int counter_addr, list items, int instruction_addr) -> nil`: Loops through a list `items`

```assembly
var mylist: num (0 2 4 6 8)
for cx &mylist pcx	; 0 2 4 6 8
```

## math :: APIs.STD.Math

`add (int addr, num b) -> nil`:

`add (int addr, vec b) -> nil`: Adds `b` to address `addr`.

---

`add (num a, num b) -> num`:

`add (int a, int b) -> int`:

`add (vec a, vec b) -> vec`: Adds `a` and `b`, returns answer to accumulator

---

`sub (int addr, num b) -> nil`:

`sub (int addr, vec b) -> nil`: Substracts `b` from address `addr`.

---

`sub (num a, num b) -> num`:

`sub (int a, int b) -> int`:

`sub (vec a, vec b) -> vec`: Subtracts `a` and `b`, returns answer to accumulator

---

`mul (int addr, num b) -> nil`: Multiplies value in `addr` by `b`

`mul (num a, num b) -> num`: Multiplies `a` and `b`

`mul (vec a, num b) -> vec`: Multiplies vector `a` by a constant `b`

---

`div (int addr, num b) -> nil`: Divides value in `addr` by `b`

`div (num a, num b) -> num`: Divides `a` by `b`

`div (vec a, num b) -> vec`: Divides vector `a` by a constant `b`

---

`mod (int addr, num value) -> nil`: Perform modulus operation on value at address `addr`

`mod (num a, num b) -> num`: Performs modulus operation between `a` and `b`

---

`pow (int addr, num value) -> nil`: Raises value in `addr` to the power `value`

`pow (num a, num b) -> num`: Raises `a` to the power `b`

---

`abs (num a) -> num`: Return the absolute value of `a`

`ceil (num a) -> num`: Return the ceil value of `a`

`floor (num a) -> num`: Return the floored value of `a`

`round (num a) -> num`: Return the rounded value of `a`

`sin (num a) -> num`: Return the sin of `a`

`cos (num a) -> num`: Return the cos of `a`

`tan (num a) -> num`: Return the tan of `a`

---

`rand -> num`: Returns a random number

`perlin (vec pos) -> num`: Returns  Perlin noise value at `pos`

## std :: native

`error (str msg) -> nil`: Outputs error

`error (str msg, int callptr) -> nil`: Outputs error and location of error

`throw (str msg, int callptr) -> nil`: Outputs error and halts

`halt -> nil`: Halts

`print (any msg) -> nil`: Prints to the console

`print (str msg) -> nil`: Prints `out` plus `msg` to the console

`pcx -> nil`: Prints contents in the counter register (`CX`)

`inc (int addr) -> nil`: Increments address

`dec (int addr) -> nil`: Decrements address

`setall (list registers, any value) -> nil`: Sets multiple registers to value

## file :: APIs.File

> Not included by default, use `require $file`

> Prefix function names with `file.` or use `#include file`

`remove (str path) -> nil`: Removes a file

---

`write (str path, str content) -> nil`: Write contents to file

`append (str path, str content) -> nil`: Append contents to file

`read (str path) -> str`: Read contents from file
