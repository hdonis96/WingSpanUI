using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using Xamarin.Essentials;
using System.Diagnostics;
using System.Collections;

namespace WingSpan2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        double lat;
        double lon;
        private static int NUM_ADDRESSES = 20; 
    ArrayList addressInfo = new ArrayList();
        String yourAddress;
        String yourCityState;
        Label loadingPinLabel = new Label { Text = "loading pins...", TextColor = Color.FromHex("#1D8348"), FontSize = 20 };

    public MapPage()
        {
            InitializeComponent();
            var stack = new StackLayout { Spacing = 0 };
            var label = new Label { Text = "Loading..." };
            stack.Children.Add(label);
            Content = stack;
            getLocationAsync();
        }
        // public async Task showMap()
        public async Task getLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Low);
                var location = await Geolocation.GetLocationAsync(request);


                if (location != null)
                {
                    lat = location.Latitude;
                    lon = location.Longitude;
                    Debug.WriteLine("----------------lat: " + lat + ", long:" + lon + "-----------");

                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    //  showMapAsync(); old
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Debug.WriteLine("-------fnsEx---------");
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                Debug.WriteLine("--------fneEx--------");
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Debug.WriteLine("--------pEx--------");
            }
            catch (Exception ex)
            {
                // Unable to get location
                Debug.WriteLine("------can't get location----------");
                lat = 38.824310;
                lon = -121.282731;
            }
               lat = 38.824310; lon = -121.282731; //rocklin house
            //lat = 38.742248; lon = -121.290019; //roseville house
            //  lat = 38.560923; lon = -121.422378; //<- sac state
            createMapAsync(lat, lon, "Your Location", "home"); 
        }


        public async Task createMapAsync(double lat1, double lon1, String name, String address)
        {
            CustomMap customMap = new CustomMap
            {
                MapType = MapType.Street,
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };
            placeMap(customMap);
            //lat = up & doqn
            //lon = L & R
            double longStart = lon - 0.0016;
            double latStart = lat + 0.0016;
            await populateAddress(lat, lon, customMap, true, 2); //for your location

            for (int i = 0; i < NUM_ADDRESSES; i++) { 
                for (int j = 0; j < NUM_ADDRESSES; j++) {
                    double lat2 = latStart - (i * .0002);
                    // lat2 += .0005;
                    double long2 = longStart + (j * .0002); //.0004, .0008, .0012, .0016
                    await populateAddress(lat2, long2, customMap, false, j);
                }
            }
            //  createPin(customMap);
            loadingPinLabel.Text = "";
        }

        //decide which geolocations are valid and add them to list
        async System.Threading.Tasks.Task populateAddress(double lat1, double lon1, CustomMap customMap, Boolean yourLoc, int index)
        {
            if(index%5==0) loadingPinLabel.Text = "";
            else loadingPinLabel.Text = "loading pins...";
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(lat1, lon1);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    var geocodeAddress =
                        $"AdminArea:       {placemark.AdminArea}\n" +
                        $"CountryCode:     {placemark.CountryCode}\n" +
                        $"CountryName:     {placemark.CountryName}\n" +
                        $"FeatureName:     {placemark.FeatureName}\n" +
                        $"Locality:        {placemark.Locality}\n" +
                        $"PostalCode:      {placemark.PostalCode}\n" +
                        $"SubAdminArea:    {placemark.SubAdminArea}\n" +
                        $"SubLocality:     {placemark.SubLocality}\n" +
                        $"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                        $"Thoroughfare:    {placemark.Thoroughfare}\n";
                    System.Diagnostics.Debug.WriteLine(geocodeAddress);
                    System.String address = placemark.FeatureName + " " + placemark.Thoroughfare;

                    System.String cityState = placemark.Locality + ", " + placemark.AdminArea;
                    if (!yourLoc && cityState.Equals(yourCityState) && address.Equals(yourAddress)) { return; }
                    String houseInfo = address + " " + placemark.Locality + " " + placemark.AdminArea;
                    if (addressInfo.Contains(houseInfo)) return;
                    if (address.Equals("Unnamed Road Unnamed Road")) return;
                    String id = "Xamarin";
                    if (yourLoc)
                    {
                        id = "yourLocation";
                        yourAddress = address;
                        yourCityState = cityState;
                    }
                    addressInfo.Add(houseInfo);
                    createPinAsync(customMap, lat1, lon1, id, address, cityState);
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString() + "Error with getting coordinates");
            }
        }

        public async Task createPinAsync(CustomMap customMap, double lat1, double lon1, String id, String address, String cityState) {

            try
            {
                var locations = await Geocoding.GetLocationsAsync(address + " " + cityState);
                var location = locations?.FirstOrDefault();
                if (location != null)
                {
                    lat1 = location.Latitude;
                    lon1 = location.Longitude;
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                }
                var pin = new CustomPin
                {
                    Type = PinType.Place,
                    Position = new Position(lat1, lon1),
                    Label = address,
                    Address = cityState,
                    Id = id,
                    Url = "http://xamarin.com/about/",
                    hasClicked = false
                };
                customMap.CustomPins = new List<CustomPin> { pin };
                if (id.Equals("Xamarin"))
                {
                    pin.Clicked += async (sender, args) =>
                    {
                        pinClickedAsync(pin, customMap);
                    };
                }
                else
                {
                    pin.Clicked += async (sender, args) =>
                    {
                        popUpInfoAsync(address, cityState);
                    };
                }
                customMap.Pins.Add(pin);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString() + "Error with converting address to coordinates");
            }
        }
            
        public void placeMap(CustomMap customMap)
        {
            var stack = new StackLayout { Spacing = 0 };
            Button buttonZoomIn = new Button
            {
                Text = "+",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38

            };
            Button buttonZoomOut = new Button
            {
                Text = "-",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38,

            };
            Button buttonLoc = new Button
            {
                Text = "Location",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,

            };
            buttonZoomIn.Clicked += async (sender, args) =>
            {
                onZoomInClicked(customMap);
            };
            buttonZoomOut.Clicked += async (sender, args) =>
            {
                onZoomOutClicked(customMap);
            };
            buttonLoc.Clicked += async (sender, args) =>
            {
                customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(0.1))); //1.0
                
                buttonLoc.IsVisible = false;
                stack.Children.Add(buttonZoomIn);
                stack.Children.Add(buttonZoomOut);
                stack.Children.Add(loadingPinLabel);
                customMap.HasZoomEnabled = false;
            };
            stack.Children.Add(customMap);
            stack.Children.Add(buttonLoc);
            Content = stack;
            customMap.HasScrollEnabled = false;
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(10))); //1.0
        }

        private void onZoomInClicked(CustomMap customMap)
        {
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(0.05))); //1.0
        }
        private void onZoomOutClicked(CustomMap customMap)
        {
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(0.1))); //1.0
        }


        // when pin info window is clicked, change color and then call pop up info method: 
        private async Task pinClickedAsync(CustomPin pin, CustomMap customMap)
        {
            string address = pin.Label;
            string cityState = pin.Address;
            Position p = pin.Position;
            System.Diagnostics.Debug.WriteLine(address + " " + cityState);
            customMap.Pins.Remove(pin);
            
            var newPin = new CustomPin
            {
                Type = PinType.Place,
                Position = p,
                Label = address,
                Address = cityState,
                Id = "hasClicked",
                Url = "http://xamarin.com/about/",
                hasClicked = true
            };
            customMap.Pins.Add(newPin);
            await popUpInfoAsync(address, cityState);
        }

        //pop up info on a house----
        public async Task popUpInfoAsync(string address, string cityState) { 
            await DisplayAlert("", "-House Information-" + "\n " +  address + "\n" + cityState, "Ok");
            // pop up this info....
        }
        public ArrayList getHouseInfoList()
        {
            return addressInfo;
        }
    }
}