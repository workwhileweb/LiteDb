using System.ComponentModel.DataAnnotations;

namespace LiteDbExplorer.Core
{
    public enum DatabaseFileMode
    {
        [Display(Name = "Shared")]
        Shared,

        [Display(Name = "Exclusive (recommended)")]
        Exclusive
    }
}