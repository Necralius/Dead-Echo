using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static NekraByte.FPS_Utility;

namespace NekraliusDevelopmentStudio
{
    public class ReticleManager : MonoBehaviour
    {
        //Code made by Victor Paulo Melo da Silva - Game Developer - GitHub - https://github.com/Necralius
        //ReticleManager - (0.1)
        //Code State - (Needs Refactoring, Needs Coments, Needs Improvement)
        //This code represents (Code functionality or code meaning)

        #region - Singleton Pattern -
        public static ReticleManager Instance;
        void Awake() => Instance = this;
        #endregion

        #region - Reticle Settings -
        [Header("Reticle Settings")]
        [SerializeField] private float reticleChangeSpeed = 2f;
        public ReticleState currentState;
        #endregion

        #region - Reticle Size Data -
        [Header("Reticle Sizes")]
        [SerializeField, Range(0f, 50f)] private float defaultSize = 10;
        [SerializeField, Range(0f, 50f)] private float movingSize = 13f;
        [SerializeField, Range(0f, 50f)] private float runningSize = 15f;
        [SerializeField, Range(0f, 50f)] private float crouchSize = 7f;
        [SerializeField, Range(0f, 50f)] private float shoottingSize = 22f;
        [SerializeField, Range(0f, 50f)] private float aimSize = 3f;
        [SerializeField, Range(0f, 50f)] private float inAirSize = 15f;
        [SerializeField, Range(0f, 50f)] private float isReloadingSize = 18f;
        #endregion

        #region - Private Data -
        private float targetReticleSize;
        private float currentReticleSize;
        private RectTransform reticleObject => GetComponent<RectTransform>();
        #endregion

        #region - Current States Booleans -
        private bool isMoving       = false;
        private bool isRunning      = false;
        private bool isCrouching    = false;
        private bool isShooting     = false;
        private bool isAiming       = false;
        private bool inAir          = false;
        private bool isReloading    = false;
        #endregion

        //-------------------------------------------Methods--------------------------------//

        #region - Reticle State Management -
        // ------------------------------------------------------------------
        // Name : DataReceiver
        // Desc : This method gets an FPS Controller instance and sets all state booleans.
        // ------------------------------------------------------------------
        public void DataReceiver(FPS_Controller controller)
        {
            isMoving        = controller._isWalking;
            isRunning       = controller._isSprinting;
            isCrouching     = controller._isCrouching;
            inAir           = controller._inAir;
            
            if (controller.equippedGun != null)
            {
                isShooting      = controller.equippedGun._isShooting;
                isAiming        = controller.equippedGun._isAiming;
                isReloading     = controller.equippedGun._isReloading;
            }

            SetReticleState();
        }

        // ------------------------------------------------------------------
        // Name : SetReticleState
        // Desc : This method set the current reticle state, also overriding
        //        the state by the more important state.
        // ------------------------------------------------------------------
        private void SetReticleState()
        {
            if (isCrouching)                                currentState = ReticleState.Crouching;
            else if (isReloading)                           currentState = ReticleState.Reloading;
            else if (inAir)                                 currentState = ReticleState.InAir;
            else if (isShooting)                            currentState = ReticleState.Shooting;
            else if (isMoving && !isRunning)                currentState = ReticleState.Walking;
            else if (isRunning && isMoving && !isCrouching) currentState = ReticleState.Spriting;
            else if (isAiming && isMoving)                  currentState = ReticleState.Aiming;
            else                                            currentState = ReticleState.Idle;
        }
        #endregion

        #region - Reticle Behavior -
        // ------------------------------------------------------------------
        // Name : ReticleManagement
        // Desc : This method manages the reticle target size based on the
        //        current reticle state.
        // ------------------------------------------------------------------
        private void ReticleManagement()
        {
            switch (currentState)
            {
                case ReticleState.Walking   :   targetReticleSize = movingSize;         break;
                case ReticleState.Spriting  :   targetReticleSize = runningSize;        break;
                case ReticleState.Crouching :   targetReticleSize = crouchSize;         break;
                case ReticleState.Shooting  :   targetReticleSize = shoottingSize;         break;
                case ReticleState.Aiming    :   targetReticleSize = aimSize;            break;
                case ReticleState.InAir     :   targetReticleSize = inAirSize;          break;
                case ReticleState.Reloading :   targetReticleSize = isReloadingSize;    break;
                case ReticleState.Idle      :   targetReticleSize = defaultSize;        break;
            }

            currentReticleSize = Mathf.Lerp(currentReticleSize, (targetReticleSize * 10), reticleChangeSpeed * Time.deltaTime);

            reticleObject.sizeDelta = new Vector2(currentReticleSize, currentReticleSize);
        }
        private void Update() => ReticleManagement();
        #endregion
    }
}