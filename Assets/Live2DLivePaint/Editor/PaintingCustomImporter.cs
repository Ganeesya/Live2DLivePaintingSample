using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using Live2D.Cubism.Editor.Importers;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Raycasting;
using Live2D.Cubism.Rendering;
using UnityEditor;
using UnityEngine;

// todo namespace set
internal static class PaintingCustomImporter
{
    [InitializeOnLoadMethod]
    private static void RegisterModelImporter()
    {
        CubismImporter.OnDidImportModel += OnModelImport;
    }

    private static void OnModelImport(CubismModel3JsonImporter sender, CubismModel model)
    {
        // drawableのテクスチャ番号からマテリアルの入れ替えとRaycastableの付与を行う。
        var drawables = model.Drawables;

        foreach (var drawable in drawables)
        {
            if( drawable.TextureIndex != 0 ) continue;

            var renderer = drawable.GetComponent<CubismRenderer>();
            switch (renderer.Material.name.Replace(" (Instance)",""))
            {
                case "Unlit" :
                    renderer.Material =
                        Resources.Load<Material>("compositMats/UnlitRp");
                    break;
                case "UnlitAdditiveMasked" :
                    renderer.Material =
                        Resources.Load<Material>("compositMats/UnlitAdditiveMaskedRp");
                    break;
                case "UnlitAdditive" :
                    renderer.Material =
                        Resources.Load<Material>("compositMats/UnlitAdditiveRp");
                    break;
                case "UnlitMasked" :
                    renderer.Material =
                        Resources.Load<Material>("compositMats/UnlitMaskedRp");
                    break;
                case "UnlitMultiplyMasked" :
                    renderer.Material =
                        Resources.Load<Material>("compositMats/UnlitMultiplyMaskedRp");
                    break;
                case "UnlitMultiply" :
                    renderer.Material =
                        Resources.Load<Material>("compositMats/UnlitMultiplyRp");
                    break;
                default: Debug.LogWarning("unknown mat Type!!");
                    break;
            }

            var rayCastable = drawable.GetOrAddComponent(typeof(CubismRaycastable)) as CubismRaycastable;
            if (rayCastable != null)
            {
                rayCastable.Precision = CubismRaycastablePrecision.Triangles;
            }
        }
        
        // 不要なAnimatiorを削除
        var animator = model.GetComponent<Animator>();
        if (animator != null)
        {
            Object.DestroyImmediate(animator);
        }

        // modelに対して必要なComponentを付与
        model.GetOrAddComponent(typeof(Animation));
        model.GetOrAddComponent(typeof(SlidableMotionController));
        model.GetOrAddComponent(typeof(CubismUpdateController));
        model.GetOrAddComponent(typeof(PaintController));
        model.GetOrAddComponent(typeof(CubismRaycaster));
    }
}
