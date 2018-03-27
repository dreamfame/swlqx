using UnityEngine;
using System.Collections;

public class UIData : MonoBehaviour {

    public GameObject DialogAtCenter = null;

    public GameObject VideoControl = null;

    public GameObject VolumeControl = null;

    public GameObject totalTime = null;

    public GameObject curTime = null;

    public GameObject Play = null;

    public GameObject Pause = null;

    public GameObject BlackBoard = null;
    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        if (VolumeControl != null&&VolumeControl.active)
        {
            var volume = VolumeControl.GetComponent<UISlider>().value;
            var movieManager = GameObject.Find("MovieManager");
            if (movieManager != null)
            {
                movieManager.GetComponent<MovieManage>().SetVolume(volume);
            }
        }
	}
}
