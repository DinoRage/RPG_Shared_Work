using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BLINK.RPGBuilder.AI;

namespace DinoRage
{
    public class Spawner_Over_Ride_Test : MonoBehaviour
    {
        private NPCSpawner _the_spawner;
        // make sure the child holding the spawner is set as dissabled not just the spawner script
        private GameObject _child_holding_Spawner = null;
        // this is where you set the spawners maximum count now
        public int _spawner_maximum = 15;
        //public List<NPCSpawner.NPC_SPAWN_DATA> spawnData = new List<NPCSpawner.NPC_SPAWN_DATA>();
        private void OnEnable()
        {
            _child_holding_Spawner = transform.GetChild(0).gameObject;
            _the_spawner = _child_holding_Spawner.GetComponent<NPCSpawner>();
        }
        // update stuff was just for testing
        /*
        void Update()
        {
            if (Input.GetKeyDown("j"))
            {
                Enable_Spawner();
            }
            if (Input.GetKeyDown("k"))
            {
                Dissable_Spawner();
            }
        }
        
        public void OverRider()
        {
            _the_spawner.ManualSpawnNPC();
        }
        */
        public void Dissable_Spawner()
        {
            // seting spawn count to 0 stops the spawner spawning any new NPCs till needed again
            _the_spawner.npcCountMax = 0;
            DestroyAllNPC();
            _child_holding_Spawner.SetActive(false);
        }
        public void Enable_Spawner()
        {
            _the_spawner.npcCountMax = _spawner_maximum;
            // this turns the spawner on for the first time
            _child_holding_Spawner.SetActive(true);
            // this makes sure it re spawns NPCs on re entery
            Manualy_Spawn_NPCs();
        }
        public void Manualy_Spawn_NPCs()
        {
            for (int i = 0; i <= _spawner_maximum; i++)
            {
                _the_spawner.ManualSpawnNPC();
            }
        }
        // this section destroys all the npcs used by that spawner in a clean way
        public void DestroyAllNPC()
        {
            for (int i = 0; i < _the_spawner.curNPCs.Count; i++)
            {
                Destroy(_the_spawner.curNPCs[i].gameObject);
            }
            /*
            for (int i = 0; i < _the_spawner.curNPCs.Count; i++)
            {
                for (int n = 0; n < _the_spawner.curNPCs[i].nodeStats.Count; n++)
                {
                    if (_the_spawner.curNPCs[i].nodeStats[n]._name == "Health")
                    {
                        _the_spawner.curNPCs[i].nodeStats[n].curMaxValue = 0;
                        _the_spawner.curNPCs[i].nodeStats[n].curValue = 0;
                        break;
                    }
                }
                Debug.Log("DestroyedAllNPC");
            }
            */
        }
    }
}

