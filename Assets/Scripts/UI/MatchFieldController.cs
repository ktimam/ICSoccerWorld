using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchFieldController : MonoBehaviour
{
    public GameObject homePlayerPrefab;
    public GameObject awayPlayerPrefab;

    public GameObject[] fieldObjects = new GameObject[11];

    JSONInfo snapshot;
    ScreenSnaps screensnaps;

    int currentscrn = 0;
    int rndrcount = 0;
    int RNDRRATE = 10;

    static float unitperpixel = 64f;
    float xScale = 1f / unitperpixel;
    float yScale = -1f / unitperpixel;
    float zScale = 1f / unitperpixel;

    // Start is called before the first frame update
    void Start()
    {
        string snapshotstr = PlayerPrefs.GetString("snapshot", "");

        snapshot = JSONInfo.CreateFromJSON(snapshotstr);
        screensnaps = ScreenSnaps.CreateFromJSON(snapshot.snapshot);

        ScreenSnap scrn = screensnaps.scrnsnaps[currentscrn++];
        for (int i = 1; i < 6; i++)
        {
            fieldObjects[i] = Instantiate(homePlayerPrefab, 
                new Vector3(scrn.scrn[i].p[0] * xScale, scrn.scrn[i].p[1] * yScale, scrn.scrn[i].p[2]*zScale), 
                Quaternion.identity);
        }
        for (int i = 6; i < 11; i++)
        {
            fieldObjects[i] = Instantiate(awayPlayerPrefab,
                new Vector3(scrn.scrn[i].p[0] * xScale, scrn.scrn[i].p[1] * yScale, scrn.scrn[i].p[2] * zScale),
                Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(screensnaps.scrnsnaps.Length <= currentscrn || rndrcount++ % RNDRRATE != 1)
        {
            return;
        }
            
        ScreenSnap scrn = screensnaps.scrnsnaps[currentscrn++];
        for (int i = 1; i < 11; i++)
        {
            fieldObjects[i].gameObject.transform.position =
                new Vector3(scrn.scrn[i].p[0] * xScale, scrn.scrn[i].p[1] * yScale, scrn.scrn[i].p[2] * zScale);
        }
    }
}
