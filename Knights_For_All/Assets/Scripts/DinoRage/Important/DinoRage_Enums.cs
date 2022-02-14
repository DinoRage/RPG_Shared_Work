using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DinoRage.Classes;


namespace DinoRage.Enums
{
    public class DinoRage_Enums : MonoBehaviour
    {
        public enum RANDOM_SETTINGS
        { DONT_USE_RANDOM, USE_RANDOM }

        public enum GENDER
        {
            MALE, FEMALE
        }
        public enum Animation_Type
        {
            TRIGGER, BOOL, FLOAT, INT
        }
        public enum What_To_Call_On_NPC
        {
            SFX, Shape_Shift, Move_Collider, Weather_Change, Slow_Time, Animation_Change, Time_Of_Day, Advance_SFX
        }






    }
}