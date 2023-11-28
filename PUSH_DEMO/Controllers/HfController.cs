using Hangfire;
using Microsoft.AspNetCore.Mvc;
using PUSH_DEMO.Services;

namespace PUSH_DEMO.Controllers
{
    public class HfController : Controller
    {

        private readonly PushNotiService _repository;

        public HfController(PushNotiService pushNotificationRepository)
        {
            _repository = pushNotificationRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public string createproduct()
        {
            string message = "sdfsdf";
            // Schedule the job to run at a specific time (e.g., tomorrow at 3:00 PM)
            var delay = TimeSpan.Zero;

            // Schedule the job with no delay Console.WriteLine("Hello world from Hangfire!")
            var jobId = BackgroundJob.Enqueue(() => _repository.Notify(message));

            return $"Job ID: {jobId}. You added one product into your checklist successfully!";
        }
    }
}
