using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipes
{
    /// <summary>
    /// A pull-based stream, similar to IEnumerable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class PullPipe<T> : IPipe<T>
    {
        internal Func<T> func;
        public PullPipe(PullProvider provider, Func<T> k)
        {
            func = k;
            Provider = provider;
        }
        public IPipeProvider Provider { get; private set; }
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

    /// <summary>
    /// Pull-based provider.
    /// </summary>
    public class PullProvider : IPipeProvider
    {
        Func<T> Cast<T>(IPipe<T> pipe)
        {
            var pull = (PullPipe<T>)pipe;
            return pull.func;
        }
        public IPipe<R> Select<T, R>(IPipe<T> pipe, Func<T, R> f)
        {
            var pull = Cast(pipe);
            return new PullPipe<R>(this, () => f(pull()));
        }
        public IPipe<R> SelectMany<T, R>(IPipe<T> pipe, Func<T, IPipe<R>> f)
        {
            var pull = Cast(pipe);
            return new PullPipe<R>(this, () => Cast(f(pull()))());
        }
        public IPipe<T> Where<T>(IPipe<T> pipe, Func<T, bool> clause)
        {
            var pull = Cast(pipe);
            return new PullPipe<T>(this, () =>
            {
                T x;
                do x = pull();
                while (!clause(x));
                return x;
            });
        }
        public IPipe<S> SelectMany<T, R, S>(IPipe<T> pipe, Func<T, IPipe<R>> f, Func<T, R, S> g)
        {
            var pull = Cast(pipe);
            return new PullPipe<S>(this, () =>
            {
                var x = pull();
                var y = Cast(f(x))();
                return g(x, y);
            });
        }
    }
}
