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
                Username = user.Username
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

        public ActionResult AddRoute(Boolean isToWork)
        {
            DatabaseInterface db = new DatabaseInterface();
            RouteModel model = new RouteModel()
            {
                AvailableBuses = db.GetAvailableBuses(),
                AvailableStops = db.GetAvailableStops(),
                AvailableDrivers = db.GetAvailableDrivers(),
                Name = "",
                RouteId = "",
                Stops = { },
                UpdatingRoute = false,
                DriverBusList = null,
                IsToWork = isToWork
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
                Name = route.Name,
                RouteId = routeId,
                Stops = route.Stops,
                UpdatingRoute = true,
                DriverBusList = route.DriverBusList,
                IsActive = route.IsActive,
                IsToWork = int.Parse(routeId) < 500
            };
            return PartialView("AddRoute", model);
        }

        public ActionResult AddNewRoute(List<int> stopIds, string routeName, bool isToWork, bool isActive, List<String> buses,
                    List<String> drivers, List<String> times, List<String> statuses)
        {
            DatabaseInterface db = new DatabaseInterface();
            if (!db.IsRouteNameUnique(routeName))
                return Json("false");
            int routeId;
            if (isToWork)
                routeId = db.GetNextLowRouteId();
            else
                routeId = db.GetNextHighRouteId();
            List<Stop> stops = new List<Stop>();
            if (stopIds != null)
            {
                foreach (int id in stopIds)
                {
                    stops.Add(db.GetStopByStopId(id));
                }
            }
            List<DriverBus> driverBusList = new List<DriverBus>();
            if (buses != null)//then drivers won't be null either - no need to check both
            {

                for (int i = 0; i < buses.Count; i++)
                {
                    String sBusId = buses[i];
                    int busId = -1;
                    String sDriverId = drivers[i];
                    int driverId = -1;
                    bool entryIsActive = statuses[i].Equals("ACTIVE") ? true : false;
                    if (!sBusId.Equals("None"))
                    {
                        busId = int.Parse(sBusId);
                        //order matters - do NOT assign the bus first
                        db.AssignBusToRoute(busId, routeId);
                        if (entryIsActive && isActive)
                            db.BusSetActive(busId, true, routeId);
                    }
                    if (!sDriverId.Equals("None"))
                    {
                        driverId = int.Parse(sDriverId);
                        //order matters - do NOT assign the driver first
                        db.AssignDriverToRoute(driverId, routeId);
                        if (entryIsActive && isActive)
                            db.DriverSetActive(driverId, true, routeId);
                    }
                    String[] timeArray = times[i].Split(':');
                    String hour = timeArray[0];
                    timeArray = timeArray[1].Split(' ');
                    String minute = timeArray[0];
                    String ampm = timeArray[1];

                    driverBusList.Add(new DriverBus()
                    {
                        AMPM = ampm,
                        BusId = busId,
                        DriverId = driverId,
                        Hour = hour,
                        Minute = minute,
                        IsActive = entryIsActive
                    });
                }
            }

            Route route = new Route()
            {
                Stops = stops,
                Name = routeName,
                RouteId = routeId,
                Id = ObjectId.GenerateNewId(),
                IsActive = isActive,
                DriverBusList = driverBusList,
                HasBeenDeleted = false
            };
            db.AddRoute(route);
            return Json(new
            {
                success = "true",
                id = route.Id.ToString()
            });

        }

        public ActionResult UpdateRoute(int routeId, List<int> stopIds, string routeName, bool isActive, List<String> buses,
            List<String> drivers, List<String> times, List<String> statuses)
        {
            DatabaseInterface db = new DatabaseInterface();
            String sRouteId = routeId.ToString();
            if (!db.IsRouteNameUnique(routeName, sRouteId))
                return Json("false");
            List<Stop> stops = new List<Stop>();
            if (stopIds != null)
            {
                foreach (int id in stopIds)
                {
                    stops.Add(db.GetStopByStopId(id));
                }
            }

            //need to unassign all buses and drivers assigned to this route and then we'll assign the ones that are now assigned to the route
            //but first (ORDER MATTERS!!!) set those buses and drivers to be inactive
            db.SetInactiveBusesDriversFromRoute(routeId);
            db.UnassignBusesDriversFromRoute(routeId);

            List<DriverBus> driverBusList = new List<DriverBus>();
            if (buses != null)//this means drivers won't be null either - no need to check both
            {
                for (int i = 0; i < buses.Count; i++)
                {
                    String sBusId = buses[i];
                    int busId = -1;
                    String sDriverId = drivers[i];
                    int driverId = -1;
                    bool entryIsActive = statuses[i].Equals("ACTIVE") ? true : false;
                    if (!sBusId.Equals("None"))
                    {
                        busId = int.Parse(sBusId);
                        //order matters - do NOT assign the bus first
                        db.AssignBusToRoute(busId, routeId);
                        if (entryIsActive && isActive)
                            db.BusSetActive(busId, true, routeId);
                    }
                    if (!sDriverId.Equals("None"))
                    {
                        driverId = int.Parse(sDriverId);
                        //order matters - do NOT assign the driver first
                        db.AssignDriverToRoute(driverId, routeId);
                        if (entryIsActive && isActive)
                            db.DriverSetActive(driverId, true, routeId);
                    }
                    String[] timeArray = times[i].Split(':');
                    String hour = timeArray[0];
                    timeArray = timeArray[1].Split(' ');
                    String minute = timeArray[0];
                    String ampm = timeArray[1];

                    driverBusList.Add(new DriverBus()
                    {
                        AMPM = ampm,
                        BusId = busId,
                        DriverId = driverId,
                        Hour = hour,
                        Minute = minute,
                        IsActive = entryIsActive
                    });
                }
            }

            Route route = db.GetRouteByRouteId(routeId);
            route.Stops = stops;
            route.Name = routeName;
            route.RouteId = routeId;
            route.IsActive = isActive;
            route.DriverBusList = driverBusList;
            db.UpdateRoute(route);
            return Json(new
            {
                success = "true",
                assigned = db.GetEmployeesAssignedToRoute(routeId).Count().ToString(),
                capacity = db.GetTotalCapacity(routeId),
                stops = db.GetStopsAssignedToRoute(routeId).Count().ToString(),
                id = route.Id.ToString()
            });
        }

        public ActionResult DeleteRoute(string id)
        {
            DatabaseInterface db = new DatabaseInterface();
            ObjectId objId = new ObjectId(id);
            Route r = db.GetRouteById(objId);
            bool isToWork = r.RouteId < 500;
            IEnumerable<DriverBus> drbss = r.DriverBusList;
            IEnumerable<Employee> employees = db.GetAvailableEmployees();

            //unassign buses and drivers
            db.UnassignBusesDriversFromRoute(r.RouteId);
            db.SetInactiveBusesDriversFromRoute(r.RouteId);
            //foreach (DriverBus drbs in drbss)
            //{
            //    if (isToWork)
            //    {
            //        drbs.Bus.MorningAssignedTo = -1;
            //        drbs.Bus.MorningIsActive = false;
            //        drbs.Driver.MorningAssignedTo = -1;
            //        drbs.Driver.MorningIsActive = false;
            //    }
            //    else
            //    {
            //        drbs.Bus.EveningAssignedTo = -1;
            //        drbs.Bus.EveningIsActive = false;
            //        drbs.Driver.EveningAssignedTo = -1;
            //        drbs.Driver.EveningIsActive = false;
            //    }
            //    db.UpdateBus(drbs.Bus);
            //    db.UpdateDriver(drbs.Driver);
            //}

            //unassign employees
            foreach (Employee employee in employees)
            {
                if (employee.MorningAssignedTo == r.RouteId)
                {
                    employee.MorningAssignedTo = -1;
                    db.UpdateEmployee(employee);
                }
                if (employee.EveningAssignedTo == r.RouteId)
                {
                    employee.EveningAssignedTo = -1;
                    db.UpdateEmployee(employee);
                }
            }

            //db.DeleteRouteByObjId(objId);
            r.HasBeenDeleted = true;
            db.UpdateRoute(r);
            return null;

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
                Capacity = capacity,
                State = state,
                MorningAssignedTo = -1,
                EveningAssignedTo = -1,
                HasBeenDeleted = false

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
                MorningIsActive = bus.MorningIsActive,
                State = bus.State,
                BusId = busId.ToString()
            };
            return PartialView("AddBus", model);
        }

        public ActionResult UpdateBus(string busId, int capacity, string license, string state)
        {
            DatabaseInterface db = new DatabaseInterface();
            license = license.ToUpper();
            if (!db.IsLicenseUnique(license, busId))
               return Json(new { success = "false", reason = "The license plate already exists. Please enter a unique license plate" });
            Bus bus = db.GetBusByBusId(int.Parse(busId));
            int diff = capacity - bus.Capacity;
            Route morning = db.GetRouteByRouteId(bus.MorningAssignedTo);
            Route evening = db.GetRouteByRouteId(bus.EveningAssignedTo);
            int minCapMorning = morning == null ? 100 : db.GetTotalCapacity(morning.RouteId) - db.GetEmployeesAssignedToRoute(morning.RouteId).Count();
            int minCapEvening = evening == null ? 100 : db.GetTotalCapacity(evening.RouteId) - db.GetEmployeesAssignedToRoute(evening.RouteId).Count();
            if (minCapMorning + diff < 0 || minCapEvening + diff < 0)
               return Json(new { success = "false", reason = "Capacity is too low" });
            bus.LicensePlate = license;
            bus.BusId = int.Parse(busId);
            bus.Capacity = capacity;
            bus.State = state;
            db.UpdateBus(bus);
            return Json(new { success = "true", id = bus.Id.ToString() });
        }

        public ActionResult DeleteBus(string id)
        {
            DatabaseInterface db = new DatabaseInterface();
            ObjectId objId = new ObjectId(id);
            IEnumerable<Route> routes = db.GetAvailableRoutes();
            Bus bus = db.GetBusById(objId);
            if ((bus.MorningAssignedTo == -1) && (bus.EveningAssignedTo == -1))
            {
                //db.DeleteBusByObjId(objId);
                bus.HasBeenDeleted = true;
                db.UpdateBus(bus);
            }
            else
            {
                foreach (Route route in routes)
                {
                    if (route.DriverBusList.Exists(s => s.BusId == bus.BusId))
                    {
                        if (route.DriverBusList.Count == 1)
                        {
                            //the bus is the only bus of the route
                            db.RouteSetInactive(route);
                        }

                        //Set the driverbus to inactive
                        DriverBus drbs = route.DriverBusList.Find(s => s.BusId == bus.BusId);
                        drbs.IsActive = false;
                        drbs.BusId = -1;
                        db.UpdateRoute(route);
                        //db.DeleteBusByObjId(objId);
                        bus.HasBeenDeleted = true;
                        db.UpdateBus(bus);
                    }
                }
            }
            return Json(new { success = "true", msg = "" });

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
                StopId = db.GetNextStopId(),
                HasBeenDeleted = false
            };
            db.SaveStop(stop);
            return Json(new { success = "true", id = stop.Id.ToString() });
        }

        public ActionResult DeleteStop(string id)
        {
            DatabaseInterface db = new DatabaseInterface();
            ObjectId objId = new ObjectId(id);
            IEnumerable<Route> routes = db.GetAvailableRoutes();
            //If stop is the only stop of a route, then the route is set to inactive
            foreach (Route route in routes)
            {
                if (route.Stops.Exists(s => s.Id == objId) && route.Stops.Count == 1)
                {
                    db.RouteSetInactive(route);
                }
            }
            foreach (Route route in routes)
            {
                if (route.Stops.RemoveAll(s => s.Id == objId) > 0)
                {
                    db.SaveRoute(route);
                }
            }
            //db.DeleteStopByObjId(objId);
            Stop stop = db.GetStop(objId);
            stop.HasBeenDeleted = true;
            db.UpdateStop(stop);
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
                MorningAssignedTo = -1,
                EveningAssignedTo = -1,
                State = state,
                DriverId = db.GetNextDriverId(),
                HasBeenDeleted = false
            };
            db.SaveDriver(driver);
            return Json(new { success = "true", id = driver.Id.ToString(), driverId = driver.DriverId });
        }

        public ActionResult ModifyDriver(int driverId)
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

        public ActionResult UpdateDriver(int driverId, string state, string name, string license)
        {
            DatabaseInterface db = new DatabaseInterface();
            if (!db.IsDriverLicenseUnique(license, state, driverId))
                return Json("false");
            Driver driver = db.GetDriverById(driverId);
            driver.DriverLicense = license;
            driver.Name = name;
            driver.State = state;
            db.UpdateDriver(driver);
            return Json(new { success = "true", id = driver.Id.ToString() });
        }

        public ActionResult DeleteDriver(string id)
        {
            DatabaseInterface db = new DatabaseInterface();
            ObjectId objId = new ObjectId(id);
            IEnumerable<Route> routes = db.GetAvailableRoutes();
            Driver driver = db.GetDriverByobjId(objId);
            if (driver.MorningAssignedTo == -1 && driver.EveningAssignedTo == -1)
            {
                //db.DeleteDriverByObjId(objId);
                driver.HasBeenDeleted = true;
                db.UpdateDriver(driver);
            }
            else
            {
                foreach (Route route in routes)
                {
                    if (route.DriverBusList.Exists(s => s.DriverId == driver.DriverId))//TODO this also needs to check if 
                    {
                        if (route.DriverBusList.Count == 1)
                        {
                            //the bus is the only bus of the route
                            db.RouteSetInactive(route);
                        }

                        //Set the driverbus to inactive
                        DriverBus drbs = route.DriverBusList.Find(s => s.DriverId == driver.DriverId);
                        drbs.IsActive = false;
                        drbs.DriverId = -1;
                        db.UpdateRoute(route);
                        //db.DeleteDriverByObjId(objId);
                        driver.HasBeenDeleted = true;
                        db.UpdateDriver(driver);
                    }
                }
            }
            return null;

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

        public ActionResult AddNewEmployee(bool isMale, string email, string phone, string address, string city, string state, int zip, int morningRouteId, int eveningRouteId, long ssn, string position, string name)
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
                MorningAssignedTo = morningRouteId,
                EveningAssignedTo = eveningRouteId,
                EmployeeId = db.GetNextEmployeeId(),
                HasBeenDeleted = false

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
                MorningAssignedTo = employee.MorningAssignedTo,
                EveningAssignedTo = employee.EveningAssignedTo,
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

        public ActionResult UpdateEmployee(int employeeId, string address, int morningRouteId, int eveningRouteId, string city, string email,
            string phone, string position, string state, int zip)
        {
            DatabaseInterface db = new DatabaseInterface();
            Employee e = db.GetEmployeeById(employeeId);
            e.Address = address;
            e.MorningAssignedTo = morningRouteId;
            e.EveningAssignedTo = eveningRouteId;
            e.City = city;
            e.Email = email;
            e.Phone = phone;
            e.Position = position;
            e.State = state;
            e.Zip = zip;
            db.UpdateEmployee(e);
            return Json(new { success = "true", id = e.Id.ToString() });
        }

        public ActionResult DeleteEmployee(string id)
        {
            DatabaseInterface db = new DatabaseInterface();
            ObjectId objId = new ObjectId(id);
            //db.DeleteEmployeeByObjId(objId);
            Employee employee = db.GetEmployeeById(objId);
            employee.HasBeenDeleted = true;
            db.UpdateEmployee(employee);
            return null;
        }

        #endregion

        public ActionResult ViewRoutes()
        {
            DatabaseInterface db = new DatabaseInterface();
            var routes = db.GetAvailableRoutes();
            var model = new CustomTable()
            {
                Headers = new List<string>(){
               "ID",
               "Name",
               "Time of Day",
               "Status",
               "# of Stops",
               "# Assigned/Capacity"
               },
                Rows = routes.Select(r => new CustomRow()
                {
                    ObjectId = r.Id.ToString(),
                    ModifyCall = "modifyRoute(event," + r.RouteId + ")",
                    DeleteCall = "deleteItemClick('route', '" + r.Id.ToString() + "', event)",
                    Columns = new List<string>(){
                        r.RouteId.ToString(),
                        r.Name,
                        (r.RouteId < 500) ? "Morning" : "Evening",
                        r.IsActive? "Active" : "Inactive",
                        db.GetStopsAssignedToRoute(r.RouteId).Count().ToString(),
                        db.GetEmployeesAssignedToRoute(r.RouteId).Count().ToString() + "/" + db.GetTotalCapacity(r.RouteId)
                    }
                }).ToList()
            };
            return PartialView("ViewItem", model);
        }

        public ActionResult ViewEmployees()
        {
            DatabaseInterface db = new DatabaseInterface();
            var employees = db.GetAvailableEmployees();
            var model = new CustomTable()
            {
                Headers = new List<string>(){
                   "ID",
                   "Name",
                   "SSN",
                   "Position",
                   "Email",
                   "Gender",
                   "Phone",
                   "Address",
                   "Morning Route",
                   "Evening Route"
                   },
                Rows = employees.Select(e => new CustomRow()
                {
                    ObjectId = e.Id.ToString(),
                    ModifyCall = "modifyEmployee(event," + e.EmployeeId + ")",
                    DeleteCall = "deleteItemClick('employee', '" + e.Id.ToString() + "', event)",
                    Columns = new List<string>(){
                       e.EmployeeId.ToString(),
                       e.Name,
                       e.SocialSecurityNumber.ToString(),
                       e.Position,
                       e.Email,
                       e.IsMale? "Male": "Female",
                       e.Phone.ToString(),
                       e.Address + ", " + e.City + ", " + e.State + " " + e.Zip.ToString(),
                       e.MorningAssignedTo == -1 ? "none" : e.MorningAssignedTo.ToString() + " - " + db.GetRouteByRouteId(e.MorningAssignedTo).Name,
                       e.EveningAssignedTo == -1 ? "none" : e.EveningAssignedTo.ToString() + " - " + db.GetRouteByRouteId(e.EveningAssignedTo).Name
                       }
                }).ToList()
            };
            return PartialView("ViewItem", model);
        }

        public ActionResult ViewBuses()
        {
            DatabaseInterface db = new DatabaseInterface();
            var buses = db.GetAvailableBuses();
            var model = new CustomTable()
            {
                Headers = new List<string>(){
                   "ID",
                   "License",
                   "State",
                   "Capacity",
                   "Morning Route",
                   "Morning Driver",
                   "Morning Status",
                   "Evening Route",
                   "Evening Driver",
                   "Evening Status"
                   },
                Rows = buses.Select(b => new CustomRow()
                {
                    ObjectId = b.Id.ToString(),
                    ModifyCall = "modifyBus(event," + b.BusId + ")",
                    DeleteCall = "deleteItemClick('bus', '" + b.Id.ToString() + "', event)",
                    Columns = new List<string>(){
                       b.BusId.ToString(),
                       b.LicensePlate.ToString(),
                       b.State,
                       b.Capacity.ToString(),
                       b.MorningAssignedTo  == -1? "none" : b.MorningAssignedTo.ToString() + " - " + db.GetRouteByRouteId(b.MorningAssignedTo).Name,
                       b.MorningAssignedTo  == -1? "none" : db.GetDriverById(db.GetRouteByRouteId(b.MorningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.BusId == b.BusId).DriverId).DriverId == -1 ? "none" : db.GetDriverById(db.GetRouteByRouteId(b.MorningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.BusId == b.BusId).DriverId).DriverId.ToString() + " - " + db.GetDriverById(db.GetRouteByRouteId(b.MorningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.BusId == b.BusId).DriverId).Name,
                       b.MorningIsActive ? "Active" : "Inactive",
                       b.EveningAssignedTo  == -1? "none" : b.EveningAssignedTo.ToString() + " - " + db.GetRouteByRouteId(b.EveningAssignedTo).Name,
                       b.EveningAssignedTo  == -1? "none" : db.GetDriverById(db.GetRouteByRouteId(b.EveningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.BusId == b.BusId).DriverId).DriverId == -1 ? "none" : db.GetDriverById(db.GetRouteByRouteId(b.EveningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.BusId == b.BusId).DriverId).DriverId.ToString() + " - " +  db.GetDriverById(db.GetRouteByRouteId(b.EveningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.BusId == b.BusId).DriverId).Name,
                       b.EveningIsActive ? "Active" : "Inactive"
                   }
                }).ToList()
            };
            return PartialView("ViewItem", model);
        }

        public ActionResult ViewDrivers()
        {
            DatabaseInterface db = new DatabaseInterface();
            var drivers = db.GetAvailableDrivers();
            var model = new CustomTable()
            {
                Headers = new List<string>(){
                   "ID",
                   "Name",
                   "Driver License",
                   "State",
                   "Morning Route",
                   "Morning Bus",
                   "Morning Status",
                   "Evening Route",
                   "Evening Bus",
                   "Evening Status"
                },
                Rows = drivers.Select(d => new CustomRow()
                {
                    ObjectId = d.Id.ToString(),
                    ModifyCall = "modifyDriver(event," + d.DriverId + ")",
                    DeleteCall = "deleteItemClick('driver', '" + d.Id.ToString() + "', event)",
                    Columns = new List<string>(){
                       d.DriverId.ToString(),
                       d.Name,
                       d.DriverLicense,
                       d.State,
                       d.MorningAssignedTo  == -1? "none" : d.MorningAssignedTo.ToString() + " - " + db.GetRouteByRouteId(d.MorningAssignedTo).Name,
                       d.MorningAssignedTo  == -1? "none" : db.GetBusByBusId(db.GetRouteByRouteId(d.MorningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.DriverId == d.DriverId).BusId).BusId == -1 ? "none" : db.GetBusByBusId(db.GetRouteByRouteId(d.MorningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.DriverId == d.DriverId).BusId).BusId.ToString() + " - " + db.GetBusByBusId(db.GetRouteByRouteId(d.MorningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.DriverId == d.DriverId).BusId).LicensePlate,
                       d.MorningIsActive ? "Active" : "Inactive",
                       d.EveningAssignedTo  == -1? "none" : d.EveningAssignedTo.ToString() + " - " + db.GetRouteByRouteId(d.EveningAssignedTo).Name,
                       d.EveningAssignedTo  == -1? "none" : db.GetBusByBusId(db.GetRouteByRouteId(d.EveningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.DriverId == d.DriverId).BusId).BusId == -1 ? "none" : db.GetBusByBusId(db.GetRouteByRouteId(d.EveningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.DriverId == d.DriverId).BusId).BusId.ToString() + " - " + db.GetBusByBusId(db.GetRouteByRouteId(d.EveningAssignedTo).DriverBusList.FirstOrDefault(driverBus => driverBus.DriverId == d.DriverId).BusId).LicensePlate,
                       d.EveningIsActive ? "Active" : "Inactive"
                }
                }).ToList()
            };
            return PartialView("ViewItem", model);
        }

        public ActionResult ViewStops()
        {
            DatabaseInterface db = new DatabaseInterface();
            var stops = db.GetAvailableStops();
            var model = new CustomTable()
            {
                Headers = new List<string>(){
                    "ID",
                    "Location",
                },
                Rows = stops.Select(s => new CustomRow()
                {
                    ObjectId = s.Id.ToString(),
                    ModifyCall = "",
                    DeleteCall = "deleteItemClick('stop', '" + s.Id.ToString() + "', event)",
                    Columns = new List<string>(){
                        s.StopId.ToString(),
                        s.Location,
                    }
                }).ToList()
            };
            return PartialView("ViewItem", model);
        }

        public ActionResult ViewSystemUsage()
        {
            DatabaseInterface db = new DatabaseInterface();
            List<EmployeeActivity> list = db.GetEmployeeActivity();
            CustomRow row;
            List<CustomRow> rows = new List<CustomRow>();
            foreach (EmployeeActivity activity in list)
            {
                row = new CustomRow();
                row.ObjectId = activity.Id.ToString();
                Route route = activity.Route;
                Driver driver = activity.Driver;
                Bus bus = activity.Bus;
                Employee employee = activity.Employee;
                Stop stop = activity.Stop;
                row.Columns = new List<String>()
                {
                    route.RouteId + " - " + route.Name,
                    bus.BusId + " - " + bus.LicensePlate,
                    driver.DriverId + " - " + driver.Name,
                    stop.StopId + " - " + stop.Location,
                    TimeZoneInfo.ConvertTimeFromUtc(activity.Date, TimeZoneInfo.Local).ToString(), 
                    employee.EmployeeId + " - " + employee.Name
                };
                rows.Add(row);
            }
            var model = new CustomTable()
            {
                Headers = new List<string>(){
                    "Route",
                    "Bus",
                    "Driver",
                    "Stop",
                    "Date/Time",
                    "Employee"
                },
                Rows = rows,
                Totals = new List<string>()
                {
                    "0",//db.GetDistinctEmployeActivityCount("RouteId").ToString(),
                    "0",//db.GetDistinctEmployeActivityCount("BusId").ToString(),
                    "0",//db.GetDistinctEmployeActivityCount("DriverId").ToString(),
                    "0",//db.GetDistinctEmployeActivityCount("StopId").ToString(),
                    "-",
                    "0",//db.GetDistinctEmployeActivityCount("EmployeeId").ToString()
                }
            };
            return PartialView("ViewSystemUsage", model);
        }
    }
}
