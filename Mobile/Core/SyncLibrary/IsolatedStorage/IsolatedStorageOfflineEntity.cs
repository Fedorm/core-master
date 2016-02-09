// Copyright 2010 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License"); 
// You may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
// MERCHANTABLITY OR NON-INFRINGEMENT. 

// See the Apache 2 License for the specific language governing 
// permissions and limitations under the License.

using System;
using System.Globalization;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using BitMobile.Common.Entites;
using BitMobile.Utilities.LogManager;
using BitMobile.DbEngine;
using BitMobile.ValueStack;

namespace Microsoft.Synchronization.ClientServices.IsolatedStorage
{
    /// <summary>
    /// This class is the base entity from which all entities used by the isolated
    /// storage offline context must inherit
    /// </summary>
    [DataContract()]
    [Serializable]
    public abstract class IsolatedStorageOfflineEntity :
        IOfflineEntity,
        ISqliteEntity,
        IEntity
    {
        /// <summary>
        /// Protected constructor because class is private.  Initial state of created
        /// entities will be Detached.
        /// </summary>
        protected IsolatedStorageOfflineEntity()
        {
            _state = OfflineEntityState.Detached;
            _entityMetadata = new OfflineEntityMetadata();
            isNew = true;
        }

        /// <summary>
        /// Whether the entity is a tombstone.
        /// </summary>
        /// <remarks>
        /// The setter can only be called if the state is detached.  Otherwise,
        /// the tombstone state should be managed by the context.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Display(AutoGenerateField = false)]
        [DataMember]
        [NonLog]
        public OfflineEntityMetadata ServiceMetadata
        {
            get
            {
                return _entityMetadata;
            }
            set
            {
                if (EntityState == OfflineEntityState.Detached || EntityState == OfflineEntityState.Saved)
                    SetServiceMetadata(value);
                else
                    throw new InvalidOperationException("EntityMetadata can only be set when the entity state is Detached.");
            }
        }

        protected void OnPropertyChanged()
        {
            if ((EntityState == OfflineEntityState.Unmodified ||
                EntityState == OfflineEntityState.Saved))
                EntityState = OfflineEntityState.Modified;
        }

        /// <summary>
        /// Returns the current state of the entity.
        /// </summary>
        [NonLog]
        [Display(AutoGenerateField = false)]
        public OfflineEntityState EntityState
        {
            get
            {
                return _state;
            }

            internal set
            {
                if (_state != value)
                {
                    if (value == OfflineEntityState.Unmodified)
                        isNew = false;

                    _state = value;
                }
            }
        }

        /// <summary>
        /// Sets the metadata for the entity and does any notification.
        /// The property setter asserts on whether or not the entity is attached, but this
        /// method does not
        /// </summary>
        /// <param name="metadata">Metadata to set</param>
        internal void SetServiceMetadata(OfflineEntityMetadata metadata)
        {
            if (metadata != _entityMetadata)
            {
                _entityMetadata = metadata;
            }
        }

        /// <summary>
        /// Specifies the current state of the entity.  It is updated based on various actions
        /// performed on the entity.  See the OfflineEntityState enum definition for descriptions
        /// of the states.
        /// </summary>
        OfflineEntityState _state;
        
        /// <summary>
        /// Stores the information that must be persisted for OData.  The most important attributes
        /// are tombstone and id.
        /// </summary>
        OfflineEntityMetadata _entityMetadata;
        
        public void SetIsNew()
        {
            this.isNew = true;
            this.EntityState = OfflineEntityState.Modified;
        }

        public Guid EntityId
        {
            get
            {
                if (this._entityMetadata != null && _entityMetadata.Id != null)
                    return new Guid(this._entityMetadata.Id.Substring(this._entityMetadata.Id.IndexOf("guid'") + 5, 36));
                else
                    return Guid.Empty;
            }
        }

        bool ISqliteEntity.IsTombstone
        {
            get
            {
                return _entityMetadata.IsTombstone;
            }
            set
            {
                _entityMetadata.IsTombstone = value;
            }
        }


        protected bool isNew;
        public bool IsNew()
        {
            return isNew;
        }

        public bool IsModified()
        {
            return _state == OfflineEntityState.Modified;
        }

        public void Load(bool isNew)
        {
            this.isNew = isNew;
            this._state = OfflineEntityState.Unmodified;
        }

        public void Save()
        {
            this.Save(true);
        }

        public void Save(bool inTran)
        {
            if (_state == OfflineEntityState.Modified)
            {
                Database.Current.Save(this, inTran);
                _state = OfflineEntityState.Unmodified;
            }
        }

        #region IEntity

        public EntityType EntityType { get; protected set; }

        public abstract object GetValue(string propertyName);

        public abstract bool HasProperty(string propertyName);

        public abstract void SetValue(string propertyName, object value);
        #endregion

    }
}
