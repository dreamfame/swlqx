using UnityEngine;
using System.Collections;

public class UIObject : MonoBehaviour {


    public GameObject M2P_Answer_Panel;

    public GameObject P2M_Ask_Panel;

    public void ShowM2PAnswerPanel() 
    {
        M2P_Answer_Panel.SetActive(true);
    }

    public void HideM2PAnswerPanel()
    {
        M2P_Answer_Panel.SetActive(false);
    }

    public void ShowP2MAskPanel()
    {
        P2M_Ask_Panel.SetActive(true);
    }

    public void HideP2MAskPanel()
    {
        P2M_Ask_Panel.SetActive(false);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
