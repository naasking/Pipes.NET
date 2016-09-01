using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipes
{
    /// <summary>
    /// A push stream based on continuations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class PushPipe<T> : IPipe<T>
    {
        internal Action<Action<T>> cont;
        public PushPipe(IPipeProvider provider, Action<Action<T>> k)
        {
            cont = k;
            Provider = provider;
        }
        public IPipeProvider Provider { get; private set; }
        public Action Compile(Action<T> accept)
        {
            return () => cont(accept);
        }
    }

    /// <summary>
    /// The continuation-based push stream provider.
    /// </summary>
    public class PushProvider : IPipeProvider
    {
        Action<Action<T>> Cast<T>(IPipe<T> pipe)
        {
            var push = (PushPipe<T>)pipe;
            return push.cont;
        }
        public IPipe<R> Select<T, R>(IPipe<T> pipe, Func<T, R> f)
        {
            var push = Cast(pipe);
            return new PushPipe<R>(this, k => push(i => k(f(i))));
        }
        public IPipe<R> SelectMany<T, R>(IPipe<T> pipe, Func<T, IPipe<R>> f)
        {
            var push = Cast(pipe);
            return new PushPipe<R>(this, k => push(i => Cast(f(i))(i1 => k(i1))));
        }
        public IPipe<S> SelectMany<T, R, S>(IPipe<T> pipe, Func<T, IPipe<R>> f, Func<T, R, S> g)
        {
            var push = Cast(pipe);
            return new PushPipe<S>(this, k => push(i => Cast(f(i))(i1 => k(g(i, i1)))));
        }
        public IPipe<T> Where<T>(IPipe<T> pipe, Func<T, bool> clause)
        {
            var push = Cast(pipe);
            return new PushPipe<T>(this, k => push(i => { if (clause(i)) k(i); }));
        }
        public void Execute<T>(IPipe<T> pipe, Action<T> accept)
        {
            Cast(pipe)(accept);
        }
    }
}
