using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    AudioSource _audioSource = null;
    [SerializeField]
    AssetReference[] _soundtracks = null;
    int _playingSoundtrack = 0;
    
    IEnumerator Start()
    {
        while (true)
        {
            var currentMusicHandle = _soundtracks[_playingSoundtrack].LoadAssetAsync<AudioClip>();
            yield return currentMusicHandle;

            var newAudioSource = currentMusicHandle.Result;
            _audioSource.clip = newAudioSource;
            _audioSource.Play();

            yield return new WaitUntil(() => _audioSource.isPlaying == false);

            _audioSource.clip = null;
            Addressables.Release(currentMusicHandle);

            _playingSoundtrack = (_playingSoundtrack + 1) % _soundtracks.Length;
        }
    }
}
