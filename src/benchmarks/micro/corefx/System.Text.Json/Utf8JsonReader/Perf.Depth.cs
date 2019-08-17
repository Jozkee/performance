// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using MicroBenchmarks;
using Newtonsoft.Json;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

namespace System.Text.Json.Tests
{
    [BenchmarkCategory(Categories.CoreFX, Categories.JSON)]
    public class Perf_Depth
    {
        private byte[] _dataUtf8;
        private MemoryStream _stream;
        private StreamReader _reader;

        [Params(1, 2, 4, 8, 16, 32, 64, 65, 66, 128, 256, 512)]
        public int Depth;

        [GlobalSetup]
        public void Setup()
        {
            var output = new ArrayBufferWriter<byte>(1024);
            var jsonUtf8 = new Utf8JsonWriter(output);

            WriteDepth(ref jsonUtf8, Depth - 1);

            string actualStr = Encoding.UTF8.GetString(output.WrittenSpan);

            _dataUtf8 = Encoding.UTF8.GetBytes(actualStr);

            _stream = new MemoryStream(_dataUtf8);
            _reader = new StreamReader(_stream, Encoding.UTF8, false, 1024, true);
        }

        [Benchmark]
        public void ReadSpanEmptyLoop()
        {
            var json = new Utf8JsonReader(_dataUtf8, 
                new JsonReaderOptions { 
                    MaxDepth = Depth
                });
            while (json.Read()) ;
        }

        private static void WriteDepth(ref Utf8JsonWriter jsonUtf8, int depth)
        {
            jsonUtf8.WriteStartObject();
            for (int i = 0; i < depth; i++)
            {
                jsonUtf8.WriteStartObject("message" + i);
            }
            jsonUtf8.WriteString("message" + depth, "Hello, World!");
            for (int i = 0; i < depth; i++)
            {
                jsonUtf8.WriteEndObject();
            }
            jsonUtf8.WriteEndObject();
            jsonUtf8.Flush();
        }
    }
}
