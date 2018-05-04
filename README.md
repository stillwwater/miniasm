## MiniASM

MiniASM is a small programming language I created to aid with game development. There are many languages that can be embedded in the C# runtime, but none of those are weird enough.

Check out the get started guide, how to extend interpreter functionality and the API docs to become more familiar with the use of the language:

- [Get Started](docs/GetStarted.md)
- [API Documentation](docs/APIDoc.md)
- [Game API](docs/GameAPI.md)
- [Extending the interpreter](docs/ExtendingTheInterpreter.md)
- [Preprocessor Documentation](docs/Preprocessor.md)

Here are some example programs:

### Hello World

```assembly
mov out "hello world!"
io out			; hello world!
```

```assembly
;; this is a label
;; it can be used like a macro to group instructions
print (str s):
	mov out &s	; change 'mov' to 'add' for a superior implementation
	io out
end

print "hello world!"	; hello world!
```

### Cat

```assembly
io in
mov out &in
io out
```

### Factorial

```assembly
;; returns factorial of n
f! (num n):
	num a: 1
	f!.b
	ret &a
f!.b:
	mul a &n
	dec n
	cmp &n 1
	jg f!.b
end

f! 5	; <num> 120
```

### Quine

```assembly
mov R0 2
```

### Placing game objects randomly in a 3D grid

```assembly
#include go

place (str prefab, vec scale, num threshold):
	var objs: str ( )	; list to hold references to spawned objects
	mov pos (0 0 0)		; reset origin
	for cx &scale getrand
	push &objs		; push spawned objects to the game object stack
getrand:
	rand
	cmp &threshold &ax
	jg spawnobj
spawnobj:
	spawn &prefab
	push &ax
	move &cx
	pop
	push objs &ax		; push reference to object to objs list
end

place $cube (5 5 5) 0.5
```
### 99 bottles of beer

```assembly
99bottles:
	num bottle : 100
	jmp nextbottle
nextbottle:
	dec bottle
	str &bottle		; convert bottle to string
	mov out &ax		; move bottle to output register
	add out ' bottles of beer on the wall, '
	str &bottle
	add out &ax
	print ' bottles of beer'
	mov out 'take one down and pass it around, '
	sub &bottle 1
	str &ax
	add out &ax
	print ' bottles of beer on the wall.'
	cmp &bottle 1
	jg nextbottle		; loop while bottle is greater than 1
end

99bottles
```

## Install

**Unity:** copy contents from `src` to your unity project (mini is written in C# 4 to support the Unity engine).

**Windows:** compile the source as a library or download the release executable to test it out in the console.

**Unix:** compile the source.

---

If you want syntax highlighting for mini in your editor set language to assembly. <sup>haha</sup>
