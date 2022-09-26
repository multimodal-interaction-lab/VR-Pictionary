using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;



namespace Com.MyCompany.MyGame
{

    public class GameManager : MonoBehaviourPunCallbacks
    {


        #region Public Fields

        public static GameManager Instance;

        [Tooltip("The prefab to use representing the player")]
        public GameObject playerPrefab;

        #endregion


        #region Photon Callbacks


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene. 
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }


        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); //not seen if you're the player connecting

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerEnteredRoom


                LoadArena();

            }


        }


        public override void OnPlayerLeftRoom(Player other)
        {

            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            LoadArena();

        }




        #endregion


        #region Public Methods

        void Start()
        {

            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are instantiating local player from {0}", SceneManagerHelper.ActiveSceneName);
                //We're in a room. Spawn a character for the local player. It gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);


            }
            else
            {

                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);

            }
            Instance = this;
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        #endregion


        #region Private Methods


        void LoadArena()
        {

            if (!PhotonNetwork.IsMasterClient)
            {

                Debug.LogError("PhotonNetwork : Trying to load a level but we are not the master Client");

            }

            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);

        }


        #endregion


    }
}