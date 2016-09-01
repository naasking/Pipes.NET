﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipes
{
    // A basic implementation of Streams ala carte using the .NET provider model:
    // https://yanniss.github.io/algebras-ecoop15.pdf

    /// <summary>
    /// A pipe algebra using the standard LINQ interface.
    /// </summary>
    public interface IPipeProvider
    {
        IPipe<R> Select<T, R>(IPipe<T> pipe, Func<T, R> f);
        IPipe<R> SelectMany<T, R>(IPipe<T> pipe, Func<T, IPipe<R>> f);
        IPipe<S> SelectMany<T, R, S>(IPipe<T> pipe, Func<T, IPipe<R>> f, Func<T, R, S> g);
        IPipe<T> Where<T>(IPipe<T> pipe, Func<T, bool> clause);
        //void Execute<T>(IPipe<T> pipe, Action<T> accept);
    }

    /// <summary>
    /// A stream of values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPipe<T>
    {
        IPipeProvider Provider { get; }
        Action Compile(Action<T> accept);
    }

    /// <summary>
    /// Extensions that dispatch to the provider.
    /// </summary>
    public static class Pipe
    {
        //sealed class PushObserver<T> : IObserver<T>
        //{
        //    Action<T> k;
        //    public void OnNext(T value)
        //    {
        //        k(value);
        //    }
        //    public void OnError(Exception e)
        //    {
        //    }
        //    public void OnCompleted() { }
        //}

        //public static IPipe<T> AsPipe<T>(this IObservable<T> source)
        //{
        //    var sink = new PushObserver<T>();
        //    var handle = source.Subscribe(sink);
        //    return new PushPipe<T>(new PushProvider(), k => i => k(source.OnNext(i)));
        //}

        public static IPipe<T> AsPipe<T>(this IEnumerable<T> source)
        {
            var ie = source.GetEnumerator();
            return new PullPipe<T>(new PullProvider(), () =>
            {
                while (ie.MoveNext()) return ie.Current;
                ie.Dispose();
                throw new ObjectDisposedException("End of stream.");
            });
        }

        public static IPipe<R> Select<T, R>(IPipe<T> pipe, Func<T, R> f)
        {
            return pipe.Provider.Select(pipe, f);
        }
        public static IPipe<R> SelectMany<T, R>(IPipe<T> pipe, Func<T, IPipe<R>> f)
        {
            return pipe.Provider.SelectMany(pipe, f);
        }
        public static IPipe<T> Where<T>(IPipe<T> pipe, Func<T, bool> clause)
        {
            return pipe.Provider.Where(pipe, clause);
        }
        public static IPipe<S> SelectMany<T, R, S>(IPipe<T> pipe, Func<T, IPipe<R>> f, Func<T, R, S> g)
        {
            return pipe.Provider.SelectMany(pipe, f, g);
        }
    }
}