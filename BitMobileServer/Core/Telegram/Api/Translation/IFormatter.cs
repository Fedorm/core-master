using Telegram.Math;

namespace Telegram.Translation
{
    public interface IFormatter<TResult>
    {
        TResult FromInt32(int input);
        TResult FromInt64(long input);
        TResult FromInt128(BigInteger input);
        TResult FromInt256(BigInteger input);
        TResult FromString(string input);
        TResult FromBytes(byte[] input);

        int ReadInt32(TResult input, ref int offset);
        long ReadInt64(TResult input, ref int offset);
        BigInteger ReadInt128(TResult input, ref int offset);
        BigInteger ReadInt256(TResult input, ref int offset);
        string ReadString(TResult input, ref int offset);
        byte[] ReadBytes(TResult input, ref int offset);
        byte[] ReadToEnd(TResult input, ref int offset);

        TResult Merge(TResult first, TResult second);
        TResult Merge(TResult first, TResult[] second);

        byte[] RawString(string input);
    }
}