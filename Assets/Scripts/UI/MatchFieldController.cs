using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchFieldController : MonoBehaviour
{
    public GameObject homePlayerPrefab;
    public GameObject awayPlayerPrefab;
    public GameObject ballPrefab;

    public GameObject[] fieldObjects = new GameObject[3];

    JSONInfo snapshot;
    ScreenSnaps screensnaps;

    int currentscrn = 0;
    int rndrcount = 0;
    int RNDRRATE = 60;

    static float xunitperpixel = 26f / 5.5f;
    static float yunitperpixel = 13f / 3.5f;
    float xScale = 1f / xunitperpixel;
    float yScale = -1f / yunitperpixel;
    float zScale = 1f / xunitperpixel;

    // Start is called before the first frame update
    void Start()
    {
        string snapshotstr = PlayerPrefs.GetString("snapshot", "");

        snapshot = JSONInfo.CreateFromJSON(snapshotstr);
        screensnaps = ScreenSnaps.CreateFromJSON(snapshot.snapshot);

        ScreenSnap scrn = screensnaps.scrnsnaps[currentscrn++];
        fieldObjects[0] = Instantiate(ballPrefab,
            new Vector3(scrn.scrn[0].p[0] * xScale, scrn.scrn[0].p[1] * yScale, scrn.scrn[0].p[2] * zScale),
             Quaternion.identity);// new Quaternion(scrn.scrn[0].h[0], scrn.scrn[0].h[1], 1, 1));
        for (int i = 1; i < 2; i++)
        {
            fieldObjects[i] = Instantiate(homePlayerPrefab, 
                new Vector3(scrn.scrn[i].p[0] * xScale, scrn.scrn[i].p[1] * yScale, scrn.scrn[i].p[2]*zScale),
                 Quaternion.identity);// new Quaternion(scrn.scrn[i].h[0], scrn.scrn[i].h[1], 1, 1));
        }
        for (int i = 2; i < 3; i++)
        {
            fieldObjects[i] = Instantiate(awayPlayerPrefab,
                new Vector3(scrn.scrn[i].p[0] * xScale, scrn.scrn[i].p[1] * yScale, scrn.scrn[i].p[2] * zScale),
                Quaternion.identity);// new Quaternion(scrn.scrn[i].h[0], scrn.scrn[i].h[1], 1, 1));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (screensnaps.scrnsnaps.Length <= currentscrn)
        {
            // load the previous scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            return;
        }
        if (rndrcount++ % RNDRRATE != 1)
        {
            return;
        }
            
        ScreenSnap scrn = screensnaps.scrnsnaps[currentscrn++];
        for (int i = 0; i < 3; i++)
        {
            fieldObjects[i].gameObject.transform.position =
                new Vector3(scrn.scrn[i].p[0] * xScale, scrn.scrn[i].p[1] * yScale, scrn.scrn[i].p[2] * zScale);

            fieldObjects[i].gameObject.transform.right =
                new Vector3(scrn.scrn[i].h[0], -scrn.scrn[i].h[1], 0);
        }
    }
}
