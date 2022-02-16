using BLINK.WorldClusters;
using UnityEngine;

namespace BLINK.WorldClusters
{
    public class ClusterInstantTrigger : MonoBehaviour
    {
        public Cluster cluster;
        public ClUSTER_ACTION_EVENT_TYPE actionEventType;
        public int clusterGroupIndex;
        public bool isToggle;
        public bool toggled;
        public bool mouseActions, onMouseEnter, onMouseExit, onClick;

        public void OnMouseDown()
        {
            if (!mouseActions) return;
            if (onClick && Input.GetMouseButtonDown(0))
            {
                TriggerThisCluster();
            }
        }

        public void OnMouseEnter()
        {
            if (!mouseActions || !onMouseEnter) return;
            TriggerThisCluster();
        }

        public void OnMouseExit()
        {
            if (!mouseActions || !onMouseExit) return;
            TriggerThisCluster();
        }

        public void TriggerThisCluster()
        {
            ClusterLogic.TriggerClusterInstantly(cluster, clusterGroupIndex, actionEventType, false);
            HandleToggle();
        }

        private void HandleToggle()
        {
            switch (isToggle)
            {
                case true when !toggled:
                    toggled = true;
                    actionEventType = ClUSTER_ACTION_EVENT_TYPE.Exit;
                    break;
                case true when toggled:
                    actionEventType = ClUSTER_ACTION_EVENT_TYPE.Enter;
                    toggled = false;
                    break;
            }
        }
    }
}
