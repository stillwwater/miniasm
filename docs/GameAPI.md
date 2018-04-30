## go :: APIs.GameObjectAPI

The `GameObject` API is used to interface with the Unity Engine.

> The `go` API is loaded by default, but it is not included, prefix its instructions with `go.` or use `#include go`.

`spawn (str prefab) -> str`: Instantiates a new `GameObject` from `prefab`.

`clone (str gameobject_id) -> str`: Clones `gameobject` with name `gameobject_id`

`remove (str gameobject_id) -> nil`: Destroys `gameobject` with name `gameobject_id`

---

The following instructions use a `stack` known as the game object buffer. This buffer contains the address of game objects in the scene (all objects referenced in the API's `gameObjects` list).

Instructions can use this buffer to apply transformations to multiple objects at once.

If the `go.buffermode` is set to `multi` (default) or `buffered`, the instruction will apply to all objects in the buffer. Otherwise, if `go.buffermode` is set to `single` or `top`, the instruction will only apply to the object referenced at the top of the buffer stack.

```assembly
go.buffermode $multi	; transformations apply to all objects in buffer
go.buffermode $single	; transformations apply to last object in buffer (top of stack)
```

`push (str gameobject_id) -> nil`: Pushes a reference to `gameobject` to the buffer

`pop -> str`: Pops and returns the last object in the buffer

`clear -> nil`: Clears the buffer

`buffermode (str mode) -> nil`: Changes the `buffermode`

`move (vec pos) -> nil`: Moves objects in buffer by a relative position `pos`

`rotate (vec rot) -> nil`: Rotates objects in buffer by a relative rotation`rot`

`scale (vec scl) -> nil`: Scale objects in buffer by a relative scale`scl`

`remove -> nil`: Destroys objects in buffer

`clone -> nil`: Clones objects in buffer

```assembly
go.spawn $cube		; instantiate a cube
go.push &ax		; push a reference to the object spawned into the buffer
go.move (0 1 0)		; move the object by 1 unit in the y axis

go.spawn $cube
go.push &ax		; spawn and push another cube
print &buffersize	; <num> 2

go.clone		; clone objects in the buffer
			; this clears the buffer and pushes the new objects
print &buffersize	; <num> 2
print &scenesize	; <num> 4
go.move (0 2 0)		; move the objects on top of the original 2 cubes
```

---

The `go` API define a few registers. `R4-R6` are accumulators which can be read and written to. `R20-R26` contain information about the scene, they may be read and written to, however they are overwritten by the API after a state change.

| Register Name | Address | Type  | Description                       |
| ------------- | ------- | ----- | --------------------------------- |
| `rx`          | `R4`    | `num` | The `x` accumulator               |
| `ry`          | `R5`    | `num` | The `y` accumulator               |
| `rz`          | `R6`    | `num` | The `z` accumulator               |
| `gx`          | `R20`   | `str` | Last object in buffer             |
| `pos`         | `R21`   | `vec` | Position of last object in buffer |
| `scl`         | `R22`   | `vec` | Scale of last object in buffer    |
| `rot`         | `R23`   | `vec` | Rotation of last object in buffer |

## go :: native

Extended `go` library

`spawn (str prefab, vec pos) -> nil`: Instantiates `prefab` and moves it to `pos`

`push (list objects) -> nil`: Pushes multiple objects to the buffer

---

`fixed (int fptr, vec from, vec to) -> nil`: Helper used to transform objects to a non relative vector.

`fmove (vec newpos) -> nil`: Moves last object in buffer to a fixed position `newpos`

`frotate (vec newrot) -> nil`: Rotates last object in buffer to a fixed rotation`newrot`

`fscale (vec newscl) -> nil`: Scales last object in buffer to a fixed scale `newscl`

---

`box (str prefab, vec scale) -> nil`: Creates a box made from `prefab`

`rplace (str prefab, vec scale, num threshold) -> nil`: randomly places objects in a grid

## game :: APIs.GameAPI

The `game` API is used to interface with the Unity Engine, however it is not specific to `gameobjects`, rather it exposes other engine features.

`loop (int callptr) -> nil`: calls instruction at `callptr` every frame

```asm
update:
	go.move (0 0.02 0)
end

go.spawn $cube
go.push &ax

loop update
```
