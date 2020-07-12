using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Editor.Importers;
using UnityEditor;
using UnityEngine;

internal static class MotionLegacyChangImporter
{
    [InitializeOnLoadMethod]
    private static void RegisterMotionImporter()
    {
        CubismImporter.OnDidImportMotion += OnFadeMotionImport;
    }

    private static void OnFadeMotionImport(CubismMotion3JsonImporter importer, AnimationClip animationClip)
    {
        animationClip.legacy = true;
    }
}
