// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Net.Http.Headers
{
    public class MediaTypeHeaderValueTest
    {
        [Fact]
        public void Ctor_MediaTypeNull_Throw()
        {
            Assert.Throws<ArgumentException>(() => new MediaTypeHeaderValue(null));
            // null and empty should be treated the same. So we also throw for empty strings.
            Assert.Throws<ArgumentException>(() => new MediaTypeHeaderValue(string.Empty));
        }

        [Fact]
        public void Ctor_MediaTypeInvalidFormat_ThrowFormatException()
        {
            // When adding values using strongly typed objects, no leading/trailing LWS (whitespaces) are allowed.
            AssertFormatException(" text/plain ");
            AssertFormatException("text / plain");
            AssertFormatException("text/ plain");
            AssertFormatException("text /plain");
            AssertFormatException("text/plain ");
            AssertFormatException(" text/plain");
            AssertFormatException("te xt/plain");
            AssertFormatException("te=xt/plain");
            AssertFormatException("teäxt/plain");
            AssertFormatException("text/pläin");
            AssertFormatException("text");
            AssertFormatException("\"text/plain\"");
            AssertFormatException("text/plain; charset=utf-8; ");
            AssertFormatException("text/plain;");
            AssertFormatException("text/plain;charset=utf-8"); // ctor takes only media-type name, no parameters
        }

        [Fact]
        public void Ctor_MediaTypeValidFormat_SuccessfullyCreated()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");
            Assert.Equal("text/plain", mediaType.MediaType);
            Assert.Equal(0, mediaType.Parameters.Count);
            Assert.Null(mediaType.Charset);
        }

        [Fact]
        public void Ctor_AddNameAndQuality_QualityParameterAdded()
        {
            var mediaType = new MediaTypeHeaderValue("application/xml", 0.08);
            Assert.Equal(0.08, mediaType.Quality);
            Assert.Equal("application/xml", mediaType.MediaType);
            Assert.Equal(1, mediaType.Parameters.Count);
        }

        [Fact]
        public void Parameters_AddNull_Throw()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");
            Assert.Throws<ArgumentNullException>(() => mediaType.Parameters.Add(null));
        }

        [Fact]
        public void Copy_SimpleMediaType_Copied()
        {
            var mediaType0 = new MediaTypeHeaderValue("text/plain");
            var mediaType1 = mediaType0.Copy();
            Assert.NotSame(mediaType0, mediaType1);
            Assert.Same(mediaType0.MediaType, mediaType1.MediaType);
            Assert.NotSame(mediaType0.Parameters, mediaType1.Parameters);
            Assert.Equal(mediaType0.Parameters.Count, mediaType1.Parameters.Count);
        }

        [Fact]
        public void CopyAsReadOnly_SimpleMediaType_CopiedAndReadOnly()
        {
            var mediaType0 = new MediaTypeHeaderValue("text/plain");
            var mediaType1 = mediaType0.CopyAsReadOnly();
            Assert.NotSame(mediaType0, mediaType1);
            Assert.Same(mediaType0.MediaType, mediaType1.MediaType);
            Assert.NotSame(mediaType0.Parameters, mediaType1.Parameters);
            Assert.Equal(mediaType0.Parameters.Count, mediaType1.Parameters.Count);

            Assert.False(mediaType0.IsReadOnly);
            Assert.True(mediaType1.IsReadOnly);
            Assert.Throws<InvalidOperationException>(() => { mediaType1.MediaType = "some/value"; });
        }

        [Fact]
        public void Copy_WithParameters_Copied()
        {
            var mediaType0 = new MediaTypeHeaderValue("text/plain");
            mediaType0.Parameters.Add(new NameValueHeaderValue("name", "value"));
            var mediaType1 = mediaType0.Copy();
            Assert.NotSame(mediaType0, mediaType1);
            Assert.Same(mediaType0.MediaType, mediaType1.MediaType);
            Assert.NotSame(mediaType0.Parameters, mediaType1.Parameters);
            Assert.Equal(mediaType0.Parameters.Count, mediaType1.Parameters.Count);
            var pair0 = mediaType0.Parameters.First();
            var pair1 = mediaType1.Parameters.First();
            Assert.NotSame(pair0, pair1);
            Assert.Same(pair0.Name, pair1.Name);
            Assert.Same(pair0.Value, pair1.Value);
        }

        [Fact]
        public void CopyAsReadOnly_WithParameters_CopiedAndReadOnly()
        {
            var mediaType0 = new MediaTypeHeaderValue("text/plain");
            mediaType0.Parameters.Add(new NameValueHeaderValue("name", "value"));
            var mediaType1 = mediaType0.CopyAsReadOnly();
            Assert.NotSame(mediaType0, mediaType1);
            Assert.False(mediaType0.IsReadOnly);
            Assert.True(mediaType1.IsReadOnly);
            Assert.Same(mediaType0.MediaType, mediaType1.MediaType);

            Assert.NotSame(mediaType0.Parameters, mediaType1.Parameters);
            Assert.False(mediaType0.Parameters.IsReadOnly);
            Assert.True(mediaType1.Parameters.IsReadOnly);
            Assert.Equal(mediaType0.Parameters.Count, mediaType1.Parameters.Count);
            Assert.Throws<InvalidOperationException>(() => mediaType1.Parameters.Add(new NameValueHeaderValue("name")));
            Assert.Throws<InvalidOperationException>(() => mediaType1.Parameters.Remove(new NameValueHeaderValue("name")));
            Assert.Throws<InvalidOperationException>(() => mediaType1.Parameters.Clear());

            var pair0 = mediaType0.Parameters.First();
            var pair1 = mediaType1.Parameters.First();
            Assert.NotSame(pair0, pair1);
            Assert.False(pair0.IsReadOnly);
            Assert.True(pair1.IsReadOnly);
            Assert.Same(pair0.Name, pair1.Name);
            Assert.Same(pair0.Value, pair1.Value);
        }

        [Fact]
        public void CopyFromReadOnly_WithParameters_CopiedAsNonReadOnly()
        {
            var mediaType0 = new MediaTypeHeaderValue("text/plain");
            mediaType0.Parameters.Add(new NameValueHeaderValue("name", "value"));
            var mediaType1 = mediaType0.CopyAsReadOnly();
            var mediaType2 = mediaType1.Copy();

            Assert.NotSame(mediaType2, mediaType1);
            Assert.Same(mediaType2.MediaType, mediaType1.MediaType);
            Assert.True(mediaType1.IsReadOnly);
            Assert.False(mediaType2.IsReadOnly);
            Assert.NotSame(mediaType2.Parameters, mediaType1.Parameters);
            Assert.Equal(mediaType2.Parameters.Count, mediaType1.Parameters.Count);
            var pair2 = mediaType2.Parameters.First();
            var pair1 = mediaType1.Parameters.First();
            Assert.NotSame(pair2, pair1);
            Assert.True(pair1.IsReadOnly);
            Assert.False(pair2.IsReadOnly);
            Assert.Same(pair2.Name, pair1.Name);
            Assert.Same(pair2.Value, pair1.Value);
        }

        [Fact]
        public void MediaType_SetAndGetMediaType_MatchExpectations()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");
            Assert.Equal("text/plain", mediaType.MediaType);

            mediaType.MediaType = "application/xml";
            Assert.Equal("application/xml", mediaType.MediaType);
        }

        [Fact]
        public void Charset_SetCharsetAndValidateObject_ParametersEntryForCharsetAdded()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");
            mediaType.Charset = "mycharset";
            Assert.Equal("mycharset", mediaType.Charset);
            Assert.Equal(1, mediaType.Parameters.Count);
            Assert.Equal("charset", mediaType.Parameters.First().Name);

            mediaType.Charset = null;
            Assert.Null(mediaType.Charset);
            Assert.Equal(0, mediaType.Parameters.Count);
            mediaType.Charset = null; // It's OK to set it again to null; no exception.
        }

        [Fact]
        public void Charset_AddCharsetParameterThenUseProperty_ParametersEntryIsOverwritten()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");

            // Note that uppercase letters are used. Comparison should happen case-insensitive.
            var charset = new NameValueHeaderValue("CHARSET", "old_charset");
            mediaType.Parameters.Add(charset);
            Assert.Equal(1, mediaType.Parameters.Count);
            Assert.Equal("CHARSET", mediaType.Parameters.First().Name);

            mediaType.Charset = "new_charset";
            Assert.Equal("new_charset", mediaType.Charset);
            Assert.Equal(1, mediaType.Parameters.Count);
            Assert.Equal("CHARSET", mediaType.Parameters.First().Name);

            mediaType.Parameters.Remove(charset);
            Assert.Null(mediaType.Charset);
        }

        [Fact]
        public void Quality_SetCharsetAndValidateObject_ParametersEntryForCharsetAdded()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");
            mediaType.Quality = 0.563156454;
            Assert.Equal(0.563, mediaType.Quality);
            Assert.Equal(1, mediaType.Parameters.Count);
            Assert.Equal("q", mediaType.Parameters.First().Name);
            Assert.Equal("0.563", mediaType.Parameters.First().Value);

            mediaType.Quality = null;
            Assert.Null(mediaType.Quality);
            Assert.Equal(0, mediaType.Parameters.Count);
            mediaType.Quality = null; // It's OK to set it again to null; no exception.
        }

        [Fact]
        public void Quality_AddQualityParameterThenUseProperty_ParametersEntryIsOverwritten()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");

            var quality = new NameValueHeaderValue("q", "0.132");
            mediaType.Parameters.Add(quality);
            Assert.Equal(1, mediaType.Parameters.Count);
            Assert.Equal("q", mediaType.Parameters.First().Name);
            Assert.Equal(0.132, mediaType.Quality);

            mediaType.Quality = 0.9;
            Assert.Equal(0.9, mediaType.Quality);
            Assert.Equal(1, mediaType.Parameters.Count);
            Assert.Equal("q", mediaType.Parameters.First().Name);

            mediaType.Parameters.Remove(quality);
            Assert.Null(mediaType.Quality);
        }

        [Fact]
        public void Quality_AddQualityParameterUpperCase_CaseInsensitiveComparison()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");

            var quality = new NameValueHeaderValue("Q", "0.132");
            mediaType.Parameters.Add(quality);
            Assert.Equal(1, mediaType.Parameters.Count);
            Assert.Equal("Q", mediaType.Parameters.First().Name);
            Assert.Equal(0.132, mediaType.Quality);
        }

        [Fact]
        public void Quality_LessThanZero_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MediaTypeHeaderValue("application/xml", -0.01));
        }

        [Fact]
        public void Quality_GreaterThanOne_Throw()
        {
            var mediaType = new MediaTypeHeaderValue("application/xml");
            Assert.Throws<ArgumentOutOfRangeException>(() => mediaType.Quality = 1.01);
        }

        [Fact]
        public void ToString_UseDifferentMediaTypes_AllSerializedCorrectly()
        {
            var mediaType = new MediaTypeHeaderValue("text/plain");
            Assert.Equal("text/plain", mediaType.ToString());

            mediaType.Charset = "utf-8";
            Assert.Equal("text/plain; charset=utf-8", mediaType.ToString());

            mediaType.Parameters.Add(new NameValueHeaderValue("custom", "\"custom value\""));
            Assert.Equal("text/plain; charset=utf-8; custom=\"custom value\"", mediaType.ToString());

            mediaType.Charset = null;
            Assert.Equal("text/plain; custom=\"custom value\"", mediaType.ToString());
        }

        [Fact]
        public void GetHashCode_UseMediaTypeWithAndWithoutParameters_SameOrDifferentHashCodes()
        {
            var mediaType1 = new MediaTypeHeaderValue("text/plain");
            var mediaType2 = new MediaTypeHeaderValue("text/plain");
            mediaType2.Charset = "utf-8";
            var mediaType3 = new MediaTypeHeaderValue("text/plain");
            mediaType3.Parameters.Add(new NameValueHeaderValue("name", "value"));
            var mediaType4 = new MediaTypeHeaderValue("TEXT/plain");
            var mediaType5 = new MediaTypeHeaderValue("TEXT/plain");
            mediaType5.Parameters.Add(new NameValueHeaderValue("CHARSET", "UTF-8"));

            Assert.NotEqual(mediaType1.GetHashCode(), mediaType2.GetHashCode());
            Assert.NotEqual(mediaType1.GetHashCode(), mediaType3.GetHashCode());
            Assert.NotEqual(mediaType2.GetHashCode(), mediaType3.GetHashCode());
            Assert.Equal(mediaType1.GetHashCode(), mediaType4.GetHashCode());
            Assert.Equal(mediaType2.GetHashCode(), mediaType5.GetHashCode());
        }

        [Fact]
        public void Equals_UseMediaTypeWithAndWithoutParameters_EqualOrNotEqualNoExceptions()
        {
            var mediaType1 = new MediaTypeHeaderValue("text/plain");
            var mediaType2 = new MediaTypeHeaderValue("text/plain");
            mediaType2.Charset = "utf-8";
            var mediaType3 = new MediaTypeHeaderValue("text/plain");
            mediaType3.Parameters.Add(new NameValueHeaderValue("name", "value"));
            var mediaType4 = new MediaTypeHeaderValue("TEXT/plain");
            var mediaType5 = new MediaTypeHeaderValue("TEXT/plain");
            mediaType5.Parameters.Add(new NameValueHeaderValue("CHARSET", "UTF-8"));
            var mediaType6 = new MediaTypeHeaderValue("TEXT/plain");
            mediaType6.Parameters.Add(new NameValueHeaderValue("CHARSET", "UTF-8"));
            mediaType6.Parameters.Add(new NameValueHeaderValue("custom", "value"));
            var mediaType7 = new MediaTypeHeaderValue("text/other");

            Assert.False(mediaType1.Equals(mediaType2), "No params vs. charset.");
            Assert.False(mediaType2.Equals(mediaType1), "charset vs. no params.");
            Assert.False(mediaType1.Equals(null), "No params vs. <null>.");
            Assert.False(mediaType1.Equals(mediaType3), "No params vs. custom param.");
            Assert.False(mediaType2.Equals(mediaType3), "charset vs. custom param.");
            Assert.True(mediaType1.Equals(mediaType4), "Different casing.");
            Assert.True(mediaType2.Equals(mediaType5), "Different casing in charset.");
            Assert.False(mediaType5.Equals(mediaType6), "charset vs. custom param.");
            Assert.False(mediaType1.Equals(mediaType7), "text/plain vs. text/other.");
        }

        [Fact]
        public void Parse_SetOfValidValueStrings_ParsedCorrectly()
        {
            CheckValidParse("\r\n text/plain  ", new MediaTypeHeaderValue("text/plain"));
            CheckValidParse("text/plain", new MediaTypeHeaderValue("text/plain"));

            CheckValidParse("\r\n text   /  plain ;  charset =   utf-8 ", new MediaTypeHeaderValue("text/plain") { Charset = "utf-8" });
            CheckValidParse("  text/plain;charset=utf-8", new MediaTypeHeaderValue("text/plain") { Charset = "utf-8" });

            CheckValidParse("text/plain; charset=iso-8859-1", new MediaTypeHeaderValue("text/plain") { Charset = "iso-8859-1" });

            var expected = new MediaTypeHeaderValue("text/plain") { Charset = "utf-8" };
            expected.Parameters.Add(new NameValueHeaderValue("custom", "value"));
            CheckValidParse(" text/plain; custom=value;charset=utf-8", expected);

            expected = new MediaTypeHeaderValue("text/plain");
            expected.Parameters.Add(new NameValueHeaderValue("custom"));
            CheckValidParse(" text/plain; custom", expected);

            expected = new MediaTypeHeaderValue("text/plain") { Charset = "utf-8" };
            expected.Parameters.Add(new NameValueHeaderValue("custom", "\"x\""));
            CheckValidParse("text / plain ; custom =\r\n \"x\" ; charset = utf-8 ", expected);

            expected = new MediaTypeHeaderValue("text/plain") { Charset = "utf-8" };
            expected.Parameters.Add(new NameValueHeaderValue("custom", "\"x\""));
            CheckValidParse("text/plain;custom=\"x\";charset=utf-8", expected);

            expected = new MediaTypeHeaderValue("text/plain");
            CheckValidParse("text/plain;", expected);

            expected = new MediaTypeHeaderValue("text/plain");
            expected.Parameters.Add(new NameValueHeaderValue("name", ""));
            CheckValidParse("text/plain;name=", expected);

            expected = new MediaTypeHeaderValue("text/plain");
            expected.Parameters.Add(new NameValueHeaderValue("name", "value"));
            CheckValidParse("text/plain;name=value;", expected);

            expected = new MediaTypeHeaderValue("text/plain");
            expected.Charset = "iso-8859-1";
            expected.Quality = 1.0;
            CheckValidParse("text/plain; charset=iso-8859-1; q=1.0", expected);

            expected = new MediaTypeHeaderValue("*/xml");
            expected.Charset = "utf-8";
            expected.Quality = 0.5;
            CheckValidParse("\r\n */xml; charset=utf-8; q=0.5", expected);

            expected = new MediaTypeHeaderValue("*/*");
            CheckValidParse("*/*", expected);

            expected = new MediaTypeHeaderValue("text/*");
            expected.Charset = "utf-8";
            expected.Parameters.Add(new NameValueHeaderValue("foo", "bar"));
            CheckValidParse("text/*; charset=utf-8; foo=bar", expected);

            expected = new MediaTypeHeaderValue("text/plain");
            expected.Charset = "utf-8";
            expected.Quality = 0;
            expected.Parameters.Add(new NameValueHeaderValue("foo", "bar"));
            CheckValidParse("text/plain; charset=utf-8; foo=bar; q=0.0", expected);
        }

        [Fact]
        public void Parse_SetOfInvalidValueStrings_Throws()
        {
            CheckInvalidParse("");
            CheckInvalidParse("  ");
            CheckInvalidParse(null);
            CheckInvalidParse("text/plain会");
            CheckInvalidParse("text/plain ,");
            CheckInvalidParse("text/plain,");
            CheckInvalidParse("text/plain; charset=utf-8 ,");
            CheckInvalidParse("text/plain; charset=utf-8,");
            CheckInvalidParse("textplain");
            CheckInvalidParse("text/");
            CheckInvalidParse(",, , ,,text/plain; charset=iso-8859-1; q=1.0,\r\n */xml; charset=utf-8; q=0.5,,,");
            CheckInvalidParse("text/plain; charset=iso-8859-1; q=1.0, */xml; charset=utf-8; q=0.5");
            CheckInvalidParse(" , */xml; charset=utf-8; q=0.5 ");
            CheckInvalidParse("text/plain; charset=iso-8859-1; q=1.0 , ");
        }

        [Fact]
        public void TryParse_SetOfValidValueStrings_ParsedCorrectly()
        {
            var expected = new MediaTypeHeaderValue("text/plain");
            CheckValidTryParse("\r\n text/plain  ", expected);
            CheckValidTryParse("text/plain", expected);

            // We don't have to test all possible input strings, since most of the pieces are handled by other parsers.
            // The purpose of this test is to verify that these other parsers are combined correctly to build a
            // media-type parser.
            expected.Charset = "utf-8";
            CheckValidTryParse("\r\n text   /  plain ;  charset =   utf-8 ", expected);
            CheckValidTryParse("  text/plain;charset=utf-8", expected);

            var value1 = new MediaTypeHeaderValue("text/plain");
            value1.Charset = "iso-8859-1";
            value1.Quality = 1.0;

            CheckValidTryParse("text/plain; charset=iso-8859-1; q=1.0", value1);

            var value2 = new MediaTypeHeaderValue("*/xml");
            value2.Charset = "utf-8";
            value2.Quality = 0.5;

            CheckValidTryParse("\r\n */xml; charset=utf-8; q=0.5", value2);
        }

        [Fact]
        public void TryParse_SetOfInvalidValueStrings_ReturnsFalse()
        {
            CheckInvalidTryParse("");
            CheckInvalidTryParse("  ");
            CheckInvalidTryParse(null);
            CheckInvalidTryParse("text/plain会");
            CheckInvalidTryParse("text/plain ,");
            CheckInvalidTryParse("text/plain,");
            CheckInvalidTryParse("text/plain; charset=utf-8 ,");
            CheckInvalidTryParse("text/plain; charset=utf-8,");
            CheckInvalidTryParse("textplain");
            CheckInvalidTryParse("text/");
            CheckInvalidTryParse(",, , ,,text/plain; charset=iso-8859-1; q=1.0,\r\n */xml; charset=utf-8; q=0.5,,,");
            CheckInvalidTryParse("text/plain; charset=iso-8859-1; q=1.0, */xml; charset=utf-8; q=0.5");
            CheckInvalidTryParse(" , */xml; charset=utf-8; q=0.5 ");
            CheckInvalidTryParse("text/plain; charset=iso-8859-1; q=1.0 , ");
        }

        [Fact]
        public void ParseList_NullOrEmptyArray_ReturnsEmptyList()
        {
            var results = MediaTypeHeaderValue.ParseList(null);
            Assert.NotNull(results);
            Assert.Equal(0, results.Count);

            results = MediaTypeHeaderValue.ParseList(new string[0]);
            Assert.NotNull(results);
            Assert.Equal(0, results.Count);

            results = MediaTypeHeaderValue.ParseList(new string[] { "" });
            Assert.NotNull(results);
            Assert.Equal(0, results.Count);
        }

        [Fact]
        public void TryParseList_NullOrEmptyArray_ReturnsFalse()
        {
            IList<MediaTypeHeaderValue> results;
            Assert.False(MediaTypeHeaderValue.TryParseList(null, out results));
            Assert.False(MediaTypeHeaderValue.TryParseList(new string[0], out results));
            Assert.False(MediaTypeHeaderValue.TryParseList(new string[] { "" }, out results));
        }

        [Fact]
        public void ParseList_SetOfValidValueStrings_ReturnsValues()
        {
            var inputs = new[] { "text/html,application/xhtml+xml,", "application/xml;q=0.9,image/webp,*/*;q=0.8" };
            var results = MediaTypeHeaderValue.ParseList(inputs);

            var expectedResults = new[]
            {
                new MediaTypeHeaderValue("text/html"),
                new MediaTypeHeaderValue("application/xhtml+xml"),
                new MediaTypeHeaderValue("application/xml", 0.9),
                new MediaTypeHeaderValue("image/webp"),
                new MediaTypeHeaderValue("*/*", 0.8),
            }.ToList();

            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public void TryParseList_SetOfValidValueStrings_ReturnsTrue()
        {
            var inputs = new[] { "text/html,application/xhtml+xml,", "application/xml;q=0.9,image/webp,*/*;q=0.8" };
            IList<MediaTypeHeaderValue> results;
            Assert.True(MediaTypeHeaderValue.TryParseList(inputs, out results));

            var expectedResults = new[]
            {
                new MediaTypeHeaderValue("text/html"),
                new MediaTypeHeaderValue("application/xhtml+xml"),
                new MediaTypeHeaderValue("application/xml", 0.9),
                new MediaTypeHeaderValue("image/webp"),
                new MediaTypeHeaderValue("*/*", 0.8),
            }.ToList();

            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public void ParseList_WithSomeInvlaidValues_Throws()
        {
            var inputs = new[]
            {
                "text/html,application/xhtml+xml, ignore-this, ignore/this",
                "application/xml;q=0.9,image/webp,*/*;q=0.8"
            };
            Assert.Throws<FormatException>(() => MediaTypeHeaderValue.ParseList(inputs));
        }

        [Fact]
        public void TryParseList_WithSomeInvlaidValues_ReturnsFalse()
        {
            var inputs = new[]
            {
                "text/html,application/xhtml+xml, ignore-this, ignore/this",
                "application/xml;q=0.9,image/webp,*/*;q=0.8",
                "application/xml;q=0 4"
            };
            IList<MediaTypeHeaderValue> results;
            Assert.False(MediaTypeHeaderValue.TryParseList(inputs, out results));
        }

        [Theory]
        [InlineData("*/*;", "*/*")]
        [InlineData("text/*", "text/*")]
        [InlineData("text/*;", "*/*")]
        [InlineData("text/plain;", "text/plain")]
        [InlineData("text/plain", "text/*")]
        [InlineData("text/plain;", "*/*")]
        [InlineData("*/*;missingparam=4", "*/*")]
        [InlineData("text/*;missingparam=4;", "*/*;")]
        [InlineData("text/plain;missingparam=4", "*/*;")]
        [InlineData("text/plain;missingparam=4", "text/*")]
        [InlineData("text/plain;charset=utf-8", "text/plain;charset=utf-8")]
        [InlineData("text/plain;version=v1", "Text/plain;Version=v1")]
        [InlineData("text/plain;version=v1", "tExT/plain;version=V1")]
        [InlineData("text/plain;version=v1", "TEXT/PLAIN;VERSION=V1")]
        [InlineData("text/plain;charset=utf-8;foo=bar;q=0.0", "text/plain;charset=utf-8;foo=bar;q=0.0")]
        [InlineData("text/plain;charset=utf-8;foo=bar;q=0.0", "text/plain;foo=bar;q=0.0;charset=utf-8")] // different order of parameters
        [InlineData("text/plain;charset=utf-8;foo=bar;q=0.0", "text/*;charset=utf-8;foo=bar;q=0.0")]
        [InlineData("text/plain;charset=utf-8;foo=bar;q=0.0", "*/*;charset=utf-8;foo=bar;q=0.0")]
        public void IsSubsetOf_PositiveCases(string mediaType1, string mediaType2)
        {
            // Arrange
            var parsedMediaType1 = MediaTypeHeaderValue.Parse(mediaType1);
            var parsedMediaType2 = MediaTypeHeaderValue.Parse(mediaType2);

            // Act
            var isSubset = parsedMediaType1.IsSubsetOf(parsedMediaType2);

            // Assert
            Assert.True(isSubset);
        }

        [Theory]
        [InlineData("application/html", "text/*")]
        [InlineData("application/json", "application/html")]
        [InlineData("text/plain;version=v1", "text/plain;version=")]
        [InlineData("*/*;", "text/plain;charset=utf-8;foo=bar;q=0.0")]
        [InlineData("text/*;", "text/plain;charset=utf-8;foo=bar;q=0.0")]
        [InlineData("text/*;charset=utf-8;foo=bar;q=0.0", "text/plain;missingparam=4;")]
        [InlineData("*/*;charset=utf-8;foo=bar;q=0.0", "text/plain;missingparam=4;")]
        [InlineData("text/plain;charset=utf-8;foo=bar;q=0.0", "text/plain;missingparam=4;")]
        [InlineData("text/plain;charset=utf-8;foo=bar;q=0.0", "text/*;missingparam=4;")]
        [InlineData("text/plain;charset=utf-8;foo=bar;q=0.0", "*/*;missingparam=4;")]
        public void IsSubsetOf_NegativeCases(string mediaType1, string mediaType2)
        {
            // Arrange
            var parsedMediaType1 = MediaTypeHeaderValue.Parse(mediaType1);
            var parsedMediaType2 = MediaTypeHeaderValue.Parse(mediaType2);

            // Act
            var isSubset = parsedMediaType1.IsSubsetOf(parsedMediaType2);

            // Assert
            Assert.False(isSubset);
        }

        private void CheckValidParse(string input, MediaTypeHeaderValue expectedResult)
        {
            var result = MediaTypeHeaderValue.Parse(input);
            Assert.Equal(expectedResult, result);
        }

        private void CheckInvalidParse(string input)
        {
            Assert.Throws<FormatException>(() => MediaTypeHeaderValue.Parse(input));
        }

        private void CheckValidTryParse(string input, MediaTypeHeaderValue expectedResult)
        {
            MediaTypeHeaderValue result = null;
            Assert.True(MediaTypeHeaderValue.TryParse(input, out result));
            Assert.Equal(expectedResult, result);
        }

        private void CheckInvalidTryParse(string input)
        {
            MediaTypeHeaderValue result = null;
            Assert.False(MediaTypeHeaderValue.TryParse(input, out result));
            Assert.Null(result);
        }

        private static void AssertFormatException(string mediaType)
        {
            Assert.Throws<FormatException>(() => new MediaTypeHeaderValue(mediaType));
        }
    }
}
