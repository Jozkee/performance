// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using MicroBenchmarks;
using MicroBenchmarks.Serializers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Tests
{
    [GenericTypeArguments(typeof(LoginViewModel))]
    [GenericTypeArguments(typeof(Location))]
    [GenericTypeArguments(typeof(IndexViewModel))]
    [GenericTypeArguments(typeof(MyEventsListerViewModel))]
    [GenericTypeArguments(typeof(SimpleListOfInt))]
    [GenericTypeArguments(typeof(SimpleStructWithProperties))]
    public class WriteIgnoreCycle<T>
    {

        private T _value;
        private JsonSerializerOptions _options;
        private JsonSerializerSettings _settings;

        [GlobalSetup]
        public void Setup()
        {
            _value = DataGenerator.Generate<T>();
            _options = new JsonSerializerOptions { MaxDepth = 1000 };

            // Temporarily use reflection to set the feature since its not publicly available yet.
            Type typeOfOptions = typeof(JsonSerializerOptions);
            Type typeOfRefHandler = typeof(ReferenceHandler);
            typeOfOptions.GetProperty("ReferenceHandler").SetValue(_options, typeOfRefHandler.GetProperty("IgnoreCycle").GetValue(_options.ReferenceHandler));

            _settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            JsonSerializer.Serialize(_value, _options);
        }

        [BenchmarkCategory(Categories.Libraries, Categories.JSON)]
        [Benchmark]
        public string SerializeWithIgnoreCycle_Generic() => JsonSerializer.Serialize(_value, _options);

        [BenchmarkCategory(Categories.ThirdParty, Categories.JSON)]
        [Benchmark(Baseline = true)]
        public string NewtonsoftSerializeWithIgnore_Generic() => JsonConvert.SerializeObject(_value, _settings);
    }

    public class WriteIgnoreCycle
    {
        [Params(1, 10, 100, 500, 999)]
        public int Depth;

        private Node _value;
        private JsonSerializerOptions _options;
        private JsonSerializerSettings _settings;

        [GlobalSetup]
        public void Setup()
        {
            Node current = _value = new Node();

            for (int i = 0; i < Depth; i++)
            {
                current = current.Next = new Node();
            }

            _options = new JsonSerializerOptions { MaxDepth = 1000 };

            // Temporarily use reflection to set the feature that is not publicly available.
            Type typeOfOptions = typeof(JsonSerializerOptions);
            Type typeOfRefHandler = typeof(ReferenceHandler);
            typeOfOptions.GetProperty("ReferenceHandler").SetValue(_options, typeOfRefHandler.GetProperty("IgnoreCycle").GetValue(_options.ReferenceHandler));

            _settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            JsonSerializer.Serialize(_value, _options);
        }

        [BenchmarkCategory(Categories.Libraries, Categories.JSON)]
        [Benchmark]
        public string SerializeWithIgnoreCycle() => JsonSerializer.Serialize(_value, _options);

        [BenchmarkCategory(Categories.ThirdParty, Categories.JSON)]
        [Benchmark(Baseline = true)]
        public string NewtonsoftSerializeWithIgnore() => JsonConvert.SerializeObject(_value, _settings);

        private class Node
        {
            public Node Next { get; set; }
        }
    }
}