using System;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Entites;

namespace BitMobile.Application.Entites
{
    public struct EntityField : IEntityField
    {

        internal EntityField(string name, Type type, bool keyField, bool allowNull, int index, string dbRefTable = null)
            : this()
        {
            Name = name;
            Type = type;
            KeyField = keyField;
            AllowNull = allowNull;
            Index = index;
            DbRefTable = dbRefTable;
        }

        public string Name { get; private set; }

        public Type Type { get; private set; }

        public bool KeyField { get; private set; }

        public bool AllowNull { get; private set; }

        public int Index { get; private set; }

        public string DbRefTable { get; private set; }

        public bool IsRef
        {
            get { return Type == typeof(IDbRef); }
        }

        //        public bool Unique { get; private set; }
        //
        //        public int Length { get; private set; }
        //
        //        public int Precision { get; private set; }
        //
        //        public int Scale { get; private set; }
    }

}