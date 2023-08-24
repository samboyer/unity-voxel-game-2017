using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AmbientSoundController : MonoBehaviour {
    [Range(0, 1)]
    public float ambientMinVolume = .01f;

    [Range(0,1)]
    public float ambientMaxVolume = .05f;

    public float ambientMaxVolumeDistance;

    public float shoreMaxVolume = .1f;
    public float shoreMaxVolumeDistance = 200;


    public AudioMixerSnapshot mixerSnapshotNormal;
    public AudioMixerSnapshot mixerSnapshotUnderwater;

    Transform camera;
    AudioSource aSource;

    AudioSource shoreAudioSource;

    void Start () {
        camera = Camera.main.transform;
        aSource = GetComponent<AudioSource>();
        shoreAudioSource = transform.Find("SHORE").GetComponent<AudioSource>();
    }

    bool underwater;

	void Update () {
        float zoomDist = -camera.localPosition.z;
        aSource.volume = Mathf.Lerp(ambientMinVolume, ambientMaxVolume, zoomDist / ambientMaxVolumeDistance);

        if (underwater && camera.position.y >= 0)
            mixerSnapshotNormal.TransitionTo(0);

        if (!underwater && camera.position.y<0)
            mixerSnapshotUnderwater.TransitionTo(0);

        underwater = camera.position.y < 0;

        float shoreDist = Mathf.Abs(camera.position.y);
        shoreAudioSource.volume = Mathf.Lerp(shoreMaxVolume, 0, shoreDist / shoreMaxVolumeDistance);
    }
}
