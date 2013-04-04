﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TransportationService.Utility;
using MongoDB.Bson;
using TransportationService.Models;

namespace TransportationService.Controllers
{

   public class AdminController : TransportationBaseController
   {

      #region state declarations
      List<String> stateNames = new List<string>()
        {
            "Alaska",
                 "Alabama",
                 "Arkansas",
                 "Arizona",
                 "California",
                 "Colorado",
                 "Connecticut",
                 "District of Columbia",
                 "Delaware",
                 "Florida",
                 "Georgia",
                 "Hawaii",
                 "Iowa",
                 "Idaho",
                 "Illinois",
                 "Indiana",
                 "Kansas",
                 "Kentucky",
                 "Louisiana",
                 "Massachusetts",
                 "Maryland",
                 "Maine",
                 "Michigan",
                 "Minnesota",
                 "Missouri",
                 "Mississippi",
                 "Montana",
                 "North Carolina",
                 "North Dakota",
                 "Nebraska",
                 "New Hampshire",
                 "New Jersey",
                 "New Mexico",
                 "Nevada",
                 "New York",
                 "Ohio",
                 "Oklahoma",
                 "Oregon",
                 "Pennsylvania",
                 "Rhode Island",
                 "South Carolina",
                 "South Dakota",
                 "Tennessee",
                 "Texas",
                 "Utah",
                 "Virginia",
                 "Vermont",
                 "Washington",
                 "Wisconsin",
                 "West Virginia",
                 "Wyoming"
        };
      List<String> stateAbbreviations = new List<String>() {
            "AK",
                 "AL",
                 "AR",
                 "AZ",
                 "CA",
                 "CO",
                 "CT",
                 "DC",
                 "DE",
                 "FL",
                 "GA",
                 "HI",
                 "IA",
                 "ID",
                 "IL",
                 "IN",
                 "KS",
                 "KY",
                 "LA",
                 "MA",
                 "MD",
                 "ME",
                 "MI",
                 "MN",
                 "MO",
                 "MS",
                 "MT",
                 "NC",
                 "ND",
                 "NE",
                 "NH",
                 "NJ",
                 "NM",
                 "NV",
                 "NY",
                 "OH",
                 "OK",
                 "OR",
                 "PA",
                 "RI",
                 "SC",
                 "SD",
                 "TN",
                 "TX",
                 "UT",
                 "VA",
                 "VT",
                 "WA",
                 "WI",
                 "WV",
                 "WY"
        };
      #endregion

      [HttpGet]
      public ActionResult LoadView()
      {
         DatabaseInterface db = new DatabaseInterface();
         var user = sessionManager.User;
         if (user == null)
         {
            return PartialView("Login");
         }
         var model = new OutputViewModel()
         {
            Username = user.Username,
            Routes = db.GetAvailableRoutes(),
            Buses = db.GetAvailableBuses(),
            Employees = db.GetAvailableEmployees(),
            Stops = db.GetAvailableStops(),
            Drivers = db.GetAvailableDrivers()
         };
         return PartialView("AdminView", model);
      }

      public ActionResult RefreshAdmin()
      {
         User user = sessionManager.User;
         if (user != null)
         {
            return Json(new
            {
               user = JsonUtility.ToUserJson(user),
               headerText = "Welcome, " + user.Username
            });
         }
         return Json(new { error = true });
      }

      #region Route

      public ActionResult AddRoute()
      {
         DatabaseInterface db = new DatabaseInterface();
         RouteModel model = new RouteModel()
         {
            AvailableBuses = db.GetAvailableBuses(),
            AvailableStops = db.GetAvailableStops(),
            AvailableDrivers = db.GetAvailableDrivers(),
            Bus = null,
            Driver = null,
            Name = "",
            RouteId = "",
            Stops = { },
            UpdatingRoute = false
         };
         return PartialView("AddRoute", model);
      }

