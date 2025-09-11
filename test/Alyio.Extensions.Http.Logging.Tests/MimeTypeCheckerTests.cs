// MIT License

using Alyio.Extensions.Http.Logging;
using Xunit;

namespace Alyio.Extensions.Http.Logging.Tests;

public class MimeTypeCheckerTests
{
    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("audio/mpeg")]
    [InlineData("video/mp4")]
    [InlineData("application/octet-stream")]
    [InlineData("application/pdf")]
    [InlineData("application/zip")]
    public void IsTextBased_ShouldReturnFalse_ForNonTextMimeTypes(string mimeType)
    {
        Assert.False(MimeTypeChecker.IsTextBased(mimeType));
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/json")]
    [InlineData("application/xml")]
    [InlineData("application/javascript")]
    [InlineData("text/html; charset=utf-8")]
    [InlineData("application/ld+json")]
    [InlineData("application/atom+xml")]
    [InlineData("image/svg+xml")]
    public void IsTextBased_ShouldReturnTrue_ForTextMimeTypes(string mimeType)
    {
        Assert.True(MimeTypeChecker.IsTextBased(mimeType));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsTextBased_ShouldReturnFalse_ForNullOrWhiteSpace(string mimeType)
    {
        Assert.False(MimeTypeChecker.IsTextBased(mimeType));
    }
}
