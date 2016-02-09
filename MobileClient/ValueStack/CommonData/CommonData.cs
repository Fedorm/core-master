using System;
using BitMobile.Common.DbEngine;
using BitMobile.Common.ValueStack;

namespace BitMobile.ValueStack.CommonData
{
    public class CommonData : ICommonData
    {
        private readonly IDbRefFactory _dbRefFactory;

        public CommonData(string os, IDbRefFactory dbRefFactory)
        {
            _dbRefFactory = dbRefFactory;
            OS = os;
            SyncIsOK = true;
        }

        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        public Guid UserId { get; set; }

        public IDbRef UserRef
        {
            get
            {
                return _dbRefFactory.CreateDbRef("Catalog_User", UserId);
            }
        }

        public String LastUpdated
        {
            get
            {
                return DateTime.Now.ToLongTimeString();
            }
        }

        public DateTime Today
        {
            get
            {
                return DateTime.Now.Date;
            }
        }

        public string LoadingStatus { get; set; }

        public string OS { get; private set; }

        public bool SyncIsOK { get; set; }
    }
}

