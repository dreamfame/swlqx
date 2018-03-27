using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class DynamicProgressBar:MonoBehaviour
    {
        private float repeatRate = 0.05f;    //方法执行的时间间隔，变化频率/
        private float lower = 0f;            //进度条起点值/
        private float upper = 0f;            //进度条目的值/
        private float deltaNum = 0.03f;      //进度条每次变化量，变化量/

        private bool isIncreaseValue = true; //是否属于增加值的操作/
        private int diffValue = 0;           //值变化抵达0或1时候要传出的差值，可正可负/
        private Action<bool, int> surplusValueAction = null;   //富余值，在进度动画结束时执行/
        private UISlider progressBar = null; //NGUI的进度条/

        /// <summary>
        /// 开启动态进度条，此进度条可进可退，只会在0和1之间发生变化，到达0、1或指定进度（01之间）时就会停止变化，并会执行回调，传出指定参数
        /// </summary>
        /// <param name="progressSlider">NGUI进度条</param>
        /// <param name="from">进度条变化的起点值，在0和1之间，调用时可不用处理范围，会自动处理范围</param>
        /// <param name="to">进度条变化的终点值，在0和1之间，调用时可不用处理范围，会自动处理范围</param>
        /// <param name="surplusValue">富余值，用于回调时传出值</param>
        /// <param name="valueAtLimitPos">值变化到达0、1或指定进度（01之间）时执行的回调,bool值为0说明进度条的进度是增，bool值小于0说明进度条的进度是减</param>
        /// <param name="changeRate">进度条变化的频率，按秒算</param>
        /// <param name="changeDeltaValue">进度条每次发生变化的变化量，01之间，不在此范围会被强制限定到此范围</param>
        public void StartDynamicBar(UISlider progressSlider, float from, float to, int surplusValue = 0, Action<bool, int> valueAtLimitPos = null, float changeRate = 0.05f, float changeDeltaValue = 0.03f)    //启动动态条/
        {
            if (progressSlider==null)
            {
                Debug.Log("进度条不能为空！请输入有效参数！");
                return;
            }
            progressBar = progressSlider;
            lower = from;
            upper = to;
            repeatRate = Mathf.Abs(changeRate);
            deltaNum = Mathf.Clamp01(changeDeltaValue);
            isIncreaseValue = from < to ? true : false;
            diffValue = surplusValue;
            surplusValueAction = valueAtLimitPos;
            progressBar.value = from;
            InvokeRepeating("DynamicBar", repeatRate, repeatRate);
        }

        /// <summary>
        /// 动态条（此方法会被重复执行，直到满足结束条件）
        /// </summary>
        private void DynamicBar()    //动态条/
        {
            if (isIncreaseValue)
            {
                if (lower + deltaNum < upper)
                {
                    if (lower + deltaNum < 1)
                    {
                        lower += deltaNum;
                        progressBar.value = lower;
                    }
                    else
                    {
                        lower = 1;
                        progressBar.value = lower;
                        CancelInvoke("DynamicBar");
                        if (surplusValueAction != null) surplusValueAction(isIncreaseValue, diffValue);
                    }

                }
                else
                {
                    lower = upper;
                    progressBar.value = lower;
                    CancelInvoke("DynamicBar");
                    if (surplusValueAction != null) surplusValueAction(isIncreaseValue, diffValue);
                }
            }
            else
            {
                if (lower - deltaNum > upper)
                {
                    if (lower - deltaNum > 0)
                    {
                        lower -= deltaNum;
                        progressBar.value = lower;
                    }
                    else
                    {
                        lower = 0;
                        progressBar.value = lower;
                        CancelInvoke("DynamicBar");
                        if (surplusValueAction != null) surplusValueAction(isIncreaseValue, diffValue);
                    }

                }
                else
                {
                    lower = upper;
                    progressBar.value = lower;
                    CancelInvoke("DynamicBar");
                    if (surplusValueAction != null) surplusValueAction(isIncreaseValue, diffValue);
                }
            }
        }


        void OnDisable()
        {
            if (IsInvoking("DynamicBar"))
            {
                if (isIncreaseValue)
                    progressBar.value = Mathf.Min(upper,1);
                else
                    progressBar.value = Mathf.Max(upper, 0);
                if (surplusValueAction != null) surplusValueAction(isIncreaseValue, diffValue);
                CancelInvoke("DynamicBar");
            }
        }
    }
}
