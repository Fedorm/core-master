using BitMobile.Application.Entites;
using BitMobile.Common.Entites;

namespace Microsoft.Synchronization.ClientServices.IsolatedStorage
{
    public class IsolatedStorageSchema
    {
        readonly EntityType[] _entities;

        public IsolatedStorageSchema(EntityType[] entities)
        {
            _entities = entities;
        }

        public EntityType[] Collections
        {
            get
            {
                return _entities;
            }
        }
    }
}
