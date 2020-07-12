using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 描画コントロールに必要なヒエラルキー上のオブジェクトを入れてPaintControllerで参照するためのCompo
 */
public class PaintManager : MonoBehaviour
{
    public MeshRenderer penObject;

    public Camera paintingCamera;

    public ColorPickerTriangle colorPicker;

    [NonSerialized]
    public Mesh penMesh;
    // Start is called before the first frame update
    void Start()
    {
        penMesh = penObject.gameObject.GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
