using System;
using System.Collections.Generic;
using System.Reflection;

public class WorldTypeManager 
{
    public static void InitializeWorldAssemblies(World world, IBehaviourExecution behaviourExecution)
    {
        // Fix #5: 不再用 static 字段存 behaviourExecution，改为方法参数传递，避免并发/重入问题

        Assembly worldAssembly = world.GetType().Assembly;
        string nameSpace = world.GetType().Namespace;

        Type logicType = typeof(ILogicBehaviour);
        Type dataType = typeof(IDataBehaviour);
        Type msgType = typeof(IMsgBehaviour);

        Type[] typeArr = worldAssembly.GetTypes();
        List<TypeOrder> logicBehaviourList = new List<TypeOrder>();
        List<TypeOrder> dataBehaviourList = new List<TypeOrder>();
        List<TypeOrder> msgBehaviourList = new List<TypeOrder>();

        foreach (var type in typeArr)
        {
            if (!string.Equals(type.Namespace, nameSpace) || type.IsAbstract)
                continue;

            if (logicType.IsAssignableFrom(type))
                logicBehaviourList.Add(new TypeOrder(GetOrderIndex(type.Name, behaviourExecution?.GetLogicBehaviourExecution()), type));
            else if (dataType.IsAssignableFrom(type))
                dataBehaviourList.Add(new TypeOrder(GetOrderIndex(type.Name, behaviourExecution?.GetDataBehaviourExecution()), type));
            else if (msgType.IsAssignableFrom(type))
                msgBehaviourList.Add(new TypeOrder(GetOrderIndex(type.Name, behaviourExecution?.GetMsgBehaviourExecution()), type));
        }

        logicBehaviourList.Sort((a, b) => a.order.CompareTo(b.order));
        dataBehaviourList.Sort((a, b) => a.order.CompareTo(b.order));
        msgBehaviourList.Sort((a, b) => a.order.CompareTo(b.order));

        List<IDataBehaviour> dataList = new List<IDataBehaviour>();
        List<IMsgBehaviour> msgList = new List<IMsgBehaviour>();
        List<ILogicBehaviour> logicList = new List<ILogicBehaviour>();

        for (int i = 0; i < dataBehaviourList.Count; i++)
        {
            IDataBehaviour data = Activator.CreateInstance(dataBehaviourList[i].type) as IDataBehaviour;
            world.AddDataMgr(data);
            dataList.Add(data);
        }
        for (int i = 0; i < msgBehaviourList.Count; i++)
        {
            IMsgBehaviour msg = Activator.CreateInstance(msgBehaviourList[i].type) as IMsgBehaviour;
            world.AddMsgMgr(msg);
            msgList.Add(msg);
        }
        for (int i = 0; i < logicBehaviourList.Count; i++)
        {
            ILogicBehaviour logic = Activator.CreateInstance(logicBehaviourList[i].type) as ILogicBehaviour;
            world.AddLogicCtrl(logic);
            logicList.Add(logic);
        }

        world.OnInitCreate(dataList, msgList, logicList);
    }

    // Fix #5: 合并三个几乎相同的私有方法为一个，消除重复代码
    private static int GetOrderIndex(string typeName, string[] executionOrder)
    {
        if (executionOrder == null) return 999;
        for (int i = 0; i < executionOrder.Length; i++)
        {
            if (executionOrder[i] == typeName)
                return i;
        }
        return 999;
    }
}
