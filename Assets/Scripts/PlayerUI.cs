using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Com.MyCompany.MyGame
{


    public class PlayerUI : MonoBehaviour
    {


        #region Private Fields

        [Tooltip("Pixel offset from the player target")]
        [SerializeField]
        private Vector3 screenOffset = new Vector3(0f, 30f, 0f);


        [Tooltip("UI Text to display Player's Name")]
        [SerializeField]
        private Text playerNameText;


        [Tooltip("UI Slider to display Player's Health")]
        [SerializeField]
        private Slider playerHealthSlider;

        private PlayerManager target;

        float characterControllerHeight = 0f;
        Transform targetTransform;
        Renderer targetRenderer;
        CanvasGroup _canvasGroup;
        Vector3 targetPosition;


        #endregion



        #region MonoBehaviour Callbacks


        void Awake()
        {

            _canvasGroup = this.GetComponent<CanvasGroup>();
            this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        }



        // Update is called once per frame
        void Update()
        {

            //Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
            if(target == null)
            {
                Destroy(this.gameObject);
                return;
            }

            //Reflect the player health 
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = target.Health;
            }



        }


        void LateUpdate()
        {
            // do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI but not seeing the player itself. 
            if(targetRenderer != null)
            {
                targetPosition = targetTransform.position;
                targetPosition.y += characterControllerHeight;
                this.transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
            }


        }



        #endregion


        #region Public Methods

        public void SetTarget(PlayerManager _target)
        {

            if (_target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManger target for PlayerUI.SetTarget.", this);
                return;
            }

            //cache references for efficiency
            target = _target;

            targetTransform = this.target.GetComponent<Transform>();
            targetRenderer = this.target.GetComponent<Renderer>();
            CharacterController characterController = _target.GetComponent<CharacterController>();
            
            // Get data from the Player that won't change during the lifetime of this Component
            if(characterController != null)
            {
                characterControllerHeight = characterController.height;
            }


            if (playerNameText != null)
            {
                playerNameText.text = target.photonView.Owner.NickName;
            }


        }


        #endregion



    }




}