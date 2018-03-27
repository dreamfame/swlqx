using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UI
{
    #region   UI中可能会用到的定义


    /// <summary>
    /// 子界面类型,功能按钮打开的窗口的类型
    /// </summary>
    public enum WndType
    {
        None = 0,   //默认/
        
    }

    //-------------------------------------------------//
    #endregion

    /// <summary>
    /// UI中事件所使用的委托类型
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <param name="sender"></param>
    /// <param name="evtMsg"></param>
    public delegate void UIEventHandler<S, E>(S sender, E evtMsg);

    /// <summary>
    /// 所有UI类的基类
    /// </summary>
    public abstract class UIbase : MonoBehaviour
    {
        /// <summary>
        /// 对齐方式
        /// </summary>
        public enum Align
        {
            Left = 0,
            Center,
            Right
        }

        /// <summary>
        /// NGUI中的depth
        /// </summary>
        public abstract int Depth
        {
            get;
            set;
        }


        /// <summary>
        /// 清除指定父物体下面的所有子物体
        /// </summary>
        /// <param name="root"></param>
        protected void clearChildren(Transform root)
        {
            int childrenNum = root.childCount;
            for (int i = 0; i < childrenNum; i++)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 在指定父物体下面根据给定的大小、位置、旋转生成子物体
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="preObj"></param>
        /// <param name="localScale"></param>
        /// <param name="localPos"></param>
        /// <param name="localEulerAngle"></param>
        /// <returns></returns>
        protected GameObject generate(Transform parentNode, GameObject preObj, Vector3 localScale, Vector3 localPos, Vector3 localEulerAngle)
        {
            GameObject obj = Instantiate(preObj) as GameObject;
            obj.transform.parent = parentNode;
            obj.transform.localScale = localScale;
            obj.transform.localPosition = localPos;
            obj.transform.localEulerAngles = localEulerAngle;
            return obj;
        }

        /// <summary>
        /// 在指定父物体下面根据给定的大小、位置、旋转生成子物体，返回指定泛型的组件，若无，返回空
        /// </summary>
        /// <typeparam name="T">继承自MonoBehaviour的组件类型</typeparam>
        /// <param name="parentNode"></param>
        /// <param name="preObj"></param>
        /// <param name="localScale"></param>
        /// <param name="localPos"></param>
        /// <param name="localEulerAngle"></param>
        /// <returns></returns>
        protected T generate<T>(Transform parentNode, GameObject preObj, Vector3 localScale, Vector3 localPos, Vector3 localEulerAngle) where T:MonoBehaviour
        {
            GameObject obj = Instantiate(preObj) as GameObject;
            obj.transform.parent = parentNode;
            obj.transform.localScale = localScale;
            obj.transform.localPosition = localPos;
            obj.transform.localEulerAngles = localEulerAngle;
            return obj.GetComponent<T>();
        }


        /// <summary>
        /// 动态创建label并按指定行间距从上到下垂直排列，并返回最后一个label最低点的相对坐标
        /// </summary>
        /// <param name="contents">要在label里显示的文本内容的list，一个string对应一个label</param>
        /// <param name="parent">挂载label的父物体</param>
        /// <param name="align">label里的文本的对齐方式</param>
        /// <param name="fontsize">字号</param>
        /// <param name="font">字体</param>
        /// <param name="labelDepth">label的depth</param>
        /// <param name="lineSpace">行间距</param>
        /// <param name="firstLabelPos">第一个label的生成坐标</param>
        /// <param name="labelWidth">label的宽度值</param>
        /// <returns>返回最后一个label最低点的相对坐标</returns>
        protected Vector3 spanLabels(List<string> contents, GameObject parent, Align align, int fontsize, Font font, int labelDepth, float lineSpace,Vector3 firstLabelPos,int labelWidth)
        {
            NGUIText.Alignment alinmt = NGUIText.Alignment.Left ;   //对齐方式,默认左对齐/
            switch(align)
            {
                case Align.Center:
                    alinmt = NGUIText.Alignment.Center;
                    break;
                case Align.Right:
                    alinmt = NGUIText.Alignment.Right;
                    break;
            }

            //要返回的值/
            Vector3 bottomLocalPos = Vector3.zero;

            //生成uilabel/
            for (int i = 0; i < contents.Count; i++)
            {
                UILabel lbl = NGUITools.AddChild<UILabel>(parent);
                lbl.applyGradient = false;  //关闭文字的颜色渐变/
                lbl.pivot = UIWidget.Pivot.TopLeft; //将坐标轴位置都设置为label的右上角/
                lbl.overflowMethod = UILabel.Overflow.ResizeHeight;  //文字溢出时自动增加文本框高度/
                lbl.alignment = alinmt;
                if(font!=null)lbl.ambigiousFont = font;
                lbl.fontSize = fontsize;
                lbl.depth = labelDepth;  //label的深度/
                lbl.width = labelWidth;  //label的宽度/
                lbl.text = contents[i];              

                //设置uilabel的位置/
                if (i > 0)
                {
                    lbl.transform.localPosition = bottomLocalPos - new Vector3(0f, lineSpace, 0f);
                    bottomLocalPos = lbl.transform.localPosition - new Vector3(0f, lbl.height, 0f);
                }
                else
                {
                    lbl.transform.localPosition = firstLabelPos;
                    bottomLocalPos = firstLabelPos - new Vector3(0f, lbl.height, 0f);
                }
                Debug.Log(lbl.height);
            }

            return bottomLocalPos;
        }


        protected void showTip()
        {

        }


        /// <summary>
        /// 开启动态进度条，此进度条可进可退，只会在0和1之间发生变化，到达0、1或指定进度（01之间）时就会停止变化，并会执行回调，传出指定参数
        /// </summary>
        /// <param name="progressSlider">NGUI进度条</param>
        /// <param name="from">进度条变化的起点值，在0和1之间，调用时可不用处理范围，会自动处理范围</param>
        /// <param name="to">进度条变化的终点值，在0和1之间，调用时可不用处理范围，会自动处理范围</param>
        /// <param name="surplusValue">富余值，用于回调时传出值</param>
        /// <param name="valueAtLimitPos">值变化到达0、1或指定进度（01之间）时执行的回调</param>
        /// <param name="changeRate">进度条变化的频率，按秒算</param>
        /// <param name="changeDeltaValue">进度条每次发生变化的变化量，01之间，不在此范围会被强制限定到此范围</param>
        protected void showDynamicProgressBar(UISlider progressSlider, float from, float to, int surplusValue = 0, Action<bool, int> valueAtLimitPos = null, float changeRate = 0.05f, float changeDeltaValue = 0.03f)
        {
            if (progressSlider == null) return;
            DynamicProgressBar bar = progressSlider.GetComponent<DynamicProgressBar>();
            if (bar == null)
            {
                bar = progressSlider.gameObject.AddComponent<DynamicProgressBar>();
            }
            bar.StartDynamicBar(progressSlider, from, to, surplusValue, valueAtLimitPos, changeRate, changeDeltaValue);
        }

    }


    /// <summary>
    /// UI按钮抽象类
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="E"></typeparam>
    public abstract class UIbtn<S,E> : UIbase
    {
        /// <summary>
        /// 一些btn常用事件
        /// </summary>
        public abstract event UIEventHandler<S, E> SingleClick;
        public abstract event UIEventHandler<S, E> MouseEnter;
        public abstract event UIEventHandler<S, E> MouseExit;
    }


    /// <summary>
    /// UI窗口抽象类
    /// </summary>
    //public abstract class UIwnd<T> : UIbase where T : UIwnd<T>
    public abstract class UIwnd : UIbase, IupdateUI
    {

        #region  UIupdate()---静态界面更新方法
        protected static List<IupdateUI> children = new List<IupdateUI>();

        protected virtual void Awake(){ children.Add(this);}

        public static void UIupdate()
        {
            foreach (IupdateUI item in children)
            {
                if (item.ifUpdate) item.updateUI();
            }
        }
        #endregion


        #region   实现可重写的界面更新接口方法
        /// <summary>
        /// 是否更新【在每个子类中重写界面更新的条件】
        /// </summary>
        public virtual bool ifUpdate
        {
            get{return false;}
        }


        /// <summary>
        /// 执行更新【在每个子类中重写界面更新的方法】
        /// </summary>
        public virtual void updateUI()
        {
            Debug.Log("UI更新的接口方法->" + this.name);
        }
        #endregion


        /// <summary>
        /// 为窗口标题栏区域添加可拖动功能
        /// </summary>
        /// <param name="titleSprite"></param>
        protected void addTitleDrag(UISprite titleSprite)
        {
            if (titleSprite.GetComponent<BoxCollider>() == null)
            {
                titleSprite.gameObject.AddComponent<BoxCollider>().isTrigger = true;
            }
            else
            {
                titleSprite.GetComponent<BoxCollider>().isTrigger = true;
            }
            titleSprite.autoResizeBoxCollider = true;
            UIDragObject dragObj = titleSprite.gameObject.AddComponent<UIDragObject>();
            dragObj.target = titleSprite.transform;
            dragObj.momentumAmount = 0f;
            dragObj.dragEffect = UIDragObject.DragEffect.None;
            dragObj.restrictWithinPanel = true;
            dragObj.contentRect = titleSprite.GetComponent<UIWidget>();
        }


        /// <summary>
        /// 添加可拖拽区域 ,响应拖拽的区域面积由uiwidget的size决定
        /// </summary>
        /// <param name="widget"></param>
        protected void addTitleDrag(UIWidget widget)
        {
            if (widget.GetComponent<BoxCollider>() == null)
            {
                widget.gameObject.AddComponent<BoxCollider>().isTrigger = true;
            }
            else
            {
                widget.GetComponent<BoxCollider>().isTrigger = true;
            }
            widget.autoResizeBoxCollider = true;
            widget.GetComponent<BoxCollider>().size = new Vector3(widget.localSize.x,widget.localSize.y,0f);
            UIDragObject dragObj = widget.gameObject.AddComponent<UIDragObject>();
            dragObj.target = widget.transform;
            dragObj.momentumAmount = 0f;
            dragObj.dragEffect = UIDragObject.DragEffect.Momentum;
            dragObj.restrictWithinPanel = true;
            dragObj.contentRect = widget;
  
        }

        /// <summary>
        /// 打开
        /// </summary>
        public abstract void open();

        /// <summary>
        /// 关闭
        /// </summary>
        public abstract void close();

    }
}
