using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.Generic;

namespace WingSpan2
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        //When we click the login button, this method fires.
        private async void OnLoginButton_Clicked(object sender, EventArgs e)
        {
            //Grab the username and password from the XAML page through x:Names
            var username = usernameEntry.Text; 
            var password = passwordEntry.Text;

            //Validation checks. Make sure the user has entered a username and password.
            if (username == null || password == null)
            {
                await DisplayAlert("", "Please enter a username and password.", "Ok");
                return;
            }
            //If the user is not connected to the internet, through an error.
            else if (username.Equals("holly") && password.Equals("donis")) //(!CrossConnectivity.Current.IsConnected)
            {
                //Xamarin.Forms.Application.Current.MainPage = new MapView();
                //  MapPage mp = new MapPage();

                await Navigation.PushAsync(new MapPage());
            }

            else
            {
                //await DisplayAlert("", "No internet connection found!", "Ok");
                await DisplayAlert("", "Login Failed. Invalid username or password.", "Ok");
                return;
            }
            /*
            //Secure our password by using SHA512Hash. This method is custom made and I'll post it below.
            var securePassword = SHA512Hash(password);

            //When the button is pressed, we load a new job and show a custom loading dialog.
            var loading = UserDialogs.Instance.Loading("Logging In");
            await Task.Run(() => loading.Show());

            //loginMethod code is also posted below.
            string loginResult = await LoginMethod(username, securePassword);

            //If our login was successful, change our applications MainPage to something else.
            if (loginResult.Equals("true"))
            {
                Xamarin.Forms.Application.Current.MainPage = new SelectionPage();
                App.HideCustomLoadingDialog(loading);
            }
            else
            {
                App.HideCustomLoadingDialog(loading);
                await DisplayAlert("", "Login Failed. Invalid username or password.", "Ok");
                return;
            } */
        }
    }
}
