// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using MicroBenchmarks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Text.Json.Serialization.Tests
{
    public class BenchmarkResolver
    {
        [Params(2, 20, 200, 2_000)]
        public int Count;

        private ObjectIDGenerator _oidg = new ObjectIDGenerator();
        private DefaultReferenceResolver _drr = new DefaultReferenceResolver(true);
        private object[] _object;

        [GlobalSetup]
        public void Setup()
        {
            _drr = new DefaultReferenceResolver(true);
            _oidg = new ObjectIDGenerator();

            _object = new object[Count];
            for (int i = 0; i < Count; i++)
            {
                _object[i] = new object();
            }
        }

        [BenchmarkCategory(Categories.Libraries, Categories.JSON)]
        [Benchmark]
        public void DefaultReferenceResolver()
        {
            for (int i = 0; i < _object.Length; i++)
            {
                _drr.TryGetOrAddReferenceOnSerialize(_object, out uint referenceId);
            }
        }

        [BenchmarkCategory(Categories.Libraries, Categories.JSON)]
        [Benchmark]
        public void ObjectIDGenerator() 
        {
            for (int i = 0; i < _object.Length; i++)
            {
                _oidg.GetId(_object, out bool firstTime);
            }
        }
    }

    internal struct DefaultReferenceResolver
    {
        private uint _referenceCount;
        private readonly Dictionary<string, object> _referenceIdToObjectMap;
        private readonly Dictionary<object, uint> _objectToReferenceIdMap;

        public DefaultReferenceResolver(bool writing)
        {
            _referenceCount = default;

            if (writing)
            {
                // Comparer used here to always do a Reference Equality comparison on serialization which is where we use the objects as the TKey in our dictionary.
                _objectToReferenceIdMap = new Dictionary<object, uint>(ReferenceEqualsEqualityComparer<object>.Comparer);
                _referenceIdToObjectMap = null;
            }
            else
            {
                _referenceIdToObjectMap = new Dictionary<string, object>();
                _objectToReferenceIdMap = null;
            }
        }


        /// <summary>
        /// Adds an entry to the bag of references using the specified id and value.
        /// This method gets called when an $id metadata property from a JSON object is read.
        /// </summary>
        /// <param name="referenceId">The identifier of the respective JSON object or array.</param>
        /// <param name="value">The value of the respective CLR reference type object that results from parsing the JSON object.</param>
        public void AddReferenceOnDeserialize(string referenceId, object value)
        {
            if (TryAdd(_referenceIdToObjectMap!, referenceId, value))
            {
                throw new Exception();//ThrowHelper.ThrowJsonException_MetadataDuplicateIdFound(referenceId);
            }
        }

        internal static bool TryAdd<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
#if NETSTANDARD2_0 || NETFRAMEWORK
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return true;
            }

            return false;
#else
            return dictionary.TryAdd(key, value);
#endif
        }

        /// <summary>
        /// Gets the reference id of the specified value if exists; otherwise a new id is assigned.
        /// This method gets called before a CLR object is written so we can decide whether to write $id and the rest of its properties or $ref and step into the next object.
        /// The first $id value will be 1.
        /// </summary>
        /// <param name="value">The value of the CLR reference type object to get or add an id for.</param>
        /// <param name="referenceId">The id realated to the object.</param>
        /// <returns></returns>
        public bool TryGetOrAddReferenceOnSerialize(object value, out uint referenceId)
        {
            if (!_objectToReferenceIdMap!.TryGetValue(value, out referenceId!))
            {
                _referenceCount++;
                referenceId = _referenceCount;//.ToString();
                _objectToReferenceIdMap.Add(value, referenceId);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Resolves the CLR reference type object related to the specified reference id.
        /// This method gets called when $ref metadata property is read.
        /// </summary>
        /// <param name="referenceId">The id related to the returned object.</param>
        /// <returns></returns>
        public object ResolveReferenceOnDeserialize(string referenceId)
        {
            if (!_referenceIdToObjectMap.TryGetValue(referenceId, out object value))
            {
                throw new Exception();//ThrowHelper.ThrowJsonException_MetadataReferenceNotFound(referenceId);
            }

            return value;
        }
    }

    internal sealed class ReferenceEqualsEqualityComparer<T> : IEqualityComparer<T>
    {
        public static ReferenceEqualsEqualityComparer<T> Comparer = new ReferenceEqualsEqualityComparer<T>();

        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
