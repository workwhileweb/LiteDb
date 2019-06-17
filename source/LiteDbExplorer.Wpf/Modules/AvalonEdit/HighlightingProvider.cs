using System.Reflection;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace LiteDbExplorer.Wpf.Modules.AvalonEdit
{
    public class HighlightingProvider
    {
        internal static IHighlightingDefinition LoadDefaultHighlighting(string name, bool isDark)
        {
            if (isDark)
            {
                name = name.Replace(@".xshd", @".dark.xshd");
            }

            if (!name.StartsWith("LiteDbExplorer"))
            {
                name = $"LiteDbExplorer.Wpf.Modules.AvalonEdit.Highlighting.{name}";
            }

            return LoadHighlightingFromAssembly(typeof(HighlightingProvider).Assembly, name);
        }

        public static IHighlightingDefinition LoadHighlightingFromResources(Assembly assembly, string resourceName, bool isDark)
        {
            if (isDark)
            {
                resourceName = resourceName.Replace(@".xshd", @".dark.xshd");
            }

            return LoadHighlightingFromAssembly(assembly, resourceName);
        }

        public static IHighlightingDefinition LoadHighlightingFromAssembly(Assembly assembly, string name)
        {
            using (var s = assembly.GetManifestResourceStream(name))
            {
                using (var reader = new XmlTextReader(s))
                {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
    }
}