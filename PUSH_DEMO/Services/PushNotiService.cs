using Microsoft.EntityFrameworkCore;
using SmartSikshya.Data;
using SmartSikshya.DataLayer;
using SmartSikshya.Repo.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmartSikshya.Repo
{
    public class PushNotificationRepository : IPushNotificatrion
    {
        private ApplicationDbContext _context;
        private DbSet<OneSignalSubscribers> _entity;
        private readonly DbSet<Student> _student;
        private readonly DbSet<SchoolClass> _schoolClass;
        private readonly DbSet<SchoolStaff> _schoolStaff;
        private readonly DbSet<Admission> _admission;

        public PushNotificationRepository(ApplicationDbContext context)
        {
            _context = context;
            _entity = context.Set<OneSignalSubscribers>();
            _student = context.Set<Student>();
            _schoolClass = context.Set<SchoolClass>();
            _schoolStaff = context.Set<SchoolStaff>();
            _admission = context.Set<Admission>();
        }

        public async Task<string> Notify(string message)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var apiUrl = "https://onesignal.com/api/v1/notifications";
                    var apiKey = "YThjZGZjYmItMTlkOS00Y2ZjLTlmMjItOTU2NTE4MGE5MGY4";

                    // Prepare the JSON payload
                    var requstBody = new
                    {
                        included_segments = new[] { "All" },
                        excluded_segments = new[] { "Inactive Users" },
                        contents = new
                        {
                            en = message,
                        },
                        name = "INTERNAL_CAMPAIGN_NAME",
                        app_id = "60a708b8-c143-4282-882a-2f50a4afd650"
                    };

                    var requestBodyString = Newtonsoft.Json.JsonConvert.SerializeObject(requstBody);

                    // Configure the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("accept", "application/json");
                    request.Headers.Add("Authorization", $"Basic {apiKey}");
                    request.Content = new StringContent(requestBodyString, Encoding.UTF8, "application/json");

                    // Send the request and handle the response
                    var responseString = await client.SendAsync(request);

                    if (responseString.IsSuccessStatusCode)
                    {
                        var responseBody = await responseString.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody);
                        return responseBody;
                    }
                    else
                    {
                        var errorResponse = await responseString.Content.ReadAsStringAsync();
                        Console.WriteLine($"OneSignal API error: {errorResponse}");
                        return ($"OneSignal API error: {errorResponse}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return ($"Internal server error: {ex.Message}");
            }

        }

        public async Task<string> SelectedNotify(string message, string[] subscribersId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var apiUrl = "https://onesignal.com/api/v1/notifications";
                    var apiKey = "YThjZGZjYmItMTlkOS00Y2ZjLTlmMjItOTU2NTE4MGE5MGY4";

                    // Prepare the JSON payload
                    var requstBody = new
                    {
                        include_subscription_ids = subscribersId,
                        contents = new
                        {
                            en = message
                        },
                        name = "INTERNAL_CAMPAIGN_NAME",
                        app_id = "60a708b8-c143-4282-882a-2f50a4afd650"
                    };

                    var requestBodyString = Newtonsoft.Json.JsonConvert.SerializeObject(requstBody);

                    // Configure the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("accept", "application/json");
                    request.Headers.Add("Authorization", $"Basic {apiKey}");
                    request.Content = new StringContent(requestBodyString, Encoding.UTF8, "application/json");

                    // Send the request and handle the response
                    var responseString = await client.SendAsync(request);

                    if (responseString.IsSuccessStatusCode)
                    {
                        var responseBody = await responseString.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody);
                        return responseBody;
                    }
                    else
                    {
                        var errorResponse = await responseString.Content.ReadAsStringAsync();
                        Console.WriteLine($"OneSignal API error: {errorResponse}");
                        return ($"OneSignal API error: {errorResponse}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return ($"Internal server error: {ex.Message}");
            }

        }

        public async Task<string> OneSignalIdSave(OneSignalSubscribers onesignalmodel)
        {
            try
            {
                _context.Entry(onesignalmodel).State = EntityState.Added;
                _context.SaveChanges();

                return "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return ($"Internal server error: {ex.Message}");

            }

        }

        public async Task<OneSignalSubscribers> Get(string id)
        {
            return await _entity.FirstOrDefaultAsync(item => item.SubscriprionId == id);
        }

        public async Task<string[]> StudentSubscriberFilter(filterStudentClass filter)
        {
            try
            {
                int[] studentIds = null;
                if (filter.classId != null)
                {
                    /*                    var schoolClass = _schoolClass.FirstOrDefault(school => school.ClassName == filter.ClassName && school.SchoolId == filter.SchoolId);
                    */
                    if (filter.sectionId > 0)
                    {
                        studentIds = _admission.Where(z => z.SchoolId == filter.SchoolId && z.ClassId == filter.classId[0] && z.SectionId == filter.sectionId)
                      .Select(z => z.StudentId)
                      .ToArray();
                    }
                    else
                    {
                        studentIds = _student
                      .Where(s => s.SchoolId == filter.SchoolId && s.ClassId == filter.classId[0])
                      .Select(s => s.Id)
                      .ToArray();
                    }


                    if (studentIds.Length != 0)
                    {
                        var selectedSubscriberIds = _entity
                            .Where(s => s.Role == 8 && studentIds.Contains(s.Subscriber))
                            .Select(s => s.SubscriprionId)
                            .ToList();
                        return selectedSubscriberIds.ToArray();

                    }
                }
                else
                {
                    var schoolStaffIds = _schoolStaff
                  .Where(s => s.SchoolId == filter.SchoolId)
                  .Select(s => s.Id)
                  .ToArray();

                    if (schoolStaffIds.Length != 0)
                    {
                        var selectedSubscriberIds = _entity
                            .Where(s => s.Role == 11 && schoolStaffIds.Contains(s.Subscriber))
                            .Select(s => s.SubscriprionId)
                            .ToList();
                        return selectedSubscriberIds.ToArray();

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new InvalidOperationException("An error occurred while processing subscriber filtering. See inner exception for details.", ex);
            }
            return new string[0]; // Return an empty array if no data is found
        }

        public async Task<string[]> SubscriberFilterMultipleClass(filterStudentClass filter)
        {
            try
            {
                /*var schoolClass = _schoolClass.Where(s => filter.ClassName.Contains(s.ClassName) && s.SchoolId == filter.SchoolId)
                            .Select(s => s.Id)
                            .ToArray();*/
                var studentIds = _student
                            .Where(s => s.SchoolId == filter.SchoolId && filter.classId.Contains(s.ClassId))
                            .Select(s => s.Id)
                            .ToArray();


                var selectedSubscriberIds = _entity
                                            .Where(s => s.Role == 8 && studentIds.Contains(s.Subscriber))
                                            .Select(s => s.SubscriprionId)
                                            .ToList();

                return selectedSubscriberIds.ToArray();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


    }
}