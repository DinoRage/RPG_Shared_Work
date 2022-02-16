using System.Collections;
using System.Collections.Generic;
using BLINK.WorldClusters;
using UnityEngine;
using UnityEngine.Events;

namespace BLINK.WorldClusters
{
    public class WorldClustersManager : MonoBehaviour
    {
        public List<ActiveCluster> ActiveClusters = new List<ActiveCluster>();

        public static WorldClustersManager Instance { get; private set; }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void AddNewActiveCluster(Cluster cluster, int clusterGroupindex, ClusterCollider clusterCollider)
        {
            if (IsClusterAlreadyActive(cluster, clusterGroupindex, clusterCollider))
            {
                return;
            }
            ActiveCluster newActiveCluster = new ActiveCluster();
            newActiveCluster.cluster = cluster;
            newActiveCluster.clusterGroupIndex = clusterGroupindex;
            newActiveCluster.lastClusterCollider = clusterCollider;
            ActiveClusters.Add(newActiveCluster);
        }

        public void RemoveActiveCluster(Cluster cluster, int clusterGroupIndex)
        {
            foreach (var activeCluster in ActiveClusters)
            {
                if (activeCluster.cluster != cluster || activeCluster.clusterGroupIndex != clusterGroupIndex) continue;
                ActiveClusters.Remove(activeCluster);
                return;
            }
        }

        public void RemoveOverridenCluster(Cluster cluster, int clusterGroupIndex)
        {
            foreach (var activeCluster in ActiveClusters)
            {
                if (activeCluster.cluster != cluster ||
                    activeCluster.clusterGroupIndex != clusterGroupIndex && activeCluster.isOverriden) continue;
                ActiveClusters.Remove(activeCluster);
                return;
            }
        }

        public void OverrideActiveCluster(Cluster cluster, int clusterGroupIndex, int overridenByClusterGroupIndex)
        {
            foreach (var activeCluster in ActiveClusters)
            {
                if (activeCluster.cluster != cluster || activeCluster.clusterGroupIndex != clusterGroupIndex) continue;
                activeCluster.isOverriden = true;
                activeCluster.overridenByClusterGroupIndex = overridenByClusterGroupIndex;
                return;
            }
        }

        private bool IsClusterAlreadyActive(Cluster cluster, int clusterGroupIndex, ClusterCollider clusterCollider)
        {
            foreach (var activeCluster in ActiveClusters)
            {
                if (activeCluster.cluster != cluster || activeCluster.clusterGroupIndex != clusterGroupIndex) continue;
                activeCluster.lastClusterCollider = clusterCollider;
                return true;
            }

            return false;
        }

