using System;
using System.IO;
using System.Windows.Media;
using LiteDbExplorer.Wpf.Framework;

namespace LiteDbExplorer.Framework
{
    public enum GroupDisplayVisibility
    {
        Auto = 0,
        AlwaysVisible = 10
    }

    public interface IDocument : ILayoutItem
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

    public interface IDocument<in T> : IReferenceNode, IDocument where T : IReferenceNode
    {
        void Init(T item);
    }
}