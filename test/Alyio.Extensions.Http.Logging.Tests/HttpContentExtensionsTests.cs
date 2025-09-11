// MIT License

using System.Text;

namespace Alyio.Extensions.Http.Logging.Tests;

public class HttpContentExtensionsTests
{
    [Fact]
    public async Task ReadRawMessageAsync_WhenContentIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        HttpContent? content = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => content!.ReadRawMessageAsync());
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("audio/mpeg")]
    [InlineData("video/mp4")]
    [InlineData("application/octet-stream")]
    [InlineData("application/pdf")]
    [InlineData("application/zip")]
    public async Task ReadRawMessageAsync_WhenNonTextBasedContent_ShouldReturnMimeTypeInBrackets(string mimeType)
    {
        // Arrange
        var content = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Equal($"[{mimeType}]", message);
        Assert.Same(content, returnedContent);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenContentWithoutMimeType_ShouldReturnUnknownInBrackets()
    {
        // Arrange
        var content = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Equal("[unknown]", message);
        Assert.Same(content, returnedContent);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenTextBasedContentWithSeekableStream_ShouldReturnContentText()
    {
        // Arrange
        var text = "Hello, World!";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Equal(text, message);
        Assert.Same(content, returnedContent);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenTextBasedContentWithNonSeekableStream_ShouldReturnContentTextAndNewContent()
    {
        // Arrange
        var text = "Hello, World!";
        var stream = new NonSeekableStream(Encoding.UTF8.GetBytes(text));
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Equal(text, message);
        Assert.NotSame(content, returnedContent);
        Assert.IsType<StreamContent>(returnedContent);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenTextBasedContentWithNonSeekableStream_ShouldPreserveHeaders()
    {
        // Arrange
        var text = "Hello, World!";
        var stream = new NonSeekableStream(Encoding.UTF8.GetBytes(text));
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        content.Headers.Add("Custom-Header", "Custom-Value");

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Equal(text, message);
        Assert.NotSame(content, returnedContent);
        Assert.True(returnedContent.Headers.Contains("Custom-Header"));
        Assert.Equal("Custom-Value", returnedContent.Headers.GetValues("Custom-Header").First());
        Assert.Equal("application/json", returnedContent.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenCancellationTokenProvided_ShouldCompleteSuccessfully()
    {
        // Arrange
        var text = "Hello, World!";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        using var cts = new CancellationTokenSource();

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync(cts.Token);

        // Assert
        Assert.Equal(text, message);
        Assert.Same(content, returnedContent);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenCancellationTokenIsCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var text = "Hello, World!";
        var stream = new NonSeekableStream(Encoding.UTF8.GetBytes(text));
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => content.ReadRawMessageAsync(cts.Token));
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/xml")]
    [InlineData("text/html")]
    [InlineData("text/plain")]
    public async Task ReadRawMessageAsync_WhenTextBasedMimeType_ShouldReturnContentText(string mimeType)
    {
        // Arrange
        var text = "Sample content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Equal(text, message);
        Assert.Same(content, returnedContent);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenEmptyTextContent_ShouldReturnEmptyString()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Equal(string.Empty, message);
        Assert.Same(content, returnedContent);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenMultipartFormDataContent_ShouldReturnMimeTypeInBrackets()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("value1"), "field1");
        content.Add(new StringContent("value2"), "field2");

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Contains("multipart/form-data", message);
        Assert.Same(content, returnedContent);
    }

    [Fact]
    public async Task ReadRawMessageAsync_WhenFormUrlEncodedContent_ShouldReturnContentText()
    {
        // Arrange
        var formData = new List<KeyValuePair<string, string>>
        {
            new("field1", "value1"),
            new("field2", "value2")
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var (message, returnedContent) = await content.ReadRawMessageAsync();

        // Assert
        Assert.Contains("field1=value1", message);
        Assert.Contains("field2=value2", message);
        Assert.Same(content, returnedContent);
    }

    /// <summary>
    /// A non-seekable stream implementation for testing purposes.
    /// </summary>
    private class NonSeekableStream : Stream
    {
        private readonly MemoryStream _innerStream;

        public NonSeekableStream(byte[] data)
        {
            _innerStream = new MemoryStream(data);
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => false; // This is the key - non-seekable
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position
        {
            get => _innerStream.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush() => _innerStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => _innerStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await _innerStream.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
