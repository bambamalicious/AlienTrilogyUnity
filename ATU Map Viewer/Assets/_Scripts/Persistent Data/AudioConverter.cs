using System;
using System.IO;
using UnityEngine;
public static class AudioConverter
{
    ///
    /// Converts a raw audio file into a Unity AudioClip.
    ///
    /// The path to the raw audio file.
    /// The sample rate of the audio (default is 11025 Hz).
    /// The number of audio channels (default is 1 for mono).
    /// A Unity AudioClip containing the converted audio, or null if the conversion fails.
    public static AudioClip ConvertRawToAudioClip(string filePath, int sampleRate = 11025, int channels = 1)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return null;
        }
        try
        {
            // Read raw audio data
            byte[] rawData = File.ReadAllBytes(filePath);
            // Convert raw data to float samples (assuming 8-bit PCM unsigned format)
            float[] audioData = new float[rawData.Length];
            for (int i = 0; i < rawData.Length; i++)
            {
                // Convert unsigned 8-bit (0-255) to signed float (-1.0 to 1.0)
                audioData[i] = (rawData[i] - 128) / 128f;
            }
            // Create an AudioClip
            AudioClip audioClip = AudioClip.Create(Path.GetFileNameWithoutExtension(filePath), audioData.Length / channels, channels, sampleRate, false);
            audioClip.SetData(audioData, 0);
            Debug.Log($"Successfully converted {filePath} to AudioClip.");
            return audioClip;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting raw audio to AudioClip: {ex.Message}");
            return null;
        }
    }
}