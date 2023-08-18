#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class MapSettingsTabView
    {
        private readonly VisualElement m_Root;
        private readonly SceneMapEditorWindow m_Window;
        public MapSettingsTabView( SceneMapEditorWindow window, VisualElement root )
        {
            m_Window = window;
            m_Root = root;
        }
    }
}
#endif