namespace GZippy
{
    public interface ICompressionStrategy
    {
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);
    }
}