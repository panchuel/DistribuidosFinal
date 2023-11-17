using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GamePlayer : MonoBehaviour
{
    public string Id { get; set; }
    public string Username { get; set; }
    public Vector2 Position { get { return transform.position; } }

    public TextMeshProUGUI usernameText;

    private void Start()
    {
        usernameText.text = Username;
    }
}
