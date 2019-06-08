using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using LiteDbExplorer.Framework.Shell;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IShellStatusBar))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class ShellStatusBarViewModel : ViewAware, IShellStatusBar
    {
        public ShellStatusBarViewModel()
        {
            LeftContentCollection = new BindableCollection<IStatusBarContent>();

            RightContentCollection = new BindableCollection<IStatusBarContent>();

            CurrentVersion = Versions.CurrentVersion;
        }

        public Version CurrentVersion { get; }

        public BindableCollection<IStatusBarContent> LeftContentCollection { get; }

        public BindableCollection<IStatusBarContent> RightContentCollection { get; }

        public IStatusBarContent ActivateContent(IStatusBarContent content, StatusBarContentLocation location)
        {
            DeactivateContent(content);

            switch (location)
            {
                case StatusBarContentLocation.Left:
                    LeftContentCollection.AddSorted(content, DisplayOrderComparer.Default);
                    break;
                case StatusBarContentLocation.Right:
                    RightContentCollection.AddSorted(content, DisplayOrderComparer.Default);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(location), location, null);
            }

            return content;
        }

        public void DeactivateContent(string instanceId)
        {
            var leftContent = LeftContentCollection
                .Where(p => !string.IsNullOrEmpty(p.ContentId) && p.ContentId.Equals(instanceId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (leftContent.Any())
            {
                LeftContentCollection.RemoveRange(leftContent);
            }

            var rightContent = RightContentCollection
                .Where(p => !string.IsNullOrEmpty(p.ContentId) && p.ContentId.Equals(instanceId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (rightContent.Any())
            {
                RightContentCollection.RemoveRange(rightContent);
            }
        }

        public void DeactivateContent(IStatusBarContent content)
        {
            var leftContent = LeftContentCollection.Where(p => p == content).ToList();
            if (leftContent.Any())
            {
                DeactivateLeftContent(leftContent);
            }

            var rightContent = RightContentCollection.Where(p => p == content).ToList();
            if (rightContent.Any())
            {
                DeactivateRightContent(rightContent);
            }
        }

        protected void DeactivateLeftContent(IList<IStatusBarContent> contentSet)
        {
            foreach (var content in contentSet)
            {
                if (content is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                LeftContentCollection.Remove(content);
            }
        }

        protected void DeactivateRightContent(IList<IStatusBarContent> contentSet)
        {
            foreach (var content in contentSet)
            {
                if (content is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                RightContentCollection.Remove(content);
            }
        }

    }
}