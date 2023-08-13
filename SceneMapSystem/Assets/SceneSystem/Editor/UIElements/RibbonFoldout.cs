#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class RibbonFoldout : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<RibbonFoldout, UxmlTraits> { }
        
        private readonly VisualElement m_FoldoutContainer;
        private readonly Foldout m_Foldout;
        public readonly ScrollView m_ScrollView;
        private readonly IMGUIContainer m_IMGUIContainer;

        public void SetRibbonColor(Color color)
        {
            m_FoldoutContainer.style.borderLeftColor = color;
        }

        public void SetExpandedState( bool expanded )
        {
            m_Foldout.value = expanded;
        }
        
        public void SetLabel( string text )
        {
            m_Foldout.text = text;
        }

        public void CreateGUI(Action action)
        {
            m_IMGUIContainer.onGUIHandler = action;
        }

        public RibbonFoldout()
        {
            name = "RibbonFoldoutCustomControl";
            viewDataKey = "RibbonFoldoutCustomControl";
            
            m_FoldoutContainer = new VisualElement();
            m_FoldoutContainer.style.marginLeft = 5f;
            m_FoldoutContainer.style.marginRight = 5f;
            m_FoldoutContainer.style.marginTop = 10f;
            m_FoldoutContainer.style.backgroundColor = GUIUtility.FoldoutBackgroundColor2;
            m_FoldoutContainer.style.borderLeftWidth = 5f;
            m_FoldoutContainer.style.borderLeftColor = GUIUtility.RibbonFoldoutColorOrange;

            m_Foldout = new Foldout();
            m_Foldout.name = "RibbonFoldout";
            m_Foldout.viewDataKey = "RibbonFoldout_Foldout";
            m_Foldout.style.marginLeft = 5f;
            m_Foldout.style.marginRight = 5f;
            m_Foldout.style.marginTop = 5f;
            m_Foldout.style.marginBottom = 5f;
            m_Foldout.style.backgroundColor = GUIUtility.FoldoutBackgroundColor2;

            m_ScrollView = new ScrollView();
            m_ScrollView.style.marginLeft = -20f;
            m_ScrollView.style.marginRight = 5f;
            m_ScrollView.style.backgroundColor = GUIUtility.FoldoutBackgroundColor2;

            m_IMGUIContainer = new IMGUIContainer();
            m_IMGUIContainer.name = "RibbonFoldoutIMGUI";
            m_IMGUIContainer.style.marginLeft = 15f;
            m_IMGUIContainer.style.marginRight = 5f;
            m_IMGUIContainer.style.marginTop = 5f;
            m_IMGUIContainer.style.backgroundColor = GUIUtility.FoldoutBackgroundColor2;
            
            m_ScrollView.Add(m_IMGUIContainer);
            m_Foldout.Add(m_ScrollView);
            m_FoldoutContainer.Add(m_Foldout);
            
            hierarchy.Add(m_FoldoutContainer);
        }
    }
}

#endif