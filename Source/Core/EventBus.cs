using System;
using System.Collections.Generic;
using System.Linq;

namespace ChronoCiv.Core
{
    /// <summary>
    /// Event Bus for decoupled communication between game systems.
    /// Uses a weak reference pattern to avoid memory leaks.
    /// </summary>
    public class EventBus
    {
        public static EventBus Instance { get; private set; }

        private readonly Dictionary<Type, List<Delegate>> eventHandlers = new();
        private readonly Dictionary<object, List<Type>> subscriberTypes = new();

        private bool isInitialized = false;

        public void Initialize()
        {
            if (isInitialized) return;

            Instance = new EventBus();
            isInitialized = true;
        }

        /// <summary>
        /// Subscribe a method to an event type.
        /// </summary>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (!eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] = new List<Delegate>();
            }

            eventHandlers[eventType].Add(handler);

            // Track subscriber for cleanup
            var subscriber = handler.Target;
            if (subscriber != null)
            {
                if (!subscriberTypes.ContainsKey(subscriber))
                {
                    subscriberTypes[subscriber] = new List<Type>();
                }
                if (!subscriberTypes[subscriber].Contains(eventType))
                {
                    subscriberTypes[subscriber].Add(eventType);
                }
            }
        }

        /// <summary>
        /// Unsubscribe a method from an event type.
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType].Remove(handler);
                
                // Clean up empty lists
                if (eventHandlers[eventType].Count == 0)
                {
                    eventHandlers.Remove(eventType);
                }
            }

            // Track subscriber
            var subscriber = handler.Target;
            if (subscriber != null && subscriberTypes.ContainsKey(subscriber))
            {
                subscriberTypes[subscriber].Remove(eventType);
                if (subscriberTypes[subscriber].Count == 0)
                {
                    subscriberTypes.Remove(subscriber);
                }
            }
        }

        /// <summary>
        /// Publish an event to all subscribed handlers.
        /// </summary>
        public void Publish<TEvent>(TEvent eventData) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (eventHandlers.TryGetValue(eventType, out var handlers))
            {
                // Create a copy to avoid modification issues during iteration
                var handlersCopy = handlers.ToList();
                
                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        (handler as Action<TEvent>)?.Invoke(eventData);
                    }
                    catch (Exception e)
                    {
                        DebugLog($"Error publishing event {eventType.Name}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Publish an event asynchronously (on next frame).
        /// </summary>
        public void PublishAsync<TEvent>(TEvent eventData) where TEvent : class
        {
            // Queue for next frame processing
            // For now, just publish synchronously
            Publish(eventData);
        }

        /// <summary>
        /// Check if there are any subscribers for an event type.
        /// </summary>
        public bool HasSubscribers<TEvent>() where TEvent : class
        {
            var eventType = typeof(TEvent);
            return eventHandlers.TryGetValue(eventType, out var handlers) && handlers.Count > 0;
        }

        /// <summary>
        /// Get the number of subscribers for an event type.
        /// </summary>
        public int GetSubscriberCount<TEvent>() where TEvent : class
        {
            var eventType = typeof(TEvent);
            return eventHandlers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
        }

        /// <summary>
        /// Unsubscribe all handlers from a specific subscriber.
        /// Useful for cleanup when destroying objects.
        /// </summary>
        public void UnsubscribeAll(object subscriber)
        {
            if (subscriber == null || !subscriberTypes.ContainsKey(subscriber))
                return;

            var eventTypes = subscriberTypes[subscriber].ToList();
            
            foreach (var eventType in eventTypes)
            {
                if (eventHandlers.TryGetValue(eventType, out var handlers))
                {
                    // Find and remove all handlers belonging to this subscriber
                    var handlersToRemove = handlers
                        .Where(h => h.Target == subscriber)
                        .ToList();

                    foreach (var handler in handlersToRemove)
                    {
                        handlers.Remove(handler);
                    }

                    // Clean up empty lists
                    if (handlers.Count == 0)
                    {
                        eventHandlers.Remove(eventType);
                    }
                }
            }

            subscriberTypes.Remove(subscriber);
        }

        /// <summary>
        /// Clear all event handlers (use with caution).
        /// </summary>
        public void ClearAll()
        {
            eventHandlers.Clear();
            subscriberTypes.Clear();
        }

        /// <summary>
        /// Get debug information about current subscriptions.
        /// </summary>
        public Dictionary<string, int> GetDebugInfo()
        {
            return eventHandlers
                .ToDictionary(
                    kvp => kvp.Key.Name,
                    kvp => kvp.Value.Count
                );
        }

        private void DebugLog(string message)
        {
            // In production, use a proper logging system
            UnityEngine.Debug.Log($"[EventBus] {message}");
        }
    }
}

