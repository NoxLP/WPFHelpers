using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFHelpers.MessengerNSpace
{
    public abstract class aMessagesListener : IMessagesListener
    {
        public aMessagesListener()
        {
            RegisterListener();
        }

        protected Dictionary<string, Action<object>> _MessagesActions;
        public virtual void MsgRecieved(string key, object msg)
        {
            if (!_MessagesActions.ContainsKey(key))
                return;

            _MessagesActions[key](msg);
        }
        public virtual object MsgWithResponse(string key , object msg)
        {
            throw new NotImplementedException();
        }
        protected abstract void RegisterListener();
    }
}
