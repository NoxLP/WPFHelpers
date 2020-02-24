using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFHelpers.MessengerNSpace
{
    public static class Messenger
    {
        private static Dictionary<string, object> _Messages = new Dictionary<string, object>();
        private static List<string> _PermanentMessages = new List<string>();
        private static Dictionary<string, HashSet<IMessagesListener>> _Listeners = new Dictionary<string, HashSet<IMessagesListener>>();
        
        public static object GetMessage(string key)
        {
            if (!_Messages.ContainsKey(key))
                return null;

            object msg = _Messages[key];
            if(!_PermanentMessages.Contains(key))
                _Messages.Remove(key);

            return msg;
        }
        public static void StoreMessage(string key, object msg, bool permanent = false)
        {
            if (_Messages.ContainsKey(key))
                _Messages[key] = msg;
            else
                _Messages.Add(key, msg);

            if (permanent && !_PermanentMessages.Contains(key))
                _PermanentMessages.Add(key);
        }

        public static void RegisterListener(IMessagesListener listener, string key)
        {
            if (!_Listeners.ContainsKey(key))
            {
                _Listeners.Add(key, new HashSet<IMessagesListener>() { listener });
            }
            else
            {
                if (!_Listeners[key].Contains(listener))
                    _Listeners[key].Add(listener);
            }
            //else
            //    throw new ArgumentException("Se esta intentando añadir un listener que ya existe al messenger");
        }
        public static void SendGuidedMessage(string key)
        {
            if (!_Listeners.ContainsKey(key))
            {
                throw new ArgumentException("Se esta intentando enviar un mensaje a un listener que no esta registrado");
                return;
            }

            foreach (var listener in _Listeners[key])
            {
                listener.MsgRecieved(key, 1);
            }
        }
        public static void SendGuidedMessage(string key, object msg)
        {
            if (!_Listeners.ContainsKey(key))
            {
                throw new ArgumentException("Se esta intentando enviar un mensaje a un listener que no esta registrado");
                return;
            }

            foreach (var listener in _Listeners[key])
            {
                listener.MsgRecieved(key, msg);
            }
        }
        public static bool TrySendGuidedMessage(string key, object msg)
        {
            if (!_Listeners.ContainsKey(key))
            {
                return false;
            }

            foreach (var listener in _Listeners[key])
            {
                listener.MsgRecieved(key, msg);
            }

            return true;
        }
        public static IEnumerable<object> SendGuidedMessageWithResponse(string key)
        {
            if (!_Listeners.ContainsKey(key))
            {
                throw new ArgumentException("Se esta intentando enviar un mensaje a un listener que no esta registrado");
            }

            foreach (var listener in _Listeners[key])
            {
                yield return listener.MsgWithResponse(key, 1);
            }

            yield break;
        }
        public static IEnumerable<object> SendGuidedMessageWithResponse(string key, object msg)
        {
            if (!_Listeners.ContainsKey(key))
            {
                throw new ArgumentException("Se esta intentando enviar un mensaje a un listener que no esta registrado");
            }

            foreach (var listener in _Listeners[key])
            {
                yield return listener.MsgWithResponse(key, msg);
            }

            yield break;
        }
        public static void RemoveListener(string key)
        {
            _Listeners.Remove(key);
        }
    }
}
