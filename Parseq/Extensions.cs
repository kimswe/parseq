﻿/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable,Action<T> action)
        {
            foreach (var i in enumerable)
                action(i);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, Int32> action)
        {
            var index = 0;
            foreach (var i in enumerable)
                action(i, index++);
        }

        public static T With<T>(this T value, Action<T> action) {
            if (action == null)
                throw new ArgumentNullException("action");
            action(value);
            return value;
        }

        public static TResult Case<T, TResult>(
            this IEnumerable<T> enumerable,
            Func<TResult> nil,
            Func<T, IEnumerable<T>, TResult> cons)
        {
            var enumerator = enumerable.GetEnumerator();
            return !(enumerator.MoveNext()) ? nil() : cons(enumerator.Current, enumerator.Enumerate());
        }

        public static TResult Case<T, TResult>(
            this Tuple<T, IEnumerable<T>> pattern,
            Func<T, IEnumerable<T>, TResult> selector)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return selector(pattern.Item1, pattern.Item2);
        }

        public static Tuple<T, IEnumerable<T>> HeadAndTail<T>(this IEnumerable<T> enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException();

            var head = enumerator.Current;
            var tail = enumerator.Enumerate();

            return Tuple.Create(head, tail);
        }

        public static Tuple<T, IEnumerable<T>> LastAndInit<T>(this IEnumerable<T> enumerable)
        {
            var enumerator = enumerable.Reverse().GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException();

            var init = enumerator.EnumerateWithoutLast();
            var last = enumerator.Current;

            return Tuple.Create(last, init);
        }        

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, T second)
        {
            foreach (var t in first)
                yield return t;

            yield return second;
        }

        public static IEnumerable<T> Concat<T>(this T first,IEnumerable<T> second)
        {
            yield return first;

            foreach (var t in second)
                yield return t;
        }

        public static IEnumerable<T> Replicate<T>(this T value)
        {
            return Extensions.Replicate(() => value);
        }

        public static IEnumerable<T> Replicate<T>(this Func<T> selector)
        {
            while (true)
                yield return selector();
        }

        public static IEnumerable<T> Enumerate<T>(this T value)
        {
            yield return value;
        }

        public static IEnumerable<T> Enumerate<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        private static IEnumerable<T> EnumerateWithoutLast<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");

            if (enumerator.MoveNext())
            {
                do yield return enumerator.Current; while (enumerator.MoveNext());
            }
        }

        public static TResult Foldl<T, TResult>(this IEnumerable<T> enumerable, TResult seed, Func<TResult, T, TResult> folder)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (folder == null)
                throw new ArgumentNullException("folder");

            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    seed = folder(seed, enumerator.Current);
                return seed;
            }            
        }

        public static TResult Foldr<T, TResult>(this IEnumerable<T> enumerable, TResult seed, Func<T, TResult, TResult> folder)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (folder == null)
                throw new ArgumentNullException("folder");

            using (var enumerator = enumerable.Reverse().GetEnumerator())
            {
                while (enumerator.MoveNext())
                    seed = folder(enumerator.Current, seed);
                return seed;
            }
        }
    }
}