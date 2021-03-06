﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UOCApp.Models;
using Xamarin.Forms;
using Newtonsoft.Json;
using UOCApp.Helpers;

namespace UOCApp
{
	public partial class AdminPage : ContentPage
	{
        //temporary, for testing
        //bool loggedIn = true;

        //should this be at the app level?
        HttpClient client;
        GetResultsHelper resultsHelper;
        DeleteResultsHelper deleteHelper;

        List<AdminResult> baseResults = new List<AdminResult>();
        ObservableCollection<AdminResult> results = new ObservableCollection<AdminResult>();

        public AdminPage ()
		{
			InitializeComponent ();

            MessagingCenter.Subscribe<LoginPage, Boolean>(this, "LoginComplete", (sender, arg) => OnLoginComplete(arg));

            client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;

            resultsHelper = new GetResultsHelper(client, App.API_URL);
            deleteHelper = new DeleteResultsHelper(client, App.API_URL);

            //test data
            //results.Add(new AdminResult {result_id = 5, student_name = "John Doe", date = "April 28 2016", time="11:11.111"});

            ListViewAdmin.ItemsSource = results;
        }

        private void OnLoginComplete(bool arg)
        {
            //unsubscribe
            MessagingCenter.Unsubscribe<LoginPage, Boolean>(this, "LoginComplete");

            //on return from login page do something
            //Console.WriteLine(arg);

            //if it returned true, the login was successful and we can do nothing

            //if it returned false, the login was unsuccessful and we need to leave immediately
            if(!arg && Navigation.NavigationStack.Count > 0) //for safety
            {
                Navigation.PopAsync();
            }

            if(arg)
            {
                //show the page if we're logged in
                //AdminLayout.IsVisible = true;
                //IsVisible is broken, use Opacity instead
                AdminLayout.Opacity = 1.0;

                //load result list
                GetResults();
            }
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
            else
            {
                //show the page if we're logged in
                //AdminLayout.IsVisible = true;
                AdminLayout.Opacity = 1.0;

                GetResults();
            }

        }

        //get the list of results from the server
        private async void GetResults()
        {
            //build the querystring with the help of the helper
            string selectedGrade = !(PickerGrade == null) ? PickerGrade.Items[PickerGrade.SelectedIndex] : "Grade 4";
            string selectedGender = !(PickerGender == null) ? PickerGender.Items[PickerGender.SelectedIndex] : "Male";
            string selectedSchool = !(EntrySchool == null) ? EntrySchool.Text : null;
            string query = resultsHelper.CreateQueryString(selectedGrade, selectedGender, selectedSchool);

            try
            {
                //see how elegant using the helper makes this?

                List<RawResult> rawresults = await resultsHelper.GetRawResults(query);

                this.baseResults = resultsHelper.ConvertAdminResults(rawresults);

                    //copy results
                    CopyResults();


            }
            catch(Exception e) //pokemon exception handling
            {
                Console.WriteLine("Caught exception " + e.Message);
                await DisplayAlert("Alert", "An unexpected error occurred while getting the list", "OK");
            }

        }

        private void ResortResults(string selectedItem)
        {

            //sort results with helper
            resultsHelper.SortResults(baseResults, selectedItem);

            //copy the results to the observable list
            CopyResults();
        }

        //copy backing list to display list
        private void CopyResults()
        {
            this.results.Clear();

            foreach (AdminResult result in this.baseResults)
            {
                this.results.Add(result);
            }
        } 

        private async void ButtonLogoutClick(object sender, EventArgs args)
        {
            Application.Current.Properties["loggedin"] = false;

            await Application.Current.SavePropertiesAsync();

            await DisplayAlert("Alert", "You have been logged out successfully", "OK");

            Navigation.PopAsync();
        }

        private async void ButtonDeleteClick(object sender, EventArgs args)
        {
            int result_id = (int)((Button)sender).CommandParameter;

            var sure = await DisplayAlert("Confirm", "Delete this record permanently/", "Yes", "No");
            if(!(bool)sure)
            {
                return;
            }

            bool success;
            try
            {
                success = await deleteHelper.DeleteResult(result_id, App.password);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                success = false;
            }

            //if successful, refresh, if not, display a message
            if(success)
            {
                await DisplayAlert("Alert", "Deleted result successfully.", "OK");
                GetResults();
            }
            else
            {                
                await DisplayAlert("Alert", "An unexpected error occured. Please try again later.", "OK");
            }

        }

        private void ButtonRefreshClick(object sender, EventArgs args)
        {
            GetResults();
        }

        //Fired when any filter is changed, refilters the list
        private void FilterChange(object sender, EventArgs args)
        {
            //sanity check
            if (PickerGrade == null)
                return;

            GetResults();
        }

        //Fired when the sort is changed, resorts the list
        private void SortChange(object sender, EventArgs args)
        {
            //abort if the picker isn't actually loaded yet
            if (PickerSort == null)
                return;

            string selectedItem = PickerSort.Items[PickerSort.SelectedIndex];

            ResortResults(selectedItem);
        }

        private void NavHome(object sender, EventArgs args)
        {
            Navigation.PopToRootAsync();
        }

        private void NavLeaderboard(object sender, EventArgs args)
        {
            Navigation.PushAsync(new LeaderboardPage());
        }

        private void NavTimes(object sender, EventArgs args)
        {
            Navigation.PushAsync(new TimesPage());
        }

    }
}
