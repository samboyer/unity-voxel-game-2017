using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorExpandPanel : MonoBehaviour {

    public VoxelEditorMulti editor;

    public bool contractInstead = false;
    public int direction = 1;

    private void OnMouseUp()
    {
        if (contractInstead)
            editor.ContractModelDimensions(direction);
        else
            editor.ExpandModelDimensions(direction);
    }
}
