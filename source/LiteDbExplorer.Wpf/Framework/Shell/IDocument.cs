using System;
using System.Windows.Media;

namespace LiteDbExplorer.Wpf.Framework.Shell
{
    public enum GroupDisplayVisibility
    {
        Auto = 0,
        AlwaysVisible = 10
    }

    public interface IDocument : ILayoutItem, IReferenceId
    {
        string GroupDisplayName { get; set; }

        bool GroupDisplayNameIsVisible { get; set; }

        GroupDisplayVisibility GroupDisplayVisibility { get; set; }

        SolidColorBrush GroupDisplayBackground { get; set; }

        string GroupId { get; set; }

        void UpdateGroupDisplay();

        event EventHandler<EventArgs> UpdateGroupDisplayRequest;
    }

    public interface IStartupDocument : IDocument
    {

    }

    public interface IDocument<in T> : IDocument where T : IReferenceId
    {
        void Init(T item);
    }
}