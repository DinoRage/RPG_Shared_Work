using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.Controller;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using UnityEngine.AI;

namespace BLINK.RPGBuilder.Controller
{
    public class TopDownClickToMoveControllerEssentials : RPGBCharacterControllerEssentials
    {
        public TopDownClickToMoveController controller;

        private static readonly int moveSpeedModifier = Animator.StringToHash("MoveSpeedModifier");

        public bool isLockedOnCursor;
        
        /*
        -- EVENT FUNCTIONS --
        */
        public override void MovementSpeedChange(float newSpeed)
        {
            controller.SetSpeed(newSpeed);
        }

        /*
        -- INIT --
        */
        public override void Awake()
        {
            anim = GetComponent<Animator>();
            controller = GetComponent<TopDownClickToMoveController>();
            charController = GetComponent<CharacterController>();
        }

        public override IEnumerator InitControllers()
        {
            yield return new WaitForFixedUpdate();
            controllerIsReady = true;
        }

        /*
        -- DEATH --
        */
        public override void InitDeath()
        {
            anim.Rebind();
            anim.SetBool("Dead", true);
            controller.ResetAgentActions();
            playerIsDead = true;
        }

        public override void CancelDeath()
        {
            playerIsDead = false;
            anim.Rebind();
            anim.SetBool("Dead", false);
        }


        /*
        -- GROUND LEAP FUNCTIONS --
        Ground leaps are mobility abilities. Configurable inside the editor under Combat > Abilities > Ability Type=Ground Leap
        They allow to quickly dash or leap to a certain ground location.
        */
        public override void InitGroundLeap()
        {
            isLeaping = true;
            controller.ResetAgentActions();
            controller.agent.enabled = false;
            lastPosition = transform.position;
        }

        public override void EndGroundLeap()
        {
            isLeaping = false;
            controller.agent.enabled = true;
        }

        /*
        -- FLYING FUNCTIONS --
        */

        public override void InitFlying()
        {
        }

        public override void EndFlying()
        {
        }

        /*
        -- STAND TIME FUNCTIONS --
        Stand time is an optional mechanic for abilities. It allows to root the caster for a certain duration after using the ability.
        */
        public override void InitStandTime(float max)
        {
            standTimeActive = true;
            controller.ResetAgentActions();
            currentStandTimeDur = 0;
            maxStandTimeDur = max;
        }

        protected override void HandleStandTime()
        {
            currentStandTimeDur += Time.deltaTime;
            if (currentStandTimeDur >= maxStandTimeDur) ResetStandTime();
        }

        protected override void ResetStandTime()
        {
            standTimeActive = false;
            currentStandTimeDur = 0;
            maxStandTimeDur = 0;
        }

        /* KNOCKBACK FUNCTIONS
         */
        public bool knockbackActive;
        private Vector3 knockBackTarget;
        private float knockbackDistanceRequired;
        private Vector3 knockbackStartPOS;
        private float cachedAngularSpeed, cachedAcceleration;

        public override void InitKnockback(float knockbackDistance, Transform attacker)
        {
            controller.SetCharacterState(TopDownClickToMoveController.CharacterState.Idle);
            cachedAngularSpeed = controller.agent.angularSpeed;
            cachedAcceleration = controller.agent.acceleration;
            knockbackDistanceRequired = knockbackDistance;
            knockbackDistance *= 5;
            knockbackStartPOS = transform.position;
            knockbackActive = true;
            controller.agent.enabled = true;
            controller.agent.stoppingDistance = 0;
            controller.agent.acceleration = 10;
            controller.agent.velocity = Vector3.zero;
            if(controller.agent.enabled) controller.agent.ResetPath();
            controller.agent.angularSpeed = 0;
            knockBackTarget = (transform.position - attacker.position).normalized * knockbackDistance;
            controller.agent.velocity = knockBackTarget;
        }

        protected override void HandleKnockback()
        {
            if (!knockbackActive) return;
            if(CombatManager.playerCombatNode.dead) ResetKnockback();
            if (!(Vector3.Distance(knockbackStartPOS, transform.position) >= knockbackDistanceRequired)) return;
            ResetKnockback();
        }

        protected override void ResetKnockback()
        {
            knockbackActive = false;
            controller.agent.velocity = Vector3.zero;
            controller.agent.enabled = true;
            controller.ResetAgentActions();
            controller.agent.speed = GetMoveSpeed();
            controller.agent.angularSpeed = cachedAngularSpeed;
            controller.agent.acceleration = cachedAcceleration;
        }

        /* MOTION FUNCTIONS
         */
        private float curMotionSpeed;