      public ActionResult ModifyRoute(String routeId)
      {
         DatabaseInterface db = new DatabaseInterface();
         Route route = db.GetRouteByRouteId(int.Parse(routeId));
         RouteModel model = new RouteModel()
         {
            AvailableBuses = db.GetAvailableBuses(),
            AvailableStops = db.GetAvailableStops(),
            AvailableDrivers = db.GetAvailableDrivers(),
            Bus = route.Bus,
            Driver = route.Driver,
            Name = route.Name,
            RouteId = routeId,
            Stops = route.Stops,
            UpdatingRoute = true
         };
         return PartialView("AddRoute", model);
      }

      public ActionResult AddNewRoute(List<int> stopIds, string routeName, int busId, bool startsAtWork, string driverLicense)
      {
         DatabaseInterface db = new DatabaseInterface();
         if (!db.IsRouteNameUnique(routeName))
            return Json("false");
         int routeId;
         if (startsAtWork)
            routeId = db.GetNextLowRouteId();
         else
            routeId = db.GetNextHighRouteId();
         List<Stop> stops = new List<Stop>();
         foreach (int id in stopIds)
         {
            stops.Add(db.GetStopByStopId(id));
         }
         List<Route> routes = new List<Route>();
         routes = db.GetAvailableRoutes();
         db.AssignBusToRoute(busId, routeId);
         db.AssignDriverToRoute(driverLicense, routeId);
         Route route = new Route()
         {
            Stops = stops,
            Driver = db.GetDriverByDriverLicense(driverLicense),
            Name = routeName,
            RouteId = routeId,
            Id = ObjectId.GenerateNewId(),
            Bus = db.GetBusByBusId(busId)
         };
         db.AddRoute(route);
         return Json(new
         {
            success = "true",
            id = route.Id.ToString()
         });

      }

      public ActionResult UpdateRoute(int routeId, List<int> stopIds, string routeName, int busId, string driverLicense)
      {
         DatabaseInterface db = new DatabaseInterface();
         String sRouteId = routeId.ToString();
         if (!db.IsRouteNameUnique(routeName, sRouteId))
            return Json("false");
         List<Stop> stops = new List<Stop>();
         foreach (int id in stopIds)
         {
            stops.Add(db.GetStopByStopId(id));
         }
         db.AssignBusToRoute(busId, routeId);
         db.AssignDriverToRoute(driverLicense, routeId);
         Route route = new Route()
         {
            Stops = stops,
            Driver = db.GetDriverByDriverLicense(driverLicense),
            Name = routeName,
            RouteId = routeId,
            Bus = db.GetBusByBusId(busId)
         };
         db.UpdateRoute(route);
         return Json(new { success = "true", licensePlate = route.Bus.LicensePlate, driverName = route.Driver.Name });
      }

      #endregion


      #region Bus

      public ActionResult AddBus()
      {
         BusModel model = new BusModel
         {
            StateNames = stateNames,
            StateAbbreviations = stateAbbreviations,
            Capacity = "",
            License = "",
            UpdatingBus = false,
            BusId = ""
         };
         return PartialView("AddBus", model);
      }

      public ActionResult AddNewBus(int capacity, string license, string state)
      {
         DatabaseInterface db = new DatabaseInterface();
         if (!db.IsLicenseUnique(license))
            return Json("false");

         Bus bus = new Bus()
         {
            Id = ObjectId.GenerateNewId(),
            LicensePlate = license,
            BusId = db.GetNextBusId(),
            Status = BusStatus.Active,
            Capacity = capacity,
            State = state,
            AssignedTo = -1

         };
         db.AddBus(bus);
         return Json(new
         {
            success = "true",
            id = bus.Id.ToString()
         });

      }

