using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Live2D.Cubism.Core;
using Live2D.Cubism.Rendering;
using UnityEngine;

/**
 * 描画コントロールに必要なヒエラルキー上のオブジェクトを入れてPaintControllerで参照するためのCompo
 */
public class PaintManager : MonoBehaviour
{
    public MeshRenderer penObject;

    public Camera paintingCamera;

    public ColorPickerTriangle colorPicker;

    public CubismModel saveTarget;

    [NonSerialized]
    public Mesh penMesh;
    // Start is called before the first frame update
    void Start()
    {
        penMesh = penObject.gameObject.GetComponent<MeshFilter>().mesh;
    }

    public void ClearBuffer()
    {
        RenderTexture current = RenderTexture.active; 
        RenderTexture.active = paintingCamera.targetTexture;
        GL.Clear(true, true, Color.clear );
        RenderTexture.active = current;
    }

    public void OutputPaintedTexture()
    {
        String path;
#if UNITY_EDITOR
        path = Directory.GetCurrentDirectory();//Editor上では普通にカレントディレクトリを確認
#else
        FilePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');//EXEを実行したカレントディレクトリ (ショートカット等でカレントディレクトリが変わるのでこの方式で)
#endif
        path = path + "\\OutputTest\\";
        Directory.CreateDirectory(path);
        Debug.Log(path);
        OutputPaintedTexture(path);
    }

    private void OutputPaintedTexture( String outPath )
    {
        var sampleMaterials = saveTarget.Drawables.Select( it => it.GetComponent<CubismRenderer>() );
        var samplePaintedCubismRenderer = sampleMaterials.Where(it => Regex.IsMatch(it.Material.name, "Rp\\s*\\(Instance\\)$")).First();
        var mainTexture = samplePaintedCubismRenderer.Material.mainTexture;
        
        RenderTexture drawingBuffer = new RenderTexture( mainTexture.width, mainTexture.height, 1 );
        
        Material bufferCreater = new Material(samplePaintedCubismRenderer.Material);
        bufferCreater.SetInt("isPremultiedAlphaOut", 0);
        
        Graphics.Blit(mainTexture, drawingBuffer, bufferCreater);
        
        var currentActive = RenderTexture.active;
        
        Texture2D writeOut = new Texture2D( mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false );
        RenderTexture.active = drawingBuffer;
        writeOut.ReadPixels(new Rect(0f,0f, mainTexture.width, mainTexture.height),0,0 );
        writeOut.Apply();

        RenderTexture.active = currentActive;

        var pngData = ImageConversion.EncodeToPNG(writeOut);
        File.WriteAllBytes(outPath + mainTexture.name + ".png", pngData );
        
        Destroy( writeOut );
        Destroy( drawingBuffer );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
