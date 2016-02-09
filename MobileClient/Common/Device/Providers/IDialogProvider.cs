using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BitMobile.Common.Device.Providers
{
    public interface IDialogProvider
    {
        Task<int> Alert(string message, IDialogButton positive, IDialogButton negative
            , IDialogButton neutral);

        Task Message(string message, IDialogButton ok);

        Task<bool> Ask(string message, IDialogButton positive, IDialogButton negative);

        Task<IDialogAnswer<DateTime>> DateTime(string caption, DateTime current, IDialogButton positive, IDialogButton negative);

        Task<IDialogAnswer<DateTime>> Date(string caption, DateTime current, IDialogButton positive, IDialogButton negative);

        Task<IDialogAnswer<DateTime>> Time(string caption, DateTime current, IDialogButton positive, IDialogButton negative);

        /// <remarks>We're using KeyValuePair array instead Dictionary because arrays has permanent order of items</remarks>
        Task<IDialogAnswer<object>> Choose(string caption, KeyValuePair<object, string>[] items, int index
            , IDialogButton positive, IDialogButton negative);
    }

    public interface IDialogAnswer<out T>
    {
        bool Positive { get; }
        T Result { get; }
    }
}