using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipes
{
    /// <summary>
    /// A pull-based stream, similar to IEnumerable.
    /// </summary>
    /// <ypeparam name="T"></typeparam>
    class PullPipe<T> : IPipe<T>
    {
        internal Func<T> func;
        public PullPipe(Func<T> k)
        {
            func = k;
        }
        Func<T0> Cast<T0>(IPipe<T0> pipe)
        {
            var pull = (PullPipe<T0>)pipe;
            return pull.func;
        }
        public IPipe<R> Select<R>(Func<T, R> f)
        {
            return new PullPipe<R>(() => f(func()));
        }
        public IPipe<R> SelectMany<R>(Func<T, IPipe<R>> f)
        {
            return new PullPipe<R>(() => Cast(f(func()))());
        }
        public IPipe<T> Where( Func<T, bool> clause)
        {
            return new PullPipe<T>(() =>
            {
                T x;
                do x = func();
                while (!clause(x));
                return x;
            });
        }
        public IPipe<S> SelectMany<R, S>(Func<T, IPipe<R>> f, Func<T, R, S> g)
        {
            return new PullPipe<S>(() =>
            {
                var x = func();
                var y = Cast(f(x))();
                return g(x, y);
            });
        }
        public void Execute(Action<T> accept)
        {
            try
            {
                while (true) accept(func());
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }
    }
}
