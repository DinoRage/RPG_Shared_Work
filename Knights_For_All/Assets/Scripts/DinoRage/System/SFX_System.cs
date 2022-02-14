using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DinoRage.Classes;
using DinoRage.Enums;

namespace DinoRage.Classes
{
public class SFX_System : MonoBehaviour
{
    private AudioSource m_AudioSource = null;
    // first try using a list from somewhere else
    public DinoRage_Classes.EVENT_SOUNDS[] SFX_list;           
         private void OnEnable()
         {
            m_AudioSource = GetComponent<AudioSource>();
            if (m_AudioSource == null)
            {
                Debug.LogError("No AudioSource found");
            }
         }
        public void Play_SFX(string _SFX_name)
        {
            // goes thru each sfx
            for (int _SFX_Picked = 0; _SFX_Picked < SFX_list.Length; _SFX_Picked++)
            {
                // check the name of the SFX
                if (_SFX_name == SFX_list[_SFX_Picked].SFX_name)
                {
                    int _sound_picked = Random.Range(1, SFX_list[_SFX_Picked].SFX_Data.SFX_lists.Count);
                    // adds sound to audio source
                    // adds the random settings

                    switch (SFX_list[_SFX_Picked].SFX_Data._SFX_info._is_it_random)
                    {
                        case DinoRage_Enums.RANDOM_SETTINGS.DONT_USE_RANDOM:
                            m_AudioSource.volume = SFX_list[_SFX_Picked].SFX_Data._SFX_info._pre_pick_volume;
                            m_AudioSource.pitch = SFX_list[_SFX_Picked].SFX_Data._SFX_info._pre_pick_pitch;
                            break;
                        case DinoRage_Enums.RANDOM_SETTINGS.USE_RANDOM:
                            m_AudioSource.volume = Random.Range(SFX_list[_SFX_Picked].SFX_Data.SFX_lists[_sound_picked]._random_volume.x, SFX_list[_SFX_Picked].SFX_Data.SFX_lists[_sound_picked]._random_volume.y);
                            m_AudioSource.pitch = Random.Range(SFX_list[_SFX_Picked].SFX_Data.SFX_lists[_sound_picked]._random_pitch.x, SFX_list[_SFX_Picked].SFX_Data.SFX_lists[_sound_picked]._random_pitch.y);
                            break;
                    }
                    //Play the sound once
                    m_AudioSource.PlayOneShot(SFX_list[_SFX_Picked].SFX_Data.SFX_lists[_sound_picked].sound);
                    return;
                }
            }
        }
        
        public void _Play_Advance_SFX(SFX_DATA sfx_used)
        {
            int _sound_picked = Random.Range(1, sfx_used.SFX_lists.Count);
            // picks if it need to use value recived or random values set on each audio
            switch (sfx_used._SFX_info._is_it_random)
            {
                case DinoRage_Enums.RANDOM_SETTINGS.DONT_USE_RANDOM:
                    m_AudioSource.volume = sfx_used._SFX_info._pre_pick_volume;
                    m_AudioSource.pitch = sfx_used._SFX_info._pre_pick_pitch;
                    break;
                case DinoRage_Enums.RANDOM_SETTINGS.USE_RANDOM:
                    m_AudioSource.volume = Random.Range(sfx_used.SFX_lists[_sound_picked]._random_volume.x, sfx_used.SFX_lists[_sound_picked]._random_volume.y);
                    m_AudioSource.pitch = Random.Range(sfx_used.SFX_lists[_sound_picked]._random_pitch.x, sfx_used.SFX_lists[_sound_picked]._random_pitch.y);
                    break;
            }
            //Play the sound once
            m_AudioSource.PlayOneShot(sfx_used.SFX_lists[_sound_picked].sound);
            return;
        }
        





    }
}