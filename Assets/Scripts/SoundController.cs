using System;
using UnityEngine;

public class SoundController : SingletonPersistent<SoundController>
{
    float _soundValue = 0;
    float _musicValue = 0;
    bool _isMute = false;
    public bool isMute { get => _isMute; set => _isMute = value; }
    public float SoundValue
    {
        get
        {
            return (_isMute) ? 0 : _soundValue;
        }
        set
        {
            _soundValue = (float)Math.Round(value, 2);
            Debug.Log($"new Value {_soundValue}");
            if (_soundValue > 100)
                _soundValue = 100;
            else if (_soundValue < 0)
                _soundValue = 0;
        }
        
    }
    public float MusicValue
    {
        get
        {
            return (_isMute) ? 0 : _musicValue;
        }
        set
        {
            _musicValue = (float)Math.Round(value, 2);
            Debug.Log($"new Value {_musicValue}");
            if (_musicValue > 100)
                _musicValue = 100;
            else if (_musicValue < 0)
                _musicValue = 0;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        bgm = GetComponent<AudioSource>();
    }
    [SerializeField] AudioSource bgm;
    void Update()
    {
        bgm.volume = MusicValue / 100;
    }
}