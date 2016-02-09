using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Widget;
using BitMobile.Application.Translator;
using BitMobile.Droid.Application;

namespace BitMobile.Droid.Screens
{
    delegate void InfobaseSelected(Settings settings);

    class InfobasesScreen : InitialScreen
    {
        readonly ISharedPreferences _prefs;
        readonly InfobaseSelected _resultCallback;
        readonly InfobaseManager _manager;


        public InfobasesScreen(BaseScreen activity, ISharedPreferences prefs, InfobaseSelected resultCallback)
            : base(activity, null)
        {
            _prefs = prefs;
            _resultCallback = resultCallback;

            _manager = InfobaseManager.Current;
        }

        public void Start()
        {
            Settings settings = null;

            var infobase = _manager.Infobases.FirstOrDefault(val => val.IsAutorun);
            if (infobase == null)
            {
                Activity.FlipScreen(Resource.Layout.Infobases);

                using (var tv = Activity.FindViewById<TextView>(Resource.Id.infobasesCaption))
                    tv.Text = D.INFOBASES;
                
                LoadList();
            }
            else
                settings = new Settings(_prefs, Activity.Resources.Configuration.Locale.Language, infobase);
            

            if (settings != null)
                _resultCallback(settings);
        }

        void LoadList()
        {
            using (var basesList = Activity.FindViewById<ListView>(Resource.Id.infobasesList))
            {
                var captions = new string[_manager.Infobases.Length + 1];
                for (int i = 0; i < _manager.Infobases.Length; i++)
                {
                    InfobaseManager.Infobase infobase = _manager.Infobases[i];
                    captions[i] = infobase.Name;
                    if (infobase.IsActive)
                        captions[i] += "*";
                }
                captions[captions.Length - 1] = "...";

                basesList.Adapter =
                    new ArrayAdapter<string>(Activity, Resource.Layout.TextViewForListView, captions);

                basesList.ItemClick += basesList_ItemClick;
                basesList.ItemLongClick += basesList_ItemLongClick;
            }
        }

        void basesList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (e.Position < _manager.Infobases.Length)
            {
                InfobaseManager.Infobase currentInfobase = _manager.Infobases[e.Position];
                foreach (var infobase in _manager.Infobases)
                    if (infobase != currentInfobase)
                        infobase.IsActive = false;

                var settings = new Settings(_prefs, Activity.Resources.Configuration.Locale.Language, currentInfobase);
                _resultCallback(settings);
            }
            else
                using (var builder = new AlertDialog.Builder(Activity))
                {
                    builder.SetTitle(D.CREATE_NEW_INFOBASE);

                    var ll = new LinearLayout(Activity) { Orientation = Orientation.Vertical };
                    ll.SetPadding(10, 5, 10, 5);
                    builder.SetView(ll);

                    ll.AddView(new TextView(Activity) { Text = D.INFOBASE_NAME });
                    var editName = new EditText(Activity);
                    editName.SetSingleLine();
                    ll.AddView(editName);

                    ll.AddView(new TextView(Activity) { Text = D.URL });
                    var editUrl = new EditText(Activity) { Text = "http://" };
                    editUrl.SetSingleLine();
                    ll.AddView(editUrl);

                    ll.AddView(new TextView(Activity) { Text = D.APPLICATION });
                    var editApplication = new EditText(Activity) { Text = "app" };
                    editApplication.SetSingleLine();
                    ll.AddView(editApplication);

                    ll.AddView(new TextView(Activity) { Text = D.FTP_PORT });
                    var editFtpPort = new EditText(Activity) { Text = "21" };
                    editFtpPort.SetSingleLine();
                    ll.AddView(editFtpPort);

                    builder.SetPositiveButton(D.OK, (s, args) =>
                    {
                        _manager.CreateInfobase(editName.Text, editUrl.Text
                            , editApplication.Text, editFtpPort.Text);
                        LoadList();
                    });
                    builder.SetNegativeButton(D.CANCEL, (s, args) => { });
                    builder.Show();
                }
        }

