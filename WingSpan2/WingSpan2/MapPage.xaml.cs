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
        int INF = 10000;
        ArrayList addressInfo = new ArrayList(); //the house list
        ArrayList filteredAddresses = new ArrayList(); //the house list after the user filters results
        String yourAddress;
        String yourCityState;
        Label loadingPinLabel = new Label { Text = "🕐 loading pins...", TextColor = Color.FromHex("#1D8348"), FontSize = 20 };
        String[] loadingClocks = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛",};
        int curIndex = 0;
        Boolean minPriceClicked = true;
        Boolean minFeetClicked = true;
        Color minMaxColor = Color.Green;
        int filteredPinsPlaced = 0;
        double zoomInAmt = 0.05; //in miles
        double zoomOutAmt = 0.12; 

        public MapPage() 
        {
            InitializeComponent();
            var stack = new StackLayout { Spacing = 0 };
            var label = new Label { Text = "🕐 Loading..." };
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
                if (filteredAddresses.Count > 0 && filteredPinsPlaced > filteredAddresses.Count) break;
            }
            filteredPinsPlaced = 0;
            loadingPinLabel.Text = "";
        }

        //decide which geolocations are valid and add them to list
        async System.Threading.Tasks.Task populateAddress(double lat1, double lon1, CustomMap customMap, Boolean yourLoc, int index)
        {
            if (index % 5 == 0)
            {
                curIndex++;
                loadingPinLabel.Text = loadingClocks[curIndex] + " loading pins...";
                if (curIndex == loadingClocks.Length - 1) curIndex = -1;
            }
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
                    System.String cityState = placemark.Locality + " " + placemark.AdminArea;
                    if (!yourLoc && cityState.Equals(yourCityState) && address.Equals(yourAddress)) { return; }
                    if (filteredAddresses.Count > 0 && !filteredAddresses.Contains(address + " " + cityState)) return;
                    filteredPinsPlaced++;
                    String houseInfo = address + " " + placemark.Locality + " " + placemark.AdminArea;
                    if (addressInfo.Contains(houseInfo)) return;
                    if (address.Equals("Unnamed Road Unnamed Road")) return;
                    //  String id = "Xamarin";
                    bool isLocation = false;
                    if (yourLoc)
                    {
                        //   id = "yourLocation";
                        isLocation = true;
                        yourAddress = address;
                        yourCityState = cityState;
                    }
                    addressInfo.Add(houseInfo);
                    createPinAsync(customMap, lat1, lon1, isLocation, address, cityState);
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString() + "Error with getting coordinates");
            }
        }

        public async Task createPinAsync(CustomMap customMap, double lat1, double lon1, bool isLocation, String address, String cityState) {

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
                   // Id = id,
                   isYourLocation = isLocation,
                    Url = "http://xamarin.com/about/",
                    hasClicked = false
                };

               if(!isLocation)
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
                       // popUpInfoAsync(address, cityState);
                        pinClickedAsync(pin, customMap);
                    };
                }
                customMap.Pins.Add(pin);
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString() + "Error with converting address to coordinates");
            }
        }
        // when pin info window is clicked, change color and then call pop up info method: 
        private async Task pinClickedAsync(CustomPin pin, CustomMap customMap)
        {
            string address = pin.Label;
            string cityState = pin.Address;
            Position p = pin.Position;
            bool isLocation = pin.isYourLocation;
            System.Diagnostics.Debug.WriteLine(address + " " + cityState);
            customMap.Pins.Remove(pin);

            var newPin = new CustomPin
            {
                Type = PinType.Place,
                Position = p,
                Label = address,
                Address = cityState,
                // Id = "hasClicked",
                Url = "http://xamarin.com/about/",
                isYourLocation = isLocation,
                hasClicked = true
            };
            customMap.Pins.Add(newPin);
            await popUpInfoAsync(address, cityState);
        }

        //pop up info on a house---------
        public async Task popUpInfoAsync(string address, string cityState)
        {
            await DisplayAlert("", "-House Information-" + "\n " + address + "\n" + cityState, "Ok");
            // pop up this info....
        }
        public ArrayList getHouseInfoList()
        {
            return addressInfo;
        }
        public void placeMap(CustomMap customMap)
        {
            customMap.HasScrollEnabled = false;
            customMap.HasZoomEnabled = false;
            var stack = new StackLayout { Spacing = 0 };
            Button buttonZoomIn = new Button
            {
                Text = "➕ ",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38

            };
            Button buttonZoomOut = new Button
            {
                Text = "➖",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38,

            };
            Button buttonLoc = new Button
            {
                Text = "📌 Location ",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            Button polygonButton = new Button
            {
                Text = "🏠",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38,

            };
            Button searchButton = new Button
            {
                Text = "🔍",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38,

            };
            Button resetFiltersButton = new Button
            {
                Text = "🔄",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38,
            };
            resetFiltersButton.Clicked += async (sender, args) =>
            {
                filteredAddresses.Clear();
                String[] city0 = yourCityState.Split(' ');
                findHouseAsync(yourAddress, city0[0], city0[1]);

            };
            polygonButton.Clicked += async (sender, args) =>
            {
                if (customMap.ShapeCoordinates.Count < 4)
                {
                    await DisplayAlert("", "Please select 4 points for the polygon first", "Ok");
                }
                else
                {
                    getPolygonAddresses(customMap);
                }
            };
            buttonZoomIn.Clicked += async (sender, args) =>
            {
                customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(zoomInAmt))); 
            };
            buttonZoomOut.Clicked += async (sender, args) =>
            {
                customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(zoomOutAmt))); 
            };
            searchButton.Clicked += async (sender, args) =>
            {
                searchClicked(customMap);
            };
            
            var refreshLoc = new Button
            {
                Text = " 🔄 Location",
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            refreshLoc.Clicked += async (sender, args) => { refreshLocation(customMap); };
            stack.Children.Add(refreshLoc);
            stack.Children.Add(customMap);
            stack.Children.Add(buttonLoc);
            Content = stack;
            customMap.HasScrollEnabled = false;
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(10))); //1.0

            buttonLoc.Clicked += async (sender, args) =>
            {
                customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(zoomOutAmt))); //1.0

                buttonLoc.IsVisible = false;
                stack.Children.Add(buttonZoomIn);
                stack.Children.Add(buttonZoomOut);
                // stack.Children.Add(polygonButton); //takes up a lot of room...
                stack.Children.Add(searchButton);
                if (filteredAddresses.Count > 0)
                {
                    stack.Children.Add(resetFiltersButton);
                }
                stack.Children.Add(loadingPinLabel);
                customMap.HasZoomEnabled = false;
                customMap.HasScrollEnabled = false;
            };
        }

        public async Task refreshLocation(CustomMap customMap)
        {
            Debug.WriteLine("--------location refreshed!");
            StackLayout stack = new StackLayout {Spacing = 0 };
            stack.Children.Add(new Label { Text = "changing location..." });
            Content = stack;
            try
            {
              //  var request = new GeolocationRequest(GeolocationAccuracy.Low);
             //   var location = await Geolocation.GetLocationAsync(request);
             //   if (location != null)
             //   {
             //       lat = location.Latitude;
             //       lon = location.Longitude;
             //   }
                customMap.ShapeCoordinates = null;
                customMap.polygonPoints = 0;
                foreach (CustomPin p in customMap.Pins) customMap.Pins.Remove(p);
                addressInfo = new ArrayList();
                yourAddress = ""; yourCityState = "";
                lat = 38.826200; lon = -121.282283; //testing
                customMap = null;
                createMapAsync(lat, lon, "Your Location", "home");
            }
            catch (Exception e)
            {
                await DisplayAlert("", "Could not refresh location", "Ok");
                Debug.WriteLine(e.ToString());
            }
        }

        //calculate if pins are in the polygon and add them to a list
        private void getPolygonAddresses(CustomMap customMap)
        {
            List<String> polygonAddresses = new List<String>();
            Point[] polygonPoints = new Point[4];
            int i = 0;

            //convert shape coordinates to points
            foreach (Position pos in customMap.ShapeCoordinates)
            {
                Point point = new Point();
                point.x = pos.Latitude;
                point.y = pos.Longitude;
                polygonPoints[i] = point;
                i++;
            }

            //if CustomPin inside polygon: add to polygonAddresses
            foreach (CustomPin pin in customMap)
            {
                Point point = new Point();
                point.x = pin.Position.Latitude;
                point.y = pin.Position.Longitude;

                //   Debug.WriteLine(p + " ");
                if (isInside(polygonPoints, 4, point))
                {
                    polygonAddresses.Add(pin.Label + " " + pin.Address);
                }
            }

            //print out addresses in polygon
            foreach (String add in polygonAddresses)
            {
                Debug.WriteLine("addresses in polygon: " + add);
            }
        }
        // search button pressed:
        public void searchClicked(CustomMap customMap)
        {
            Label title = new Label { Text = "Search for a house", FontSize = 30, TextColor = Color.Black };
            Label filterLabel = new Label { Text = "Or filter house results ", FontSize = 30, TextColor = Color.Black };
            var stack = new StackLayout { Spacing = 0 };
            var address = new Entry { Placeholder = "Address" };
            var city = new Entry { Placeholder = "City" };
            var state = new Entry { Placeholder = "State" };
        
            Button enter = new Button
            {
                Text = "enter"
            };
            enter.Clicked += async(sender, args) =>
            {
                try
                {
                    Debug.WriteLine("enter clicked:" + address.Text + city.Text + state.Text);
                    if (address.Text.Equals("") || city.Text.Equals("") || state.Text.Equals(""))
                    {
                        await DisplayAlert("", "House search failed", "Ok");
                    }
                    else
                    {
                        findHouseAsync(address.Text, city.Text, state.Text);
                    }
                } catch (Exception e)
                {
                    await DisplayAlert("", "House search failed", "Ok");
                }
            };
            Button back = new Button
            {
            Text = "◀️ back to map page"
            };
            Button filter = new Button
            {
                Text = "filter"
            };
            back.Clicked += async(sender, args) => { placeMap(customMap); };
            stack.Children.Add(title);
            stack.Children.Add(address);
            stack.Children.Add(city);
            stack.Children.Add(state);
            stack.Children.Add(enter);
           // stack.Children.Add(new Label { Text = " " });
            stack.Children.Add(filterLabel);
            filterLayout(customMap, stack);
            stack.Children.Add(new Label { Text = " " });
            stack.Children.Add(back);
            Content = stack;
        }

        // find a specific house on the map and place the pins (restart everything)
        public async Task findHouseAsync(String address, String city, String state)
        {
            try { 
                var locations = await Geocoding.GetLocationsAsync(address + " " + city + " " + state);
                var location = locations?.FirstOrDefault();
                if (location != null)
                {
                    lat = location.Latitude;
                    lon = location.Longitude;
                    customMap.ShapeCoordinates = null;
                    customMap.polygonPoints = 0;
                    foreach (CustomPin p in customMap.Pins) customMap.Pins.Remove(p); 
                    addressInfo = new ArrayList();
                    yourAddress = "";
                    yourCityState = "";
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                    createMapAsync(lat, lon, "Your Location", "home");
                }
                else
                {
                    Debug.WriteLine("location is null!!" + "add: " + address + "city: " + city);
                } 
            }
            catch (Exception e){
                placeMap(customMap);
                await DisplayAlert("", "House search failed", "Ok");
            }
        }

        //if one if the filter labels are clicked, change the boolean value so the slider can update appropriatley
        public void OnLabelClicked(String labelStr, Label label, Label oldLabel)
        {
            if(labelStr.Equals("minPrice"))
            {
                minPriceClicked = true;
            }
            else if(labelStr.Equals("maxPrice"))
            {
                minPriceClicked = false;
            }
            else if(labelStr.Equals("minFeet"))
            {
                minFeetClicked = true;
            }
            else if(labelStr.Equals("maxFeet"))
            {
                minFeetClicked = false;
            }
            label.TextColor = minMaxColor;
            oldLabel.TextColor = Color.Default;
        }
        //add a tap gesture to a label
        public void addGestureRecognizer(Label label, Label oldLabel, String labelText)
        {
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => OnLabelClicked(labelText, label, oldLabel);
            label.GestureRecognizers.Add(tgr);
        }

        // sliders, labels, buttons for the filter menu
        public void filterLayout(CustomMap customMap, StackLayout stack) {
            int bedMin = 0;
            double bathMin = 0;
            int minPriceInt = 0;
            int maxPriceInt = 0;
            int minFeetInt = 0;
            int maxFeetInt = 0;

            var city = new Entry { Placeholder = "City" };
            var county = new Entry { Placeholder = "County" };
            Label priceLabel = new Label { Text = "Price" };
            Label minPrice = new Label { Text = "Min: $", TextColor = minMaxColor };
            Label maxPrice = new Label { Text = "Max: $" };
            addGestureRecognizer(minPrice, maxPrice, "minPrice");
            addGestureRecognizer(maxPrice, minPrice, "maxPrice");
            Slider priceSlider = new Slider
            {
                Maximum = 1000000,
                Minimum = 100000,
            };
            priceSlider.ValueChanged += (sender, args) =>
            {
                if (minPriceClicked)
                {
                    minPriceInt = Convert.ToInt32(args.NewValue);
                    minPrice.Text = "Min: $" + formatInt(minPriceInt);
                }
                else
                {
                    maxPriceInt = Convert.ToInt32(args.NewValue);
                    maxPrice.Text = "Max: $" + formatInt(maxPriceInt);
                }
            };
            Label feetLabel = new Label { Text = "Square Feet" };
            Label minFeet = new Label { Text = "Min: ", TextColor = minMaxColor };
            Label maxFeet = new Label { Text = "Max: " };
            addGestureRecognizer(minFeet, maxFeet, "minFeet");
            addGestureRecognizer(maxFeet, minFeet, "maxFeet");
            Slider sqFt = new Slider
            {
                Maximum = 10000,
                Minimum = 500
            };
            sqFt.ValueChanged += (sender, args) =>
            {
                if (minFeetClicked)
                {
                    minFeetInt = Convert.ToInt32(args.NewValue);
                    minFeet.Text = "Min: " + formatInt(minFeetInt);
                }
                else
                {
                    maxFeetInt = Convert.ToInt32(args.NewValue);
                    maxFeet.Text = "Max: " + formatInt(maxFeetInt);
                }
            };
            List<String> bedList = new List<String> { "0+ Beds", "1+ Beds", "2+ Beds", "3+ Beds", "4+ Beds", "5+ Beds", "6+ Beds" };
            List<String> bathList = new List<String> { "0+ Baths", "1+ Baths", "1.5+ Baths", "2+ Baths", "3+ Baths", "4+ Baths", "5+ Baths", "6+ Baths" };
            Picker bedPicker = new Picker { Title = "Beds" };
            Picker bathPicker = new Picker { Title = "Baths" };
            foreach (String b in bedList) { bedPicker.Items.Add(b); }
            foreach (String b in bathList) { bathPicker.Items.Add(b); }
            bedPicker.SelectedIndexChanged += (sender, args) =>
            {
                if (bedPicker.SelectedIndex != -1)
                {
                    string bedNum = bedPicker.Items[bedPicker.SelectedIndex];
                    bedMin = Convert.ToInt32((bedNum.ToCharArray()[0]).ToString());
                }
            };
            bathPicker.SelectedIndexChanged += (sender, args) =>
            {
                if (bathPicker.SelectedIndex != -1)
                {
                    string bathNum = bathPicker.Items[bathPicker.SelectedIndex];
                    bathMin = Convert.ToInt32((bathNum.ToCharArray()[0]).ToString());
                    if (bathMin == 1) {
                        if (bathNum.ToCharArray()[1] == '.') { bathMin = 1.5; }
                    }
                    Debug.WriteLine("bathMin: " + bathMin);
                }
            };
            Button enter = new Button { Text = "enter" };
            enter.Clicked += async (sender, args) =>
            {
                if ((minPriceInt > maxPriceInt) || minFeetInt > maxFeetInt)
                {
                    await DisplayAlert("", "Error: Min must be less than Max", "Ok");
                }
                else
                {
                    requestFilteredResults(customMap, city.Text, county.Text, minPriceInt, maxPriceInt, minFeetInt, maxFeetInt, bedMin, bathMin);
                }

            };
            stack.Children.Add(city);
            stack.Children.Add(county);
            stack.Children.Add(priceLabel);
            stack.Children.Add(minPrice);
            stack.Children.Add(maxPrice);
            stack.Children.Add(priceSlider);
            stack.Children.Add(feetLabel);
            stack.Children.Add(minFeet);
            stack.Children.Add(maxFeet);
            stack.Children.Add(sqFt);
            stack.Children.Add(bedPicker);
            stack.Children.Add(bathPicker);
            stack.Children.Add(enter);
        }

        //formats long integers so that they have commas in them
        public String formatInt(int num)
        {
            char[] arr = (num + "").ToCharArray();
            String returnNum = "";
            int index = 0;
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (index!=0 && index % 3 == 0)
                {
                    returnNum = returnNum.Insert(0, ",");
                }
                returnNum = returnNum.Insert(0, arr[i] + "");
                index++;
            }
            return returnNum;
        }

        //get filtered results from back end
        public void requestFilteredResults(CustomMap customMap, String city, String county, int minPrice, int maxPrice, int minFeet, int maxFeet, int bedMin, double bathMin)
        {
            // -call back end scraper with mins, maxes, and the list of addresses to filter,
            // -and save the returned filtered list
            // filteredAddresses = filter(city, county, maxPrice, minPrice, minFeet, maxFeet, bedMin, bathMin, addressInfo); //<- filter() is a method in scraper

            filteredAddresses.Add("1000 Sedona Court Rocklin California"); //testing...
            filteredAddresses.Add("803 Dusty Trail Court Rocklin California"); //just for testing
            filteredAddresses.Add("911 Wild Horse Court Rocklin California"); //just for testing
            filteredAddresses.Add("1008 Sedona Court Rocklin California"); //just for testing
            filteredAddresses.Add("1857 Sorrell Court Rocklin California"); //just for testing
            // -at the end, after filtering...
            String[] city0 = yourCityState.Split();
            findHouseAsync(yourAddress, city0[0], city0[1]); //place map again at location
        }
        
        //-----------these last functions are to get which pins are inside polygon-------------------

        // Given three colinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        bool onSegment(Point p, Point q, Point r)
        {
            if (q.x <= max(p.x, r.x) && q.x >= min(p.x, r.x) &&
                    q.y <= max(p.y, r.y) && q.y >= min(p.y, r.y))
                return true;
            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        int orientation(Point p, Point q, Point r)
        {
            double val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (val == 0) return 0;  // colinear 
            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }
        bool doIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }
        // Returns true if the point p lies inside the polygon[] with n vertices 
        bool isInside(Point[] polygon, int n, Point p)
        {
            // There must be at least 3 vertices in polygon[] 
            if (n < 3) return false;

            // Create a point for line segment from p to infinite 
            Point extreme = new Point();
            extreme.x = INF;
            extreme.y = p.y;

            // Count intersections of the above line with sides of polygon 
            int count = 0, i = 0;
            do
            {
                int next = (i + 1) % n;

                // Check if the line segment from 'p' to 'extreme' intersects with the line segment from 'polygon[i]' to 'polygon[next]' 
                if (doIntersect(polygon[i], polygon[next], p, extreme))
                {
                    // If the point 'p' is colinear with line segment 'i-next', 
                    // then check if it lies on segment. If it lies, return true, otherwise false 
                    if (orientation(polygon[i], p, polygon[next]) == 0)
                        return onSegment(polygon[i], p, polygon[next]);

                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise 
            return (count % 2 == 1);   
        }

        struct Point
        {
            public double x;
            public double y;
        };
        public double max(double a, double b)
        {
            return (a < b) ? b : a;
        }
        public double min(double a, double b)
        {
            return !(b < a) ? a : b;
        }
    }
}