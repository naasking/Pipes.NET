using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipes
{
    /// <summary>
    /// A push stream based on partially evaluated continuations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Could also build a LINQ expression backend based on this model. This would
    /// eliminate all of the nested lambdas.
    /// </remarks>
    class EvalPushPipe<T> : IPipe<T>
    {
        internal Func<Action<T>, Action> build;
        public EvalPushPipe(IPipeProvider provider, Func<Action<T>, Action> k)
        {
            build = k;
            Provider = provider;
        }
        public IPipeProvider Provider { get; private set; }
        public Action Compile(Action<T> accept)
        {
            return build(accept);
        }
    }

    /// <summary>
    /// The continuation-based push stream provider.
    /// </summary>
    public class EvalPushProvider : IPipeProvider
    {
        Func<Action<T>, Action> Cast<T>(IPipe<T> pipe)
        {
            var push = (EvalPushPipe<T>)pipe;
            return push.build;
        }
        public IPipe<R> Select<T, R>(IPipe<T> pipe, Func<T, R> f)
        {
            var push = Cast(pipe);
            return new EvalPushPipe<R>(this, k => push(i => k(f(i))));
        }
        public IPipe<R> SelectMany<T, R>(IPipe<T> pipe, Func<T, IPipe<R>> f)
        {
            var push = Cast(pipe);
            return new EvalPushPipe<R>(this, k => push(i => Cast(f(i))(i1 => k(i1))));
        }
        public IPipe<S> SelectMany<T, R, S>(IPipe<T> pipe, Func<T, IPipe<R>> f, Func<T, R, S> g)
        {
            var push = Cast(pipe);
            return new EvalPushPipe<S>(this, k => push(i => Cast(f(i))(i1 => k(g(i, i1)))));
        }
        public IPipe<T> Where<T>(IPipe<T> pipe, Func<T, bool> clause)
        {
            var push = Cast(pipe);
            return new EvalPushPipe<T>(this, k => push(i => { if (clause(i)) k(i); }));
        }
        public void Execute<T>(IPipe<T> pipe, Action<T> accept)
        {
            Cast(pipe)(accept);
        }
    }
}
