using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFHelpers.MessengerNSpace
{
    public interface IMessagesListener
    {
        void MsgRecieved(string key, object msg);
        object MsgWithResponse(string key, object msg);
    }
}
