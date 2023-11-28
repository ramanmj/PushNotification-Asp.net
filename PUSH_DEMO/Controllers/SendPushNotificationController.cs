using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SmartSikshya.Data;
using SmartSikshya.Helper;
using SmartSikshya.Models;
using SmartSikshya.Repo;
using SmartSikshya.Repo.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSikshya.Controllers
{
    public class PushNotificationController : Controller
    {
        private readonly IPushNotificatrion _repository;
        private readonly IClassRepository _classRepository;
        private readonly IClassSectionRepository _classSectionRepository;
        private readonly ISectionRepository _sectionRepository;


        public PushNotificationController(PushNotificationRepository pushNotificationRepository,
            IClassRepository classRepository,
            IClassSectionRepository classSectionRepository,
            ISectionRepository sectionRepository
            )
        {
            _repository = pushNotificationRepository;
            _classRepository = classRepository;
            _classSectionRepository = classSectionRepository;
            _sectionRepository = sectionRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region Get Logged in User Info
        private string GetUserId()
        {
            var userId = HttpContext.Session.GetLoggedinUserId();
            return userId;
        }
        private int GetCurrentSessionId()
        {
            var sessionid = HttpContext.Session.GetCurrentSession();
            return sessionid;
        }

        private int GetSchoolId()
        {
            var schoolId = HttpContext.Session.GetSchoolId();
            return schoolId;
        }

        private int GetCurrentFiscalYearId()
        {
            var fiscalYearId = HttpContext.Session.GetCurrentFiscalYear();
            return fiscalYearId;
        }
        private string GetUserRole()
        {
            var UserRole = HttpContext.Session.GetUserRole();
            return UserRole;
        }

        public string GetLocalIPAddress()
        {
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            if (ip.ToString() != null)
            {
                return ip.ToString();
            }
            else
            {
                return ("Can't able to Track Ip address");
            }
        }
        #endregion


        public async Task<IActionResult> PushNotification()
        {

            try
            {
                string message = "hello world";
                string[] playerIds = new string[] { "dfbd108a-2129-483c-bce6-d21a05a09a11", "2219b7a5-2af7-4b80-8abc-05fc722b0e30" };
                var pushReq = await _repository.SelectedNotify(message, playerIds);
                return Content(pushReq, "text/plain");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }


        }

        public string HfController()
        {

            try
            {
                string message = "sdfsdf";
                // Schedule the job to run at a specific time (e.g., tomorrow at 3:00 PM)
                var delay = TimeSpan.Zero;

                // Schedule the job with no delay Console.WriteLine("Hello world from Hangfire!")
                var jobId = BackgroundJob.Enqueue(() => _repository.Notify(message));

                return $"Job ID: {jobId}. You added one product into your checklist successfully!";

            }
            catch (Exception ex)
            {
                return ($"Internal server error: {ex.Message}");
            }


        }

        public async Task<IActionResult> Subssaver(string SignalId)
        {
            int userId = int.Parse(GetUserId());
            short userRole = short.Parse(GetUserRole());
            try
            {
                var oneSignalmodel = new OneSignalSubscribers
                {
                    SubscriprionId = SignalId,
                    Subscriber = userId,
                    Role = userRole
                };



                var OneSignalIdSaveResponse = await _repository.OneSignalIdSave(oneSignalmodel);
                return Content(OneSignalIdSaveResponse, "text/plain");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        public async Task<bool> GetSubs(string SignalId)
        {
            try
            {
                var getSignalId = await _repository.Get(SignalId);
                return getSignalId != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
                //StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        public async Task<IActionResult> CallFilterOneSignal(filterStudentClass filter, string message)
        {
            try
            {
                int schoolId = GetSchoolId();

                var filterStudentClass = new filterStudentClass();

                /*                abc.ClassName = "Nursery";
                */
                filterStudentClass.SchoolId = schoolId;
                filterStudentClass.classId = filter.classId;

                var filterStudentSubscribers = await _repository.StudentSubscriberFilter(filterStudentClass);
                var notufyResponse = await _repository.SelectedNotify(message, filterStudentSubscribers);
                return Content(notufyResponse, "text/plain");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public async Task<IActionResult> CallFilterClass([FromBody] filterStudentClass filter)
        {
            try
            {
                int schoolId = GetSchoolId();

                var filterStudentClass = new filterStudentClass();
                filterStudentClass.SchoolId = schoolId;
                filterStudentClass.sectionId = filter.sectionId;
                filterStudentClass.classId = filter.classId;


                if (filter.sectionId > 0)
                {
                    var filterStudentClassResponse = await _repository.StudentSubscriberFilter(filterStudentClass);
                    var notifyResponse = await _repository.SelectedNotify(filter.message, filterStudentClassResponse);
                    return Content(notifyResponse, "text/plain");

                }
                else
                {
                    var filterStudentClassResponse = await _repository.SubscriberFilterMultipleClass(filterStudentClass);
                    var notifyResponse = await _repository.SelectedNotify(filter.message, filterStudentClassResponse);
                    return Content(notifyResponse, "text/plain");

                }



            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public IActionResult Notify()
        {
            var model = new PushNotifiacationViewModel();
            var schoolId = GetSchoolId();
            var classData = _classRepository.GetAll(schoolId).ToList();
            var classIds = classData.Select(x => x.Id.ToString()).ToArray();
            var sectionMappingData = _classSectionRepository.GetAllByClassIds(classIds).ToList();

            var sectionDataList = _sectionRepository.GetAll(schoolId).ToList();
            var sectionModelList = new List<SectionMap>();
            foreach (var data in sectionMappingData.Where(x => sectionDataList.Any(y => y.Id == x.SectionId)))
            {
                if (data.SectionId != 1)
                {
                    var sectionData = sectionDataList.FirstOrDefault(x => x.Id == data.SectionId);
                    var model1 = new SectionMap
                    {
                        ClassId = data.ClassId,
                        ClassName = classData.FirstOrDefault(x => x.Id == data.ClassId).ClassName,
                        SectionId = data.SectionId.ToString(),
                        SectionName = sectionData != null ? sectionData.SectionName : ""
                    };

                    if (!string.IsNullOrEmpty(data.DisplayName))
                    {
                        model1.SectionName = data.DisplayName;
                    }

                    sectionModelList.Add(model1);
                    model.SectionList.Add(new SelectListItem
                    {
                        Value = model1.SectionId,
                        Text = model1.SectionName,
                    });

                }
            }
            ViewBag.ClassSectionList = JsonConvert.SerializeObject(sectionModelList);


            foreach (var cls in classData)
            {
                var m = new ClassMapModel
                {
                    Id = cls.Id,
                    ClassName = cls.ClassName
                };
                model.ClassList.Add(new SelectListItem { Value = cls.Id.ToString(), Text = cls.ClassName });

            }
            return View(model);

        }
    }
}
