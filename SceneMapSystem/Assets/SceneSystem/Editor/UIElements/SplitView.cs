#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SplitView: TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }

        private VisualElement dragLineAnchor;

        public SplitView()
        {
            // Just here to change the color to of the split view drag handle
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(GUIUtility.SplitViewUssPath));
        }
    }
}

#endif