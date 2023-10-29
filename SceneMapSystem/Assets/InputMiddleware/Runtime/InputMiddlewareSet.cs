using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace TNS.InputMiddleware
{
    public class InputMiddlewareSet : ScriptableObject
    {
        private List<IInputMiddleware> InputMiddleware { get; set; } = new List<IInputMiddleware>();
        [SerializeField] private UnityEvent<IInputMiddleware> middlewareAddedEvent;
        [SerializeField] private UnityEvent<IInputMiddleware> middlewareRemovedEvent;

        public List<IInputMiddleware> GetSortedSet() => InputMiddleware.OrderBy(m  => m.Priority).ToList();

        public void AddMiddleware(IInputMiddleware inputMiddleware)
        {
            if (InputMiddleware.Contains(inputMiddleware)) return;

            InputMiddleware.Add(inputMiddleware);

            // Debug.Log($"Added {inputMiddleware.Name} to InputMiddlewareSet");

            middlewareAddedEvent?.Invoke(inputMiddleware);
        }

        public void RemoveMiddleware(IInputMiddleware inputMiddleware)
        {
            if (!InputMiddleware.Contains(inputMiddleware)) return;

            InputMiddleware.Remove(inputMiddleware);

            // Debug.Log($"Removed {inputMiddleware.Name} from InputMiddlewareSet");

            middlewareRemovedEvent?.Invoke(inputMiddleware);
        }
    }
}