        public override void InitMotion(float motionDistance, Vector3 motionDirection, float motionSpeed, bool immune)
        {
            if (CombatManager.playerCombatNode.appearanceREF.isShapeshifted) return;
            if (knockbackActive) return;
            if (motionActive) return;
            controller.agent.ResetPath();
            controller.agent.enabled = false;
            controller.LookAtCursor();
            cachedMotionSpeed = motionSpeed;
            curMotionSpeed = cachedMotionSpeed;
            cachedPositionBeforeMotion = transform.position;
            lastPosition = transform.position;
            cachedMotionDistance = motionDistance;
            motionTarget = transform.TransformDirection(motionDirection) * motionDistance;
            CombatManager.playerCombatNode.isMotionImmune = immune;
            motionActive = true;
        }
        protected override void HandleMotion()
        {
            float distance = Vector3.Distance(cachedPositionBeforeMotion, transform.position);
            if (distance < cachedMotionDistance)
            {
                lastPosition = transform.position;
                controller.charController.Move(motionTarget * (Time.deltaTime * curMotionSpeed));
                
                if (IsInMotionWithoutProgress(0.05f))
                {
                    ResetMotion();
                    return;
                }

                if (!(distance < cachedMotionDistance * 0.75f)) return;
                curMotionSpeed = Mathf.Lerp(curMotionSpeed, 0, Time.deltaTime * 5f);
                if (curMotionSpeed < (cachedMotionSpeed * 0.2f))
                {
                    curMotionSpeed = cachedMotionSpeed * 0.2f;
                }
            }
            else
            {
                ResetMotion();
            }
        }

        public override bool IsInMotionWithoutProgress(float treshold)
        {
            float speed = (transform.position - lastPosition).magnitude;
            return speed > -treshold && speed < treshold;
        }

        protected override void ResetMotion()
        {
            cachedMotionSpeed = 0;
            curMotionSpeed = 0;
            cachedPositionBeforeMotion = Vector3.zero;
            controller.agent.enabled = true;
            controller.ResetAgentActions();
            motionActive = false;
            CombatManager.playerCombatNode.isMotionImmune = false;
            cachedMotionDistance = 0;
            motionTarget = Vector3.zero;
        }

        /*
        -- CAST SLOWED FUNCTIONS --
        Cast slow is an optional mechanic for abilities. It allows the player to be temporarily slowed while
        casting an ability. I personally use it to increase the risk of certain ability use, to increase the chance of being hit
        by enemies attacks while casting it. Of course this is targetting abilities that can be casted while moving.
        */
        public override void InitCastMoveSlow(float speedPercent, float castSlowDuration, float castSlowRate)
        {
            curSpeedPercentage = 1;
            speedPercentageTarget = speedPercent;
            currentCastSlowDur = 0;
            maxCastSlowDur = castSlowDuration;
            speedCastSlowRate = castSlowRate;
            isCastingSlowed = true;
        }

        protected override void HandleCastSlowed()
        {
            curSpeedPercentage -= speedCastSlowRate;
            if (curSpeedPercentage < speedPercentageTarget) curSpeedPercentage = speedPercentageTarget;

            currentCastSlowDur += Time.deltaTime;

            MovementSpeedChange(GetMoveSpeed());

            if (currentCastSlowDur >= maxCastSlowDur) ResetCastSlow();
        }

        public float GetMoveSpeed()
        {
            float newMoveSpeed = RPGBuilderUtilities.getCurrentMoveSpeed(CombatManager.playerCombatNode);
            newMoveSpeed *= curSpeedPercentage;
            return (float) Math.Round(newMoveSpeed, 2);
        }

        protected override void ResetCastSlow()
        {
            isCastingSlowed = false;
            curSpeedPercentage = 1;
            speedPercentageTarget = 1;
            currentCastSlowDur = 0;
            maxCastSlowDur = 0;
            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                controller.anim.SetFloat(moveSpeedModifier, curSpeedPercentage);
            }

            MovementSpeedChange(RPGBuilderUtilities.getCurrentMoveSpeed(CombatManager.playerCombatNode));
        }

        /*
        -- LOGIC UPDATES --
        */
        public override void FixedUpdate()
        {
            if (CombatManager.playerCombatNode == null) return;
            if (CombatManager.playerCombatNode.dead) return;

            HandleCombatStates();

            if (knockbackActive)
                HandleKnockback();

            if (motionActive)
                HandleMotion();

            if (isTeleporting)
                HandleTeleporting();

            if (controller.isSprinting)
                HandleSprint();


            if (isResetingSprintCamFOV)
                HandleSprintCamFOVReset();

        }

        private void HandleSprintCamFOVReset()
        {
            controller.playerCamera.fieldOfView = Mathf.Lerp(controller.playerCamera.fieldOfView,
                controller.normalCameraFOV, Time.deltaTime * controller.cameraFOVLerpSpeed);

            if (Mathf.Abs(controller.playerCamera.fieldOfView - controller.normalCameraFOV) < 0.25f)
            {
                controller.playerCamera.fieldOfView = controller.normalCameraFOV;
                isResetingSprintCamFOV = false;
            }
        }
        
        protected override void HandleTeleporting()
        {
            transform.position = teleportTargetPos;
            isTeleporting = false;
        }

