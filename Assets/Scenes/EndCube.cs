using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCube : MonoBehaviour
{
    [SerializeField] Collider2D RefCollider;
    [SerializeField] Camera MainCam;
    [SerializeField] AudioClip EndMusic;
    [SerializeField] GameObject WinSign;
    AudioSource MainCamAudio;

    bool TriggeredEnd = false;
    bool AudioFaded = false;

    private void Awake()
    {
        MainCamAudio = MainCam.GetComponent<AudioSource>();
        WinSign.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!TriggeredEnd) 
        {
            TriggeredEnd = true;
            StartCoroutine(CrossFadeMusic(MainCamAudio, EndMusic));
            WinSign.gameObject.SetActive(true);
        }
    }

    public IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        AudioFaded = true;
        yield break;
    }

    public IEnumerator CrossFadeMusic(AudioSource audioSource, AudioClip newAudio) 
    {
        StartCoroutine(StartFade(audioSource, 1f, 50f));
        yield return new WaitWhile(() => AudioFaded);
        
        audioSource.clip = newAudio;
        audioSource.Play();
        StartFade(audioSource, 1f, 100f);
    }
    
}
