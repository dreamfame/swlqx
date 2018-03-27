using UnityEngine;
using System.Collections;

public class main : MonoBehaviour {

    public GameObject Movie;

    public bool isPlayed = false;
    private int videoNo = 0;
    private float DeltaT = 0f;

	// Use this for initialization
	void Start () {
        Movie.SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {
        if (!isPlayed)
        {
            isPlayed = true;
            MovieManage m = Movie.GetComponent<MovieManage>();
            if (m.videoNo == 0)
            {
                m.PlayMovie("a1");
            }
            else if (m.videoNo == 1) 
            {
                Movie.SetActive(true);
                m.PlayMovie("a2");
            }
        }
    }

    public void CloseMovie() 
    {
        Movie.SetActive(false);
    }
}
