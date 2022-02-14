using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DinoRage.Enums;
using System;

namespace DinoRage.Classes
{
    public class DinoRage_Classes : MonoBehaviour
    {


        [Serializable]
        public class EVENT_SOUNDS
        {
            public string SFX_name;
            public SFX_DATA SFX_Data = null;
        }
        public List<EVENT_SOUNDS> SFX_list = new List<EVENT_SOUNDS>();


        [Serializable]
        public class SFX_DATA_HOLDER
        {
            public SFX_DATA sfx_used = null;
            public DinoRage_Enums.RANDOM_SETTINGS _is_it_random = DinoRage_Enums.RANDOM_SETTINGS.DONT_USE_RANDOM;
            public float _pre_pick_pitch = 1f;
            public float _pre_pick_volume = 1f;
        }


    }
}