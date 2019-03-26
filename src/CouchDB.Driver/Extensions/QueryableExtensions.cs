﻿using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CouchDB.Driver.Extensions
{
    public static class QueryableExtensions
    {
        #region Helper methods to obtain MethodInfo in a safe way

        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6)
        {
            return f.Method;
        }

        #endregion

        public static ICouchList<TSource> ToCouchList<TSource>(this IQueryable<TSource> source)
        {
            if (source is CouchQuery<TSource> couchQuery)
            {
                return couchQuery.ToCouchList();
            }
            throw new NotSupportedException($"The method CompleteResult is not supported on this type of IQueryable");
        }
        public static Task<ICouchList<TSource>> ToCouchListAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task<ICouchList<TSource>>.Factory.StartNew(() => source.ToCouchList());
        }
        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task<List<TSource>>.Factory.StartNew(() => source.ToList());
        }
        public static IQueryable<TSource> UseBookmark<TSource>(this IQueryable<TSource> source, string bookmark)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(bookmark))
                throw new ArgumentNullException(nameof(bookmark));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(UseBookmark, source, bookmark),
                    new Expression[] { source.Expression, Expression.Constant(bookmark) }));
        }
        public static IQueryable<TSource> WithReadQuorum<TSource>(this IQueryable<TSource> source, int quorum)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (quorum < 1)
                throw new ArgumentException(nameof(quorum), "Read quorum cannot be less than 1.");

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(WithReadQuorum, source, quorum),
                    new Expression[] { source.Expression, Expression.Constant(quorum) }));
        }
        public static IQueryable<TSource> WithoutIndexUpdate<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(WithoutIndexUpdate, source),
                    new Expression[] { source.Expression }));
        }
        public static IQueryable<TSource> FromStable<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(FromStable, source),
                    new Expression[] { source.Expression }));
        }
        public static IQueryable<TSource> UseIndex<TSource>(this IQueryable<TSource> source, params string[] indexes)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));
            if (indexes.Length != 1 && indexes.Length != 2)
                throw new ArgumentException(nameof(indexes), "Only 1 or 2 parameters are allowed. \"<design_document>\" or [\"<design_document>\",\"<index_name>\"]");

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(UseIndex, source, indexes),
                    new Expression[] { source.Expression, Expression.Constant(indexes) }));
        }
        public static IQueryable<TSource> IncludeExecutionStats<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfo(IncludeExecutionStats, source),
                    new Expression[] { source.Expression }));
        }
    }
}