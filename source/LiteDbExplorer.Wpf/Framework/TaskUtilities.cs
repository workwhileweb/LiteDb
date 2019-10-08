using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LiteDbExplorer.Framework
{
    public static class TaskUtilities
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void FireAndForgetSafeAsync(this Task task, IErrorHandler handler = null)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            { 
                await task;
            }
            catch (Exception ex)
            {
                handler?.HandleError(ex);
            }
        }

        public static void InvokeOnMainThread(this MethodInfo mi, object target, params object[] parameters)
        {
            void ThreadMain()
            {
                mi.Invoke(target, parameters);
            }

            Application.Current.Dispatcher.Invoke(ThreadMain, DispatcherPriority.Normal);

        }

        public static void InvokeOnNewThread(this MethodInfo mi, object target, params object[] parameters)
        {
            void ThreadMain()
            {
                mi.Invoke(target, parameters);
            }

            new Thread(ThreadMain).Start();
        }
    }
}