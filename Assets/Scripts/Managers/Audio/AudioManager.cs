using UnityEngine;
using NaughtyAttributes;

public class AudioManager : PersistentSingleton<AudioManager>
{
    [Header("-------AudioSource-------")]
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource SFX;

    [Header("-------AudioClipSFX-------")]
    public AudioClip clic1;
    public AudioClip clic2;
    public AudioClip clic3;

    public AudioClip coqSound;
    public AudioClip carsStartSound;

    public AudioClip mergeItem;

    public AudioClip inChest;
    

    [Header("-------AudioClipMusic-------")]
    public AudioClip MusicMainMenuAndStore;
    public AudioClip MusicWarehouse;


    [Foldout("AudioClipSFX/Step")]
    public AudioClip step1;
    [Foldout("AudioClipSFX/Step")]
    public AudioClip step2;
    [Foldout("AudioClipSFX/Step")]
    public AudioClip step3;

    [Foldout("AudioClipSFX/Trunk")]
    public AudioClip putInTrunk1;

    [Foldout("AudioClipSFX/Trunk")]
    public AudioClip putInTrunk2;

    [Foldout("AudioClipSFX/Trunk")]
    public AudioClip putInTrunk3;

    [Foldout("AudioClipSFX/Trunk")]
    public AudioClip putInTrunk4;



    void Start()
    {
        PlayMusic(MusicMainMenuAndStore);
    }

    public void PlaySFX(AudioClip clip)
    {
        SFX.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        music.clip = musicClip;
        music.loop = true;
        music.mute = false;
        music.Play();
    }

    public void StopMusic()
    {
        music.mute = true;
    }

    public void PlayClic()
    {
        var id = Random.Range(1,4);
        switch (id)
        {
            case 1:
                PlaySFX(clic1);
                break;
            case 2:
                PlaySFX(clic2);
                break;
            case 3:
                PlaySFX(clic3);
                break;
        }
    }

    public void PlayStep()
    {
        int randomIndex = Random.Range(0, 3);
        AudioClip stepClip = null;

        switch (randomIndex)
        {
            case 0:
                stepClip = step1;
                break;
            case 1:
                stepClip = step2;
                break;
            case 2:
                stepClip = step3;
                break;
        }

        if (stepClip != null)
        {
            PlaySFX(stepClip);
        }
    }

    public void PlayPutInTrunk()
    {
        int randomIndex = Random.Range(0, 4);
        AudioClip putInTrunk = null;

        switch (randomIndex)
        {
            case 0:
                putInTrunk = putInTrunk1;
                break;
            case 1:
                putInTrunk = putInTrunk2;
                break;
            case 2:
                putInTrunk = putInTrunk3;
                break;
            case 3:
                putInTrunk = putInTrunk4;
                break;
        }

        if (putInTrunk != null)
        {
            PlaySFX(putInTrunk);
        }
    }
}
