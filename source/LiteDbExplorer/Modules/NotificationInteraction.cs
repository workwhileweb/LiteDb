using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Enterwell.Clients.Wpf.Notifications;

namespace LiteDbExplorer.Modules
{
    public enum UINotificationType
    {
        None,
        Info,
        Warning,
        Error
    }

    public class NotificationInteraction
    {
        private static readonly Lazy<NotificationInteraction> _instance =
            new Lazy<NotificationInteraction>(() => new NotificationInteraction());

        private readonly NotificationMessageManager _manager;

        private NotificationInteraction()
        {
            _manager = new NotificationMessageManager();
        }

        public static INotificationMessageManager Manager => _instance.Value._manager;

        public static NotificationMessageBuilder Default()
        {
            var builder = Manager
                .CreateMessage()
                .Animates(true);

            return builder;
        }

        public static INotificationMessage Alert(string message, UINotificationType type = UINotificationType.None, Action closeAction = null)
        {
            return Default()
                .HasMessage(message)
                .Dismiss().WithButton("Close", button => { closeAction?.Invoke(); })
                .WithBadgeType(type)
                .Queue();
        }

        public static Task<string> ActionsSheet(string message, IDictionary<string, string> actions)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            var builder = Default()
                .HasMessage(message);

            foreach (var action in actions)
            {
                if (action.Key.StartsWith(@"-"))
                {
                    builder = builder.WithButton(action.Value, button => { taskCompletionSource.TrySetResult(action.Key); });
                }
                else
                {
                    builder = builder
                        .Dismiss().WithButton(action.Value, button => { taskCompletionSource.TrySetResult(action.Key); });
                }
            }
            
            builder.Queue();

            return taskCompletionSource.Task;
        }
    }

    public static class NotificationInteractionExtensions
    {
        public static NotificationMessageBuilder WithBadgeType(this NotificationMessageBuilder builder, UINotificationType type)
        {
            switch (type)
            {
                case UINotificationType.Info:
                    builder.SetBadge("Info");
                    builder.Message.BadgeAccentBrush =
                        new SolidColorBrush(Color.FromRgb(2,160,229));
                    break;
                case UINotificationType.Warning:
                    builder.SetBadge("Warn");
                    builder.Message.BadgeAccentBrush =
                        new SolidColorBrush(Color.FromRgb(224,160,48));
                    break;
                case UINotificationType.Error:
                    builder.SetBadge("Error");
                    builder.Message.BadgeAccentBrush =
                        new SolidColorBrush(Color.FromRgb(232,13,0));
                    break;
            }

            return builder;
        }
    }

    public class NotificationInteractionProxy : Freezable
    {
        public INotificationMessageManager Manager => NotificationInteraction.Manager;

        protected override Freezable CreateInstanceCore()
        {
            return new NotificationInteractionProxy();
        }
    }
}