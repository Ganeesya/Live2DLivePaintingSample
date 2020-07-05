using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class clickMove : MonoBehaviour
{
    private Boolean isDrag = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            var newPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
//            newPos.z = 0;
//            transform.position = newPos;
//        }
    }

    private void OnMouseDrag()
    {
        var newPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        newPos.z = 0;
        transform.position = newPos;
    }
}
