using System;
using System.IO;

public static class WavUtility
{
    public static byte[] ToWav(float[] samples, int channels, int sampleRate, int bitDepth)
    {
        MemoryStream stream = new MemoryStream();
        WriteWavHeader(stream, channels, sampleRate, bitDepth, samples.Length);
        ConvertAndWrite(stream, samples);
        return stream.ToArray();
    }

    private static void WriteWavHeader(MemoryStream stream, int channels, int sampleRate, int bitDepth, int dataLength)
    {
        stream.Seek(0, SeekOrigin.Begin);
        WriteString(stream, "RIFF");
        WriteInt(stream, 36 + dataLength);
        WriteString(stream, "WAVE");
        WriteString(stream, "fmt ");
        WriteInt(stream, 16);
        WriteShort(stream, 1);
        WriteShort(stream, (short)channels);
        WriteInt(stream, sampleRate);
        WriteInt(stream, sampleRate * channels * bitDepth / 8);
        WriteShort(stream, (short)(channels * bitDepth / 8));
        WriteShort(stream, (short)bitDepth);
        WriteString(stream, "data");
        WriteInt(stream, dataLength);
    }

    private static void WriteString(MemoryStream stream, string s)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteShort(MemoryStream stream, short value)
    {
        stream.WriteByte((byte)(value & 0xff));
        stream.WriteByte((byte)((value >> 8) & 0xff));
    }

    private static void WriteInt(MemoryStream stream, int value)
    {
        stream.WriteByte((byte)(value & 0xff));
        stream.WriteByte((byte)((value >> 8) & 0xff));
        stream.WriteByte((byte)((value >> 16) & 0xff));
        stream.WriteByte((byte)((value >> 24) & 0xff));
    }

    private static void ConvertAndWrite(MemoryStream stream, float[] samples)
    {
        Int16[] intData = new Int16[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767.0f);
        }
        byte[] bytesData = new byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        stream.Write(bytesData, 0, bytesData.Length);
    }


    private static WavHeader ReadWavHeader(MemoryStream stream)
    {
        WavHeader header = new WavHeader();
        stream.Seek(22, SeekOrigin.Begin);
        header.channels = stream.ReadByte();
        header.frequency = ReadInt(stream);
        stream.Seek(34, SeekOrigin.Begin);
        header.bitDepth = stream.ReadByte();
        header.bytes = (int)stream.Length - 44;
        header.headerSize = 44;
        header.sampleCount = header.bytes / 2;
        return header;
    }

    private static int ReadInt(MemoryStream stream)
    {
        byte[] bytes = new byte[4];
        stream.Read(bytes, 0, 4);
        return BitConverter.ToInt32(bytes, 0);
    }

    private class WavHeader
    {
        public int channels;
        public int frequency;
        public int bitDepth;
        public int bytes;
        public int headerSize;
        public int sampleCount;
    }
}
