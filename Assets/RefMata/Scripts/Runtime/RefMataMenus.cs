#if UNITY_EDITOR
using StartupFlag;

namespace RefMata
{
    [StartupFlag("RefMata/IsCancelPostProcessOnStagingPrefab")]
    public partial class CancelPostProcessOnStagingPrefab
    {
    }

    [StartupFlag("RefMata/IsLogOnCancel")]
    public partial class LogOnCancel
    {
    }
}
#endif
