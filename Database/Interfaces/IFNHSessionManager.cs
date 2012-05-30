using NHibernate;

namespace SampleApp.Database.Interfaces
{
    public interface IFNHSessionManager
    {
        ISession Session { get; }

        void CleanUp();
        void Rollback();
        void Commit();
    }
}