      public ActionResult ModifyBus(int busId)
      {
         DatabaseInterface db = new DatabaseInterface();
         Bus bus = db.GetBusByBusId(busId);
         BusModel model = new BusModel
         {
            StateNames = stateNames,
            StateAbbreviations = stateAbbreviations,
            Capacity = bus.Capacity.ToString(),
            License = bus.LicensePlate,
            UpdatingBus = true,
            Status = bus.Status.ToString(),
            State = bus.State,
            BusId = busId.ToString()
         };
         return PartialView("AddBus", model);
      }

      public ActionResult UpdateBus(string busId, int capacity, string license, string state, string status)
      {
         DatabaseInterface db = new DatabaseInterface();
         if (!db.IsLicenseUnique(license, busId))
            return Json("false");
         Bus bus = db.GetBusByBusId(int.Parse(busId));
         bus.LicensePlate = license;
         bus.BusId = int.Parse(busId);
         BusStatus busStatus;
         if (status.Equals("0"))
         {
            busStatus = BusStatus.Active;
         }
         else
         {
            busStatus = BusStatus.Inactive;
         }
         bus.Status = busStatus;
         bus.Capacity = capacity;
         bus.State = state;
         db.UpdateBus(bus);
         return Json("true");
      }

      public ActionResult DeleteBus(string id)
      {
          DatabaseInterface db = new DatabaseInterface();
          ObjectId objId = new ObjectId(id);
          if (db.GetBusById(objId).AssignedTo == -1)
          {
              db.DeleteBusByObjId(objId);
              return Json(new { success = "true", msg = "" });
          }
          else
          {
              return Json(new { success = "false", msg = "Please unassign the bus first!" });
          }

      }
      #endregion


      #region Stop

      public ActionResult AddStop()
      {
         return PartialView("AddStop");
      }

      public ActionResult AddNewStop(string location)
      {
         DatabaseInterface db = new DatabaseInterface();
         if (!db.IsStopLocationUnique(location))
            return Json("false");

         Stop stop = new Stop()
         {
            Id = ObjectId.GenerateNewId(),
            Location = location,
            StopId = db.GetNextStopId()
         };
         db.SaveStop(stop);
         return Json(new { success = "true", id = stop.Id.ToString() });
      }

      public ActionResult DeleteStop(string id)
      {
          DatabaseInterface db = new DatabaseInterface();
          ObjectId objId = new ObjectId(id);
          IEnumerable<Route> routes = db.GetAvailableRoutes();
          foreach (Route route in routes)
          {
              if (route.Stops.RemoveAll(s => s.Id == objId) > 0)
              {
                  db.SaveRoute(route);
              }
          }
          db.DeleteStopByObjId(objId);
          return Json(new { success = "true", msg = "" });
      }
      #endregion


      #region Driver

      public ActionResult AddDriver()
      {
         DriverModel model = new DriverModel()
         {
            StateNames = stateNames,
            StateAbbreviations = stateAbbreviations,
            UpdatingDriver = false
         };
         return PartialView("AddDriver", model);
      }

      public ActionResult AddNewDriver(string state, string name, string license)
      {
         DatabaseInterface db = new DatabaseInterface();
         if (!db.IsDriverLicenseUnique(license, state))
            return Json("false");

         Driver driver = new Driver()
         {
            Id = ObjectId.GenerateNewId(),
            DriverLicense = license,
            Name = name,
            AssignedTo = -1,
            State = state,
            DriverId = db.GetNextDriverId()

         };
         db.SaveDriver(driver);
         return Json(new { success = "true", id = driver.Id.ToString(), driverId = driver.DriverId });
      }

      public ActionResult ModifyDriver(string driverId)
      {
         DatabaseInterface db = new DatabaseInterface();
         Driver driver = db.GetDriverById(driverId);
         DriverModel model = new DriverModel
         {
            StateNames = stateNames,
            StateAbbreviations = stateAbbreviations,
            Name = driver.Name,
            State = driver.State,
            License = driver.DriverLicense,
            DriverId = driver.DriverId,
            UpdatingDriver = true
         };
         return PartialView("AddDriver", model);
      }

