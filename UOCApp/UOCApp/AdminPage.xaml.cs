﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace UOCApp
{
	public partial class AdminPage : ContentPage
	{
        //temporary, for testing
        //bool loggedIn = true;

		public AdminPage ()
		{
			InitializeComponent ();
		}

        protected override async void OnAppearing() //is this safe?
        {
            base.OnAppearing();

            await Task.Yield();

            bool loggedIn = false;
            if (Application.Current.Properties.ContainsKey("loggedin"))
            {
                loggedIn = Convert.ToBoolean(Application.Current.Properties["loggedin"]);
            }


            //TODO: check if we're actually logged in
            if (!loggedIn)
            {
                //try to login
                await Navigation.PushModalAsync(new LoginPage());
            }            

            //are we logged in now? no?
            if(!loggedIn)
            {
                //leave the page
                Navigation.PopAsync();
            }


        }

        private void NavHome(object sender, EventArgs args)
        {
            Console.WriteLine("Clicked Nav Home");
            Navigation.PopToRootAsync();
        }

        private void NavLeaderboard(object sender, EventArgs args)
        {
            Console.WriteLine("Clicked Nav Leaderboard");
            Navigation.PushAsync(new LeaderboardPage());
        }

        private void NavTimes(object sender, EventArgs args)
        {
            Console.WriteLine("Clicked Nav Times");
            Navigation.PushAsync(new TimesPage());
        }

    }
}
