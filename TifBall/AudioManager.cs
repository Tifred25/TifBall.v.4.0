using Microsoft.Xna.Framework.Audio;
using NVorbis;
using System;
using System.Collections.Generic;
using System.IO;

namespace TifBall;

internal sealed class AudioManager : IDisposable
{
    private readonly Dictionary<string, SoundEffect> _soundEffects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _soundFallbacks = new(StringComparer.OrdinalIgnoreCase);
    private SoundEffect? _backgroundMusicEffect;
    private SoundEffectInstance? _backgroundMusicInstance;
    private string _backgroundMusicStatus = "Non chargee";
    private bool _soundsEnabled = true;
    private bool _musicEnabled = true;

    public string BackgroundMusicStatus => _backgroundMusicStatus;
    public bool SoundsEnabled => _soundsEnabled;
    public bool MusicEnabled => _musicEnabled;

    public void LoadEmbeddedSounds(params string[] soundFileNames)
    {
        foreach (string soundFileName in soundFileNames)
        {
            TryLoadEmbeddedSound(soundFileName);
        }

        if (!_soundEffects.ContainsKey("Crunch") && _soundEffects.ContainsKey("Pop"))
        {
            _soundFallbacks["Crunch"] = "Pop";
            WriteDiagnosticLog("Sound fallback registered: Crunch -> Pop");
        }
    }

    public void Play(string soundName, float volume = 1f)
    {
        if (!_soundsEnabled)
        {
            return;
        }

        SoundEffect? soundEffect = null;
        if (!_soundEffects.TryGetValue(soundName, out SoundEffect? loadedSoundEffect)
            && _soundFallbacks.TryGetValue(soundName, out string? fallbackName))
        {
            _soundEffects.TryGetValue(fallbackName, out soundEffect);
        }
        else
        {
            soundEffect = loadedSoundEffect;
        }

        if (soundEffect != null)
        {
            soundEffect.Play(volume, 0f, 0f);
        }
    }

    public bool TryLoadBackgroundMusic(string musicFileName)
    {
        try
        {
            DisposeBackgroundMusic();
            using Stream stream = EmbeddedAssetLoader.OpenRequired(Path.Combine("Assets", musicFileName));
            _backgroundMusicEffect = CreateSoundEffectFromOggStream(stream);
            _backgroundMusicInstance = _backgroundMusicEffect.CreateInstance();
            _backgroundMusicStatus = "Chargee: " + musicFileName;
            WriteDiagnosticLog("Background music loaded from embedded resource: " + musicFileName);
            return true;
        }
        catch (Exception ex)
        {
            DisposeBackgroundMusic();
            _backgroundMusicStatus = "Echec chargement: " + musicFileName;
            WriteDiagnosticLog("Background music load failed for embedded resource " + musicFileName + Environment.NewLine + ex);
            return false;
        }
    }

    public bool TryLoadBackgroundMusicFromCandidates(params string[] musicFileNames)
    {
        foreach (string musicFileName in musicFileNames)
        {
            if (TryLoadBackgroundMusic(musicFileName))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryPlayBackgroundMusic(float volume = 0.6f, bool isRepeating = true)
    {
        if (!_musicEnabled)
        {
            _backgroundMusicStatus = "Musique desactivee";
            return false;
        }

        if (_backgroundMusicInstance == null)
        {
            _backgroundMusicStatus = "Aucune musique chargee";
            return false;
        }

        try
        {
            _backgroundMusicInstance.Stop();
            _backgroundMusicInstance.Volume = Math.Clamp(volume, 0f, 1f);
            _backgroundMusicInstance.IsLooped = isRepeating;
            _backgroundMusicInstance.Play();
            _backgroundMusicStatus = "Lecture en cours";
            WriteDiagnosticLog("Background music playback started.");
            return true;
        }
        catch (Exception ex)
        {
            StopBackgroundMusic();
            _backgroundMusicStatus = "Echec lecture";
            WriteDiagnosticLog("Background music playback failed." + Environment.NewLine + ex);
            return false;
        }
    }

    public void StopBackgroundMusic()
    {
        try
        {
            _backgroundMusicInstance?.Stop();
        }
        catch
        {
        }
    }

    public void SetSoundsEnabled(bool enabled)
    {
        _soundsEnabled = enabled;
    }

    public void SetMusicEnabled(bool enabled)
    {
        _musicEnabled = enabled;
        if (!enabled)
        {
            StopBackgroundMusic();
        }
    }

    private static void WriteDiagnosticLog(string message)
    {
        string logPath = Path.Combine(AppContext.BaseDirectory, "monogame-audio.log");
        string logEntry =
            "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " +
            message +
            Environment.NewLine;

        File.AppendAllText(logPath, logEntry);
    }

    private void TryLoadEmbeddedSound(string soundFileName)
    {
        string soundKey = Path.GetFileNameWithoutExtension(soundFileName);

        try
        {
            using Stream stream = EmbeddedAssetLoader.OpenRequired(Path.Combine("Assets", soundFileName));
            _soundEffects[soundKey] = SoundEffect.FromStream(stream);
        }
        catch (Exception ex)
        {
            WriteDiagnosticLog("Sound load failed for embedded resource " + soundFileName + Environment.NewLine + ex);
        }
    }

    private static SoundEffect CreateSoundEffectFromOggStream(Stream stream)
    {
        using VorbisReader vorbisReader = new(stream, false);
        List<byte> pcmBytes = new();
        float[] sampleBuffer = new float[4096];

        while (true)
        {
            int samplesRead = vorbisReader.ReadSamples(sampleBuffer, 0, sampleBuffer.Length);
            if (samplesRead == 0)
            {
                break;
            }

            for (int i = 0; i < samplesRead; i++)
            {
                float clampedSample = Math.Clamp(sampleBuffer[i], -1f, 1f);
                short pcmSample = (short)Math.Round(clampedSample * short.MaxValue, MidpointRounding.AwayFromZero);
                pcmBytes.Add((byte)(pcmSample & 0xFF));
                pcmBytes.Add((byte)((pcmSample >> 8) & 0xFF));
            }
        }

        AudioChannels channels = vorbisReader.Channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo;
        return new SoundEffect(pcmBytes.ToArray(), vorbisReader.SampleRate, channels);
    }

    private void DisposeBackgroundMusic()
    {
        _backgroundMusicInstance?.Dispose();
        _backgroundMusicInstance = null;
        _backgroundMusicEffect?.Dispose();
        _backgroundMusicEffect = null;
    }

    public void Dispose()
    {
        StopBackgroundMusic();
        DisposeBackgroundMusic();

        foreach (SoundEffect soundEffect in _soundEffects.Values)
        {
            soundEffect.Dispose();
        }

        _soundEffects.Clear();
    }
}
