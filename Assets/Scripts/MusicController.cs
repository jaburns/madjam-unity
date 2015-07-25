using UnityEngine;

public class MusicController : MonoBehaviour
{
    static public MusicController Instance { get {
        return GameObject.FindObjectOfType<MusicController>();
    } }

    public AudioSource[] Musics;

    int _prevIndex = -1;
    int _thisIndex = -1;

    float _t;
    bool _fading;

    void Awake()
    {
        var music = GameObject.Find("TheMusic");
        if (music && music != gameObject) {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);
            reset();
        }
    }

    void reset()
    {
        foreach (var src in Musics) {
            src.volume = 0;
        }
    }

    public void SetMusic(int index)
    {
        _fading = true;
        _t = 0;
        _thisIndex = index;
    }

    void FixedUpdate()
    {
        if (!_fading) return;

        _t += 0.03f;
        if (_t >= 1) {
            _t = 1;
            _fading = false;
        }

        if (_thisIndex >= 0) Musics[_thisIndex].volume = _t;
        if (_prevIndex >= 0) Musics[_prevIndex].volume = 1 - _t;

        if (!_fading) {
            _prevIndex = _thisIndex;
        }
    }
}
