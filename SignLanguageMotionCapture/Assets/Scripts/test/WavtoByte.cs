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

        // stream을 array 형태로 바꿈
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
        WriteInteger(stream, fileSize - 8);  // 파일 크기 - 8
        WriteString(stream, wave);
        WriteString(stream, fmt);
        WriteInteger(stream, 16);  // fmt 청크 크기
        WriteShort(stream, 1);  // 오디오 포맷 (1은 PCM)
        WriteShort(stream, (ushort)audioClip.channels);  // 채널 수 (모노: 1, 스테레오: 2)
        WriteInteger(stream, audioClip.frequency);  // 샘플링 레이트
        WriteInteger(stream, audioClip.frequency * BlockSize_16Bit * audioClip.channels);  // 바이트 속도
        WriteShort(stream, (ushort)(audioClip.channels * BlockSize_16Bit));  // 블록 크기
        WriteShort(stream, (ushort)(BlockSize_16Bit * 8));  // 비트 해상도
        WriteString(stream, data);
        WriteInteger(stream, fileSize - 44);  // 데이터 크기
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
