using System;
using System.Windows.Media;
using LiteDB;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public static class BsonValueForeground
    {
        public static SolidColorBrush GetBsonValueForeground(BsonValue value)
        {
            try
            {
                if (value != null)
                {
                    if (value.IsDocument || value.IsArray || value.IsBinary)
                    {
                        return ThemeManager.TypeHighlighting.ObjectForeground;
                    }
                
                    if (value.IsInt32 || value.IsInt64 || value.IsDecimal || value.IsDouble)
                    {
                        return ThemeManager.TypeHighlighting.NumberForeground;
                    }

                    if (value.IsBoolean)
                    {
                        return ThemeManager.TypeHighlighting.BooleanForeground;
                    }

                    if (value.IsDateTime)
                    {
                        return ThemeManager.TypeHighlighting.DateTimeForeground;
                    }

                    if (value.IsString)
                    {
                        return ThemeManager.TypeHighlighting.StringForeground;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return ThemeManager.TypeHighlighting.Default;
        }

    }
}