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
      public Random ran;
      public ActionResult AddRoute()
      {
         DatabaseInterface db = new DatabaseInterface();
         AddRouteModel model = new AddRouteModel()
         {
            AvailableBuses = db.GetAvailableBuses(),
            AvailableStops = db.GetAvailableStops(),
            AvailableEmployees = db.GetAvailableEmployees()
         };
         return PartialView("AddRoute", model);
      }
      public ActionResult AddStop()
      {
         return PartialView("AddStop");
      }
      public ActionResult AddNewStop(string streetName, int streetNumber)
      {
         Stop stop = new Stop()
         {
            Id = ObjectId.GenerateNewId(),
            StreetName = streetName,
            StreetNumber = streetNumber,
            StopId = ran.Next(1000)
         };
         DatabaseInterface db = new DatabaseInterface();
         db.SaveStop(stop);
         return Json(new{});
      }
   }
}