#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class ItemWrapperBase
    {
        public Type ItemType;
    }

    public class ItemWrapper<T> : ItemWrapperBase
    {
        public int Index;
        public int ID;
        public T Data;
    }

    public struct Margins
    {
        public float Left, Right, Bottom, Top;

        public Margins( float left, float right, float bottom, float top )
        {
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }
    }

    public class UnityStyleListView<T>
    {
        protected readonly ListView ListView;
        private readonly EditorWindow m_Window;
        private List<T> m_Items;

        private Color m_ItemColor;
        private Texture2D m_ItemIcon;
        private Margins m_ItemIconMargins;
        private Color m_IndicatorColor;
        private Texture2D m_IndicatorIcon;
        private Vector2 m_IndicatorScale;

        public event Action<VisualElement> onBindItem;
        public event Action<VisualElement> itemRightClicked;
        public event Action<VisualElement> itemLeftClicked;
        public event Action<VisualElement> itemDoubleClicked;
        public event Action<VisualElement, string> itemLabelChanged;

        public int SelectedItemIndex => ListView.selectedIndex;
        public T SelectedItem => (T) ListView.selectedItem;
        public IEnumerable<object> SelectedItems => ListView.selectedItems;

        public event Action<IEnumerable<object>> selectionChanged
        {
            add => ListView.selectionChanged += value;
            remove => ListView.selectionChanged -= value;
        }

        protected UnityStyleListView( EditorWindow window, ListView listView )
        {
            ListView = listView;
            m_Window = window;

            m_Items = new List<T>();

            m_ItemColor = GUIUtility.ListItemColorCollection;
            m_IndicatorColor = GUIUtility.InBuildEnabledColor;
            m_IndicatorIcon = AssetDatabase.LoadAssetAtPath<Texture2D>( GUIUtility.StarIconPath );
            m_IndicatorScale = Vector2.one;

            ListView.makeItem = MakeListViewItem;
            ListView.bindItem = BindListViewItem;
            ListView.selectionType = SelectionType.Single;
        }

        protected void SetItemIconMargins( float left, float right, float bottom, float top )
        {
            m_ItemIconMargins.Left = left;
            m_ItemIconMargins.Right = right;
            m_ItemIconMargins.Bottom = bottom;
            m_ItemIconMargins.Top = top;
            ListView.Rebuild();
        }

        protected void SetItemColor( Color color )
        {
            m_ItemColor = color;
            ListView.Rebuild();
        }

        protected void SetItemIcon( Texture2D icon )
        {
            m_ItemIcon = icon;
            ListView.Rebuild();
        }

        protected void HideIndicator()
        {
            m_IndicatorColor = GUIUtility.ZeroAlphaColor;
            ListView.Rebuild();
        }

        protected void SetIndicatorColor( Color color )
        {
            m_IndicatorColor = color;
            ListView.Rebuild();
        }

        protected void SetIndicatorIcon( Texture2D icon )
        {
            m_IndicatorIcon = icon;
            ListView.Rebuild();
        }

        protected void SetIndicatorSize( Vector2 scale )
        {
            this.m_IndicatorScale = scale;
            ListView.Rebuild();
        }

        public void Rebuild( List<T> itemsSource )
        {
            m_Items = itemsSource;
            ListView.itemsSource = itemsSource;

            ListView.Rebuild();
        }

        public bool TrySelectIndex( int index )
        {
            if ( index < 0 ) return false;
            if ( index > m_Items.Count - 1 ) return false;
            if ( m_Items.Count <= 0 ) return false;

            ListView.SetSelection( index );
            return true;
        }

        public bool TrySelectItem( string name )
        {
            for ( var index = 0; index < m_Items.Count; index++ )
            {
                var collection = m_Items[index];

                if ( collection.ToString() == name )
                {
                    ListView.SetSelection( index );
                    return true;
                }
            }

            return false;
        }

        protected void DisableFields( VisualElement element )
        {
            var floatField = (FloatField) element.Q( "floatField" );
            var intField = (IntegerField) element.Q( "integerField" );
            var boolField = (Toggle) element.Q( "boolField" );
            var triggerField = (RadioButton) element.Q( "triggerField" );

            floatField.style.display = DisplayStyle.None;
            intField.style.display = DisplayStyle.None;
            boolField.style.display = DisplayStyle.None;
            triggerField.style.display = DisplayStyle.None;
        }

        protected virtual void BindListViewItem( VisualElement element, int i )
        {
            var label = (Label) element.Q( "", "list-item__label" );
            var data = (T) ListView.viewController.GetItemForIndex( i );
            var id = ListView.viewController.GetIdForIndex( i );

            var itemWrapper = new ItemWrapper<T>();
            itemWrapper.ItemType = typeof( T );
            itemWrapper.Data = data;
            itemWrapper.ID = id;
            itemWrapper.Index = i;

            element.userData = itemWrapper;
            label.text = data.ToString();

            onBindItem?.Invoke( element );
        }

        protected virtual VisualElement MakeListViewItem()
        {
            var container = new VisualElement();
            container.AddToClassList( "list-item__container" );
            container.styleSheets.Add( AssetDatabase.LoadAssetAtPath<StyleSheet>( GUIUtility.ListItemUssPath ) );
            container.AddManipulator( new DragAndDrop( (SceneMapEditor) m_Window ) );

            var rightClickEvent = new Clickable( evt => itemRightClicked?.Invoke( (VisualElement) evt.currentTarget ) );
            rightClickEvent.activators.Clear();
            rightClickEvent.activators.Add( new ManipulatorActivationFilter {button = MouseButton.RightMouse} );
            container.AddManipulator( rightClickEvent );

            var doubleLeftClickEvent = new Clickable( evt => itemDoubleClicked?.Invoke( (VisualElement) evt.currentTarget ) );
            doubleLeftClickEvent.activators.Clear();
            doubleLeftClickEvent.activators.Add( new ManipulatorActivationFilter {button = MouseButton.LeftMouse, clickCount = 2} );
            container.AddManipulator( doubleLeftClickEvent );

            var leftClickEvent = new Clickable( evt => itemLeftClicked?.Invoke( (VisualElement) evt.currentTarget ) );
            container.AddManipulator( leftClickEvent );

            var dragHandle = new VisualElement();
            dragHandle.AddToClassList( "drag-handle__element" );
            dragHandle.style.width = 15f;
            dragHandle.style.backgroundColor = m_ItemColor;
            dragHandle.tooltip = GUIUtility.DragHandleTooltip;

            var dragHandleIcon = new VisualElement();
            dragHandleIcon.pickingMode = PickingMode.Ignore;
            dragHandleIcon.style.backgroundImage = m_ItemIcon;
            dragHandleIcon.style.flexGrow = 1;
            dragHandleIcon.SetMargins( m_ItemIconMargins );
            dragHandle.Add( dragHandleIcon );

            var itemLabel = new Label();
            itemLabel.AddToClassList( "list-item__label" );
            itemLabel.style.paddingLeft = 15f;
            itemLabel.style.unityTextAlign = new StyleEnum<TextAnchor>( TextAnchor.MiddleLeft );

            var titleTextField = new TextField {isDelayed = true};
            titleTextField.style.display = DisplayStyle.None;

            itemLabel.RegisterCallback<MouseDownEvent>( e =>
            {
                if ( e.clickCount != 2 || e.button != (int) MouseButton.LeftMouse ) return;

                titleTextField.style.display = DisplayStyle.Flex;
                itemLabel.style.display = DisplayStyle.None;
                titleTextField.focusable = true;

                titleTextField.SetValueWithoutNotify( itemLabel.text );
                titleTextField.Focus();
                titleTextField.SelectAll();
            } );

            titleTextField.RegisterValueChangedCallback( e => CloseAndSaveTitleEditor( container, itemLabel, titleTextField, e.newValue ) );

            titleTextField.RegisterCallback<MouseDownEvent>( e =>
            {
                if ( e.clickCount == 2 && e.button == (int) MouseButton.LeftMouse )
                    CloseAndSaveTitleEditor( container, itemLabel, titleTextField, titleTextField.value );
            } );

            titleTextField.RegisterCallback<FocusOutEvent>
            (
                _ => CloseAndSaveTitleEditor( container, itemLabel, titleTextField, titleTextField.value )
            );

            void CloseAndSaveTitleEditor( VisualElement c, Label label, TextField textField, string newTitle )
            {
                label.text = newTitle;

                // hide title TextBox
                textField.style.display = DisplayStyle.None;
                label.style.display = DisplayStyle.Flex;
                textField.focusable = false;

                itemLabelChanged?.Invoke( c, newTitle );
            }

            var indicatorContainer = new VisualElement();
            indicatorContainer.AddToClassList("list-item__indicator");
            indicatorContainer.style.flexGrow = 1;
            indicatorContainer.style.flexDirection = new StyleEnum<FlexDirection>( FlexDirection.RowReverse );

            var floatField = new FloatField();
            var integerField = new IntegerField();
            var boolField = new Toggle();
            var triggerField = new RadioButton();
            floatField.name = "floatField";
            integerField.name = "integerField";
            boolField.name = "boolField";
            triggerField.name = "triggerField";

            indicatorContainer.Add( floatField );
            indicatorContainer.Add( integerField );
            indicatorContainer.Add( boolField );
            indicatorContainer.Add( triggerField );

            var indicator = new VisualElement();
            indicator.AddToClassList("list-item__indicator-icon");
            indicator.style.marginRight = 2f;
            indicator.style.width = 25f;
            indicator.style.unityTextAlign = new StyleEnum<TextAnchor>( TextAnchor.MiddleLeft );
            indicator.style.scale = new StyleScale( m_IndicatorScale );
            indicator.ScaleToFit();
            indicator.SetBackgroundImage( m_IndicatorIcon );
            indicator.SetBackgroundImageColor( m_IndicatorColor );
            indicatorContainer.Add( indicator );

            container.Add( dragHandle );
            container.Add( titleTextField );
            container.Add( itemLabel );
            container.Add( indicatorContainer );

            return container;
        }
    }
}
#endif