using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;

namespace LiteDbExplorer.Core
{
    public class CountingSubject<T>
    {
        private readonly ISubject<T> _internalSubject;
        private int _subscriberCount;

        public CountingSubject()
            : this(new Subject<T>())
        {
        }

        public CountingSubject(ISubject<T> internalSubject)
        {
            _internalSubject = internalSubject;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Interlocked.Increment(ref _subscriberCount);

            return new CompositeDisposable(
                this._internalSubject.Subscribe(observer),
                Disposable.Create(() => Interlocked.Decrement(ref _subscriberCount))
            );
        }

        public int SubscriberCount => _subscriberCount;
    }
}