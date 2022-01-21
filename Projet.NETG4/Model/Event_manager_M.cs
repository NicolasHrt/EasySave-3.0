using System;
using EventListner;
using System.Collections.Generic;

namespace EventManager
{
    /// <summary>
    /// The event manager is useful to send real time updates of the saveworks
    /// </summary>
    public class EventManager_M
    {
        /// <summary>
        /// Create the list who will contain the observer's classes
        /// </summary>
        public List<IEventListner> observers = new List<IEventListner>();

        /// <summary>
        /// Methode by wich the objects are added to the event listner list
        /// </summary>
        /// <param name="observer"></param>
        public void Register(IEventListner observer)
        {
            observers.Add(observer);
        }

        /// <summary>
        /// Methode by wich the objects are remove to the event listner list
        /// </summary>
        /// <param name="observer"></param>
        public void Unregister(IEventListner observer)
        {
            observers.Remove(observer);
        }

        /// <summary>
        /// method which can be used to activate the update method for objects in the list of observers
        /// </summary>
        /// <param name="infoUpdate"></param>
        /// <param name="listUpdate"></param>
        public void Notify(string infoUpdate, Dictionary<string, string> listUpdate)
        {
            foreach (IEventListner o in observers)
            {
                o.Update(infoUpdate, listUpdate);
            }
        }
    }
}
