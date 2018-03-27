using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Event
{

    /// <summary>
    /// 目标基类，被观察者
    /// </summary>
    public class ModelBase : MonoBehaviour,IBaseModel
    {

        public delegate void SubEventHandler();

        public event SubEventHandler SubEvent;



        #region IBaseModel 成员

        public virtual void Notify()
        {
        }

        #endregion
    }
}
