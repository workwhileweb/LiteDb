using System.ComponentModel;
using System.Runtime.CompilerServices;
using Forge.Forms.Annotations;
using JetBrains.Annotations;

namespace LiteDbExplorer.Modules.Shared
{
    [Title("{Binding Caption}")]
    [Text("{Binding Message}")]
    [Action(CANCEL_ACTION, "CANCEL", IsCancel = true, ClosesDialog = true)]
    [Action(CONFIRM_ACTION, "OK", IsDefault = true, ClosesDialog = true, Validates = true)]
    public class PasswordInput : INotifyPropertyChanged
    {
        public const string CANCEL_ACTION = @"cancel";
        public const string CONFIRM_ACTION = @"ok";

        public PasswordInput(string message, string caption = "", string predefined = "", bool rememberMe = false)
        {
            Message = message;
            Caption = caption;
            Password = predefined;
            RememberMe = rememberMe;
        }

        public string Caption { get; }

        public string Message { get; }
        
        [Password]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}