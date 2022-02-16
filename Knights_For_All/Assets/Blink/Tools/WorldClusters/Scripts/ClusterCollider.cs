using System;
using BLINK.WorldClusters;
using UnityEngine;

namespace BLINK.WorldClusters
{
    public class ClusterCollider : MonoBehaviour
    {
        public Cluster cluster;
        public int clusterGroupIndex;

        private void OnTriggerEnter(Collider other)
        {
            ClusterLogic.TriggerCluster(cluster, clusterGroupIndex, other.gameObject, ClUSTER_ACTION_EVENT_TYPE.Enter, this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ShouldResetClusterGroup()) return;
            ClusterLogic.TriggerCluster(cluster, clusterGroupIndex, other.gameObject, ClUSTER_ACTION_EVENT_TYPE.Exit, this);
        }

        private bool ShouldResetClusterGroup()
        {
            foreach (var activeCluster in WorldClustersManager.Instance.ActiveClusters)
            {
                if(activeCluster.cluster != cluster && activeCluster.clusterGroupIndex != clusterGroupIndex) continue;
                if (activeCluster.lastClusterCollider == this) return true;
            }

            return false;
        }
    }
}
