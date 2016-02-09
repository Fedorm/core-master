using System;

namespace BitMobile.ValueStack
{
    public class CommonData
    {
        public CommonData()
        {
            SyncIsOK = true;
        }

        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        private Guid userId;

        public Guid UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
            }
        }

        public DbEngine.DbRef UserRef
        {
            get
            {
                return DbEngine.DbRef.CreateInstance("Catalog_User", userId);
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

        public String LoadingStatus { get; set; }

        public bool SyncIsOK { get; set; }
    }
}

