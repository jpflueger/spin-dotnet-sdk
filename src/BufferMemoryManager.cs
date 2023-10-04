using System.Buffers;

internal class BufferMemoryManager<T> : MemoryManager<T>
{
  private readonly nint _ptr;
  private readonly int _length;

  public BufferMemoryManager(nint ptr, int length)
  {
    _ptr = ptr;
    _length = length;
  }

  public unsafe override Span<T> GetSpan()
  {
    return new Span<T>((void*)_ptr, _length);
  }

  public unsafe override MemoryHandle Pin(int elementIndex = 0)
  {
    // no actual pinning occurs
    if (elementIndex < 0 || elementIndex >= _length)
      throw new ArgumentOutOfRangeException(nameof(elementIndex));
    return new MemoryHandle((void*)(_ptr + elementIndex));
  }

  public override void Unpin()
  {
    // Has no effect
  }

  protected override void Dispose(bool disposing)
  {
    // Has no effect
  }
}