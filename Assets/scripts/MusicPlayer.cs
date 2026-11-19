using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MusicPlayer : MonoBehaviour
{
    public string musicFolderPath = "Music";
    public float fadeDuration = 3f;
    public float masterVolume = 0.8f;

    public bool isRandomMusic = false;
    public bool repeatCurrentTrack = false;
    public bool loopPlaylist = true;

    private AudioClip[] tracks = new AudioClip[0];
    private AudioSource sourceA;
    private AudioSource sourceB;
    
    private int currentTrackIndex = 0;
    private bool isSourceAActive = true;
    private bool isFading = false;

    private void Start()
    {
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        
        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;
        sourceA.loop = false;
        sourceB.loop = false;

        StartCoroutine(LoadMusicAndPlay());
    }

    private IEnumerator LoadMusicAndPlay()
    {
        string basePath = Path.Combine(Application.streamingAssetsPath, musicFolderPath);
        
        if (!Directory.Exists(basePath))
            yield break;

        string[] files = Directory.GetFiles(basePath, "*.*");
        List<string> audioFiles = new List<string>();
        foreach (string file in files)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".ogg" || ext == ".mp3" || ext == ".wav")
                audioFiles.Add(file);
        }

        if (audioFiles.Count == 0)
            yield break;

        List<AudioClip> loadedTracks = new List<AudioClip>();
        for (int i = 0; i < audioFiles.Count; i++)
        {
            string filePath = audioFiles[i];
            string url = string.Concat("file:", "/", "/", filePath);
            
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                clip.name = Path.GetFileNameWithoutExtension(filePath);
                loadedTracks.Add(clip);
            }
        }

        tracks = loadedTracks.ToArray();

        if (tracks.Length == 0)
            yield break;

        if (repeatCurrentTrack || tracks.Length == 1)
        {
            PlayTrackOnSource(sourceA, currentTrackIndex);
            sourceA.loop = true;
            yield break;
        }

        PlayTrackOnSource(sourceA, currentTrackIndex);
    }

    private void Update()
    {
        if (tracks.Length <= 1 || isFading || repeatCurrentTrack)
            return;

        AudioSource activeSource = isSourceAActive ? sourceA : sourceB;

        if (activeSource.isPlaying)
        {
            if (activeSource.time >= activeSource.clip.length - fadeDuration)
                StartCoroutine(CrossFadeToNextTrack());
        }
        else
        {
            StartCoroutine(CrossFadeToNextTrack());
        }
    }

    private void PlayTrackOnSource(AudioSource source, int trackIndex)
    {
        source.clip = tracks[trackIndex];
        source.volume = 0f;
        source.Play();
        
        if (!isFading && source == sourceA && trackIndex == 0)
            source.volume = masterVolume;
    }

    private IEnumerator CrossFadeToNextTrack()
    {
        isFading = true;

        AudioSource fadingOutSource = isSourceAActive ? sourceA : sourceB;
        AudioSource fadingInSource = isSourceAActive ? sourceB : sourceA;

        int nextTrackIndex = GetNextTrackIndex();
        PlayTrackOnSource(fadingInSource, nextTrackIndex);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            fadingOutSource.volume = Mathf.Lerp(masterVolume, 0f, easedProgress);
            fadingInSource.volume = Mathf.Lerp(0f, masterVolume, easedProgress);

            yield return 0;
        }

        fadingOutSource.Stop();
        fadingOutSource.volume = 0f;
        fadingInSource.volume = masterVolume;

        isSourceAActive = !isSourceAActive;
        currentTrackIndex = nextTrackIndex;
        
        isFading = false;
    }

    private int GetNextTrackIndex()
    {
        if (isRandomMusic)
        {
            int nextIndex = Random.Range(0, tracks.Length);
            if (nextIndex == currentTrackIndex && tracks.Length > 1)
                nextIndex = (nextIndex + 1) % tracks.Length;
            return nextIndex;
        }

        int sequentialIndex = currentTrackIndex + 1;
        if (sequentialIndex >= tracks.Length)
            sequentialIndex = loopPlaylist ? 0 : tracks.Length - 1;

        return sequentialIndex;
    }

    public void NextTrack()
    {
        if (tracks.Length <= 1 || isFading)
            return;

        StopAllCoroutines();
        isFading = false;
        StartCoroutine(CrossFadeToNextTrack());
    }

    public void SetVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (!isFading && tracks.Length > 0)
        {
            AudioSource activeSource = isSourceAActive ? sourceA : sourceB;
            activeSource.volume = masterVolume;
        }
    }
}
