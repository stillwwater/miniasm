using MiniASM.APIs;
using MiniASM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditor : MonoBehaviour
{
    readonly Vector3 X_OFFSET = new Vector3(1, 0, 0);
    readonly Vector3 Y_OFFSET = new Vector3(0, 1, 0);
    readonly Vector3 Z_OFFSET = new Vector3(0, 0, 1);

    public Text input;
    public GameObjectAPI go;
    public GameAPI game;
    public GameObject[] prefabs;

    Interpreter mini;

    public float incrementMultiplier = 0.5f;

    [Header("Controls")]
    [Range(2, 40)]
    public int mouseSensitivity = 20;
    [Range(1, 180)]
    public int rotateOffset = 90;

    [Header("Operations")]

    [Tooltip("(Ctrl)")]
    public bool modifier;

    [Tooltip("(Shift) multi select")]
    public bool multiSelect;

    [Tooltip("(G) grab object")]
    public bool grab;

    [Tooltip("(S) scale object | (Ctrl+S) save")]
    public bool scale;

    [Tooltip("(R) rotate object")]
    public bool rotate;

    [Tooltip("(Z) scale object")]
    public bool useZ;

    [Tooltip("(I) interact with object")]
    public bool interact;

    [Tooltip("(Alt) small increment")]
    public bool smallIncrement;

    [Tooltip("(X + ENTER) delete object")]
    public bool delete;

    void Start() {
        StandardAPI.output = Debug.Log;
        StandardAPI.input = null;
        Interpreter.debugOut = Debug.Log;
        Interpreter.errorOut = Debug.LogError;
        Interpreter.Init(32);

        mini = Interpreter.Instance;

        Bootloader.CreateReference();
        FileAPI.CreateReference();
        StandardAPI.Load(mini);

        game.AddAll(mini);
        go.Init(mini, transform, prefabs);
        go.AddAll();

        for (int i = 0; i < transform.childCount; i++) {
            go.AddTranform(transform.GetChild(i));
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            mini.RunUnchecked(input.text);
            input.text = "";
        }

        if (input.text != "")
            return;

        if (Input.GetMouseButtonDown(1)) {
            // Right mouse click
            Debug.Log(true);
            grab = false;
            SelectObject(Input.mousePosition);
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift)) {
            if (!multiSelect) {
                multiSelect = true;
                mini.Run("go.buffermode $multi");
            }

        } else if (multiSelect) {
            multiSelect = false;
            mini.Run("go.buffermode $single");
        }

        if (Input.GetKey(KeyCode.S)) {
            scale = true;
        } else if (scale) {
            scale = false;
        }

        if (Input.GetKey(KeyCode.R)) {
            rotate = true;
        } else if (rotate) {
            rotate = false;
        }

        if (Input.GetKey(KeyCode.LeftAlt)) {
            smallIncrement = true;
        } else if (smallIncrement) {
            smallIncrement = false;
        }

        if (Input.GetKeyDown(KeyCode.G)) {
            grab = !(grab);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            useZ = !(useZ);
            return;
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            delete = true;
            return;
        }

        // Delete object
        if (delete && Input.GetKeyDown(KeyCode.Return)) {
            //if (currentT != null) Destroy(currentT.gameObject);
            mini.Run("go.remove");
            delete = false;
        }

        // Move

        if (InputControl(KeyCode.LeftArrow)) {
            Control(-X_OFFSET);
            return;
        }

        if (InputControl(KeyCode.RightArrow)) {
            Control(X_OFFSET);
            return;
        }

        if (InputControl(KeyCode.UpArrow)) {
            if (useZ) {
                Control(Z_OFFSET);
            } else {
                Control(Y_OFFSET);
            }
            return;
        }

        if (InputControl(KeyCode.DownArrow)) {
            if (useZ) {
                Control(-Z_OFFSET);
            } else {
                Control(-Y_OFFSET);
            }
            return;
        }
    }

    bool InputControl(KeyCode code) {
        return Input.GetKeyDown(code);
    }


    void Control(Vector3 offset) {
        if (smallIncrement) {
            offset *= incrementMultiplier;
        }

        string fun;

        if (scale) {
            fun = "scale";
        } else if (rotate) {
            fun = "rotate";
        } else {
            fun = "move";
        }
        mini.Run(string.Format("go.{0} {1}", fun, offset.ToString().Replace(",", "")));
    }

    void SelectObject(Vector3 position) {
        Transform selected = Select(position);
        Debug.Log(selected);
        if (selected == null) return;

        mini.Run("go.push $" + selected.name);
    }

    Transform Select(Vector3 position) {
        Ray ray = Camera.main.ScreenPointToRay(position);
        //Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        RaycastHit hit;

        Physics.Raycast(ray, out hit, 100);

        return hit.transform;
    }
}
