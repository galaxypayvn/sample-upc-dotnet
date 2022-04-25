using System;
using System.Collections.Generic;

namespace Demo.Model
{
    public class ExtraData
    {
        public FlightInfo FlightInfo { get; set; } = new FlightInfo();
        //public List<Passenger> passengers { get; set; } = new List<Passenger>();
    }
    public class FlightInfo
    {
        public string airline_code { get; set; } = "airline_code";
        public string operating_airline_code { get; set; } = "operating_airline_code";
        public int journey_type { get; set; } = 1;
        public int flight_number { get; set; } = new Random().Next();
        public string class_of_service { get; set; } = "class_of_service";
        public string departure_airport { get; set; } = "departure_airport";
        public string departure_time { get; set; } = "departure_time";
        public string arrival_airport { get; set; } = "arrival_airport";
        public string arrival_time { get; set; } = "arrival_time";
        public string fare_basis_code { get; set; } = "fare_basis_code";
    }

    public class Passenger
    {
        public string pax_id { get; set; } = "pax_id";
        public string pax_type { get; set; } = "pax_type";
        public string gender { get; set; } = "gender";
        public string dob { get; set; } = "dob";
        public string first_name { get; set; } = "first_name";
        public string last_name { get; set; } = "last_name";
        public string name_in_pnr { get; set; } = "name_in_pnr";
        public string customer_id { get; set; } = "customer_id";
        public string title { get; set; } = "title";
        public string member_ticket { get; set; } = "member_ticket";
    }
}
