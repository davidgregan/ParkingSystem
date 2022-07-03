# Parking Garage System

Parking Garage System is an API back end and client library for managing parking garages.

## Installation

For demonstration purposes only. Production installation information would go here.


## Usage

### Client Library
```csharp
//Set client handler options appropriate for the application.
var httpClientHandler = new HttpClientHandler()
{
    UseDefaultCredentials = true,
    MaxRequestContentBufferSize = 2147483647
};

// Handle certificate errors, return true for self signed or add necessary validation.
httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

// Create ParkingSystem Client and set options appropriate for the application.
HttpClient httpClient = new HttpClient (httpClientHandler)
{
    Timeout = new TimeSpan(0, 0, 30)
};

//Instantiate client with service path and options.
_apiClient = new ParkingSystemClient("https://localhost:44373/", httpClient);

//Call API functions as needed.
var result = _apiClient.GetAsync();
```

### Garage Design
Each garage should include the desired instantiated parking spaces in the Garage.ParkingSpaces property. Parking spaces must be related by setting the  "NextSpace" property of the previous space to its adjacent space in order to be eligible for selection for Bus parking. Each section defined in this manner must abide by the following constraints:

1) Sections must only contain parking spaces in the same floor and in the same row. 
2) Each related space must have a contiguous parking number assigned with an adjacent spot in the NextSpace property.
3) Multiple unrelated sections are allowed in each row. This is to allow for rows that may be split by driveways or building equipment so that they won't be identified as adjacent and elligible for bus parking.