        void basesList_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if (e.Position < _manager.Infobases.Length)
                using (var builder = new AlertDialog.Builder(Activity))
                {
                    var infobase = _manager.Infobases[e.Position];

                    builder.SetTitle(infobase.Name);

                    var ll = new LinearLayout(Activity) { Orientation = Orientation.Vertical };
                    ll.SetPadding(10, 5, 10, 5);
                    builder.SetView(ll);

                    ll.AddView(new TextView(Activity) { Text = D.URL });
                    var editUrl = new EditText(Activity) { Text = infobase.BaseUrl };
                    editUrl.SetSingleLine();
                    ll.AddView(editUrl);

                    ll.AddView(new TextView(Activity) { Text = D.APPLICATION });
                    var editApplication = new EditText(Activity) { Text = infobase.ApplicationString };
                    editApplication.SetSingleLine();
                    ll.AddView(editApplication);

                    ll.AddView(new TextView(Activity) { Text = D.FTP_PORT });
                    var editFtpPort = new EditText(Activity) { Text = infobase.FtpPort };
                    editFtpPort.SetSingleLine();
                    ll.AddView(editFtpPort);

                    builder.SetPositiveButton(D.OK, (s, args) =>
                    {
                        infobase.BaseUrl = editUrl.Text;
                        infobase.ApplicationString = editApplication.Text;
                        infobase.FtpPort = editFtpPort.Text;
                        _manager.SaveInfobases();
                        LoadList();
                    });
                    builder.SetNegativeButton(D.CANCEL, (s, args) => { });
                    builder.SetNeutralButton(D.DELETE, (s, args) => DeleteInfobase(e.Position));

                    builder.Show();
                }
        }

        void DeleteInfobase(int position)
        {
            using (var builder = new AlertDialog.Builder(Activity))
            {
                builder.SetTitle(D.DELETE_INFOBASE);
                builder.SetPositiveButton(D.YES, (s, args) =>
                {
                    _manager.RemoveInfobaseAt(position);
                    LoadList();
                });
                builder.SetNegativeButton(D.NO, (s, args) => { });
                builder.Show();
            }
        }

        //TODO: implement customer code
        void OpenCustomerCodeMenu(object sender, EventArgs e)
        {
            using (var builder = new AlertDialog.Builder(Activity))
            {
                builder.SetTitle(D.ENTER_CUSTOMER_CODE);

                using (var ll = new LinearLayout(Activity) { Orientation = Orientation.Vertical })
                {
                    ll.SetPadding(10, 5, 10, 5);
                    builder.SetView(ll);

                    using (var editName = new EditText(Activity))
                    {
                        editName.Id = 0;
                        editName.Hint = D.CUSTOMER_CODE;
                        editName.SetSingleLine();
                        ll.AddView(editName);
                    }

                    using (var editPassword = new EditText(Activity))
                    {
                        editPassword.Id = 1;
                        editPassword.Hint = D.PASSWORD;
                        editPassword.SetSingleLine();
                        ll.AddView(editPassword);
                    }
                }

                builder.SetPositiveButton(D.OK, DownloadInfobases);
                builder.SetNegativeButton(D.CANCEL, (s, args) => { });
                builder.Show();
            }
        }

        private void DownloadInfobases(object sender, DialogClickEventArgs e)
        {
            string customerCode;
            using (var editText = ((AlertDialog)sender).FindViewById<EditText>(0))
                customerCode = editText.Text;

            string password;
            using (var editPassword = ((AlertDialog)sender).FindViewById<EditText>(1))
                password = editPassword.Text;

            string url = string.Format("http://192.168.0.2/bitmobile/test/script/locator/products?code={0}&pwd={1}"
                , customerCode, password);

            if (_manager.DownloadInfobases(url))
                LoadList();
            else
                Toast.MakeText(Activity, D.ERROR, ToastLength.Long).Show();
        }
    }
}