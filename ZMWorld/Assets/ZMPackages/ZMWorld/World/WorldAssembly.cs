using System.Collections.Generic;
using UnityEngine;

public partial class World
{
    public void AddLogicCtrl(ILogicBehaviour behaviour)
    {
        // Fix #2: 使用 Type 作为 key
        mLogicBehaviourDic[behaviour.GetType()] = behaviour;
    }

    public void AddDataMgr(IDataBehaviour behaviour)
    {
        mDataBehaviourDic[behaviour.GetType()] = behaviour;
    }

    public void AddMsgMgr(IMsgBehaviour behaviour)
    {
        mMsgBehaviourDic[behaviour.GetType()] = behaviour;
    }

    public void OnInitCreate(List<IDataBehaviour> dataList, List<IMsgBehaviour> msgList, List<ILogicBehaviour> logicList)
    {
        // Fix #6: 初始化顺序 Data → Msg → Logic，Logic 通常依赖前两层
        foreach (var item in dataList) item.OnCreate();
        foreach (var item in msgList) item.OnCreate();
        foreach (var item in logicList) item.OnCreate();
    }
}
