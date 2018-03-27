using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Assets.Scripts.UI
{
    public class SingletonUI<T> : UIModel where T : SingletonUI<T>
    {
        private static T instance;

        public static T getInstance()
        {
            if (!instance)
            {
                instance = (T)GameObject.FindObjectOfType(typeof(T));
                if (!instance)
                {
                    //Logger.error(Module.Framework, "There needs to be one active " + typeof(T) + " script on a GameObject in your scene.");
                }
            }
            return instance;
        }
    }
}
