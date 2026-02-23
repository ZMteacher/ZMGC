using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class World
{
    public  void AddLogicCtrl(ILogicBehaviour behaviour)
    {
        mLogicBehaviourDic.Add(behaviour.GetType().Name, behaviour);
    }

    public  void AddDataMgr(IDataBehaviour behaviour)
    {
        mDataBehaviourDic.Add(behaviour.GetType().Name, behaviour);
    }
    public  void AddMsgMgr(IMsgBehaviour behaviour)
    {
        mMsgBehaviourDic.Add(behaviour.GetType().Name, behaviour);
    }
    public  void OnInitCreate(List<IDataBehaviour> dataList,List<IMsgBehaviour> msgList,List<ILogicBehaviour> logicList)
    {
        foreach (var item in logicList)
        {
            item.OnCreate();
        }
        foreach (var item in dataList)
        {
            item.OnCreate();
        }
        foreach (var item in msgList)
        {
            item.OnCreate();
        }
    }

  
    public static void ClearWorldBehaviours()
    {
        mMsgBehaviourDic.Clear();
        mDataBehaviourDic.Clear();
        mLogicBehaviourDic.Clear();
    }
}
