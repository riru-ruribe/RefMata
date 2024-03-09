using UnityEngine;

namespace RefMata
{
    public interface IRefMataReferenceable
    {
        void RunOnValidate(Component root);
        void RunLoad();
    }
}
