using UnityEngine;
using Zenject;

[RequireComponent(typeof(AudioSource))]
public class PhoneRinger : MonoBehaviour
{
    [SerializeField] private AudioClip ringClip;
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = true;
        _source.clip = ringClip;
    }

    public void StartRinging()
    {
        if (!_source.isPlaying) _source.Play();
    }

    public void StopRinging()
    {
        if (_source.isPlaying) _source.Stop();
    }
}