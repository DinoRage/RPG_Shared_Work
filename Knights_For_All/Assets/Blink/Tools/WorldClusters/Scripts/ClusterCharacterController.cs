using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace BLINK.WorldClusters
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class ClusterCharacterController : MonoBehaviour
    {
        public NavMeshAgent agent;

        // CAMERA
        public Camera playerCamera;
        public Vector3 cameraPositionOffset = new Vector3(0, 10, 1);
        public Vector3 cameraRotationOffset = new Vector3(45, 0, 0);

        private void Awake()
        {
            InitCamera();
        }

        private void Update()
        {
            MovementLogic();
        }

        private void LateUpdate()
        {
            HandleCamera();
        }

        #region INIT
        private void InitCamera()
        {
            if (playerCamera == null) return;
            playerCamera.transform.eulerAngles = cameraRotationOffset;
            playerCamera.transform.position = cameraPositionOffset;
        }

        #endregion

        #region LOGIC

        private void MovementLogic()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse1)) return;
            if (!Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out var hit)) return;
            var destination = hit.point;
            TriggerNewDestination(destination);
        }
        #endregion

        #region CAMERA
        private void HandleCamera()
        {
            playerCamera.transform.position = transform.position - (playerCamera.transform.forward * cameraPositionOffset.y);
        }
        #endregion

        #region NAVIGATION
        private void TriggerNewDestination(Vector3 location)
        {
            agent.ResetPath();
            agent.SetDestination(location);
        }

        #endregion
    }
}
