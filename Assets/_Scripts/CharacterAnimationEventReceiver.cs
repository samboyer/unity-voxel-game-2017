using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationEventReceiver : MonoBehaviour {

    CharacterInteractionController interactionController;

    AudioSource aSource;

    public AudioClip[] sfxFootstepGrass;

    public AudioClip sfxPunchWhoosh;

    private void Start()
    {
        interactionController = transform.parent.GetComponent<CharacterInteractionController>();
        aSource = GetComponent<AudioSource>();
    }

    void FOOTSTEP(int i)
    {
        aSource.PlayOneShot(sfxFootstepGrass[Random.Range(0, sfxFootstepGrass.Length)]);
    }

	void PUNCH(int i)
    {
        //aSource.PlayOneShot(sfxFootstepGrass[Random.Range(0, sfxFootstepGrass.Length)]);
        interactionController.DoPunch();
    }

    void PUNCHWHOOSH(int i)
    {
        aSource.PlayOneShot(sfxPunchWhoosh);
    }
}
