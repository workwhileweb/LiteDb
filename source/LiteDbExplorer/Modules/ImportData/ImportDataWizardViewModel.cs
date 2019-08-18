using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Wpf.Framework;
using ReactiveUI;

namespace LiteDbExplorer.Modules.ImportData
{
    [Export(typeof(ImportDataWizardViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ImportDataWizardViewModel : Conductor<IStepsScreen>.Collection.OneActive, INavigationTarget<ImportDataOptions>
    {
        private IDisposable _activeItemObservable;
        private bool _suppressPreviousPush;

        public ImportDataWizardViewModel()
        {
            DisplayName = "Import Wizard";

            ActivateItem(IoC.Get<ImportDataHandlerSelector>());
        }

        public void Init(ImportDataOptions modelParams)
        {
        }

        public Stack<IStepsScreen> PreviousItems { get; } = new Stack<IStepsScreen>();

        public bool CanNext => ActiveItem?.HasNext ?? false;

        public bool CanPrevious => PreviousItems.Count > 1;

        public override void ActivateItem(IStepsScreen item)
        {
            _activeItemObservable?.Dispose();

            if (!_suppressPreviousPush)
            {
                PreviousItems.Push(ActiveItem);
            }
            
            base.ActivateItem(item);
            
            _activeItemObservable = item
                .ObservableForProperty(screen => screen.HasNext)
                .Subscribe(args => NotifyOfPropertyChange(nameof(CanNext)));

            InvalidateProperties();
        }

        public override void DeactivateItem(IStepsScreen item, bool close)
        {
            base.DeactivateItem(item, close);

            PreviousItems.Push(item);

            InvalidateProperties();
        }

        [UsedImplicitly]
        public void Next()
        {
            if (ActiveItem == null || !ActiveItem.Validate())
            {
                return;
            }

            if (ActiveItem?.Next() is IStepsScreen next)
            {
                ActivateItem(next);
            }
        }

        [UsedImplicitly]
        public void Previous()
        {
            var previous = PreviousItems.Pop();
            if (previous != null)
            {
                _suppressPreviousPush = true;
                ActivateItem(previous);
                _suppressPreviousPush = false;
            }

            InvalidateProperties();
        }

        private void InvalidateProperties()
        {
            NotifyOfPropertyChange(nameof(CanNext));
            NotifyOfPropertyChange(nameof(CanPrevious));
        }
    }

}