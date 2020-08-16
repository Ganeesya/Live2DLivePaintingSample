using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Raycasting;
using Live2D.Cubism.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UIElements.Slider;

public class PaintController : MonoBehaviour
{
    public RenderTexture paintTex;

    public Texture2D paintArea;
    
    public Texture2D overline;

    //-------------------------------
    private PaintManager manager;

    private CommandBuffer _commandBuffer;

    private CubismRenderer[] renderers;
    
    private CubismRaycaster rayCaster;

    private CubismRaycastHit[] Results;
    
    // Start is called before the first frame update
    void Start()
    {
        // Importerで切り替えが終わっているマテリアルに必要なテクスチャ情報を渡す。
        var rayCastable = GetComponentsInChildren<CubismRaycastable>();
        renderers = rayCastable.Select(raycastable => raycastable.GetComponent<CubismRenderer>()).ToArray(); 
        
        foreach (var renderer in renderers)
        {
            renderer.Material.mainTexture = renderer.MainTexture;
            renderer.Material.SetTexture("_Retatch",paintTex );
            renderer.Material.SetTexture("_OverLine", overline);

//            var meshCollider = renderer.gameObject.GetComponent<MeshCollider>();
//            meshCollider.sharedMesh = renderer.Mesh;
        }
        
        // rayCasterの取得
        rayCaster = GetComponent<CubismRaycaster>();
        if (rayCaster == null)
        {
            throw new InvalidOperationException("PaintController need CubismRayCaster Component.");
        }
        
        Results = new CubismRaycastHit[4];
        
        // managerの取得
        manager = FindObjectOfType<PaintManager>();
        if (manager == null)
        {
            throw new InvalidOperationException("PaintController need PaintManger on someone object in hierarchy.");
        }
        
        // マネージャ管理のカメラに対してコマンドバッファの登録
        _commandBuffer = new CommandBuffer();
        manager.paintingCamera.AddCommandBuffer( CameraEvent.BeforeForwardAlpha, _commandBuffer );
    }

    private void DoRayCast()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var intersectionInWorldSpace = ray.origin + ray.direction * (ray.direction.z / ray.origin.z);
        var intersectionInLocalSpace = transform.InverseTransformPoint(intersectionInWorldSpace);

//        for (int i = 0; i < _colliders.Length; i++)
//        {
//            if( !_colliders[i].Raycast(ray,out hit, float.MaxValue) ) continue;
//            
//            Debug.Log(hit.collider.gameObject.name);
//            
//            var texturePosX = hit.textureCoord.x * paintArea.width;
//            var texturePosY = hit.textureCoord.y * paintArea.height;
//            var color = paintArea.GetPixel((int)texturePosX,(int)texturePosY);
//            if( color.a < (1f/255f) ) continue; // 透明色判定
//
//            var mat = Matrix4x4.Translate(
//                new Vector3(hit.textureCoord.x - 0.5f,
//                    hit.textureCoord.y - 0.5f,
//                    0f));
//            _commandBuffer.DrawMesh(manager.penMesh,mat,manager.penObject.material);
//        }

        Vector2 result;
        for (int i = 0; i < renderers.Length; i++)
        {
            if( !RayCast(intersectionInLocalSpace, renderers[i].Mesh,out result) ) continue;
            
            var texturePosX = result.x * paintArea.width;
            var texturePosY = result.y * paintArea.height;
            var color = paintArea.GetPixel((int)texturePosX,(int)texturePosY);
            if( color.a < (1f/255f) ) continue; // 透明色判定

//            Debug.LogFormat("{0}, {1}",result.x, result.y);
            var mat = Matrix4x4.TRS(
                new Vector3(result.x * 10f - 5f,
                    result.y * 10f - 5f,
                    -500f + 9f),
                Quaternion.Euler(-90f,0f,0f),
                new Vector3(0.05f,0.05f,0.05f));
            _commandBuffer.DrawMesh(manager.penMesh,mat,manager.penObject.material);
        }
    }

    private bool RayCast(Vector3 inputPosition, Mesh mesh, out Vector2 resultUV)
    {
        var vertices = mesh.vertices;
        var indices = mesh.triangles;
        var uvs = mesh.uv;
        for (int i = 0; i < indices.Length; i += 3)
        {
            var vertexPositionA = vertices[indices[i]];
            var vertexPositionB = vertices[indices[i + 1]];
            var vertexPositionC = vertices[indices[i + 2]];

            var crossProduct1 =
                (vertexPositionB.x - vertexPositionA.x) * (inputPosition.y - vertexPositionB.y) -
                (vertexPositionB.y - vertexPositionA.y) * (inputPosition.x - vertexPositionB.x);
            var crossProduct2 =
                (vertexPositionC.x - vertexPositionB.x) * (inputPosition.y - vertexPositionC.y) -
                (vertexPositionC.y - vertexPositionB.y) * (inputPosition.x - vertexPositionC.x);
            var crossProduct3 =
                (vertexPositionA.x - vertexPositionC.x) * (inputPosition.y - vertexPositionA.y) -
                (vertexPositionA.y - vertexPositionC.y) * (inputPosition.x - vertexPositionA.x);

            if ((crossProduct1 > 0 && crossProduct2 > 0 && crossProduct3 > 0) ||
                (crossProduct1 < 0 && crossProduct2 < 0 && crossProduct3 < 0))
            {
                // todo リファクタ
                var vAB = vertexPositionB - vertexPositionA;
                var vAC = vertexPositionC - vertexPositionA;
                var vCA = vertexPositionA - vertexPositionC;
                var weightAll = ( vAB.x * vAC.y - vAB.y * vAC.x ) / 2f;
                var weightAll2 = ( vAB.x * vCA.y - vAB.y * vCA.x ) / 2f;
                
                var vAP = inputPosition - vertexPositionA;
                var weightC = ( vAB.x * vAP.y - vAB.y * vAP.x ) / 2f;
                
                var vBC = vertexPositionC - vertexPositionB;
                var vBP = inputPosition - vertexPositionB;
                var weightA = ( vBC.x * vBP.y - vBC.y * vBP.x ) / 2f;

                var vCP = inputPosition - vertexPositionC;
                var weightB = weightAll - weightA - weightC;
                var weightB2 = (vCA.x * vCP.y - vCA.y * vCP.x) / 2f;

                var uvA = uvs[indices[i + 0]];
                var uvB = uvs[indices[i + 1]];
                var uvC = uvs[indices[i + 2]];

                var weightAll3 = weightA + weightB + weightC;

                weightA /= weightAll;
                weightB /= weightAll;
                weightC /= weightAll;
                weightB2 /= weightAll3;

                resultUV = uvA * weightA + uvB * weightB + uvC * weightC;
                var checkUV = uvA * weightA + uvB * weightB2 + uvC * weightC;
                return true;
            }
        }

        resultUV = new Vector2();
        return false;
    }
    
    // Update is called once per frame
    void Update()
    {
        _commandBuffer.Clear();
        if( !Input.GetMouseButton(0) ) return;
        
        DoRayCast();
    }

}
