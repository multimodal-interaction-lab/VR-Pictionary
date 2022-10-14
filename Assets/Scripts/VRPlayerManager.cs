using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// VRPlayer manager
    /// Handles fire input and laser beams.
    /// </summary>
    public class VRPlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        #region Private Fields

        /*
        [Tooltip("The Beams Gameobject to control")]
        [SerializeField]
        private GameObject beams;
        */
        //true, when the user is firing
        bool IsFiring;

        [Tooltip("Head representation")]
        [SerializeField]
        private GameObject head;

        [Tooltip("Left hand representation")]
        [SerializeField]
        private GameObject leftHand;

        [Tooltip("Right hand representation")]
        [SerializeField]
        private GameObject rightHand;


        #endregion


        #region Public Fields


        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        /*
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;
        */

        #endregion


        #region MonoBehaviour Callbacks


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {

            /*
            if (beams == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }
            */

            Debug.Log("VR Manager Awaking!!");

            //#important
            //Used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {

                Debug.Log("We are instantiating a local player instance");

                VRPlayerManager.LocalPlayerInstance = this.gameObject;


                head.GetComponent<MeshRenderer>().enabled = false;
                leftHand.GetComponent<MeshRenderer>().enabled = false;
                rightHand.GetComponent<MeshRenderer>().enabled = false;

            }
            //#critical
            //we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);

        }



        void Start()
        {

            /*
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();


            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }
            */

            /*
            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);

            }
            */

#if UNITY_5_4_OR_NEWER
//unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

#endif
        }




        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {

            if (photonView.IsMine)
            {
                ProcessInputs();
                if (Health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();
                }

            }

            /*
            if (beams != null && IsFiring != beams.activeInHierarchy)
            {
                beams.SetActive(IsFiring);
            }
            */

        }

        /// <summary>
        /// MonoBehaviour method called when the collider 'other' enters the trigger.
        /// Affect health of the player if the colllider is a beam
        /// Note: when jumping and firing at the same time, you'll find that the player's own beam intersects with itself.
        /// One could move the collider further away to prevent this or check if the beam belongs to the player. 
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerEnter(Collider other)
        {

            if (!photonView.IsMine)
            {
                return;
            }
            // We are only interested in Beamers.
            // we should be using tags but for the sake of distribution, let's simply check by name. 
            
            /*
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Health -= 0.1f;
            */
        }



        /// <summary>
        /// MonoBehaviour method called once per frame 
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerStay(Collider other)
        {
            // we don't do anyting if we're not the local player. 
            if (!photonView.IsMine)
            {
                return;
            }

            /*
            //we are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name. 
            if (!other.name.Contains("Beam"))
            {
                return;
            }

            // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death. 
            Health -= 0.1f * Time.deltaTime;
            */

        }



#if !UNITY_5_4_OR_NEWER
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }

#endif

        void CalledOnLevelWasLoaded(int level)
        {
            /*
            GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            */

            //check if we are outside the arena and if that's the case, spawn around center of arena in a safe zone.
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }



        }


#if UNITY_5_4_OR_NEWER

    public override void OnDisable()
    {
        // always call the base to remove callbacks
        base.OnDisable();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

    }

#endif

        #endregion


        #region IPunObservable implementation
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            
            if (stream.IsWriting)
            {
                //we own this player: send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
            }
            else
            {
                //network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();

            }

            

        }

         

        #endregion

        #region Private Methods

#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode){

        this.CalledOnLevelWasLoaded(scene.buildIndex);

        }
#endif


        #endregion



        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire. 
        /// </summary>
        void ProcessInputs()
        {

            head.transform.position = OVRCameraRig.Instance.centerEyeAnchor.position;
            head.transform.rotation = OVRCameraRig.Instance.centerEyeAnchor.rotation;

            leftHand.transform.position = OVRCameraRig.Instance.leftHandAnchor.position;
            leftHand.transform.rotation = OVRCameraRig.Instance.leftHandAnchor.rotation;

            rightHand.transform.position = OVRCameraRig.Instance.rightHandAnchor.position;
            rightHand.transform.rotation = OVRCameraRig.Instance.rightHandAnchor.rotation;


            /*
            if (Input.GetButtonDown("Fire1"))
            {
                if (!IsFiring)
                {
                    IsFiring = true;
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                if ((IsFiring))
                {
                    IsFiring = false;
                }
            }
            */
        }


        #endregion


    }
}