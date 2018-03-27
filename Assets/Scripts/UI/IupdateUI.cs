using System;
using System.Collections.Generic;

namespace Assets.Scripts.UI
{
    public interface IupdateUI
    {
        bool ifUpdate{get;}  //是否更新/
        void updateUI();   //执行更新/
    }
}
