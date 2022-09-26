using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


namespace Com.MyCompany.MyGame
{



    public class Launcher : MonoBehaviourPunCallbacks
    {

        #region private serializable fields

        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, so a new room will be created.")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;

        [Tooltip("The UI panel to let the user  enter name, connect, and play")]
        [SerializeField]
        private GameObject controlPanel;

        [Tooltip("The UI label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;

        #endregion



        #region private fields


        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion(which allows you to make breaking changes).
        /// </summary>
        string gameVersion = "1";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;


        #endregion


        #region monobehaviour callbacks

        /// <summary>
        /// Monobehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void awake()
        {

            // #critical
            // this makes sure we can use PhotonNetwork.loadLevel() on the master client and all clients in the same room sync automatically
            PhotonNetwork.AutomaticallySyncScene = true;

        }

        /// <summary>
        /// Monobehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Start()
        {

            progressLabel.SetActive(false);
            controlPanel.SetActive(true);

            
        }


        #endregion


        #region MonoBehaviourPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            //#Critical: the first we try to do is join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()

            //we don't want to do anything if we are not attempting to join a room. 
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything
            if (isConnecting)
            {
                //#Critical: the first we try to do is to join a potential existing room. If there is, good, else, we'll be called  back with OnJoinRandomFailed();
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);


            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            //#Critical: we failed to join a random room, maybe none exists or they are all full. We create a new room. 
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutoiral/Launcher: OnJoinedRoom() was called by PUN. Now this client is in a room :)");

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the 'Room for 1' ");


                // #Critical
                // Load the Room level
                PhotonNetwork.LoadLevel("Room for 1");
            }


            if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                Debug.Log("We load the 'Room for 2' ");

                //#Critical
                // Load the Room level
                PhotonNetwork.LoadLevel("Room for 2");

            }



        }


        #endregion


        #region Public Methods
        /// <summary>
        /// Start the connection process.
        /// - if already connected, we attempt joining a random room. 
        /// - if not connected, connect this application instance to Photon Cloud network.
        /// </summary>
        public void Connect()
        {

            progressLabel.SetActive(true);
            controlPanel.SetActive(false);

            

            //checks if the Photon Cloud network is connected.
            if (PhotonNetwork.IsConnected)
            {

                //#critical we need at this point to attempt joining a random room. If it fails, we'll get notified in OnJoinRoomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {

                //#critical, we need to first and foremost connect to Photon Online server. 
                PhotonNetwork.GameVersion = gameVersion;
                isConnecting = PhotonNetwork.ConnectUsingSettings();    

            }
        }



        #endregion

    }
}