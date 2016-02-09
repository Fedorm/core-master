using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BitMobile.Controls
{
    public class GoogleMapBehavior
    {
        #region PAGE

        public readonly string Page = @"
<!DOCTYPE html>
<html>
<head>
    <meta name=""viewport"" content=""initial-scale=1.0, user-scalable=no"" />
    <style type=""text/css"">
        html {
            height: 100%;
        }

        body {
            height: 100%;
            margin: 0;
            padding: 0;
        }

        #map_canvas {
            height: 100%;
        }
    </style>
    <script type=""text/javascript""
        src=""http://maps.googleapis.com/maps/api/js?sensor=false"">
    </script>
    <script type=""text/javascript"">
        var map;
        var bounds;
        var infoWindow = new google.maps.InfoWindow;

        function initialize(markerSetString) {
            bounds = new google.maps.LatLngBounds();
            var mapOptions = {
                center: new google.maps.LatLng(59.879911, 30.439712),
                zoom: 10,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            map = new google.maps.Map(document.getElementById(""map_canvas""), mapOptions);
    		google.maps.event.addListener(map, 'click', function() {
	    	   infoWindow.close();
	        });            


            var markers = markerSetString.split(""##&"");
            for (var i = 0; i < markers.length; i++) {
                showMarker(markers[i]);
            }
        }

        function showMarker(markerString) {
            var params = markerString.split(""##@"");

            var lat = parseFloat(params[1]);
            var lon = parseFloat(params[2]);

            var marker = new google.maps.Marker({
                position: new google.maps.LatLng(lat, lon),
                map: map
            });

            marker.setIcon(""http://maps.google.com/mapfiles/ms/icons/"" + params[3] + ""-dot.png"");

		    var content = '<div style=""text-align:left; font-family:Verdana; font-size:12px; font-weight:bold; font-style:normal;"">' +
						    '<p>' + params[0] + '</p>' +
					      '</div>';

    		google.maps.event.addListener(marker, 'click', function() {
			    infoWindow.setContent(content);
		        infoWindow.open(map, marker);
	    	});

            bounds.extend(marker.position);
            map.fitBounds(bounds);
        }
    </script>
</head>
<body>
    <div id=""spike"" style=""width: 100%; height: 0%"">_</div>
    <div id=""map_canvas"" style=""width: 100%; height: 100%""></div>
</body>
</html>";
        #endregion

        List<string> _points = new List<string>();

        public void AddMarker(string caption, double latitude, double longitude, string color)
        {
            string point = GetMarkerString(caption, latitude, longitude, color);

            _points.Add(point);
        }

        public string BuildShowMarkerFunction(string caption, double latitude, double longitude, string color)
        {
            string point = GetMarkerString(caption, latitude, longitude, color);

            return string.Format("showMarker(\"{0}\")", point);
        }

        public string BuildInitializeFunction()
        {
            string result = "initialize(\"";

            for (int i = 0; i < _points.Count; i++)
            {
                result += _points[i];
                if (i == _points.Count - 1)
                    result += "\")";
                else
                    result += "##&";
            }

            if (_points.Count == 0)
                result += "\")";

            _points.Clear();

            return result;
        }

        static string GetMarkerString(string caption, double latitude, double longitude, string color)
        {
            string point = string.Format("{0}##@{1}##@{2}##@{3}"
                , caption
                , latitude.ToString(CultureInfo.InvariantCulture)
                , longitude.ToString(CultureInfo.InvariantCulture)
                , color);
            return point.Replace(@"""", @"\""");
        }
    }
}
