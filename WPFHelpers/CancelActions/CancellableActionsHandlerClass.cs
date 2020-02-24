using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFHelpers;

namespace WPFHelpers.CancelActions
{
    public class CancellableActionsHandlerClass
    {
        static CancellableActionsHandlerClass()
        {
            Instance = new CancellableActionsHandlerClass();
        }
        private CancellableActionsHandlerClass()
        {
            _OnGoingCancellableActions = new HashSet<IObjectWithCancellableAction>();
        }

        private HashSet<IObjectWithCancellableAction> _OnGoingCancellableActions;

        public static CancellableActionsHandlerClass Instance { get; private set; }

        public void NewCancellableAction(IObjectWithCancellableAction objectWith)
        {
            _OnGoingCancellableActions.Add(objectWith);
        }
        public void CancellableActionFinished(IObjectWithCancellableAction objectWith)
        {
            _OnGoingCancellableActions.Remove(objectWith);
        }

        #region cancel actions
        public void CancelAllOnGoingActions()
        {
            try
            {
                var toRemove = new HashSet<IObjectWithCancellableAction>();

                foreach (var item in _OnGoingCancellableActions)
                {
                    item.CancelCurrentActionDelegate?.Invoke();

                    toRemove.Add(item);
                }

                _OnGoingCancellableActions.RemoveWhere(x => toRemove.Contains(x));
            }
            catch(Exception e)
            {
                e.ShowException();
            }
        }
        public void CancelAllOnGoingActions(Predicate<IObjectWithCancellableAction> conditionForCancellation)
        {
            try
            {
                var toRemove = new HashSet<IObjectWithCancellableAction>();
                foreach (var item in _OnGoingCancellableActions)
                {
                    if (item.CancelCurrentActionDelegate != null && conditionForCancellation(item))
                    {
                        item.CancelCurrentActionDelegate();
                    }
                    toRemove.Add(item);
                }

                _OnGoingCancellableActions.RemoveWhere(x => toRemove.Contains(x));
            }
            catch (Exception e)
            {
                e.ShowException();
            }
        }
        public async Task CancelAllOnGoingActionsAsync()
        {
            List<Task> delegates = new List<Task>();
            var toRemove = new HashSet<IObjectWithCancellableAction>();
            foreach (var item in _OnGoingCancellableActions)
            {
                if (item.AsyncCancelCurrentActionDelegate != null)
                {
                    delegates.Add(Task.Run(() => item.AsyncCancelCurrentActionDelegate()));
                }
                toRemove.Add(item);
            }

            try
            {
                await Task.WhenAll(delegates);
                _OnGoingCancellableActions.RemoveWhere(x => toRemove.Contains(x));
            }
            catch(Exception e)
            {
                e.ShowException();
            }
        }
        public async Task CancelAllOnGoingActionsAsync(Predicate<IObjectWithCancellableAction> conditionForCancellation)
        {
            List<Task> delegates = new List<Task>();
            var toRemove = new HashSet<IObjectWithCancellableAction>();
            foreach (var item in _OnGoingCancellableActions)
            {
                if (item.AsyncCancelCurrentActionDelegate != null && conditionForCancellation(item))
                {
                    delegates.Add(Task.Run(() => item.AsyncCancelCurrentActionDelegate()));
                }
                toRemove.Add(item);
            }

            try
            {
                await Task.WhenAll(delegates);
                _OnGoingCancellableActions.RemoveWhere(x => toRemove.Contains(x));
            }
            catch (Exception e)
            {
                e.ShowException();
            }
        }

        public bool CancelObjectOnGoingAction(IObjectWithCancellableAction objectWith)
        {
            if (!_OnGoingCancellableActions.Contains(objectWith))
                return false;

            try
            {
                if (objectWith.CancelCurrentActionDelegate != null)
                {
                    objectWith.CancelCurrentActionDelegate();
                    _OnGoingCancellableActions.Remove(objectWith);
                }
                else
                    return false;
            }
            catch(Exception e)
            {
                e.ShowException();
                return false;
            }

            return true;
        }
        public async Task<bool> CancelObjectOnGoingActionAsync(IObjectWithCancellableAction objectWith)
        {
            if (!_OnGoingCancellableActions.Contains(objectWith))
                return false;

            try
            {
                if (objectWith.AsyncCancelCurrentActionDelegate != null)
                {
                    await objectWith.AsyncCancelCurrentActionDelegate();
                    _OnGoingCancellableActions.Remove(objectWith);
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                e.ShowException();
                return false;
            }

            return true;
        }
        #endregion
    }
}
