using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace BLINK.WorldClusters
{
    public static class ClusterLogic
    {
        public static void TriggerCluster(Cluster cluster, int groupIndex, GameObject collided, ClUSTER_ACTION_EVENT_TYPE actionEventType, ClusterCollider clusterCollider)
        {
            if (!SceneHasManager()) return;
            if (cluster.enabled == false) return;
            if (groupIndex > cluster.clusterGroups.Count - 1) return;
            if (actionEventType == ClUSTER_ACTION_EVENT_TYPE.Exit && !WorldClustersManager.Instance.IsPlayerInClusterGroup(cluster, groupIndex))
            {
                WorldClustersManager.Instance.RemoveOverridenCluster(cluster, groupIndex);
                return;
            }
            foreach (var entry in cluster.clusterGroups[groupIndex].Entries)
            {
                if (!IsValidCollision(entry.action, collided)) continue;
                if (clusterCollider != null) HandleActiveClusters(cluster, groupIndex, actionEventType, clusterCollider);
                switch (actionEventType)
                {
                    case ClUSTER_ACTION_EVENT_TYPE.Enter:
                        HandleClusterGroupOverrides(cluster, groupIndex);
                        break;
                    case ClUSTER_ACTION_EVENT_TYPE.Exit:
                        HandleOverridenClusters(cluster, groupIndex);
                        break;
                }
                HandleCluster(entry, actionEventType);
            }
        }

        public static void TriggerClusterInstantly(Cluster cluster, int groupIndex, ClUSTER_ACTION_EVENT_TYPE actionEventType, bool isOverride)
        {
            if (!SceneHasManager()) return;
            if (cluster.enabled == false) return;
            if (groupIndex > cluster.clusterGroups.Count - 1) return;
            foreach (var entry in cluster.clusterGroups[groupIndex].Entries)
            {
                if(!isOverride) HandleActiveClusters(cluster, groupIndex, actionEventType, null);
                HandleCluster(entry, actionEventType);
            }
        }

        public static void HandleOverridenClusters(Cluster cluster, int overridenByClusterGroupIndex)
        {
            foreach (var activeCluster in WorldClustersManager.Instance.ActiveClusters)
            {
                if(!activeCluster.isOverriden) continue;
                if(activeCluster.overridenByClusterGroupIndex != overridenByClusterGroupIndex) continue;
                TriggerClusterInstantly(cluster, activeCluster.clusterGroupIndex, ClUSTER_ACTION_EVENT_TYPE.Enter, false);
                activeCluster.isOverriden = false;
                activeCluster.overridenByClusterGroupIndex = -1;
            }
        }

        public static bool SceneHasManager()
        {
            if (WorldClustersManager.Instance != null) return true;
            Debug.LogError("Hey, you need to add a World Cluster Manager to your scene. \n" +
                           "You can easily do that by clicking on the Blink Tab then World Clusters > Manager and go to utilities and click `ADD MANAGER TO SCENE`");
            return false;
        }

        private static void HandleActiveClusters (Cluster cluster, int groupIndex, ClUSTER_ACTION_EVENT_TYPE actionEventType, ClusterCollider clusterCollider)
        {
            switch (actionEventType)
            {
                case ClUSTER_ACTION_EVENT_TYPE.Enter:
                    WorldClustersManager.Instance.AddNewActiveCluster(cluster, groupIndex, clusterCollider);
                    break;
                case ClUSTER_ACTION_EVENT_TYPE.Exit:
                    WorldClustersManager.Instance.RemoveActiveCluster(cluster, groupIndex);
                    break;
            }
        }

        private static void HandleCluster(ClusterEntry entry, ClUSTER_ACTION_EVENT_TYPE actionEventType)
        {
            WorldClustersManager.Instance.StartCoroutine(WorldClustersManager.Instance.ExecuteClusterEntry(entry, actionEventType));
        }

        private static void HandleClusterGroupOverrides(Cluster cluster, int newGroupIndex)
        {
            foreach (var cOverride in cluster.clusterGroups[newGroupIndex].overrides)
            {
                if (newGroupIndex == cOverride.clusterGroupIndex) continue;
                if (cOverride.clusterGroupIndex > cluster.clusterGroups.Count) continue;
                if(!WorldClustersManager.Instance.IsPlayerInClusterGroup(cluster, cOverride.clusterGroupIndex)) continue;
                TriggerClusterInstantly(cluster, cOverride.clusterGroupIndex, ClUSTER_ACTION_EVENT_TYPE.Exit, true);
                WorldClustersManager.Instance.OverrideActiveCluster(cluster, cOverride.clusterGroupIndex, newGroupIndex);
            }
        }

        public static void HandleMissingListEntry(string entryType)
        {
            Debug.LogWarning("WORLD CLUSTERS: " + entryType + " was missing from this cluster list. Nothing will break, but it is better to remove them and keep things clean!");
        }

        public static void ChangeGameObjectsActivation(List<GameObject> gameObjectsList, bool active)
        {
            foreach (var go in gameObjectsList)
            {
                if (go == null)
                {
                    HandleMissingListEntry("Game Object");
                    continue;
                }
                go.SetActive(active);
            }
        }
        
        public static void ChangeRenderersActivation(List<Renderer> renderers, bool active)
        {
            foreach (var re in renderers)
            {
                if (re == null)
                {
                    HandleMissingListEntry("Renderer");
                    continue;
                }
                re.enabled = active;
            }
        }
        public static void ChangeRenderersVisibility(List<Renderer> renderers, bool active)
        {
            foreach (var re in renderers)
            {
                if (re == null)
                {
                    HandleMissingListEntry("Renderer");
                    continue;
                }
                re.shadowCastingMode = active ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
            }
        }
        public static void ChangeLightsVisibility(List<Light> lights, bool active)
        {
            foreach (var light in lights)
            {
                if (light == null)
                {
                    HandleMissingListEntry("Light");
                    continue;
                }
                light.enabled = active;
            }
        }
        
        public static void ChangeParticleSystemsActivation(List<ParticleSystem> particleSystems, bool active)
        {
            foreach (var ps in particleSystems)
            {
                if (ps == null)
                {
                    HandleMissingListEntry("Particle System");
                    continue;
                }
                if (active)
                {
                    ps.Play();
                }
                else
                {
                    ps.Stop();
                }
            }
        }
        
        public static void ChangeRenderersMaterial(List<Renderer> renderers, Material mat)
        {
            foreach (var re in renderers)
            {
                if (re == null)
                {
                        HandleMissingListEntry("Renderer");
                    continue;
                }
                re.material = mat;
            }
        }
        
        public static void ChangeGameObjectsTag(List<GameObject> gameObjectsList, string tag)
        {
            foreach (var go in gameObjectsList)
            {
                if (go == null)
                {
                    HandleMissingListEntry("Game Object");
                    continue;
                }
                go.tag = tag;
            }
        }
        public static void ChangeGameObjectsLayer(List<GameObject> gameObjectsList, int layer, bool applyOnChild)
        {
            foreach (var go in gameObjectsList)
            {
                if (go == null)
                {
                    HandleMissingListEntry("Game Object");
                    continue;
                }
                go.layer = layer;
                if(!applyOnChild) continue;
                foreach (var child in go.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = layer;
                }
            }
        }

        public static void TriggerUnityEvents(UnityEvent events)
        {
            events.Invoke();
        }
        
        
        private static bool IsValidCollision(ClusterAction action, GameObject collidedObject)
        {
            if (collidedObject == null) return true;
            List<bool> optionalResults = new List<bool>();
            List<bool> requiredResults = new List<bool>();
            foreach (var validCollision in action.conditions.collisionConditions)
            {
                bool result = false;
                switch (validCollision.type)
                {
                    case ClUSTER_COLLISION_CONDITION_TYPE.GameObjectName:
                        result = collidedObject.name == validCollision.gameObjectName;
                        break;
                    case ClUSTER_COLLISION_CONDITION_TYPE.LayerMask:
                        result = validCollision.layer == collidedObject.layer;
                        break;
                    case ClUSTER_COLLISION_CONDITION_TYPE.Tag:
                        result = collidedObject.CompareTag(validCollision.tagName);
                        break;
                }

                if (validCollision.requirementType == ClUSTER_CONDITION_REQUIREMENT_TYPE.Optional)
                    optionalResults.Add(result);
                else requiredResults.Add(result);
            }

            bool optionalIsValid = optionalResults.Count == 0 || optionalResults.Contains(true);
            bool requiredIsValid = requiredResults.Count == 0 || !requiredResults.Contains(false);
            return optionalIsValid && requiredIsValid;
        }

        private static bool LayerContains(LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
    }
}
