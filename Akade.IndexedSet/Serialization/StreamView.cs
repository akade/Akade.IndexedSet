namespace Akade.IndexedSet.Serialization;
internal class PartialReadOnlyStream(Stream underlyingStream) : Stream
{
    private readonly Stream _underlyingStream = underlyingStream;
    private long _length = 0;
    private long _position = 0;
    private long _remaining = 0;

    public void SetSegment(long length)
    {
        _length = length;
        _position = 0;
        _remaining = length;
    }

    public override bool CanRead => _underlyingStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
        _underlyingStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return _underlyingStream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        long remaining = _remaining;
        if (remaining == 0)
        {
            return 0;
        }

        int toRead = buffer.Length;
        if (toRead > remaining)
        {
            toRead = (int)remaining;
        }

        int read = _underlyingStream.Read(buffer[..toRead]);
        _position += read;
        _remaining = remaining - read;
        return read;
    }

    public override int ReadByte()
    {
        if (_remaining == 0)
        {
            return -1;
        }

        int value = _underlyingStream.ReadByte();
        if (value != -1)
        {
            _position++;
            _remaining--;
        }

        return value;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        long remaining = _remaining;
        if (remaining == 0)
        {
            return 0;
        }

        int toRead = buffer.Length;
        if (toRead > remaining)
        {
            toRead = (int)remaining;
        }

        int read = await _underlyingStream.ReadAsync(buffer[..toRead], cancellationToken).ConfigureAwait(false);
        _position += read;
        _remaining = remaining - read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
