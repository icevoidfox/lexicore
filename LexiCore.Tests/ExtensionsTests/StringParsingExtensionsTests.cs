using System;

using LexiCore.Extensions;

namespace LexiCore.Tests.ExtensionsTests;

public class StringParsingExtensionsTests
{
    #region ExtractDelimitedContent - char delimiters

    [Theory]
    [InlineData("Meow {World}!", '{', '}', 0, "World", 11)]
    [InlineData("Check {{{balance}}}", '{', '}', 0, "{{balance}}", 18)]
    [InlineData("No delimiters", '{', '}', 0, null, -1)]
    [InlineData("No open delimiter}", '{', '}', 0, null, -1)]
    [InlineData("No close {delimiter", '{', '}', 0, null, -1)]
    [InlineData("No {balanced {delimiter}", '{', '}', 0, null, -1)]
    [InlineData("Empty {} content", '{', '}', 0, "", 7)]
    [InlineData("Skip {this} check {that}", '{', '}', 10, "that", 23)]
    public void ExtractDelimitedContent_CharDelimiter_Works(
        string input,
        char openDelimiter,
        char closeDelimiter,
        int startIndex,
        string expectedContent,
        int expectedCloseDelimiterIndex
    )
    {
        var result = input.ExtractDelimitedContent(openDelimiter, closeDelimiter, startIndex);

        if (expectedContent == null)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Equal(expectedContent, result.Value.content);
            Assert.Equal(expectedCloseDelimiterIndex, result.Value.closeDelimiterIndex);
        }
    }

    #endregion

    #region ExtractDelimitedContent - string delimiters

    [Theory]
    [InlineData("Meow [<World>]!", "[<", ">]", 0, "World", 12)]
    [InlineData("Check [<[<[<balance>]>]>]", "[<", ">]", 0, "[<[<balance>]>]", 23)]
    [InlineData("No delimiters", "[<", ">]", 0, null, -1)]
    [InlineData("No open delimiter>]", "[<", ">]", 0, null, -1)]
    [InlineData("No close [<delimiter", "[<", ">]", 0, null, -1)]
    [InlineData("No [<balanced [<delimiter>]", "[<", ">]", 0, null, -1)]
    [InlineData("Empty [<>] content", "[<", ">]", 0, "", 8)]
    [InlineData("Skip [<this>] check [<that>]", "[<", ">]", 12, "that", 26)]
    public void ExtractDelimitedContent_StringDelimiter_Works(
        string input,
        string openDelimiter,
        string closeDelimiter,
        int startIndex,
        string expectedContent,
        int expectedCloseDelimiterIndex
    )
    {
        var result = input.ExtractDelimitedContent(openDelimiter, closeDelimiter, startIndex);

        if (expectedContent == null)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Equal(expectedContent, result.Value.content);
            Assert.Equal(expectedCloseDelimiterIndex, result.Value.closeDelimiterIndex);
        }
    }

    #endregion

    #region TryExtractDelimitedContent - char delimiters

    [Fact]
    public void TryExtractDelimitedContent_CharSpan_Identical_ReturnsCorrect()
    {
        string expectedContent = "inner";
        int expectedCloseDelimiterIndex = 16;

        ReadOnlySpan<char> text = $"Identical *{expectedContent}* *text**".AsSpan();

        Assert.True(text.TryExtractDelimitedContent('*', '*', out var content, out var closeDelimiterIndex));
        Assert.Equal(expectedContent, content.ToString());
        Assert.Equal(expectedCloseDelimiterIndex, closeDelimiterIndex);
    }

    #endregion

    #region TryExtractDelimitedContent - string delimiters

    [Fact]
    public void TryExtractDelimitedContent_StringSpan_Identical_ReturnsCorrect()
    {
        string expectedContent = "inner";
        int expectedCloseDelimiterIndex = 17;

        ReadOnlySpan<char> text = $"Identical **{expectedContent}** text**".AsSpan();

        Assert.True(text.TryExtractDelimitedContent("**", "**", out var content, out var closeDelimiterIndex));
        Assert.Equal(expectedContent, content.ToString());
        Assert.Equal(expectedCloseDelimiterIndex, closeDelimiterIndex);
    }

    [Fact]
    public void TryExtractDelimitedContent_AllEmptyDelimiter_Throws()
    {
        Assert.Throws<ArgumentException>(() => string.Empty.AsSpan().TryExtractDelimitedContent("", "", out _, out _));
    }

    [Fact]
    public void TryExtractDelimitedContent_OneEmptyDelimiter_Throws()
    {
        Assert.Throws<ArgumentException>(() => string.Empty.AsSpan().TryExtractDelimitedContent("*", "", out _, out _));
    }

    #endregion
}
