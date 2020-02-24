using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFHelpers.View
{
    public interface ISingleInstanceWindow
    {
        bool IsOpen { get; }
    }
}
