namespace CustomCompanions.Framework.Interfaces
{
    public interface IJsonAssetsApi
    {
        void LoadAssets(string path);

        int GetObjectId(string name);
    }
}
