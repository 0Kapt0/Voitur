using UnityEngine;
using UnityEngine.Video;

public class FogVideo : MonoBehaviour
{
    public VideoClip fogVideo;
    public string textureProperty = "_MainTex";

    void Start()
    {
        VideoPlayer player = gameObject.AddComponent<VideoPlayer>();
        player.clip = fogVideo;
        player.isLooping = true;
        player.audioOutputMode = VideoAudioOutputMode.None;
        player.renderMode = VideoRenderMode.MaterialOverride;
        player.targetMaterialRenderer = GetComponent<Renderer>();
        player.targetMaterialProperty = textureProperty;
        player.Play();
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
