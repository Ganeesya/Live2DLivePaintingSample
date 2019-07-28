using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Live2D.Cubism.Core;
using Live2D.Cubism.Rendering;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UIElements.Slider;

public class PaintController : MonoBehaviour
{

    public CubismRenderer[] renderers;

    public RenderTexture paintTex;

    public Texture overline;

    public String[] materialKeys;

    public Material[] materialValues;

    // Start is called before the first frame update
    void Start()
    {

        renderers = GetComponentsInChildren<CubismRenderer>();
        
        const string ResourcesDirectory = "Live2D/Cubism/Materials/NewFolder";
        Dictionary<String, Material> dic = new Dictionary<String, Material>();
        for (int i = materialKeys.Length - 1; i >= 0; i--)
        {
            dic.Add(materialKeys[i],materialValues[i]);
        }
        
        foreach (var renderer in renderers)
        {
            var materialType = renderer.Material.name.Replace(" (Instance)","");
            
            renderer.Material = Instantiate(dic[materialType]);
            renderer.Material.mainTexture = renderer.MainTexture;
            renderer.Material.SetTexture("_Retatch",paintTex );
            renderer.Material.SetTexture("_OverLine", overline);
        }
    }


    // Update is called once per frame
    void Update()
    {
    }

}
