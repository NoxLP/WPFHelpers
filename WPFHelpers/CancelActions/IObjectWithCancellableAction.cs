using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFHelpers.CancelActions
{
    public interface IObjectWithCancellableAction : IEquatable<IObjectWithCancellableAction>
    {
        string CancellableId { get; }
        Action CancelCurrentActionDelegate { get; }
        Func<Task> AsyncCancelCurrentActionDelegate { get; }
    }
}
