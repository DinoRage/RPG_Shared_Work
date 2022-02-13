using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Controller;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace BLINK.Controller
{
    public class ValidCoordinate
    {
        public bool Valid;
        public Vector3 ValidPoint;
    }

    public class MoveInputType
    {
        public bool Valid;
        public bool Held;
    }

    public class ClickInteractableResult
    {
        public bool Valid;
        public bool CancelMovement;
        public float StoppingDistance;
        public Vector3 InteractablePosition;
    }

    public class QueuedRPGBAction
    {
        public RPGCombatDATA.INTERACTABLE_TYPE InteractableType;
        public IPlayerInteractable interactableREF;
        public GameObject go;
    }

    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(TopDownClickToMoveControllerEssentials))]
    public class TopDownClickToMoveController : MonoBehaviour
    {

        // REFERENCES
        public AudioSource cameraAudio;
        public Animator anim;
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        public NavMeshAgent agent;
        public CharacterController charController;

        // CAMERA
        public Camera playerCamera;
        public bool cameraEnabled = true;
        public bool initCameraOnSpawn = true;
        public string cameraName = "Main Camera";
        public Vector3 cameraPositionOffset = new Vector3(0, 10, 1);
        public Vector3 cameraRotationOffset = new Vector3(45, 0, 0);

        public float minCameraHeight = 2,
            maxCameraHeight = 15,
            minCameraVertical = 1.5f,
            maxCameraVertical = 14.5f,
            cameraZoomSpeed = 15,
            cameraZoomPower = 15,
            cameraRotateSpeed = 150f;

        private float currentCameraHeight, cameraHeightTarget, currentCameraVertical, cameraVerticalTarget;

        // NAVIGATION
        public bool movementEnabled = true;
        public bool stunned;
        public float destinationTreshold = 0.25f;
        public LayerMask groundLayers;
        public float maxGroundRaycastDistance = 100;
        public float minimumPathDistance = 0.5f;
        public float samplePositionDistanceMax = 5f;

        // INPUT SETTINGS
        public KeyCode moveKey = KeyCode.Mouse0,
            standKey = KeyCode.LeftShift,
            camRotateKeyLeft = KeyCode.LeftArrow,
            camRotateKeyRight = KeyCode.RightArrow;

        public bool allowHoldKey = true;
        public float holdMoveCd = 0.1f, nextHoldMove, lastClick, timeAfterClickForHolding = 0.1f;
        public bool charLookAtCursorWhileStanding = true, canRotateCamera = true;

        // INPUT FEEDBACK
        public bool alwaysTriggerGroundPathFeedback;
        public GameObject validGroundPathPrefab, rectifiedGroundPathPrefab;
        public AudioClip validGroundPathAudio, rectifiedGroundPathAudio;
        public float groundMarkerDuration = 2;
        public Vector3 markerPositionOffset = new Vector3(0, 0.1f, 0);

        // STATES
        public enum CharacterState
        {
            Idle,
            Moving,
            Standing
        }

        public CharacterState currentCharacterState;
        private static readonly int Standing = Animator.StringToHash("isStanding");

        // RPGB INTEGRATION
        public TopDownClickToMoveControllerEssentials ControllerEssentials;
        public bool isSprinting;
        public float normalCameraFOV = 60, sprintingCameraFOV = 70;
        public float cameraFOVLerpSpeed = 5;
        public float interactionDefaultRange = 3, maxInteractionRange = 500;
        public QueuedRPGBAction currentQueuedRPGBAction;
        public bool useActionKeys;
        public string moveKeyActionKeyName, standKeyActionKeyName, rotateLeftActionKeyName, rotateRightActionKeyName;
        public float sprintSpeedModifier = 1.5f;

        private void Start()
        {
            if (!RPGBuilderEssentials.Instance.isInGame) return;
            ControllerEssentials = GetComponent<TopDownClickToMoveControllerEssentials>();
            InitCameraValues();
            InitCamera();
            InitAudio();
        }

        private void Update()
        {
            if (!RPGBuilderEssentials.Instance.isInGame) return;
            StandingLogic();
            MovementLogic();
            CameraLogic();
        }

        private void LateUpdate()
        {
            HandleCamera();
            CharacterStateLogic();
        }

        #region INIT

        private void InitCameraValues()
        {
            currentCameraHeight = cameraPositionOffset.y;
            cameraHeightTarget = currentCameraHeight;
            currentCameraVertical = cameraPositionOffset.z;
            cameraVerticalTarget = currentCameraVertical;
        }

        private void InitCamera()
        {
            if (!initCameraOnSpawn && playerCamera != null) return;
            Camera cam = GameObject.Find(cameraName).GetComponent<Camera>();
            if (cam == null)
            {
                cam = GameObject.FindGameObjectWithTag("Main Camera").GetComponent<Camera>();
                if (cam == null)
                {
                    Debug.LogError(
                        "TOPDOWN_CLICK_CONTROLLER: NO CAMERA FOUND! MAKE SURE TO EITHER DRAG AND DROP ONE, OR ENABLE INIT CAMERA AND TYPE A VALID CAMERA NAME OR MAIN CAMERA TAG");
                }
                else
                {
                    playerCamera = cam;
                }
            }
            else
            {
                playerCamera = cam;
            }

            if (playerCamera == null) return;
            playerCamera.transform.eulerAngles = cameraRotationOffset;
            InstantCameraUpdate();
        }

        private void InitAudio()
        {
            if (cameraAudio == null) InitAudioSource();
        }

        #endregion

        #region LOGIC

        private void MovementLogic()
        {
            if (ControllerEssentials.HasMovementRestrictions() || !movementEnabled || stunned ||
                RPGBuilderUtilities.IsPointerOverUIObject()) return;
            if (IsStanding()) return;
            MoveInputType moveInputType = MovingInput();
            if (!moveInputType.Valid) return;
            agent.stoppingDistance = 0;
            ClickInteractableResult clickInteractableResult = IsClickingOnInteractable();
            if (clickInteractableResult.Valid && !moveInputType.Held)
            {
                if (clickInteractableResult.CancelMovement)
                {
                    LookAtCursor();
                    return;
                }

                agent.stoppingDistance = clickInteractableResult.StoppingDistance;
            }

            if (!Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out var hit,
                maxGroundRaycastDistance, groundLayers)) return;
            var destination = clickInteractableResult.Valid ? clickInteractableResult.InteractablePosition : hit.point;
            bool validClick = true;
            if (IsPathTooClose(destination)) return;
            if (!IsPathAllowed(destination))
            {
                ValidCoordinate newResult = closestAllowedDestination(destination);
                if (newResult.Valid)
                {
                    destination = newResult.ValidPoint;
                    validClick = false;
                }
                else
                {
                    return;
                }
            }

            TriggerNewDestination(destination);

            if ((!alwaysTriggerGroundPathFeedback && moveInputType.Held) || clickInteractableResult.Valid) return;
            SpawnGroundPathMarker(destination, validClick);
            PlayGroundPathAudio(validClick);
        }


        private ClickInteractableResult IsClickingOnInteractable()
        {
            ClickInteractableResult clickInteractableResult = new ClickInteractableResult();
            if (!Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out var hit,
                maxGroundRaycastDistance)) return clickInteractableResult;

            IPlayerInteractable playerInteractable = hit.transform.gameObject.GetComponent<IPlayerInteractable>();
            if (playerInteractable == null) return clickInteractableResult;
            clickInteractableResult.Valid = true;
            clickInteractableResult.InteractablePosition = hit.transform.position;
            float curStopDistance = GetCurrentStoppingDistance();
            RPGCombatDATA.INTERACTABLE_TYPE interactableType = playerInteractable.getInteractableType();
            switch (interactableType)
            {
                case RPGCombatDATA.INTERACTABLE_TYPE.None:
                    clickInteractableResult.Valid = false;
                    break;
                case RPGCombatDATA.INTERACTABLE_TYPE.AlliedUnit:
                case RPGCombatDATA.INTERACTABLE_TYPE.NeutralUnit:
                case RPGCombatDATA.INTERACTABLE_TYPE.EnemyUnit:
                    float realStopDistance = interactableType == RPGCombatDATA.INTERACTABLE_TYPE.AlliedUnit
                        ? interactionDefaultRange
                        : curStopDistance;
                    if (Vector3.Distance(transform.position, hit.transform.position) <= realStopDistance)
                    {
                        clickInteractableResult.CancelMovement = true;
                    }
                    else
                    {
                        clickInteractableResult.StoppingDistance = realStopDistance;
                    }

                    break;
                case RPGCombatDATA.INTERACTABLE_TYPE.InteractiveNode:
                case RPGCombatDATA.INTERACTABLE_TYPE.CraftingStation:
                case RPGCombatDATA.INTERACTABLE_TYPE.LootBag:
                case RPGCombatDATA.INTERACTABLE_TYPE.WorldDroppedItem:
                    if (Vector3.Distance(transform.position, hit.transform.position) <= interactionDefaultRange)
                    {
                        clickInteractableResult.CancelMovement = true;
                    }
                    else
                    {
                        clickInteractableResult.StoppingDistance = interactionDefaultRange;
                    }

                    break;
            }

            if (clickInteractableResult.Valid)
            {
                if (Vector3.Distance(transform.position, hit.transform.position) >= maxInteractionRange)
                {
                    clickInteractableResult.CancelMovement = true;
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This is too far", 3);
                    return clickInteractableResult;
                }

                currentQueuedRPGBAction = new QueuedRPGBAction
                {
                    InteractableType = interactableType, interactableREF = playerInteractable,
                    go = hit.transform.gameObject
                };
            }

            return clickInteractableResult;
        }

        private float GetCurrentStoppingDistance()
        {
            if (CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID == -1)
                return interactionDefaultRange;
            RPGAbility.RPGAbilityRankData rankREF = RPGBuilderUtilities.getCurrentAbilityRankREF(
                CombatManager.playerCombatNode,
                RPGBuilderUtilities.GetAbilityFromID(CombatManager.playerCombatNode.AutoAttackData
                    .currentAutoAttackAbilityID), false);
            return rankREF.maxRange;
        }

        private void CameraLogic()
        {
            if (!cameraEnabled) return;
            CameraInputs();
            LerpCameraHeight();
        }

        private void StandingLogic()
        {
            if (IsKeyDown(useActionKeys
                ? RPGBuilderUtilities.GetCurrentKeyByActionKeyName(standKeyActionKeyName)
                : standKey)) InitStanding();
            if (IsStanding() || ControllerEssentials.isLockedOnCursor)
            {
                HandleStanding();
            }
            else if (IsEndStanding())
            {
                ResetStanding();
            }
        }


        private void InitStanding()
        {
            ResetAgentActions();
            StartCoroutine(SetCharacterState(CharacterState.Standing));
        }

        private void ResetStanding()
        {
            StartCoroutine(SetCharacterState(CharacterState.Idle));
        }

        public void ResetAgentActions()
        {
            agent.ResetPath();
            agent.stoppingDistance = 0;
            StartCoroutine(SetCharacterState(CharacterState.Idle));
            
            if (currentQueuedRPGBAction != null)
            {
                TriggerQueuedRPGBAction();
            }
        }

        private void HandleStanding()
        {
            if (!charLookAtCursorWhileStanding) return;
            LookAtCursor();
        }

        public void LookAtCursor()
        {
            ValidCoordinate validCoordinate = GetGroundRayPoint();
            if (!validCoordinate.Valid) return;
            var targetRotation = Quaternion.LookRotation(validCoordinate.ValidPoint - transform.position);
            targetRotation.x = 0;
            targetRotation.z = 0;
            //transform.rotation = targetRotation;
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }

        public void LookAtPos(Vector3 pos)
        {
            var targetRotation = Quaternion.LookRotation(pos - transform.position);
            targetRotation.x = 0;
            targetRotation.z = 0;
            transform.rotation = targetRotation;
        }

        private ValidCoordinate GetGroundRayPoint()
        {
            var playerPlane = new Plane(Vector3.up, transform.position);
            var ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            ValidCoordinate validCoordinate = new ValidCoordinate();
            if (!playerPlane.Raycast(ray, out var hitDist)) return validCoordinate;
            validCoordinate.Valid = true;
            validCoordinate.ValidPoint = ray.GetPoint(hitDist);
            return validCoordinate;
        }

        public bool IsStanding()
        {
            return IsKeyHeld(useActionKeys
                ? RPGBuilderUtilities.GetCurrentKeyByActionKeyName(standKeyActionKeyName)
                : standKey);
        }

        public bool IsEndStanding()
        {
            return IsKeyUp(useActionKeys
                ? RPGBuilderUtilities.GetCurrentKeyByActionKeyName(standKeyActionKeyName)
                : standKey);
        }

        private void CharacterStateLogic()
        {
            switch (currentCharacterState)
            {
                case CharacterState.Idle:
                    break;

                case CharacterState.Moving:
                    if (IsDestinationReached()) ResetAgentActions();
                    break;
            }
        }

        public IEnumerator SetCharacterState(CharacterState state)
        {
            yield return new WaitForEndOfFrame();
            currentCharacterState = state;
            StartAnimation(state);
        }

        #endregion

        #region RPGBIntegration

        public void SetSpeed(float newSpeed)
        {
            agent.speed = newSpeed;
            float speedMod = newSpeed / 5;
            anim.SetFloat(moveSpeedModifier, speedMod);
        }

        #endregion

        #region CAMERA

        void InstantCameraUpdate()
        {
            Vector3 targetPos = transform.position - (playerCamera.transform.forward * currentCameraHeight);
            targetPos.z -= currentCameraVertical;
            playerCamera.transform.position = targetPos;
        }

        private void CameraInputs()
        {
            HandleCameraZoom();
        }

        private void HandleCameraZoom()
        {
            if (Input.mouseScrollDelta.y == 0) return;
            float heightDifference = Input.mouseScrollDelta.y < 0f ? cameraZoomPower : -cameraZoomPower;
            cameraHeightTarget = currentCameraHeight + heightDifference;
            cameraVerticalTarget = currentCameraVertical + heightDifference;
            if (cameraHeightTarget > maxCameraHeight) cameraHeightTarget = maxCameraHeight;
            else if (cameraHeightTarget < minCameraHeight) cameraHeightTarget = minCameraHeight;
            if (cameraVerticalTarget > maxCameraVertical) cameraVerticalTarget = maxCameraVertical;
            else if (cameraVerticalTarget < minCameraVertical) cameraVerticalTarget = minCameraVertical;
        }

        private static readonly int moveSpeedModifier = Animator.StringToHash("MoveSpeedModifier");

        private void HandleCamera()
        {
            if (!cameraEnabled) return;
            if (canRotateCamera)
            {
                Vector3 eulerAngles = playerCamera.transform.rotation.eulerAngles;
                if (IsKeyHeld(useActionKeys
                    ? RPGBuilderUtilities.GetCurrentKeyByActionKeyName(rotateLeftActionKeyName)
                    : camRotateKeyLeft))
                {
                    playerCamera.transform.rotation = Quaternion.Euler(eulerAngles.x,
                        eulerAngles.y - cameraRotateSpeed * Time.deltaTime, eulerAngles.z);

                }
                else if (IsKeyHeld(useActionKeys
                    ? RPGBuilderUtilities.GetCurrentKeyByActionKeyName(rotateRightActionKeyName)
                    : camRotateKeyRight))
                {
                    playerCamera.transform.rotation = Quaternion.Euler(eulerAngles.x,
                        eulerAngles.y + cameraRotateSpeed * Time.deltaTime, eulerAngles.z);
                }
            }

            playerCamera.transform.position = (transform.position + Vector3.up * 0.8f) -
                                              (playerCamera.transform.forward * currentCameraHeight);
        }


        private void LerpCameraHeight()
        {
            currentCameraHeight = Mathf.Lerp(currentCameraHeight, cameraHeightTarget, Time.deltaTime * cameraZoomSpeed);
            currentCameraVertical =
                Mathf.Lerp(currentCameraVertical, cameraVerticalTarget, Time.deltaTime * cameraZoomSpeed);
        }

        #endregion

        #region NAVIGATION


        private void TriggerQueuedRPGBAction()
        {
            switch (currentQueuedRPGBAction.InteractableType)
            {
                case RPGCombatDATA.INTERACTABLE_TYPE.None:
                    break;
                case RPGCombatDATA.INTERACTABLE_TYPE.NeutralUnit:
                    break;
                case RPGCombatDATA.INTERACTABLE_TYPE.EnemyUnit:
                    break;
                case RPGCombatDATA.INTERACTABLE_TYPE.AlliedUnit:
                case RPGCombatDATA.INTERACTABLE_TYPE.InteractiveNode:
                case RPGCombatDATA.INTERACTABLE_TYPE.CraftingStation:
                case RPGCombatDATA.INTERACTABLE_TYPE.LootBag:
                case RPGCombatDATA.INTERACTABLE_TYPE.WorldDroppedItem:
                    currentQueuedRPGBAction.interactableREF.Interact();
                    break;
            }

            LookAtPos(currentQueuedRPGBAction.go.transform.position);
            currentQueuedRPGBAction = null;
        }

        private bool IsPathAllowed(Vector3 point)
        {
            NavMeshPath path = new NavMeshPath();
            return NavMesh.CalculatePath(transform.position, point, NavMesh.AllAreas, path);
        }

        private ValidCoordinate closestAllowedDestination(Vector3 point)
        {
            ValidCoordinate newResult = new ValidCoordinate();
            if (!NavMesh.SamplePosition(point, out var hit, samplePositionDistanceMax, NavMesh.AllAreas))
                return newResult;
            newResult.Valid = true;
            newResult.ValidPoint = hit.position;
            return newResult;
        }

        private bool IsKeyDown(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        private bool IsKeyHeld(KeyCode key)
        {
            return Input.GetKey(key);
        }

        private bool IsKeyUp(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        private MoveInputType MovingInput()
        {
            MoveInputType moveInputType = new MoveInputType();
            if (IsKeyDown(useActionKeys
                ? RPGBuilderUtilities.GetCurrentKeyByActionKeyName(moveKeyActionKeyName)
                : moveKey))
            {
                lastClick = Time.time;
                moveInputType.Valid = true;
                return moveInputType;
            }

            if (!(Time.time >= nextHoldMove))
            {
                moveInputType.Valid = false;
                return moveInputType;
            }

            if (!(Time.time >= lastClick + timeAfterClickForHolding))
            {
                moveInputType.Valid = false;
                return moveInputType;
            }

            if (!allowHoldKey || !IsKeyHeld(useActionKeys
                ? RPGBuilderUtilities.GetCurrentKeyByActionKeyName(moveKeyActionKeyName)
                : moveKey)) return moveInputType;
            nextHoldMove = Time.time + holdMoveCd;
            moveInputType.Valid = true;
            moveInputType.Held = true;
            return moveInputType;
        }

        private void TriggerNewDestination(Vector3 location)
        {
            agent.SetDestination(location);
            StartCoroutine(SetCharacterState(CharacterState.Moving));
        }

        private bool IsDestinationReached()
        {
            return !agent.hasPath || agent.remainingDistance <= (agent.stoppingDistance + destinationTreshold);
        }

        private bool IsPathTooClose(Vector3 point)
        {
            return Vector3.Distance(transform.position, point) < minimumPathDistance;
        }

        #endregion

        #region FEEDBACK

        private void SpawnGroundPathMarker(Vector3 point, bool rectified)
        {
            GameObject prefab = rectified ? validGroundPathPrefab : rectifiedGroundPathPrefab;
            if (prefab == null) return;
            GameObject marker = Instantiate(prefab,
                new Vector3(point.x + markerPositionOffset.x, point.y + markerPositionOffset.y,
                    point.z + markerPositionOffset.z), prefab.transform.rotation);
            Destroy(marker, groundMarkerDuration);
        }

        private void PlayGroundPathAudio(bool rectified)
        {
            AudioClip audio = rectified ? validGroundPathAudio : rectifiedGroundPathAudio;
            if (audio == null) return;
            if (cameraAudio == null) InitAudioSource();
            cameraAudio.PlayOneShot(audio);
        }

        #endregion

        #region OTHER

        private void InitAudioSource()
        {
            AudioSource ASource = playerCamera.GetComponent<AudioSource>();
            if (ASource == null)
            {
                ASource = playerCamera.gameObject.AddComponent<AudioSource>();
            }

            cameraAudio = ASource;
        }

        public void StartAnimation(CharacterState state)
        {
            if (anim == null) return;
            ResetStateAnimations();
            switch (state)
            {
                case CharacterState.Idle:
                    break;
                case CharacterState.Moving:
                    anim.SetBool(IsMoving, true);
                    break;
                case CharacterState.Standing:
                    anim.SetBool(Standing, true);
                    break;
            }
        }

        private void ResetStateAnimations()
        {
            anim.SetBool(IsMoving, false);
            anim.SetBool(Standing, false);
        }

        #endregion

    }
}
