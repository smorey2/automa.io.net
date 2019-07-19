namespace Automa.IO
{
    /// <summary>
    /// ITryMethod
    /// </summary>
    public interface ITryMethod
    {
        bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null);
        void TryLogin(bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M);
    }
}
