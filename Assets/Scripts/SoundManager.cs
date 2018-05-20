using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    List<AudioSource> emitters = new List<AudioSource>();

    public enum SoundList
    {
        WALK,
        CLOCK,
        DEATH,
        MENU_SELECTION,
        MENU_VALIDATION,
        WIN_MUSIC,
        LOSE_MUSIC
    }

    public enum MusicList
    {
        NONE,
        MENU_MUSIC,
        GAME_MUSIC
    }

    public struct LoopedSound
    {
        public AudioSource audioSource;
        public float timeUntilStop;
    }
    List<LoopedSound> loopedSoundList = new List<LoopedSound>();

    MusicList currentMusicPlaying = MusicList.NONE;

    List<AudioClip> listWalkSounds = new List<AudioClip>();
    List<AudioClip> listCarSounds = new List<AudioClip>();

    [Header("VolumeSounds")]
    [SerializeField] AudioMixer audioMixer;

    [Header("Sounds")]
    [SerializeField] AudioClip clockClip;
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip menuSelection;
    [SerializeField] AudioClip menuValidation;

    [Header("WalkClips")]
    [SerializeField] AudioClip walkClip1;
    [SerializeField] AudioClip walkClip2;
    [SerializeField] AudioClip walkClip3;
    

    [Header("Musics")]
    [SerializeField] AudioClip menuMusicClip;
    [SerializeField] AudioClip gameMusicClip;
    [SerializeField] AudioClip winMusicClip; // Not looped
    [SerializeField] AudioClip loseMusicClip; // Not looped

    [Header("Emmiters")]
    [SerializeField] GameObject emitterPrefab;
    [SerializeField] int emitterNumber;
    [SerializeField] AudioSource musicEmitter;
   

    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(gameObject);

        for(int i = 0; i <= emitterNumber;i++)
        {
            GameObject audioObject = Instantiate(emitterPrefab, emitterPrefab.transform.position, emitterPrefab.transform.rotation);
            emitters.Add(audioObject.GetComponent<AudioSource>());
            DontDestroyOnLoad(audioObject);
        }

        listWalkSounds = new List<AudioClip>{walkClip1,
                                    walkClip2,
                                    walkClip3
        };
    }

    private void Update()
    {
        foreach(LoopedSound loopedSound in loopedSoundList)
        {
            if(Utility.IsOver(loopedSound.timeUntilStop))
            {
                loopedSound.audioSource.Stop();
                loopedSoundList.Remove(loopedSound);
                break;
            }
        }
    }

    public void PlaySound(SoundList sound, float timeToLoop = 0.0f)
    {
        AudioSource emitterAvailable = null;

        foreach(AudioSource emitter in emitters)
        {
            if(!emitter.isPlaying)
            {
                emitterAvailable = emitter;
            }
        }

        if (emitterAvailable != null)
        {
            emitterAvailable.loop = false;
            switch (sound)
            {
                case SoundList.WALK:
                    int random = Random.Range(0, listWalkSounds.Count);
                    emitterAvailable.clip = listWalkSounds[random];
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Footsteps")[0];
                    break;

                case SoundList.CLOCK:
                    emitterAvailable.clip = clockClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Clock")[0];
                    break;

                case SoundList.DEATH:
                    emitterAvailable.clip = deathClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Death")[0];
                    break; 

                case SoundList.MENU_SELECTION:
                    emitterAvailable.clip = menuSelection;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Menu")[0];
                    break;

                case SoundList.MENU_VALIDATION:
                    emitterAvailable.clip = menuValidation;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Menu")[0];
                    break;
                case SoundList.WIN_MUSIC:
                    musicEmitter.clip = winMusicClip;
                    musicEmitter.Play();
                    break;
                case SoundList.LOSE_MUSIC:
                    musicEmitter.clip = loseMusicClip;
                    musicEmitter.Play();
                    break;
            }

            if(timeToLoop > 0.0f)
            {
                emitterAvailable.loop = true;
                LoopedSound newLoopSound = new LoopedSound
                {
                    audioSource = emitterAvailable,
                    timeUntilStop = Utility.StartTimer(timeToLoop)
                };
                loopedSoundList.Add(newLoopSound);  
            }
            
            emitterAvailable.Play();
        }
        else
        {
            Debug.Log("no emitter available");
        }        
    }

    public void PlayMusic(MusicList music)
    {
        if (currentMusicPlaying != music)
        {
            musicEmitter.loop = true;

            switch (music)
            {
                case MusicList.MENU_MUSIC:
                    musicEmitter.clip = menuMusicClip;
                    musicEmitter.Play();
                    break;
                case MusicList.GAME_MUSIC:
                    musicEmitter.clip = gameMusicClip;
                    musicEmitter.Play();
                    break;
                case MusicList.NONE:
                    musicEmitter.Stop();
                    break;
            }
            currentMusicPlaying = music;
        }
    }
}
