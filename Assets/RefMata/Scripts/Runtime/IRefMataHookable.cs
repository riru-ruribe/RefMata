using System.Collections.Generic;

namespace RefMata
{
    public interface IRefMataHookable
    {
        RefMataKinds Kinds { get; }
        HashSet<string> Labels { get; }
        void RunOnValidate();
        void RunLoad();
    }
}
