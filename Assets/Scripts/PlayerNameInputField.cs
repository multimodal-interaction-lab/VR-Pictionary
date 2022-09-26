using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;



namespace Com.MyCompany.MyGame
{

    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in game.
    /// </summary>
    [RequireComponent(typeof(PlayerNameInputField))]
    public class PlayerNameInputField : MonoBehaviour
    {

        #region Private Constants

        // Store the PlayerPrefKey to avoid typos
        const string PlayerNamePrefKey = "PlayerName";


        #endregion


        #region MonoBehaviour Callbacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase
        /// </summary>
        void Start()
        {

            string defaulName = string.Empty;
            InputField _inputField = this.GetComponent<InputField>();
            if (_inputField != null)
            {
                if (PlayerPrefs.HasKey(PlayerNamePrefKey))
                {
                    defaulName = PlayerPrefs.GetString(PlayerNamePrefKey);
                    _inputField.text = defaulName;
                }


            }

            PhotonNetwork.NickName = defaulName;

        }



        #endregion


        #region Public Methods

        /// <summary>
        /// Sets the name of the Player, and save it in the PlayerPrefs for future sessions.
        /// </summary>
        /// <param name="value">The name of the Player</param>

        public void SetPlayerName(string value)
        {
            //#Important
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player name is null or empty");
                return;
            }
            PhotonNetwork.NickName = value;


            PlayerPrefs.SetString(PlayerNamePrefKey, value);

        }


        #endregion


    }
}