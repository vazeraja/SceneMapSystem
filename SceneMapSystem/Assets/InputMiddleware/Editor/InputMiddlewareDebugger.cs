using System;
using System.Collections.Generic;
using TNS.InputMiddleware;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;

namespace TNS.InputMiddleware.Editor
{
    public class InputMiddlewareDebugger : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset = default;
        [SerializeField] private InputMiddlewareSet inputMiddlewareSet = default;

        private SerializedObject so;

        //private IGlobalSystemChoice selectedSystem;
        private ToolbarMenu systemSelect;
        //private List<IGlobalSystemChoice> systemChoices;

        private ScrollView leftPane;
        private VisualElement rightPane;
        private MultiColumnListView multiColumnListView;


        [MenuItem( "Tools/Input System/Input Middleware Debugger" )]
        public static void ShowDebuggerWindow()
        {
            // Dock window with inspector
            var inspectorWindowType = typeof( UnityEditor.Editor ).Assembly.GetType( "UnityEditor.InspectorWindow" );
            var window = CreateWindow<InputMiddlewareDebugger>( typeof( InputMiddlewareDebugger ), inspectorWindowType );

            window.titleContent = new GUIContent( "Systems Debugger" );
            window.Focus();
        }

        private void OnEnable()
        {
            EditorExtensions.EnteredEditMode += CreateGUI;
        }

        private void OnDisable()
        {
            EditorExtensions.EnteredEditMode -= CreateGUI;
        }

        public void CreateGUI()
        {
            // Clear the root VisualElement before cloning the VisualTreeAsset into it
            rootVisualElement.Clear();

            visualTreeAsset.CloneTree( rootVisualElement );
            systemSelect = rootVisualElement.Q<ToolbarMenu>( "toolbar-menu" );
            leftPane = rootVisualElement.Q<ScrollView>( "left-scroll-view" );
            rightPane = rootVisualElement.Q<VisualElement>( "right-pane" );
            multiColumnListView = rootVisualElement.Q<MultiColumnListView>( "MultiColumnListView" );

            // RefreshSystemSelectMenu();
            SetupMiddlewareListView();
        }

        private void SetupMiddlewareListView()
        {
            var middleware = inputMiddlewareSet.GetSortedSet();
            multiColumnListView.itemsSource = middleware;

            var cols = multiColumnListView.columns;
            cols["Name"].makeCell = () => new Label();
            cols["Active"].makeCell = () => new Label();
            cols["Priority"].makeCell = () => new Label();

            cols["Name"].bindCell = ( element, i ) =>
            {
                var label = element as Label;
                label!.text = $"  {middleware[i].Name}  ";
            };
            cols["Active"].bindCell = ( element, i ) => { ( element as Label )!.text = middleware[i].Active ? "  Active  " : "  Inactive  "; };
            cols["Priority"].bindCell = ( element, i ) => { ( element as Label )!.text = $"  {middleware[i].Priority.ToString()}  "; };
        }

        // private void RefreshSystemSelectMenu()
        // {
        //     systemSelect.name = "systemChoicesPopup";
        //     systemSelect.variant = ToolbarMenu.Variant.Popup;
        //     systemChoices = new List<IGlobalSystemChoice>();
        //
        //     var systems = Resources.Load<GameObject>( "App" ).GetComponents<IGlobalSystem>();
        //     foreach ( var system in systems ) {
        //         systemChoices.Add( new GlobalSystemChoice( system ) );
        //     }
        //
        //     DropdownMenu menu = systemSelect.menu;
        //     foreach ( IGlobalSystemChoice systemChoice in systemChoices ) {
        //         menu.AppendAction(
        //             systemChoice.ToString(),
        //             ( action ) =>
        //             {
        //                 selectedSystem = (GlobalSystemChoice)action.userData;
        //                 Debug.Log( selectedSystem.System.SystemName );
        //             },
        //             DropdownMenuAction.AlwaysEnabled,
        //             systemChoice
        //         );
        //     }
        // }

        public Vector2 GetVisualElementCenter( VisualElement visualElement )
        {
            // Get the layout information for the VisualElement
            var layout = visualElement.layout;

            // Calculate the center point of the VisualElement by adding half of its width and height to its position
            var centerPoint = new Vector2( layout.x + layout.width / 2, layout.y + layout.height / 2 );

            return centerPoint;
        }
    }
}