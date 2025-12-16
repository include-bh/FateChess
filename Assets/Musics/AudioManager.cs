using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> backgroundMusicClips;
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        if (backgroundMusicClips == null || backgroundMusicClips.Count == 0)
        {
            Debug.LogError("No background music clips assigned!");
            return;
        }

        PlayRandomBackgroundMusic();
    }

    void PlayRandomBackgroundMusic()
    {
        int index = Random.Range(0, backgroundMusicClips.Count);
        AudioClip clip = backgroundMusicClips[index];

        audioSource.clip = clip;
        audioSource.loop = true;  // 循环播放
        audioSource.Play();
    }
}