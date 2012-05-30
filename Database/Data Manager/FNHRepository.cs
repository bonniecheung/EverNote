using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SampleApp.Database.Interfaces;

namespace SampleApp.Database.DataManager
{
    public class FNHRepository<T> : IFNHRepository<T>
    {
        public readonly IFNHSessionManager _sessionManager;

        public FNHRepository(IFNHSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        /// <summary>
        /// Retrieve an object instance from the database by id.
        /// </summary>
        /// <param name="id">Id of the object to retrieve.</param>
        /// <returns>The object instance to use in the application.</returns>
        public T RetrieveById(int id)
        {
            return _sessionManager.Session.Get<T>(id);
        }

        /// <summary>
        /// Update an object in the database.
        /// </summary>
        /// <param name="objectToUpdate">Object instance containing the information to change in the database.</param>
        public void Update(T objectToUpdate)
        {
            _sessionManager.Session.Update(objectToUpdate);
            _sessionManager.Commit();
        }

        /// <summary>
        /// Create an object in the database.
        /// </summary>
        /// <param name="objectToAdd">Object instance containing the information to add to the database.</param>
        public void Create(T objectToAdd)
        {
            _sessionManager.Session.Save(objectToAdd);
            _sessionManager.Commit();
        }

        /// <summary>
        /// Delete an object from the database.
        /// </summary>
        /// <param name="objectToDelete">Object instance containing the information to delete from the database.</param>
        public void Delete(T objectToDelete)
        {
            _sessionManager.Session.Delete(objectToDelete);
            _sessionManager.Commit();
        }
    }
}
