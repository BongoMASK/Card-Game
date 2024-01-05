using Photon.Pun;
using Photon.Realtime;
using Properties;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    Player player;

    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text timeText;

    [SerializeField] Image backgroundImage;
    [SerializeField] Slider slider;

    float turnDuration;

    public float ElapsedTimeInTurn {
        get { return ((float)(PhotonNetwork.ServerTimestamp - PhotonNetwork.CurrentRoom.GetTurnStartTime())) / 1000.0f; }
    }

    public float RemainingSecondsInTurn {
        get { return Mathf.Max(0f, this.turnDuration - this.ElapsedTimeInTurn); }
    }

    public void SetUpPlayerListItem(Player p, Color32 color, float turnDuration) {
        player = p;
        playerNameText.text = player.NickName;
        backgroundImage.color = color;
        
        slider.maxValue = turnDuration;
        this.turnDuration = turnDuration;
        timeText.text = "";
        slider.value = 0;
    }

    private void SetUpTime() {
        StartCoroutine(Cor_SetupTime());
    }

    IEnumerator Cor_SetupTime() {
        while(PhotonNetwork.CurrentRoom.GetActivePlayer() == player) {
            timeText.text = ((int)RemainingSecondsInTurn).ToString();
            slider.value = RemainingSecondsInTurn;

            yield return null;
        }

        // Set back to default after it is over
        timeText.text = "";
        slider.value = 0;
    }

    #region PUN CallBack Functions

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        if(propertiesThatChanged.ContainsKey(RoomProps.ActivePlayerPropKey)) {
            if ((Player)PhotonNetwork.CurrentRoom.CustomProperties[RoomProps.ActivePlayerPropKey] == player) {
                SetUpTime();
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (changedProps.ContainsKey(PlayerProps.MaxManaPropKey) || changedProps.ContainsKey(PlayerProps.ManaPropKey)) {
            if (targetPlayer != this.player)
                return;

            int mana = (int)this.player.CustomProperties[PlayerProps.ManaPropKey];
            int maxMana = (int)this.player.CustomProperties[PlayerProps.MaxManaPropKey];

            playerNameText.text  = player.NickName + " (" + mana + "/" + maxMana + ")";
        }
    }

    #endregion
}