        public bool IsPlayerInClusterGroup(Cluster cluster, int clusterGroupIndex)
        {
            foreach (var activeCluster in ActiveClusters)
            {
                if (activeCluster.cluster == cluster && activeCluster.clusterGroupIndex == clusterGroupIndex &&
                    !activeCluster.isOverriden)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator ExecuteClusterEntry(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE clusterActionEventType)
        {
            yield return new WaitForSeconds(clusterActionEventType == ClUSTER_ACTION_EVENT_TYPE.Enter
                ? entry.action.enterDelay
                : entry.action.exitDelay);
            HandleEntryLogic(entry, clusterActionEventType);
        }

        private bool GetActionType(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE clusterActionEventType)
        {
            return clusterActionEventType == ClUSTER_ACTION_EVENT_TYPE.Enter
                ? entry.action.enterActionType == ClUSTER_ACTION_TYPE.Enable
                : entry.action.exitActionType == ClUSTER_ACTION_TYPE.Enable;
        }

        private ClUSTER_ACTION_TYPE GetActionType2(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE clusterActionEventType)
        {
            return clusterActionEventType == ClUSTER_ACTION_EVENT_TYPE.Enter
                ? entry.action.enterActionType
                : entry.action.exitActionType;
        }

        private Material GetActionMaterial(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE clusterActionEventType)
        {
            return clusterActionEventType == ClUSTER_ACTION_EVENT_TYPE.Enter
                ? entry.action.enterMaterial
                : entry.action.exitMaterial;
        }
        private UnityEvent GetActionUnityEvent(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE clusterActionEventType)
        {
            return clusterActionEventType == ClUSTER_ACTION_EVENT_TYPE.Enter
                ? entry.action.enterEvents
                : entry.action.exitEvents;
        }
        
        private string GetActionTag(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE clusterActionEventType)
        {
            return clusterActionEventType == ClUSTER_ACTION_EVENT_TYPE.Enter
                ? entry.action.enterTag
                : entry.action.exitTag;
        }

        private int GetActionLayer(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE clusterActionEventType)
        {
            return clusterActionEventType == ClUSTER_ACTION_EVENT_TYPE.Enter
                ? entry.action.enterLayer
                : entry.action.exitLayer;
        }

        public static IEnumerator LightsTransition(ClusterEntry entry, ClUSTER_ACTION_TYPE clusterActionType)
        {
            float intensityTarget = clusterActionType == ClUSTER_ACTION_TYPE.Enable
                ? entry.enableLightIntensityAmount
                : entry.disableLightIntensityAmount;

            if (entry.LightList.Count == 0 || entry.LightList[0] == null)
            {
                Debug.LogWarning(
                    "WORLD CLUSTERS: The first light of the list is missing, and it is needed to trigger the light intensity transition");
                yield break;
            }

            float cachedCurrentIntensity = entry.LightList[0].intensity;

            foreach (var light in entry.LightList)
            {
                if (light == null)
                {
                    ClusterLogic.HandleMissingListEntry("Light");
                    continue;
                }

                light.enabled = true;
            }

            float timeElapsed = 0;
            while (timeElapsed < entry.lightTransitionTime)
            {
                foreach (var light in entry.LightList)
                {
                    if (light == null) continue;
                    light.intensity = Mathf.Lerp(cachedCurrentIntensity, intensityTarget,
                        timeElapsed / entry.lightTransitionTime);
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            foreach (var light in entry.LightList)
            {
                if (light == null) continue;
                light.intensity = intensityTarget;
                if (intensityTarget == 0) light.enabled = false;
            }
        }


        public void HandleEntryLogic(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE clusterActionEventType)
        {
            switch (entry.Type)
            {
                case ClUSTER_ENTRY_TYPE.GameObject:
                    ClusterLogic.ChangeGameObjectsActivation(entry.GameObjectList,
                        GetActionType(entry, clusterActionEventType));
                    break;
                case ClUSTER_ENTRY_TYPE.Light:
                    if (entry.lightTransition)
                    {
                        if (entry.LightTransitionCoroutine != null) StopCoroutine(entry.LightTransitionCoroutine);
                        entry.LightTransitionCoroutine =
                            StartCoroutine(LightsTransition(entry, GetActionType2(entry, clusterActionEventType)));
                    }
                    else
                    {
                        ClusterLogic.ChangeLightsVisibility(entry.LightList,
                            GetActionType(entry, clusterActionEventType));
                    }

                    break;
                case ClUSTER_ENTRY_TYPE.Renderer:
                    if (entry.hideRendererNotDisable)
                    {
                        ClusterLogic.ChangeRenderersVisibility(entry.RendererList,
                            GetActionType(entry, clusterActionEventType));
                    }
                    else
                    {
                        ClusterLogic.ChangeRenderersActivation(entry.RendererList,
                            GetActionType(entry, clusterActionEventType));
                    }

                    break;
                case ClUSTER_ENTRY_TYPE.ParticleSystem:
                    ClusterLogic.ChangeParticleSystemsActivation(entry.ParticleSystemList,
                        GetActionType(entry, clusterActionEventType));
                    break;
                case ClUSTER_ENTRY_TYPE.MaterialChange:
                    ClusterLogic.ChangeRenderersMaterial(entry.RendererList,
                        GetActionMaterial(entry, clusterActionEventType));
                    break;
                case ClUSTER_ENTRY_TYPE.TagChange:
                    ClusterLogic.ChangeGameObjectsTag(entry.GameObjectList,
                        GetActionTag(entry, clusterActionEventType));
                    break;
                case ClUSTER_ENTRY_TYPE.LayerChange:
                    ClusterLogic.ChangeGameObjectsLayer(entry.GameObjectList,
                        GetActionLayer(entry, clusterActionEventType), entry.layerAppliedOnChild);
                    break;
                case ClUSTER_ENTRY_TYPE.UnityEvent:
                    ClusterLogic.TriggerUnityEvents(GetActionUnityEvent(entry, clusterActionEventType));
                    break;
            }
        }
    }
}
