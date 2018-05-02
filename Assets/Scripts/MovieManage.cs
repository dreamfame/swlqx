using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.UI;

public class MovieManage: SingletonUI<MovieManage>
{
    public GameObject FigerGestures;
    private string rootPath = "Movie/";//存放视频的地址
    private MovieTexture movieTexture;
    private UITexture uiTexture;
    private AudioSource as1;
    private int State = 1;//0为视频暂停状态；1为视频播放状态;

    private Action<string> action;
    private  bool checkFlag = false;
    private string currentPlayMovieName;
    private int width = 0;
    private int height = 0;

    private bool SPFlag = true;
    private bool autouShut;
    private Transform uicamera = null;
    public int videoNo = 0;

    public void setAction(Action<string> action)
    {
        this.action = action;
    }
	// Use this for initialization
	void Start () {
        uicamera = GameObject.Find("UI Root").transform.Find("Camera");
    }
	// Update is called once per frame
	void Update () {
        if (movieTexture != null && checkFlag)
        {  
            if (SPFlag)
            {
                if (!movieTexture.isPlaying)
                {
                    if(action != null)
                        action(this.currentPlayMovieName);
                    checkFlag = false;
                    movieTexture.Stop();
                    if(autouShut)  
                        DestroyMovie();//
                    enabled = false;//任务执行完毕：使得该类不再继续执行update方法
                }
            }
        } 
	}
    /// <summary>
    /// 播放视频
    /// </summary>
    /// <param name="movie"></param>
    public void PlayMovie(string movieName,bool autouShut_= true)
    {
        width = 1920;
        height = 1080;
        MovieManage sp = GameObject.Find("Movie").GetComponent<MovieManage>();
        if (sp == null) sp = GameObject.Find("Movie").AddComponent<MovieManage>();
        if (!sp.enabled)
        {
            sp.enabled = true; //任务开始执行执行：使得该类开始执行update方法
        }

        this.autouShut = autouShut_;
        if (uiTexture == null)
        {
            Creat(movieName);
        }
        if (movieTexture != null)
        {
            movieTexture.Play();
            if (movieTexture.audioClip != null)
            {
                uiTexture.gameObject.AddComponent<AudioSource>();
                var as_array = uiTexture.gameObject.GetComponents(typeof(AudioSource));
                as1 = (AudioSource)as_array[0];
                as1.clip = (AudioClip)movieTexture.audioClip;
                as1.Play();

            } 
        }
        checkFlag = true;
        SPFlag = true;
    }
    /// <summary>
    /// 停止播放
    /// </summary>
    public void StopMovie()
    {
        if (movieTexture.audioClip != null)
        {
            as1.Stop();
        }
        SPFlag = false;
        movieTexture.Stop();
    }

    /// <summary>
    /// 继续播放
    /// </summary>
    public void ContinueMovie()
    {
        State = 1;
        movieTexture.Play();
        SPFlag = true;
        if (movieTexture.audioClip != null)
        {
            as1.Play();
        }
    }

    /// <summary>
    /// 暂停视频
    /// </summary>
    public void PauseMovie()
    {
        State = 0;
        movieTexture.Pause();
        SPFlag = false;
        if (movieTexture.audioClip != null)
        {
            as1.Pause();
        }
    }
    /// <summary>
    /// 销毁视频及其相应组件
    /// </summary>
    /// <param name="movieName"></param>
    public void DestroyMovie()
    {
        Camera.main.GetComponent<main>().CloseMovie();
        if (uiTexture != null)
        {
            State = 1;
            Transform uicamera = GameObject.Find("UI Root").transform.Find("Camera");
            movieTexture.Stop();
            Destroy(transform.Find("tt").gameObject);
            checkFlag = false;
            action = null;
        }
        if (enabled)
            enabled = false;
        videoNo++;
        GameObject.Find("UI Root").transform.Find("Camera").GetComponent<main>().isPlayed = false;
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    /// <param name="volume"></param>
    public void SetVolume(float volume)
    {
        if (movieTexture != null)
        {
            if (movieTexture.audioClip != null)
            {
                as1.volume = volume;
            }
        }
    }
   
    private void Creat(string movieName)
    {
        movieTexture = (MovieTexture)Resources.Load(rootPath + movieName, typeof(MovieTexture));
        if (movieTexture != null)
        {

            this.currentPlayMovieName = movieName;
            uiTexture = NGUITools.AddChild<UITexture>(this.gameObject);
            uiTexture.name = "tt";
            Transform uicamera = GameObject.Find("UI Root").transform.Find("Camera");
            uiTexture.transform.localPosition = new Vector3(0, 0, 0);
            uiTexture.transform.localPosition = new Vector3(0, 0, 0);
            uiTexture.width = width;
            uiTexture.height = height;
            //动态添加boxcollider
            uiTexture.gameObject.AddComponent<BoxCollider>();
            //在这里资源加载，movie是视频的文件名不需要后缀名
            uiTexture.mainTexture = movieTexture;
        }
        
    }
    
}
