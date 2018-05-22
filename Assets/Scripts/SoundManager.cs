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
        ITEM_PICKUP,
        GUN_FIRE,
        DEMON_NOISE,
        DEMON_DEATH,
        NEKO_NOISE,
        SPACE_NEKO_DEATH,
        STAIRS
    }

    public enum MusicList
    {
        NONE,
        MENU_MUSIC,
        GAME_MUSIC_1,
        GAME_MUSIC_2
    }

    public struct LoopedSound
    {
        public AudioSource audioSource;
        public float timeUntilStop;
    }
    List<LoopedSound> loopedSoundList = new List<LoopedSound>();

    MusicList currentMusicPlaying = MusicList.NONE;

    List<AudioClip> listWalkSounds = new List<AudioClip>();
    List<AudioClip> listDemonNoiseSounds = new List<AudioClip>();

    [Header("VolumeSounds")]
    [SerializeField] AudioMixer audioMixer;

    [Header("Sounds")]
    [SerializeField] AudioClip clockClip;
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip getItemClip;
    [SerializeField] AudioClip gunFireClip;
    [SerializeField] AudioClip nekoNoiseClip;
    [SerializeField] AudioClip spaceNekoNoiseClip;
    [SerializeField] AudioClip stairsClip;

    [Header("WalkClips")]
    [SerializeField] AudioClip walkClip1;
    [SerializeField] AudioClip walkClip2;

    [Header("DemonClips")]
    [SerializeField] AudioClip DemonNoise1;
    [SerializeField] AudioClip DemonNoise2;
    [SerializeField] AudioClip DemonNoise3;

    [SerializeField] AudioClip DemonDeath;

    [Header("Musics")]
    [SerializeField] AudioClip menuMusicClip;
    [SerializeField] AudioClip gameMusicClip1;
    [SerializeField] AudioClip gameMusicClip2;

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

        listWalkSounds = new List<AudioClip>{walkClip1, walkClip2};

        listWalkSounds = new List<AudioClip>{DemonNoise1, DemonNoise2, DemonNoise3};
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

    public AudioSource PlaySound(SoundList sound, float timeToLoop = 0.0f)
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
                    int indexWalk = Random.Range(0, listWalkSounds.Count);
                    emitterAvailable.clip = listWalkSounds[indexWalk];
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;

                case SoundList.CLOCK:
                    emitterAvailable.clip = clockClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;

                case SoundList.DEATH:
                    emitterAvailable.clip = deathClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;

                case SoundList.ITEM_PICKUP:
                    emitterAvailable.clip = getItemClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;
                case SoundList.GUN_FIRE:
                    emitterAvailable.clip = gunFireClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;
                case SoundList.DEMON_NOISE:
                    int indexNoise = Random.Range(0, listDemonNoiseSounds.Count);
                    emitterAvailable.clip = listDemonNoiseSounds[indexNoise];
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;
                case SoundList.DEMON_DEATH:
                    emitterAvailable.clip = DemonDeath;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;
                case SoundList.NEKO_NOISE:
                    emitterAvailable.clip = nekoNoiseClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;
                case SoundList.SPACE_NEKO_DEATH:
                    emitterAvailable.clip = spaceNekoNoiseClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;
                case SoundList.STAIRS:
                    emitterAvailable.clip = stairsClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
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
            return emitterAvailable;
        }
        else
        {
            Debug.Log("no emitter available");
            return null;
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
                case MusicList.GAME_MUSIC_1:
                    musicEmitter.clip = gameMusicClip1;
                    musicEmitter.Play();
                    break;
                case MusicList.GAME_MUSIC_2:
                    musicEmitter.clip = gameMusicClip2;
                    musicEmitter.Play();
                    break;
                case MusicList.NONE:
                    musicEmitter.Stop();
                    break;
            }
            currentMusicPlaying = music;
        }
    }

    public void StopSound(AudioSource source)
    {
        source.Stop();
        foreach(LoopedSound looped in loopedSoundList)
        {
            if(looped.audioSource == source)
            {
                loopedSoundList.Remove(looped);
                break;
            }
        }
    }
}
