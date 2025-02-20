using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    private VideoPlayer _videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        _videoPlayer = GetComponent<VideoPlayer>();

        var videoName = Managers.Data.MusicList.FirstOrDefault(music => music.clip == AudioManager.Instance.CurrentBGM.ToString())?.video;

        PlayVideo(videoName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayVideo(string videoName)
    {
        _videoPlayer.clip = Managers.Resource.Load<VideoClip>($"Video/{videoName}");
        Debug.Log($"Video {videoName} loaded");
        _videoPlayer.SetDirectAudioMute(0, true);
        _videoPlayer.Play();
    }
}
