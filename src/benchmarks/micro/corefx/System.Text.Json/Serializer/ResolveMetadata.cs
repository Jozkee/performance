// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using MicroBenchmarks;

namespace System.Text.Json.Serialization.Tests
{
    public class ResolveMetadata
    {

        private static byte[] s_id = new byte[] { (byte)'i', (byte)'d' };
        private static byte[] s_ref = new byte[] { (byte)'r', (byte)'e', (byte)'f' };
        private static byte[] s_values = new byte[] { (byte)'v', (byte)'a', (byte)'l', (byte)'u', (byte)'e', (byte)'s' };

        [Params("$id", "$ref", "$values", "$idddd", "values")]
        public string propertyName;

        private byte[] propertyNameAsBytes;

        [GlobalSetup]
        public void Setup()
        {
            propertyNameAsBytes = Encoding.UTF8.GetBytes(propertyName);
        }

        [BenchmarkCategory(Categories.CoreFX, Categories.JSON)]
        [Benchmark]
        public MetadataPropertyName GetMetadataUsingSequenceEquals() => GetMetadataUsingSequenceEquals(propertyNameAsBytes.AsSpan());

        private MetadataPropertyName GetMetadataUsingSequenceEquals(ReadOnlySpan<byte> propertyName)
        {
            if (propertyName.SequenceEqual(s_id))
            {
                return MetadataPropertyName.Id;
            }
            else if (propertyName.SequenceEqual(s_ref))
            {
                return MetadataPropertyName.Ref;
            }
            else if (propertyName.SequenceEqual(s_values))
            {
                return MetadataPropertyName.Values;
            }
            else
            {
                return MetadataPropertyName.NoMetadata;
            }
        }

        [BenchmarkCategory(Categories.CoreFX, Categories.JSON)]
        [Benchmark]
        public MetadataPropertyName GetMetadataUsingSequenceEquals2() => GetMetadataUsingSequenceEquals2(propertyNameAsBytes.AsSpan());

        private MetadataPropertyName GetMetadataUsingSequenceEquals2(ReadOnlySpan<byte> propertyName)
        {
            if (propertyName[0] == '$')
            {
                switch (propertyName.Length)
                {
                    case 3:
                        if (propertyName.Slice(1).SequenceEqual(s_id))
                        {
                            return MetadataPropertyName.Id;
                        }
                        break;

                    case 4:
                        if (propertyName.Slice(1).SequenceEqual(s_ref))
                        {
                            return MetadataPropertyName.Ref;
                        }
                        break;

                    case 7:
                        if (propertyName.Slice(1).SequenceEqual(s_values))
                        {
                            return MetadataPropertyName.Values;
                        }
                        break;
                }
            }

            return MetadataPropertyName.NoMetadata;
        }

        [BenchmarkCategory(Categories.CoreFX, Categories.JSON)]
        [Benchmark]
        public MetadataPropertyName GetMetadataPropertyName() => GetMetadataPropertyName(propertyNameAsBytes.AsSpan());

        private MetadataPropertyName GetMetadataPropertyName(ReadOnlySpan<byte> propertyName)
        {
            if (propertyName[0] == '$')
            {
                switch (propertyName.Length)
                {
                    case 3:
                        if (propertyName[1] == 'i' &&
                            propertyName[2] == 'd')
                        {
                            return MetadataPropertyName.Id;
                        }
                        break;

                    case 4:
                        if (propertyName[1] == 'r' &&
                            propertyName[2] == 'e' &&
                            propertyName[3] == 'f')
                        {
                            return MetadataPropertyName.Ref;
                        }
                        break;

                    case 7:
                        if (propertyName[1] == 'v' &&
                            propertyName[2] == 'a' &&
                            propertyName[3] == 'l' &&
                            propertyName[4] == 'u' &&
                            propertyName[5] == 'e' &&
                            propertyName[6] == 's')
                        {
                            return MetadataPropertyName.Values;
                        }
                        break;
                }
            }

            return MetadataPropertyName.NoMetadata;
        }

        public enum MetadataPropertyName
        {
            NoMetadata,
            Values,
            Id,
            Ref,
        }
    }
}
