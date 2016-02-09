using System;
using System.IO;

namespace BitMobile.Common.DataAccessLayer
{
    public interface IDal : ITranslator
    {
        string TranslateString(String s);
        void ClearStringCache();
        Stream GetScreenByName(String screenName);
        Stream GetConfiguration();
        Stream GetBusinessProcess(String bpName);
        Stream GetBusinessProcess2(String bpName);
        bool TryGetStyleByName(String styleName, out Stream style);
        Stream GetImageByName(String imagePath);
        bool TryGetScriptByName(String scriptPath, out Stream script);
        void UpdateCredentials(String userName, String password);
        string[] GetResources(string type);
        Guid UserId { get; }
        string DeviceId { get; }
        string UserEmail { get; }
        string ConfigName { get; }
        string ConfigVersion { get; }
        string ResourceVersion { get; }
        object Dao { get; }
        void Dispose();
        void LoadSolution(bool clearCache, bool syncAfterLoad, SyncEventHandler handler);
        void RefreshAsync(SyncEventHandler handler);
        void Wait();
        void SaveChanges();
        void CancelChanges();
    }
}