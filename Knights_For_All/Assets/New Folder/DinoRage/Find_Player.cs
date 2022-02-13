using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BLINK.Controller;

namespace DinoRage.Classes
{
    public class Find_Player : MonoBehaviour
    {

        public GameObject player = null;
        public int waitTime = 3;
        //public GrassBender _Players_Grass;
        //public float _Grass_Scaler = 2.5f;
        //public bool _using_time = true;
        //public DinoRage_RegionsManager _region;
        // settings for camera
        //public float _cam_min_hight = 7.5f;
        //public float _cam_max_hight = 12f;
        //public float set_cam_hight = 10f;
       // public TopDownWASDController _controller = null;


        void Start()
        {
            FingPlayer();
            //if (_Players_Grass != null)
            //{
           //     Set_Up_Grass_Settings();
           // }
        }
        private void OnEnable()
        {
           // if (_using_time == true)
           // {
           //     _region = DinoRage_RegionsManager.Instance;
           //     _region.Set_Time_Of_Day();
           // }

        }


        public IEnumerator Timer()
        {
            // waits few seconds
            yield return new WaitForSeconds(waitTime);
            FingPlayer();
        }

        public IEnumerator CheckAgain()
        {
            yield return new WaitForSeconds(1);
            //Debug.Log("checked again");
            FingPlayer();
        }


        public void FingPlayer()
        {
            
            if (GameObject.FindWithTag("Player") == null)
            {
                //Debug.Log("didnt find player test");
                StartCoroutine("CheckAgain");
                return;
            }

            // this runs because player isnt null
            player = GameObject.FindWithTag("Player");
            transform.position = player.transform.position;
            transform.parent = player.transform;
            transform.rotation = player.transform.rotation;
            //Run_When_Player_Found();

        }

        /*
        public void Set_Up_Grass_Settings()
        {
            _Players_Grass.scaleMultiplier = _Grass_Scaler;
        }

        public void Run_When_Player_Found()
        {

            // this alows settings per scene for the cam
            _controller = GetComponentInParent<TopDownWASDController>();
            if(_controller != null)
            {
                _controller.minCameraHeight = _cam_min_hight;
                _controller.maxCameraHeight = _cam_max_hight;
            }



        }
        */
    }
}
