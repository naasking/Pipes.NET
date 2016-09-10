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
        internal Action<Action<T>> push;
        public PushPipe(Action<Action<T>> k)
        {
            push = k;
        }

        Action<Action<T0>> Cast<T0>(IPipe<T0> pipe)
        {
            var push = (PushPipe<T0>)pipe;
            return push.push;
        }
        public IPipe<R> Select<R>(Func<T, R> f)
        {
            return new PushPipe<R>(k => push(i => k(f(i))));
        }
        public IPipe<R> SelectMany<R>(Func<T, IPipe<R>> f)
        {
            return new PushPipe<R>(k => push(i => Cast(f(i))(i1 => k(i1))));
        }
        public IPipe<S> SelectMany<R, S>(Func<T, IPipe<R>> f, Func<T, R, S> g)
        {
            return new PushPipe<S>(k => push(i => Cast(f(i))(i1 => k(g(i, i1)))));
        }
        public IPipe<T> Where(Func<T, bool> clause)
        {
            return new PushPipe<T>(k => push(i => { if (clause(i)) k(i); }));
        }
        public void Execute(Action<T> accept)
        {
            push(accept);
        }
    }
}
