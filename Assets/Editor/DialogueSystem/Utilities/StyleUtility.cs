using UnityEditor;
using UnityEngine.UIElements;

namespace Mert.DialogueSystem.Utilities
{
    public static class StyleUtility 
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string styleSheetName in styleSheetNames)
            {
                StyleSheet styleSheet = EditorGUIUtility.Load(styleSheetName) as StyleSheet;
                element.styleSheets.Add(styleSheet);
            }

            return element;
        }
    }
}
