#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    internal static class GUIUtility
    {
        public static readonly EditorEvents Events = new EditorEvents();

        public static readonly string OpenManualTooltip = L10n.Tr( "Open the relevant documentation entry." );
        public const string HelpIconButtonClass = "icon-button__help-icon";
        public const string MenuIconButtonClass = "icon-button__menu-icon";
        
        private const string UIToolKitAssetsPath = "Assets/SceneSystem/Editor/UIElements/";
        private const string UxmlFilesPath = UIToolKitAssetsPath + "UXML/";
        private const string StyleSheetsPath = UIToolKitAssetsPath + "Styles/";
        private const string IconsPath = UIToolKitAssetsPath + "Icons/";

        public const string RibbonUxmlPath = UxmlFilesPath + "Ribbon.uxml";
        public const string RibbonUssPath = StyleSheetsPath + "Ribbon.uss";
        public const string RibbonDarkUssPath = StyleSheetsPath + "Ribbon_dark.uss";
        public const string RibbonLightUssPath = StyleSheetsPath + "Ribbon_light.uss";
        public const string SplitViewUssPath = StyleSheetsPath + "SplitView.uss";
        
        public const string SceneAssetIconPath = IconsPath + "SceneAssetIcon.png";
        public const string SceneTemplateIconPath = IconsPath + "SceneTemplateIcon.png";
        public const string StarIconPath = IconsPath + "star.png";
        public const string HomeIconPath = IconsPath + "home.png";
        public const string PlusIconPath = IconsPath + "plus.png";
        public const string AlignBottomIconPath = IconsPath + "align_vertically_bottom.png";
        public const string AlignTopIconPath = IconsPath + "align_vertically_top.png";
        public const string ReturnIconPath = IconsPath + "return.png";
        public const string RewindIconPath = IconsPath + "rewind.png";

        public const string SceneMapEditorWindowUxmlPath = UxmlFilesPath + "SceneMapEditorWindow.uxml";
        public const string NodeViewUxmlPath = UxmlFilesPath + "SceneNodeView.uxml";
        public const string CircleTemplateUxmlPath = UxmlFilesPath + "CircleTemplate.uxml";
        
        // Scene Control View
        public const string RightPanel = nameof( RightPanel );
        public const string RightPanelHeader = nameof( RightPanelHeader );
        
        public const string SceneControlButtons = nameof( SceneControlButtons );
        public const string OpenSceneButton = nameof( OpenSceneButton );
        public const string CloseSceneButton = nameof( CloseSceneButton );
        public const string PlaySceneButton = nameof( PlaySceneButton );
        public const string BuildSettingsButton = nameof( BuildSettingsButton );
        
        public const string FoldoutContainer = "foldout-container__content";
        public const string ScenePropertiesFoldout = nameof( ScenePropertiesFoldout );
        public const string ScenePropertiesIMGUI = nameof( ScenePropertiesIMGUI );
        
        public const string InspectorContainer = nameof( InspectorContainer );
        
        // Ribbon Tabs View
        public const string RibbonContainer = nameof( RibbonContainer );
        public const string RibbonTabs = nameof( RibbonTabs );
        public const string SettingsTab = nameof( SettingsTab );
        public const string GraphTab = nameof( GraphTab );
        public const string ExtraSettingsTab = nameof( ExtraSettingsTab );

        // Graph Tab View
        public const string GraphDetailsToggle = "Details__ToggleButton";
        public const string GraphParametersToggle = "Parameters__ToggleButton";
        public const string GraphViewHeader = "GraphView__Header";
        public const string GraphViewLabel = "GraphView__Label";
        public const string TabsContainer = nameof( TabsContainer );
        public const string AddParameterButton = nameof( AddParameterButton );
        public const string GraphParametersTab = nameof( GraphParametersTab );
        public const string ParametersListView = nameof( ParametersListView );
        public const string GraphDetailsTab = nameof( GraphDetailsTab );
        public const string ResetButtonStyleSheet = StyleSheetsPath + "ResetButton.uss";
        
        // ListView
        public const string SceneCollectionsListView = nameof( SceneCollectionsListView );
        public const string SceneReferenceListView = nameof( SceneReferenceListView );
        public const string ListItemUssPath = StyleSheetsPath + "ListItemContainer.uss";
        public static readonly string DragHandleTooltip = L10n.Tr( "Drag onto an element to swap positions." );
        public static readonly Color ListItemBorderColor = new Color32( 36, 36, 36, 255 );
        public static readonly Color ListItemBorderHoverColor = new Color32( 44, 93, 135, 255 );
        public static readonly Color ListItemColorCollection = new Color32( 87, 113, 118, 255 );
        public static readonly Color ListItemColorScene = new Color32( 170, 183, 180, 255 );

        public static readonly Color FoldoutBackgroundColor = new Color32( 51, 51, 51, 255 );
        public static readonly Color FoldoutBackgroundColor2 = new Color32( 41, 41, 41, 255 );
        public static readonly Color RibbonFoldoutColorOrange = new Color32( 205, 117, 66, 255 );
        public static readonly Color RibbonFoldoutColorRed = new Color32( 64, 40, 50, 255 );
        public static readonly Color RibbonFoldoutColorBlue = new Color32( 29, 42, 52, 255 );

        public static readonly Color ZeroAlphaColor = new Color32( 0, 0, 0, 0 );
        public static readonly Color InBuildEnabledColor = new Color32( 89, 158, 94, 255 );
        public static readonly Color InBuildDisabledColor = new Color32( 255, 193, 7, 255 );
        public static readonly Color NotInBuildColor = new Color32( 36, 36, 36, 255 );


        public static void HideElements( params VisualElement [] elements )
        {
            foreach ( var element in elements )
            {
                SetVisibility( element, false );
            }
        }

        public static void SetVisibility( VisualElement element, bool visible )
        {
            SetElementDisplay( element, visible );
        }

        private static void SetElementDisplay( VisualElement element, bool value )
        {
            if ( element == null )
                return;

            element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            element.style.visibility = value ? Visibility.Visible : Visibility.Hidden;
        }

        public static void SetValue( this ToolbarToggle tab, bool value )
        {
            tab.value = value;
        }

        public static VisualElement Clone( this VisualTreeAsset tree, VisualElement target = null,
            string styleSheetPath = null, Dictionary<string, VisualElement> slots = null )
        {
            var ret = tree.CloneTree();
            if ( !string.IsNullOrEmpty( styleSheetPath ) )
                ret.styleSheets.Add( AssetDatabase.LoadAssetAtPath<StyleSheet>( styleSheetPath ) );
            if ( target != null )
                target.Add( ret );
            ret.style.flexGrow = 1f;
            return ret;
        }

        public static void SwitchClasses( this VisualElement element, string classToAdd, string classToRemove )
        {
            if ( !element.ClassListContains( classToAdd ) )
                element.AddToClassList( classToAdd );
            element.RemoveFromClassList( classToRemove );
        }

        public static VisualElement GetFirstAncestorWhere( this VisualElement ele, Predicate<VisualElement> predicate )
        {
            for ( VisualElement parent = ele.hierarchy.parent; parent != null; parent = parent.hierarchy.parent )
            {
                if ( predicate( parent ) )
                    return parent;
            }

            return (VisualElement) null;
        }

        public static void ScaleToFit( this VisualElement ele )
        {
            ele.style.backgroundPositionX = new BackgroundPosition( BackgroundPositionKeyword.Center );
            ele.style.backgroundPositionY = new BackgroundPosition( BackgroundPositionKeyword.Center );
            ele.style.backgroundRepeat = new BackgroundRepeat( Repeat.NoRepeat, Repeat.NoRepeat );
            ele.style.backgroundSize = new BackgroundSize( BackgroundSizeType.Contain );
        }

        public static void SetBackgroundImage( this VisualElement ele, Texture2D image )
        {
            ele.style.backgroundImage = new StyleBackground( image );
        }

        public static void SetBackgroundImageColor( this VisualElement ele, Color color )
        {
            ele.style.unityBackgroundImageTintColor = color;
        }

        public static void SetBackgroundColor( this VisualElement ele, Color color )
        {
            ele.style.backgroundColor = color;
        }

        public static void SetMargins( this VisualElement ele, Margins margins )
        {
            ele.style.marginLeft = margins.Left;
            ele.style.marginBottom = margins.Bottom;
            ele.style.marginRight = margins.Right;
            ele.style.marginTop = margins.Top;
        }

        public static void SwitchToLeftClick<T>( this T manipulator ) where T : MouseManipulator
        {
            manipulator.activators.Clear();
            var manipulatorActivationFilter = new ManipulatorActivationFilter {button = MouseButton.LeftMouse};
            manipulator.activators.Add( manipulatorActivationFilter );
        }

        internal static VisualElement GetSeparatingLine()
        {
            VisualElement line = new VisualElement();
            line.style.width = new Length( 100f, LengthUnit.Percent );
            line.style.height = new Length( 1f, LengthUnit.Pixel );
            line.style.backgroundColor = new StyleColor( new Color( 63f, 63f, 63f, 0.3f ) );
            return line;
        }
    }
}

#endif