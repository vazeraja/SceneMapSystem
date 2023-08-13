#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace TNS.SceneSystem.Editor
{
    [CustomEditor( typeof( SceneMapImporter ) )]
    public class SceneMapImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            var inputActionAsset = GetAsset();

            serializedObject.Update();

            if ( inputActionAsset == null )
                EditorGUILayout.HelpBox( "The currently selected object is not an editable input action asset.",
                    MessageType.Info );

            // Button to pop up window to edit the asset.
            using ( new EditorGUI.DisabledScope( inputActionAsset == null ) ) {
                if ( GUILayout.Button( "Edit asset" ) )
                    SceneMapEditor.OpenEditor( inputActionAsset );
            }
            
            EditorGUILayout.Space();
            
            // Importer settings UI.
            var generateWrapperCodeProperty = serializedObject.FindProperty("m_GenerateWrapperCode");
            EditorGUILayout.PropertyField(generateWrapperCodeProperty, m_GenerateWrapperCodeLabel);
            if (generateWrapperCodeProperty.boolValue)
            {
                var wrapperCodePathProperty = serializedObject.FindProperty("m_WrapperCodePath");
                var wrapperClassNameProperty = serializedObject.FindProperty("m_WrapperClassName");
                var wrapperCodeNamespaceProperty = serializedObject.FindProperty("m_WrapperCodeNamespace");
            
                EditorGUILayout.BeginHorizontal();
            
                string defaultFileName = "";
                if (inputActionAsset != null)
                {
                    var assetPath = AssetDatabase.GetAssetPath(inputActionAsset);
                    defaultFileName = Path.ChangeExtension(assetPath, ".cs");
                }
            
                wrapperCodePathProperty.PropertyFieldWithDefaultText(m_WrapperCodePathLabel, defaultFileName);
            
                if (GUILayout.Button("…", EditorStyles.miniButton, GUILayout.MaxWidth(20)))
                {
                    var fileName = EditorUtility.SaveFilePanel("Location for generated C# file",
                        Path.GetDirectoryName(defaultFileName),
                        Path.GetFileName(defaultFileName), "cs");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (fileName.StartsWith(Application.dataPath))
                            fileName = "Assets/" + fileName.Substring(Application.dataPath.Length + 1);
            
                        wrapperCodePathProperty.stringValue = fileName;
                    }
                }
                EditorGUILayout.EndHorizontal();
            
                string typeName = null;
                if (inputActionAsset != null)
                    typeName = CSharpCodeHelpers.MakeTypeName(inputActionAsset?.name);
                wrapperClassNameProperty.PropertyFieldWithDefaultText(m_WrapperClassNameLabel, typeName ?? "<Class name>");
            
                if (!CSharpCodeHelpers.IsEmptyOrProperIdentifier(wrapperClassNameProperty.stringValue))
                    EditorGUILayout.HelpBox("Must be a valid C# identifier", MessageType.Error);
            
                wrapperCodeNamespaceProperty.PropertyFieldWithDefaultText(m_WrapperCodeNamespaceLabel, "<Global namespace>");
            
                if (!CSharpCodeHelpers.IsEmptyOrProperNamespaceName(wrapperCodeNamespaceProperty.stringValue))
                    EditorGUILayout.HelpBox("Must be a valid C# namespace name", MessageType.Error);
            }

            // Using ApplyRevertGUI requires calling Update and ApplyModifiedProperties around the serializedObject,
            // and will print warning messages otherwise (see warning message in ApplyRevertGUI implementation).
            serializedObject.ApplyModifiedProperties();

            ApplyRevertGUI();
        }

        private SceneMapAsset GetAsset()
        {
            return assetTarget as SceneMapAsset;
        }

        private readonly GUIContent m_GenerateWrapperCodeLabel = EditorGUIUtility.TrTextContent( "Generate C# Class" );
        private readonly GUIContent m_WrapperCodePathLabel = EditorGUIUtility.TrTextContent( "C# Class File" );
        private readonly GUIContent m_WrapperClassNameLabel = EditorGUIUtility.TrTextContent( "C# Class Name" );
        private readonly GUIContent m_WrapperCodeNamespaceLabel = EditorGUIUtility.TrTextContent( "C# Class Namespace" );
    }
}
#endif