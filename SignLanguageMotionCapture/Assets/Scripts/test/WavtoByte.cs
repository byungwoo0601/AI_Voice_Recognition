using System;
using System.IO;
using UnityEngine;
using System.Text;

public static class WavtoByte
{
    private const int BlockSize_16Bit = 2;

    public static byte[] ConvertAudioClipToWav(AudioClip audioClip)
    {
        MemoryStream stream = new MemoryStream();
        const int headerSize = 44;
        ushort bitDepth = 16;

        int fileSize = audioClip.samples * BlockSize_16Bit + headerSize;

        WriteFileHeader(ref stream, fileSize, audioClip);
        WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
        WriteFileData(ref stream, audioClip, bitDepth);

        // stream�� array ���·� �ٲ�
        byte[] bytes = stream.ToArray();

        return bytes;
    }

    public static void WriteFileHeader(ref MemoryStream stream, int fileSize, AudioClip audioClip)
    {
        string riff = "RIFF";
        string wave = "WAVE";
        string fmt = "fmt ";
        string data = "data";

        WriteString(stream, riff);
        WriteInteger(stream, fileSize - 8);  // ���� ũ�� - 8
        WriteString(stream, wave);
        WriteString(stream, fmt);
        WriteInteger(stream, 16);  // fmt ûũ ũ��
        WriteShort(stream, 1);  // ����� ���� (1�� PCM)
        WriteShort(stream, (ushort)audioClip.channels);  // ä�� �� (���: 1, ���׷���: 2)
        WriteInteger(stream, audioClip.frequency);  // ���ø� ����Ʈ
        WriteInteger(stream, audioClip.frequency * BlockSize_16Bit * audioClip.channels);  // ����Ʈ �ӵ�
        WriteShort(stream, (ushort)(audioClip.channels * BlockSize_16Bit));  // ��� ũ��
        WriteShort(stream, (ushort)(BlockSize_16Bit * 8));  // ��Ʈ �ػ�
        WriteString(stream, data);
        WriteInteger(stream, fileSize - 44);  // ������ ũ��
    }

    public static void WriteFileFormat(ref MemoryStream stream, int channels, int frequency, ushort bitDepth)
    {
        WriteShort(stream, (ushort)channels);
        WriteInteger(stream, frequency);
        WriteInteger(stream, frequency * BlockSize_16Bit * channels);
        WriteShort(stream, (ushort)(channels * BlockSize_16Bit));
        WriteShort(stream, bitDepth);
    }

    public static void WriteFileData(ref MemoryStream stream, AudioClip audioClip, ushort bitDepth)
    {
        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);

        for (int i = 0; i < samples.Length; i++)
        {
            ushort value = (ushort)(samples[i] * ushort.MaxValue);
            WriteShort(stream, value);
        }
    }

    public static void WriteString(MemoryStream stream, string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    public static void WriteShort(MemoryStream stream, ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    public static void WriteInteger(MemoryStream stream, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}