        protected override void HandleCombatStates()
        {
            if (isCastingSlowed) HandleCastSlowed();
            if (standTimeActive) HandleStandTime();
        }

        /*
        -- TELEPORT FUNCTIONS --
        Easy way to instantly teleport the player to a certain location.
        Called by DevUIManager and CombatManager
        */
        public override void TeleportToTarget(Vector3 pos) // Teleport to the Vector3 Coordinates
        {
            isTeleporting = true;
            teleportTargetPos = pos;
        }

        public override void TeleportToTarget(CombatNode target) // Teleport to the CombatNode Coordinates
        {
            isTeleporting = true;
            teleportTargetPos = target.transform.position;
        }

        /*
        -- CHECKING CONDITIONAL FUNCTIONS --
        */
        public override bool HasMovementRestrictions()
        {
            if (CombatManager.Instance == null || CombatManager.playerCombatNode == null) return true;
            return CombatManager.playerCombatNode.dead ||
                   !canMove ||
                   isTeleporting ||
                   standTimeActive ||
                   knockbackActive ||
                   motionActive ||
                   isLeaping ||
                   CombatManager.playerCombatNode.isStunned() ||
                   CombatManager.playerCombatNode.isSleeping();
        }

        public override bool HasRotationRestrictions()
        {
            if (CombatManager.Instance == null || CombatManager.playerCombatNode == null) return true;
            return CombatManager.playerCombatNode.dead ||
                   isLeaping ||
                   knockbackActive ||
                   motionActive ||
                   CombatManager.playerCombatNode.isStunned() ||
                   CombatManager.playerCombatNode.isSleeping();
        }

        /*
        -- UI --
        */
        public override void GameUIPanelAction(bool opened)
        {
        }

        /*
         *
         * MOVEMENT
         */

        public override void StartSprint()
        {
            controller.isSprinting = true;
            controller.SetSpeed(GetMoveSpeed() * controller.sprintSpeedModifier);
            controller.anim.SetFloat(moveSpeedModifier, curSpeedPercentage);
        }

        public override void EndSprint()
        {
            controller.isSprinting = false;
            isResetingSprintCamFOV = true;
            controller.SetSpeed(GetMoveSpeed());
        }


        public override void HandleSprint()
        {
            if (controller.normalCameraFOV != controller.sprintingCameraFOV)
            {
                controller.playerCamera.fieldOfView = Mathf.Lerp(controller.playerCamera.fieldOfView,
                    controller.sprintingCameraFOV, Time.deltaTime * controller.cameraFOVLerpSpeed);
            }


            if (RPGBuilderEssentials.Instance.sprintStatDrainReference == null) return;

            if (!(Time.time >= nextSprintStatDrain)) return;
            nextSprintStatDrain = Time.time + RPGBuilderEssentials.Instance.combatSettings.sprintStatDrainInterval;
            CombatManager.playerCombatNode.AlterVitalityStat(
                RPGBuilderEssentials.Instance.combatSettings.sprintStatDrainAmount,
                RPGBuilderEssentials.Instance.combatSettings.sprintStatDrainID);
        }

        public override bool isSprinting()
        {
            return controller.isSprinting;
        }

        /*
        -- CONDITIONS --
        */
        public override bool ShouldCancelCasting()
        {
            return !IsGrounded() || IsMoving();
        }

        public override bool IsGrounded()
        {
            return true;
        }

        public override bool IsMoving()
        {
            return controller.agent.hasPath;
        }

        public override bool IsThirdPersonShooter()
        {
            return false;
        }

        public override RPGGeneralDATA.ControllerTypes GETControllerType()
        {
            return RPGGeneralDATA.ControllerTypes.TopDownClickToMove;
        }

        public override void MainMenuInit()
        {
            Destroy(GetComponent<TopDownClickToMoveController>());
            Destroy(GetComponent<TopDownClickToMoveControllerEssentials>());
            Destroy(GetComponent<NavMeshAgent>());
        }
        
        public override void AbilityInitActions(RPGAbility.RPGAbilityRankData rankREF)
        {
            switch (rankREF.activationType)
            {
                case RPGAbility.AbilityActivationType.Casted when rankREF.faceCursorWhileCasting:
                    isLockedOnCursor = true;
                    break;
                case RPGAbility.AbilityActivationType.Casted when rankREF.faceCursorWhenOnCastStart:
                    controller.LookAtCursor();
                    break;
            }
        }
        public override void AbilityEndCastActions(RPGAbility.RPGAbilityRankData rankREF)
        {
            switch (rankREF.activationType)
            {
                case RPGAbility.AbilityActivationType.Casted when rankREF.faceCursorWhileCasting:
                    isLockedOnCursor = false;
                    break;
                case RPGAbility.AbilityActivationType.Casted when rankREF.faceCursorWhenOnCastEnd:
                    controller.LookAtCursor();
                    break;
            }
        }
    }
}
