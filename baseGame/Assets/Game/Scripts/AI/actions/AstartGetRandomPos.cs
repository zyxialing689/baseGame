using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AstartGetRandomPos : Action
{

    public SharedVector3List path;
    public override void OnStart()
    {
        Vector3 endPos = new Vector3(RandomMgr.Range(0, 200), RandomMgr.Range(0, 200), 0);
        path.Value = null;
        PathManager._instance.RequestPath(transform.position, endPos, (newPath) =>
        {
            this.path.Value = newPath;
        });
    }

    public override TaskStatus OnUpdate()
    {
        if (path.Value == null)
        {
            return TaskStatus.Running;
        }
        else
        {
            return TaskStatus.Success;
        }
    }
}
