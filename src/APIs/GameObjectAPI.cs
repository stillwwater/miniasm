#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniASM;
using MiniASM.Builtin;
using System;

namespace MiniASM.APIs
{
    class GameObjectError : InterpreterError
    {
        public GameObjectError(object msg)
            : base(string.Format("GXError: {0}", msg)) { }

        public GameObjectError(object msg, object nullobj)
            : base(string.Format("GXError: {0}", msg, nullobj)) { }
    }

    [CreateAssetMenu(menuName = "Interpreter APIs/GameObject", order = 1)]
    public class GameObjectAPI : ScriptableObject
    {
        public static Action<object> output = Debug.Log;
        public static Action<object> warn = Debug.LogWarning;
        protected Interpreter mini;
        protected Transform parent;
        protected GameObject[] prefabs;
        protected List<Transform> gameObjects;
        protected Stack<int> buffer;

        // if true, transformations only apply to the object at the top of the buffer stack
        bool singleTransformMode;

        bool EmptyBuffer {
            get { return buffer.Count == 0; }
        }

        public void Init(Interpreter mini, Transform parent, GameObject[] prefabs) {
            gameObjects = new List<Transform>();
            buffer = new Stack<int>();
            this.prefabs = prefabs;
            this.parent = parent;
            this.mini = mini;
        }

        public void AddTranform(Transform transform) {
            gameObjects.Add(transform);
        }

        #region API methods

        public Symbol<string> Spawn(Symbol<string> prefabName) {
            var prefab = FindPrefab(prefabName.Value);

            if (prefab == null) {
                throw new GameObjectError("Could not find", prefabName.Value);
            }

            var newObj = Instantiate(prefab, parent);
            newObj.name = string.Format("{0}_{1}", prefab.name, GenObjectID());

            gameObjects.Add(newObj.transform);

            return new Symbol<string>(Tokens.STR, newObj.name);
        }

        public Symbol<string> Clone(Symbol<string> name) {
            var gx = gameObjects[FindIndex(name.Value)];
            string pname = gx.name;

            var tmp = Instantiate(gx.gameObject, parent);

            if (pname.Contains("_")) {
                pname = pname.Split('_')[0];
            }

            tmp.name = string.Format("{0}_{1}", pname, GenObjectID());

            var t = tmp.transform;
            t.position = gx.position;
            t.rotation = gx.rotation;
            t.localScale = gx.localScale;

            gameObjects.Add(t);

            return new Symbol<string>(Tokens.STR, tmp.name);
        }

        public void Remove() {
            ThrowIfEmpty();

            if (singleTransformMode) {
                Destroy(gameObjects[buffer.Peek()].gameObject);
                gameObjects.RemoveAt(buffer.Peek());
            } else {
                foreach (int i in buffer) {
                    Destroy(gameObjects[i].gameObject);
                    gameObjects.RemoveAt(i);
                }
            }

            Clear();
        }

        public void Remove(Symbol<string> prefabName) {
            int index = FindIndex(prefabName.Value);

            if (index == -1) {
                throw new GameObjectError("Could not find", prefabName.Value);
            }

            var obj = gameObjects[index];

            Destroy(obj.gameObject);
            gameObjects.RemoveAt(index);
            Clear();
        }

        public void Push(Symbol<string> name) {
            int index = FindIndex(name.Value);

            if (index == -1) {
                throw new GameObjectError("Could not find", name.Value);
            }

            buffer.Push(index);
            UpdateTargetRegisters();
            ApplyTransform();
        }

        public Symbol<string> Pop() {
            if (EmptyBuffer) {
                return MetaSymbol.Str;
            }

            int index = buffer.Pop();

            if (!EmptyBuffer) {
                UpdateTargetRegisters();
                ApplyTransform();
            }
            return new Symbol<string>(Tokens.STR, gameObjects[index].name);
        }

        public void Clear() {
            if (EmptyBuffer) {
                return;
            }

            buffer.Clear();
        }

        public void BufferMode(Symbol<string> mode) {
            if (mode.Value == "top" || mode.Value == "single") {
                singleTransformMode = true;
                return;
            }

            if (mode.Value == "buffered" || mode.Value == "multi") {
                singleTransformMode = false;
                return;
            }

            warn("Unkown buffer_mode " + mode.Value);
        }

        public Symbol<Vector3> Move(Symbol<Vector3> offset) {
            ThrowIfEmpty();

            Vector3 delta = offset.Value;

            if (singleTransformMode) {
                gameObjects[buffer.Peek()].position += delta;
            } else {
                foreach (int i in buffer) {
                    gameObjects[i].position += delta;
                }
            }

            ApplyPosition();
            return new Symbol<Vector3>(Tokens.VEC, GetTopTransform().position);
        }

