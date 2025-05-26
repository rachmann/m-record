using m_record.Models;

namespace m_record.Interfaces
{
    public interface IAppSettingsService
    {
        AppSettings Current { get; }
        void Save();
        void Reload();
        void Update(Action<AppSettings> updateAction);
    }
}
