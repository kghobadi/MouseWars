using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerSound : AudioHandler
{
    //audio stuff
    [Header("Speaker Bools")]
    [Tooltip("enables voice audio")]
    public bool hasVoiceAudio;
    public bool countsUp;
    public bool randomPitch;
    public int speakFreq = 4;
    
    //for random sound reading
    [Header("Speaker Sounds")]
    public AudioClip[] spokenSounds;

    //checks what kind of audio to play for the speaker 
    public void AudioCheck(string lineOfText, int letter)
    {
        if (hasVoiceAudio)
        {
            if (!countsUp)
            {
                if (letter % speakFreq == 0)
                    Speak(lineOfText[letter]);
            }
            else
            {
                if (!voices[voiceCounter].isPlaying)
                {
                    PlaySoundUp(spokenSounds);
                }
            }
        }
    }

    //check through our alphabet of sounds and play corresponding character
    public void Speak(char letter)
    {
        //cycle through audioSources for voice
        voiceCounter = CountUpArray(voiceCounter, voices.Length - 1);

        if (randomPitch)
        {
            PlayRandomSoundRandomPitch(spokenSounds, 1f);
        }
        else
        {
            PlayRandomSound(spokenSounds, 1f);
        }
    }

}
