using System.Collections.Generic;
using BLINK.WorldClusters;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cluster Collision Condition", menuName = "BLINK/World Clusters/Collision Condition")]
public class ClusterConditions : ScriptableObject
{
    public List<CollisionCondition> collisionConditions = new List<CollisionCondition>();
}
