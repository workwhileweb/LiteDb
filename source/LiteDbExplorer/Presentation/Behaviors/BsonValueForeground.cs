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
                return GetBsonValueForeground(value?.Type);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return ThemeManager.TypeHighlighting.Default;
        }

        public static SolidColorBrush GetBsonValueForeground(BsonType? bsonType)
        {
            var result = ThemeManager.TypeHighlighting.Default;
            
            if (bsonType.HasValue)
            {
                switch (bsonType.Value)
                {
                    case BsonType.String:
                        result = ThemeManager.TypeHighlighting.StringForeground;
                        break;
                    case BsonType.Boolean:
                        result = ThemeManager.TypeHighlighting.BooleanForeground;
                        break;
                    case BsonType.DateTime:
                        result = ThemeManager.TypeHighlighting.DateTimeForeground;
                        break;
                    case BsonType.Int32:
                    case BsonType.Int64:
                    case BsonType.Decimal:
                    case BsonType.Double:
                        result = ThemeManager.TypeHighlighting.NumberForeground;
                        break;
                    case BsonType.ObjectId:
                    case BsonType.Guid:
                        result = ThemeManager.TypeHighlighting.IdentityForeground;
                        break;
                    case BsonType.Document:
                    case BsonType.Array:
                    case BsonType.Binary:
                        result = ThemeManager.TypeHighlighting.ObjectForeground;
                        break;
                }
            }

            return result;
        }
    }
}