        public Symbol<Vector3> Rotate(Symbol<Vector3> offset) {
            ThrowIfEmpty();

            Vector3 delta = offset.Value;
            Transform tmp;

            if (singleTransformMode) {
                tmp = gameObjects[buffer.Peek()];
                tmp.rotation = Quaternion.Euler(tmp.rotation.eulerAngles + delta);
            } else {
                foreach (int i in buffer) {
                    tmp = gameObjects[i];
                    tmp.rotation = Quaternion.Euler(tmp.rotation.eulerAngles + delta);
                }
            }

            ApplyRotation();
            return new Symbol<Vector3>(Tokens.VEC, GetTopTransform().rotation.eulerAngles);
        }

        public Symbol<Vector3> Scale(Symbol<Vector3> offset) {
            ThrowIfEmpty();

            Vector3 delta = offset.Value;

            if (singleTransformMode) {
                gameObjects[buffer.Peek()].localScale += delta;
            } else {
                foreach (int i in buffer) {
                    gameObjects[i].localScale += delta;
                }
            }

            ApplyScale();
            return new Symbol<Vector3>(Tokens.VEC, GetTopTransform().localScale);
        }

        public void Clone() {
            ThrowIfEmpty();

            int[] tmp = buffer.ToArray();

            Clear();

            if (singleTransformMode || tmp.Length == 1) {
                var cloned = Clone(new Symbol<string>(Tokens.STR, gameObjects[tmp[0]].name));
                Push(cloned);
                return;
            }

            foreach (int i in tmp) {
                var cloned = Clone(new Symbol<string>(Tokens.STR, gameObjects[i].name));
                Push(cloned);
            }
        }

        #endregion

        #region Helpers

        void UpdateTargetRegisters() {
            if (EmptyBuffer) {
                return;
            }

            mini.SetAddress(20, new Symbol<string>(Tokens.STR, GetTopTransform().name));
            mini.SetAddress(24, new Symbol<float>(Tokens.NUM, gameObjects.Count));
            mini.SetAddress(25, new Symbol<float>(Tokens.NUM, buffer.Count));
            mini.SetAddress(26, new Symbol<float>(Tokens.NUM, singleTransformMode ? 1 : 0));
        }

        void ApplyTransform() {
            ApplyPosition();
            ApplyRotation();
            ApplyScale();
        }

        void ApplyPosition() {
            mini.SetAddress(21, new Symbol<Vector3>(Tokens.VEC, GetTopTransform().position));
        }

        void ApplyRotation() {
            mini.SetAddress(22, new Symbol<Vector3>(Tokens.VEC, GetTopTransform().rotation.eulerAngles));
        }

        void ApplyScale() {
            mini.SetAddress(23, new Symbol<Vector3>(Tokens.VEC, GetTopTransform().localScale));
        }

        int FindIndex(string gameObjectName) {
            return gameObjects.FindIndex(x => x.name.ToLower() == gameObjectName.ToLower());
        }

        GameObject FindPrefab(string name) {
            foreach (var prefab in prefabs) {
                if (prefab.name.ToLower() == name.ToLower()) {
                    return prefab;
                }
            }
            return null;
        }

        Transform GetTopTransform() {
            ThrowIfEmpty();

            if (EmptyBuffer) {
                return gameObjects[0];
            }

            return gameObjects[buffer.Peek()];
        }

        void ThrowIfEmpty() {
            if (gameObjects.Count == 0) {
                throw new GameObjectError("No game objects in scene");
            }
        }

        void ThrowIfEmptyBuffer() {
            if (EmptyBuffer) {
                throw new GameObjectError("Buffer stack is empty, use 'push <str>' to reference a gameobject");
            }
        }

        string GenObjectID() {
            return UnityEngine.Random.Range(0x0, 0xFFFF).ToString("X4");
        }

        void AddFun(string method, params MetaSymbol[] args) {
            string fullname = "go." + method.ToLower();
            mini.AddFun(new Fun("go", fullname, method, args));
        }

        public void AddAll() {
            mini.AddObj(new Obj("go", this));

            AddFun("Spawn", MetaSymbol.Str);
            AddFun("Clone", MetaSymbol.Str);
            AddFun("Clone");
            AddFun("Remove", MetaSymbol.Str);
            AddFun("Remove");
            AddFun("Push", MetaSymbol.Str);
            AddFun("Pop");
            AddFun("Clear");
            AddFun("BufferMode", MetaSymbol.Str);
            AddFun("Move", MetaSymbol.Vec3);
            AddFun("Rotate", MetaSymbol.Vec3);
            AddFun("Scale", MetaSymbol.Vec3);    

            mini.Run("import $go");
        }

        #endregion
    }
}
#endif