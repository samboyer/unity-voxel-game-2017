using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {

    public float FPSUpdateInterval=0.5f;
    public KeyCode toggleKey = KeyCode.F10;

    bool showStats = false;

    Transform player;

	void Start () {
        player = GameObject.Find("PLAYER").transform;

        lastFPSUpdate = Time.realtimeSinceStartup;
    }

    int frames = 0;
    float lastFPSUpdate;
    float fps;

	void Update () {
        if (Input.GetKeyDown(toggleKey)) showStats = !showStats;

        frames++;
        if (Time.realtimeSinceStartup - lastFPSUpdate > FPSUpdateInterval)
        {
            lastFPSUpdate = Time.realtimeSinceStartup;
            fps = frames / FPSUpdateInterval;
            frames = 0;
        }
    }

    private void OnGUI()
    {
        if (showStats)
        {
            string content = "VoxelEngine v0.1.1\n";

            content += string.Format("{0} FPS\n", fps);
            content += string.Format("Player pos: {0}X, {1}Y, {2}Z. Bearing {3}°\n", player.position.x.ToString("f2"), player.position.y.ToString("f2"), player.position.z.ToString("f2"), player.Find("MODEL").localRotation.eulerAngles.y.ToString("f2"));
            content += string.Format("World time: {0}", Time.time.ToString("f1"));
            GUI.Label(new Rect(Screen.width/2, Screen.height/2-200, 500, 1000), content);
        }

    }
}
