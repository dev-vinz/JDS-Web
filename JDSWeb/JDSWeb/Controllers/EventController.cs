﻿using JDSCommon.Database;
using JDSCommon.Database.DataContract;
using JDSCommon.Services;
using JDSWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JDSContext = JDSCommon.Database.Models.JDSContext;

namespace JDSWeb.Controllers
{
    public class EventController : Controller
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\
        |*                           PUBLIC METHODS                          *|
        \* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        public IActionResult ActualEvents()
        {
            EventViewModel vm = new EventViewModel
            {
                Events = FetchActualEvents(),
            };

            return View(vm);
        }

        public IActionResult PassedEvents()
        {
            EventViewModel vm = new EventViewModel
            {
                Events = FetchPassedEvents(),
            };

            return View(vm);
        }

        public IActionResult Create()
        {
            if ((ERole)(HttpContext.Session.GetInt32(UserViewModel.SessionKeyUserRole) ?? -1) < ERole.Manager)
            {
                return RedirectToAction("ActualEvents", "Event");
            }

            return View();
        }

        public IActionResult Update()
        {
            /*
            if ((ERole)(HttpContext.Session.GetInt32(UserViewModel.SessionKeyUserRole) ?? -1) < ERole.Manager)
            {
                return RedirectToAction("ActualEvents", "Event");
            }
            */

            EventViewModel vm = new EventViewModel
            {
                Events = FetchEvents(),
            };

            return View(vm);
        }

        public IActionResult Delete()
        {
            if ((ERole)(HttpContext.Session.GetInt32(UserViewModel.SessionKeyUserRole) ?? -1) < ERole.Manager)
            {
                return RedirectToAction("ActualEvents", "Event");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ParseEvent(string title, string description, DateTime date, ICollection<IFormFile> files)
        {
            JDSContext ctx = new JDSContext();

            ctx.Events.Add(new Event
            {
                Title = title,
                Description = description,
                Date = date,
                Images = ImagesFromFileNames(files)
            });

            ctx.SaveChanges();

            ctx.Dispose();

            return RedirectToAction("ActualEvents", "Event");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ParseEventUpdate(string title, string description, DateTime date, ICollection<IFormFile> files)
        {
            JDSContext ctx = new JDSContext();

            ctx.Events.Update(new Event
            {
                Title = title,
                Description = description,
                Date = date,
                Images = ImagesFromFileNames(files)
            });

            ctx.SaveChanges();

            ctx.Dispose();

            return RedirectToAction("ActualEvents", "Event");
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\
        |*                          PRIVATE METHODS                          *|
        \* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        private static Image[] ImagesFromFileNames(ICollection<IFormFile> files)
        {
            Image[] images = new Image[files.Count];

            int count = 0;

            foreach(var image in files)
            {
                images[count++] = new Image
                {
                    Alt = "none",
                    Picture = ImageService.FromStreamToBytes(image.OpenReadStream()),
                };
            }

            return images;
        }

        private static Event[] FetchEvents()
        {
            JDSContext ctx = new JDSContext();

            Event[] events = ctx.Events.Fetch();

            ctx.Dispose();

            return events;
        }

        private static Event[] FetchActualEvents()
        {
            Event[] events = FetchEvents();
            Event[] actualEvents = events.Where(e => DateTime.Compare(e.Date, DateTime.Now) >= 0).ToArray();

            return actualEvents;
        }

        private static Event[] FetchPassedEvents()
        {
            Event[] events = FetchEvents();
            Event[] passedEvents = events.Where(e => DateTime.Compare(e.Date, DateTime.Now) < 0).ToArray();

            return passedEvents;
        }

    }
}
