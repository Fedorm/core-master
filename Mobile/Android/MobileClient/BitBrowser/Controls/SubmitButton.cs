using BitMobile.Droid;

namespace BitMobile.Controls
{
    // ReSharper disable once UnusedMember.Global
    internal class SubmitButton : Button
    {
        public SubmitButton(BaseScreen activity)
            : base(activity)
        {
            Scope = string.Empty;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string Scope { get; set; }

        protected override bool InvokeClickAction()
        {
            bool allowed = true;
            if (!string.IsNullOrWhiteSpace(Scope))
                allowed = _applicationContext.Validate(Scope);

            if (allowed)
                return base.InvokeClickAction();

            return false;
        }
    }
}
