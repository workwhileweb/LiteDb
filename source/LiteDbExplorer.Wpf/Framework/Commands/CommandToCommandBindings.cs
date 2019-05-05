using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PropertyChanged;

namespace LiteDbExplorer.Framework.Commands
{
    /// <summary>
    /// Command to command binding can be used to redirect one command to another. The primary use case for it
    /// is a redirection of a RoutedCommand to a different ICommand implementation (e.g. DelegateCommand, RelayCommand, etc.).
    /// Unlike the standard WPF CommandBinding, CommandToCommandBinding is a DependencyObject with DependencyProperties,
    /// meaning that the source and target commands can be easily bound to in Xaml, and the commands can be implemented in the View Model.
    /// Example usage:
    /// <![CDATA[
    /// <Window xmlns:commands="clr-namespace:LiteDbExplorer.Framework.Commands;assembly=LiteDbExplorer.Wpf" ...>
    ///     <commands:CommandToCommand.Bindings>
    ///         <commands:CommandToCommandBinding SourceCommand="Save" TargetCommand="{Binding MySaveDelegateCommand}" />
    ///     </commands:CommandToCommand.Bindings>
    /// </Window>
    /// ]]>
    /// Based on: https://gist.github.com/SlyZ/ca7b03931412115cc5fb1416180ad1b4
    /// </summary>
    public static class CommandToCommand
    {
        /// <summary>
        /// Identifies the CommandToCommand.Bindings attached property.
        /// </summary>
        public static readonly DependencyProperty BindingsProperty =
            DependencyProperty.RegisterAttached("CommandToCommandBindingsInternal", typeof(CommandToCommandBindingCollection), typeof(CommandToCommand),
                new UIPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the CommandToCommand.Bindings attached property from a given System.Windows.UIElement.
        /// </summary>
        /// <param name="uiElement">The element from which to read the property value.</param>
        /// <returns>The value of the CommandToCommand.Bindings attached property.</returns>
        public static CommandToCommandBindingCollection GetBindings(UIElement uiElement)
        {
            if (uiElement == null)
            {
                throw new ArgumentNullException(nameof(uiElement));
            }

            var bindings = (CommandToCommandBindingCollection)uiElement.GetValue(BindingsProperty);
            if (bindings == null)
            {
                bindings = new CommandToCommandBindingCollection(uiElement);
                uiElement.SetValue(BindingsProperty, bindings);
            }

            return bindings;
        }

        /// <summary>
        /// Sets the value of the CommandToCommand.Bindings attached property to a given System.Windows.UIElement.
        /// </summary>
        /// <param name="uiElement">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetBindings(UIElement uiElement, CommandToCommandBindingCollection value)
        {
            if (uiElement == null)
            {
                throw new ArgumentNullException(nameof(uiElement));
            }

            var bindings = (CommandToCommandBindingCollection)uiElement.GetValue(BindingsProperty);
            
            bindings?.Unhook();

            uiElement.SetValue(BindingsProperty, value);
        }
    }

    /// <summary>
    /// A command binding encapsulating a single command-to-command mapping.
    /// Unlike <see cref="System.Windows.Input.CommandBinding"/>, this binding is a DependencyObject,
    /// and hence can be bound to in Xaml.
    /// </summary>
    public class CommandToCommandBinding : Freezable
    {
        /// <summary>
        /// A dummy command used as a placeholder when source command is not set.
        /// </summary>
        private static readonly RoutedCommand _fallbackCommand = new RoutedCommand();

        /// <summary>
        /// Identifies the <see cref="SourceCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceCommandProperty =
                DependencyProperty.Register(nameof(SourceCommand), typeof(ICommand), typeof(CommandToCommandBinding), 
                    new FrameworkPropertyMetadata(null, SourceCommandChanged));

        /// <summary>
        /// Identifies the <see cref="TargetCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetCommandProperty =
                DependencyProperty.Register(nameof(TargetCommand), typeof(ICommand), typeof(CommandToCommandBinding), 
                    new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandToCommandBinding"/> class.
        /// </summary>
        public CommandToCommandBinding()
        {
            CommandBinding = new CommandBinding(_fallbackCommand, OnSourceExecuted, OnSourceCanExecute);
        }

        /// <summary>
        /// Gets a <see cref="System.Windows.Input.CommandBinding"/> instance representing command-to-command mapping
        /// that will be added to the <see cref="System.Windows.UIElement.CommandBindings"/> collection to enable
        /// listening to routed commands.
        /// </summary>
        public CommandBinding CommandBinding { get; private set; }

        /// <summary>
        /// Gets or sets the source command for the binding.
        /// </summary>
        public ICommand SourceCommand
        {
            get => (ICommand)GetValue(SourceCommandProperty);
            set => SetValue(SourceCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the target command for the binding.
        /// </summary>
        public ICommand TargetCommand
        {
            get => (ICommand)GetValue(TargetCommandProperty);
            set => SetValue(TargetCommandProperty, value);
        }

        private static void SourceCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c2cBinding = (CommandToCommandBinding)obj;
            c2cBinding.WritePreamble();
            c2cBinding.CommandBinding.Command = ((ICommand)e.NewValue) ?? _fallbackCommand;
            c2cBinding.WritePostscript();
        }

        private void OnSourceCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var targetCommand = TargetCommand;
            if (targetCommand != null)
            {
                e.CanExecute = targetCommand.CanExecute(e.Parameter);
                e.Handled = true;
            }
        }

        private void OnSourceExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var targetCommand = TargetCommand;
            if (targetCommand != null)
            {
                targetCommand.Execute(e.Parameter);
                e.Handled = true;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CommandToCommandBinding();
        }
    }

    /// <summary>
    /// A specialised collection used by the CommandToCommand.Bindings property as a storage for the command mappings.
    /// This collection automatically adds and removes its own CommandToCommandBindings to the
    /// <see cref="System.Windows.UIElement.CommandBindings"/> collection of the UIElement to which CommandToCommand.Bindings is attached.
    /// </summary>
    [DoNotNotify]
    public sealed class CommandToCommandBindingCollection : FreezableCollection<CommandToCommandBinding>
    {
        private readonly UIElement _uIElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandToCommandBindingCollection"/> class.
        /// </summary>
        /// <param name="uiElement">UIElement to which this collection should add command bindings.</param>
        internal CommandToCommandBindingCollection(UIElement uiElement)
        {
            _uIElement = uiElement;

            Hook();
        }

        private void Hook()
        {
            ((INotifyCollectionChanged)this).CollectionChanged += OnCollectionChanged;
        }

        internal void Unhook()
        {
            ((INotifyCollectionChanged)this).CollectionChanged -= OnCollectionChanged;

            if (_uIElement != null)
            {
                for (var i = 0; i < Count; ++i)
                {
                    _uIElement.CommandBindings.Remove(this.ElementAt(i).CommandBinding);
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_uIElement != null)
            {
                if (e.OldItems != null)
                {
                    foreach (CommandToCommandBinding c2cBinding in e.OldItems)
                    {
                        _uIElement.CommandBindings.Remove(c2cBinding.CommandBinding);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (CommandToCommandBinding c2cBinding in e.NewItems)
                    {
                        _uIElement.CommandBindings.Add(c2cBinding.CommandBinding);
                    }
                }
            }
        }
    }
}