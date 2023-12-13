using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public User user;

    public void SetUpPlayer() {
        Debug.Log(user.username);
    }
}