      public ActionResult UpdateDriver(string driverId, string state, string name, string license)
      {
         DatabaseInterface db = new DatabaseInterface();
         if (!db.IsDriverLicenseUnique(license, state, driverId))
            return Json("false");
         Driver driver = db.GetDriverById(driverId);
         driver.DriverLicense = license;
         driver.Name = name;
         driver.State = state;
         db.UpdateDriver(driver);
         return Json("true");
      }

      public ActionResult DeleteDriver(string id)
      {
          DatabaseInterface db = new DatabaseInterface();
          ObjectId objId = new ObjectId(id);
          if (db.GetDriverByobjId(objId).AssignedTo == -1)
          {
              db.DeleteDriverByObjId(objId);
              return null;
          }
          else
          {
              return Json(new { success = "false", msg = "Please unassign the driver first!" });
          }

      }
      #endregion


      #region Employee

      public ActionResult AddEmployee()
      {
         DatabaseInterface db = new DatabaseInterface();
         EmployeeModel model = new EmployeeModel()
         {
             StateNames = stateNames,
             StateAbbreviations = stateAbbreviations,
             Name = "",
             Address = "",
             AvailableRoutes = db.GetAvailableRoutes(),
             City = "",
             Email = "",
             Phone = "",
             Position = "",
             UpdatingEmployee = false,
         };
         return PartialView("AddEmployee", model);
      }

      public ActionResult AddNewEmployee(bool isMale, string email, string phone, string address, string city, string state, int zip, int routeId, long ssn, string position, string name)
      {
         DatabaseInterface db = new DatabaseInterface();
         if (!db.IsSocialSecurityNumberUnique(ssn))
            return Json("false");

         Employee employee = new Employee()
         {
            Id = ObjectId.GenerateNewId(),
            SocialSecurityNumber = ssn,
            Position = position,
            Name = name,
            IsMale = isMale,
            Email = email,
            Phone = phone,
            Address = address,
            City = city,
            State = state,
            Zip = zip,
            AssignedTo = routeId,
            EmployeeId = db.GetNextEmployeeId()

         };
         db.SaveEmployee(employee);
         return Json(new { success = "true", id = employee.Id.ToString(), employeeId = employee.EmployeeId });
      }

      public ActionResult ModifyEmployee(string employeeId)
      {
          DatabaseInterface db = new DatabaseInterface();
          Employee employee = db.GetEmployeeById(int.Parse(employeeId));
          EmployeeModel model = new EmployeeModel
          {
              StateNames = stateNames,
              StateAbbreviations = stateAbbreviations,
              Name = employee.Name,
              Address = employee.Address,
              AssignedTo = employee.AssignedTo,
              AvailableRoutes = db.GetAvailableRoutes(),
              City = employee.City,
              Email = employee.Email,
              EmployeeId = employee.EmployeeId,
              IsMale = employee.IsMale,
              Phone = employee.Phone,
              Position = employee.Position,
              SocialSecurityNumber = employee.SocialSecurityNumber,
              State = employee.State,
              UpdatingEmployee = true,
              Zip = employee.Zip
          };
          return PartialView("AddEmployee", model);
      }

      public ActionResult UpdateEmployee(int employeeId, string address, int assignedTo, string city, string email,
          string phone, string position, string state, int zip)
      {
          DatabaseInterface db = new DatabaseInterface();
          Employee e = db.GetEmployeeById(employeeId);
          e.Address = address;
          e.AssignedTo = assignedTo;
          e.City = city;
          e.Email = email;
          e.Phone = phone;
          e.Position = position;
          e.State = state;
          e.Zip = zip;
          db.UpdateEmployee(e);
          return Json("true");
      }

      public ActionResult DeleteEmployee(string id)
      {
          DatabaseInterface db = new DatabaseInterface();
          ObjectId objId = new ObjectId(id);
          db.DeleteEmployeeByObjId(objId);
          return null;
      }

      #endregion


   }
}
