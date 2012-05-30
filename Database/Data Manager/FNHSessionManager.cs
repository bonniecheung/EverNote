using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SampleApp.Database.Interfaces;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Context;

namespace SampleApp.Database.DataManager
{
    public class FNHSessionManager<T> : IFNHSessionManager, IDisposable
    {

        #region Enumerations

        /// <summary>
        /// The database type that we are using to execute the operations.
        /// </summary>
        public enum DatabaseType
        {
            Postgres = 0,
            //MsSql = 0,
            MsSqlCe
        }

        #endregion

        #region Properties

        private readonly ISessionFactory _sessionFactory;

        public ISessionFactory SessionFactory
        {
            get { return _sessionFactory; }
        }

        public ISession Session
        {
            get
            {
                if (!ManagedWebSessionContext.HasBind(HttpContext.Current, SessionFactory))
                {
                    ManagedWebSessionContext.Bind(HttpContext.Current, SessionFactory.OpenSession());
                }
                return _sessionFactory.GetCurrentSession();
            }
        }

        private readonly ITransaction _transaction;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor to create a Fluent NHibernate Manager.
        /// </summary>
        /// <param name="dbConfigKey">The database connection string.</param>
        /// <param name="dbType">The database type to create a connection. </param>
        public FNHSessionManager(string dbConfigKey, DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.Postgres:
                    _sessionFactory = Fluently.Configure()
                        .Database(
                            PostgreSQLConfiguration.PostgreSQL82.ConnectionString(dbConfigKey)
                            //MsSqlConfiguration.MsSql2008
                                //.ConnectionString(dbConfigKey)
                        )
                        .Mappings(m => m.FluentMappings.AddFromAssemblyOf<T>())
                        .CurrentSessionContext(typeof(ManagedWebSessionContext).FullName)
                        .BuildSessionFactory();
                    break;
                case DatabaseType.MsSqlCe:
                    _sessionFactory = Fluently.Configure()
                        .Database(
                            MsSqlConfiguration.MsSql2008
                                .ConnectionString(c => c.FromConnectionStringWithKey(dbConfigKey))
                        )
                        .Mappings(m => m.FluentMappings.AddFromAssemblyOf<T>())
                        .CurrentSessionContext(typeof(ManagedWebSessionContext).FullName)
                        .BuildSessionFactory();
                    break;
            }

            if (HttpContext.Current != null && HttpContext.Current.ApplicationInstance != null)
            {
                HttpContext.Current.ApplicationInstance.EndRequest += (sender, args) => CleanUp();
            }

            _transaction = Session.BeginTransaction();
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Rollback the current transaction set.
        /// </summary>
        public void Rollback()
        {
            if (_transaction.IsActive)
                _transaction.Rollback();
        }

        /// <summary>
        /// Commit the current transaction set to the database.
        /// </summary>
        public void Commit()
        {
            if (_transaction.IsActive)
                _transaction.Commit();
        }

        /// <summary>
        /// Clean up the session.
        /// </summary>
        public void CleanUp()
        {
            CleanUp(HttpContext.Current, _sessionFactory);
        }

        /// <summary>
        /// Static function to clean up the session.
        /// </summary>
        /// <param name="context">The context to which the session has been bound.</param>
        /// <param name="sessionFactory">The session factory that contains the session.</param>
        public static void CleanUp(HttpContext context, ISessionFactory sessionFactory)
        {
            ISession session = ManagedWebSessionContext.Unbind(context, sessionFactory);

            if (session != null)
            {
                if (session.Transaction != null && session.Transaction.IsActive)
                {
                    session.Transaction.Rollback();
                }
                else if (context.Error == null && session.IsDirty())
                {
                    session.Flush();
                }
                session.Close();
            }
        }

        /// <summary>
        /// Dispose of the session factory.
        /// </summary>
        public void Dispose()
        {
            CleanUp();
            _sessionFactory.Dispose();
        }

        #endregion
    }
}
