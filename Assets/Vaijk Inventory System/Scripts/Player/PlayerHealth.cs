using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float health;

    private void Update ()
    {
        if (health > 100f)
            health = 100f;
    }

    private void OnGUI ()
    {
        GUILayout.Label(string.Format("<b><size=20>Health: {0}</size></b>", health));
    }
}